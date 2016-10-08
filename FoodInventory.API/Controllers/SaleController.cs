using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using FoodInventory.Data;
using FoodInventory.Data.Models;
using FoodInventory.Models.API;

namespace FoodInventory.API.Controllers
{
    [RoutePrefix("api/Sale")]
    public class SaleController : ApiController
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();    

        [HttpGet]
        public HttpResponseMessage Get([FromUri]int id = 0)
        {
            try
            {
                if (id == 0)
                {
                    //Get all sale info                    
                    var dbListingOfSalesToReturn = _unitOfWork.SaleRepository.Get();
                   
                    var tempListToReturn = new List<ProductSaleModel>();
                    foreach (var item in dbListingOfSalesToReturn)
                    {
                        ProductSaleModel saleProd = new ProductSaleModel();
                        saleProd.SaleId = item.SaleId;
                        saleProd.ProductId = item.ProductId;
                        var productInfo = _unitOfWork.ProductRepository.Get().Where(p => p.ID == item.ProductId).FirstOrDefault();
                        saleProd.ProductName = productInfo.Name;
                        saleProd.UnitsSold = item.UnitsSold;
                        saleProd.SaleDate = item.SaleDate;
                        saleProd.Discount = item.Discount;
                        saleProd.PaymentType = item.PaymentType;
                        saleProd.UnitsAvailable = item.UnitsAvailable;

                        tempListToReturn.Add(saleProd);
                    }
                                                
                                             
                    return Request.CreateResponse(HttpStatusCode.OK, tempListToReturn.ToList());
                }
                else
                {
                    //Get specified sale info
                    var dbSaleToReturn = _unitOfWork.SaleRepository.Get().Where(p => p.ProductId == id).FirstOrDefault();
                    var finalSaleToReturn = new FoodInventory.Data.Models.DTOs.SalesDTO
                    {
                        SaleId = dbSaleToReturn.SaleId,
                        ProductId = dbSaleToReturn.ProductId,
                        UnitsSold = dbSaleToReturn.UnitsSold,
                        SaleDate = dbSaleToReturn.SaleDate,
                        Discount = dbSaleToReturn.Discount,
                        PaymentType = dbSaleToReturn.PaymentType
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, finalSaleToReturn);
                }
            }
            catch (Exception exc)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exc.ToString());
            }
        }
    }
}
