namespace ewms_api.Route.Product
{
    public static class ProductRoute
    {
        public const string INDEX = "api/product";

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
