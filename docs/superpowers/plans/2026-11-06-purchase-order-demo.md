# Purchase Order Demo Flow Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a functional, testable end-to-end demo for the Purchase Order & Receiving flow (UC09 – UC14) strictly conforming to SRS and SDS specifications: Purchase Staff creates PO, Warehouse Manager approves/rejects, Warehouse Staff validates quantity and confirms receipt, and the System atomically updates inventory and records inventory transactions.

**Architecture:** 
- **Backend:** ASP.NET Core 8 Web API with 3-tier layering (`Controllers` → `Services` → `Repositories` → `Models / DbContext`) backed by Microsoft SQL Server (`MSSQLSERVER` local instance) using EF Core Code-First with migrations and automated seed data.
- **Frontend:** React 19 + TypeScript + Vite SPA consuming REST API via Axios, with role-based routing and a quick-switch test panel for the 3 demo personas.
- **Enforcement:** State machine transitions, atomic receiving transactions with rollback on failure, and standardized application message codes (`MSG01`–`MSG17`).

**Tech Stack:** C# 12, .NET 8.0, ASP.NET Core Web API, Entity Framework Core 8, Microsoft SQL Server, xUnit, Moq, React 19, TypeScript 5.8, Vite 8, React Router v7, Axios.

## Global Constraints
- Target Framework: `net8.0` for Backend, React 19 + Vite for Frontend.
- Standardized API Response Envelope: `{ code: string, message: string, data?: object }`.
- Status transitions strictly follow SDS 3.17.2:
  `Pending` → `Approved` | `Rejected`
  `Approved` | `PartiallyReceived` → `Processing`
  `Processing` → `PartiallyReceived` | `Completed`
  `Processing` → `Approved` | `PartiallyReceived` (Cancel)
- All database operations in Receiving (updating PO items, updating stock in `inventories`, writing `inventory_transactions`) must be wrapped in an explicit `IDbContextTransaction`. Any failure rolls back and returns `MSG17`.
- Message codes `MSG01`, `MSG02`, `MSG04`, `MSG05`, `MSG09`, `MSG10`, `MSG11`, `MSG12`, `MSG15`, `MSG16`, `MSG17`, `MSG-EMP-01`, `MSG-EMP-03` must be used accurately as defined in SRS 5.3.

---

## File Structure Overview

### Backend (`backend/backend/`)
- `backend.csproj`: Add JWT Bearer & Authentication packages.
- `appsettings.json`: Connection string to local SQL Server `Server=localhost;Database=EWMN;Trusted_Connection=True;TrustServerCertificate=True;`.
- `Common/`:
  - `ApiResponse.cs`: Standard response wrapper.
  - `AppException.cs`: Domain exceptions carrying Message Code and HTTP Status.
  - `GlobalExceptionFilter.cs`: Catches domain exceptions and formats JSON envelope.
- `Enums/`:
  - `POStatus.cs`, `TransactionType.cs`, `SupplierStatus.cs`.
- `Models/Entities/`:
  - `User.cs`, `Role.cs`, `UserRole.cs`, `Supplier.cs`, `Category.cs`, `Product.cs`, `Warehouse.cs`, `Inventory.cs`, `PurchaseOrder.cs`, `PurchaseOrderItem.cs`, `InventoryTransaction.cs`.
- `Models/DTOs/`:
  - `AuthDtos.cs`, `PurchaseOrderDtos.cs`, `ReceivingDtos.cs`, `InventoryDtos.cs`.
- `Models/AppDbContext.cs`: EF Core mappings and relationships.
- `Repositories/`:
  - Interfaces & implementations for `PurchaseOrder`, `Supplier`, `Product`, `Inventory`, `InventoryTransaction`, `User`.
- `Services/`:
  - `AuthService.cs`: Login validation & JWT generation.
  - `POStatusService.cs`: Enforces state transitions.
  - `PurchaseOrderService.cs`: UC09, UC10, UC11 implementation.
  - `ReceivingService.cs`: UC12, UC13, UC14 implementation.
  - `InventoryService.cs`: Stock retrieval and receiving update.
- `Controllers/`:
  - `AuthController.cs`, `PurchaseOrderController.cs`, `ReceivingController.cs`, `InventoryController.cs`.
- `Data/DbInitializer.cs`: Automatically seeds roles, users, suppliers, categories, products, warehouses on startup.

