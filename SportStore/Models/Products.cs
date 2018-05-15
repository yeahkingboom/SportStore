using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace SportStore.Models
{
    public class Products
    {
        [Key]
        public int ID { get; set; }

        //public int ShopID { get; set; }

        [Required(ErrorMessage = "必须输入{0}")]
        [Display(Name = "商品名称")]
        public string Name { get; set; }

        [Required(ErrorMessage = "必须输入{0}")]
        [Display(Name = "商品描述")]
        public string Description { get; set; }

        [Required(ErrorMessage = "必须输入{0}")]
        [Display(Name = "商品价格")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "必须输入{0}")]
        [Display(Name = "商品类别")]
        public Category Category { get; set; }

        //public Category CategoryID { get; set; }

        public Status Status { get; set; }

        //public int StatusID { get; set; }

        public Shop Shop { get; set; }

        [Required(ErrorMessage = "必须输入{0}")]
        [Display(Name = "商品库存")]
        public int Stocks { get; set; }

        public ICollection<Image> Images{ get; set; }

        public ICollection<CartItem> CartItems { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }

    public class Image
    {
        [Key]
        public int ID { get; set; }

        public Products Product { get; set; }

        public string Url { get; set; }
    }

    public class Comment
    {
        [Key]
        public long ID { get; set; }

        public Products Product { get; set;}

        public string Content { get; set; }

        public Account Account { get; set; }

        public DateTime CreateTime { get; set; }

        public ICollection<Reply> Replies { get; set; }
    }

    public class Reply
    {
        [Key]
        public long ID { get; set;}

        public Comment Comment { get; set; }

        public Account Account { get; set; }

        public string Content { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
