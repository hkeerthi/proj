using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.OleDb;
using System.Xml;
using System.Data.SqlClient;
using System.Configuration;
using FoodInventory.Data;
using FoodInventory.Data.Models;
using FoodInventory.Models;
using System.Collections;
using ClosedXML;
using ClosedXML.Excel;
using System.IO;

namespace FoodInventory.Controllers
{
    public class HomeController : Controller
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();
        ArrayList returnMessages = new ArrayList();

        #region Index
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            DataSet ds = new DataSet();

            if (Request.Files["file"].ContentLength > 0)
            {
                string fileExtension =
                                     System.IO.Path.GetExtension(Request.Files["file"].FileName);

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Request.Files["file"].FileName;

                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {

                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);


                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }
                }



                // insert data into product table
                InsertProduct(ds);

                if (returnMessages.Count > 0)
                {
                    ViewBag.MyErrorMessage = returnMessages;
                    ViewData["MyErrorMessage"] = returnMessages;
                }
            }

            return View("Index");
        }

        /// <summary>
        /// insert product records into database
        /// </summary>
        /// <param name="ds"></param>
        public void InsertProduct(DataSet ds)
        {
            // iterate the dataset and validate fields 
            bool errorFlag = false;
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                //Add this product. First check to see if product name might have been added already.
                if (ds.Tables[0].Rows[i][1] != null && !string.IsNullOrEmpty(ds.Tables[0].Rows[i][1].ToString()))
                {
                    var nameValue = ds.Tables[0].Rows[i][0].ToString();
                    var productToAdd = _unitOfWork.ProductRepository.Get().Where(p => p.Name.Trim().ToUpper() == nameValue.Trim().ToUpper()).FirstOrDefault();
                    if (productToAdd == null)
                    {
                        productToAdd = new Product();

                        // check name is not null
                        if (ds.Tables[0].Rows[i][1] != null && !string.IsNullOrEmpty(ds.Tables[0].Rows[i][1].ToString()))
                        {
                            productToAdd.Name = ds.Tables[0].Rows[i][1].ToString().Trim();
                        }
                        else
                        {
                            returnMessages.Add("Invalid product Name or don't have product name excel sheet row ID " + ds.Tables[0].Rows[i][0].ToString());
                            errorFlag = true;
                            //continue;
                        }
                        if (ds.Tables[0].Rows[i][2] != null && !string.IsNullOrEmpty(ds.Tables[0].Rows[i][2].ToString()))
                        {
                            productToAdd.Description = ds.Tables[0].Rows[i][2].ToString().Trim();
                        }
                        else
                        {
                            returnMessages.Add("Invalid product description or don't have product description in excel sheet row ID " + ds.Tables[0].Rows[i][0].ToString());
                            errorFlag = true;
                        }

                        if (ds.Tables[0].Rows[i][3] != null && Convert.ToDecimal(ds.Tables[0].Rows[i][3]) > 0)
                        {
                            productToAdd.PurchasePrice = Convert.ToDecimal(ds.Tables[0].Rows[i][3].ToString());
                        }
                        else
                        {
                            returnMessages.Add("Invalid product purchaseprice or don't have product purchaseprice in excel sheet row ID " + ds.Tables[0].Rows[i][0].ToString());
                            errorFlag = true;
                        }

                        if (ds.Tables[0].Rows[i][4] != null && Convert.ToDecimal(ds.Tables[0].Rows[i][4]) > 0)
                        {
                            productToAdd.SalesPrice = Convert.ToDecimal(ds.Tables[0].Rows[i][4].ToString());
                        }
                        else
                        {
                            returnMessages.Add("Invalid product saleprice or don't have product saleprice in excel sheet row ID " + ds.Tables[0].Rows[i][0].ToString());
                            errorFlag = true;
                        }
                        if (ds.Tables[0].Rows[i][6] != null && Convert.ToInt16(ds.Tables[0].Rows[i][6]) > 0)
                        {
                            productToAdd.UnitsAvailable = Convert.ToInt16(ds.Tables[0].Rows[i][6].ToString());
                        }
                        else
                        {
                            returnMessages.Add("Invalid product UnitsAvailable or don't have product UnitsAvailable in excel sheet row ID " + ds.Tables[0].Rows[i][0].ToString());
                            errorFlag = true;
                        }
                        if (ds.Tables[0].Rows[i][5] != null && !string.IsNullOrEmpty(ds.Tables[0].Rows[i][5].ToString()))
                        {
                            productToAdd.SpoilDate = Convert.ToDateTime(ds.Tables[0].Rows[i][5].ToString());
                        }

                        if (ds.Tables[0].Rows[i][7] != null && !string.IsNullOrEmpty(ds.Tables[0].Rows[i][7].ToString()))
                        {
                            productToAdd.DeletedDate = Convert.ToDateTime(ds.Tables[0].Rows[i][7].ToString());
                        }

                        //save the data 
                        if (!errorFlag)
                        {
                            _unitOfWork.ProductRepository.Insert(productToAdd);
                            _unitOfWork.Save();
                            //log the message
                            returnMessages.Add("Added Product Name (" + productToAdd.Name + ") from excel row ID " + ds.Tables[0].Rows[i][0].ToString());
                        }
                    }
                }
                else
                {
                    returnMessages.Add("Invalid product Name or don't have product name excel sheet row ID" + ds.Tables[0].Rows[i][0].ToString());
                    continue;
                }
            }
        }

        /// <summary>
        /// Export product list into excel sheet
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportData()
        {
            //get product list
            var productList = _unitOfWork.ProductRepository.Get();

            // create a data table with requested number of columns
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("Name");
            dt.Columns.Add("Description");
            dt.Columns.Add("PurchasePrice");
            dt.Columns.Add("SalesPrice");
            dt.Columns.Add("SpoilDate");
            dt.Columns.Add("UnitsAvailable");

            // iterate the data and insert records into datatable
            foreach (var item in productList)
            {
                DataRow _rec = dt.NewRow();
                _rec["Name"] = item.Name;
                _rec["Description"] = item.Description;
                _rec["PurchasePrice"] = item.PurchasePrice;
                _rec["SalesPrice"] = item.SalesPrice;
                _rec["SpoilDate"] = item.SpoilDate;
                _rec["UnitsAvailable"] = item.UnitsAvailable;
                dt.Rows.Add(_rec);
            }


            //export to excel file
            using (XLWorkbook wb = new XLWorkbook())
            {

                wb.Worksheets.Add(dt, "ExportData");
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename= ExportProductInfo.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
            return RedirectToAction("Index", "home");
        }

        #endregion

        #region Upload Images
        public ActionResult UploadProductImage()
        {
            List<ProductImageModel> prodList = new List<ProductImageModel>();
            ProductImageModel modelInfo = new ProductImageModel();
            var productInfo = _unitOfWork.ProductRepository.Get();
            foreach (var product in productInfo)
            {
                ProductImageModel model = new ProductImageModel();
                model.ID = product.ID;
                model.Name = product.Name;
                prodList.Add(model);
            }
            modelInfo.ProductOptions = prodList.ToList();
            return View(modelInfo);
        }

        [HttpPost]
        public ActionResult UploadProductImage(ProductImageModel model)
        {
            HttpPostedFileBase file = Request.Files["ImageData"];
            var result = UploadImageInDataBase(file, model);
            if (returnMessages.Count > 0)
            {
                ViewBag.MyErrorMessage = returnMessages;
                ViewData["MyErrorMessage"] = returnMessages;
            }
            return View("Index");
        }

        public bool UploadImageInDataBase(HttpPostedFileBase file, ProductImageModel model)
        {
            var productToEdit = _unitOfWork.ProductRepository.Get().Where(p => p.ID == model.ID).FirstOrDefault();

            if (productToEdit != null)
            {
                productToEdit.ProductImage = ConvertToBytes(file);

                _unitOfWork.ProductRepository.Update(productToEdit);
                _unitOfWork.Save();
                returnMessages.Add("Image Added to the Product Name " + productToEdit.Name);
                return true;
            }
            return false;
        }

        public byte[] ConvertToBytes(HttpPostedFileBase image)
        {
            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(image.InputStream);
            imageBytes = reader.ReadBytes((int)image.ContentLength);
            return imageBytes;
        }

        public ActionResult RetrieveImage(int id)
        {
            var productToEdit = _unitOfWork.ProductRepository.Get().Where(p => p.ID == id).FirstOrDefault();
            if (productToEdit != null)
            {
                byte[] cover = productToEdit.ProductImage;
                if (cover != null)
                {
                    return File(cover, "image/jpg");
                }
            }
            return null;
        }

        #endregion

        #region Product Sales

        public ActionResult ProductSale()
        {            
            ProductSaleModel modelInfo = GetDefaultModel();          
            return View(modelInfo);
        }

        public ProductSaleModel GetDefaultModel()
        {
            var productInfo = _unitOfWork.ProductRepository.Get();
            List<ProductImageModel> prodList = new List<ProductImageModel>();
            ProductSaleModel modelInfo = new ProductSaleModel();
            foreach (var product in productInfo)
            {
                ProductImageModel modelinfo = new ProductImageModel();
                modelinfo.ID = product.ID;
                modelinfo.Name = product.Name;
                prodList.Add(modelinfo);
            }
            modelInfo.ProductOptions = prodList.ToList();
            return (modelInfo);
        }

        [HttpPost]
        public ActionResult ProductSale(ProductSaleModel model)
        {            
            var result = PerformSale(model);
            if (returnMessages.Count > 0)
            {
                ViewBag.DisplayMessage = returnMessages[0].ToString();
                ViewData["DisplayMessage"] = returnMessages[0].ToString();
            }
            ProductSaleModel modelInfo = GetDefaultModel();
            return View(modelInfo);
        }

        public bool PerformSale(ProductSaleModel model)
        {
            if (model != null)
            {
                var productToEdit = _unitOfWork.ProductRepository.Get().Where(p => p.ID == model.ProductId).FirstOrDefault();

                if (productToEdit != null)
                {
                    if (productToEdit.UnitsAvailable < model.UnitsSold)
                    {
                        returnMessages.Add("Inventory is very low " + productToEdit.Name);
                    }
                    else
                    {
                        Sale productSale = new Sale();
                        productSale.ProductId = model.ProductId;
                        productSale.UnitsSold = model.UnitsSold;
                        productSale.Discount = model.Discount;
                        productSale.PaymentType = model.PaymentType;
                        productSale.SaleDate = DateTime.Now;
                        productSale.UnitsAvailable = productToEdit.UnitsAvailable;

                        _unitOfWork.SaleRepository.Insert(productSale);

                        productToEdit.UnitsAvailable = productToEdit.UnitsAvailable - Convert.ToInt16(model.UnitsSold);
                        _unitOfWork.ProductRepository.Update(productToEdit);

                        _unitOfWork.Save();
                        returnMessages.Add("Purchase completed " + productToEdit.Name);
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        public ActionResult AddEditProduct()
        {
            return PartialView("_AddEditProduct");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}