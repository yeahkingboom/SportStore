using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SportStore.Models
{
    public class Account
    {
        [Key]
        public long ID { get; set; }

        [Required(ErrorMessage ="必须输入{0}")]
        [StringLength(30,MinimumLength =4,ErrorMessage ="{0}长度必须为{2}-{1}之间")]
        [Display(Name ="用户名")]
        //[Remote("这里换成检测用户名是否存在的action")]
        public string Name { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "必须输入{0}")]
        [StringLength(256, ErrorMessage = "{0}长度少于{1}个字符")]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "必须输入{0}")]
        [Display(Name = "邮箱")]
        //[Remote("换成检测邮箱是否存在的action")]
        //[RegularExpression(".+\\@.+\\..+",ErrorMessage ="邮箱格式不正确")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "必须输入{0}")]
        [Display(Name = "手机号")]
        [StringLength(11,MinimumLength =11,ErrorMessage ="手机号码格式不正确")]
        public string Phone { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 购物车
        /// </summary>
        public Cart ShoppingCart { get; set; }
        //public string Discriminator { get; set; }


        public ICollection<Receiver> Receivers { get; set; }

        /// <summary>
        /// 用户订单
        /// </summary>
        public ICollection<Order> Orders { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public ICollection<Reply> Replies { get; set; }

        public Apply Apply { get; set; }

        public ICollection<Payment> Payments { get; set; }
    }

    public class Apply
    {
        [Key]
        public long ID { get; set; }
        /// <summary>
        /// 申请材料Url
        /// </summary>
        public String DocumentUrl { get; set; }

        /// <summary>
        /// 申请材料名称
        /// </summary>
        public String DocumentName { get; set; }


        public DateTime CreateTime { get; set; }
        /// <summary>
        /// -1审核不通过，0未审核，1审核通过
        /// </summary>
        public ApplyStatus Status { get; set; }

        public Account Account { get; set; }

        public long AccountID { get; set; }
    }

    public enum ApplyStatus
    {
        False = -1,
        Uncheck = 0,
        True = 1
    }

    public class Receiver
    {
        [Key]
        public long ID { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Name { get; set; }

        public Account Account { get; set; }

        public ICollection<Order> Orders { get; set; }

        

    }
}
