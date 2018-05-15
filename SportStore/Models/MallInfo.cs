using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SportStore.Models
{
    public class MallInfo
    {
        [Key]
        public int ID { get; set; }
       /// <summary>
       /// 商城折扣
       /// </summary>
        public int Discount { get; set; }

        /// <summary>
        /// 商城公告
        /// </summary>
        public string Notice { get; set; }
    }
}
