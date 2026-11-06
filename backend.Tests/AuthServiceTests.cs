using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using backend.Common;
using backend.Models.DTOs;
using backend.Models.Entities;
using backend.Repositories;
using backend.Services;

namespace backend.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly IConfiguration _configuration;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "TestSecretKeyForAuthServiceUnitTestingPurposesOnly123456!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new AuthService(_mockUserRepo.Object, _configuration);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsLoginResponseWithJwtToken()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                EmployeeCode = "EMP001",
                FullName = "Test Staff",
                Email = "staff@example.com",
                Phone = "0123456789",
                PasswordHash = HashPassword("Password123!"),
                Status = "Active",
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        RoleId = 1,
                        Role = new Role { RoleId = 1, RoleName = "Purchase Staff" }
                    }
                }
            };
            _mockUserRepo.Setup(r => r.GetByEmailAsync("staff@example.com")).ReturnsAsync(user);

            var request = new LoginRequest
            {
                Email = "staff@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await _service.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.NotNull(result.User);
            Assert.Equal(1, result.User.UserId);
            Assert.Equal("EMP001", result.User.EmployeeCode);
            Assert.Equal("Test Staff", result.User.FullName);
            Assert.Equal("staff@example.com", result.User.Email);
            Assert.Equal("Purchase Staff", result.User.Role);
            Assert.Contains("Purchase Staff", result.User.Roles);

            // Validate JWT Token Claims
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result.Token);
            Assert.Equal("TestIssuer", token.Issuer);
            Assert.Contains("TestAudience", token.Audiences);
            Assert.Equal("1", token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal("staff@example.com", token.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal("Test Staff", token.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal("Purchase Staff", token.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public async Task LoginAsync_UserWithMultipleRoles_PopulatesAllRolesAndPrimaryRole()
        {
            // Arrange
            var user = new User
            {
                UserId = 2,
                EmployeeCode = "MGR001",
                FullName = "Manager User",
                Email = "mgr@example.com",
                PasswordHash = HashPassword("Admin123!"),
                Status = "Active",
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        RoleId = 2,
                        Role = new Role { RoleId = 2, RoleName = "Warehouse Manager" }
                    },
                    new UserRole
                    {
                        RoleId = 4,
                        Role = new Role { RoleId = 4, RoleName = "Administrator" }
                    }
                }
            };
            _mockUserRepo.Setup(r => r.GetByEmailAsync("mgr@example.com")).ReturnsAsync(user);

            var request = new LoginRequest
            {
                Email = "mgr@example.com",
                Password = "Admin123!"
            };

            // Act
            var result = await _service.LoginAsync(request);

            // Assert
            Assert.Equal("Warehouse Manager", result.User.Role);
            Assert.Equal(2, result.User.Roles.Count);
            Assert.Contains("Warehouse Manager", result.User.Roles);
            Assert.Contains("Administrator", result.User.Roles);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result.Token);
            var roleClaims = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            Assert.Contains("Warehouse Manager", roleClaims);
            Assert.Contains("Administrator", roleClaims);
        }

        [Theory]
        [InlineData(null, "pass")]
        [InlineData("", "pass")]
        [InlineData("   ", "pass")]
        [InlineData("email@example.com", null)]
        [InlineData("email@example.com", "")]
        [InlineData("email@example.com", "   ")]
        public async Task LoginAsync_MissingEmailOrPassword_ThrowsAppExceptionMSGEMP01(string? email, string? password)
        {
            var request = new LoginRequest { Email = email!, Password = password! };
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.LoginAsync(request));
            Assert.Equal("MSG-EMP-01", ex.Code);
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_NullRequest_ThrowsAppExceptionMSGEMP01()
        {
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.LoginAsync(null!));
            Assert.Equal("MSG-EMP-01", ex.Code);
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsAppExceptionMSGEMP01()
        {
            _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            var request = new LoginRequest { Email = "notfound@example.com", Password = "Password123!" };

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.LoginAsync(request));
            Assert.Equal("MSG-EMP-01", ex.Code);
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsAppExceptionMSGEMP01()
        {
            var user = new User
            {
                UserId = 1,
                Email = "staff@example.com",
                PasswordHash = HashPassword("CorrectPassword"),
                Status = "Active"
            };
            _mockUserRepo.Setup(r => r.GetByEmailAsync("staff@example.com")).ReturnsAsync(user);

            var request = new LoginRequest { Email = "staff@example.com", Password = "WrongPassword" };

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.LoginAsync(request));
            Assert.Equal("MSG-EMP-01", ex.Code);
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ThrowsAppExceptionMSGEMP02()
        {
            var user = new User
            {
                UserId = 1,
                Email = "staff@example.com",
                PasswordHash = HashPassword("Password123!"),
                Status = "Inactive"
            };
            _mockUserRepo.Setup(r => r.GetByEmailAsync("staff@example.com")).ReturnsAsync(user);

            var request = new LoginRequest { Email = "staff@example.com", Password = "Password123!" };

            var ex = await Assert.ThrowsAsync<AppException>(() => _service.LoginAsync(request));
            Assert.Equal("MSG-EMP-02", ex.Code);
            Assert.Equal(403, ex.StatusCode);
        }
    }
}
