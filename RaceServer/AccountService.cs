using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using RaceServer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceServer
{
    public class AccountServiceImpl : AccountService.AccountServiceBase
    {
        private readonly AccountDatabase _db;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public AccountServiceImpl(AccountDatabase db)
        {
            _db = db;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public override async Task<RegisterResponse> Register(
            RegisterRequest request,
            ServerCallContext context)
        {
            try
            {
                var account = new Account
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow
                };

                return _db.CreateUser(account)
                    ? new RegisterResponse { Success = true }
                    : new RegisterResponse { Success = false, ErrorMessage = "Username exists" };
            }
            catch
            {
                return new RegisterResponse { Success = false, ErrorMessage = "Server error" };
            }
        }

        public override async Task<LoginResponse> Login(
            LoginRequest request,
            ServerCallContext context)
        {
            var account = _db.GetUser(request.Username);
            if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
            {
                return new LoginResponse { Success = false, ErrorMessage = "Invalid credentials" };
            }

            var token = GenerateJwtToken(account);
            return new LoginResponse { Success = true, JwtToken = token };
        }

        private string GenerateJwtToken(Account account)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "c#-craft-server",
                audience: "c#-craft-client",
                claims: new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, account.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                },
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return _tokenHandler.WriteToken(token);
        }
    }
}