using Google.Cloud.Firestore;
using OtpNet;

namespace RaceServer.Services;

public class TwoFactorService
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<TwoFactorService> _logger;
    private readonly EmailService _emailService;

    public TwoFactorService(
        FirestoreDb firestoreDb,
        ILogger<TwoFactorService> logger,
        EmailService emailService)
    {
        _firestoreDb = firestoreDb;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<(string SecretKey, string QrCodeUrl)> GenerateAuthenticatorSetup(string userId, string email)
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        var secretKey = Base32Encoding.ToString(key);

        var totp = new Totp(key);

        var issuer = "C# Craft";
        var qrCodeUrl = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";

        var userDoc = _firestoreDb.Collection("users").Document(userId);
        await userDoc.SetAsync(new Dictionary<string, object>
        {
            { "TwoFactorSecret", secretKey },
            { "TwoFactorType", "AUTHENTICATOR_APP" },
            { "TwoFactorEnabled", true }
        });

        return (secretKey, qrCodeUrl);
    }

    public async Task<bool> VerifyAuthenticatorCode(string userId, string code)
    {
        try
        {
            var userDoc = await _firestoreDb.Collection("users").Document(userId).GetSnapshotAsync();
            if (!userDoc.Exists)
            {
                return false;
            }

            var userData = userDoc.ToDictionary();
            if (!userData.ContainsKey("TwoFactorSecret"))
            {
                return false;
            }

            var secretKey = userData["TwoFactorSecret"].ToString();
            var key = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(key);

            return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error verifying authenticator code: {ex.Message}");
            return false;
            throw;
        }
    }

    public async Task<bool> SendEmailVerificationCode(string email, string code)
    {
        try
        {
            var verificationCode = await _emailService.GenerateAndStoreResetCode(email);

            await _emailService.SendVerificationEmail(email, verificationCode);

            var userDoc = _firestoreDb.Collection("users").Document(email);
            await userDoc.SetAsync(new Dictionary<string, object>
            {
                { "TwoFactorType", "EMAIL_CODE" },
                { "IsTwoFactorEnabled", true }
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending email verification code: {ex.Message}");
            return false;
            throw;
        }
    }

    public async Task<bool> Disable2FA(string userId)
    {
        try
        {
            var userDoc = _firestoreDb.Collection("users").Document(userId);
            await userDoc.SetAsync(new Dictionary<string, object>
            {
                { "TwoFactorSecret", FieldValue.Delete },
                { "TwoFactorType", "NONE" },
                { "IsTwoFactorEnabled", false }
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error disabling 2FA: {ex.Message}");
            return false;
            throw;
        }
    }

    public async Task<(bool IsEnabled, string Type)> Get2FAStatus(string userId)
    {
        try
        {
            var userDoc = await _firestoreDb.Collection("users").Document(userId).GetSnapshotAsync();
            if (!userDoc.Exists)
            {
                return (false, "NONE");
            }

            var userData = userDoc.ToDictionary();
            var isEnabled = userData.ContainsKey("IsTwoFactorEnabled") && (bool)userData["IsTwoFactorEnabled"];
            var type = userData.ContainsKey("TwoFactorType") ? userData["TwoFactorType"].ToString() : "NONE";

            return (isEnabled, type);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving 2FA status: {ex.Message}");
            return (false, "NONE");
            throw;
        }
    }
}
