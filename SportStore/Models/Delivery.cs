using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportStore.Models
{
    public  class Delivery
    {
        public int ID { get; set; }

        ///// <summary>
        ///// 订单ID
        ///// </summary>
        public int OrderID { get; set; }

        /// <summary>
        /// 订单
        /// </summary>
        public Order Order { get; set; }

        /// <summary>
        /// 运单号
        /// </summary>
        public string DeliveryNo { get; set; }

        /// <summary>
        /// 运单状态
        /// </summary>
        public DeliveryStatus Status { get; set; }
    }

    public class DeliveryStatus
    {
        [Key]
        public long ID { get; set; }

        public string Name { get; set; }

        public ICollection<Delivery> Deliverys { get; set; }

    }
}
