# HƯỚNG DẪN KHỞI CHẠY VÀ KIỂM THỬ DEMO LUỒNG PURCHASE ORDER (EWMS)

> **Dự án:** Electronic Warehouse Management System (EWMS) - SE1972-NET - Group 5  
> **Phạm vi Demo:** Toàn bộ luồng Quản lý Đơn mua hàng & Nhập kho (Use Cases **UC09 – UC14** theo SRS Report 1 & SDS Report 2).

---

## 1. Môi trường yêu cầu (Prerequisites)

- **Hệ điều hành:** Windows 10/11
- **Database:** Microsoft SQL Server (Instance mặc định `localhost` / `MSSQLSERVER` hoặc `SQLEXPRESS`)
- **Backend:** .NET 8.0 SDK
- **Frontend:** Node.js (v18 trở lên) & npm

---

## 2. Chuẩn bị Cơ sở dữ liệu (Database Setup)

File script SQL hoàn chỉnh gồm cấu trúc bảng (DDL) và dữ liệu mẫu (Seed Data) được lưu tại:  
📁 [`database/EWMN_SeedData.sql`](file:///c:/Users/Admin/Desktop/SWD392_Group_5/database/EWMN_SeedData.sql)

### Cách chạy:
- **Cách 1 (Khuyên dùng - SQL Server Management Studio / Azure Data Studio):**  
  Mở file `database/EWMN_SeedData.sql` trong SSMS và bấm **Execute** (`F5`).
- **Cách 2 (PowerShell):**
  ```powershell
  $conn = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Database=master;Integrated Security=True;"); $conn.Open(); (Get-Content "database/EWMN_SeedData.sql" -Raw) -split "(?im)^\s*GO\s*`$" | ForEach-Object { if($_.Trim()){ $cmd = $conn.CreateCommand(); $cmd.CommandText = $_; $cmd.ExecuteNonQuery() | Out-Null } }; $conn.Close()
  ```
- *Lưu ý:* Khi khởi động Backend, hệ thống cũng tích hợp cơ chế tự động tạo dữ liệu mẫu (`DbInitializer.cs`) nếu các bảng còn trống.

---

## 3. Khởi chạy hệ thống (How to Run)

### 3.1. Khởi chạy Backend (C# ASP.NET Core 8 Web API)
Mở một cửa sổ Terminal (PowerShell):
```powershell
cd c:\Users\Admin\Desktop\SWD392_Group_5\backend\backend
dotnet run
```
- Backend sẽ chạy tại: **`http://localhost:5000`**
- Swagger / OpenAPI: `http://localhost:5000/swagger`

### 3.2. Khởi chạy Frontend (React 19 + TypeScript + Vite)
Mở một cửa sổ Terminal thứ 2:
```powershell
cd c:\Users\Admin\Desktop\SWD392_Group_5\frontend
npm run dev
```
- Mở trình duyệt web truy cập: **`http://localhost:5173`**

---

## 4. Danh sách tài khoản thử nghiệm (Seed Accounts)

Mật khẩu mặc định cho tất cả tài khoản là: **`123456`**

| Vai trò (Role) | Email đăng nhập | Tên nhân viên | Quyền hạn chính trong luồng |
|---|---|---|---|
| **Purchase Staff** | `staff_purchase@ewms.com` | Nguyễn Văn Mua | Tạo đơn mua hàng (UC09), theo dõi trạng thái PO (UC11) |
| **Warehouse Manager** | `manager_kho@ewms.com` | Trần Quản Lý | Duyệt / từ chối đơn mua hàng (UC10), xem toàn bộ PO |
| **Warehouse Staff** | `staff_kho@ewms.com` | Lê Nhân Viên Kho | Kiểm tra số lượng (UC12), nhận hàng và cập nhật tồn kho (UC13, UC14) |

> 💡 **Tính năng tiện ích (Quick Persona Switcher):**  
> Trên thanh **Navbar** ở góc trên cùng bên phải và tại trang **Đăng nhập**, luôn có sẵn 3 nút chuyển đổi nhanh vai trò: **[Mua hàng]**, **[Quản lý]**, **[Thủ kho]**. Bạn chỉ cần click 1 lần là hệ thống tự động đổi token và quyền tương ứng mà không cần gõ lại mật khẩu!

---

## 5. Kịch bản kiểm thử chi tiết từng bước (Step-by-step Walkthrough)

### 🔹 Bước 1: Đăng nhập & Tạo đơn mua hàng (UC09 - Purchase Staff)
1. Đăng nhập với vai trò **Purchase Staff** (hoặc bấm nút *[Mua hàng]* trên Navbar).
2. Chọn menu **Đơn mua hàng** $\to$ Bấm nút **"+ Tạo đơn mua hàng"**.
3. **Kiểm tra Business Rules & Mã lỗi (Edge Cases):**
   - *Test BR-04 (NCC Inactive):* Chọn nhà cung cấp *"Nhà cung cấp Ngừng Hợp Tác Demo"* $\to$ Hệ thống lập tức cảnh báo lỗi **`[MSG15] The selected supplier is inactive and cannot be used.`**
   - *Test BR-03 (Số lượng $\le$ 0):* Nhập số lượng `0` hoặc `-2` $\to$ Hệ thống báo lỗi **`[MSG02] Quantity must be greater than 0.`**
4. **Tạo đơn hợp lệ:**
   - Chọn nhà cung cấp: **Công ty TNHH Điện tử Samsung Việt Nam** (`SUP001`).
   - Ngày giao dự kiến: Chọn ngày trong tương lai (ví dụ sau 7 ngày).
   - Dòng 1: Chọn *iPhone 16 Pro Max 256GB*, số lượng: `10` chiếc (Đơn giá 34,990,000 đ $\to$ Thành tiền 349,900,000 đ).
   - Bấm *"+ Thêm sản phẩm"*: Chọn *Samsung Galaxy S24 Ultra*, số lượng: `5` chiếc (Đơn giá 29,990,000 đ $\to$ Thành tiền 149,950,000 đ).
   - Xem tổng tiền tự động nhảy: **499,850,000 đ**.
   - Bấm **"Gửi đơn mua hàng"** $\to$ Hệ thống báo thành công **`[MSG10] Purchase order created successfully.`**
   - Đơn hàng mới sinh ra với mã tự động dạng `PO-yyyyMMdd-0001` và trạng thái **`Pending`** (Chờ duyệt).

---

### 🔹 Bước 2: Duyệt đơn mua hàng (UC10 - Warehouse Manager)
1. Chuyển sang vai trò **Warehouse Manager** (bấm nút *[Quản lý]* trên Navbar).
2. Vào menu **Đơn mua hàng**, bấm vào mã đơn vừa tạo ở trạng thái `Pending`.
3. Màn hình chi tiết hiển thị đầy đủ thông tin: Người tạo, Nhà cung cấp, Bảng sản phẩm chi tiết (SL đặt, SL đã nhận = 0, SL còn lại).
4. Phía trên có thanh công cụ phê duyệt:
   - **Test Từ chối (Reject):** Bấm *Từ chối* $\to$ Hộp thoại bật lên yêu cầu nhập lý do từ chối (nếu để trống sẽ báo `MSG01`).
   - **Thực hiện Duyệt (Approve):** Bấm **"Duyệt đơn hàng"** $\to$ Xác nhận $\to$ Hệ thống cập nhật trạng thái đơn sang **`Approved`** kèm thông báo **`[MSG11] Purchase order approved successfully.`**

---

### 🔹 Bước 3: Kiểm tra hàng & Nhận hàng đợt 1 (UC12, UC13, UC14 - Warehouse Staff)
1. Chuyển sang vai trò **Warehouse Staff** (bấm nút *[Thủ kho]* trên Navbar).
2. Vào menu **Nhận hàng** $\to$ Bạn sẽ thấy đơn hàng vừa được duyệt hiển thị trong danh sách chờ nhận hàng.
3. Bấm nút **"Kiểm & Nhận hàng"** để vào chi tiết đơn nhận:
   - Chọn kho nhận: **Kho Tổng Hà Nội** (`WH-HN-01`).
   - Ghi chú nhận hàng: Gõ `"Nhận đợt 1 - Hàng mới 100% nguyên đai kiện"`.
4. **Kiểm tra nghiệp vụ kiểm hàng (UC12 - Check Quantity):**
   - Cột *"SL Nhận đợt này"*: Thử gõ iPhone = `15` (vượt quá 10 chiếc còn lại).
   - Bấm **"Kiểm tra số lượng"** $\to$ Dòng sản phẩm bị bôi đỏ và cảnh báo: **`[MSG09] Received quantity cannot exceed the remaining ordered quantity.`**
5. **Bắt đầu quá trình nhận hàng (UC13 - Start Receiving):**
   - Chỉnh lại số lượng nhận thực tế: iPhone = `6` chiếc (còn lại 4), Samsung = `5` chiếc (nhận đủ 5).
   - Bấm **"Kiểm tra số lượng"** $\to$ Hệ thống hiển thị tích xanh hợp lệ.
   - Bấm **"Bắt đầu nhận hàng"** $\to$ Trạng thái chuyển sang **`Processing`** (Đang kiểm hàng).
6. **Xác nhận nhập kho nguyên tử (UC13 + UC14 - Confirm Receive):**
   - Bấm **"Xác nhận nhập kho"** $\to$ Popup xác nhận xuất hiện tóm tắt các sản phẩm và kho nhận.
   - Bấm **"Xác nhận"** $\to$ Hệ thống báo thành công **`[MSG12] Products received and inventory updated successfully.`**
   - Trạng thái đơn tự động chuyển thành **`PartiallyReceived`** (Đã nhận một phần) vì dòng iPhone mới nhận 6/10 chiếc!

---

### 🔹 Bước 4: Kiểm chứng cập nhật Tồn kho & Nhật ký giao dịch (Traceability Audit)
1. Click menu **Tồn kho** (`/inventory`):
2. **Tại bảng "Tồn kho theo từng kho":**
   - Lọc theo kho **Kho Tổng Hà Nội**:
   - iPhone 16 Pro Max: Tồn kho tăng từ `0` $\to$ **`6`** cái.
   - Samsung Galaxy S24 Ultra: Tồn kho tăng từ `0` $\to$ **`5`** cái.
3. **Tại bảng "Nhật ký giao dịch nhập kho":**
   - Xuất hiện 2 dòng giao dịch mới nhất với loại `IN`:
   - Ghi nhận đầy đủ: Tên sản phẩm, Kho nhận, Số lượng (`+6`, `+5`), Nhân viên thực hiện (*Lê Nhân Viên Kho*), Mã PO tham chiếu và Ghi chú.

---

### 🔹 Bước 5: Nhận nốt phần hàng còn lại & Hoàn thành đơn hàng
1. Quay lại trang **Nhận hàng** $\to$ Đơn hàng vẫn hiển thị ở trạng thái `PartiallyReceived`.
2. Bấm vào nhận tiếp:
   - Dòng iPhone: SL Đã nhận: `6`, SL Còn lại: `4`.
   - Dòng Samsung: SL Đã nhận: `5`, SL Còn lại: `0` (không thể nhận thêm).
3. Nhập số lượng iPhone nhận đợt này: `4` chiếc.
4. Bấm **"Bắt đầu nhận hàng"** $\to$ Bấm **"Xác nhận nhập kho"**.
5. **Kết quả:** Toàn bộ sản phẩm trong đơn đã được giao đủ $\to$ Trạng thái PO tự động chuyển thành **`Completed`** (Hoàn thành)!
6. Đơn hàng chuyển sang `Completed` sẽ biến mất khỏi danh sách chờ nhận hàng (tuân thủ **BR-08**: Đơn đã hoàn tất không thể nhận thêm).

---

## 6. Chạy kiểm thử tự động (Automated Testing)

Trong thư mục gốc dự án, bạn có thể chạy bộ kiểm thử tự động bất cứ lúc nào:

1. **Chạy 113 Unit Tests cho Backend:**
   ```powershell
   dotnet test backend.Tests/backend.Tests.csproj
   ```
   *Kết quả mong đợi:* `Passed! - Failed: 0, Passed: 113, Skipped: 0`

2. **Kiểm tra biên dịch & Linting của Frontend:**
   ```powershell
   npm --prefix frontend run build
   npm --prefix frontend run lint
   ```
   *Kết quả mong đợi:* `0 errors, 0 warnings`
