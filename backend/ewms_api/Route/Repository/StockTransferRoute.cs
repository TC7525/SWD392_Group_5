namespace ewms_api.Route.Repository
{
    public static class StockTransferRoute
    {
        public const string INDEX = "api/stock-transfer";

        public static class Action
        {
            public const string Search = "search";
            public const string GetById = "{id}";
            public const string Create = "create";
            public const string Approve = "approve";
            public const string Reject = "reject";
            public const string Cancel = "cancel";
            public const string Dispatch = "dispatch";
            public const string Receive = "receive";
        }
    }
}
