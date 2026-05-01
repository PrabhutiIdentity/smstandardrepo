
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
using static Lucene.Net.Index.CheckIndex;

namespace SMEnterprise.Controllers
{
    public class PaymentController : Controller
    {
        AccountData accountData = new AccountData();
        //private readonly string _razorpayKeyId = "rzp_test_xsUBcEJ9m0np5p"; 
        //private readonly string _razorpaySecret = "aCtoOu3y7WibjJ4j3ckwV04V";
        private readonly string _razorpayKeyId = "rzp_live_RSpysm7E9l1iVG";
        private readonly string _razorpaySecret = "BU5GhnJ4xZfN2IOny0FeRbKL";

        public ActionResult Index1(OrderModel oModel)
        {
            string transactionId = Guid.NewGuid().ToString();
            //int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int StudentID = oModel.StudentID;
            

            StudentModel studentmodel = accountData.GetStudentDetailsForPayment(StudentID, SBranchID);

            StudentSessionFeeStatusPageModel oFeeModel = new StudentSessionFeeStatusPageModel();
            oFeeModel.StudentID = StudentID;
            accountData.GetStudentMonthWiseSessionFeeDetails(oFeeModel);

            // Find the applicable fee details based on the month and year from the form
            decimal applicableFee = oFeeModel.FeeWiseDetails.Where(c => c.FeeMonth == oModel.FeeMonth && c.FeeYear == oModel.FeeYear).Sum(x => x.ApplicableFee - x.RDiscount - x.PaidAmount);
            oModel.ApplicableFee = applicableFee;



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
                razorpaySecret = _razorpaySecret,
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

            //string hashString = orderModel.razorpayKey + "|" + orderModel.OrderID + "|" + orderModel.Amount + "|" + orderModel.Name + "|" + orderModel.EmailID + "|" + orderModel.Description + "|" + orderModel.StudentID + "|" + orderModel.FeeMonth + "|" + orderModel.FeeYear + "|" + orderModel.razorpaySecret;
           // orderModel.Hash = GenerateSha256(hashString);

            orderModel.Date = SMEnterprise.Repository.CommonUsage.GetCurrentDate();
            accountData.InsertOrderID(orderModel);
            ViewBag.OrderDetails = orderModel;
            TempData["OrderDetails"] = orderModel;


            return View(studentmodel);
        }

        public ActionResult Index(OrderModel oModel, string SelectedMonthsJson)
        {
            string transactionId = Guid.NewGuid().ToString();
            //int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int StudentID = oModel.StudentID;

            // 1. Get Student Details
            StudentModel studentmodel = accountData.GetStudentDetailsForPayment(StudentID, SBranchID);
            // 2. Get All Fee Details
            StudentSessionFeeStatusPageModel oFeeModel = new StudentSessionFeeStatusPageModel();
            oFeeModel.StudentID = StudentID;
            accountData.GetStudentMonthWiseSessionFeeDetails(oFeeModel);

            decimal totalApplicableFee = 0;
            List<FeeSelectionViewModel> selectedMonthsList = null;
            bool isMultiMonth = false;
            // 3. Logic for Multiple Months
            if (!string.IsNullOrEmpty(SelectedMonthsJson))
            {
                // Deserialize the JSON sent from the checkbox view
                selectedMonthsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FeeSelectionViewModel>>(SelectedMonthsJson);

                foreach (var item in selectedMonthsList)
                {
                    decimal monthDue = oFeeModel.FeeWiseDetails
                        .Where(c => c.FeeMonth == item.Month && c.FeeYear == item.Year)
                        .Sum(x => x.ApplicableFee - x.RDiscount - x.PaidAmount);

                    // ← Keep the validated server-side amount (don't trust client Amount)
                    item.Amount = monthDue;
                    totalApplicableFee += monthDue;
                }

                // Anchor FeeMonth/FeeYear to the earliest selected month
                /*  var first = selectedMonthsList.OrderBy(x => x.Year).ThenBy(x => x.Month).First();
                  oModel.FeeMonth = first.Month;                                   // ← CHANGED
                  oModel.FeeYear = first.Year;
                */
                // *** FIX: anchor to LATEST month, not first ***
                // Callback will use this as @QDate
                var latest = selectedMonthsList
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.Month)
                    .First();
                oModel.FeeMonth = latest.Month;   // 8 (August)  ← was 7 (July)
                oModel.FeeYear = latest.Year;    // 2025

                isMultiMonth = selectedMonthsList.Count > 1;
            }
            else
            {
                // Single month fallback
                totalApplicableFee = oFeeModel.FeeWiseDetails
                    .Where(c => c.FeeMonth == oModel.FeeMonth && c.FeeYear == oModel.FeeYear)
                    .Sum(x => x.ApplicableFee - x.RDiscount - x.PaidAmount);

                // Build a single-item list so the view can use the same path   ← NEW
                selectedMonthsList = new List<FeeSelectionViewModel>
        {
            new FeeSelectionViewModel
            {
                Month  = oModel.FeeMonth,
                Year   = oModel.FeeYear,
                Amount = totalApplicableFee
            }
        };
            }
           
