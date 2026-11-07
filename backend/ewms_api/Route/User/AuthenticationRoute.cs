namespace ewms_api.Route.User
{
    public static class AuthenticationRoute
    {
        public const string INDEX = "api/auth";

        public static class Action
        {
            public const string Login = "login";
            public const string Profile = "profile";
            public const string UpdateProfile = "update-profile";
            public const string ChangePassword = "change-password";
            public const string SwitchWarehouse = "switch-warehouse";
        }
    }
}
