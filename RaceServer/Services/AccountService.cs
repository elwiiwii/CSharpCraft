using System.Formats.Tar;
using System.Security.Cryptography;
using AccountService;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Grpc.Core;
using Microsoft.Identity.Client;

namespace RaceServer.Services;

public class AccountServiceImpl : AccountService.AccountService.AccountServiceBase
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<AccountServiceImpl> _logger;
    private readonly EmailService _emailService;
    private readonly TwoFactorService _twoFactorService;
    private readonly IKeyVaultService _keyVaultService;
    private readonly Dictionary<string, (int Attempts, DateTime LastAttempt)> _loginAttempts = new();
    private readonly Dictionary<string, (int Attempts, DateTime LastAttempt)> _verifyAttempts = new();
    private const int MAX_LOGIN_ATTEMPTS = 5;
    private const int MAX_VERIFY_ATTEMPTS = 3;
    private const int LOGIN_COOLDOWN_MINUTES = 15;
    private const int VERIFY_COOLDOWN_MINUTES = 5;

    public AccountServiceImpl(
        FirebaseAuth firebaseAuth,
        FirestoreDb firestoreDb,
        ILogger<AccountServiceImpl> logger,
        EmailService emailService,
        TwoFactorService twoFactorService,
        IKeyVaultService keyVaultService)
    {
        _firebaseAuth = firebaseAuth;
        _firestoreDb = firestoreDb;
        _logger = logger;
        _emailService = emailService;
        _twoFactorService = twoFactorService;
        _keyVaultService = keyVaultService;
    }

    public async Task<bool> IsTokenValid(string userId, string refreshToken)
    {
        _logger.LogInformation($"Validating token for user {userId}");

        try
        {
            var userDoc = await _firestoreDb.Collection("users").Document(userId).GetSnapshotAsync();
            if (!userDoc.Exists)
            {
                _logger.LogWarning($"User document not found for {userId}");
                return false;
            }

            var userData = userDoc.ToDictionary();
            if (!userData.ContainsKey("activeToken"))
            {
                _logger.LogWarning($"No active token found for user {userId}");
                return false;
            }

            var activeToken = userData["activeToken"].ToString();
            var isValid = activeToken == refreshToken;

            _logger.LogInformation($"Token validation result for user {userId}: {isValid}");

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error validating token for user {userId}");
            return false;
            throw;
        }
    }

    public async Task UpdateActiveToken(string userId, string refreshToken)
    {
        _logger.LogInformation($"Updating active token for user {userId}");

        try
        {
            var userDoc = _firestoreDb.Collection("users").Document(userId);
            await userDoc.UpdateAsync("activeToken", refreshToken);
            _logger.LogInformation($"Active token updated for user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating active token for user {userId}");
            throw;
        }
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task RevokeAllUserTokens(string userId)
    {
        _logger.LogInformation($"Revoking all tokens for user {userId}");

        try
        {
            var sessionsRef = _firestoreDb.Collection("sessions");
            var query = sessionsRef.WhereEqualTo("userId", userId);
            var snapshot = await query.GetSnapshotAsync();

            _logger.LogInformation($"Found {snapshot.Count} sessions for user {userId}");
            foreach (var doc in snapshot)
            {
                await doc.Reference.DeleteAsync();
                _logger.LogInformation($"Deleted session {doc.Id} for user {userId}");
            }

            var userDoc = _firestoreDb.Collection("users").Document(userId);
            await userDoc.UpdateAsync("activeToken", FieldValue.Delete);
            _logger.LogInformation($"All tokens revoked for user {userId}");

            var finalSnapshot = await query.GetSnapshotAsync();
            if (finalSnapshot.Count > 0)
            {
                _logger.LogWarning($"Found {finalSnapshot.Count} remaining sessions after revocation, attempting to delete again");
                foreach (var doc in finalSnapshot)
                {
                    await doc.Reference.DeleteAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error revoking tokens for user {userId}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public override async Task<VerifyEmailBeforeRegistrationResponse> VerifyEmailBeforeRegistration(
        VerifyEmailBeforeRegistrationRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation($"Verifying email before registration for {request.Email}");

        try
        {
            try
            {
                await _firebaseAuth.GetUserByEmailAsync(request.Email);
                return new VerifyEmailBeforeRegistrationResponse
                {
                    Success = false,
                    Message = "An account with this email already exists."
                };
            }
            catch (FirebaseAuthException)
            {

            }

            var verificationCode = await _emailService.GenerateAndStoreVerificationCode(request.Email);
            await _emailService.SendVerificationEmail(request.Email, verificationCode);

            return new VerifyEmailBeforeRegistrationResponse
            {
                Success = true,
                Message = "Verification code sent successfully. Please check your email."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error verifying email before registration for {request.Email}");
            return new VerifyEmailBeforeRegistrationResponse
            {
                Success = false,
                Message = "An error occurred while sending the verification code."
            };
            throw;
        }
    }

    public override async Task<RegisterResponse> Register(
        RegisterRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation($"Registration attempt for email: {request.Email}");

        try
        {
            var isValid = await _emailService.VerifyEmailCode(request.Email, request.VerificationCode);
            if (!isValid)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Invalid or expired verification code."
                };
            }

            var userArgs = new UserRecordArgs
            {
                Email = request.Email,
                Password = request.Password,
                EmailVerified = true
            };

            var userRecord = await _firebaseAuth.CreateUserAsync(userArgs);
            _logger.LogInformation($"User created: {userRecord.Uid}");

            var userDoc = _firestoreDb.Collection("users").Document(userRecord.Uid);
            await userDoc.SetAsync(new
            {
                Username = request.Username,
                Email = request.Email,
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                IsEmailVerified = true
            });

            return new RegisterResponse
            {
                Success = true,
                Message = "User registered successfully.",
                UserId = userRecord.Uid
            };
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogError($"Registration failed: {ex.Message}");
            return new RegisterResponse
            {
                Success = false,
                Message = $"Registration failed: {ex.Message}"
            };
            throw;
        }
    }

    public override async Task<VerifyEmailResponse> VerifyEmail(VerifyEmailRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation($"Email verification attempt for: {request.Email}");

        try
        {
            var isValid = await _emailService.VerifyEmailCode(request.Email, request.VerificationCode);
            if (!isValid)
            {
                return new VerifyEmailResponse
                {
                    Success = false,
                    Message = "Invalid or expired verification code."
                };
            }

            var userRecord = await _firebaseAuth.GetUserByEmailAsync(request.Email);
            var updateArgs = new UserRecordArgs
            {
                Uid = userRecord.Uid,
                EmailVerified = true
            };
            await _firebaseAuth.UpdateUserAsync(updateArgs);

            var userDoc = _firestoreDb.Collection("users").Document(userRecord.Uid);
            await userDoc.UpdateAsync("IsEmailVerified", true);

            var customToken = await _firebaseAuth.CreateCustomTokenAsync(userRecord.Uid);
            var refreshToken = GenerateRefreshToken();

            await userDoc.SetAsync(new Dictionary<string, object>
            {
                { "activeToken", refreshToken },
                { "lastLoginToken", Timestamp.FromDateTime(DateTime.UtcNow) }
            }, SetOptions.MergeAll);

            var sessionId = Guid.NewGuid().ToString();
            var sessionDoc = _firestoreDb.Collection("sessions").Document(sessionId);
            await sessionDoc.SetAsync(new Dictionary<string, object>
            {
                { "userId", userRecord.Uid },
                { "refreshToken", refreshToken },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) },
                { "lastActive", Timestamp.FromDateTime(DateTime.UtcNow) }
            });

            return new VerifyEmailResponse
            {
                Success = true,
                Message = "Email verified successfully.",
                AccessToken = customToken,
                RefreshToken = refreshToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Email verification failed: {ex.Message}");
            return new VerifyEmailResponse
            {
                Success = false,
                Message = $"Email verification failed: {ex.Message}"
            };
            throw;
        }
    }

    private bool IsRateLimited(string key, Dictionary<string, (int Attempts, DateTime LastAttempt)> attempts, int maxAttempts, int cooldownMinutes)
    {
        if (attempts.TryGetValue(key, out var attempt))
        {
            var timeSinceLastAttempt = DateTime.UtcNow - attempt.LastAttempt;
            if (timeSinceLastAttempt.TotalMinutes < cooldownMinutes)
            {
                if (attempt.Attempts >= maxAttempts)
                {
                    return true;
                }
                else
                {
                    attempts[key] = (0, DateTime.UtcNow);
                }
            }
        }
        return false;
    }

    private void IncrementAttempts(string key, Dictionary<string, (int Attempts, DateTime LastAttempt)> attempts)
    {
        if (attempts.TryGetValue(key, out var attempt))
        {
            attempts[key] = (attempt.Attempts + 1, DateTime.UtcNow);
        }
        else
        {
            attempts[key] = (1, DateTime.UtcNow);
        }
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        try
        {
            if (IsRateLimited(request.Email, _loginAttempts, MAX_LOGIN_ATTEMPTS, LOGIN_COOLDOWN_MINUTES))
            {
                _logger.LogWarning($"Login rate limit exceeded for email: {request.Email}");
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Too many login attempts. Please try again in {LOGIN_COOLDOWN_MINUTES} minutes."
                };
            }

            _logger.LogInformation($"Login attempt for email: {request.Email}");

            var userRecord = await _firebaseAuth.GetUserByEmailAsync(request.Email);
            _logger.LogInformation($"User found: {userRecord.Uid}");

            var authClient = new HttpClient();
            var apiKey = await _keyVaultService.GetSecretAsync("FirebaseApiKey");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Firebase API key not found in Key Vault.");
            }

            var signInRequest = new
            {
                email = request.Email,
                password = request.Password,
                returnSecureToken = true
            };

            var response = await authClient.PostAsJsonAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}",
                signInRequest);

            if (!response.IsSuccessStatusCode)
            {
                IncrementAttempts(request.Email, _loginAttempts);
                _logger.LogError($"Password verification failed for user: {userRecord.Uid}");
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            _loginAttempts.Remove(request.Email);

            var userDoc = await _firestoreDb.Collection("users").Document(userRecord.Uid).GetSnapshotAsync();
            if (!userDoc.Exists)
            {
                _logger.LogWarning($"User document not found for {userRecord.Uid}");
                return new LoginResponse
                {
                    Success = false,
                    Message = "User document not found."
                };
            }

            var userData = userDoc.ToDictionary();
            if (!userData.ContainsKey("IsTwoFactorEnabled") && (bool)userData["IsTwoFactorEnabled"])
            {
                _logger.LogInformation($"Two-factor authentication is enabled for user: {userRecord.Uid}");

                if (userData["TwoFactorType"].ToString() == "EMAIL_CODE")
                {
                    var verificationCode = await _emailService.GenerateAndStoreVerificationCode(request.Email);
                    await _emailService.SendVerificationEmail(request.Email, verificationCode);
                    _logger.LogInformation($"Verification code sent to email: {request.Email}");
                }

                return new LoginResponse
                {
                    Success = true,
                    Message = "2FA verification required",
                    RequiresTwoFactor = true,
                    TwoFactorType = userData["TwoFactorType"].ToString() == "AUTHENTICATOR_APP" ?
                        TwoFactorType.AuthenticatorApp : TwoFactorType.EmailCode
                };
            }

            _logger.LogInformation($"2FA is not enabled for user {userRecord.Uid}, proceeding with normal login");

            var customToken = await _firebaseAuth.CreateCustomTokenAsync(userRecord.Uid);
            var refreshToken = GenerateRefreshToken();
            _logger.LogInformation($"Generated refresh token for user {userRecord.Uid}");

            await userDoc.Reference.SetAsync(new Dictionary<string, object>
            {
                { "activeToken", refreshToken },
                { "lastLoginToken", Timestamp.FromDateTime(DateTime.UtcNow) }
            }, SetOptions.MergeAll);
            _logger.LogInformation("Stored active token in Firestore");

            var sessionId = Guid.NewGuid().ToString();
            var sessionDoc = _firestoreDb.Collection("sessions").Document(sessionId);
            await sessionDoc.SetAsync(new Dictionary<string, object>
            {
                { "userId", userRecord.Uid },
                { "refreshToken", refreshToken },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) },
                { "lastActive", Timestamp.FromDateTime(DateTime.UtcNow) }
            });
            _logger.LogInformation("Created new session document");

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = customToken,
                RefreshToken = refreshToken,
                UserId = userRecord.Uid,
                RequiresTwoFactor = false
            };
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogError($"Login failed: {ex.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
            return new LoginResponse
            {
                Success = false,
                Message = $"Login failed: {ex.Message}"
            };
            throw;
        }
    }

    public override async Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
    {
        try
        {
            var sessionsRef = _firestoreDb.Collection("sessions");
            var query = sessionsRef.WhereEqualTo("refreshToken", request.RefreshToken);
            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                var sessionDoc = snapshot.Documents[0].Reference;
                var sessionData = snapshot.Documents[0].ToDictionary();
                var userId = sessionData["userId"].ToString();

                await sessionDoc.DeleteAsync();

                return new LogoutResponse
                {
                    Success = true,
                    Message = "Logout successful."
                };
            }

            return new LogoutResponse
            {
                Success = false,
                Message = "Session not found."
            };
        }
        catch (Exception ex)
        {
            return new LogoutResponse
            {
                Success = false,
                Message = $"Logout failed: {ex.Message}"
            };
            throw;
        }
    }

    public override async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
    {
        const int MAX_RETRIES = 3;
        const int RETRY_DELAY_MS = 1000;
        int retryCount = 0;

        while (true)
        {
            try
            {
                _logger.LogInformation($"Token refresh attempt {retryCount + 1} for token: {request.RefreshToken}");

                var sessionQuery = _firestoreDb.Collection("sessions")
                    .WhereEqualTo("refreshToken", request.RefreshToken);
                var sessionSnapshot = await sessionQuery.GetSnapshotAsync();

                if (sessionSnapshot.Count == 0)
                {
                    _logger.LogWarning($"No session found for the provided refresh token: {request.RefreshToken}");
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Session has been invalidated"
                    };
                }

                var sessionData = sessionSnapshot.Documents[0].ToDictionary();
                var userId = sessionData["userId"].ToString();
                _logger.LogInformation($"Found session for user: {userId}");

                var userDoc = await _firestoreDb.Collection("users").Document(userId).GetSnapshotAsync();
                if (!userDoc.Exists)
                {
                    _logger.LogWarning($"User document not found for {userId}");
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Session has been invalidated"
                    };
                }

                var userData = userDoc.ToDictionary();
                if (!userData.ContainsKey("activeToken"))
                {
                    _logger.LogWarning($"No active token found for user {userId}");
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Session has been invalidated"
                    };
                }

                var activeToken = userData["activeToken"].ToString();
                _logger.LogInformation($"Active token in Firebase: {activeToken}");
                _logger.LogInformation($"Provided token: {request.RefreshToken}");

                if (activeToken != request.RefreshToken)
                {
                    _logger.LogWarning($"Provided token does not match the active token for user {userId}");
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Session has been invalidated"
                    };
                }

                var newAccessToken = await _firebaseAuth.CreateCustomTokenAsync(userId);
                var newRefreshToken = GenerateRefreshToken();
                _logger.LogInformation($"Generated new refresh token for user {userId}");

                try
                {
                    var batch = _firestoreDb.StartBatch();
                    batch.Update(sessionSnapshot[0].Reference, new Dictionary<string, object>
                    {
                        { "refreshToken", newRefreshToken },
                        { "lastActive", Timestamp.FromDateTime(DateTime.UtcNow) }
                    });

                    batch.Update(userDoc.Reference, "activeToken", newRefreshToken);
                    await batch.CommitAsync();
                    _logger.LogInformation("Successfully updated session and user documents");

                    return new RefreshTokenResponse
                    {
                        Success = true,
                        Message = "Token refreshed successfully",
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken
                    };
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is HttpProtocolException)
                {
                    _logger.LogWarning($"Network error during token refresh (attempt {retryCount + 1}): {ex.Message}");
                    if (retryCount < MAX_RETRIES - 1)
                    {
                        retryCount++;
                        await Task.Delay(RETRY_DELAY_MS * retryCount);
                        continue;
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token refresh failed: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");

                if (retryCount < MAX_RETRIES - 1)
                {
                    retryCount++;
                    await Task.Delay(RETRY_DELAY_MS * retryCount);
                    continue;
                }

                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = $"Token refresh failed after {MAX_RETRIES} attempts: {ex.Message}"
                };
                throw;
            }
        }
    }

    public override async Task<UpdateAccountResponse> UpdateAccount(UpdateAccountRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Account update attempt");

        try
        {
            var sessionQuery = _firestoreDb.Collection("sessions")
                .WhereEqualTo("refreshToken", request.RefreshToken);
            var sessionSnapshot = await sessionQuery.GetSnapshotAsync();

            if (sessionSnapshot.Count == 0)
            {
                _logger.LogWarning($"No session found for the provided refresh token: {request.RefreshToken}");
                return new UpdateAccountResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var sessionData = sessionSnapshot[0].ToDictionary();
            var userId = sessionData["userId"].ToString();
            _logger.LogInformation($"Found session for user: {userId}");

            var userDoc = await _firestoreDb.Collection("users").Document(userId).GetSnapshotAsync();
            if (!userDoc.Exists)
            {
                _logger.LogWarning($"User document not found for {userId}");
                return new UpdateAccountResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var userData = userDoc.ToDictionary();
            if (!userData.ContainsKey("activeToken"))
            {
                _logger.LogWarning($"No active token found for user {userId}");
                return new UpdateAccountResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var activeToken = userData["activeToken"].ToString();
            if (activeToken != request.RefreshToken)
            {
                _logger.LogWarning($"Provided token does not match the active token for user {userId}");
                return new UpdateAccountResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var updateArgs = new UserRecordArgs
            {
                Uid = userId
            };

            // Firebase Auth Update
            if (!string.IsNullOrEmpty(request.NewEmail))
            {
                updateArgs.Email = request.NewEmail;
            }
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                updateArgs.Password = request.NewPassword;
            }

            await _firebaseAuth.UpdateUserAsync(updateArgs);
            _logger.LogInformation($"Updated user in Firebase Auth: {userId}");

            // Firestore Update
            var updates = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(request.NewEmail))
            {
                updates["Email"] = request.NewEmail;
            }
            if (!string.IsNullOrEmpty(request.NewUsername))
            {
                updates["Username"] = request.NewUsername;
            }

            if (updates.Count > 0)
            {
                await userDoc.Reference.UpdateAsync(updates);
                _logger.LogInformation($"Updated user document in Firestore: {userId}");
            }

            var newAccessToken = await _firebaseAuth.CreateCustomTokenAsync(userId);
            var newRefreshToken = GenerateRefreshToken();

            var existingSessionDoc = sessionSnapshot[0].Reference;
            await existingSessionDoc.UpdateAsync(new Dictionary<string, object>
            {
                { "refreshToken", newRefreshToken },
                { "lastActive", Timestamp.FromDateTime(DateTime.UtcNow) }
            });

            await userDoc.Reference.UpdateAsync("activeToken", newRefreshToken);
            _logger.LogInformation("Successfully updated session and user documents");

            return new UpdateAccountResponse
            {
                Success = true,
                Message = "Account updated successfully",
                NewAccessToken = newAccessToken,
                NewRefreshToken = newRefreshToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Account update failed: {ex.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
            return new UpdateAccountResponse
            {
                Success = false,
                Message = $"Account update failed: {ex.Message}"
            };
            throw;
        }
    }

    public override async Task<RequestPasswordResetResponse> RequestPasswordReset(
        RequestPasswordResetRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation($"Password reset request for email: {request.Email}");

        try
        {
            try
            {
                await _firebaseAuth.GetUserByEmailAsync(request.Email);
            }
            catch (FirebaseAuthException ex)
            {
                return new RequestPasswordResetResponse
                {
                    Success = true,
                    Message = "If an account exists with this email, a reset code will be sent."
                };
                throw;
            }

            var resetCode = await _emailService.GenerateAndStoreResetCode(request.Email);
            await _emailService.SendPasswordResetEmail(request.Email, resetCode);

            return new RequestPasswordResetResponse
            {
                Success = true,
                Message = "If an account exists with this email, a reset code will be sent."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error requesting password reset: {ex.Message}");
            return new RequestPasswordResetResponse
            {
                Success = false,
                Message = "An error occurred while processing your request."
            };
            throw;
        }
    }

    public override async Task<VerifyResetCodeResponse> VerifyResetCode(
        VerifyResetCodeRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation($"Verifying reset code for email: {request.Email}");

        try
        {
            var isValid = await _emailService.VerifyResetCode(request.Email, request.ResetCode);
            if (!isValid)
            {
                return new VerifyResetCodeResponse
                {
                    Success = false,
                    Message = "Invalid or expired reset code."
                };
            }

            var verificationToken = GenerateRefreshToken();
            var verificationDoc = _firestoreDb.Collection("password_reset_verifications").Document();
            await verificationDoc.SetAsync(new
            {
                Email = request.Email,
                Token = verificationToken,
                ExpiryTime = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(5)),
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
            });

            return new VerifyResetCodeResponse
            {
                Success = true,
                Message = "Reset code verified successfully.",
                VerificationToken = verificationToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error verifying reset code: {ex.Message}");
            return new VerifyResetCodeResponse
            {
                Success = false,
                Message = "An error occurred while verifying the reset code."
            };
            throw;
        }
    }

    public override async Task<ResetPasswordResponse> ResetPassword(
        ResetPasswordRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Processing password reset request");

        try
        {
            var verificationQuery = _firestoreDb.Collection("password_reset_verifications")
                .WhereEqualTo("Token", request.VerificationToken)
                .OrderByDescending("CreatedAt")
                .Limit(1);

            var snapshot = await verificationQuery.GetSnapshotAsync();
            if (snapshot.Count == 0)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Invalid verification token."
                };
            }

            var verificationDoc = snapshot[0];
            var verificationData = verificationDoc.ToDictionary();
            var email = verificationData["Email"].ToString();
            var expiryTime = ((Timestamp)verificationData["ExpiryTime"]).ToDateTime();

            if (DateTime.UtcNow > expiryTime)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Verification token has expired."
                };
            }

            var userRecord = await _firebaseAuth.GetUserByEmailAsync(email);

            var updateArgs = new UserRecordArgs
            {
                Uid = userRecord.Uid,
                Password = request.NewPassword
            };
            await _firebaseAuth.UpdateUserAsync(updateArgs);

            await verificationDoc.Reference.DeleteAsync();

            await RevokeAllUserTokens(userRecord.Uid);

            return new ResetPasswordResponse
            {
                Success = true,
                Message = "Password reset successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error resetting password: {ex.Message}");
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "An error occurred while resetting the password."
            };
            throw;
        }
    }

    public override async Task<DeleteAccountResponse> DeleteAccount(
        DeleteAccountRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Processing account deletion request");

        try
        {
            var sessionQuery = _firestoreDb.Collection("sessions")
                .WhereEqualTo("refreshToken", request.RefreshToken);
            var sessionSnapshot = await sessionQuery.GetSnapshotAsync();

            if (sessionSnapshot.Count == 0)
            {
                _logger.LogWarning($"No session found for the provided refresh token: {request.RefreshToken}");
                return new DeleteAccountResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var sessionData = sessionSnapshot[0].ToDictionary();
            var userId = sessionData["userId"].ToString();
            _logger.LogInformation($"Found session for user: {userId}");

            var userDoc = await _firestoreDb.Collection("users").Document(userId).GetSnapshotAsync();
            if (!userDoc.Exists)
            {
                _logger.LogWarning($"User document not found for {userId}");
                return new DeleteAccountResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var userData = userDoc.ToDictionary();
            if (!userData.ContainsKey("activeToken"))
            {
                _logger.LogWarning($"No active token found for user {userId}");
                return new DeleteAccountResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var activeToken = userData["activeToken"].ToString();
            if (activeToken != request.RefreshToken)
            {
                _logger.LogWarning($"Provided token does not match the active token for user {userId}");
                return new DeleteAccountResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var authClient = new HttpClient();
            var apiKey = await _keyVaultService.GetSecretAsync("FirebaseApiKey");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Firebase API key not found in Key Vault.");
            }

            var signInRequest = new
            {
                email = userData["Email"].ToString(),
                password = request.Password,
                returnSecureToken = true
            };

            var response = await authClient.PostAsJsonAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}",
                signInRequest);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Password verification failed for user: {userId}");
                return new DeleteAccountResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            var sessionsRef = _firestoreDb.Collection("sessions");
            var sessionsQuery = sessionsRef.WhereEqualTo("userId", userId);
            var sessionsSnapshot = await sessionsQuery.GetSnapshotAsync();
            foreach (var doc in sessionsSnapshot)
            {
                await doc.Reference.DeleteAsync();
            }

            var resetVerificationsRef = _firestoreDb.Collection("password_reset_verifications");
            var resetVerificationsQuery = resetVerificationsRef.WhereEqualTo("Email", userData["Email"].ToString());
            var resetVerificationsSnapshot = await resetVerificationsQuery.GetSnapshotAsync();
            foreach (var doc in resetVerificationsSnapshot)
            {
                await doc.Reference.DeleteAsync();
            }

            await userDoc.Reference.DeleteAsync();
            await _firebaseAuth.DeleteUserAsync(userId);
            _logger.LogInformation($"User account deleted: {userId}");

            return new DeleteAccountResponse
            {
                Success = true,
                Message = "Account deleted successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting account: {ex.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
            return new DeleteAccountResponse
            {
                Success = false,
                Message = "An error occurred while deleting the account."
            };
            throw;
        }
    }

    public override async Task<Enable2FAResponse> Enable2FA(Enable2FARequest request, ServerCallContext context)
    {
        try
        {
            var sessionQuery = _firestoreDb.Collection("sessions")
                .WhereEqualTo("refreshToken", request.RefreshToken);
            var sessionSnapshot = await sessionQuery.GetSnapshotAsync();

            if (sessionSnapshot.Count == 0)
            {
                return new Enable2FAResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var sessionData = sessionSnapshot[0].ToDictionary();
            var userId = sessionData["userId"].ToString();

            var userDoc = await _firestoreDb.Collection("users").Document(userId).GetSnapshotAsync();
            if (!userDoc.Exists)
            {
                return new Enable2FAResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var userData = userDoc.ToDictionary();
            var email = userData["Email"].ToString();

            if (request.TwoFactorType == TwoFactorType.AuthenticatorApp)
            {
                var (secretKey, qrCodeUrl) = await _twoFactorService.GenerateAuthenticatorSetup(userId, email);

                await userDoc.Reference.UpdateAsync(new Dictionary<string, object>
                {
                    { "IsTwoFactorEnabled", true },
                    { "TwoFactorType", "AUTHENTICATOR_APP" },
                    { "AuthenticatorSecretKey", secretKey }
                });

                return new Enable2FAResponse
                {
                    Success = true,
                    Message = "2FA enabled with authenticator app. Please scan the QR code with your authenticator app.",
                    SecretKey = secretKey,
                    QrCodeUrl = qrCodeUrl
                };
            }
            else if (request.TwoFactorType == TwoFactorType.EmailCode)
            {
                var success = await _twoFactorService.SendEmailVerificationCode(userId, email);

                if (!success)
                {
                    await userDoc.Reference.UpdateAsync(new Dictionary<string, object>
                    {
                        { "IsTwoFactorEnabled", false },
                        { "TwoFactorType", "EMAIL_CODE" }
                    });
                }

                return new Enable2FAResponse
                {
                    Success = success,
                    Message = success ?
                        "2FA enabled with email verification. A verification code has been sent to your email." :
                        "Failed to enable 2FA with email verification."
                };
            }

            return new Enable2FAResponse
            {
                Success = false,
                Message = "Invalid 2FA type"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error enabling 2FA: {ex.Message}");
            return new Enable2FAResponse
            {
                Success = false,
                Message = "An error occurred while enabling 2FA"
            };
            throw;
        }
    }

    public override async Task<Disable2FAResponse> Disable2FA(Disable2FARequest request, ServerCallContext context)
    {
        try
        {
            var sessionQuery = _firestoreDb.Collection("sessions")
                .WhereEqualTo("refreshToken", request.RefreshToken);
            var sessionSnapshot = await sessionQuery.GetSnapshotAsync();

            if (sessionSnapshot.Count == 0)
            {
                return new Disable2FAResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var sessionData = sessionSnapshot[0].ToDictionary();
            var userId = sessionData["userId"].ToString();

            var userDoc = await _firestoreDb.Collection("users").Document(userId).GetSnapshotAsync();
            if (!userDoc.Exists)
            {
                return new Disable2FAResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var userData = userDoc.ToDictionary();
            var twoFactorType = userData["TwoFactorType"].ToString();

            bool isValid = false;
            if (twoFactorType == "AUTHENTICATOR_APP")
            {
                isValid = await _twoFactorService.VerifyAuthenticatorCode(userId, request.VerificationCode);
            }
            else if (twoFactorType == "EMAIL_CODE")
            {
                var email = userData["Email"].ToString();
                isValid = await _emailService.VerifyEmailCode(email, request.VerificationCode);
            }

            if (!isValid)
            {
                return new Disable2FAResponse
                {
                    Success = false,
                    Message = "Invalid verification code"
                };
            }

            var success = await _twoFactorService.Disable2FA(userId);
            return new Disable2FAResponse
            {
                Success = success,
                Message = success ? "2FA has been disabled" : "Failed to disable 2FA"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error disabling 2FA: {ex.Message}");
            return new Disable2FAResponse
            {
                Success = false,
                Message = "An error occurred while disabling 2FA"
            };
            throw;
        }
    }

    public override async Task<Verify2FAResponse> Verify2FA(Verify2FARequest request, ServerCallContext context)
    {
        try
        {
            if (IsRateLimited(request.Email, _verifyAttempts, MAX_VERIFY_ATTEMPTS, VERIFY_COOLDOWN_MINUTES))
            {
                _logger.LogWarning($"2FA verification rate limit exceeded for email: {request.Email}");
                return new Verify2FAResponse
                {
                    Success = false,
                    Message = $"Too many verification attempts. Please try again in {VERIFY_COOLDOWN_MINUTES} minutes."
                };
            }

            _logger.LogInformation($"2FA verification attempt for email: {request.Email}");

            var userRecord = await _firebaseAuth.GetUserByEmailAsync(request.Email);
            var userId = userRecord.Uid;
            _logger.LogInformation($"Found user with ID: {userId}");

            var userDoc = await _firestoreDb.Collection("users").Document(userId).GetSnapshotAsync();
            if (!userDoc.Exists)
            {
                IncrementAttempts(request.Email, _verifyAttempts);
                _logger.LogWarning($"User document not found for {userId}");
                return new Verify2FAResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var userData = userDoc.ToDictionary();
            var twoFactorType = userData["TwoFactorType"].ToString();
            _logger.LogInformation($"Verifying 2FA code for type: {twoFactorType}");

            bool isValid = false;
            if (twoFactorType == "AUTHENTICATOR_APP")
            {
                isValid = await _twoFactorService.VerifyAuthenticatorCode(userId, request.VerificationCode);
            }
            else if (twoFactorType == "EMAIL_CODE")
            {
                var email = userData["Email"].ToString();
                isValid = await _emailService.VerifyEmailCode(email, request.VerificationCode);
            }

            if (!isValid)
            {
                IncrementAttempts(request.Email, _verifyAttempts);
                _logger.LogWarning($"Invalid 2FA verification code for user: {userId}");
                return new Verify2FAResponse
                {
                    Success = false,
                    Message = "Invalid verification code"
                };
            }

            _verifyAttempts.Remove(request.Email);
            _logger.LogInformation($"2FA verification successful for user: {userId}");

            var customToken = await _firebaseAuth.CreateCustomTokenAsync(userId);
            var refreshToken = GenerateRefreshToken();
            _logger.LogInformation($"Generated new refresh token for user: {userId}");

            try
            {
                var batch = _firestoreDb.StartBatch();
                batch.Update(userDoc.Reference, new Dictionary<string, object>
                {
                    { "activeToken", refreshToken },
                    { "lastLogin", Timestamp.FromDateTime(DateTime.UtcNow) }
                });

                var sessionId = Guid.NewGuid().ToString();
                var sessionDoc = _firestoreDb.Collection("sessions").Document(sessionId);
                batch.Set(sessionDoc, new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "refreshToken", refreshToken },
                    { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) },
                    { "lastActive", Timestamp.FromDateTime(DateTime.UtcNow) }
                });

                await batch.CommitAsync();
                _logger.LogInformation($"Successfully created session for user: {userId}");

                return new Verify2FAResponse
                {
                    Success = true,
                    Message = "2FA verification successful",
                    AccessToken = customToken,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating session: {ex.Message}");
                return new Verify2FAResponse
                {
                    Success = false,
                    Message = "An error occurred while creating your session"
                };
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error verifying 2FA: {ex.Message}");
            return new Verify2FAResponse
            {
                Success = false,
                Message = "An error occurred while verifying 2FA"
            };
            throw;
        }
    }

    public override async Task<Get2FAStatusResponse> Get2FAStatus(Get2FAStatusRequest request, ServerCallContext context)
    {
        try
        {
            var sessionQuery = _firestoreDb.Collection("sessions")
                .WhereEqualTo("refreshToken", request.RefreshToken);
            var sessionSnapshot = await sessionQuery.GetSnapshotAsync();

            if (sessionSnapshot.Count == 0)
            {
                return new Get2FAStatusResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var sessionData = sessionSnapshot[0].ToDictionary();
            var userId = sessionData["userId"].ToString();

            var (isEnabled, type) = await _twoFactorService.Get2FAStatus(userId);
            return new Get2FAStatusResponse
            {
                Success = true,
                Message = "2FA status retrieved successfully",
                IsEnabled = isEnabled,
                TwoFactorType = type == "AUTHENTICATOR_APP" ? TwoFactorType.AuthenticatorApp :
                               type == "EMAIL_CODE" ? TwoFactorType.EmailCode :
                               TwoFactorType.None
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting 2FA status: {ex.Message}");
            return new Get2FAStatusResponse
            {
                Success = false,
                Message = "An error occurred while getting 2FA status"
            };
            throw;
        }
    }
}