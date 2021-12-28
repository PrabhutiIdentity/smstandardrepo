using Razorpay.Api;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMEnterprise.Controllers
{
    public class PaymentController : Controller
    {
        // GET: Payment
        public ActionResult Index(OrderModel oModel)
        {
            int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            AccountData Accountdata = new AccountData();
            StudentOnlineFeeDetailModel model = Accountdata.GetStudentDetailsForPayment(StudentID, SBranchID);
            model.FeeMonth = oModel.FeeMonth;
            model.ApplicableFee = oModel.ApplicableFee;
            model.FeeYear = oModel.FeeYear;
            OrderModel om = new OrderModel();
            om.FeeMonth = oModel.FeeMonth;
            om.FeeYear = oModel.FeeYear;
            om.ApplicableFee = oModel.ApplicableFee;
            om.StudentID = oModel.StudentID;
            TempData["OrderDetails"] = om;

            OnlinePaymentModel _Payment = new OnlinePaymentModel();

            _Payment.Amount = Convert.ToInt32(oModel.ApplicableFee);
            _Payment.ContactNumber = model.FatherMobileNo;
            _Payment.Name = model.Name;
            string transactionId = Guid.NewGuid().ToString();

            //RazorpayClient client = new RazorpayClient("rzp_live_FBGoyHexVX5cYl", "HGaoC579R4jHOdinBRwRJoVK");
            RazorpayClient client = new RazorpayClient("rzp_test_TQjMHwtFMijJsT", "Untl4scpcYUcEysNp6S7BCoD");

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", _Payment.Amount * 100);  // Amount will in paise
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
                //razorpayKey = "rzp_live_FBGoyHexVX5cYl",
                razorpayKey = "rzp_test_TQjMHwtFMijJsT",
                Amount = _Payment.Amount * 100,
                currency = "INR",
                OrderID = StudentID.ToString(),
                Name = _Payment.Name,
                EmailID = _Payment.EmailID,
                ContactNumber = _Payment.ContactNumber,
                Address = _Payment.Address,
                Description = "School Name",
                FeeMonth = oModel.FeeMonth,
                FeeYear = oModel.FeeYear,
                ApplicableFee = oModel.ApplicableFee,
                StudentID = oModel.StudentID

            };
            orderModel.Date = SMEnterprise.Repository.CommonUsage.GetCurrentDate();
            Accountdata.InsertOrderID(orderModel);
            ViewBag.OrderDetails = orderModel;

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

    }
}