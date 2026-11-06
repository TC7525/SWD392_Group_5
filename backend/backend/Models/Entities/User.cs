using System;
using System.Collections.Generic;

namespace backend.Models.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
