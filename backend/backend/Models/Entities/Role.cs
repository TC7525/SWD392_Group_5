using System;
using System.Collections.Generic;

namespace backend.Models.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
