using System.Collections.Generic;

namespace backend.Models.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Active";

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
