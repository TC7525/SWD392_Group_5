# Design Document: Purchase Order Demo Flow (EWMS)

**Date:** 2026-11-06  
**Status:** Validated & Ready for Review  
**Project:** Electronic Warehouse Management System (EWMS) — SE1972-NET - Group 5  
**Topic:** Purchase Order End-to-End Flow (UC09 – UC14)  
**Stack:** Backend: C# / ASP.NET Core 8 Web API + EF Core + SQL Server; Frontend: React 18/19 + TypeScript + Vite.

---

## 1. Context & Business Scope

### 1.1 Business Goals
Deliver a functional, verified end-to-end demo of the **Purchase Order** and **Receiving** module based strictly on the SRS (Report 1) and SDS (Report 2) for EWMS:
- **UC09:** Create Purchase Order (Purchase Staff)
- **UC10:** Review Purchase Order — Approve / Reject (Warehouse Manager)
- **UC11:** Track Purchase Order Status (Purchase Staff / Warehouse Manager / System)
- **UC12:** Check Receiving Quantity (Warehouse Staff)
- **UC13:** Receive Products (Warehouse Staff)
- **UC14:** Update Inventory from Receiving (System)

Supporting entities (Users, Roles, Suppliers, Categories, Products, Warehouses, Inventories) are seeded with realistic data. A dedicated **Inventory View** is provided to visually verify that stock levels increase after confirming receipt.

### 1.2 Actors & Roles
1. **Purchase Staff** (`HE190703` / `purchase@ewms.com`): creates and tracks POs.
2. **Warehouse Manager** (`HE194720` / `manager@ewms.com`): reviews, approves, or rejects POs.
3. **Warehouse Staff** (`HE191354` / `warehouse@ewms.com`): checks quantities, starts receiving, and confirms receiving goods into a designated warehouse.

### 1.3 Business Rules (SRS Section 5.1)
- **BR-01 / BR-04:** Only active suppliers can be selected for a new PO (`MSG15` if inactive).
- **BR-03:** Each PO item must reference an existing product (`MSG05` if not found), and ordered quantity must be > 0 (`MSG02`).
- **BR-05:** A PO must contain at least one valid product item before submission.
- **BR-06:** Only POs with `Pending` status can be approved or rejected by the Warehouse Manager.
- **BR-07:** PO workflow strictly follows:
  - `Pending` → `Approved` OR `Rejected`
  - `Approved` OR `PartiallyReceived` → `Processing` (Start Receiving)
  - `Processing` → `PartiallyReceived` OR `Completed` (Confirm Receive)
  - `Processing` → `Approved` OR `PartiallyReceived` (Cancel Receiving)
- **BR-08:** `Rejected` and `Completed` POs cannot undergo further receiving operations.
- **BR-09:** PO status transitions are driven by business events (approval, receiving confirmation) rather than arbitrary manual edits (`MSG16` if invalid transition attempted).
- **BR-10:** Cumulative received quantity $\le$ ordered quantity. Current receiving quantity must not exceed remaining quantity (`MSG09`).
- **BR-11:** Each PO references an existing supplier; historical records retain references.
- **BR-12 / BR-13:** Inventory quantities increase only upon confirmed receiving. Every movement records an `inventory_transactions` record of type `IN`.
- **BR-14 / BR-15:** Inventory updates, transaction logging, PO received quantity, and status updates must execute in a single atomic database transaction; rollback and return `MSG17` on any failure. Responsible user ID and timestamp must be logged.

### 1.4 Application Messages (SRS Section 5.3)
| Code | Type | Meaning |
|---|---|---|
| `MSG01` | Error | Required information is missing. |
| `MSG02` | Error | Quantity must be greater than 0. |
| `MSG04` | Error | This order cannot be processed in its current status. |
| `MSG05` | Error | The selected product was not found. |
| `MSG09` | Error | Received quantity cannot exceed the remaining ordered quantity. |
| `MSG10` | Success | Purchase order created successfully. |
| `MSG11` | Success | Purchase order approved successfully. |
| `MSG12` | Success | Products received and inventory updated successfully. |
| `MSG13` | Error | The selected supplier is invalid. |
| `MSG15` | Error | The selected supplier is inactive and cannot be used. |
| `MSG16` | Error | The requested status transition is not allowed. |
| `MSG17` | Error | The operation could not be completed. No inventory changes were saved. |
| `MSG-EMP-01` | Error | Invalid username or password. |
| `MSG-EMP-02` | Error | This employee account is inactive. |
| `MSG-EMP-03` | Error | You do not have permission to perform this action. |