### Test Project (`backend.Tests/`)
- `backend.Tests.csproj`: xUnit, `Microsoft.EntityFrameworkCore.InMemory`.
- `Services/POStatusServiceTests.cs`: Unit tests for allowed/forbidden status transitions.
- `Services/PurchaseOrderServiceTests.cs`: Validation rules tests (`MSG01`, `MSG02`, `MSG05`, `MSG15`, `MSG10`).
- `Services/ReceivingServiceTests.cs`: Receiving quantity checks (`MSG09`), atomic update, rollback (`MSG17`), completion logic.

### Frontend (`frontend/src/`)
- `package.json`: Add `react-router-dom` and `axios`.
- `types/models.ts`: TypeScript interfaces for DTOs and POStatus enum.
- `api/client.ts`: Axios instance with JWT interceptor and standard error extraction.
- `api/authApi.ts`, `api/poApi.ts`, `api/receivingApi.ts`, `api/inventoryApi.ts`.
- `context/AuthContext.tsx`: Authentication state, login/logout, quick-switch user bar.
- `components/Navbar.tsx`, `components/StatusBadge.tsx`, `components/Modal.tsx`.
- `pages/LoginPage.tsx`: Login view with quick account buttons.
- `pages/PurchaseOrderListPage.tsx`: PO table with status filter.
- `pages/CreatePurchaseOrderPage.tsx`: Form with active supplier select, dynamic items, auto-calc totals.
- `pages/PurchaseOrderDetailPage.tsx`: Detailed PO view + Manager Approve/Reject actions.
- `pages/ReceivingListPage.tsx`: Orders ready to receive.
- `pages/ReceivingDetailPage.tsx`: Quantity check, start receiving, confirm into warehouse.
- `pages/InventoryListPage.tsx`: Inventory stock list & inbound transaction log.
- `App.tsx`: Router configuration.

---

## Tasks

### Task 1: Backend Packages, Configuration & Common Infrastructure
**Files:**
- Modify: `backend/backend/backend.csproj`
- Modify: `backend/backend/appsettings.json`
- Create: `backend/backend/Common/ApiResponse.cs`
- Create: `backend/backend/Common/AppException.cs`
- Create: `backend/backend/Common/GlobalExceptionFilter.cs`
- Create: `backend/backend/Enums/POStatus.cs`
- Create: `backend/backend/Enums/TransactionType.cs`
- Create: `backend/backend/Enums/SupplierStatus.cs`

