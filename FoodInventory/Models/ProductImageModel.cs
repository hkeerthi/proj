using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodInventory.Models
{    
    public class ProductImageModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalesPrice { get; set; }
        public Nullable<System.DateTime> SpoilDate { get; set; }
        public int UnitsAvailable { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }

        public byte[] ProductImage { get; set; }

        public List<ProductImageModel>ProductOptions{ get; set; }

}
}