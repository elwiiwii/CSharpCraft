using System.Security.Cryptography;
using Google.Cloud.Firestore;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace RaceServer.Services;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly FirestoreDb _firestoreDb;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private const int RESET_CODE_LENGTH = 8;
    private const int RESET_CODE_EXPIRY_MINUTES = 5;
    private const string VERIFICATION_CODES_COLLECTION = "verification_codes";

    public EmailService(
        ILogger<EmailService> logger,
        FirestoreDb firestoreDb,
        string smtpServer,
        int smtpPort,
        string smtpUsername,
        string smtpPassword)
    {
        _logger = logger;
        _firestoreDb = firestoreDb;
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _smtpUsername = smtpUsername;
        _smtpPassword = smtpPassword;
    }

    public async Task<string> GenerateAndStoreResetCode(string email)
    {
        try
        {
            var code = GenerateResetCode();
            var expiryTime = DateTime.UtcNow.AddMinutes(RESET_CODE_EXPIRY_MINUTES);

            var resetDoc = _firestoreDb.Collection(VERIFICATION_CODES_COLLECTION).Document();
            await resetDoc.SetAsync(new
            {
                Email = email,
                Code = code,
                Type = "password_reset",
                ExpiryTime = Timestamp.FromDateTime(expiryTime),
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                IsUsed = false
            });

            return code;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating reset code: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> VerifyResetCode(string email, string code)
    {
        try
        {
            var query = _firestoreDb.Collection(VERIFICATION_CODES_COLLECTION)
                .WhereEqualTo("Email", email)
                .WhereEqualTo("Code", code)
                .WhereEqualTo("Type", "password_reset")
                .WhereEqualTo("IsUsed", false)
                .OrderByDescending("CreatedAt")
                .Limit(1);

            var snapshot = await query.GetSnapshotAsync();
            if (snapshot.Count == 0)
            {
                return false;
            }

            var resetDoc = snapshot[0];
            var resetData = resetDoc.ToDictionary();
            var expiryTime = ((Timestamp)resetData["ExpiryTime"]).ToDateTime();

            if (DateTime.UtcNow > expiryTime)
            {
                return false;
            }

            await resetDoc.Reference.UpdateAsync("IsUsed", true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error verifying reset code: {ex.Message}");
            throw;
        }
    }

    public async Task SendPasswordResetEmail(string email, string resetCode)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("C# Craft Verification", _smtpUsername));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Password Reset Code";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = $@"Your password reset code is: {resetCode}

This code will expire in {RESET_CODE_EXPIRY_MINUTES} minutes.

If you did not request this password reset, please ignore this email."
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation($"Password reset email sent to {email}.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending password reset email: {ex.Message}");
            throw;
        }
    }

    public async Task<string> GenerateAndStoreVerificationCode(string email)
    {
        try
        {
            var code = GenerateResetCode();
            var expiryTime = DateTime.UtcNow.AddMinutes(RESET_CODE_EXPIRY_MINUTES);

            var verificationDoc = _firestoreDb.Collection(VERIFICATION_CODES_COLLECTION).Document();
            await verificationDoc.SetAsync(new
            {
                Email = email,
                Code = code,
                Type = "email_verification",
                ExpiryTime = Timestamp.FromDateTime(expiryTime),
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                IsUsed = false
            });

            return code;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating verification code: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> VerifyEmailCode(string email, string code)
    {
        try
        {
            var query = _firestoreDb.Collection(VERIFICATION_CODES_COLLECTION)
                .WhereEqualTo("Email", email)
                .WhereEqualTo("Code", code)
                .WhereEqualTo("Type", "email_verification")
                .WhereEqualTo("IsUsed", false)
                .OrderByDescending("CreatedAt")
                .Limit(1);

            var snapshot = await query.GetSnapshotAsync();
            if (snapshot.Count == 0)
            {
                return false;
            }

            var verificationDoc = snapshot[0];
            var verificationData = verificationDoc.ToDictionary();
            var expiryTime = ((Timestamp)verificationData["ExpiryTime"]).ToDateTime();

            if (DateTime.UtcNow > expiryTime)
            {
                return false;
            }

            await verificationDoc.Reference.UpdateAsync("IsUsed", true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error verifying email code: {ex.Message}");
            throw;
        }
    }

    public async Task SendVerificationEmail(string email, string verificationCode)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("C# Craft Verification", _smtpUsername));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Email Verification Code";
            var bodyBuilder = new BodyBuilder
            {
                TextBody = $@"Your email verification code is: {verificationCode}

This code will expire in {RESET_CODE_EXPIRY_MINUTES} minutes.

If you did not request this verification, please ignore this email.

Best regards,
Account System"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation($"Verification email sent to {email}.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending verification email: {ex.Message}");
            throw;
        }
    }

    private string GenerateResetCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new byte[RESET_CODE_LENGTH];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
        }

        var code = new char[RESET_CODE_LENGTH];
        for (int i = 0; i < RESET_CODE_LENGTH; i++)
        {
            code[i] = chars[random[i] % chars.Length];
        }

        return new string(code);
    }
}
