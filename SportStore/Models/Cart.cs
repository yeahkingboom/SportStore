using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SportStore.Models
{
    public class Cart
    {
        [Key]
        public long ID { get; set; }
        public long AccountID { get; set; }
        public Account Account { get; set; }
        public ICollection<CartItem> Item { get; set; }
        public decimal Total { get { return Item.Sum(x => x.Total); } }
        public decimal DiscountTotal { get { return Item.Sum(x => x.DiscountTotal); } }
    }

    public class CartItem
    {
        [Key]
        public long ID { get; set; }
        public Cart Cart { get; set; }
        public Products Product { get; set; }
        public int Count { get; set; }
        public decimal Total { get { return Product.Price * Count; } }
        public decimal DiscountTotal { get {
                return Product.Price * Count*(decimal)Product.Shop.ShopDiscount/10;
            } }
        public bool Selected { get; set; }

    }
}
