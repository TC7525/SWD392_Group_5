using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ewms_common.Authentications;
using ewms_common.Configurations;
using ewms_infra.Models.Accounts;
using ewms_infra.Models.Repository;
using Microsoft.IdentityModel.Tokens;

namespace ewms_service.Utils
{
    public static class JwtHelper
    {
        public static string GenerateAccessToken(
            user account,
            role? primaryRole,
            warehouse? primaryWarehouse,
            out DateTime expiresAt)
        {
            var secret = AppSettings.Get<string>("JwtConfig:Secret", "EWMS_Super_Secret_Key_2026_SWD392_Group5_32Bytes!") ?? "EWMS_Super_Secret_Key_2026_SWD392_Group5_32Bytes!";
            var issuer = AppSettings.Get<string>("JwtConfig:ValidIssuer", "ewms_api") ?? "ewms_api";
            var audience = AppSettings.Get<string>("JwtConfig:ValidAudience", "ewms_client") ?? "ewms_client";
            var expiryMinutes = AppSettings.Get<int>("JwtConfig:TokenValidityInMinutes", 480);
            if (expiryMinutes <= 0) expiryMinutes = 480;

            expiresAt = DateTime.Now.AddMinutes(expiryMinutes);

            var roleCode = primaryRole?.role_code ?? string.Empty;
            var roleId = primaryRole?.id ?? string.Empty;
            var warehouseId = primaryWarehouse?.id ?? string.Empty;
            var warehouseCode = primaryWarehouse?.warehouse_code ?? string.Empty;
            var warehouseName = primaryWarehouse?.warehouse_name ?? string.Empty;

            var claims = new List<Claim>
            {
                new Claim(CustomJwtClaimType.UserId, account.id),
                new Claim(CustomJwtClaimType.EmployeeCode, account.employee_code),
                new Claim(CustomJwtClaimType.Username, account.employee_code),
                new Claim(CustomJwtClaimType.Email, account.email),
                new Claim(CustomJwtClaimType.Phone, account.phone ?? string.Empty),
                new Claim(CustomJwtClaimType.Fullname, account.full_name),
                new Claim(CustomJwtClaimType.WarehouseId, warehouseId),
                new Claim(CustomJwtClaimType.WarehouseCode, warehouseCode),
                new Claim(CustomJwtClaimType.WarehouseName, warehouseName),
                new Claim(CustomJwtClaimType.Role, roleCode),
                new Claim(CustomJwtClaimType.RoleId, roleId),
                new Claim(CustomJwtClaimType.SystemRole, roleCode),
                new Claim(ClaimTypes.NameIdentifier, account.id),
                new Claim(ClaimTypes.Name, account.employee_code),
                new Claim(ClaimTypes.Role, roleCode),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                expires: expiresAt.ToUniversalTime(),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
