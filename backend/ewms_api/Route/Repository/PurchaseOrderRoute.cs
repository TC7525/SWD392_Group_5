namespace ewms_api.Route.Repository
{
    public static class PurchaseOrderRoute
    {
        public const string INDEX = "api/purchase-order";

        public static class Action
        {
            public const string Search = "search";
            public const string GetById = "{id}";
            public const string Create = "create";
            public const string Approve = "approve";
            public const string Reject = "reject";
            public const string Cancel = "cancel";
            public const string ReceiveGoods = "receive-goods";
        }
    }
}
