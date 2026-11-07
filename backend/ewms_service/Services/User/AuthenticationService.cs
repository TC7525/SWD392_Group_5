using ewms_common.Authentications;
using ewms_common.Exceptions;
using ewms_common.Helpers;
using ewms_common.Infras.Models;
using ewms_infra.Context;
using ewms_infra.Enums;
using ewms_infra.Models.Accounts;
using ewms_infra.Models.Repository;
using ewms_service.Consts;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.User.Authentication.Request;
using ewms_service.DTOs.User.Authentication.Response;
using ewms_service.Services.AccessLog;
using ewms_service.Utils;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Services.User
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null);
        Task<UserProfileView> GetProfileAsync(UserHeader currentUser);
        Task<UserProfileView> UpdateProfileAsync(UpdateProfileRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<bool> ChangePasswordAsync(ChangePasswordRequest request, UserHeader currentUser, string? ipAddress = null);
        Task<LoginResponse> SwitchWarehouseAsync(SwitchWarehouseRequest request, UserHeader currentUser);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly ewms_context _context;
        private readonly IAccessLogService _accessLogService;

        public AuthenticationService(ewms_context context, IAccessLogService accessLogService)
        {
            _context = context;
            _accessLogService = accessLogService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null)
        {
            var identifier = request.username.Trim().ToLower();
            var account = await _context.users
                .Include(u => u.user_roles)
                    .ThenInclude(ur => ur.role)
                        .ThenInclude(r => r!.role_permissions)
                            .ThenInclude(rp => rp.permission)
                .Include(u => u.user_warehouses)
                    .ThenInclude(uw => uw.warehouse)
                .FirstOrDefaultAsync(u =>
                    u.email.ToLower() == identifier ||
                    u.employee_code.ToLower() == identifier);

            if (account == null)
            {
                throw new BadException(ErrorMessage.MSG02_InvalidCredentials);
            }

            if (account.status != eStatus_Account.Active)
            {
                throw new ForbiddenException(ErrorMessage.MSG03_AccountInactiveOrLocked);
            }

            var isValidPassword = HashingHelper.VerifyPassword(
                request.password,
                account.password_hash,
                account.password_salt);

            if (!isValidPassword)
            {
                throw new BadException(ErrorMessage.MSG02_InvalidCredentials);
            }

            var primaryRole = account.user_roles
                .Select(ur => ur.role)
                .FirstOrDefault(r => r != null && r.status == eStatus_Common.Active);

            var assignedWarehouses = await GetAssignedWarehousesAsync(account, primaryRole);

            warehouse? selectedWarehouse = null;
            if (!string.IsNullOrWhiteSpace(request.warehouse_id))
            {
                var foundWh = assignedWarehouses.FirstOrDefault(w => w.warehouse_id == request.warehouse_id);
                if (foundWh == null)
                {
                    throw new ForbiddenException(ErrorMessage.WarehouseNotAssigned);
                }
                selectedWarehouse = await _context.warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.id == foundWh.warehouse_id);
            }
            else if (assignedWarehouses.Any())
            {
                var firstWhId = assignedWarehouses.First().warehouse_id;
                selectedWarehouse = await _context.warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.id == firstWhId);
            }

            var token = JwtHelper.GenerateAccessToken(account, primaryRole, selectedWarehouse, out var expiresAt);
            var profile = BuildUserProfile(account, primaryRole, selectedWarehouse, assignedWarehouses);

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = account.id,
                module = "AUTH",
                action = "LOGIN",
                entity_type = "users",
                entity_id = account.id,
                description = $"Người dùng '{account.full_name}' ({account.employee_code}) đăng nhập vào hệ thống.",
                ip_address = ipAddress
            });

            return new LoginResponse
            {
                access_token = token,
                expires_at = expiresAt,
                user = profile
            };
        }

        public async Task<UserProfileView> GetProfileAsync(UserHeader currentUser)
        {
            var account = await _context.users
                .AsNoTracking()
                .Include(u => u.user_roles)
                    .ThenInclude(ur => ur.role)
                        .ThenInclude(r => r!.role_permissions)
                            .ThenInclude(rp => rp.permission)
                .Include(u => u.user_warehouses)
                    .ThenInclude(uw => uw.warehouse)
                .FirstOrDefaultAsync(u => u.id == currentUser.user_id)
                ?? throw new NotFoundException(ErrorMessage.UserNotFound);

            var primaryRole = account.user_roles.Select(ur => ur.role).FirstOrDefault(r => r != null);
            var assignedWarehouses = await GetAssignedWarehousesAsync(account, primaryRole);

            warehouse? selectedWarehouse = null;
            if (!string.IsNullOrWhiteSpace(currentUser.warehouse_id))
            {
                selectedWarehouse = await _context.warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.id == currentUser.warehouse_id);
            }
            else if (assignedWarehouses.Any())
            {
                var firstWhId = assignedWarehouses.First().warehouse_id;
                selectedWarehouse = await _context.warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.id == firstWhId);
            }

            return BuildUserProfile(account, primaryRole, selectedWarehouse, assignedWarehouses);
        }

        public async Task<UserProfileView> UpdateProfileAsync(UpdateProfileRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            var account = await _context.users.FirstOrDefaultAsync(u => u.id == currentUser.user_id)
                ?? throw new NotFoundException(ErrorMessage.UserNotFound);

            account.full_name = request.full_name.Trim();
            account.phone = request.phone?.Trim();
            account.Modified(currentUser.user_id);

            _context.users.Update(account);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "AUTH",
                action = "UPDATE_PROFILE",
                entity_type = "users",
                entity_id = account.id,
                description = $"Cập nhật thông tin cá nhân '{account.full_name}'.",
                ip_address = ipAddress
            });

            return await GetProfileAsync(currentUser);
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request, UserHeader currentUser, string? ipAddress = null)
        {
            if (request.new_password != request.confirm_password)
            {
                throw new BadException(ErrorMessage.PasswordConfirmationMismatch);
            }

            var account = await _context.users.FirstOrDefaultAsync(u => u.id == currentUser.user_id)
                ?? throw new NotFoundException(ErrorMessage.UserNotFound);

            var isOldValid = HashingHelper.VerifyPassword(request.old_password, account.password_hash, account.password_salt);
            if (!isOldValid)
            {
                throw new BadException(ErrorMessage.InvalidOldPassword);
            }

            var newSalt = HashingHelper.GenerateSalt();
            account.password_salt = newSalt;
            account.password_hash = HashingHelper.HashPassword(request.new_password, newSalt);
            account.Modified(currentUser.user_id);

            _context.users.Update(account);
            await _context.SaveChangesAsync();

            await _accessLogService.CreateAsync(new AccessLogCreating
            {
                user_id = currentUser.user_id,
                module = "AUTH",
                action = "CHANGE_PASSWORD",
                entity_type = "users",
                entity_id = account.id,
                description = $"Người dùng '{account.employee_code}' đổi mật khẩu.",
                ip_address = ipAddress
            });

            return true;
        }

        public async Task<LoginResponse> SwitchWarehouseAsync(SwitchWarehouseRequest request, UserHeader currentUser)
        {
            await ValidateHelper.EnsureWarehouseAccessAsync(_context, currentUser, request.warehouse_id);

            var account = await _context.users
                .AsNoTracking()
                .Include(u => u.user_roles)
                    .ThenInclude(ur => ur.role)
                        .ThenInclude(r => r!.role_permissions)
                            .ThenInclude(rp => rp.permission)
                .Include(u => u.user_warehouses)
                    .ThenInclude(uw => uw.warehouse)
                .FirstOrDefaultAsync(u => u.id == currentUser.user_id)
                ?? throw new NotFoundException(ErrorMessage.UserNotFound);

            var primaryRole = account.user_roles.Select(ur => ur.role).FirstOrDefault(r => r != null);
            var assignedWarehouses = await GetAssignedWarehousesAsync(account, primaryRole);
            var selectedWarehouse = await _context.warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.id == request.warehouse_id)
                ?? throw new NotFoundException(ErrorMessage.WarehouseNotFound);

            var token = JwtHelper.GenerateAccessToken(account, primaryRole, selectedWarehouse, out var expiresAt);
            var profile = BuildUserProfile(account, primaryRole, selectedWarehouse, assignedWarehouses);

            return new LoginResponse
            {
                access_token = token,
                expires_at = expiresAt,
                user = profile
            };
        }

        private async Task<List<AssignedWarehouseView>> GetAssignedWarehousesAsync(user account, role? primaryRole)
        {
            if (primaryRole?.role_code == SystemRoles.Administrator)
            {
                return await _context.warehouses
                    .AsNoTracking()
                    .Where(w => w.status == eStatus_Warehouse.Active)
                    .OrderBy(w => w.warehouse_code)
                    .Select(w => new AssignedWarehouseView
                    {
                        warehouse_id = w.id,
                        warehouse_code = w.warehouse_code,
                        warehouse_name = w.warehouse_name,
                        address = w.address
                    })
                    .ToListAsync();
            }

            return account.user_warehouses
                .Where(uw => uw.warehouse != null && uw.warehouse.status == eStatus_Warehouse.Active)
                .Select(uw => new AssignedWarehouseView
                {
                    warehouse_id = uw.warehouse!.id,
                    warehouse_code = uw.warehouse.warehouse_code,
                    warehouse_name = uw.warehouse.warehouse_name,
                    address = uw.warehouse.address
                })
                .OrderBy(w => w.warehouse_code)
                .ToList();
        }

        private static UserProfileView BuildUserProfile(
            user account,
            role? primaryRole,
            warehouse? selectedWarehouse,
            List<AssignedWarehouseView> assignedWarehouses)
        {
            var permissions = primaryRole?.role_permissions
                .Where(rp => rp.permission != null && rp.permission.status == eStatus_Common.Active)
                .Select(rp => rp.permission!.permission_code)
                .Distinct()
                .ToList() ?? new List<string>();

            return new UserProfileView
            {
                id = account.id,
                employee_code = account.employee_code,
                full_name = account.full_name,
                email = account.email,
                phone = account.phone,
                status = (int)account.status,
                status_name = account.status.GetDescription(),
                role_id = primaryRole?.id ?? string.Empty,
                role_code = primaryRole?.role_code ?? string.Empty,
                role_name = primaryRole?.role_name ?? string.Empty,
                permissions = permissions,
                current_warehouse_id = selectedWarehouse?.id ?? string.Empty,
                current_warehouse_code = selectedWarehouse?.warehouse_code ?? string.Empty,
                current_warehouse_name = selectedWarehouse?.warehouse_name ?? string.Empty,
                assigned_warehouses = assignedWarehouses
            };
        }
    }
}
