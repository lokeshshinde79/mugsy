using PhotoApp.Web;
using PhotoApp.Web.Ecommerce;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Data;

namespace MugsyDigitalOrderFulfilmentService
{
    internal class ProcessDigitalOrders
    {
        private static string BucketName = "customer";
        public static Guid AppId
        {
            get { return new Guid(ConfigurationManager.AppSettings["AppId"]); }
        }

        public static void InsertLog(string LogInfo)
        {
            try
            {
                string DirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logdata");
                if (!Directory.Exists(DirectoryPath))
                {
                    Directory.CreateDirectory(DirectoryPath);
                }
                string FileName = Path.Combine(DirectoryPath + @"\" + "Log-" + DateTime.Today.ToString("yyyyMMdd") + ".txt");
                FileStream fs = null;
                if (!File.Exists(FileName))
                {
                    using (fs = File.Create(FileName))
                        fs.Flush();
                    fs.Close();

                }
                StreamWriter sw = new StreamWriter(FileName, true);
                sw.WriteLine(LogInfo);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static void ProcessOrder()
        {
            DigiServiceProduct _digiServiceProduct = new DigiServiceProduct();
            List<string> listDigitalProducts= _digiServiceProduct.GetDigiServiceProducts().Select(x => x.ProductCode.ToLower()).ToList();
            OrderPullSettings Settings = new OrderPullSettings
            {
                Status = OrderStatus.All,
                Type = OrderType.WebOrder,
                OverdueOrdersOnly = false,
                StartDate = new DateTime(1900, 1, 1),
                EndDate = DateTime.MaxValue,
                //OverdueOrdersOnly = chkOverdueOnly.Checked // We are uses this to get filtered result as shipped or not shiupped order NOTE: ONLY FOR THIS PAGE
                CustomerName =string.Empty,
                OrderNumber = 0,
                CustomerEmail =string.Empty
            };
            Guid appId = new Guid("0E2F5B05-7374-4094-82C6-340B2B21F15B");
            List<Order> digitalOrders = Orders.GetDigitalOrderList(appId, Settings, true).Where(O => O.DigiFileStatus == false).ToList();
            InsertLog("Total orders Found : " + digitalOrders.Count + string.Format(" With order number ({0})", string.Join(", ", digitalOrders.Select(d => d.Number))));
            foreach (var digiOrd in digitalOrders)
            {
                if (!OrderNumberlist.Add_OrderId(digiOrd.OrderId))
                {
                    continue; }
                // Getting Order Items
                OrderPullSettings ordSetting = new OrderPullSettings()
                {
                    LoadOrderItems = false
                };
                var orderObj = Orders.Get(appId, digiOrd.OrderId, ordSetting);

                OrderItemPullSettings setting = new OrderItemPullSettings()
                {
                    LoadObjectAttributes = true,
                    OrderId = digiOrd.OrderId
                };
                var ordItems = PhotoApp.Web.Ecommerce.OrderItems.GetOrderItems(appId, setting);
                if (ordItems != null && ordItems.Count > 0)
                {
                    orderObj.OrderItems = ordItems;
                    bool ans = AutoLinkDigitalFileToCustomerDashboard(orderObj, orderObj.Number, orderObj.Email, true, listDigitalProducts);
                    InsertLog(DateTime.Now + " : Digital order link for order number " + orderObj.Number + " has resulted " + (ans ? "SUCCESS" : "FAIL"));
                }
            }
        }
        internal static bool AutoLinkDigitalFileToCustomerDashboard(Order orderObj, string OrderNumber, string userName, bool notifyCustomer,List<string> DigitalServiceProducts)
        {
            bool uploadedDigiFiles = false;
            List<RetouchedImageInfo> allRetouchImages = new List<RetouchedImageInfo>();
            List<RetouchedImageInfo> allNonRetouchImages = new List<RetouchedImageInfo>();
            List<RetouchedImageInfo> allItemsWithImages = new List<RetouchedImageInfo>();

            try
            {
                InsertLog(DateTime.Now + " : Starting digital file link for order number " + OrderNumber);
                List<Guid> RetouchOptionsList = (from t in Products.GetProductsByAttribute(AppId, "IsRetouch")
                                                 select t.ProductId).ToList();//get all retouch product
                List<Guid> imageIds = new List<Guid>();
                if (orderObj != null && orderObj.OrderItems.Count > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    //Step 1 : Get all digital order items ( products )
                    List<string> FixProducts = DigitalServiceProducts; //new List<string>() { "autodownload_digibonus", "HQ-DigiFile(1)".ToLower(), "HQ-DigiFile(all)".ToLower(), "HQ-DigiFile(1)STAFF".ToLower(), "GRAD_DIGIFILE(1)".ToLower() };
                    var filteredItems = (from t in orderObj.OrderItems where (t.ItemCode.ToLower().Contains("digi") && t.ParentOrderItemGuid != Guid.Empty)
                                         || FixProducts.Contains(t.ItemCode.ToLower())
                                         select t).ToList();
                    #region Process Hq-DigiFile(ALL) product
                    if (filteredItems != null)
                    {
                        //For the HQ-DigiFile(All) product orders has only one order item detail row. so we just copy a row to number of images 
                        //in the gallery times. because we need to link all images in gallery for customer dashboard.
                        var selectedItem = ListAllImageDownloadOrderItems(filteredItems);// filteredItems.Where(c => c.ItemCode.ToLower().Equals("HQ-DigiFile(all)".ToLower())).ToList();
                        selectedItem.ForEach(x=>{
                            bool doProgessAllImaegs = AllowAllImageDownload(x.ProductId);
                            if (x.ObjectAttributes != null) {
                                var imageAttribute = x.ObjectAttributes.Where(c => c.AttributeCode.ToLower().Equals("imageid")).FirstOrDefault();
                                if (imageAttribute != null && (imageAttribute.ObjectAttributeValues != null && imageAttribute.ObjectAttributeValues.Count > 0))
                                {
                                    Image objImage = imageAttribute.ObjectAttributeValues[0].Value as Image;
                                    Guid tImgID = Guid.Empty;
                                    if (objImage == null)
                                        tImgID = new Guid(imageAttribute.ObjectAttributeValues[0].Value.ToString());
                                    else
                                        tImgID = objImage.ImageId;
                                    Image img = Images.Get(AppId, tImgID);
                                    if ((tImgID != Guid.Empty && img!=null) && doProgessAllImaegs==true)
                                    {
                                        var dtImage = Galleries.GetImages(AppId, img.GalleryId);
                                        if (dtImage != null)
                                        {
                                            dtImage.Rows.Cast<DataRow>().ToList().ForEach((dr) => {
                                                Guid tempImageId = Guid.Empty;
                                                Guid.TryParse(Convert.ToString(dr["ImageId"]), out tempImageId);
                                                if (tImgID.Equals(tempImageId))
                                                    return;
                                                OrderItem temp_OrderItem = new OrderItem(AppId)
                                                {
                                                    OrderGuid = x.OrderGuid,
                                                    OrderItemGuid = Guid.NewGuid(),
                                                    ProductId = x.ProductId,
                                                    ItemCode = x.ItemCode,
                                                    ItemQuantity = x.ItemQuantity,
                                                    ItemPrice = x.ItemPrice,
                                                    ItemShipping = x.ItemShipping,
                                                    ItemSku = x.ItemSku,
                                                    ItemTitle = x.ItemTitle,
                                                    ObjectAttributes = new List<ObjectAttribute>(),
                                                };
                                                temp_OrderItem.ObjectAttributes.Clear();
                                                temp_OrderItem.OrderItemGuid = Guid.NewGuid();
                                                if (x.ObjectAttributes != null)
                                                {
                                                    for (int i = 0; i < x.ObjectAttributes.Count; i++)
                                                    {
                                                        var item = x.ObjectAttributes[i];
                                                        if (!item.AttributeCode.ToLower().Equals("imageid"))
                                                            continue;
                                                        Guid oAttriD = Guid.NewGuid();
                                                        ObjectAttribute newAttribute = new ObjectAttribute(AppId)
                                                        {
                                                            ObjectAttributeId = oAttriD,
                                                            ID = oAttriD,
                                                            AttributeCode = item.AttributeCode,
                                                            AttributeId = item.AttributeId,
                                                            AttributeID = item.AttributeID
                                                        };
                                                        if (item.ObjectAttributeValues != null && item.ObjectAttributeValues.Count > 0)
                                                        {
                                                            ObjectAttributeValue oavAttribute = new ObjectAttributeValue(AppId);
                                                            oavAttribute.ObjectAttributeValueId = Guid.NewGuid();
                                                            oavAttribute.ID = oavAttribute.ObjectAttributeValueId;
                                                            oavAttribute.ObjectAttributeID = newAttribute.ObjectAttributeId;
                                                            oavAttribute.Value = tempImageId.ToString();
                                                            newAttribute.ObjectAttributeValues.Clear();
                                                            newAttribute.ObjectAttributeValues.Add(oavAttribute);
                                                        }
                                                        temp_OrderItem.ObjectAttributes.Remove(item);
                                                        temp_OrderItem.ObjectAttributes.Add(newAttribute);
                                                        filteredItems.Add(temp_OrderItem);
                                                    }
                                                }
                                            });
                                        }
                                    }
                                }
                            }
                        });
                    }
                    #endregion
                    filteredItems.ForEach((x) =>
                    {
                        if (x.ObjectAttributes != null)
                        {
                            var imageAttribute = x.ObjectAttributes.Where(c => c.AttributeCode.ToLower().Equals("imageid")).FirstOrDefault();
                            if (imageAttribute != null && (imageAttribute.ObjectAttributeValues != null && imageAttribute.ObjectAttributeValues.Count > 0))
                            {
                                Image objImage = imageAttribute.ObjectAttributeValues[0].Value as Image;
                                Guid tImgID = Guid.Empty;
                                if (objImage == null)
                                {
                                    tImgID = new Guid(imageAttribute.ObjectAttributeValues[0].Value.ToString());
                                }
                                else
                                {
                                    tImgID = objImage.ImageId;
                                }
                                if ((allNonRetouchImages.Where(v => v.ImageId == tImgID).Count() == 0))
                                {
                                    allNonRetouchImages.Add(new RetouchedImageInfo
                                    {
                                        ImageId = tImgID,
                                        OrderId = orderObj.OrderId,
                                        orderItem = x //it is order item
                                        
                                    });
                                    allItemsWithImages.Add(new RetouchedImageInfo
                                    {
                                        ImageId = tImgID,
                                        OrderId = orderObj.OrderId,
                                        orderItem = x //it is order item
                                    });
                                }
                            }
                        }
                    });
                    //Step 2: Get all retouch product with image
                    var all_retouchProduct = (from t in orderObj.OrderItems where RetouchOptionsList.Contains(t.ProductId) select t).ToList();
                    all_retouchProduct.ForEach((x) =>
                    {
                        if (x.ObjectAttributes != null)
                        {
                            var imageAttribute = x.ObjectAttributes.Where(c => c.AttributeCode.ToLower().Equals("imageid")).FirstOrDefault();
                            if (imageAttribute != null && (imageAttribute.ObjectAttributeValues != null && imageAttribute.ObjectAttributeValues.Count > 0))
                            {
                                Image objImage = imageAttribute.ObjectAttributeValues[0].Value as Image;
                                Guid tImgID = Guid.Empty;
                                if (objImage == null)
                                {
                                    tImgID = new Guid(imageAttribute.ObjectAttributeValues[0].Value.ToString());
                                }
                                else
                                {
                                    tImgID = objImage.ImageId;
                                }
                                if ((allRetouchImages.Where(v => v.ImageId == tImgID).Count() == 0))
                                {
                                    allRetouchImages.Add(new RetouchedImageInfo
                                    {
                                        ImageId = tImgID,
                                        OrderId = orderObj.OrderId,
                                        orderItem = x //it is order item
                                    });
                                    allItemsWithImages.Add(new RetouchedImageInfo
                                    {
                                        ImageId = tImgID,
                                        OrderId = orderObj.OrderId,
                                        orderItem = x //it is order item
                                    });
                                }
                            }
                        }
                    });
                    
                    //Step 3: Gell all images 
                    var objImageAtt = (from t in filteredItems
                                       where t.ObjectAttributes != null
                                       select t.ObjectAttributes.Where(x => x.AttributeCode.ToLower() == "imageid").FirstOrDefault()).ToList();
                    
                    //Step 3: Get image id aginst the digtal order has been placed
                    Guid imgID = Guid.Empty;
                    InsertLog(DateTime.Now + " : Number of images to link for order number " + OrderNumber + " is " + objImageAtt.Count);
                    objImageAtt.ForEach((c) =>
                    {
                        if (c.ObjectAttributeValues != null && c.ObjectAttributeValues.Count > 0)
                        {

                            Image objImage = c.ObjectAttributeValues[0].Value as Image;
                            if (objImage == null)
                            {
                                imgID = new Guid(c.ObjectAttributeValues[0].Value.ToString());
                                // = imgID;
                            }
                            else
                            {
                                imgID = objImage.ImageId;
                            }
                            if (imgID != Guid.Empty /*&& !imageIds.Contains(imgID)*/)
                            {
                                try
                                {
                                    RetouchedImageInfo objRetouchImage = null;
                                    RetouchedImageInfo objNonRetouchImage = null;
                                    string Path = Images.GetEncryptedURLWithParameters(AppId, imgID, 4);
                                    InsertLog(DateTime.Now + " : Getting Image Data for order number " + OrderNumber + " and image ID : " + imgID);
                                    Image img = Images.Get(AppId, imgID);
                                    byte[] imageBytes = null;
                                    bool HasRetouched = false;
                                  
                                    if (allRetouchImages.Where(x => x.ImageId == imgID).Count() == 0 && img.OriginalAvailable == true)//if it is a retouch image
                                    {

                                        imageBytes = GetImageData(Path);
                                        objNonRetouchImage = allNonRetouchImages.Where(x => x.ImageId == imgID).FirstOrDefault();
                                        HasRetouched = false;
                                    }
                                    else if (allRetouchImages.Where(x => x.ImageId == imgID).Count() > 0 && img.OriginalAvailable == true) // if image has to be retouched
                                    {
                                        /*For Retouched image no needs to be get byte data because they are manually uploaded from admin digital order detail page.*/

                                        //if (img.OriginalAvailable)
                                        //    Path = Images.GetEncryptedURLWithParameters(SiteWeb.AppId, imgID, 4);//get original image byte[]
                                        //else
                                        //    Path = Images.GetEncryptedURLWithParameters(SiteWeb.AppId, imgID, 2);// get mid image byte[]
                                        //imageBytes = GetImageData(Path);
                                        objRetouchImage = allRetouchImages.Where(x => x.ImageId == imgID).FirstOrDefault();
                                        HasRetouched = true;
                                    }
                                    UploadImageToS3 uplFile = new UploadImageToS3();
                                    uplFile.BucketName = BucketName;
                                    uplFile.ImageExt = img.Extension;
                                    uplFile.ImageSize = img.Size;
                                    uplFile.FolderName = orderObj.OrderId.ToString();
                                    uplFile.ImageName = img.Name;
                                    uplFile.ImageId = imgID;
                                    uplFile.UploadedDate = DateTime.Now;                                    
                                    var S3UploadedImgDetails = uplFile.GetImageDetailsOfS3(BucketName, orderObj.OrderId.ToString()).Where(p => p.ImageId == imgID && p.FolderName == orderObj.OrderId.ToString() && p.ProductId == (HasRetouched? objRetouchImage.orderItem.ProductId : objNonRetouchImage.orderItem.ProductId)).FirstOrDefault();
                                    if (S3UploadedImgDetails == null)
                                    {
                                        if (imageBytes != null && imageBytes.Length > 0 && HasRetouched == false)
                                        {
                                            InsertLog(DateTime.Now + " : Auto linking image for order number " + OrderNumber + " and image : " + img.Name + " and ID : " + imgID);
                                            #region SaveFileRefrence
                                            byte[] objData = imageBytes;
                                            uplFile.FileContent = objData;
                                            uplFile.CanDownload = true;// if it is not be retouched then  we can set download imge permission
                                            InsertLog(DateTime.Now + " : Processing Iamge on bucket " + OrderNumber + " and image : " + img.Name + " and ID : " + imgID);
                                            bool digiFileStatus = uplFile.DoProcessing(uplFile);
                                            if (digiFileStatus == true && uploadedDigiFiles == false)
                                                uploadedDigiFiles = true;
                                            #endregion
                                            string itemCode = string.Empty;
                                            Guid productId = Guid.Empty;
                                            int noOfPose = 1;
                                            Guid orderItemId = Guid.Empty;
                                            if (objNonRetouchImage != null)
                                            {
                                                orderItemId = objNonRetouchImage.orderItem != null ? objNonRetouchImage.orderItem.OrderItemGuid : Guid.Empty;
                                                itemCode = objNonRetouchImage.orderItem != null ? objNonRetouchImage.orderItem.ItemCode : string.Empty;
                                                productId = objNonRetouchImage.orderItem != null ? objNonRetouchImage.orderItem.ProductId : Guid.Empty;
                                                noOfPose = objNonRetouchImage.orderItem.ObjectAttributes != null ? GetNoOfPose(objNonRetouchImage.orderItem.ObjectAttributes) : 1;
                                            }
                                            uplFile.DigitalFileNeedsToBeRetouched(orderObj.OrderId, img.ImageId, img.OriginalFileName, HasRetouched, userName, "Auto Link", itemCode, productId, noOfPose, orderItemId, Path);
                                            InsertLog(DateTime.Now + " : Auto linking image complete for order number " + OrderNumber + " and image : " + img.Name + " and ID : " + imgID + " and added to Retouch History table");
                                        }
                                        else if (HasRetouched == true)
                                        {
                                            uplFile.CanDownload = false;// if it is not be retouched then  we can set download imge permission
                                            string itemCode = string.Empty;
                                            Guid productId = Guid.Empty;
                                            int noOfPose = 1; Guid orderItemId = Guid.Empty;
                                            if (objRetouchImage != null)
                                            {
                                                orderItemId = objRetouchImage.orderItem != null ? objRetouchImage.orderItem.OrderItemGuid : Guid.Empty;
                                                itemCode = objRetouchImage.orderItem != null ? objRetouchImage.orderItem.ItemCode : string.Empty;
                                                productId = objRetouchImage.orderItem != null ? objRetouchImage.orderItem.ProductId : Guid.Empty;
                                                noOfPose = objRetouchImage.orderItem.ObjectAttributes != null ? GetNoOfPose(objRetouchImage.orderItem.ObjectAttributes) : 1;
                                            }
                                            InsertLog(DateTime.Now + " : Image with retouch found for order number " + OrderNumber + " and image : " + img.Name + " and ID : " + imgID + " and added to Retouch History table");
                                            //bool digiFileStatus = uplFile.SaveImageDetailForS3(uplFile);
                                            //if (digiFileStatus == true)
                                                uplFile.DigitalFileNeedsToBeRetouched(orderObj.OrderId, img.ImageId, img.OriginalFileName, HasRetouched, userName, "Image needs to be retouched", itemCode, productId, noOfPose, orderItemId, string.Empty);
                                        }
                                        else
                                        {
                                            if (imageBytes != null)
                                            {
                                                uplFile.FileContent = imageBytes;
                                                uplFile.CanDownload = true;
                                            }
                                            string itemCode = string.Empty;
                                            Guid productId = Guid.Empty;
                                            int noOfPose = 1; Guid orderItemId = Guid.Empty;
                                            var allImg = allItemsWithImages.Where(x => x.ImageId == imgID).FirstOrDefault();
                                            if (allImg != null)
                                            {
                                                orderItemId = allImg.orderItem != null ? allImg.orderItem.OrderItemGuid : Guid.Empty;
                                                itemCode = allImg.orderItem != null ? allImg.orderItem.ItemCode : string.Empty;
                                                productId = allImg.orderItem != null ? allImg.orderItem.ProductId : Guid.Empty;
                                                noOfPose = allImg.orderItem.ObjectAttributes != null ? GetNoOfPose(allImg.orderItem.ObjectAttributes) : 1;
                                            }
                                            InsertLog(DateTime.Now + " : Original Image not found for order number " + OrderNumber + " and image : " + img.Name + " and ID : " + imgID + " and added to Retouch History table");
                                            //bool digiFileStatus = uplFile.SaveImageDetailForS3(uplFile);
                                            //if (digiFileStatus == true)
                                                uplFile.DigitalFileNeedsToBeRetouched(orderObj.OrderId, img.ImageId, img.OriginalFileName, HasRetouched, userName, (HasRetouched == true ? "Image needs to be retouched" : (img.OriginalAvailable ? "" : "Original image not available")), itemCode, productId, noOfPose, orderItemId, string.Empty);

                                        }
                                        imageIds.Add(imgID);
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    });
                    if (uploadedDigiFiles == true)
                    {
                        Orders.UpdateDigiFileStatus(orderObj.OrderId, (int)PhotoApp.Web.Ecommerce.OrderStatus.Custom5, "Uploaded", "System");
                        LoadNotificationTemplate(orderObj, OrderNumber);
                        if (orderObj.Status == OrderStatus.NewOrder)
                        {
                            Orders.UpdateStatus(AppId, orderObj.OrderId, OrderStatus.DigitalDownloadOnly);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InsertLog("\nDate :" + DateTime.Now + " Error while updating digital file status "+ex.Message);
            }
            return uploadedDigiFiles;
        }

        private static List<OrderItem> ListAllImageDownloadOrderItems(List<OrderItem> filteredItems)
        {
            List<OrderItem> HQ_All_ImagesDownloadItems = new List<OrderItem>();
            filteredItems.ForEach(item=> {
                if (AllowAllImageDownload(item.ProductId))
                    HQ_All_ImagesDownloadItems.Add(item);
            });
            return HQ_All_ImagesDownloadItems;
        }

        private static bool AllowAllImageDownload(Guid productId)
        {
            bool allowedMultipleImages = false;
            var allImageDownloadAttributeCode = ConfigurationManager.AppSettings["Product_AllImageAttributeCode"];
            if (string.IsNullOrEmpty(allImageDownloadAttributeCode))
                return false;
            var productAttribute = PhotoApp.Web.Ecommerce.ObjectAttributes.GetObjectAttributesValues(AppId, productId, allImageDownloadAttributeCode);
            bool.TryParse(Convert.ToString(productAttribute), out allowedMultipleImages);
            return allowedMultipleImages;
        }

        private static byte[] GetImageData(string Path)
        {
            byte[] imageBytes = null;
            try
            {
                var webClient = new WebClient();
                imageBytes = webClient.DownloadData(Path);
            }
            catch
            {

            }
            return imageBytes;
        }
        private static void LoadNotificationTemplate(Order o, string OrderNumber)
        {
            Guid TemplateId = Guid.Empty;
            try
            {
                var orderFromEmailAddress = new MailAddress(ConfigurationManager.AppSettings["DigiOrderNotificationEmail"], ConfigurationManager.AppSettings["DigiOrderNotificationName"]);
                Guid.TryParse(Convert.ToString(ConfigurationManager.AppSettings["DigitalFileEmailTemplate"]), out TemplateId);
                int uploadedImgCount = Orders.GetS3UploadedImagesCount("customer", o.OrderId);

                if (TemplateId == Guid.Empty)
                    throw new Exception("Please add digital order template id");
                ApplicationEmail email = ApplicationEmails.Get(TemplateId);
                StringBuilder sb = new StringBuilder();
                sb.Append(email.BodyContent);
                string orderDetail = ConfigurationManager.AppSettings["FrontBaseURL"] + "/account/order.aspx?id=" + o.OrderId;
                sb.Replace("##OrderDetail##", orderDetail);
                if (uploadedImgCount > 0)
                {
                    sb.Replace("##Count##", Convert.ToString(uploadedImgCount));
                }
                else
                    sb.Replace("##Count##", string.Empty);
                sb.Replace("##OrderNumber##", OrderNumber);
                sb.Replace("##OrderDate##", o.Timestamp.ToString("MM/dd/yyyy"));
                sb.Replace("##OrderEmail##", o.Email);
                sb.Replace("##ordernumber##", OrderNumber);
                var orderEmail = new MailMessage();
                orderEmail.From = orderFromEmailAddress;
                orderEmail.To.Add(o.Email);
                orderEmail.Subject = email.Subject;
                orderEmail.IsBodyHtml = true;
                orderEmail.Body = sb.ToString();
                bool hasEmailSent = SendEmail(orderEmail);
            }
            catch (Exception ex)
            {
                InsertLog("\nDatetime:" + DateTime.Now + "\tError : Error when reading email template from system email for template id :=" + TemplateId + "\n" + ex.Message + " " + (ex.InnerException == null ? string.Empty : ex.InnerException.Message));
            }
        }
        private static int GetNoOfPose(List<ObjectAttribute> objAttr)
        {
            int noOfPose = 0;
            try
            {
                var noOfPoses = objAttr.Where(x => x.AttributeCode.ToLower().Equals("poses")).FirstOrDefault();
                if (noOfPoses != null && (noOfPoses.ObjectAttributeValues != null && noOfPoses.ObjectAttributeValues.Count > 0))
                {
                    var objVal = noOfPoses.ObjectAttributeValues[0].Value;
                    if (objVal != null)
                        int.TryParse(Convert.ToString(objVal), out noOfPose);
                }
            }
            catch (Exception ex)
            {
                InsertLog("\nGetNoOfPose : Error :" + ex.Message);
            }
            return noOfPose == 0 ? 1 : noOfPose;
        }

        public static Boolean SendEmail(MailMessage Message)
        {
            Application App = Applications.Get(AppId);


            Message.From = App.MailServer.SMTPFromAddress != String.Empty ? new MailAddress(App.MailServer.SMTPFromAddress, "MugsyClicks Web Site") : Message.ReplyToList[0];

            SmtpClient client = new SmtpClient();

            client.Host = App.MailServer.SMTPServer;

            if (App.MailServer.SMTPUsername != String.Empty && App.MailServer.SMTPPassword != String.Empty)
            {
                client.Credentials = new NetworkCredential(App.MailServer.SMTPUsername, App.MailServer.SMTPPassword);
            }

            client.Port = App.MailServer.SMTPPort;
            client.EnableSsl = App.MailServer.SMTPEnableSSL;

            try
            {
                client.Send(Message);
            }
            catch (Exception error)
            {
                // sofmen change 05/24/2013 : Pronob Mukharjee
                if (error.Message == "The operation has timed out.")
                {
                    // bypassing timeout isse as email will be sent successfully in background
                    return true;
                }
                /*
                 * if there is an error sending through the provider and the provider is not localhost
                 * then try to send through smtp settings in the web.config
                 */
                if (App.MailServer.SMTPServer != "localhost")
                {
                    client = new SmtpClient();

                    client.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["UseSSL"]);

                    try
                    {
                        client.Send(Message);
                    }
                    catch (Exception error2)
                    {
                        Debug.SendDebugEmail(App.ApplicationId,
                                                                 "Error Sending Email using NPCommerce.Application and web.config settings",
                                                                 "NPCommerce.Application Error Message:<br />" + error.Message +
                                                                 "<br /><br /><br />Web.Config Error Message:" + error2.Message);

                    }
                    finally
                    {
                        Debug.SendDebugEmail(App.ApplicationId, "Error Sending Email using NPCommerce.Application",
                                                                 "Error Message:<br />" + error.Message);
                    }
                }

                return false;
            }

            return true;
        }

    }

    public class RetouchedImageInfo
    {
        public Guid OrderId { get; set; }
        public OrderItem orderItem { get; set; }
        public Guid ImageId { get; set; }
    }
    public static class OrderNumberlist
    {
        private static List<Guid> _OrderNumberList = new List<Guid>();
        public static bool Add_OrderId(Guid OrderNumber)
        {
            if (!_OrderNumberList.Contains(OrderNumber))
            {
                _OrderNumberList.Add(OrderNumber);
                return true;
            }
            return false;
        }

        public static void Remove_OrderId(Guid OrderNumber)
        {
            if (_OrderNumberList != null)
            {
                if (_OrderNumberList.Contains(OrderNumber))
                    _OrderNumberList.Remove(OrderNumber);
            }
        }
    }
}
