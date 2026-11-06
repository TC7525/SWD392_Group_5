# Electronic Warehouse Management System (EWMS) - Group 5

Hệ thống Quản lý Kho Hàng Điện Tử — Đồ án môn SWD392 (SE1972-NET).  
Triển khai hoàn chỉnh bản Demo **Luồng Quản lý Đơn mua hàng & Nhập kho (Purchase Order & Receiving Flow - UC09 đến UC14)** theo tài liệu SRS (Report 1) và SDS (Report 2).

---

## 🚀 Hướng Dẫn Nhanh Cho Thành Viên Mới Clone / Pull Project Về

### 1. Yêu cầu môi trường máy tính
- **.NET 8.0 SDK** ([Tải tại đây](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Node.js** bản v18 trở lên & npm ([Tải tại đây](https://nodejs.org/))
- **Microsoft SQL Server** (bản Developer, Standard, Express hoặc LocalDB) & **SSMS** (SQL Server Management Studio)

---

### 2. Thiết lập Cơ sở dữ liệu (Database)

1. Mở công cụ **SQL Server Management Studio (SSMS)** hoặc **Azure Data Studio**, kết nối vào SQL Server trên máy bạn.
2. Mở file:  
   📂 **`database/EWMN_SeedData.sql`**
3. Bấm **Execute** (`F5`) để chạy toàn bộ script.  
   *(Script sẽ tự động tạo CSDL `EWMN`, toàn bộ 20 bảng quan hệ và nạp dữ liệu mẫu gồm 3 vai trò, 3 tài khoản nhân viên, các nhà cung cấp, danh mục, sản phẩm và kho).*
4. **Kiểm tra Connection String (Nếu cần):**  
   Mở file `backend/backend/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=EWMN;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
   - Nếu bạn dùng instance `SQLEXPRESS`, đổi thành: `Server=localhost\\SQLEXPRESS;Database=EWMN;...`
   - Nếu dùng LocalDB, đổi thành: `Server=(localdb)\\MSSQLLocalDB;Database=EWMN;...`

---

### 3. Cài đặt và Khởi chạy Backend (C# ASP.NET Core 8 Web API)

Mở Terminal thứ nhất tại thư mục gốc của project:
```bash
# 1. Di chuyển vào thư mục backend
cd backend/backend

# 2. Khôi phục các thư viện NuGet (chỉ cần chạy lần đầu)
dotnet restore

# 3. Khởi chạy server
dotnet run
```
- Backend sẽ chạy tại: **`http://localhost:5000`**
- Giao diện Swagger API Explorer: **`http://localhost:5000/swagger`**

---

### 4. Cài đặt và Khởi chạy Frontend (React + TypeScript + Vite)

Mở Terminal thứ hai tại thư mục gốc của project:
```bash
# 1. Di chuyển vào thư mục frontend
cd frontend

# 2. Cài đặt các gói npm (BẮT BUỘC khi vừa pull code về vì git không lưu node_modules)
npm install

# 3. Khởi chạy giao diện web
npm run dev
```
- Mở trình duyệt và truy cập: **`http://localhost:5173`**

---

## 👥 Danh Sách Tài Khoản Thử Nghiệm

Mật khẩu chung cho tất cả tài khoản: **`123456`**

| Vai trò | Email đăng nhập | Tên nhân viên | Nhiệm vụ chính trong demo |
|---|---|---|---|
| **Purchase Staff** | `staff_purchase@ewms.com` | Nguyễn Văn Mua | Tạo đơn mua hàng (**UC09**), theo dõi trạng thái đơn (**UC11**) |
| **Warehouse Manager** | `manager_kho@ewms.com` | Trần Quản Lý | Xem xét, duyệt hoặc từ chối đơn mua hàng (**UC10**) |
| **Warehouse Staff** | `staff_kho@ewms.com` | Lê Nhân Viên Kho | Kiểm tra số lượng (**UC12**), xác nhận nhập kho (**UC13**, **UC14**) |

> ⚡ **Mẹo tiện lợi:** Tại trang Đăng nhập và thanh Navigation trên cùng có sẵn thanh **Quick Demo Persona Switcher** với 3 nút `[Mua hàng]`, `[Quản lý]`, `[Thủ kho]`. Bạn chỉ cần click 1 lần là hệ thống tự động đổi token và vai trò mà không cần đăng xuất gõ lại email/mật khẩu!

---

## 📋 Kịch Bản Kiểm Thử Nhanh (Demo Flow)

1. **Bước 1 (Mua hàng - UC09):**  
   Bấm `[Mua hàng]` $\to$ Menu *Đơn mua hàng* $\to$ *Tạo đơn mua hàng* $\to$ Chọn NCC Samsung, thêm 10 iPhone 16 và 5 Galaxy S24 $\to$ Bấm *Gửi đơn mua hàng*.  
   *(Đơn được tạo ở trạng thái `Pending` với mã `PO-yyyyMMdd-0001`)*.
2. **Bước 2 (Duyệt đơn - UC10):**  
   Bấm `[Quản lý]` $\to$ Bấm vào đơn vừa tạo $\to$ Bấm **"Duyệt đơn hàng"** $\to$ Trạng thái chuyển thành **`Approved`**.
3. **Bước 3 (Nhập kho đợt 1 - UC12, UC13, UC14):**  
   Bấm `[Thủ kho]` $\to$ Menu *Nhận hàng* $\to$ Chọn đơn vừa duyệt $\to$ Thử nhập quá số lượng và bấm *Kiểm tra số lượng* để thấy mã lỗi `MSG09`.  
   Nhập thực tế: 6 iPhone, 5 Samsung $\to$ Bấm *Bắt đầu nhận hàng* $\to$ Chọn *Kho Hà Nội* $\to$ Bấm *Xác nhận nhập kho*.  
   *(Đơn chuyển sang `PartiallyReceived` vì iPhone còn thiếu 4 chiếc)*.
4. **Bước 4 (Kiểm tra tồn kho & Nhật ký):**  
   Vào menu *Tồn kho*: Thấy số lượng tồn kho của iPhone tại Kho Hà Nội tăng đúng `+6`, Samsung tăng `+5`. Bảng Nhật ký giao dịch lưu vết chi tiết giao dịch `IN` kèm thời gian thực và tên nhân viên.
5. **Bước 5 (Nhận nốt đợt cuối):**  
   Vào lại *Nhận hàng*, nhập nốt 4 iPhone còn lại $\to$ Xác nhận nhập kho $\to$ Đơn hàng tự động hoàn tất và chuyển sang **`Completed`**.

---

## 🧪 Chạy Kiểm Thử Tự Động (Automated Testing)

- **Backend (113 test cases xUnit):**
  ```powershell
  dotnet test backend.Tests/backend.Tests.csproj
  ```
- **Frontend Build & Linter:**
  ```powershell
  npm --prefix frontend run build
  npm --prefix frontend run lint
  ```
