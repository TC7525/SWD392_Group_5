using ewms_common.Infras.Enums;
using ewms_common.Wrappers;
using System.Linq.Dynamic.Core;

namespace ewms_common.Extensions
{
    public static class FilterTableExtension
    {
        public static IQueryable<T> GetQueryableBase<T>(this IQueryable<T> query, string? propertyName, Type? propertyType, string? filterValue, eType_FilterTableColumn filterType)
        {
            filterValue = filterValue ?? string.Empty;
            switch (filterType)
            {
                case eType_FilterTableColumn.NotNull:
                    query = query.Where($"{propertyName} != null");
                    break;

                case eType_FilterTableColumn.Null:
                    query = query.Where($"{propertyName} == null");
                    break;

                case eType_FilterTableColumn.Contain:
                    if (propertyType == typeof(string))
                    {
                        query = query.Where($"{propertyName} != null && {propertyName}.ToLower().Contains(@0)", filterValue);
                    }
                    break;

                case eType_FilterTableColumn.NotContain:
                    if (propertyType == typeof(string))
                    {
                        query = query.Where($"{propertyName} == null || !{propertyName}.ToLower().Contains(@0)", filterValue);
                    }
                    break;

                case eType_FilterTableColumn.Equal:
                    if (propertyType == typeof(string))
                    {
                        query = query.Where($"{propertyName} != null && {propertyName}.Trim().ToLower() == @0", filterValue);
                    }
                    else if (propertyType == typeof(int?) || propertyType == typeof(bool) || propertyType == typeof(int) || propertyType == typeof(bool?) || propertyType == typeof(double) || propertyType == typeof(double?) || propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                    {
                        query = query.Where($"{propertyName} == @0", filterValue);
                    }
                    break;

                case eType_FilterTableColumn.NotEqual:
                    if (propertyType == typeof(string))
                    {
                        query = query.Where($"{propertyName} == null || {propertyName}.Trim().ToLower() != @0", filterValue);
                    }
                    else if (propertyType == typeof(int?) || propertyType == typeof(bool) || propertyType == typeof(int) || propertyType == typeof(bool?) || propertyType == typeof(double) || propertyType == typeof(double?) || propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                    {
                        query = query.Where($"{propertyName} != @0", filterValue);
                    }
                    break;

                case eType_FilterTableColumn.StartWith:
                    if (propertyType == typeof(string))
                    {
                        query = query.Where($"{propertyName} != null && {propertyName}.Trim().ToLower().StartsWith(@0)", filterValue);
                    }
                    break;

                case eType_FilterTableColumn.EndWith:
                    if (propertyType == typeof(string))
                    {
                        query = query.Where($"{propertyName} != null && {propertyName}.Trim().ToLower().EndsWith(@0)", filterValue);
                    }
                    break;

                case eType_FilterTableColumn.LessThan:
                case eType_FilterTableColumn.LessThanOrEqual:
                case eType_FilterTableColumn.MoreThan:
                case eType_FilterTableColumn.MoreThanOrEqual:
                case eType_FilterTableColumn.DateEqual:
                case eType_FilterTableColumn.DateNotEqual:
                    if ((propertyType == typeof(DateTime) || propertyType == typeof(DateTime?)) && DateTime.TryParse(filterValue, out var dateValue))
                    {
                        string dateQuery = propertyType == typeof(DateTime) ? $"{propertyName}.Date" : $"{propertyName}.Value.Date";

                        switch (filterType)
                        {
                            case eType_FilterTableColumn.LessThan:
                                query = query.Where($"{propertyName} != null && {dateQuery} < @0", dateValue.Date).ToList().AsQueryable();
                                break;
                            case eType_FilterTableColumn.LessThanOrEqual:
                                query = query.Where($"{propertyName} != null && {dateQuery} <= @0", dateValue.Date).ToList().AsQueryable();
                                break;
                            case eType_FilterTableColumn.MoreThan:
                                query = query.Where($"{propertyName} != null && {dateQuery} > @0", dateValue.Date).ToList().AsQueryable();
                                break;
                            case eType_FilterTableColumn.MoreThanOrEqual:
                                query = query.Where($"{propertyName} != null && {dateQuery} >= @0", dateValue.Date).ToList().AsQueryable();
                                break;
                            case eType_FilterTableColumn.DateEqual:
                                query = query.Where($"{propertyName} != null && {dateQuery} == @0", dateValue.Date).ToList().AsQueryable();
                                break;
                            case eType_FilterTableColumn.DateNotEqual:
                                query = query.Where($"{propertyName} != null && {dateQuery} != @0", dateValue.Date);
                                break;
                        }
                    }
                    break;

                case eType_FilterTableColumn.NumberLessThan:
                case eType_FilterTableColumn.NumberLessThanOrEqual:
                case eType_FilterTableColumn.NumberMoreThan:
                case eType_FilterTableColumn.NumberMoreThanOrEqual:
                    if ((propertyType == typeof(int) || propertyType == typeof(int?) || propertyType == typeof(double) || propertyType == typeof(double?) || propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                        && double.TryParse(filterValue, out var numberValue))
                    {
                        switch (filterType)
                        {
                            case eType_FilterTableColumn.NumberLessThan:
                                query = query.Where($"{propertyName} != null && {propertyName} < @0", numberValue);
                                break;
                            case eType_FilterTableColumn.NumberLessThanOrEqual:
                                query = query.Where($"{propertyName} != null && {propertyName} <= @0", numberValue);
                                break;
                            case eType_FilterTableColumn.NumberMoreThan:
                                query = query.Where($"{propertyName} != null && {propertyName} > @0", numberValue);
                                break;
                            case eType_FilterTableColumn.NumberMoreThanOrEqual:
                                query = query.Where($"{propertyName} != null && {propertyName} >= @0", numberValue);
                                break;
                        }
                    }
                    break;
            }
            return query;
        }

        public static IQueryable<T> ApplyFilterBase<T>(this IQueryable<T> query, Paging searching) where T : class
        {
            var validProperties = typeof(T).GetProperties().Select(p => p.Name).ToHashSet();
            foreach (var filter in searching.filters)
            {
                if (filter != null && validProperties.Contains(filter.name))
                {
                    var propertyName = filter.name;
                    var filterValue = filter.value?.Trim().ToLower();
                    var propertyType = typeof(T).GetProperty(propertyName)?.PropertyType;
                    if (propertyType != null && propertyType.IsEnum)
                    {
                        propertyType = typeof(int);
                    }

                    query = query.GetQueryableBase(propertyName, propertyType, filterValue, filter.type);
                }
            }
            if (searching.sort != null && validProperties.Contains(searching.sort.name))
            {
                var propertyName = searching.sort.name;
                var propertyType = typeof(T).GetProperty(propertyName)?.PropertyType;

                if (propertyType != null)
                {
                    if (isList(propertyType))
                    {
                        var propertyAccess = $"{propertyName}.Count";

                        switch (searching.sort.type)
                        {
                            case eType_SortTableColumn.OrderBy:
                                query = query.OrderBy(propertyAccess);
                                break;
                            case eType_SortTableColumn.OrderDescendingBy:
                                query = query.OrderBy($"{propertyAccess} descending");
                                break;
                        }
                    }
                    else
                    {
                        switch (searching.sort.type)
                        {
                            case eType_SortTableColumn.OrderBy:
                                query = query.OrderBy($"{propertyName}");
                                break;
                            case eType_SortTableColumn.OrderDescendingBy:
                                query = query.OrderBy($"{propertyName} descending");
                                break;
                        }
                    }
                }
            }

            return query;
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> query, FilterTable? filter) where T : class
        {
            if (filter == null || string.IsNullOrWhiteSpace(filter.name))
                return query;

            var validProperties = typeof(T).GetProperties().Select(p => p.Name).ToHashSet();
            if (!validProperties.Contains(filter.name))
                return query;

            var propertyName = filter.name;
            var filterValue = filter.value?.Trim().ToLower();
            var propertyType = typeof(T).GetProperty(propertyName)?.PropertyType;
            if (propertyType != null && propertyType.IsEnum)
            {
                propertyType = typeof(int);
            }

            return query.GetQueryableBase(propertyName, propertyType, filterValue, filter.type);
        }

        public static IQueryable<T> Sort<T>(this IQueryable<T> query, SortTable? sort) where T : class
        {
            if (sort == null || string.IsNullOrWhiteSpace(sort.name))
                return query;

            var validProperties = typeof(T).GetProperties().Select(p => p.Name).ToHashSet();
            if (!validProperties.Contains(sort.name))
                return query;

            var propertyName = sort.name;
            var propertyType = typeof(T).GetProperty(propertyName)?.PropertyType;
            if (propertyType != null)
            {
                if (isList(propertyType))
                {
                    var propertyAccess = $"{propertyName}.Count";
                    return sort.type == eType_SortTableColumn.OrderDescendingBy
                        ? query.OrderBy($"{propertyAccess} descending")
                        : query.OrderBy(propertyAccess);
                }

                return sort.type == eType_SortTableColumn.OrderDescendingBy
                    ? query.OrderBy($"{propertyName} descending")
                    : query.OrderBy($"{propertyName}");
            }

            return query;
        }

        public static bool isList(Type? type) => type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }
}
