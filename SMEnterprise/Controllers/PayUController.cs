using Razorpay.Api;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SMEnterprise.Controllers
{
    public class PayUController : Controller
    {
        // GET: PayU
        string key = "JVDkAA"; // Your PayU key
        string salt = "MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQC0PewBdO/B+hMc2kbweqBI/0iIBm1FPeSn7wLIQIEQE7jv27XdrFSOt+fDr6Zbp8IZJTUj3eqDw4G6YbQeAfkYy+T/2orcBxsIUbArvWt1QN5GDzP0+v26Mes2fbjCM7sJXYCa4fwYmu4LPDG1X2R0P0Sw3Cjbt1nZRhhZLRw/MrFfJvyyklHU4sP2NgmQxqzdGxGROK3eG3tPMP0nJfVnfFy7VPrM0U11ytJCGalaAiYryeoxXArk/d3zjS9yEXBiRKfQDPFoouoN9PpIGYEw9eLzog2+S9De1cNi3PErBUaiCxlTXF9Bvwc+0gG1t1tBXzo9B1mdjBYnWqZyuWRlAgMBAAECggEAL9tVI5nh4xY8JugJ9+YHUvp9TqiuMLlbUf+TNIpz+knSS9WBd2c/WFmaLwofWKy1S6nmbyyIDQ5HDq7MfT823Y+dsRVc00kLs2zFIwf7VTmxCtcUi5Js9hg1BNXizaWgnr7126nvHrVcSR1lcWvvpzH5UiBStOv87kS/SEZC/sBLnN4Y0Gj51NThj/yB2cRppf51cPd/oCOAfhwZUvFSecQVWESRf2GB0dPnwjGAyB1qFKzICm/o8kqQ6ZyhmssnLuAf3AXXsVSyoZHrDFwZjLtHXA3Lhnqm/ZseCAaj9+dG/V+MRk48HNDV8TWl5JVUTYtDenNMTWm+i+mIysD/jQKBgQDmy9XkjqcELJ6Ss6VKl5MBzHotaJgF9ek3zh1eFmYtRU5nHyqs9bd8htTCcCSNP+yVlQyUJMdTHoUQL7FlK2Nf9Tba0Y33GEih8hwIbmAfhhe7kf6fiA/YDuUmIMuI7d2RDwMSROtdMDi8lQWpnXWJ6WXOWasebt+GUuIqOv7PAwKBgQDH7MiS0IVkI8ax6qlRLN2kaMPp49nNcTDTzaDN2SxVf3V1BhOmvo++M00w71kIXUFjbBtZRI9fMU0qY0/N383MQEgIsUNxfdm7BMk1brGSJTLr/+uc36cYqNWYE9Gldh2W0S5IjjS2hcrs/hpUcVz6NwL0UJlCZTXDwMFyBKcOdwKBgBqmzzxlfMPuoyEtvZiviDpc8n7r5SJLE3NuSdXjoEj6B+PApZzzgwzORSNu78mf6CId5CX3WU0v6Q3FdCbKq98Y4gzCxjISi9CntLUEifUZ9wOiCFVD0RCSJ8QPZXGahkuKAEDL1KIeP28Hhm/fzwLuAOOjVy9cflN0nslhpqshAoGAGZdYsVpZyNC/jWSxWb+5e2MhuN2+soqLqoEG1XK4NxWCroEhoNWBxuIAPoRUzDpLtXNQKOQayh+gdg2SWJMOX9fWoK44KN3oMgVR0DIkLsXuN8FhoooKdKCf/sCCtIBjFzwdhZIWc0q7CA0ax7ZfJXRHP4jVpWcZM76HxJWN4M8CgYBEKYCzLpx2SU0rjsBQAvqGIj/JxTFB4u9qjLEsUiz3WyAidKzOyBwlu00+ZK/mCwlNEVCcG9g0B2xag+sPe3CCF9jomQE14BWiCp1or6G8m1NLGrQSEUy2XAToGwff9XatpTZc6DsRBn7PFUFaxNGM+UcDWYxHWA+JZ2sToYIUhg=="; // Your PayU salt

        public ActionResult PayNow(OrderModel oModel)
        {
            string transactionId = Guid.NewGuid().ToString("N").Substring(0, 20);
            int StudentID = CommonUsage.ConvertToInt(Session["SChildID"].ToString());
            int SBranchID = 0;
            AccountData Accountdata = new AccountData();
            StudentModel studentDetails = Accountdata.GetStudentDetailsForPayment(StudentID, SBranchID);
            string amount1 = oModel.ApplicableFee.ToString("0.00"); // Format as string with two decimal points
            string email1 = string.IsNullOrEmpty(studentDetails.EmailID) ? "info@prabhutisystems.com" : studentDetails.EmailID;
            string phone = string.IsNullOrEmpty(studentDetails.FatherMobileNo) ? "9958803329" : studentDetails.FatherMobileNo;
            SBranchID = studentDetails.SBranchID;
            string txnid = transactionId.ToString();
            // string productinfo = "Fee Payment for " + model.Name + "student ID" + model.StudentID;
            string productinfo = "Fee Payment";
            string amount = oModel.ApplicableFee.ToString("0.00");

            string email = email1;
            var model = new PayUModel
            {
                MerchantKey = key,  // test key
                Salt = salt,
                Amount = amount,
                FirstName = studentDetails.Name,
                Email = email1,
                Phone = phone,
                ProductInfo = "Fee Payment",
                TransactionId = transactionId,
                SuccessUrl = Url.Action("PaymentSuccess", "PayU", null, Request.Url.Scheme),
                FailureUrl = Url.Action("PaymentFailed", "PayU", null, Request.Url.Scheme),
                udf1 = studentDetails.SessionID.ToString(),
                udf2 = studentDetails.StudentSessionUID.ToString(),
                udf3 = studentDetails.StudentID.ToString()
            };

            string hashString = model.MerchantKey + "|" + model.TransactionId + "|" + model.Amount + "|" + model.ProductInfo + "|" + model.FirstName + "|" + model.Email + "|" + model.udf1 + "|" + model.udf2 + "|" + model.udf3 + "||||||||" + model.Salt;
            model.Hash = GenerateHash512(hashString);
            OrderModel orderModel = new OrderModel
            {
                PGOrderID = transactionId,


                Amount = (int)oModel.ApplicableFee,
                currency = "INR",
                OrderID = transactionId,
                Name = studentDetails.Name,
                EmailID = email1,
                ContactNumber = phone,
                FeeMonth = oModel.FeeMonth,

                FeeYear = oModel.FeeYear,
                ApplicableFee = oModel.ApplicableFee,
                StudentID = studentDetails.StudentID,
                SBranchID = studentDetails.SBranchID,
                SessionID = studentDetails.SessionID,

            };
            orderModel.Date = SMEnterprise.Repository.CommonUsage.GetCurrentDate();
            Accountdata.InsertOrderID(orderModel);
            return View(model);
        }

        private string GenerateHash512(string text)
        {
            byte[] message = Encoding.UTF8.GetBytes(text);
            using (var hashString = new System.Security.Cryptography.SHA512Managed())
            {
                byte[] hashValue = hashString.ComputeHash(message);
                var hex = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
                return hex;
            }
        }
        public ActionResult PaymentSuccess()
        {
            var form = Request.Form;
            string status = form["status"];
            string firstname = form["firstname"];
            string amount = form["amount"];
            string txnid = form["txnid"];
            string posted_hash = form["hash"];
            string key = form["key"];
            string productinfo = form["productinfo"];
            string email = form["email"];

            // Additional Data
            string mihpayid = form["mihpayid"];
            string mode = form["mode"];
            string sessionId = form["udf1"];
            string studentSessionId = form["udf2"];
            string studentId = form["udf3"];
            string bank_ref_num = form["bank_ref_num"];

            // Handle Additional Charges if Present
            string additionalCharges = form["additionalCharges"];

            string hashSequence;

            if (!string.IsNullOrEmpty(additionalCharges))
            {
                hashSequence = additionalCharges + "|" + salt + "|" + status + "||||||||" + studentId + "|" + studentSessionId + "|" + sessionId + "|" + email + "|" + firstname + "|" + productinfo + "|" + amount + "|" + txnid + "|" + key;
            }
            else
            {
                hashSequence = salt + "|" + status + "||||||||" + studentId + "|" + studentSessionId + "|" + sessionId + "|" + email + "|" + firstname + "|" + productinfo + "|" + amount + "|" + txnid + "|" + key;
            }

            string generatedHash = GenerateHash512(hashSequence);

            if (generatedHash.Equals(posted_hash, StringComparison.OrdinalIgnoreCase))
            {
                // Payment Verified Successfully
                // Save details to DB here
                // Update Order Status

                ViewBag.Message = "Payment Successful!";
                ViewBag.PaymentId = mihpayid;
                ViewBag.Amount = amount;
                ViewBag.TransactionId = txnid;
            }
            else
            {
                // Hash mismatch — Possible Tampering
                ViewBag.Message = "Payment verification failed!";
            }

            return View();
        }

        public ActionResult PaymentFailed()
        {
            var form = Request.Form;
            string status = form["status"];
            string txnid = form["txnid"];
            string mihpayid = form["mihpayid"];
            string errorMessage = form["error_Message"];

            // Save failed transaction in DB if needed

            ViewBag.Message = "Payment Failed!";
            ViewBag.Status = status;
            ViewBag.TransactionId = txnid;
            ViewBag.PaymentId = mihpayid;
            ViewBag.Error = errorMessage;

            return View();
        }

    }
    public class PayUModel
    {
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string MerchantKey { get; set; }
        public string Salt { get; set; }
        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string ProductInfo { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string SuccessUrl { get; set; }
        public string FailureUrl { get; set; }
        public string Hash { get; set; }
    }

}