---

## 2. System Architecture & Project Structure

The project adopts a 3-tier architecture with separate Presentation (Web API + React SPA), Business Logic (Services + Enums + Status Engine), and Data Access (Repositories + EF Core DbContext + SQL Server).

```
SWD392_Group_5/
├── backend/
│   └── backend/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── PurchaseOrderController.cs
│       │   ├── ReceivingController.cs
│       │   ├── InventoryController.cs
│       │   └── SeedDataController.cs (dev utility / auto-migration)
│       ├── Services/
│       │   ├── AuthService.cs
│       │   ├── PurchaseOrderService.cs
│       │   ├── ReceivingService.cs
│       │   ├── InventoryService.cs
│       │   └── POStatusService.cs (centralized state-machine validator)
│       ├── Repositories/
│       │   ├── IPurchaseOrderRepository.cs & PurchaseOrderRepository.cs
│       │   ├── ISupplierRepository.cs & SupplierRepository.cs
│       │   ├── IProductRepository.cs & ProductRepository.cs
│       │   ├── IInventoryRepository.cs & InventoryRepository.cs
│       │   ├── IInventoryTransactionRepository.cs & InventoryTransactionRepository.cs
│       │   └── IUserRepository.cs & UserRepository.cs
│       ├── Models/
│       │   ├── Entities/ (User, Role, UserRole, Supplier, Category, Product, Warehouse, Inventory, PurchaseOrder, PurchaseOrderItem, InventoryTransaction)
│       │   ├── DTOs/ (Requests & Responses)
│       │   └── AppDbContext.cs
│       ├── Enums/
│       │   ├── POStatus.cs (Pending, Approved, Rejected, Processing, PartiallyReceived, Completed)
│       │   ├── TransactionType.cs (IN, OUT, TRANSFER)
│       │   └── SupplierStatus.cs (Active, Inactive)
│       ├── Common/
│       │   ├── ApiResponse.cs (Envelope with Code & Message)
│       │   ├── AppExceptions.cs
│       │   └── JwtHelper.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── backend.csproj
├── backend.Tests/ (xUnit test suite for business rules & state transitions)
└── frontend/
    ├── src/
    │   ├── api/ (client.ts, authApi.ts, poApi.ts, receivingApi.ts, inventoryApi.ts)
    │   ├── components/ (Navbar, Layout, ProtectedRoute, StatusBadge, Modal)
    │   ├── context/ (AuthContext.tsx)
    │   ├── pages/
    │   │   ├── LoginPage.tsx
    │   │   ├── PurchaseOrderListPage.tsx
    │   │   ├── CreatePurchaseOrderPage.tsx
    │   │   ├── PurchaseOrderDetailPage.tsx
    │   │   ├── PurchaseOrderReviewPage.tsx (Manager)
    │   │   ├── ReceivingListPage.tsx (Staff)
    │   │   ├── ReceivingDetailPage.tsx (Staff inspect & confirm)
    │   │   └── InventoryListPage.tsx
    │   ├── types/ (models.ts)
    │   ├── App.tsx
    │   └── main.tsx
```

---

## 3. Database Design & EF Core Mapping

The database matches the provided SQL schema with targeted additions approved in user decisions:
1. `purchase_orders.expected_delivery_date DATETIME NULL` (required by UC09 step 3 & 7).
2. `purchase_orders.rejection_reason VARCHAR(255) NULL` (required by UC10 step 6 & SD08).

