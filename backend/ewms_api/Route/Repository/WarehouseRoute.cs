namespace ewms_api.Route.Repository
{
    public static class WarehouseRoute
    {
        public const string INDEX = "api/warehouse";

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
