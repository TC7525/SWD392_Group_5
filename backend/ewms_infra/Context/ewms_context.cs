using System.Linq.Expressions;
using ewms_common.Configurations;
using ewms_common.Infras.Models;
using ewms_infra.Models.AccessLog;
using ewms_infra.Models.Accounts;
using ewms_infra.Models.Category;
using ewms_infra.Models.Contact;
using ewms_infra.Models.Permission;
using ewms_infra.Models.Product;
using ewms_infra.Models.Repository;
using ewms_infra.Models.Repository.InventoryTransaction;
using ewms_infra.Models.Repository.OutboundOrder;
using ewms_infra.Models.Repository.PurchaseOrder;
using ewms_infra.Models.Repository.StockTransfer;
using Microsoft.EntityFrameworkCore;

namespace ewms_infra.Context
{
    public class ewms_context : DbContext
    {
        public ewms_context(DbContextOptions<ewms_context> options) : base(options)
        {
        }

        #region DbSet - 20 Tables
        public DbSet<user> users { get; set; }
        public DbSet<role> roles { get; set; }
        public DbSet<user_role> user_roles { get; set; }
        public DbSet<permission> permissions { get; set; }
        public DbSet<role_permission> role_permissions { get; set; }
        public DbSet<activity_history> activity_histories { get; set; }
        public DbSet<customer> customers { get; set; }
        public DbSet<supplier> suppliers { get; set; }
        public DbSet<category> categories { get; set; }
        public DbSet<product> products { get; set; }
        public DbSet<warehouse> warehouses { get; set; }
        public DbSet<user_warehouse> user_warehouses { get; set; }
        public DbSet<inventory> inventories { get; set; }
        public DbSet<purchase_order> purchase_orders { get; set; }
        public DbSet<purchase_order_item> purchase_order_items { get; set; }
        public DbSet<outbound_order> outbound_orders { get; set; }
        public DbSet<outbound_order_detail> outbound_order_details { get; set; }
        public DbSet<stock_transfer> stock_transfers { get; set; }
        public DbSet<stock_transfer_item> stock_transfer_items { get; set; }
        public DbSet<inventory_transaction> inventory_transactions { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new user_configuration());
            modelBuilder.ApplyConfiguration(new role_configuration());
            modelBuilder.ApplyConfiguration(new user_role_configuration());
            modelBuilder.ApplyConfiguration(new permission_configuration());
            modelBuilder.ApplyConfiguration(new role_permission_configuration());
            modelBuilder.ApplyConfiguration(new activity_history_configuration());
            modelBuilder.ApplyConfiguration(new customer_configuration());
            modelBuilder.ApplyConfiguration(new supplier_configuration());
            modelBuilder.ApplyConfiguration(new category_configuration());
            modelBuilder.ApplyConfiguration(new product_configuration());
            modelBuilder.ApplyConfiguration(new warehouse_configuration());
            modelBuilder.ApplyConfiguration(new user_warehouse_configuration());
            modelBuilder.ApplyConfiguration(new inventory_configuration());
            modelBuilder.ApplyConfiguration(new purchase_order_configuration());
            modelBuilder.ApplyConfiguration(new purchase_order_item_configuration());
            modelBuilder.ApplyConfiguration(new outbound_order_configuration());
            modelBuilder.ApplyConfiguration(new outbound_order_detail_configuration());
            modelBuilder.ApplyConfiguration(new stock_transfer_configuration());
            modelBuilder.ApplyConfiguration(new stock_transfer_item_configuration());
            modelBuilder.ApplyConfiguration(new inventory_transaction_configuration());

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ExtendModel).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ExtendModel.deleted));
                    var filter = Expression.Lambda(Expression.Not(property), parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }

                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new UtcToLocalTimeConverter());
                    }
                }
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp");
            configurationBuilder.Properties<DateTime?>().HaveColumnType("timestamp");
        }
    }
}
