using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace POSv01.Domain.Entities
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public decimal QtyOnHand { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
