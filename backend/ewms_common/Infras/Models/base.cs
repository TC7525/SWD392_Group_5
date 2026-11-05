using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ewms_common.Infras.Models
{
    public abstract class BaseModel
    {
        [Key]
        public virtual string id { get; set; } = Guid.NewGuid().ToString();
    }

    public abstract class WarehouseModel : ExtendModel
    {
        [Comment("Mã kho")]
        public string warehouse_id { get; set; } = null!;
    }

    public abstract class ExtendModel : BaseModel
    {
        public DateTime? created_on { get; set; }
        public string? created_by { get; set; }
        public bool deleted { get; set; }
        public string? deleted_by { get; set; }
        public DateTime? deleted_on { get; set; }
        public string? modified_by { get; set; }
        public DateTime? modified_on { get; set; }
    }

    public static class entity_extensions
    {
        public static T Created<T>(this T entity, string actor_id) where T : ExtendModel
        {
            entity.created_on = DateTime.Now;
            entity.created_by = actor_id;
            entity.modified_on = DateTime.Now;
            entity.modified_by = actor_id;
            return entity;
        }

        public static T Modified<T>(this T entity, string actor_id) where T : ExtendModel
        {
            entity.modified_on = DateTime.Now;
            entity.modified_by = actor_id;
            return entity;
        }

        public static void Deleted<T>(ref T entity, string actor_id) where T : ExtendModel
        {
            entity.deleted = true;
            entity.deleted_by = actor_id;
            entity.deleted_on = DateTime.Now;
            entity.modified_on = DateTime.Now;
            entity.modified_by = actor_id;
        }

        public static T Deleted<T>(this T entity, string actor_id) where T : ExtendModel
        {
            entity.deleted = true;
            entity.deleted_by = actor_id;
            entity.deleted_on = DateTime.Now;
            entity.modified_on = DateTime.Now;
            entity.modified_by = actor_id;
            return entity;
        }
    }
}
