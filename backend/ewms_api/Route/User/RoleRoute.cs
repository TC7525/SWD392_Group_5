namespace ewms_api.Route.User
{
    public static class RoleRoute
    {
        public const string INDEX = "api/role";

        public static class Action
        {
            public const string Search = "search";
            public const string GetById = "{id}";
            public const string Permissions = "permissions";
            public const string Create = "create";
            public const string Update = "update";
            public const string Delete = "delete";
        }
    }
}
