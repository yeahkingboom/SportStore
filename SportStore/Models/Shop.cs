using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SportStore.Models
{
   public class Shop
    {
        [Key]
        public long ID { get; set; }

        public Merchant Merchant { get; set; }

        public string Name { get; set; }

        [Range(0.0,10.0,ErrorMessage ="折扣应该介于0到10之间")]
        public float? ShopDiscount { get; set; }

        public string Notice { get; set; }

        public ICollection<Products> Products { get; set; }

        public bool Check { get; set; } 

        public int Status { get; set; }

        public DateTime CreateTime { get; set; }

        public ICollection<Order> Orders { get; set; }


    }
}
