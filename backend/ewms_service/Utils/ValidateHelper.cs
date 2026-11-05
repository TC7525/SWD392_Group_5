using ewms_common.Authentications;
using ewms_common.Exceptions;
using ewms_infra.Context;
using ewms_infra.Enums;
using ewms_service.Consts;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.Utils
{
    public static class ValidateHelper
    {
        public static async Task EnsureWarehouseAccessAsync(
            ewms_context context,
            UserHeader currentUser,
            string warehouseId)
        {
            var wh = await context.warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.id == warehouseId)
                ?? throw new NotFoundException(ErrorMessage.WarehouseNotFound);

            if (wh.status != eStatus_Warehouse.Active)
            {
                throw new BadException(ErrorMessage.WarehouseInactive);
            }

            if (currentUser.role == SystemRoles.Administrator || currentUser.system_role == SystemRoles.Administrator)
            {
                return;
            }

            var isAssigned = await context.user_warehouses
                .AsNoTracking()
                .AnyAsync(x => x.user_id == currentUser.user_id && x.warehouse_id == warehouseId);

            if (!isAssigned)
            {
                throw new ForbiddenException(ErrorMessage.WarehouseNotAssigned);
            }
        }
    }
}
