using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using backend.Common;
using backend.Models.DTOs;
using backend.Repositories;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new AppException("MSG-EMP-01", "Invalid username or password.", 401);
            }

            var user = await _userRepository.GetByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                throw new AppException("MSG-EMP-01", "Invalid username or password.", 401);
            }

            var computedHash = ComputeSha256Hash(request.Password);
            if (!string.Equals(user.PasswordHash, computedHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new AppException("MSG-EMP-01", "Invalid username or password.", 401);
            }

            if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                throw new AppException("MSG-EMP-02", "This employee account is inactive.", 403);
            }

            var roles = user.UserRoles?
                .Where(ur => ur.Role != null && !string.IsNullOrWhiteSpace(ur.Role.RoleName))
                .Select(ur => ur.Role!.RoleName)
                .Distinct()
                .ToList() ?? new List<string>();

            var primaryRole = roles.FirstOrDefault() ?? "Warehouse Staff";

            var token = GenerateJwtToken(user, primaryRole, roles);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                EmployeeCode = user.EmployeeCode,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = primaryRole,
                Roles = roles
            };

            return new LoginResponse
            {
                Token = token,
                User = userDto
            };
        }

        private string GenerateJwtToken(backend.Models.Entities.User user, string primaryRole, List<string> roles)
        {
            var secretKey = _configuration["Jwt:SecretKey"]
                ?? "EWMN_Secret_Key_For_Demo_Authentication_Token_2026_Minimum_32_Characters!";
            var issuer = _configuration["Jwt:Issuer"] ?? "EWMN";
            var audience = _configuration["Jwt:Audience"] ?? "EWMN";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, primaryRole)
            };

            foreach (var r in roles)
            {
                if (!string.Equals(r, primaryRole, StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Role, r));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
