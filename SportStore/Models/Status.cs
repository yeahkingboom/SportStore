using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportStore.Models
{
    public class Status
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Products> Products { get; set; }
    }

    public enum ProductStatus
    {
        上架=2,
        下架=1,
        热销=3,
        活动=4
    }
}