### 3.1 Key Tables in Flow
- **`suppliers`**: `supplier_id (PK)`, `supplier_code`, `supplier_name`, `contact_person`, `phone`, `email`, `address`, `status (VARCHAR(20): Active/Inactive)`, timestamps.
- **`categories`**: `category_id (PK)`, `category_name`, `description`, `status`.
- **`products`**: `product_id (PK)`, `product_code`, `product_name`, `category_id (FK)`, `brand`, `description`, `unit_price`, `minimum_stock_level`, `status`, timestamps.
- **`warehouses`**: `warehouse_id (PK)`, `warehouse_code`, `warehouse_name`, `address`, `status`, timestamps.
- **`inventories`**: `inventory_id (PK)`, `product_id (FK)`, `warehouse_id (FK)`, `quantity (INT >= 0)`, `last_updated_at (DATETIME)`. Unique on `(product_id, warehouse_id)`.
- **`purchase_orders`**:
  - `purchase_order_id (PK)`
  - `purchase_order_code (VARCHAR(50), UNIQUE)` (e.g. `PO-20261106-0001`)
  - `supplier_id (FK -> suppliers)`
  - `created_by (FK -> users)`
  - `approved_by (FK -> users, nullable)`
  - `status (VARCHAR(30))`
  - `order_date (DATETIME)`
  - `expected_delivery_date (DATETIME, nullable)`
  - `approved_date (DATETIME, nullable)`
  - `completed_date (DATETIME, nullable)`
  - `rejection_reason (VARCHAR(255), nullable)`
  - `created_at`, `updated_at`
- **`purchase_order_items`**:
  - `purchase_order_item_id (PK)`
  - `purchase_order_id (FK -> purchase_orders)`
  - `product_id (FK -> products)`
  - `ordered_quantity (INT > 0)`
  - `received_quantity (INT >= 0, default 0)`
  - `unit_price (DECIMAL(18,2))`
  - Unique on `(purchase_order_id, product_id)`
- **`inventory_transactions`**:
  - `inventory_transaction_id (PK)`
  - `product_id (FK -> products)`
  - `warehouse_id (FK -> warehouses)`
  - `user_id (FK -> users)`
  - `purchase_order_item_id (FK -> purchase_order_items, nullable)`
  - `transaction_type (VARCHAR(20))` -> `'IN'`
  - `quantity (INT > 0)`
  - `transaction_date (DATETIME)`
  - `note (VARCHAR(255))`
- **`users`**, **`roles`**, **`user_roles`**: authentication & RBAC.

### 3.2 Seeding Plan
- **Roles**: `Purchase Staff`, `Warehouse Manager`, `Warehouse Staff`.
- **Users**:
  - `staff_purchase@ewms.com` (Pass: `123456`, Name: "Nguyen Van Mua", Role: Purchase Staff)
  - `manager_kho@ewms.com` (Pass: `123456`, Name: "Tran Quan Ly", Role: Warehouse Manager)
  - `staff_kho@ewms.com` (Pass: `123456`, Name: "Le Nhan Vien Kho", Role: Warehouse Staff)
- **Suppliers**:
  - `SUP001`: Công ty TNHH Điện tử Samsung VN (Active)
  - `SUP002`: Nhà phân phối Linh kiện Synnex FPT (Active)
  - `SUP003`: Nhà cung cấp Cũ Dừng Hợp tác (Inactive — to test `MSG15`)
- **Warehouses**:
  - `WH-HN-01`: Kho Hà Nội (Active)
  - `WH-HCM-01`: Kho TP Hồ Chí Minh (Active)
- **Categories & Products**:
  - `PRD-IP16`: iPhone 16 Pro Max 256GB (Price: 34,990,000, Min: 10)
  - `PRD-S24`: Samsung Galaxy S24 Ultra (Price: 29,990,000, Min: 10)
  - `PRD-MBA`: MacBook Air M3 16GB (Price: 27,500,000, Min: 5)
  - `PRD-SNY`: Tai nghe Sony WH-1000XM5 (Price: 7,490,000, Min: 8)

---

## 4. State Machine & Status Engine (Report 2 Diagram 3.17.2)