- [ ] **Step 1: Update `backend.csproj` with required NuGet packages**
Add `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.13), `System.IdentityModel.Tokens.Jwt` (8.5.0), and CORS support.
- [ ] **Step 2: Update `appsettings.json`**
Add SQL Server connection string for `EWMN` and JWT settings (`SecretKey`, `Issuer`, `Audience`).
- [ ] **Step 3: Create Enums (`POStatus`, `TransactionType`, `SupplierStatus`)**
- [ ] **Step 4: Create standard `ApiResponse<T>`, `AppException`, and `GlobalExceptionFilter`**
- [ ] **Step 5: Verify build**
Run: `dotnet build backend/backend/backend.csproj`
Expected: Build succeeded with 0 errors.
- [ ] **Step 6: Commit Task 1**
`git add backend/backend/`
`git commit -m "feat(backend): configure packages, connection string, enums and exception filter"`

---

### Task 2: Domain Entities, DbContext & Data Seeder
**Files:**
- Create: `backend/backend/Models/Entities/User.cs`
- Create: `backend/backend/Models/Entities/Role.cs`
- Create: `backend/backend/Models/Entities/UserRole.cs`
- Create: `backend/backend/Models/Entities/Supplier.cs`
- Create: `backend/backend/Models/Entities/Category.cs`
- Create: `backend/backend/Models/Entities/Product.cs`
- Create: `backend/backend/Models/Entities/Warehouse.cs`
- Create: `backend/backend/Models/Entities/Inventory.cs`
- Create: `backend/backend/Models/Entities/PurchaseOrder.cs`
- Create: `backend/backend/Models/Entities/PurchaseOrderItem.cs`
- Create: `backend/backend/Models/Entities/InventoryTransaction.cs`
- Create: `backend/backend/Models/AppDbContext.cs`
- Create: `backend/backend/Data/DbInitializer.cs`

- [ ] **Step 1: Create Entity classes mapping to database schema**
Ensure table and column names map correctly (`purchase_orders`, `purchase_order_items`, `inventories`, etc.).
- [ ] **Step 2: Create `AppDbContext.cs` with Fluent API relationships**
Configure primary keys, auto-increment, unique indexes (`purchase_order_code`, `(purchase_order_id, product_id)`, `(product_id, warehouse_id)`), decimal precisions (`18,2`).
- [ ] **Step 3: Create `DbInitializer.cs`**
Seed:
- 3 Roles: `Purchase Staff`, `Warehouse Manager`, `Warehouse Staff`.
- 3 Users with SHA256 hashed password `123456`: `staff_purchase@ewms.com`, `manager_kho@ewms.com`, `staff_kho@ewms.com`.
- 3 Suppliers: `SUP001` (Active), `SUP002` (Active), `SUP003` (Inactive).
- 2 Warehouses: `WH-HN-01` (Kho Hà Nội), `WH-HCM-01` (Kho TP.HCM).
- 1 Category: `Điện tử & Di động`.
- 4 Products: `PRD-IP16`, `PRD-S24`, `PRD-MBA`, `PRD-SNY`.
- Initial inventories (0 or sample baseline).
- [ ] **Step 4: Verify DbContext compiles cleanly**
Run: `dotnet build backend/backend/backend.csproj`
Expected: Build succeeded with 0 errors.
- [ ] **Step 5: Commit Task 2**
`git add backend/backend/Models backend/backend/Data`
`git commit -m "feat(backend): add domain entities, EF Core DbContext and database seeder"`

---

### Task 3: Test Project Setup & PO State Machine Engine
**Files:**
- Create: `backend.Tests/backend.Tests.csproj`
- Create: `backend/backend/Services/POStatusService.cs`
- Create: `backend.Tests/POStatusServiceTests.cs`

- [ ] **Step 1: Create `backend.Tests` project and add project reference**
Run: `dotnet new xunit -o backend.Tests -f net8.0`
Run: `dotnet add backend.Tests/backend.Tests.csproj reference backend/backend/backend.csproj`
Run: `dotnet add backend.Tests/backend.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 8.0.13`
Run: `dotnet add backend.Tests/backend.Tests.csproj package Moq --version 4.20.72`
- [ ] **Step 2: Write failing unit test `POStatusServiceTests.cs`**
Test allowed transitions:
- `Pending` → `Approved` (Valid)
- `Pending` → `Rejected` (Valid)
- `Approved` → `Processing` (Valid)
- `PartiallyReceived` → `Processing` (Valid)
- `Processing` → `PartiallyReceived` (Valid)
- `Processing` → `Completed` (Valid)
- `Processing` → `Approved` (Valid - Cancel)
- `Completed` → `Processing` (Throws `MSG16`)
- `Rejected` → `Approved` (Throws `MSG16`)
- [ ] **Step 3: Run test to verify it fails**
Run: `dotnet test backend.Tests/backend.Tests.csproj`
Expected: FAIL (types not found / not implemented)
- [ ] **Step 4: Implement `POStatusService.cs`**
Validate transition against permitted matrix; throw `AppException("MSG16", "The requested status transition is not allowed.", 400)` when invalid.
- [ ] **Step 5: Run tests to verify all pass**
Run: `dotnet test backend.Tests/backend.Tests.csproj`
Expected: PASS (all tests pass)
- [ ] **Step 6: Commit Task 3**
`git add backend/backend/Services/POStatusService.cs backend.Tests/`
`git commit -m "feat(backend): implement and test POStatusService state transitions"`

---

### Task 4: Repositories & DTOs Implementation
**Files:**
- Create: `backend/backend/Models/DTOs/PurchaseOrderDtos.cs`
- Create: `backend/backend/Models/DTOs/ReceivingDtos.cs`
- Create: `backend/backend/Models/DTOs/AuthDtos.cs`
- Create: `backend/backend/Models/DTOs/InventoryDtos.cs`
- Create: `backend/backend/Repositories/IPurchaseOrderRepository.cs` & `PurchaseOrderRepository.cs`
- Create: `backend/backend/Repositories/ISupplierRepository.cs` & `SupplierRepository.cs`
- Create: `backend/backend/Repositories/IProductRepository.cs` & `ProductRepository.cs`
- Create: `backend/backend/Repositories/IInventoryRepository.cs` & `InventoryRepository.cs`
- Create: `backend/backend/Repositories/IInventoryTransactionRepository.cs` & `InventoryTransactionRepository.cs`
- Create: `backend/backend/Repositories/IUserRepository.cs` & `UserRepository.cs`

- [ ] **Step 1: Create all DTO classes**
Define request/response contracts for Create PO, Review PO, Quantity Check, Start/Confirm Receiving, and View Inventory.
- [ ] **Step 2: Implement repository interfaces and EF Core implementations**
Implement methods required by SDS 3.16.5: `GetById`, `GetByStatus`, `Add`, `Update`, `GetQuantity`, `Increase`, `Add(transaction)`.
- [ ] **Step 3: Verify build**
Run: `dotnet build backend/backend/backend.csproj`
Expected: Build succeeded with 0 errors.
- [ ] **Step 4: Commit Task 4**
`git add backend/backend/Models/DTOs backend/backend/Repositories`
`git commit -m "feat(backend): implement repositories and DTOs according to SDS"`

---

### Task 5: Purchase Order Service & Review Logic (UC09, UC10, UC11)
**Files:**
- Create: `backend/backend/Services/IPurchaseOrderService.cs`
- Create: `backend/backend/Services/PurchaseOrderService.cs`
- Create: `backend.Tests/PurchaseOrderServiceTests.cs`

- [ ] **Step 1: Write failing unit tests for `PurchaseOrderService`**
- Test 1: Creating PO with Inactive supplier throws `MSG15`.
- Test 2: Creating PO with empty items throws `MSG01`.
- Test 3: Creating PO with ordered quantity $\le 0$ throws `MSG02`.
- Test 4: Creating PO with non-existent product throws `MSG05`.
- Test 5: Creating valid PO sets status `Pending` and generates code.
- Test 6: Approving `Pending` PO updates status to `Approved`.
- Test 7: Rejecting `Pending` PO records reason and status `Rejected`.
- [ ] **Step 2: Run tests to verify they fail**
Run: `dotnet test backend.Tests/backend.Tests.csproj`
Expected: FAIL
- [ ] **Step 3: Implement `PurchaseOrderService.cs`**
Implement `CreateOrder`, `ApproveOrder`, `RejectOrder`, `GetPendingOrders`, `GetAll`, `GetOrderDetail`, and `GetFormData`.
- [ ] **Step 4: Run tests to verify all pass**
Run: `dotnet test backend.Tests/backend.Tests.csproj`
Expected: PASS
- [ ] **Step 5: Commit Task 5**
`git add backend/backend/Services/ backend.Tests/`
`git commit -m "feat(backend): implement and test PurchaseOrderService (UC09, UC10, UC11)"`

---

### Task 6: Receiving Service & Atomic Inventory Transaction (UC12, UC13, UC14)
**Files:**
- Create: `backend/backend/Services/IReceivingService.cs`
- Create: `backend/backend/Services/ReceivingService.cs`
- Create: `backend/backend/Services/IInventoryService.cs`
- Create: `backend/backend/Services/InventoryService.cs`
- Create: `backend.Tests/ReceivingServiceTests.cs`

- [ ] **Step 1: Write failing unit tests for `ReceivingService`**
- Test 1: `CheckQuantity` returns invalid and `MSG09` if receiving quantity exceeds remaining.
- Test 2: `StartReceiving` on `Approved` PO sets status to `Processing`.
- Test 3: `ConfirmReceive` updates received quantity, increases warehouse stock, creates `inventory_transactions` row with `transaction_type = "IN"`.
- Test 4: Partial receive sets status to `PartiallyReceived`.
- Test 5: Full receive (remaining reaches 0) sets status to `Completed`.
- Test 6: Database rollback is triggered if an exception occurs during receive (simulated invalid warehouse/negative stock).
- [ ] **Step 2: Run tests to verify they fail**
Run: `dotnet test backend.Tests/backend.Tests.csproj`
Expected: FAIL
- [ ] **Step 3: Implement `ReceivingService.cs` and `InventoryService.cs`**
Use `_context.Database.BeginTransactionAsync()`. Ensure atomic update of items, stock in `inventories`, insertion of `inventory_transactions`, and PO status transition. Wrap in `try ... catch` and rollback on error.
- [ ] **Step 4: Run tests to verify all pass**
Run: `dotnet test backend.Tests/backend.Tests.csproj`
Expected: PASS
- [ ] **Step 5: Commit Task 6**
`git add backend/backend/Services/ backend.Tests/`
`git commit -m "feat(backend): implement and test ReceivingService with atomic inventory update"`

---

### Task 7: Authentication Service, Controllers, and Web API Wiring
**Files:**
- Create: `backend/backend/Services/IAuthService.cs` & `AuthService.cs`
- Create: `backend/backend/Controllers/AuthController.cs`
- Create: `backend/backend/Controllers/PurchaseOrderController.cs`
- Create: `backend/backend/Controllers/ReceivingController.cs`
- Create: `backend/backend/Controllers/InventoryController.cs`
- Modify: `backend/backend/Program.cs`

- [ ] **Step 1: Implement `AuthService` with JWT generation**
- [ ] **Step 2: Implement Controllers with proper HTTP attributes and role authorization**
- `AuthController`: `[HttpPost("login")]`
- `PurchaseOrderController`: `[HttpGet]`, `[HttpGet("{id}")]`, `[HttpGet("form-data")]`, `[HttpPost]`, `[HttpGet("pending")]`, `[HttpPost("{id}/approve")]`, `[HttpPost("{id}/reject")]`
- `ReceivingController`: `[HttpGet("orders")]`, `[HttpPost("{poId}/check-quantity")]`, `[HttpPost("{poId}/start")]`, `[HttpPost("{poId}/confirm")]`, `[HttpPost("{poId}/cancel")]`
- `InventoryController`: `[HttpGet]`
- [ ] **Step 3: Update `Program.cs`**
Configure DbContext, DI registrations, JWT Bearer authentication, CORS policy for `http://localhost:5173`, and automated database migration + seed on startup via `DbInitializer.Initialize(context)`.
- [ ] **Step 4: Verify backend builds and runs automated tests**
Run: `dotnet test backend.Tests/backend.Tests.csproj`
Expected: All tests pass.
- [ ] **Step 5: Commit Task 7**
`git add backend/backend/`
`git commit -m "feat(backend): wire Web API controllers, JWT auth, CORS, and Program.cs"`

