namespace ewms_api.Route.Contact
{
    public static class SupplierRoute
    {
        public const string INDEX = "api/supplier";

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
