using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodInventory.Models.API
{
    public class ProductSaleModel
    {
        public int SaleId { get; set; }

        public Nullable<int> ProductId { get; set; }

        public Nullable<int> UnitsSold { get; set; }

        public Nullable<System.DateTime> SaleDate { get; set; }

        public Nullable<int> Discount { get; set; }

        public string PaymentType { get; set; }

        public string ProductName { get; set; }
        public Nullable<int> UnitsAvailable { get; set; }


    }
}