---

### Task 8: Frontend Infrastructure & API Client Setup
**Files:**
- Modify: `frontend/package.json`
- Create: `frontend/src/types/models.ts`
- Create: `frontend/src/api/client.ts`
- Create: `frontend/src/api/authApi.ts`
- Create: `frontend/src/api/poApi.ts`
- Create: `frontend/src/api/receivingApi.ts`
- Create: `frontend/src/api/inventoryApi.ts`
- Create: `frontend/src/context/AuthContext.tsx`
- Create: `frontend/src/components/Navbar.tsx`
- Create: `frontend/src/components/StatusBadge.tsx`

- [ ] **Step 1: Install frontend dependencies (`react-router-dom`, `axios`)**
Run: `cd frontend; npm install react-router-dom axios`
- [ ] **Step 2: Create TypeScript interfaces in `src/types/models.ts`**
Define `PurchaseOrder`, `PurchaseOrderItem`, `Supplier`, `Product`, `Warehouse`, `InventoryItem`, `ApiResponse<T>`, `User`.
- [ ] **Step 3: Create Axios HTTP client with JWT interceptor**
Add request interceptor attaching `Authorization: Bearer <token>` and response interceptor extracting `response.data`.
- [ ] **Step 4: Create API service modules (`authApi`, `poApi`, `receivingApi`, `inventoryApi`)**
- [ ] **Step 5: Create `AuthContext` with quick persona switcher**
Include buttons:
  - "Login as Purchase Staff"
  - "Login as Warehouse Manager"
  - "Login as Warehouse Staff"
