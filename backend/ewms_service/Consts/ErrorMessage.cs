namespace ewms_service.Consts
{
    public static class ErrorMessage
    {
        #region General & System (SRS Section 5.3)
        public const string MSG01_RequiredField = "{0} không được để trống.";
        public const string MSG02_InvalidCredentials = "Email/Mã nhân viên hoặc mật khẩu không chính xác.";
        public const string MSG03_AccountInactiveOrLocked = "Tài khoản của bạn đã bị khóa hoặc ngừng hoạt động. Vui lòng liên hệ quản trị viên.";
        public const string MSG04_DuplicateCodeOrEmail = "{0} đã tồn tại trong hệ thống.";
        public const string MSG05_InsufficientStock = "Số lượng tồn kho của sản phẩm '{0}' tại kho không đủ (Tồn hiện tại: {1}, Yêu cầu: {2}).";
        public const string MSG06_ExceedRemainingQuantity = "Số lượng nhập của sản phẩm '{0}' vượt quá số lượng còn lại cần nhập (Còn lại: {1}, Nhập: {2}).";
        public const string MSG07_UnauthorizedAction = "Bạn không có quyền thực hiện thao tác này.";
        public const string MSG08_InvalidStatusTransition = "Không thể thực hiện thao tác khi phiếu đang ở trạng thái '{0}'.";
        public const string MSG09_OperationSuccess = "Thao tác thành công.";
        public const string MSG10_SystemError = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.";
        #endregion

        #region Account & Role
        public const string UserNotFound = "Không tìm thấy người dùng.";
        public const string RoleNotFound = "Không tìm thấy vai trò.";
        public const string PermissionNotFound = "Không tìm thấy quyền hạn.";
        public const string InvalidOldPassword = "Mật khẩu hiện tại không chính xác.";
        public const string PasswordConfirmationMismatch = "Mật khẩu xác nhận không khớp.";
        #endregion

        #region Master Data (Warehouse, Category, Product, Supplier, Customer)
        public const string WarehouseNotFound = "Không tìm thấy kho hàng.";
        public const string WarehouseInactive = "Kho hàng đang ngừng hoạt động.";
        public const string WarehouseNotAssigned = "Bạn chưa được phân công quản lý/làm việc tại kho hàng này.";
        public const string CategoryNotFound = "Không tìm thấy danh mục sản phẩm.";
        public const string CategoryHasProducts = "Không thể xóa danh mục đang chứa sản phẩm.";
        public const string ProductNotFound = "Không tìm thấy sản phẩm.";
        public const string SupplierNotFound = "Không tìm thấy nhà cung cấp.";
        public const string CustomerNotFound = "Không tìm thấy khách hàng.";
        #endregion

        #region Purchase Order, Outbound Order, Stock Transfer
        public const string PurchaseOrderNotFound = "Không tìm thấy đơn mua hàng.";
        public const string OutboundOrderNotFound = "Không tìm thấy đơn xuất kho.";
        public const string StockTransferNotFound = "Không tìm thấy phiếu chuyển kho.";
        public const string SameSourceAndDestinationWarehouse = "Kho xuất và kho nhận không được trùng nhau.";
        public const string OrderItemsEmpty = "Danh sách sản phẩm trong phiếu không được để trống.";
        public const string RejectionReasonRequired = "Vui lòng nhập lý do từ chối.";
        #endregion
    }
}
