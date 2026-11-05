namespace ewms_api.Route.Category
{
    public static class CategoryRoute
    {
        public const string INDEX = "api/category";

        public static class Action
        {
            public const string Search = "search";
            public const string GetById = "{id}";
            public const string Create = "create";
            public const string Update = "update";
            public const string Delete = "delete";
        }
    }
}
