using Razorpay.Api;
using SMEnterprise.Filters;
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
using static System.Web.Razor.Parser.SyntaxConstants;

namespace SMEnterprise.Controllers
{
    public class PaymentController : Controller
    {
        AccountData accountData = new AccountData();

     //  private readonly string _razorpayKeyId = "rzp_live_RSpysm7E9l1iVG";
      //  private readonly string _razorpaySecret = "BU5GhnJ4xZfN2IOny0FeRbKL";

        private SMEnterprise.Models.SBranchModel GetBranchPaymentProfile(int branchId)
        {
            try
            {
                var branch = accountData.GetBranchPaymentProfile(branchId);
                if (branch != null)
                {
                    return branch;
                }
            }
            catch
            {
            }

            try
            {
                var user = PermissionManager.GetLoggedInUser();
                if (user == null)
                {
                    return null;
                }

                return new SMEnterprise.Models.SBranchModel
                {
                    SBranchID = branchId,
                    BranchName = user.BranchName,
                    BranchSchoolName = user.BranchSchoolName,
                    EmailID = user.EmailID,
                    Logo = user.BranchLogo
                };
            }
            catch
            {
                return null;
            }
        }
        private BranchPaymentGatewayModel GetRequiredBranchGateway(int branchId)
        {
            var gateway = accountData.GetBranchGateway(branchId);
            if (gateway == null || (gateway.IsActive.HasValue && !gateway.IsActive.Value) || string.IsNullOrWhiteSpace(gateway.KeyId) || string.IsNullOrWhiteSpace(gateway.Secret))
            {
                throw new InvalidOperationException("Branch payment gateway is not configured.");
            }

            var user = PermissionManager.GetLoggedInUser();
            if (user != null)
            {
                if (string.IsNullOrWhiteSpace(gateway.BranchName))
                {
                    gateway.BranchName = user.BranchName;
                }
                if (string.IsNullOrWhiteSpace(gateway.BranchSchoolName))
                {
                    gateway.BranchSchoolName = user.BranchSchoolName;
                }
                if (string.IsNullOrWhiteSpace(gateway.EmailID))
                {
                    gateway.EmailID = user.EmailID;
                }
                if (string.IsNullOrWhiteSpace(gateway.ImageUrl))
                {
                    gateway.ImageUrl = user.BranchLogo;
                }
                if (string.IsNullOrWhiteSpace(gateway.Address))
                {
                    gateway.Logo = user.BranchLogo;
                }
            }

            return gateway;
        }
      
