namespace ewms_api.Route.User
{
    public static class AccountRoute
    {
        public const string INDEX = "api/account";

        public static class Action
        {
            public const string Search = "search";
            public const string GetById = "{id}";
            public const string Create = "create";
            public const string Update = "update";
            public const string UpdateStatus = "update-status";
            public const string ResetPassword = "reset-password";
            public const string Delete = "delete";
        }
    }
}
