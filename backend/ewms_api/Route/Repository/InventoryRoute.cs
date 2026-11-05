namespace ewms_api.Route.Repository
{
    public static class InventoryRoute
    {
        public const string INDEX = "api/inventory";

        public static class Action
        {
            public const string Search = "search";
            public const string Adjust = "adjust";
            public const string SearchTransactions = "transactions/search";
            public const string DashboardSummary = "dashboard-summary";
        }
    }
}
