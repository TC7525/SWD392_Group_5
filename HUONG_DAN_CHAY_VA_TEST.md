# HƯỚNG DẪN KHỞI CHẠY VÀ KIỂM THỬ DEMO LUỒNG PURCHASE ORDER (EWMS)
> **Dành cho thành viên nhóm hoặc giảng viên vừa Clone / Pull project từ Git về máy.**

---

## 1. Yêu cầu môi trường cài đặt sẵn trên máy
Trước khi khởi chạy, máy tính của bạn cần có:
1. **.NET 8.0 SDK** (Kiểm tra bằng lệnh: `dotnet --version` -> hiển thị 8.0.x hoặc 10.x).
2. **Node.js** (Khuyên dùng v18 đến v22, kèm npm. Kiểm tra bằng lệnh: `node -v`).
3. **Microsoft SQL Server** (Bản Developer, Standard, Express hoặc LocalDB) kèm phần mềm quản lý như **SSMS** (SQL Server Management Studio) hoặc Azure Data Studio.

---

## 2. Bước 1: Khởi tạo Cơ sở dữ liệu (Database Setup)

File script SQL hoàn chỉnh gồm cấu trúc bảng (DDL) và dữ liệu mẫu (Seed Data) được lưu tại:  
📁 [`database/EWMN_SeedData.sql`](file:///c:/Users/Admin/Desktop/SWD392_Group_5/database/EWMN_SeedData.sql)

### Cách chạy:
- **Cách 1 (Đơn giản nhất qua SSMS):**  
  1. Mở SQL Server Management Studio (SSMS) và kết nối vào SQL Server trên máy bạn.
  2. Mở file `database/EWMN_SeedData.sql`.
  3. Bấm **Execute** (`F5`) để chạy toàn bộ script.
- **Cách 2 (Chạy nhanh bằng PowerShell):**
  ```powershell
  $conn = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Database=master;Integrated Security=True;"); $conn.Open(); (Get-Content "database/EWMN_SeedData.sql" -Encoding UTF8 -Raw) -split "(?im)^\s*GO\s*`$" | ForEach-Object { if($_.Trim()){ $cmd = $conn.CreateCommand(); $cmd.CommandText = $_; $cmd.ExecuteNonQuery() | Out-Null } }; $conn.Close()
  ```

### Lưu ý quan trọng về Chuỗi kết nối (Connection String):
Mở file `backend/backend/appsettings.json` và kiểm tra cấu hình:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=EWMN;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
- Nếu SQL Server của bạn có tên instance là `SQLEXPRESS` (thường gặp khi cài bản Express), bạn sửa lại `Server=localhost\\SQLEXPRESS` hoặc `Server=.\\SQLEXPRESS`.
- Nếu dùng LocalDB, đổi thành: `Server=(localdb)\\MSSQLLocalDB;Database=EWMN;Trusted_Connection=True;`.

---

## 3. Bước 2: Khởi chạy Backend (C# ASP.NET Core 8 Web API)

Mở một cửa sổ Terminal (PowerShell hoặc CMD):
```powershell
# Di chuyển vào thư mục dự án backend
cd c:\Users\Admin\Desktop\SWD392_Group_5\backend\backend

# Khôi phục các thư viện NuGet (Bắt buộc chạy lần đầu)
dotnet restore

# Khởi chạy server
dotnet run
```
- Server Backend sẽ lắng nghe tại: **`http://localhost:5000`**
- Trang tài liệu API (Swagger UI): `http://localhost:5000/swagger`

---

## 4. Bước 3: Cài đặt thư viện & Khởi chạy Frontend (React + TypeScript + Vite)

Mở một cửa sổ Terminal thứ 2:
```powershell
# Di chuyển vào thư mục frontend
cd c:\Users\Admin\Desktop\SWD392_Group_5\frontend

# Cài đặt các gói thư viện (BẮT BUỘC vì git không lưu thư mục node_modules)
npm install

# Khởi chạy giao diện ở chế độ Development
npm run dev
```
- Sau khi chạy xong, Vite sẽ cung cấp đường link: **`http://localhost:5173`**
- Bạn mở trình duyệt (Chrome/Edge/Brave) và truy cập vào link trên.

---

## 5. Danh sách tài khoản thử nghiệm (Seed Accounts)

Mật khẩu mặc định cho tất cả tài khoản là: **`123456`**

| Vai trò (Role) | Email đăng nhập | Tên nhân viên | Quyền hạn chính trong luồng |
|---|---|---|---|
| **Purchase Staff** | `staff_purchase@ewms.com` | Nguyễn Văn Mua | Tạo đơn mua hàng (UC09), theo dõi trạng thái PO (UC11) |
| **Warehouse Manager** | `manager_kho@ewms.com` | Trần Quản Lý | Duyệt / từ chối đơn mua hàng (UC10), xem toàn bộ PO |
| **Warehouse Staff** | `staff_kho@ewms.com` | Lê Nhân Viên Kho | Kiểm tra số lượng (UC12), nhận hàng và cập nhật tồn kho (UC13, UC14) |

> ⚡ **Tính năng tiện ích (Quick Persona Switcher):**  
> Trên thanh **Navbar** ở góc trên cùng bên phải và tại trang **Đăng nhập**, luôn có sẵn 3 nút chuyển đổi nhanh vai trò: **[Mua hàng]**, **[Quản lý]**, **[Thủ kho]**. Bạn chỉ cần click 1 lần là hệ thống tự động đăng nhập và đổi quyền tương ứng mà không cần gõ lại mật khẩu!

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

