using ewms_common.Extensions;
using ewms_common.Infras.Models;
using ewms_common.Wrappers;
using ewms_infra.Context;
using ewms_infra.Models.AccessLog;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.AccessLog.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ewms_service.Services.AccessLog
{
    public interface IAccessLogService
    {
        Task<string> CreateAsync(AccessLogCreating request);
        Task<ExtendedPageResponse<AccessLogTableView>> SearchAsync(AccessLogTableFilter filter);
    }

    public class AccessLogService : IAccessLogService
    {
        private readonly ewms_context _context;
        private readonly ILogger<AccessLogService> _logger;

        public AccessLogService(ewms_context context, ILogger<AccessLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> CreateAsync(AccessLogCreating request)
        {
            try
            {
                var log = new activity_history
                {
                    user_id = request.user_id,
                    module = request.module,
                    action = request.action,
                    entity_type = request.entity_type,
                    entity_id = request.entity_id,
                    description = request.description,
                    ip_address = request.ip_address
                }.Created(request.user_id ?? "SYSTEM");

                await _context.activity_histories.AddAsync(log);
                await _context.SaveChangesAsync();
                return log.id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghi nhật ký hoạt động: {@Request}", request);
                return string.Empty;
            }
        }

        public async Task<ExtendedPageResponse<AccessLogTableView>> SearchAsync(AccessLogTableFilter filter)
        {
            var logs = await _context.activity_histories
                .AsNoTracking()
                .Include(e => e.user)
                .OrderByDescending(e => e.created_on)
                .Select(e => new AccessLogTableView
                {
                    id = e.id,
                    user_id = e.user_id,
                    employee_code = e.user != null ? e.user.employee_code : null,
                    full_name = e.user != null ? e.user.full_name : "SYSTEM",
                    module = e.module,
                    action = e.action,
                    entity_type = e.entity_type,
                    entity_id = e.entity_id,
                    description = e.description,
                    ip_address = e.ip_address,
                    created_on = e.created_on,
                    created_by = e.created_by,
                    modified_on = e.modified_on,
                    modified_by = e.modified_by
                })
                .ToListAsync();

            var query = logs.AsQueryable().GetQueryable(filter);
            return new ExtendedPageResponse<AccessLogTableView>
            {
                Items = query.Paged(filter.page_index, filter.page_size).ToList(),
                page_index = filter.page_index,
                page_size = filter.page_size,
                filters = filter.filters,
                sort = filter.sort,
                total_items = query.Count()
            };
        }
    }

    public static class AccessLogServiceExtension
    {
        public static IQueryable<AccessLogTableView> GetQueryable(
            this IQueryable<AccessLogTableView> query,
            AccessLogTableFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(e =>
                    (e.full_name != null && e.full_name.ToLower().Contains(kw)) ||
                    (e.employee_code != null && e.employee_code.ToLower().Contains(kw)) ||
                    e.module.ToLower().Contains(kw) ||
                    e.action.ToLower().Contains(kw) ||
                    (e.description != null && e.description.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(filter.user_id))
            {
                query = query.Where(e => e.user_id == filter.user_id);
            }

            if (!string.IsNullOrWhiteSpace(filter.module))
            {
                query = query.Where(e => e.module == filter.module);
            }

            if (!string.IsNullOrWhiteSpace(filter.action))
            {
                query = query.Where(e => e.action == filter.action);
            }

            if (filter.from_date != null)
            {
                query = query.Where(e => e.created_on != null && e.created_on.Value.Date >= filter.from_date.Value.Date);
            }

            if (filter.to_date != null)
            {
                query = query.Where(e => e.created_on != null && e.created_on.Value.Date <= filter.to_date.Value.Date);
            }

            if (filter.filters != null && filter.filters.Any())
            {
                query = filter.filters.Aggregate(query, (current, item) => current.Filter(item));
            }

            if (filter.sort != null)
            {
                query = query.Sort(filter.sort);
            }

            return query;
        }
    }
}