```
       [Submit Order]
             │
             ▼
        ┌─────────┐
        │ Pending │
        └────┬────┘
             │
      ┌──────┴──────┐
 [Approve]       [Reject]
      │             │
      ▼             ▼
┌──────────┐  ┌──────────┐
│ Approved │  │ Rejected │ (Final)
└─────┬────┘  └──────────┘
      │
      │ ◄──────────┐ [Cancel]
      │ [Start]    │
      ▼            │
┌────────────┐─────┘
│ Processing │
└─────┬──────┘
      │
      ├────────────────────────┬──────────────────────┐
      │ [Confirm: Partial]     │ [Confirm: Full]      │ [Cancel]
      ▼                        ▼                      │
┌──────────────────┐     ┌───────────┐                │
│ PartiallyReceived│     │ Completed │ (Final)        │
└────────┬─────────┘     └───────────┘                │
         │                                            │
         │ [Start / Continue Receiving]               │
         ▼                                            ▼
   (back to Processing) ─────────────────────► (back to PartiallyReceived)
```

**Transition Rules Table**:
| Current Status | Target Event / Action | Next Status | Allowed Roles |
|---|---|---|---|
| (None) | Create PO (UC09) | `Pending` | Purchase Staff |
| `Pending` | Approve (UC10) | `Approved` | Warehouse Manager |
| `Pending` | Reject (UC10) | `Rejected` | Warehouse Manager |
| `Approved` | Start Receiving (UC13) | `Processing` | Warehouse Staff |
| `PartiallyReceived` | Continue Receiving (UC13) | `Processing` | Warehouse Staff |
| `Processing` | Cancel Receiving | Previous state (`Approved` or `PartiallyReceived`) | Warehouse Staff |
| `Processing` | Confirm (when $\sum \text{received} < \sum \text{ordered}$) | `PartiallyReceived` | Warehouse Staff |
| `Processing` | Confirm (when all items $\text{received} == \text{ordered}$) | `Completed` | Warehouse Staff |

Any invalid status request returns HTTP 400 with `{ code: "MSG16", message: "The requested status transition is not allowed." }`.

---

## 5. API Endpoints Specification

Standard Envelope for responses:
```json
// Success
{
  "code": "MSG10",
  "message": "Purchase order created successfully.",
  "data": { ... }
}

// Error
{
  "code": "MSG09",
  "message": "Received quantity cannot exceed the remaining ordered quantity.",
  "data": null
}
```

### 5.1 Authentication API
- `POST /api/auth/login`
  - Body: `{ email, password }`
  - Returns: JWT token, user profile `{ userId, fullName, email, role }`

### 5.2 Purchase Order API (UC09, UC10, UC11)
- `GET /api/purchase-orders` (UC11)
  - Query: `?status=Pending` (optional filter)
  - Returns: list of PO headers with supplier name, total items, total quantity, status, date.
- `GET /api/purchase-orders/{id}` (UC11, UC10, UC12)
  - Returns: detailed PO with items, including calculated fields:
    - `orderedQuantity`
    - `receivedQuantity`
    - `remainingQuantity` (`orderedQuantity - receivedQuantity`)
    - `lineTotal` (`orderedQuantity * unitPrice`)
    - `grandTotal`
- `GET /api/purchase-orders/form-data` (UC09)
  - Returns: `{ suppliers: [...active only...], products: [...active products...] }`
- `POST /api/purchase-orders` (UC09)
  - Role: `Purchase Staff`
  - Body:
    ```json
    {
      "supplierId": 1,
      "expectedDeliveryDate": "2026-11-20T00:00:00",
      "items": [
        { "productId": 1, "orderedQuantity": 10, "unitPrice": 34990000 },
        { "productId": 2, "orderedQuantity": 5, "unitPrice": 29990000 }
      ]
    }
    ```
  - Validation:
    - Supplier exists and `Status == "Active"` (else `MSG15`)
    - Items count > 0 (else `MSG01`)
    - Each item `orderedQuantity > 0` (else `MSG02`)
    - Product exists (else `MSG05`)
  - Result: status `Pending`, returns `MSG10`.
- `GET /api/purchase-orders/pending` (UC10)
  - Role: `Warehouse Manager`
  - Returns: list of POs with status `Pending`.
- `POST /api/purchase-orders/{id}/approve` (UC10)
  - Role: `Warehouse Manager`
  - Status must be `Pending` (else `MSG04`/`MSG16`).
  - Sets status to `Approved`, `approved_by = currentUserId`, `approved_date = DateTime.UtcNow`.
  - Returns `MSG11`.
