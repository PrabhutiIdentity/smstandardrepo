using Razorpay.Api;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace SMEnterprise.Controllers
{
    public class PaymentController : Controller
    {
        // GET: Payment
        // GET: Payment
        public ActionResult Index(OrderModel oModel)
        {
            string transactionId = Guid.NewGuid().ToString();
            int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            AccountData Accountdata = new AccountData();
            StudentModel model = Accountdata.GetStudentDetailsForPayment(StudentID, SBranchID);
            OrderModel om = new OrderModel();
            om.FeeMonth = oModel.FeeMonth;
            om.FeeYear = oModel.FeeYear;
            om.ApplicableFee = oModel.ApplicableFee;
            om.StudentID = oModel.StudentID;
            om.SBranchID = oModel.SBranchID;
            TempData["OrderDetails"] = om;

            OnlinePaymentModel _Payment = new OnlinePaymentModel();

            _Payment.Amount = Convert.ToInt32(oModel.ApplicableFee);
            _Payment.ContactNumber = model.FatherMobileNo;
            _Payment.Name = model.Name;
            _Payment.EmailID = string.IsNullOrEmpty(model.EmailID) ? "info@prabhutisystems.com" : model.EmailID;

            
            //RazorpayClient client = new RazorpayClient("rzp_live_FBGoyHexVX5cYl", "HGaoC579R4jHOdinBRwRJoVK");
            RazorpayClient client = new RazorpayClient("rzp_test_TQjMHwtFMijJsT", "Untl4scpcYUcEysNp6S7BCoD");

            Dictionary<string, object> options = new Dictionary<string, object>();
            //options.Add("amount", _Payment.Amount * 100);  // Amount will in paise
            options.Add("amount", _Payment.Amount);  
            options.Add("receipt", transactionId);
            options.Add("currency", "INR");
            options.Add("payment_capture", "0"); // 1 - automatic  , 2 - manual
                                                 //options.Add("notes", "-- You can put any notes here --");

            Order orderResponse = client.Order.Create(options);
            string OrderID = orderResponse["id"].ToString();

            // Create order model for return on view
            OrderModel orderModel = new OrderModel
            {
                PGOrderID = orderResponse.Attributes["id"],
              
             
                Amount = _Payment.Amount * 100,
                currency = "INR",
                OrderID = transactionId.ToString(),
                Name = _Payment.Name,               
                EmailID = _Payment.EmailID,
                ContactNumber = _Payment.ContactNumber,
                Address = _Payment.Address,
                Description = "School Name",
                FeeMonth = oModel.FeeMonth,

                FeeYear = oModel.FeeYear,
                ApplicableFee = oModel.ApplicableFee,
                StudentID = oModel.StudentID,
               SBranchID=oModel.SBranchID

            };
            orderModel.Date = SMEnterprise.Repository.CommonUsage.GetCurrentDate();
            Accountdata.InsertOrderID(orderModel);
            ViewBag.OrderDetails = orderModel;

            #region
            // Payu Payment Gateway

            string amount1 = oModel.ApplicableFee.ToString("0.00"); // Format as string with two decimal points
            string email1 = string.IsNullOrEmpty(model.EmailID) ? "info@prabhutisystems.com" : model.EmailID;

            string txnid = transactionId.ToString(); 
            string key = "JVDkAA"; // Your PayU key
            string salt = "MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQC0PewBdO/B+hMc2kbweqBI/0iIBm1FPeSn7wLIQIEQE7jv27XdrFSOt+fDr6Zbp8IZJTUj3eqDw4G6YbQeAfkYy+T/2orcBxsIUbArvWt1QN5GDzP0+v26Mes2fbjCM7sJXYCa4fwYmu4LPDG1X2R0P0Sw3Cjbt1nZRhhZLRw/MrFfJvyyklHU4sP2NgmQxqzdGxGROK3eG3tPMP0nJfVnfFy7VPrM0U11ytJCGalaAiYryeoxXArk/d3zjS9yEXBiRKfQDPFoouoN9PpIGYEw9eLzog2+S9De1cNi3PErBUaiCxlTXF9Bvwc+0gG1t1tBXzo9B1mdjBYnWqZyuWRlAgMBAAECggEAL9tVI5nh4xY8JugJ9+YHUvp9TqiuMLlbUf+TNIpz+knSS9WBd2c/WFmaLwofWKy1S6nmbyyIDQ5HDq7MfT823Y+dsRVc00kLs2zFIwf7VTmxCtcUi5Js9hg1BNXizaWgnr7126nvHrVcSR1lcWvvpzH5UiBStOv87kS/SEZC/sBLnN4Y0Gj51NThj/yB2cRppf51cPd/oCOAfhwZUvFSecQVWESRf2GB0dPnwjGAyB1qFKzICm/o8kqQ6ZyhmssnLuAf3AXXsVSyoZHrDFwZjLtHXA3Lhnqm/ZseCAaj9+dG/V+MRk48HNDV8TWl5JVUTYtDenNMTWm+i+mIysD/jQKBgQDmy9XkjqcELJ6Ss6VKl5MBzHotaJgF9ek3zh1eFmYtRU5nHyqs9bd8htTCcCSNP+yVlQyUJMdTHoUQL7FlK2Nf9Tba0Y33GEih8hwIbmAfhhe7kf6fiA/YDuUmIMuI7d2RDwMSROtdMDi8lQWpnXWJ6WXOWasebt+GUuIqOv7PAwKBgQDH7MiS0IVkI8ax6qlRLN2kaMPp49nNcTDTzaDN2SxVf3V1BhOmvo++M00w71kIXUFjbBtZRI9fMU0qY0/N383MQEgIsUNxfdm7BMk1brGSJTLr/+uc36cYqNWYE9Gldh2W0S5IjjS2hcrs/hpUcVz6NwL0UJlCZTXDwMFyBKcOdwKBgBqmzzxlfMPuoyEtvZiviDpc8n7r5SJLE3NuSdXjoEj6B+PApZzzgwzORSNu78mf6CId5CX3WU0v6Q3FdCbKq98Y4gzCxjISi9CntLUEifUZ9wOiCFVD0RCSJ8QPZXGahkuKAEDL1KIeP28Hhm/fzwLuAOOjVy9cflN0nslhpqshAoGAGZdYsVpZyNC/jWSxWb+5e2MhuN2+soqLqoEG1XK4NxWCroEhoNWBxuIAPoRUzDpLtXNQKOQayh+gdg2SWJMOX9fWoK44KN3oMgVR0DIkLsXuN8FhoooKdKCf/sCCtIBjFzwdhZIWc0q7CA0ax7ZfJXRHP4jVpWcZM76HxJWN4M8CgYBEKYCzLpx2SU0rjsBQAvqGIj/JxTFB4u9qjLEsUiz3WyAidKzOyBwlu00+ZK/mCwlNEVCcG9g0B2xag+sPe3CCF9jomQE14BWiCp1or6G8m1NLGrQSEUy2XAToGwff9XatpTZc6DsRBn7PFUFaxNGM+UcDWYxHWA+JZ2sToYIUhg=="; // Your PayU salt
           // string productinfo = "Fee Payment for " + model.Name + "student ID" + model.StudentID;
            string productinfo = "Fee Payment";
            string amount = amount1;
            string firstname = model.Name;
             string lastname = model.StudentID.ToString();
            string email = email1;
            // Prepare empty or default values for UDFs and other required parameters
           
          //  string phone = model.FatherMobileNo; // Student or guardian phone number



           

            string hash = GenerateHash(key,txnid, amount1, "Fee Payment", model.Name, email1, salt );
          
            ViewBag.TransactionId = txnid;
            ViewBag.PaymentHash = hash; // Passing the generated hash to ViewBag

            #endregion

            return View(model);
        }
      
        public ActionResult CreatePayment(Models.OnlinePaymentModel _Payment)
        {
            // Generate random receipt number for order
            Random randomObj = new Random();
            string transactionId = Guid.NewGuid().ToString();
            //Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_live_FBGoyHexVX5cYl", "HGaoC579R4jHOdinBRwRJoVK");
            Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_test_TQjMHwtFMijJsT", "Untl4scpcYUcEysNp6S7BCoD");

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", _Payment.Amount * 100);  // Amount will in paise
            options.Add("receipt", transactionId);
            options.Add("currency", "INR");
            options.Add("payment_capture", "0"); // 1 - automatic  , 2 - manual
                                                 //options.Add("notes", "-- You can put any notes here --");
            Razorpay.Api.Order orderResponse = client.Order.Create(options);
            string OrderID = orderResponse["id"].ToString();

            // Create order model for return on view
            OrderModel orderModel = new OrderModel
            {
                PGOrderID = orderResponse.Attributes["id"],
                //razorpayKey = "rzp_live_FBGoyHexVX5cYl",
                razorpayKey = "rzp_test_TQjMHwtFMijJsT",
                Amount = _Payment.Amount * 100,
                currency = "INR",
                OrderID = _Payment.OrderID,
                Name = _Payment.Name,
                EmailID = _Payment.EmailID,
                ContactNumber = _Payment.ContactNumber,
                Address = _Payment.Address,
                Description = "School Name"
            };
            orderModel.Date = CommonUsage.GetCurrentDate();
            AccountData accountData = new AccountData();
            accountData.InsertOrderID(orderModel);
            // Return on PaymentPage with Order data
            return View("PaymentPage", orderModel);
        }

        [HttpPost]
        public ActionResult Complete(OrderModel objModel)
        {
            // Payment data comes in url so we have to get it from url
            if (TempData["OrderDetails"] != null)
            {
                objModel = (OrderModel)TempData["OrderDetails"];
            }

            // This id is razorpay unique payment id which can be use to get the payment details from razorpay server
            string paymentId = Request.Form["rzp_paymentid"];

            // This is orderId
            string orderId = Request.Form["rzp_orderid"];

            //Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_live_CCauQAKB8bFLSZ", "DiQ1NJl5XEDBXDUwzdW2kDsa");
            //Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_live_FBGoyHexVX5cYl", "HGaoC579R4jHOdinBRwRJoVK");
            Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_test_TQjMHwtFMijJsT", "Untl4scpcYUcEysNp6S7BCoD");

            Razorpay.Api.Payment payment = client.Payment.Fetch(paymentId);

            // This code is for capture the payment
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", payment.Attributes["amount"]);
            Razorpay.Api.Payment paymentCaptured = payment.Capture(options);
            string amt = paymentCaptured.Attributes["amount"];
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            //// Check payment made successfully

            if (paymentCaptured.Attributes["status"] == "captured")
            {
                OrderModel om = new OrderModel();
                om.PGOrderID = orderId;
                om.PGPaymentID = paymentId;
                om.Status = 1;
                om.FeeMonth = objModel.FeeMonth;
                om.FeeYear = objModel.FeeYear;
                //om.ApplicableFee = objModel.ApplicableFee;
                om.PaymentAmount = objModel.ApplicableFee;
                om.StudentID = objModel.StudentID;
                om.SBranchID = SBranchID;
                om.PaymentMode = 3;
                om.QDate = CommonUsage.GetCurrentDate();
                om.CurDate = CommonUsage.GetCurrentDate();
                om.PaymentDate = CommonUsage.GetCurrentDate();

                AccountData accountData = new AccountData();
                accountData.UpdateStudentFeePaymentStatus(om);
                return RedirectToAction("Success", new { ID = orderId });

            }
            else
            {
                return RedirectToAction("Failed");
            }
        }
        public ActionResult Success(string ID = null)
        {
            ViewBag.orderID = ID;
            return View();
        }

        [HttpPost]
        public ActionResult ProcessPayment()
        {
            // Redirect to PayU's test environment
            return Redirect("https://test.payu.in/_payment");
        }
        [HttpPost]
        public ActionResult PaymentSuccess(FormCollection form)
        {
                   // Extract values from the FormCollection
            string transactionId = form["txnid"];
            string amount = form["amount"];
            string status = form["status"]; // Example: "success" or "failure"
            string email = form["email"];
            string firstName = form["firstname"];
            string orderId = form["orderId"]; // If you're passing an order ID, ensure it's included

            // Log or process the payment details
            if (status == "success")
            {
                // Update your order status in the database or perform any other necessary actions
                // Example:
                OrderModel om = new OrderModel();
                
                    om.PGOrderID = orderId;
                   // om.PGPaymentID = paymentId;
                    om.Status = 1;
                  //  om.FeeMonth = objModel.FeeMonth;
                   // om.FeeYear = objModel.FeeYear;
                    //om.ApplicableFee = objModel.ApplicableFee;
                  //  om.PaymentAmount = objModel.ApplicableFee;
                    //om.StudentID = objModel.StudentID;
                  //  om.SBranchID = SBranchID;
                    om.PaymentMode = 3;
                    om.QDate = CommonUsage.GetCurrentDate();
                    om.CurDate = CommonUsage.GetCurrentDate();
                    om.PaymentDate = CommonUsage.GetCurrentDate();

                    AccountData accountData = new AccountData();
                    accountData.UpdateStudentFeePaymentStatus(om);
               
                return RedirectToAction("Success", new { ID = orderId });
                
            }
            else
            {
                // Handle failure scenario
                // Log failure if necessary
            }

            // Prepare any additional data to send to the view
            ViewBag.TransactionId = transactionId;
            ViewBag.Amount = amount;
            ViewBag.Status = status;

            return View("PaymentSuccess"); // Redirect to a success view
        }

        // Method to update order status
        private void UpdateOrderStatus(string orderId, string status, string transactionId, string amount)
        {
            // Implement your logic to update the order status in the database
            // ...
        }

        public ActionResult PaymentFailure(FormCollection form)
        {
            
            return View("Failed");
        }
        //public static string GenerateHash(string key, string txnid, string amount, string productinfo, string firstname, string email, string udf1, string udf2, string udf3, string udf4, string udf5, string udf6, string udf7, string udf8, string udf9, string udf10, string user_token, string offer_key, string offer_auto_apply, string cart_details, string extra_charges, string phone, string salt)
        //{
        //    string input = $"{key}|{txnid}|{amount}|{productinfo}|{firstname}|{email}|{udf1}|{udf2}|{udf3}|{udf4}|{udf5}|{udf6}|{udf7}|{udf8}|{udf9}|{udf10}|{user_token}|{offer_key}|{offer_auto_apply}|{cart_details}|{extra_charges}|{phone}|{salt}";


        //    return Sha512(input);
        //}
        public static string GenerateHash(string key, string txnid, string amount, string productinfo, string firstname, string email, string salt)
        {
            string input = $"{key}|{txnid}|{amount}|{productinfo}|{firstname}|{email}|||||||||||{salt}";
            return Sha512(input);
        }
        private static string Sha512(string input)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}