namespace ewms_common.Wrappers
{
    public class Response<T>
    {
        public Response()
        {
        }

        public Response(T result)
        {
            Result = result;
        }

        public T? Result { get; set; }

        public string[]? Errors { get; set; }

        public string? Message { get; set; }
    }

    public class PagedResponse<T> : Paged
    {
        public PagedResponse()
        {
        }

        public PagedResponse(List<T> items)
        {
            Items = items;
        }

        public List<T>? Items { get; set; }
    }

    public class InitialPagedResponse<T> : PagedResponse<T>
    {
        public bool can_initialize_data { get; set; } = false;
    }

    public class ExtendedPageResponse<T> : PagedResponse<T>
    {
        public object? metadata { get; set; }
    }

    public static class PagedExtensions
    {
        public static IQueryable<T> Paged<T>(this IQueryable<T> query, int index, int size)
        {
            if (index < 1) index = 1;
            return query.Skip((index - 1) * size).Take(size);
        }
    }
}