        public ActionResult Index(OrderModel oModel, string SelectedMonthsJson)
        {
            
            

            string transactionId = Guid.NewGuid().ToString();
            //int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            int SBranchID = PermissionManager.GetLoggedInUser().SBranchID;
            int StudentID = oModel.StudentID;

            // 1. Get Student Details
            StudentModel studentmodel = accountData.GetStudentDetailsForPayment(StudentID, SBranchID);
            var gateway = GetRequiredBranchGateway(SBranchID);

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
            _Payment.EmailID = string.IsNullOrEmpty(studentmodel.EmailID) ? gateway.EmailID : studentmodel.EmailID;


           // RazorpayClient client = new RazorpayClient(_razorpayKeyId, _razorpaySecret);
                RazorpayClient client = new RazorpayClient(gateway.KeyId, gateway.Secret);
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
                //razorpaySecret = _razorpaySecret,
                //razorpayKey = _razorpayKeyId,
                razorpaySecret = gateway.Secret,
                razorpayKey = gateway.KeyId,
                Amount = _Payment.Amount,
                currency = "INR",
                OrderID = RazorpayOrderID,
                Name = _Payment.Name,
                EmailID = _Payment.EmailID,
                ContactNumber = _Payment.ContactNumber,
                Address = _Payment.Address,
               // Description = "Chamba School Name",
                Description = gateway.BranchSchoolName,
                FeeMonth = oModel.FeeMonth,
                FeeYear = oModel.FeeYear,
                ApplicableFee = oModel.ApplicableFee,
                StudentID = oModel.StudentID,
                SessionID = studentmodel.SessionID,
                SBranchID = SBranchID,
                ImageUrl= gateway.ImageUrl,
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
            int sBranchId = PermissionManager.GetLoggedInUser().SBranchID;
            var branch = GetBranchPaymentProfile(sBranchId);
            var gateway = GetRequiredBranchGateway(sBranchId);

            // Generate random receipt number for order
            Random randomObj = new Random();
            string transactionId = Guid.NewGuid().ToString();
           
           // RazorpayClient client = new RazorpayClient(_razorpayKeyId, _razorpaySecret);
            RazorpayClient client = new RazorpayClient(gateway.KeyId, gateway.Secret);
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
              //  razorpayKey = _razorpayKeyId,
                razorpayKey = gateway.KeyId,
                Amount = _Payment.Amount * 100,
                currency = "INR",
                OrderID = _Payment.OrderID,
                Name = _Payment.Name,
                EmailID = _Payment.EmailID,
                ContactNumber = _Payment.ContactNumber,
                Address = _Payment.Address,
               // Description = "Chamba School Name",
                Description = gateway.BranchSchoolName,
               ImageUrl = gateway.ImageUrl

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
            OrderModel originalOrder = accountData.GetOrderForPayment(OrderID);

            int sBranchId = originalOrder.SBranchID;
            var branch = GetBranchPaymentProfile(sBranchId);
            var gateway = GetRequiredBranchGateway(sBranchId);

            // If the order doesn't exist, handle the error.
            if (originalOrder == null)
            {
                // Log error and redirect to failure page.
                return RedirectToAction("Failed");
            }
            // 2. Verify the payment signature. This is a crucial security step.
          //  RazorpayClient client = new RazorpayClient(_razorpayKeyId, _razorpaySecret);
                RazorpayClient client = new RazorpayClient(gateway.KeyId, gateway.Secret);
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


            decimal paidAmount = paidAmountInPaise / 100m;
            #region use for School Subscription Payment
            // Detect subscription order (explicit flag or fallback StudentID==0)
            bool isSubscriptionOrder = (originalOrder != null &&
                (originalOrder.IsSubscriptionOrder || (originalOrder.StudentID == 0 && originalOrder.SBranchID > 0)));

            if (isSubscriptionOrder)
            {
                return HandleSubscriptionPayment(OrderID, originalOrder, paymentId, paidAmount);
            }
            #endregion

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
            feeModel.FeeAmount = paidAmount;


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

            // Fetch original order record (must exist)
            var originalOrder = accountData.GetOrderForPayment(orderId);
            if (originalOrder == null)
            {
                // Order not found -> cannot continue
                TempData["PaymentError"] = "Order not found.";
                return RedirectToAction("Failed");
            }
            int sBranchId = originalOrder.SBranchID;
            var branch = GetBranchPaymentProfile(sBranchId);
            var gateway = GetRequiredBranchGateway(sBranchId);

            bool isSubscriptionOrder = (originalOrder != null &&
                (originalOrder.IsSubscriptionOrder || (originalOrder.StudentID == 0 && originalOrder.SBranchID > 0)));

            // Determine credentials to use. Subscription payments use the ERP/system gateway.
          //  string keyToUse = _razorpayKeyId;
           // string secretToUse = _razorpaySecret;
            string keyToUse = gateway.KeyId;
            string secretToUse = gateway.Secret;

            if (isSubscriptionOrder)
            {
                try
                {
                    var gw = accountData.GetSystemPaymentGateway();
                    if (gw != null && gw.IsActive && gw.UseForSubscription
                        && !string.IsNullOrWhiteSpace(gw.RazorpayKeyId)
                        && !string.IsNullOrWhiteSpace(gw.RazorpaySecret))
                    {
                        keyToUse = gw.RazorpayKeyId;
                        secretToUse = gw.RazorpaySecret;
                    }
                }
                catch { /* use configured controller defaults */ }
            }
            else if (!string.IsNullOrWhiteSpace(originalOrder.razorpayKey))
            {
                keyToUse = originalOrder.razorpayKey;
            }

            // Create client with the selected credentials
            RazorpayClient client;
            try
            {
                client = new RazorpayClient(keyToUse, secretToUse);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Razorpay client init failed: " + ex);
                TempData["PaymentError"] = "Payment gateway initialization failed.";
                return RedirectToAction("Failed");
            }

            // Fetch payment from Razorpay using the same merchant account used to create the order
            Razorpay.Api.Payment payment;
            try
            {
                payment = client.Payment.Fetch(paymentId);
            }
            catch (Razorpay.Api.Errors.BadRequestError bre)
            {
                System.Diagnostics.Debug.WriteLine("Razorpay fetch error: " + bre);
                TempData["PaymentError"] = "Payment retrieval failed: " + bre.Message;
                return RedirectToAction("Failed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Razorpay fetch unexpected error: " + ex);
                TempData["PaymentError"] = "Payment retrieval failed.";
                return RedirectToAction("Failed");
            }

            // Capture the payment
            try
            {
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", payment.Attributes["amount"]);
                Razorpay.Api.Payment paymentCaptured = payment.Capture(options);

                // Convert to rupees
                decimal paidAmount = Convert.ToDecimal(paymentCaptured.Attributes["amount"]) / 100m;

                if (isSubscriptionOrder)
                {
                    // handle subscription: mark branch paid, update order, clear session
                    return HandleSubscriptionPayment(orderId, originalOrder, paymentId, paidAmount);
                }

                // existing student payment flow (unchanged)
                if (paymentCaptured.Attributes["status"] == "captured")
                {
                    // Build FeeModel using originalOrder (secure)
                    FeePaymentModel FeeModel = new FeePaymentModel();
                    FeeModel.SBranchID = originalOrder.SBranchID;
                    FeeModel.StudentID = originalOrder.StudentID;
                    FeeModel.SessionID = originalOrder.SessionID;
                    FeeModel.FeeAmount = originalOrder.Amount;
                    FeeModel.FeePaymentMode = 3;
                    FeeModel.PaymentAmount = paidAmount;
                    FeeModel.ReferanceNumber = paymentId;
                    FeeModel.Remark = "Paid by PaymentGateway";
                    FeeModel.Month = originalOrder.FeeMonth;
                    FeeModel.Year = originalOrder.FeeYear;

                    accountData.UpdateOrderStatus(paymentId, originalOrder.StudentID.ToString(), originalOrder.SessionID.ToString(), FeeModel.ReferanceNumber, 1);
                    FeePaymentRowModel objData = accountData.SaveStudentFeePaymentOnline(FeeModel);

                    return RedirectToAction("Success", new { ID = orderId });
                }
                else
                {
                    accountData.UpdateOrderStatus(paymentId, originalOrder.StudentID.ToString(), originalOrder.SessionID.ToString(), "Failed", -1);
                    return RedirectToAction("Failed");
                }
            }
            catch (Razorpay.Api.Errors.BadRequestError bre)
            {
                System.Diagnostics.Debug.WriteLine("Razorpay capture error: " + bre);
                TempData["PaymentError"] = "Payment capture failed: " + bre.Message;
                return RedirectToAction("Failed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Razorpay capture unexpected error: " + ex);
                TempData["PaymentError"] = "Payment capture failed.";
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



        [HttpGet]
        public ActionResult Subscribe(int branchId, string paymentType = null)
        {

            string keyToUse;
            string secretToUse;

            var accountData = new AccountData();
            var systemGateway = accountData.GetSystemPaymentGateway();

            var sub = accountData.GetBranchSubscription(branchId);
            var dueAmount = sub == null ? 0m : (sub.NextDueAmount > 0m ? sub.NextDueAmount : sub.DueAmount);
            if (sub == null || !sub.IsDue || dueAmount <= 0m)
            {
                TempData["Message"] = "No subscription due for this branch.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var canPayPartial = CanPaySubscriptionPartial(sub);
            var minimumPartialAmount = canPayPartial ? GetMinimumSubscriptionPartialAmount(sub, dueAmount) : dueAmount;
            var normalizedPaymentType = string.IsNullOrWhiteSpace(paymentType)
                ? "full"
                : paymentType.Trim().ToLowerInvariant();
            var payableAmount = GetSubscriptionPayableAmount(sub, normalizedPaymentType);

            var orderModel = new OrderModel
            {
                PGOrderID = Guid.NewGuid().ToString(),
                Amount = Convert.ToInt32(payableAmount * 100),
                currency = "INR",
                OrderID = "",
                Name = "ERP Subscription",
                EmailID = PermissionManager.GetLoggedInUser()?.EmailID ?? "info@prabhutisystems.com",
                ContactNumber = "",
                Description = "Subscription Payment",
                StudentID = 0,
                SessionID = 0,
                SBranchID = branchId,
                ApplicableFee = payableAmount,
                IsSubscriptionOrder = true
            };

            // Subscription payments use the ERP/system gateway, not a school branch gateway.
            
            try
            {
               
                if (systemGateway != null && systemGateway.IsActive && systemGateway.UseForSubscription
                    && !string.IsNullOrWhiteSpace(systemGateway.RazorpayKeyId)
                    && !string.IsNullOrWhiteSpace(systemGateway.RazorpaySecret))
                {
                    keyToUse = systemGateway.RazorpayKeyId;
                    secretToUse = systemGateway.RazorpaySecret;
                }
            }
            catch { /* use configured controller defaults */ }

            // IMPORTANT: set the key you will send to the client so checkout uses the same key
            orderModel.razorpayKey = systemGateway.RazorpayKeyId;
            // Do NOT include secret in the view; we only use it server-side.
            orderModel.razorpaySecret = null;

          //  var client = new RazorpayClient(keyToUse, secretToUse);
                var client = new RazorpayClient(systemGateway.RazorpayKeyId, systemGateway.RazorpaySecret);
            var options = new Dictionary<string, object>
    {
        { "amount", orderModel.Amount },
        { "currency", orderModel.currency },
        { "receipt", orderModel.PGOrderID },
        { "payment_capture", 0 }
    };

            try
            {
                var razorOrder = client.Order.Create(options);
                orderModel.OrderID = razorOrder["id"].ToString();
            }
            catch (Exception ex)
            {
                // Log full exception for investigation (replace with your logger)
                System.Diagnostics.Debug.WriteLine("Razorpay Order.Create error: " + ex.ToString());

                // Provide more info to UI for debugging (optional)
                TempData["PaymentError"] = "Could not create payment order. Check server logs for details.";
                return RedirectToAction("Dashboard", "Admin");
            }

            orderModel.Date = CommonUsage.GetCurrentDate();
            accountData.InsertOrderID(orderModel);

            ViewBag.OrderDetails = orderModel;
            ViewBag.SubscriptionDueAmount = dueAmount;
            ViewBag.SubscriptionCanPayPartial = canPayPartial;
            ViewBag.SubscriptionMinimumPartialAmount = minimumPartialAmount;
            ViewBag.SubscriptionPaymentType = payableAmount >= dueAmount ? "full" : "partial";
            return View("SubscribeCheckout", orderModel);
        }
        private ActionResult HandleSubscriptionPayment(string orderId, OrderModel originalOrder, string paymentId, decimal paidAmount)
        {
            try
            {
                if (originalOrder.ApplicableFee > 0m && paidAmount < originalOrder.ApplicableFee)
                {
                    TempData["PaymentError"] = "Paid amount is less than the required subscription amount.";
                    return RedirectToAction("Failed");
                }

                // paidAmount is in rupees (not paise)
                accountData.MarkBranchSubscriptionPaid(originalOrder.SBranchID, paidAmount, paymentId, PermissionManager.GetLoggedInUser()?.UserID);

                // update order master to successful (Status = 1)
                accountData.UpdateOrderStatus(orderId, "0", "0", paymentId, 1);

                // clear UI session flags so modal won't reappear
                Session.Remove("ShowPaymentDue");
                Session.Remove("PaymentDueBranchID");
                Session.Remove("PaymentDueAmount");
                Session.Remove("PaymentDuePlanName");

                // Build receipt model to show to user
                var receipt = new SMEnterprise.Models.SubscriptionReceiptModel
                {
                    OrderID = orderId,
                    PaymentID = paymentId,
                    SBranchID = originalOrder.SBranchID,
                    AmountPaid = paidAmount,
                    PlanName = accountData.GetBranchSubscription(originalOrder.SBranchID)?.PlanName,
                    PaidOn = CommonUsage.GetCurrentDate()
                };

                // Return a friendly receipt page (does not log out)
                return View("SubscriptionReceipt", receipt);
            }
            catch
            {
                // on failure, mark order as failed and redirect to Failed page
                accountData.UpdateOrderStatus(orderId, "0", "0", "MarkSubscriptionPaidFailed", -1);
                return RedirectToAction("Failed");
            }
        }

        private bool CanPaySubscriptionPartial(SMEnterprise.Models.BranchSubscriptionModel sub)
        {
            return sub.AllowPartialPayment
                && sub.MaxPartialPayments > 0
                && (sub.MaxPartialPayments - sub.PartialPaymentCount) > 1;
        }

        private decimal GetMinimumSubscriptionPartialAmount(SMEnterprise.Models.BranchSubscriptionModel sub, decimal dueAmount)
        {
            var remainingPartialPayments = sub.MaxPartialPayments - sub.PartialPaymentCount;
            if (remainingPartialPayments <= 1)
            {
                return dueAmount;
            }

            return Math.Round(dueAmount / remainingPartialPayments, 2, MidpointRounding.AwayFromZero);
        }

        private decimal GetSubscriptionPayableAmount(SMEnterprise.Models.BranchSubscriptionModel sub, string paymentType)
        {
            var dueAmount = sub.NextDueAmount > 0m ? sub.NextDueAmount : sub.DueAmount;
            var canPayPartial = CanPaySubscriptionPartial(sub);
            if (!canPayPartial || paymentType == "full")
            {
                return dueAmount;
            }

            return GetMinimumSubscriptionPartialAmount(sub, dueAmount);
        }
    }
}
