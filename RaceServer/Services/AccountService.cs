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
    private const int LOGIN_COOLDOWN_MINUTES = 5;
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

    public async Task<bool> IsTokenValid(string userId, string token)
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
            var isValid = activeToken == token;

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

    public async Task UpdateActiveToken(string userId, string token)
    {
        _logger.LogInformation($"Updating active token for user {userId}");

        try
        {
            var userDoc = _firestoreDb.Collection("users").Document(userId);
            await userDoc.UpdateAsync("activeToken", token);
            _logger.LogInformation($"Active token updated for user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating active token for user {userId}");
            throw;
        }
    }

    private static string GenerateToken()
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

    public override async Task<VerifyEmailBeforeRegistrationResponse> VerifyEmailCodeForRegistration(
        VerifyEmailCodeForRegistrationRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation($"Verifying email code for registration: {request.Email}");

        try
        {
            var isValid = await _emailService.VerifyEmailCode(request.Email, request.VerificationCode.ToUpperInvariant());
            if (!isValid)
            {
                return new VerifyEmailBeforeRegistrationResponse
                {
                    Success = false,
                    Message = "Invalid or expired verification code."
                };
            }

            return new VerifyEmailBeforeRegistrationResponse
            {
                Success = true,
                Message = "Email verified successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error verifying email code: {ex.Message}");
            return new VerifyEmailBeforeRegistrationResponse
            {
                Success = false,
                Message = "An error occurred while verifying the code."
            };
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
                IsEmailVerified = true,
                ProfilePicture = 27,
                MainColor = "#7E2553",
                HexCodes = new[] {
                    "#7E2553",
                    "#FFCCAA",
                    "#AB5236"
                }
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
            var token = GenerateToken();

            await userDoc.SetAsync(new Dictionary<string, object>
            {
                { "activeToken", token },
                { "lastLoginToken", Timestamp.FromDateTime(DateTime.UtcNow) }
            }, SetOptions.MergeAll);

            var sessionId = Guid.NewGuid().ToString();
            var sessionDoc = _firestoreDb.Collection("sessions").Document(sessionId);
            await sessionDoc.SetAsync(new Dictionary<string, object>
            {
                { "userId", userRecord.Uid },
                { "refreshToken", token },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) },
                { "lastActive", Timestamp.FromDateTime(DateTime.UtcNow) }
            });

            return new VerifyEmailResponse
            {
                Success = true,
                Message = "Email verified successfully."
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

    private async Task CleanupOldData(string userId)
    {
        try
        {
            // Clean up old sessions for this user
            var sessionsRef = _firestoreDb.Collection("sessions");
            var sessionsQuery = sessionsRef.WhereEqualTo("userId", userId);
            var sessionsSnapshot = await sessionsQuery.GetSnapshotAsync();
            
            foreach (var doc in sessionsSnapshot)
            {
                await doc.Reference.DeleteAsync();
                _logger.LogInformation($"Deleted old session {doc.Id} for user {userId}");
            }

            // Clean up expired password resets
            var resetVerificationsRef = _firestoreDb.Collection("password_reset_verifications");
            var expiredResetsQuery = resetVerificationsRef
                .WhereLessThan("ExpiryTime", Timestamp.FromDateTime(DateTime.UtcNow));
            var expiredResetsSnapshot = await expiredResetsQuery.GetSnapshotAsync();
            
            foreach (var doc in expiredResetsSnapshot)
            {
                await doc.Reference.DeleteAsync();
                _logger.LogInformation($"Deleted expired password reset verification {doc.Id}");
            }

            // Clean up expired verification codes
            var verificationCodesRef = _firestoreDb.Collection("verification_codes");
            var expiredCodesQuery = verificationCodesRef
                .WhereLessThan("ExpiryTime", Timestamp.FromDateTime(DateTime.UtcNow));
            var expiredCodesSnapshot = await expiredCodesQuery.GetSnapshotAsync();
            
            foreach (var doc in expiredCodesSnapshot)
            {
                await doc.Reference.DeleteAsync();
                _logger.LogInformation($"Deleted expired verification code {doc.Id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error cleaning up old data: {ex.Message}");
            // Don't throw - we don't want cleanup failures to affect the main flow
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
            bool isTwoFactorEnabled = false;
            string twoFactorType = "NONE";

            if (userData.TryGetValue("IsTwoFactorEnabled", out var isEnabledObj) && 
                userData.TryGetValue("TwoFactorType", out var typeObj))
            {
                isTwoFactorEnabled = (bool)isEnabledObj;
                twoFactorType = typeObj.ToString();
            }

            if (isTwoFactorEnabled)
            {
                _logger.LogInformation($"Two-factor authentication is enabled for user: {userRecord.Uid}");

                if (twoFactorType == "EMAIL_CODE")
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
                    TwoFactorType = twoFactorType == "AUTHENTICATOR_APP" ?
                        TwoFactorType.AuthenticatorApp : TwoFactorType.EmailCode
                };
            }

            _logger.LogInformation($"2FA is not enabled for user {userRecord.Uid}, proceeding with normal login");

            var customToken = await _firebaseAuth.CreateCustomTokenAsync(userRecord.Uid);
            var token = GenerateToken();
            _logger.LogInformation($"Generated refresh token for user {userRecord.Uid}");

            await userDoc.Reference.SetAsync(new Dictionary<string, object>
            {
                { "activeToken", token },
                { "lastLoginToken", Timestamp.FromDateTime(DateTime.UtcNow) }
            }, SetOptions.MergeAll);
            _logger.LogInformation("Stored active token in Firestore");

            var sessionId = Guid.NewGuid().ToString();
            var sessionDoc = _firestoreDb.Collection("sessions").Document(sessionId);
            await sessionDoc.SetAsync(new Dictionary<string, object>
            {
                { "userId", userRecord.Uid },
                { "refreshToken", token },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) },
                { "lastActive", Timestamp.FromDateTime(DateTime.UtcNow) }
            });
            _logger.LogInformation("Created new session document");

            // Clean up old sessions and expired data
            await CleanupOldData(userRecord.Uid);

            // After successful login, generate a permanent token
            var permanentToken = GenerateToken();
            await userDoc.Reference.UpdateAsync(new Dictionary<string, object>
            {
                { "permanentToken", permanentToken },
                { "lastLogin", Timestamp.FromDateTime(DateTime.UtcNow) }
            });

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                UserId = userRecord.Uid,
                PermanentToken = permanentToken,
                RequiresTwoFactor = false,
                Username = userData.ContainsKey("Username") ? userData["Username"].ToString() : string.Empty
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
            var query = sessionsRef.WhereEqualTo("refreshToken", request.PermanentToken);
            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                var sessionDoc = snapshot.Documents[0].Reference;
                var sessionData = snapshot.Documents[0].ToDictionary();
                var userId = sessionData["userId"].ToString();

                // Remove the permanent token
                var userDoc = _firestoreDb.Collection("users").Document(userId);
                await userDoc.UpdateAsync("permanentToken", FieldValue.Delete);

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
        }
    }

    public override async Task<UpdateAccountResponse> UpdateAccount(UpdateAccountRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Account update attempt");

        try
        {
            var sessionQuery = _firestoreDb.Collection("sessions")
                .WhereEqualTo("refreshToken", request.PermanentToken);
            var sessionSnapshot = await sessionQuery.GetSnapshotAsync();

            if (sessionSnapshot.Count == 0)
            {
                _logger.LogWarning($"No session found for the provided refresh token: {request.PermanentToken}");
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
            if (activeToken != request.PermanentToken)
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
            var newToken = GenerateToken();

            var existingSessionDoc = sessionSnapshot[0].Reference;
            await existingSessionDoc.UpdateAsync(new Dictionary<string, object>
            {
                { "refreshToken", newToken },
                { "lastActive", Timestamp.FromDateTime(DateTime.UtcNow) }
            });

            await userDoc.Reference.UpdateAsync("activeToken", newToken);
            _logger.LogInformation("Successfully updated session and user documents");

            return new UpdateAccountResponse
            {
                Success = true,
                Message = "Account updated successfully"
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

            var verificationToken = GenerateToken();
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
                .WhereEqualTo("refreshToken", request.PermanentToken);
            var sessionSnapshot = await sessionQuery.GetSnapshotAsync();

            if (sessionSnapshot.Count == 0)
            {
                _logger.LogWarning($"No session found for the provided refresh token: {request.PermanentToken}");
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
            if (activeToken != request.PermanentToken)
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
                .WhereEqualTo("refreshToken", request.PermanentToken);
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
                .WhereEqualTo("refreshToken", request.PermanentToken);
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
            var token = GenerateToken();
            _logger.LogInformation($"Generated new refresh token for user: {userId}");

            try
            {
                var batch = _firestoreDb.StartBatch();
                batch.Update(userDoc.Reference, new Dictionary<string, object>
                {
                    { "activeToken", token },
                    { "lastLogin", Timestamp.FromDateTime(DateTime.UtcNow) }
                });

                var sessionId = Guid.NewGuid().ToString();
                var sessionDoc = _firestoreDb.Collection("sessions").Document(sessionId);
                batch.Set(sessionDoc, new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "refreshToken", token },
                    { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) },
                    { "lastActive", Timestamp.FromDateTime(DateTime.UtcNow) }
                });

                await batch.CommitAsync();
                _logger.LogInformation($"Successfully created session for user: {userId}");

                // Clean up old sessions and expired data
                await CleanupOldData(userId);

                return new Verify2FAResponse
                {
                    Success = true,
                    Message = "2FA verification successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating session after 2FA verification: {ex.Message}");
                return new Verify2FAResponse
                {
                    Success = false,
                    Message = "An error occurred while creating your session."
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during 2FA verification: {ex.Message}");
            return new Verify2FAResponse
            {
                Success = false,
                Message = "An error occurred during verification."
            };
        }
    }

    public override async Task<Get2FAStatusResponse> Get2FAStatus(Get2FAStatusRequest request, ServerCallContext context)
    {
        try
        {
            var sessionQuery = _firestoreDb.Collection("sessions")
                .WhereEqualTo("refreshToken", request.PermanentToken);
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

    public override async Task<CheckTokenResponse> CheckToken(CheckTokenRequest request, ServerCallContext context)
    {
        try
        {
            var userDoc = await _firestoreDb.Collection("users").Document(request.UserId).GetSnapshotAsync();
            if (!userDoc.Exists)
            {
                return new CheckTokenResponse
                {
                    IsValid = false,
                    Message = "User not found"
                };
            }

            var userData = userDoc.ToDictionary();
            if (!userData.ContainsKey("permanentToken"))
            {
                return new CheckTokenResponse
                {
                    IsValid = false,
                    Message = "No permanent token found"
                };
            }

            var storedToken = userData["permanentToken"].ToString();
            var isValid = storedToken == request.Token;

            return new CheckTokenResponse
            {
                IsValid = isValid,
                Message = isValid ? "Token is valid" : "Token is invalid"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking token: {ex.Message}");
            return new CheckTokenResponse
            {
                IsValid = false,
                Message = "Error checking token"
            };
        }
    }

    public override async Task<CheckEmailExistsResponse> CheckEmailExists(
        CheckEmailExistsRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation($"Checking if email exists: {request.Email}");

        try
        {
            try
            {
                await _firebaseAuth.GetUserByEmailAsync(request.Email);
                return new CheckEmailExistsResponse
                {
                    Exists = true,
                    Message = "An account with this email already exists."
                };
            }
            catch (FirebaseAuthException)
            {
                return new CheckEmailExistsResponse
                {
                    Exists = false,
                    Message = "Email is available for registration."
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking email existence for {request.Email}");
            return new CheckEmailExistsResponse
            {
                Exists = false,
                Message = "An error occurred while checking email existence."
            };
        }
    }

    public override async Task<UpdateProfileCustomizationResponse> UpdateProfileCustomization(
        UpdateProfileCustomizationRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Processing profile customization update request");

        try
        {
            var sessionQuery = _firestoreDb.Collection("sessions")
                .WhereEqualTo("refreshToken", request.PermanentToken);
            var sessionSnapshot = await sessionQuery.GetSnapshotAsync();

            if (sessionSnapshot.Count == 0)
            {
                _logger.LogWarning($"No session found for the provided refresh token: {request.PermanentToken}");
                return new UpdateProfileCustomizationResponse
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
                return new UpdateProfileCustomizationResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var userData = userDoc.ToDictionary();
            if (!userData.ContainsKey("activeToken"))
            {
                _logger.LogWarning($"No active token found for user {userId}");
                return new UpdateProfileCustomizationResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            var activeToken = userData["activeToken"].ToString();
            if (activeToken != request.PermanentToken)
            {
                _logger.LogWarning($"Provided token does not match the active token for user {userId}");
                return new UpdateProfileCustomizationResponse
                {
                    Success = false,
                    Message = "Session has been invalidated"
                };
            }

            // Validate hex codes
            if (request.HexCodes.Count == 0)
            {
                return new UpdateProfileCustomizationResponse
                {
                    Success = false,
                    Message = "At least one hex code is required"
                };
            }

            foreach (var hexCode in request.HexCodes)
            {
                if (!IsValidHexColor(hexCode))
                {
                    return new UpdateProfileCustomizationResponse
                    {
                        Success = false,
                        Message = $"Invalid hex color code: {hexCode}"
                    };
                }
            }

            // Get existing hex codes
            var existingHexCodes = userData.ContainsKey("HexCodes") ? 
                ((IEnumerable<object>)userData["HexCodes"]).Select(x => x.ToString()).ToList() : 
                new List<string>();

            // Update only the hex codes that were provided
            var updatedHexCodes = new List<string>(existingHexCodes);
            for (int i = 0; i < request.HexCodes.Count; i++)
            {
                if (i < updatedHexCodes.Count)
                {
                    updatedHexCodes[i] = request.HexCodes[i];
                }
                else
                {
                    updatedHexCodes.Add(request.HexCodes[i]);
                }
            }

            // Update the user document
            await userDoc.Reference.UpdateAsync(new Dictionary<string, object>
            {
                { "ProfilePicture", request.ProfilePicture },
                { "HexCodes", updatedHexCodes }
            });

            _logger.LogInformation($"Updated profile customization for user: {userId}");

            return new UpdateProfileCustomizationResponse
            {
                Success = true,
                Message = "Profile customization updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating profile customization: {ex.Message}");
            return new UpdateProfileCustomizationResponse
            {
                Success = false,
                Message = "An error occurred while updating profile customization"
            };
        }
    }

    private bool IsValidHexColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor))
            return false;

        // Remove # if present
        hexColor = hexColor.TrimStart('#');

        // Check if it's a valid hex color (6 characters, all valid hex digits)
        return hexColor.Length == 6 && hexColor.All(c => 
            (c >= '0' && c <= '9') || 
            (c >= 'a' && c <= 'f') || 
            (c >= 'A' && c <= 'F'));
    }

    public override async Task<GetUserByUsernameResponse> GetUserByUsername(GetUserByUsernameRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Fetching user by username: {request.Username}");
        try
        {
            var usersRef = _firestoreDb.Collection("users");
            var query = usersRef.WhereEqualTo("Username", request.Username).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            if (snapshot.Count == 0)
            {
                return new GetUserByUsernameResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }
            var userDoc = snapshot[0];
            var userData = userDoc.ToDictionary();
            return new GetUserByUsernameResponse
            {
                Success = true,
                Message = "User found.",
                UserId = userDoc.Id,
                Username = userData.ContainsKey("Username") ? userData["Username"].ToString() : string.Empty,
                ProfilePicture = userData.ContainsKey("ProfilePicture") ? Convert.ToInt32(userData["ProfilePicture"]) : 27,
                NameColor = userData.ContainsKey("NameColor") ? userData["NameColor"].ToString() : "#FFFFFF",
                ShadowColor = userData.ContainsKey("ShadowColor") ? userData["ShadowColor"].ToString() : "#C2C3C7",
                OutlineColor = userData.ContainsKey("OutlineColor") ? userData["OutlineColor"].ToString() : "#FFFFFF",
                BackgroundColor = userData.ContainsKey("BackgroundColor") ? userData["BackgroundColor"].ToString() : "#111D35",
                HexCodes = { userData.ContainsKey("HexCodes") ? ((IEnumerable<object>)userData["HexCodes"]).Select(x => x.ToString()) : new[] { "#7E2553", "#FFCCAA", "#AB5236" } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user by username: {ex.Message}");
            return new GetUserByUsernameResponse
            {
                Success = false,
                Message = "An error occurred while fetching the user."
            };
        }
    }
}