- [ ] **Step 6: Create Navbar and `StatusBadge` components**
- [ ] **Step 7: Verify frontend builds**
Run: `npm run build` inside `frontend/`
Expected: Build succeeds with 0 errors.
- [ ] **Step 8: Commit Task 8**
`git add frontend/`
`git commit -m "feat(frontend): setup Axios API client, AuthContext, types, and navigation"`

---

### Task 9: Frontend Purchase Order Management (List, Create, Review)
**Files:**
- Create: `frontend/src/pages/LoginPage.tsx`
- Create: `frontend/src/pages/PurchaseOrderListPage.tsx`
- Create: `frontend/src/pages/CreatePurchaseOrderPage.tsx`
- Create: `frontend/src/pages/PurchaseOrderDetailPage.tsx`
- Modify: `frontend/src/App.tsx`

- [ ] **Step 1: Build `LoginPage.tsx`**
Form with email/password + 3 one-click demo login buttons.
- [ ] **Step 2: Build `PurchaseOrderListPage.tsx`**
Table displaying POs, status filter dropdown, and "Tạo đơn mua hàng" action for Purchase Staff.
- [ ] **Step 3: Build `CreatePurchaseOrderPage.tsx`**
- Active Supplier dropdown.
- Expected delivery date picker.
- Dynamic item rows: Product picker, Unit price (auto-filled, editable), Quantity input, calculated Line Total.
- Real-time grand total display.
- Clear error notification displaying code (`MSG01`, `MSG02`, `MSG05`, `MSG15`).
- [ ] **Step 4: Build `PurchaseOrderDetailPage.tsx`**
- Metadata header, items table showing Ordered, Received, and Remaining quantities.
- Manager Action bar (visible when status is `Pending` and logged-in user is Warehouse Manager):
  - "Duyệt đơn hàng" button $\to$ calls `/approve` $\to$ shows `MSG11`.
  - "Từ chối" button $\to$ opens modal for Rejection Reason $\to$ calls `/reject`.
