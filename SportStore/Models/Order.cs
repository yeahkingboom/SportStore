using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SportStore.Models
{
    public enum OrdStatus
    {
        /// <summary>
        /// 待支付
        /// </summary>
        UnPay=1,

        /// <summary>
        /// 待发货
        /// </summary>
        UnDelivery=2,

        /// <summary>
        /// 待收货
        /// </summary>
        Delivering=3,

        /// <summary>
        /// 已完成
        /// </summary>
        AlreadyDone=4,

        /// <summary>
        /// 已关闭
        /// </summary>
        Closed=5,

        /// <summary>
        /// 正在取消
        /// </summary>
        Closing = 6,

        /// <summary>
        /// 新建
        /// </summary>
        New=7
    }
    public class Order 
    {
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// 所购商品  
        /// </summary>
        public ICollection<OrderItem> OrderItems { get; set; }

        /// <summary>
        /// 购物者
        /// </summary>
        public Account Customer { get; set; }

        /// <summary>
        /// 收货人
        /// </summary>
        public Receiver Receiver { get; set; }

        public long ReceiverID { get; set; }

        /// <summary>
        /// 订单所归属商店
        /// </summary>
        public Shop Shop { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        public Payment PayMent { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderStatus Status { get; set; }


        /// <summary>
        /// 物流
        /// </summary>
        public Delivery Delivery { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 订单总价
        /// </summary>
        public decimal TotalPrice { get; set; }
    }

    public class OrderStatus
    {
        [Key]
        public long ID { get; set; }

        public string Name { get; set; }

        public ICollection<Order> Orders { get; set; }

    }


    public class OrderItem
    {
        [Key]
        public long ID { get; set; }

        public Products Product { get; set; }

        public Order Order { get; set; }

        public int Count { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal Price { get; set; }
    }

    public enum PayWay
    {
        /// <summary>
        /// 在线支付
        /// </summary>
        Online=0,
        /// <summary>
        /// 活到付款
        /// </summary>
        Cash=1
    }

    public class Payment
    {
        [Key]
        public long ID { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public PayWay PayWay { get; set; }

        public Account Account { get; set; }

        public ICollection<Order> Orders { get; set; }

        public Decimal Total { get; set; }

        /// <summary>
        /// 是否支付
        /// </summary>
        public bool UnPay { get; set; }
    }
}
