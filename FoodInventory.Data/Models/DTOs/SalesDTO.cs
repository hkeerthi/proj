
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodInventory.Data.Models.DTOs
{
    public class SalesDTO
    {       
        public int SaleId { get; set; }

        public Nullable<int> ProductId { get; set; }

        public Nullable<int> UnitsSold { get; set; }

        public Nullable<System.DateTime> SaleDate { get; set; }

        public Nullable<int> Discount { get; set; }

        public string PaymentType { get; set; }
    }
}