            oModel.ApplicableFee = totalApplicableFee;

            OnlinePaymentModel _Payment = new OnlinePaymentModel();

            _Payment.Amount = Convert.ToInt32(oModel.ApplicableFee * 100);// Converted to paise
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

            // 5. Serialize validated months back to JSON for DB storage   ← NEW
            string validatedMonthsJson = isMultiMonth
                ? Newtonsoft.Json.JsonConvert.SerializeObject(selectedMonthsList)
                : null;

            // 6. Build OrderModel
            // Create order model for return on view
            OrderModel orderModel = new OrderModel
            {
                PGOrderID = transactionId.ToString(),
                razorpaySecret = _razorpaySecret,
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
                // ── NEW ──────────────────────────────────────────────
                IsMultiMonth = isMultiMonth,
                SelectedMonthsJson = validatedMonthsJson

            };

            //string hashString = orderModel.razorpayKey + "|" + orderModel.OrderID + "|" + orderModel.Amount + "|" + orderModel.Name + "|" + orderModel.EmailID + "|" + orderModel.Description + "|" + orderModel.StudentID + "|" + orderModel.FeeMonth + "|" + orderModel.FeeYear + "|" + orderModel.razorpaySecret;
            // orderModel.Hash = GenerateSha256(hashString);

            orderModel.Date = SMEnterprise.Repository.CommonUsage.GetCurrentDate();
            accountData.InsertOrderID(orderModel);
            ViewBag.OrderDetails = orderModel;
            TempData["OrderDetails"] = orderModel;
            //New for selected month
            TempData["SelectedMonths"] = selectedMonthsList;