- [ ] **Step 5: Configure routes in `App.tsx`**
- [ ] **Step 6: Test frontend compilation**
Run: `npm run build` inside `frontend/`
Expected: Build succeeds.
- [ ] **Step 7: Commit Task 9**
`git add frontend/`
`git commit -m "feat(frontend): implement PO list, create form, and manager review pages"`

---

### Task 10: Frontend Receiving & Inventory Audit Pages
**Files:**
- Create: `frontend/src/pages/ReceivingListPage.tsx`
- Create: `frontend/src/pages/ReceivingDetailPage.tsx`
- Create: `frontend/src/pages/InventoryListPage.tsx`
- Modify: `frontend/src/App.tsx`

- [ ] **Step 1: Build `ReceivingListPage.tsx`**
Shows orders ready to be received (status `Approved` or `PartiallyReceived`).
- [ ] **Step 2: Build `ReceivingDetailPage.tsx`**
- Summary of PO and supplier.
- Warehouse selector dropdown (Kho Hà Nội / Kho HCM).
- Quantity input for each item.
- Action 1: "Kiểm tra số lượng" $\to$ calls UC12 check $\to$ highlights errors (`MSG09`) if invalid.
- Action 2: "Bắt đầu nhận hàng" $\to$ transitions to `Processing`.
- Action 3: "Xác nhận nhập kho" $\to$ prompts modal confirmation with item breakdown $\to$ calls `/confirm` $\to$ shows `MSG12`.
- Action 4: "Hủy bỏ" $\to$ reverts to previous status without inventory changes.
- [ ] **Step 3: Build `InventoryListPage.tsx`**
- Real-time stock per warehouse and product (shows current stock quantity).
- Table of recent `IN` transactions (Product, Warehouse, Quantity, Timestamp, Staff Name, Note) to demonstrate complete traceability.
- [ ] **Step 4: Update routes in `App.tsx`**
- [ ] **Step 5: Test frontend compilation**
Run: `npm run build` inside `frontend/`
Expected: Build succeeds.
- [ ] **Step 6: Commit Task 10**
`git add frontend/`
`git commit -m "feat(frontend): implement Receiving inspection and Inventory audit pages"`

---

### Task 11: End-to-End Integration, Database Verification & Demo Validation
**Files:**
- Modify/Verify: `backend/backend/appsettings.json`

- [ ] **Step 1: Run all backend tests**
Run: `dotnet test backend.Tests/backend.Tests.csproj`
Expected: 100% tests passing.
- [ ] **Step 2: Apply database migration / ensure database created on local SQL Server**
Run backend to verify `DbInitializer` creates `EWMN` database and seeds all base data.
- [ ] **Step 3: Run full manual walkthrough of the demo scenario**
1. Login as `staff_purchase@ewms.com`.
2. Try creating PO with Inactive supplier $\to$ verify `MSG15` alert.
3. Create PO with 2 products $\to$ verify `PO-xxxxx` generated in `Pending` status.
4. Quick-switch to `manager_kho@ewms.com`.
5. Open PO detail $\to$ Approve $\to$ verify status updates to `Approved` with `MSG11`.
6. Quick-switch to `staff_kho@ewms.com`.
7. Open Receiving $\to$ pick approved PO.
8. Enter over-quantity $\to$ verify `MSG09`.
9. Enter partial quantity $\to$ Start $\to$ Confirm into `Kho Hà Nội` $\to$ verify status `PartiallyReceived`.
10. Check Inventory page $\to$ verify Kho Hà Nội stock increased by partial quantity and `inventory_transactions` logged.
11. Receive remaining quantity $\to$ Confirm $\to$ verify status `Completed`.
- [ ] **Step 4: Final commit and documentation**
`git add .`
`git commit -m "chore: complete purchase order demo flow integration and verification"`

---