- `POST /api/purchase-orders/{id}/reject` (UC10)
  - Role: `Warehouse Manager`
  - Body: `{ "reason": "Giá tăng bất thường" }`
  - Status must be `Pending`. Requires reason (else `MSG01`).
  - Sets status to `Rejected`, `approved_by = currentUserId`, `rejection_reason`.

### 5.3 Receiving & Inventory API (UC12, UC13, UC14)
- `GET /api/receiving/orders`
  - Role: `Warehouse Staff`
  - Returns: POs with status `Approved` or `PartiallyReceived`.
- `POST /api/receiving/{poId}/check-quantity` (UC12)
  - Role: `Warehouse Staff`
  - Body: `[ { "productId": 1, "quantity": 5 }, ... ]`
  - Checks if each `quantity > 0` (`MSG02`) and `quantity <= remainingQuantity` (`MSG09`).
  - Returns: `{ valid: true/false, items: [ { productId, requestedQty, remainingQty, isValid } ] }`.
- `POST /api/receiving/{poId}/start` (UC13)
  - Role: `Warehouse Staff`
  - Transitions PO from `Approved` / `PartiallyReceived` to `Processing`.
- `POST /api/receiving/{poId}/cancel` (UC13)
  - Role: `Warehouse Staff`
  - Cancels current inspection; reverts status to `Approved` or `PartiallyReceived`.
- `POST /api/receiving/{poId}/confirm` (UC13 + UC14)
  - Role: `Warehouse Staff`
  - Body:
    ```json
    {
      "warehouseId": 1,
      "note": "Lô hàng đợt 1 nguyên đai nguyên kiện",
      "items": [
        { "purchaseOrderItemId": 10, "receivedQuantity": 5 }
      ]
    }
    ```
  - **Atomic Transaction Sequence (BR-12, BR-13, BR-14, BR-15)**:
    1. Verify PO is in status `Processing`.
    2. Verify warehouse exists and is Active (`MSG17`).
    3. For each received item:
       - Validate $0 < \text{receivedQuantity} \le (\text{orderedQuantity} - \text{receivedQuantity})$. If invalid $\to$ throw with `MSG09`.
       - Increment `purchase_order_items.received_quantity += receivedQuantity`.
       - Find or create `inventories` row for `(productId, warehouseId)`. Increment `quantity += receivedQuantity`, set `last_updated_at = UtcNow`.
       - Insert `inventory_transactions` row (`transaction_type = "IN"`, `quantity`, `warehouse_id`, `product_id`, `purchase_order_item_id`, `user_id = staffId`, `transaction_date = UtcNow`, `note`).
    4. Calculate new PO status:
       - If all items have `received_quantity == ordered_quantity`: status = `Completed`, `completed_date = UtcNow`.
       - Else: status = `PartiallyReceived`.
    5. Save changes and commit transaction.
    6. Return `MSG12`.
    7. On exception: automatic rollback, no partial state saved, return `MSG17`.
- `GET /api/inventories`
  - Returns current stock per product and warehouse, plus recent `IN` transactions for full traceability.

---

## 6. Frontend UI / Screen Specifications

Matches Section 1.4.1 (Screen Flow) and 1.4.2 (Screen Authorization).

1. **Login Page (`/login`)**:
   - Form for email & password + quick-switch helper buttons for the 3 demo accounts (Purchase Staff, Manager, Warehouse Staff).
2. **Top Navigation Bar**:
   - App title, current logged-in user with role badge, navigation links filtered by role, logout button.
3. **Purchase Order List (`/purchase-orders`)** (Purchase Staff & Manager):
   - Table of POs: Code, Supplier, Date, Expected Delivery, Total Amount, Status (with color badge: Pending = yellow, Approved = blue, Processing = purple, PartiallyReceived = orange, Completed = green, Rejected = red).
   - "Create PO" button for Purchase Staff.
   - Filter by status dropdown.
4. **Create Purchase Order Form (`/purchase-orders/create`)** (Purchase Staff):
   - Dropdown of Active Suppliers.
   - Expected delivery date picker.
   - Dynamic line items: Product selector, Unit Price input (auto-filled with default product price), Ordered Quantity input, calculated Line Total.
   - Grand total summary display.
   - Clear validation error alerts with message codes (`MSG01`, `MSG02`, `MSG05`, `MSG15`).
