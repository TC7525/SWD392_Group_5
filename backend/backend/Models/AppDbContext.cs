using Microsoft.EntityFrameworkCore;
using backend.Models.Entities;
using backend.Enums;

namespace backend.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.EmployeeCode).HasColumnName("employee_code").HasMaxLength(50).IsRequired();
                entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Phone).IsUnique().HasFilter("[phone] IS NOT NULL");
            });

            // 2. Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.RoleName).HasColumnName("role_name").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // 3. UserRole
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles");
                entity.HasKey(e => e.UserRoleId);
                entity.Property(e => e.UserRoleId).HasColumnName("user_role_id");
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.RoleId).HasColumnName("role_id").IsRequired();
                entity.Property(e => e.AssignedAt).HasColumnName("assigned_at").IsRequired();
                entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");

                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 4. Supplier
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("suppliers");
                entity.HasKey(e => e.SupplierId);
                entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
                entity.Property(e => e.SupplierCode).HasColumnName("supplier_code").HasMaxLength(50).IsRequired();
                entity.Property(e => e.SupplierName).HasColumnName("supplier_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.ContactPerson).HasColumnName("contact_person").HasMaxLength(100);
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
                entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(255);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => e.SupplierCode).IsUnique();
            });

            // 5. Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.CategoryName).HasColumnName("category_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();

                entity.HasIndex(e => e.CategoryName).IsUnique();
            });

            // 6. Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
                entity.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(255).IsRequired();
                entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
                entity.Property(e => e.Brand).HasColumnName("brand").HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
                entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.MinimumStockLevel).HasColumnName("minimum_stock_level").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => e.ProductCode).IsUnique();

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 7. Warehouse
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.ToTable("warehouses");
                entity.HasKey(e => e.WarehouseId);
                entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
                entity.Property(e => e.WarehouseCode).HasColumnName("warehouse_code").HasMaxLength(50).IsRequired();
                entity.Property(e => e.WarehouseName).HasColumnName("warehouse_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(255);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => e.WarehouseCode).IsUnique();
            });

            // 8. Inventory
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("inventories");
                entity.HasKey(e => e.InventoryId);
                entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
                entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id").IsRequired();
                entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
                entity.Property(e => e.LastUpdatedAt).HasColumnName("last_updated_at").IsRequired();

                entity.HasIndex(e => new { e.ProductId, e.WarehouseId }).IsUnique();

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Inventories)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                    .WithMany(w => w.Inventories)
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 9. PurchaseOrder
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.ToTable("purchase_orders");
                entity.HasKey(e => e.PurchaseOrderId);
                entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
                entity.Property(e => e.PurchaseOrderCode).HasColumnName("purchase_order_code").HasMaxLength(50).IsRequired();
                entity.Property(e => e.SupplierId).HasColumnName("supplier_id").IsRequired();
                entity.Property(e => e.CreatedBy).HasColumnName("created_by").IsRequired();
                entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .IsRequired();
                entity.Property(e => e.OrderDate).HasColumnName("order_date").IsRequired();
                entity.Property(e => e.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
                entity.Property(e => e.ApprovedDate).HasColumnName("approved_date");
                entity.Property(e => e.CompletedDate).HasColumnName("completed_date");
                entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => e.PurchaseOrderCode).IsUnique();

                entity.HasOne(e => e.Supplier)
                    .WithMany(s => s.PurchaseOrders)
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Approver)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Items)
                    .WithOne(i => i.PurchaseOrder)
                    .HasForeignKey(i => i.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 10. PurchaseOrderItem
            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.ToTable("purchase_order_items");
                entity.HasKey(e => e.PurchaseOrderItemId);
                entity.Property(e => e.PurchaseOrderItemId).HasColumnName("purchase_order_item_id");
                entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id").IsRequired();
                entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
                entity.Property(e => e.OrderedQuantity).HasColumnName("ordered_quantity").IsRequired();
                entity.Property(e => e.ReceivedQuantity).HasColumnName("received_quantity").HasDefaultValue(0).IsRequired();
                entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2).IsRequired();

                entity.HasIndex(e => new { e.PurchaseOrderId, e.ProductId }).IsUnique();

                entity.HasOne(e => e.PurchaseOrder)
                    .WithMany(po => po.Items)
                    .HasForeignKey(e => e.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.PurchaseOrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 11. InventoryTransaction
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.ToTable("inventory_transactions");
                entity.HasKey(e => e.InventoryTransactionId);
                entity.Property(e => e.InventoryTransactionId).HasColumnName("inventory_transaction_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
                entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id").IsRequired();
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.PurchaseOrderItemId).HasColumnName("purchase_order_item_id");
                entity.Property(e => e.OutboundOrderDetailId).HasColumnName("outbound_order_detail_id");
                entity.Property(e => e.StockTransferId).HasColumnName("stock_transfer_id");
                entity.Property(e => e.TransactionType)
                    .HasColumnName("transaction_type")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
                entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
                entity.Property(e => e.TransactionDate).HasColumnName("transaction_date").IsRequired();
                entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(255);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PurchaseOrderItem)
                    .WithMany()
                    .HasForeignKey(e => e.PurchaseOrderItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