            return View(studentmodel);
        }
        public string GenerateSha256(string text)
        {
            byte[] message = Encoding.UTF8.GetBytes(text);
            using (var hashString = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] hashValue = hashString.ComputeHash(message);
                var hex = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
                return hex;
            }
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
        public ActionResult PaymentCallback1(OrderModel objModel)
        {
            string paymentId = Request.Form["razorpay_payment_id"];
            string OrderID = Request.Form["razorpay_order_id"];
            OrderModel originalOrder = accountData.GetOrderForPayment(OrderID);

            // If the order doesn't exist, handle the error.
            if (originalOrder == null)
            {
                // Log error and redirect to failure page.
                return RedirectToAction("Failed");
            }
            // 2. Verify the payment signature. This is a crucial security step.
            RazorpayClient client = new RazorpayClient(_razorpayKeyId, _razorpaySecret);
            Razorpay.Api.Payment payment = client.Payment.Fetch(paymentId);


            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes.Add("razorpay_payment_id", paymentId);
            attributes.Add("razorpay_order_id", Request.Form["razorpay_order_id"]);
            attributes.Add("razorpay_signature", Request.Form["razorpay_signature"]);
            try
            {
                Utils.verifyPaymentSignature(attributes);
            }
            catch (Exception ex)
            {

                return RedirectToAction("Failed");
            }

            // 4. *** CRITICAL STEP: VALIDATE THE AMOUNT ***

            decimal paidAmountInPaise = Convert.ToDecimal(payment.Attributes["amount"]);

            decimal originalAmountInPaise = originalOrder.Amount * 100;

            // Check if the amounts match.
            if (paidAmountInPaise != originalAmountInPaise)
            {
                // Amounts do not match. This is a fraud attempt.
                // Log the discrepancy and do NOT process the payment.
                int Status = -1;
                string ReferanceNumber = "AmountMismatch";
                accountData.UpdateOrderStatus(OrderID, originalOrder.StudentID.ToString(), originalOrder.SessionID.ToString(), ReferanceNumber, Status);
                return RedirectToAction("Failed");
            }

            

            // 5. Capture the payment.
            // Dictionary<string, object> options = new Dictionary<string, object>();
            //options.Add("amount", payment.Attributes["amount"]);
            //options.Add("amount", paidAmountInPaise);
            //options.Add("currency", "INR");
            // Razorpay.Api.Payment paymentCaptured = payment.Capture(options);
            //var   paymentCaptured = payment.Capture(options);
            // 6. Proceed with the successful payment logic only if the status is "captured"

            int StudentID = 0; int SBranchID = 0;
            if (payment.Attributes["status"] == "captured")
            {
                OrderModel om = new OrderModel();
                FeePaymentModel FeeModel = new FeePaymentModel();
                om.PGOrderID = paymentId;
                om.PGPaymentID = paymentId;
                om.Status = 1;
                //om.FeeMonth = objModel.FeeMonth;
                //om.FeeYear = objModel.FeeYear;
                //om.StudentID = objModel.StudentID;
                //om.SBranchID = objModel.SBranchID;

                // --- SECURE: Use originalOrder for all data ---

                om.FeeMonth = originalOrder.FeeMonth;
                om.FeeYear = originalOrder.FeeYear;
                om.StudentID = originalOrder.StudentID;
                om.SBranchID = originalOrder.SBranchID;
                om.PaymentAmount = paidAmountInPaise / 100;
                StudentID = om.StudentID;
                SBranchID = om.SBranchID;
                om.PaymentMode = 3;
                om.QDate = CommonUsage.GetCurrentDate();
                om.CurDate = CommonUsage.GetCurrentDate();
                om.PaymentDate = CommonUsage.GetCurrentDate();

                //FeeModel.SBranchID = objModel.SBranchID;
                //FeeModel.StudentID = objModel.StudentID;
                //FeeModel.SessionID = objModel.SessionID;
                //FeeModel.FeeAmount = objModel.ApplicableFee;
                FeeModel.SBranchID = originalOrder.SBranchID;
                FeeModel.StudentID = originalOrder.StudentID;
                FeeModel.SessionID = originalOrder.SessionID;
                FeeModel.FeeAmount = originalOrder.Amount;
                FeeModel.FeePaymentMode = 3;
                FeeModel.PaymentAmount = paidAmountInPaise / 100;
                FeeModel.ReferanceNumber = paymentId;
                FeeModel.Remark = "Paid by PaymentGateway";
                FeeModel.Month = originalOrder.FeeMonth;
                FeeModel.Year = originalOrder.FeeYear;
                // --- END SECURE BLOCK ---
                int Status = 1;
                string hashSequence;

                //hashSequence = _razorpayKeyId + "|" + OrderID + "|" + paidAmountInPaise + "|" + originalOrder.Name + "|" + originalOrder.EmailID + "|" + originalOrder.Description + "|" + om.StudentID + "|" + om.FeeMonth + "|" + om.FeeYear + "|" + om.PGPaymentID + "|" + _razorpaySecret;
                // string generatedHash = GenerateSha256(hashSequence);

                accountData.UpdateOrderStatus(OrderID, om.StudentID.ToString(), om.SBranchID.ToString(), FeeModel.ReferanceNumber, Status);
                FeePaymentRowModel objData = accountData.SaveStudentFeePaymentOnline(FeeModel);

                return RedirectToAction("Success", new { ID = paymentId });

            }
            else
            {
                int Status = -1;
                string ReferanceNumber = "Failed";
                accountData.UpdateOrderStatus(paymentId, StudentID.ToString(), SBranchID.ToString(), ReferanceNumber, Status);
                return RedirectToAction("Failed");
            }
        }
        [HttpPost]
        public ActionResult PaymentCallback(OrderModel objModel)
        {
            string paymentId = Request.Form["razorpay_payment_id"];
            string OrderID = Request.Form["razorpay_order_id"];
            OrderModel originalOrder = accountData.GetOrderForPayment(OrderID);

            // If the order doesn't exist, handle the error.
            if (originalOrder == null)
            {
                // Log error and redirect to failure page.
                return RedirectToAction("Failed");
            }
            // 2. Verify the payment signature. This is a crucial security step.
            RazorpayClient client = new RazorpayClient(_razorpayKeyId, _razorpaySecret);
            Razorpay.Api.Payment payment = client.Payment.Fetch(paymentId);


            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes.Add("razorpay_payment_id", paymentId);
            attributes.Add("razorpay_order_id", Request.Form["razorpay_order_id"]);
            attributes.Add("razorpay_signature", Request.Form["razorpay_signature"]);
            try
            {
                Utils.verifyPaymentSignature(attributes);
            }
            catch (Exception ex)
            {
               
                return RedirectToAction("Failed");
            }

            // 4. *** CRITICAL STEP: VALIDATE THE AMOUNT ***
           
            decimal paidAmountInPaise = Convert.ToDecimal(payment.Attributes["amount"]);
            
            decimal originalAmountInPaise = originalOrder.Amount* 100;

            // Check if the amounts match.
            if (paidAmountInPaise != originalAmountInPaise)
            {
                // Amounts do not match. This is a fraud attempt.
                // Log the discrepancy and do NOT process the payment.
                int Status = -1;
                string ReferanceNumber = "AmountMismatch";
                accountData.UpdateOrderStatus(OrderID, originalOrder.StudentID.ToString(), originalOrder.SessionID.ToString(), ReferanceNumber, Status);
                return RedirectToAction("Failed");
            }
            FeePaymentModel feeModel = new FeePaymentModel();
            // ── 5. Determine multi-month or single-month ─────────────
            bool isMultiMonth = originalOrder.IsMultiMonth && !string.IsNullOrEmpty(originalOrder.SelectedMonthsJson);

          

            if (isMultiMonth)
            {
                // Deserialize the validated JSON stored at order-creation time
                var selectedMonths = Newtonsoft.Json.JsonConvert
                    .DeserializeObject<List<FeeSelectionViewModel>>(originalOrder.SelectedMonthsJson);

                // Sort ascending so oldest month is paid first (matches SP logic)
                var latestMonth = selectedMonths
               .OrderByDescending(x => x.Year)
               .ThenByDescending(x => x.Month)
               .First();

                feeModel.Month = latestMonth.Month;   // ← Latest selected month
                feeModel.Year = latestMonth.Year;    // ← Latest selected year
                feeModel.PaymentAmount = paidAmountInPaise / 100; // ← TOTAL amount
            }
            else
            {
               
                // Single month — same as before
                feeModel.Month = originalOrder.FeeMonth;
                feeModel.Year = originalOrder.FeeYear;
                feeModel.PaymentAmount = paidAmountInPaise / 100;
            }

            // Common fields
            feeModel.SBranchID = originalOrder.SBranchID;
            feeModel.StudentID = originalOrder.StudentID;
            feeModel.SessionID = originalOrder.SessionID;
            feeModel.FeePaymentMode = 3;                        // 3 = Online
            feeModel.ReferanceNumber = paymentId;                // Razorpay payment ID
            feeModel.Remark = "Paid by Payment Gateway";
            feeModel.FeeAmount = paidAmountInPaise / 100;


            // ── 6. Save a fee payment row for EACH selected month ────
          
           
                FeePaymentRowModel result = accountData.SaveStudentFeePaymentOnline(feeModel);
               
            //hashSequence = _razorpayKeyId + "|" + OrderID + "|" + paidAmountInPaise + "|" + originalOrder.Name + "|" + originalOrder.EmailID + "|" + originalOrder.Description + "|" + om.StudentID + "|" + om.FeeMonth + "|" + om.FeeYear + "|" + om.PGPaymentID + "|" + _razorpaySecret;
            // string generatedHash = GenerateSha256(hashSequence);


            // ── 7. Mark order as successful in OrderMaster ───────────
            accountData.UpdateOrderStatus(
                OrderID,
                originalOrder.StudentID.ToString(),
                originalOrder.SessionID.ToString(),
                paymentId,   // PGPaymentID = Razorpay payment ID
                1);          // Status = 1 (success)

            return RedirectToAction("Success", new { ID = paymentId });
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
                int Status = 1;
              

                accountData.UpdateOrderStatus(paymentId, objModel.StudentID.ToString(), objModel.SessionID.ToString(), FeeModel.ReferanceNumber,Status);
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

        public ActionResult Failed(FormCollection form)
        {

            return View("Failed");
        }

      
       
    }
}