5. **Purchase Order Detail / Review Page (`/purchase-orders/:id`)**:
   - Header metadata (Supplier, Creator, Order Date, Status, Approver, Rejection Reason if any).
   - Line items table: Product Name, Unit Price, Ordered Qty, Received Qty, Remaining Qty, Subtotal.
   - **For Warehouse Manager (when `Pending`)**:
     - "Approve" button $\to$ confirmation modal $\to$ calls `/approve` $\to$ `MSG11`.
     - "Reject" button $\to$ modal prompting for Rejection Reason $\to$ calls `/reject`.
6. **Receiving Management Page (`/receiving`)** (Warehouse Staff):
   - List of orders ready for receiving (status `Approved` or `PartiallyReceived`).
   - "Receive Goods" action leading to the inspection screen.
7. **Receiving Detail / Inspection Page (`/receiving/:id`)** (Warehouse Staff):
   - Order summary and supplier note.
   - Warehouse selection dropdown (Kho Hà Nội / Kho HCM).
   - Input column: "Receiving Now" for each item.
   - Step 1: "Check Quantity" button $\to$ runs UC12 validation $\to$ displays warning if $> \text{remaining}$ (`MSG09`) or success indicator.
   - Step 2: "Start Receiving" $\to$ switches order to `Processing`.
   - Step 3: "Confirm Receipt" $\to$ prompts confirmation modal with summary of items $\to$ executes atomic inventory update $\to$ shows `MSG12`.
   - Option: "Cancel Inspection" $\to$ reverts to previous state without changing inventory.
8. **Inventory Overview Page (`/inventory`)**:
   - Shows stock per warehouse and product so evaluator can verify stock increases immediately after receiving.
   - Shows latest `IN` inventory transactions with timestamp and user.

---

## 7. Testing & Verification Plan

### 7.1 Automated Backend Tests (xUnit in `backend.Tests/`)
1. **Validation on Creation (UC09)**:
   - Inactive supplier $\to$ rejects with `MSG15`.
   - Empty items list $\to$ rejects with `MSG01`.
   - Zero or negative quantity $\to$ rejects with `MSG02`.
   - Valid payload $\to$ creates PO with status `Pending` (`MSG10`).
2. **Review Transitions (UC10)**:
   - Approve `Pending` PO $\to$ status `Approved` (`MSG11`).
   - Reject `Pending` PO $\to$ status `Rejected` with reason.
   - Attempting to approve an already `Approved` or `Completed` PO $\to$ rejects with `MSG16`.
3. **Receiving Checks & State Flow (UC12, UC13)**:
   - Check quantity $> \text{remainingQuantity}$ $\to$ flagged invalid (`MSG09`).
   - Start receiving on non-approved PO $\to$ rejected (`MSG04`).
   - Partial receive $\to$ status becomes `PartiallyReceived`, inventory increases by partial amount.
   - Second receive up to total $\to$ status becomes `Completed`, inventory increases again.
   - Over-receiving on second shipment $\to$ blocked.
4. **Transaction Atomicity (UC14)**:
   - Verify DB rollback if inventory write fails (simulated via invalid warehouse ID).

### 7.2 End-to-End Walkthrough Verification
1. Log in as Purchase Staff (`staff_purchase@ewms.com`).
2. Attempt to create PO with inactive supplier $\to$ verify `MSG15` appears.
3. Create valid PO with 2 products $\to$ verify `PO-xxxxx` generated with status `Pending`.
4. Switch user to Warehouse Manager (`manager_kho@ewms.com`).
5. Open PO Review $\to$ click Approve $\to$ verify status changes to `Approved`.
6. Switch user to Warehouse Staff (`staff_kho@ewms.com`).
7. Open Receiving Management $\to$ select approved PO.
8. Enter quantity exceeding ordered $\to$ click Check $\to$ verify `MSG09`.
9. Enter partial quantity $\to$ Start $\to$ Confirm $\to$ verify status `PartiallyReceived` and Inventory list shows increased stock.
10. Receive remainder $\to$ Confirm $\to$ verify status `Completed`.
11. Verify PO cannot be received anymore (BR-08).

