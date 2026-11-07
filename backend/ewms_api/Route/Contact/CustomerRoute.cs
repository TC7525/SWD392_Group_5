namespace ewms_api.Route.Contact
{
    public static class CustomerRoute
    {
        public const string INDEX = "api/customer";

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
