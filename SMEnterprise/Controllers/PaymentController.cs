
using Razorpay.Api;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
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
        AccountData accountData = new AccountData();
        private readonly string _razorpayKeyId = "rzp_test_xsUBcEJ9m0np5p"; 
        private readonly string _razorpaySecret = "aCtoOu3y7WibjJ4j3ckwV04V";
      
        public ActionResult Index(OrderModel oModel)
        {
            string transactionId = Guid.NewGuid().ToString();
            int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
           
            StudentModel studentmodel = accountData.GetStudentDetailsForPayment(StudentID, SBranchID);
           

            OnlinePaymentModel _Payment = new OnlinePaymentModel();

            _Payment.Amount = Convert.ToInt32(oModel.ApplicableFee*100);// Converted to paise
            _Payment.ContactNumber = studentmodel.FatherMobileNo;
            _Payment.Name = studentmodel.Name;
            _Payment.EmailID = string.IsNullOrEmpty(studentmodel.EmailID) ? "cmps2016@gmail.com" : studentmodel.EmailID;

                        
            RazorpayClient client = new RazorpayClient(_razorpayKeyId, _razorpaySecret);
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", _Payment.Amount);  // Amount will in paise         
            options.Add("receipt", transactionId);
            options.Add("currency", "INR");
            options.Add("payment_capture", "0"); // 1 - automatic  , 2 - manual
                                                 //options.Add("notes", "-- You can put any notes here --");

             Order order = client.Order.Create(options);
            Order orderResponse = client.Order.Create(options);
            string RazorpayOrderID = orderResponse["id"].ToString();

            // Create order model for return on view
            OrderModel orderModel = new OrderModel
            {
                PGOrderID = transactionId.ToString(),

                razorpayKey = _razorpayKeyId,
                Amount = _Payment.Amount,
                currency = "INR",
                OrderID = RazorpayOrderID,
                Name = _Payment.Name,
                EmailID = _Payment.EmailID,
                ContactNumber = _Payment.ContactNumber,
                Address = _Payment.Address,
                Description = "Chamba School Name",
                FeeMonth = oModel.FeeMonth,
                FeeYear = oModel.FeeYear,
                ApplicableFee = oModel.ApplicableFee,
                StudentID = oModel.StudentID,
                SessionID = studentmodel.SessionID,
                SBranchID = SBranchID,

            };
           


            orderModel.Date = SMEnterprise.Repository.CommonUsage.GetCurrentDate();
            accountData.InsertOrderID(orderModel);
            ViewBag.OrderDetails = orderModel;
            TempData["OrderDetails"] = orderModel;


            return View(studentmodel);
        }
      
        public ActionResult CreatePayment(Models.OnlinePaymentModel _Payment)
        {
            // Generate random receipt number for order
            Random randomObj = new Random();
            string transactionId = Guid.NewGuid().ToString();           
           // Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_test_xsUBcEJ9m0np5p", "aCtoOu3y7WibjJ4j3ckwV04V");
            RazorpayClient client = new RazorpayClient(_razorpayKeyId, _razorpaySecret);
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", _Payment.Amount);  // Amount will in paise
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
                razorpayKey = _razorpayKeyId,
                Amount = _Payment.Amount * 100,
                currency = "INR",
                OrderID = _Payment.OrderID,
                Name = _Payment.Name,
                EmailID = _Payment.EmailID,
                ContactNumber = _Payment.ContactNumber,
                Address = _Payment.Address,
                Description = "Chamba School Name",
                
            };
            orderModel.Date = CommonUsage.GetCurrentDate();
            AccountData accountData = new AccountData();
            accountData.InsertOrderID(orderModel);
            // Return on PaymentPage with Order data
            return View("PaymentPage", orderModel);
        }

        [HttpPost]
        public ActionResult PaymentCallback(OrderModel objModel)
        {
            string paymentId = Request.Form["razorpay_payment_id"];
            string OrderID = Request.Form["razorpay_order_id"];
            objModel = accountData.GetOrderForPayment(OrderID);
            RazorpayClient client = new RazorpayClient(_razorpayKeyId, _razorpaySecret);
           

            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes.Add("razorpay_payment_id", paymentId);
            attributes.Add("razorpay_order_id", Request.Form["razorpay_order_id"]);
            attributes.Add("razorpay_signature", Request.Form["razorpay_signature"]);
            Utils.verifyPaymentSignature(attributes);

         
            Razorpay.Api.Payment payment = client.Payment.Fetch(paymentId);

            // This code is for capture the payment
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", payment.Attributes["amount"]);
            options.Add("currency", "INR");
            Razorpay.Api.Payment paymentCaptured = payment.Capture(options);
            string amt = paymentCaptured.Attributes["amount"];
            //  int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            
                
            if (paymentCaptured.Attributes["status"] == "captured")
            {
                OrderModel om = new OrderModel();
                FeePaymentModel FeeModel = new FeePaymentModel();
                om.PGOrderID = paymentId;
                om.PGPaymentID = paymentId;
                om.Status = 1;
                om.FeeMonth = objModel.FeeMonth;
                om.FeeYear = objModel.FeeYear;
                //om.ApplicableFee = objModel.ApplicableFee;
                om.PaymentAmount = Convert.ToDecimal(paymentCaptured.Attributes["amount"]) / 100;
                om.StudentID = objModel.StudentID;
                om.SBranchID = objModel.SBranchID;
                om.PaymentMode = 3;
                om.QDate = CommonUsage.GetCurrentDate();
                om.CurDate = CommonUsage.GetCurrentDate();
                om.PaymentDate = CommonUsage.GetCurrentDate();

                FeeModel.SBranchID = objModel.SBranchID;
                FeeModel.StudentID = objModel.StudentID;
                FeeModel.SessionID = objModel.SessionID;
                FeeModel.FeeAmount = objModel.ApplicableFee;
                FeeModel.FeePaymentMode = 3;
                FeeModel.PaymentAmount = Convert.ToDecimal(paymentCaptured.Attributes["amount"]) / 100;
                FeeModel.ReferanceNumber = paymentId;
                FeeModel.Remark = "Paid by PaymentGateway";
                FeeModel.Month = objModel.FeeMonth;
                FeeModel.Year = objModel.FeeYear;

                accountData.UpdateOrderStatus(paymentId, objModel.StudentID.ToString(), objModel.SessionID.ToString(), paymentId);
                FeePaymentRowModel objData = accountData.SaveStudentFeePaymentOnline(FeeModel);
               
                return RedirectToAction("Success", new { ID = paymentId });
               
            }
            else
            {
                return RedirectToAction("Failed");
            }
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
        
            //  Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_test_xsUBcEJ9m0np5p", "aCtoOu3y7WibjJ4j3ckwV04V");
            RazorpayClient client = new RazorpayClient(_razorpayKeyId, _razorpaySecret);
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
                FeePaymentModel FeeModel = new FeePaymentModel();
                om.PGOrderID = orderId;
                om.PGPaymentID = paymentId;
                om.Status = 1;
                om.FeeMonth = objModel.FeeMonth;
                om.FeeYear = objModel.FeeYear;
                //om.ApplicableFee = objModel.ApplicableFee;
                om.PaymentAmount = Convert.ToDecimal(paymentCaptured.Attributes["amount"]) / 100;
                om.StudentID = objModel.StudentID;
                om.SBranchID = SBranchID;
                om.PaymentMode = 3;
                om.QDate = CommonUsage.GetCurrentDate();
                om.CurDate = CommonUsage.GetCurrentDate();
                om.PaymentDate = CommonUsage.GetCurrentDate();

                FeeModel.SBranchID = SBranchID;
                FeeModel.StudentID = objModel.StudentID; 
                FeeModel.SessionID = objModel.SessionID;
                FeeModel.FeeAmount = objModel.ApplicableFee;
                FeeModel.FeePaymentMode = 3;
                FeeModel.PaymentAmount = Convert.ToDecimal(paymentCaptured.Attributes["amount"]) / 100;
                FeeModel.ReferanceNumber = paymentId;
                FeeModel.Remark = "Paid by PaymentGateway";
                FeeModel.Month = objModel.FeeMonth; 
                FeeModel.Year = objModel.FeeYear; 
              
                accountData.UpdateOrderStatus(paymentId, objModel.StudentID.ToString(), objModel.SessionID.ToString(), orderId);
                FeePaymentRowModel objData = accountData.SaveStudentFeePaymentOnline(FeeModel);
                // accountData.UpdateStudentFeePaymentStatus(om);
                  return RedirectToAction("Success", new { ID = orderId });
               // return View("Parent/FeeSummery");
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

        public ActionResult PaymentFailure(FormCollection form)
        {

            return View("Failed");
        }
       

    }
}