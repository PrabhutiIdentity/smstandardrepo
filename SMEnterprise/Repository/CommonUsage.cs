using Newtonsoft.Json;
using SMEnterprise.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.OleDb;
using System.Reflection;



namespace SMEnterprise.Repository
{

    public static class DateToWords
    {
        private static CultureInfo ci = new CultureInfo("en-US");
        private static string[] unitsMap = new string[]
          { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
                "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
                "Seventeen", "Eighteen", "Nineteen" };
        private static string[] tensMap = new string[]
          { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        //https://stackoverflow.com/questions/2729752/converting-numbers-in-to-words-c-sharp


        private static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            string words = "";

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += " ";

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return Regex.Replace(words, @"\ {2,}", " ");
        }

        public static string Convert(DateTime dt)
        {
            //convert days
            string dtw = NumberToWords(dt.Day);

            //convert months
            dtw += " " + dt.ToString("MMMM", ci);

            //convert years
            dtw += ", " + NumberToWords(dt.Year);

            return dtw;
        }
    }
    public class CommonUsage
    {
        public static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                if(string.IsNullOrEmpty(row["Name"]?.ToString()))
                {
                    break;
                }
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        public static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        if (pro.PropertyType == typeof(DateTime))
                        {
                            DateTime val = GetCurrentDate();
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "M/d/yyyy", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "M/d/yyyy", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "MM/dd/yyyy HH:mm:ss tt", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            }
                            catch { }

                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }

                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "dd.MM.yy", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "dd-MMM-yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }


                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "dd-MMM-yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "dd-MM-yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "dd/MM/yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "dd/MMM/yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "ddMMyyyy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }






                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d.MM.yyyy", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d-MMM-yyyy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }

                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d.MM.yy", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d-MMM-yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }


                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d-MMM-yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d-MM-yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d/MM/yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d/MM/yyyy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d/MMM/yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }





                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d.M.yyyy", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d-M-yyyy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }

                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d.M.yy", CultureInfo.InvariantCulture);
                            }
                            catch { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d-M-yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }



                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d-M-yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d/M/yy", CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            try
                            {
                                val = DateTime.ParseExact(dr[column.ColumnName].ToString(), "d/M/yyyy", CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                try
                                {
                                    val = DateTime.Parse(dr[column.ColumnName].ToString());
                                }
                                catch { }
                            }

                            pro.SetValue(obj, val, null);
                        }
                        else if (pro.PropertyType == typeof(string))
                        {
                            pro.SetValue(obj, dr[column.ColumnName].ToString(), null);

                        }
                        else if (pro.PropertyType == typeof(int))
                        {

                            pro.SetValue(obj, ConvertToInt(dr[column.ColumnName].ToString()), null);
                        }
                    }

                    else
                        continue;
                }
            }
            return obj;
        }
        private static List<ApiAuthenticationModel> _appUsersDynamicSalt;
        public static List<ApiAuthenticationModel> AppUsersDynamicSalt
        {
            get
            {
                if (_appUsersDynamicSalt == null)
                {
                    _appUsersDynamicSalt = new List<ApiAuthenticationModel>();
                }
                return _appUsersDynamicSalt;
            }
        }
        //private static List<ApiAuthenticationModel> _SplashUUIDSchoolID;
        //public static List<ApiAuthenticationModel> SplashUUIDSchoolID
        //{
        //    get
        //    {
        //        if (_SplashUUIDSchoolID == null)
        //        {
        //            _SplashUUIDSchoolID = new List<ApiAuthenticationModel>();
        //        }
        //        return _SplashUUIDSchoolID;
        //    }
        //}
        public enum NotificationTypes
        {
            Assignment
            , Holiday
            , Attendance
            , NoticeBoard
            , BlackBoard
            , Video
            , Fee
            , Event
            , ParentDiary
        }
        private static List<GSTStateModel> _GSTStates;
        public static List<GSTStateModel> GSTStates
        {
            get
            {
                if (_GSTStates == null)
                {
                    _GSTStates = (new AdminData()).GetGSTStates();
                }
                return _GSTStates;
            }
        }
        public static Dictionary<string, string> ConnectionStrings;
        public static void LoadConnectionStrings()
        {
            ConnectionStrings = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().ToDictionary(v => v.Name, v => v.ConnectionString);

        }
        public static string GetValidFileName(string fileName)
        {
            // remove any invalid character from the filename.
            if (fileName != null)
            {
                String ret = Regex.Replace(fileName.Trim(), "[^A-Za-z0-9_. ]+", "_");
                return ret.Replace(" ", "_").Replace(",", "_");
            }
            else
            {
                return fileName;
            }
        }
        public static string ConnectionString;
        public static void SetConnectionString()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["Mystring"].ToString().Replace("***********", "Smv2@prabhuti303");
        }
        public static SMSParamsa SMSConfig;
        //public static string ConnectionString = ConfigurationManager.ConnectionStrings["Mystring"].ToString().Replace("***********", "password303");
        public static string DriverConductorImageBasePath = "~/Images/DriverConductor";
        public static string VehicleImageBasePath = "~/Images/VehicleImages";
        public static string MessageAttachmentUploadBasePath = "~/MailAttachments";
        public static string PFatherImageUploadBasePath = "~/Images/ParentImage/Father";
        public static string PMotherImageUploadBasePath = "~/Images/ParentImage/Mother";
        public static string StudentImageBasePath = "~/Images/StudentImage";
        public static string StudentDocumentsBasePath = "~/Images/StudentDocuments";
        public static string EmployeeDocumentsBasePath = "~/Images/EmployeeDocuments";
        public static string EmployeeImageBasePath = "~/Images/EmployeeImage";
        public static string ExpenceBillsBasePath = "~/Images/ExpenceBillImages";
        public static string AssignmentAttachmentBasePath = "~/Attachments/Assignments";
        public static string BookImageBasePath = "~/Images/BookImage";
        public static string FixedPrimaryEncryptionSalt = "d2f*jk#g&h";
        public static string LuceneIndexBasePath = "LuceneIndex";
        public static string GioIndexBasePath = "GioIndex";
        public static string StopIndexBasePath = "StopIndex";
        public static string DatabasebackupDirecotry = "~/Backup/Database";
        public static string ParentAppSMSTemplate = "Hello [Reciever],%0a Please download our app from [PlayStoreLink] to stay updated with school activities, your Username is [UserName] and Password is [Password]";
        //  public static string ParentAppSMSTemplate = "Hello [Reciever],%0  your Username is [UserName] and Password is [Password]";
        public static string[] MonthNames = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        public static DateTime SessionStart;
        public static DateTime SessionEnd;
        public static List<SMSConfigirationModel> SMSConfigurations = new List<SMSConfigirationModel>();
        public static DateTime GetCurrentDate()
        {

            // DateTime ctime1 = DateTime.Now;  // your DataTimeVariable
            // DateTime time1 = new DateTime(ctime1.Year, ctime1.Month, ctime1.Day, ctime1.Hour, ctime1.Minute, ctime1.Second);
            //TimeZoneInfo timeZone1 = TimeZoneInfo.FindSystemTimeZoneById(TimeZone.CurrentTimeZone.StandardName);
            // TimeZoneInfo timeZone2 = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            // DateTime newTime = TimeZoneInfo.ConvertTime(time1, timeZone1, timeZone2);
            // return newTime;
            TimeSpan tsLocal = TimeZoneInfo.Local.GetUtcOffset(new DateTime());
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            TimeSpan tsIndia = tzi.GetUtcOffset(new DateTime());
            TimeSpan tsDifference = tsIndia.Subtract(tsLocal);
            return DateTime.Now.Add(tsDifference).AddHours(-1);
            //  return DateTime.Now.Add(tsDifference).AddHours(0);
        }
        public static DateTime GetServerDate()
        {
            return DateTime.Now;
        }
        public static string GetLocalTimeZone()
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            return localZone.StandardName;
        }
        public static DateTime ConvertToDateTime(string Date)
        {
            DateTime dtDate;
            try
            {
                dtDate = Convert.ToDateTime(Date);
            }
            catch
            {
                dtDate = GetCurrentDate();
            }
            return dtDate;
        }
        public static decimal ConvertToDecimal(string Value)
        {
            decimal dValue;
            try
            {
                dValue = Convert.ToDecimal(Value);
            }
            catch
            {
                dValue = 0;
            }
            return dValue;
        }
        public static int ConvertToInt(string Value)
        {
            int dValue;
            try
            {
                dValue = Convert.ToInt32(Value);
            }
            catch
            {
                dValue = 0;
            }
            return dValue;
        }
        public static byte[] Resize2Max50Kbytes(byte[] byteImageIn, int MaxAllowedKBs, string filePath)
        {
            byte[] currentByteImageArray = byteImageIn;
            double scale = 1f;

            try
            {

                MemoryStream inputMemoryStream = new MemoryStream(byteImageIn);
                Image fullsizeImage = Image.FromStream(inputMemoryStream);

                while (currentByteImageArray.Length > MaxAllowedKBs * 000)
                {
                    Bitmap fullSizeBitmap = new Bitmap(fullsizeImage, new Size((int)(fullsizeImage.Width * scale), (int)(fullsizeImage.Height * scale)));
                    MemoryStream resultStream = new MemoryStream();

                    fullSizeBitmap.Save(resultStream, fullsizeImage.RawFormat);

                    currentByteImageArray = resultStream.ToArray();
                    resultStream.Dispose();
                    resultStream.Close();

                    scale -= 0.05f;
                }
            }
            catch (Exception ex)
            { }
            return currentByteImageArray;
        }
        public static void SaveThumbImage(string Path, string FileName)
        {
            int height = 0;
            int width = 0;

            System.Drawing.Image image = System.Drawing.Image.FromFile(Path + "\\" + FileName);
            height = image.Height;
            width = image.Width;

            decimal ratio = 1;
            if (height > width)
                ratio = Convert.ToDecimal(242) / Convert.ToDecimal(height);
            else
                ratio = Convert.ToDecimal(345) / Convert.ToDecimal(width);

            System.Drawing.Image thumbnailImage = image.GetThumbnailImage(Convert.ToInt32(width * ratio), Convert.ToInt32(height * ratio), null, IntPtr.Zero);

            ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");

            EncoderParameters parameters = new EncoderParameters(3);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            parameters.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.ScanMethod, (int)EncoderValue.ScanMethodInterlaced);
            parameters.Param[2] = new EncoderParameter(System.Drawing.Imaging.Encoder.RenderMethod, (int)EncoderValue.RenderProgressive);


            thumbnailImage.Save(Path + "\\Thumb\\" + FileName, codec, parameters);

            //SaveMiniThumbImage(Path, FileName);


        }
        public static void SaveSmallImage(string Path)
        {
            int height = 0;
            int width = 0;

            System.Drawing.Image image = System.Drawing.Image.FromFile(Path);
            height = image.Height;
            width = image.Width;

            decimal ratio = 1;
            if (height > width)
                ratio = Convert.ToDecimal(242) / Convert.ToDecimal(height);
            else
                ratio = Convert.ToDecimal(345) / Convert.ToDecimal(width);

            System.Drawing.Image thumbnailImage = image.GetThumbnailImage(Convert.ToInt32(width * ratio), Convert.ToInt32(height * ratio), null, IntPtr.Zero);

            ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");

            EncoderParameters parameters = new EncoderParameters(3);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            parameters.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.ScanMethod, (int)EncoderValue.ScanMethodInterlaced);
            parameters.Param[2] = new EncoderParameter(System.Drawing.Imaging.Encoder.RenderMethod, (int)EncoderValue.RenderProgressive);

            //File.Delete(Path);
            thumbnailImage.Save(Path.Replace("\\StudentImage\\", "\\StudentImage\\thumb\\"), codec, parameters);

            //SaveMiniThumbImage(Path, FileName);


        }
        public static void SaveMiniThumbImage(string Path, string FileName)
        {
            int height = 0;
            int width = 0;

            System.Drawing.Image image = System.Drawing.Image.FromFile(Path + "\\" + FileName);
            height = image.Height;
            width = image.Width;

            decimal ratio = 1;
            if (height > width)
                ratio = Convert.ToDecimal(85) / Convert.ToDecimal(height);
            else
                ratio = Convert.ToDecimal(115) / Convert.ToDecimal(width);

            System.Drawing.Image thumbnailImage = image.GetThumbnailImage(Convert.ToInt32(width * ratio), Convert.ToInt32(height * ratio), null, IntPtr.Zero);

            ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");

            EncoderParameters parameters = new EncoderParameters(3);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            parameters.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.ScanMethod, (int)EncoderValue.ScanMethodInterlaced);
            parameters.Param[2] = new EncoderParameter(System.Drawing.Imaging.Encoder.RenderMethod, (int)EncoderValue.RenderProgressive);


            thumbnailImage.Save(Path + "\\MiniThumb\\" + FileName, codec, parameters);

        }
        public static void SaveBase64Image(string Image, string FileName)
        {
            System.Drawing.Image image = Base64ToImage(Image);

            ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");

            EncoderParameters parameters = new EncoderParameters(3);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            parameters.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.ScanMethod, (int)EncoderValue.ScanMethodInterlaced);
            parameters.Param[2] = new EncoderParameter(System.Drawing.Imaging.Encoder.RenderMethod, (int)EncoderValue.RenderProgressive);


            image.Save(FileName, codec, parameters);

        }
        public static System.Drawing.Image Base64ToImage(string base64String)
        {
            string base64PartTemp = base64String.Split(',')[1];

            //The final part of the base64 had a \" to remove
            //Now base64PartFinal is the base64 part of the image only
            string base64PartFinal = base64PartTemp.Split('\"')[0];
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64PartFinal);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }
        public static System.Drawing.Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }

        public static string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
        public static string EncryptPassword(string password)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedDataBytes;
            UTF8Encoding encoder = new UTF8Encoding();
            hashedDataBytes = md5Hasher.ComputeHash(encoder.GetBytes(password));
            System.Text.StringBuilder strHex = new System.Text.StringBuilder();
            foreach (byte b in hashedDataBytes)
            {
                strHex.Append(b.ToString("x2").ToLower());
            }

            string str = strHex.ToString();
            return str;
        }
        public static string SpellDecimal(decimal number)
        {
            string[] digit =
            {
            "", "One", "Two", "Three", "Four", "Five", "Six",
            "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve",
            "Thirteen", "Fourteen", "Fifteen", "Sixteen",
            "Seventeen", "Eighteen", "Nineteen"
      };

            string[] baseten =
            {
            "", "", "Twenty", "Thirty", "Fourty", "Fifty",
            "Sixty", "Seventy", "Eighty", "Ninety"
      };

            string[] expo =
            {
            "", "Thousand", "Million", "Billion", "Trillion",
            "Quadrillion", "Quintillion"
      };

            if (number == Decimal.Zero)
                return "zero";

            decimal n = Decimal.Truncate(number);
            decimal cents = Decimal.Truncate((number - n) * 100);

            StringBuilder sb = new StringBuilder();
            int thousands = 0;
            decimal power = 1;

            if (n < 0)
            {
                sb.Append("minus ");
                n = -n;
            }

            for (decimal i = n; i >= 1000; i /= 1000)
            {
                power *= 1000;
                thousands++;
            }

            bool sep = false;
            for (decimal i = n; thousands >= 0; i %= power, thousands--, power /= 1000)
            {
                int j = (int)(i / power);
                int k = j % 100;
                int hundreds = j / 100;
                int tens = j % 100 / 10;
                int ones = j % 10;

                if (j == 0)
                    continue;

                if (hundreds > 0)
                {
                    if (sep)
                        sb.Append(", ");

                    sb.Append(digit[hundreds]);
                    sb.Append(" Hundred");
                    sep = true;
                }

                if (k != 0)
                {
                    if (sep)
                    {
                        sb.Append(" and ");
                        sep = false;
                    }

                    if (k < 20)
                        sb.Append(digit[k]);
                    else
                    {
                        sb.Append(baseten[tens]);
                        if (ones > 0)
                        {
                            sb.Append("-");
                            sb.Append(digit[ones]);
                        }
                    }
                }

                if (thousands > 0)
                {
                    sb.Append(" ");
                    sb.Append(expo[thousands]);
                    sep = true;
                }
            }

            sb.Append(" Rupees ");
            //if (cents < 10) sb.Append("0");
            sb.Append(SpellDecimalPlace(cents));

            return sb.ToString();
        }
        public static string SpellDecimalPlace(decimal number)
        {
            string[] digit =
            {
            "", "One", "Two", "Three", "Four", "Five", "Six",
            "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve",
            "Thirteen", "Fourteen", "Fifteen", "Sixteen",
            "Seventeen", "Eighteen", "Nineteen"
      };

            string[] baseten =
            {
            "", "", "Twenty", "Thirty", "Fourty", "Fifty",
            "Sixty", "Seventy", "Eighty", "Ninety"
      };

            string[] expo =
            {
            "", "Thousand", "Million", "Billion", "Trillion",
            "Quadrillion", "Quintillion"
      };

            if (number == Decimal.Zero)
                return "";

            decimal n = Decimal.Truncate(number);
            decimal cents = Decimal.Truncate((number - n) * 100);

            StringBuilder sb = new StringBuilder();
            int thousands = 0;
            decimal power = 1;

            if (n < 0)
            {
                //sb.Append("minus ");
                n = -n;
            }

            for (decimal i = n; i >= 1000; i /= 1000)
            {
                power *= 1000;
                thousands++;
            }

            bool sep = false;
            for (decimal i = n; thousands >= 0; i %= power, thousands--, power /= 1000)
            {
                int j = (int)(i / power);
                int k = j % 100;
                int hundreds = j / 100;
                int tens = j % 100 / 10;
                int ones = j % 10;

                if (j == 0)
                    continue;

                if (hundreds > 0)
                {
                    if (sep)
                        sb.Append(", ");

                    sb.Append(digit[hundreds]);
                    sb.Append(" Hundred");
                    sep = true;
                }

                if (k != 0)
                {
                    if (sep)
                    {
                        sb.Append(" and ");
                        sep = false;
                    }

                    if (k < 20)
                        sb.Append(digit[k]);
                    else
                    {
                        sb.Append(baseten[tens]);
                        if (ones > 0)
                        {
                            sb.Append("-");
                            sb.Append(digit[ones]);
                        }
                    }
                }

                if (thousands > 0)
                {
                    sb.Append(" ");
                    sb.Append(expo[thousands]);
                    sep = true;
                }
            }

            sb.Append(" Paisa");
            return sb.ToString();
        }

        public static string ConvertNumbertoWords(long number)
        {
            if (number == 0) return "ZERO";
            if (number < 0) return "minus " + ConvertNumbertoWords(Math.Abs(number));
            string words = "";
            if ((number / 1000000) > 0)
            {
                words += ConvertNumbertoWords(number / 100000) + " Lakes ";
                number %= 1000000;
            }
            if ((number / 1000) > 0)
            {
                words += ConvertNumbertoWords(number / 1000) + " Thousand ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += ConvertNumbertoWords(number / 100) + " Hundred ";
                number %= 100;
            }
            //if ((number / 10) > 0)  
            //{  
            // words += ConvertNumbertoWords(number / 10) + " RUPEES ";  
            // number %= 10;  
            //}  
            if (number > 0)
            {
                if (words != "") words += "And ";
                var unitsMap = new[]
                {
            "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
        };
                var tensMap = new[]
                {
            "Zero", "Ten", "Twenty", "Thirty", "Fourty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };
                if (number < 20) words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0) words += " " + unitsMap[number % 10];
                }
            }
            return words;
        }
        public static string SendNotificationFCM(string[] deviceId, string message, string Type, string NotificationServerKey)
        {
            string sResponseFromServer = "";
            try
            {
                NotificationSendModel objNotification = new NotificationSendModel();
                objNotification.data = new NotificationModel();
                objNotification.data.Message = message;
                objNotification.data.NotificationDateTime = CommonUsage.GetCurrentDate();
                objNotification.data.Type = Type;
                objNotification.registration_ids = deviceId;
                string SERVER_API_KEY = NotificationServerKey;
                var value = message;
                WebRequest tRequest;

                tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.UseDefaultCredentials = true;
                tRequest.PreAuthenticate = true;
                tRequest.Credentials = CredentialCache.DefaultCredentials;
                tRequest.Method = "post";
                tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
                tRequest.ContentType = " application/json";
                //tRequest.Headers.Add(string.Format("Content-Type : {0}", "application/json"));


                string postData = JsonConvert.SerializeObject(objNotification);
                Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                tRequest.ContentLength = byteArray.Length;

                Stream dataStream = tRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse tResponse = tRequest.GetResponse();

                dataStream = tResponse.GetResponseStream();

                StreamReader tReader = new StreamReader(dataStream);

                sResponseFromServer = tReader.ReadToEnd();

                tReader.Close();
                dataStream.Close();
                tResponse.Close();
            }
            catch (Exception ex)
            {
                sResponseFromServer = ex.Message;
            }
            return sResponseFromServer;
        }


        public static string DateToText(DateTime dt, bool includeTime, bool isUK)
        {
            string[] ordinals =
            {
           "First",
           "Second",
           "Third",
           "Fourth",
           "Fifth",
           "Sixth",
           "Seventh",
           "Eighth",
           "Ninth",
           "Tenth",
           "Eleventh",
           "Twelfth",
           "Thirteenth",
           "Fourteenth",
           "Fifteenth",
           "Sixteenth",
           "Seventeenth",
           "Eighteenth",
           "Nineteenth",
           "Twentieth",
           "Twenty First",
           "Twenty Second",
           "Twenty Third",
           "Twenty Fourth",
           "Twenty Fifth",
           "Twenty Sixth",
           "Twenty Seventh",
           "Twenty Eighth",
           "Twenty Ninth",
           "Thirtieth",
           "Thirty First"
        };

            int day = dt.Day;
            int month = dt.Month;
            int year = dt.Year;
            DateTime dtm = new DateTime(1, month, 1);
            string date;

            if (isUK)
            {
                date = "The " + ordinals[day - 1] + " of " + dtm.ToString("MMMM") + " " + NumberToText(year, true);
            }
            else
            {
                date = dtm.ToString("MMMM") + " " + ordinals[day - 1] + " " + NumberToText(year, false);
            }

            if (includeTime)
            {
                int hour = dt.Hour;
                int minute = dt.Minute;
                string ap = "AM";

                if (hour >= 12)
                {
                    ap = "PM";
                    hour = hour - 12;
                }

                if (hour == 0) hour = 12;
                string time = NumberToText(hour, false);
                if (minute > 0) time += " " + NumberToText(minute, false);
                time += " " + ap;
                date += ", " + time;
            }

            return date;
        }
        public static string NumberToText(int number, bool isUK)
        {
            if (number == 0) return "Zero";
            string and = isUK ? "and " : ""; // deals with UK or US numbering
            if (number == -2147483648) return "Minus Two Billion One Hundred " + and +
            "Forty Seven Million Four Hundred " + and + "Eighty Three Thousand " +
            "Six Hundred " + and + "Forty Eight";
            int[] num = new int[4];
            int first = 0;
            int u, h, t;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (number < 0)
            {
                sb.Append("Minus ");
                number = -number;
            }
            string[] words0 = { "", "One ", "Two ", "Three ", "Four ", "Five ", "Six ", "Seven ", "Eight ", "Nine " };
            string[] words1 = { "Ten ", "Eleven ", "Twelve ", "Thirteen ", "Fourteen ", "Fifteen ", "Sixteen ", "Seventeen ", "Eighteen ", "Nineteen " };
            string[] words2 = { "Twenty ", "Thirty ", "Forty ", "Fifty ", "Sixty ", "Seventy ", "Eighty ", "Ninety " };
            string[] words3 = { "Thousand ", "Million ", "Billion " };
            num[0] = number % 1000;           // units
            num[1] = number / 1000;
            num[2] = number / 1000000;
            num[1] = num[1] - 1000 * num[2];  // thousands
            num[3] = number / 1000000000;     // billions
            num[2] = num[2] - 1000 * num[3];  // millions
            for (int i = 3; i > 0; i--)
            {
                if (num[i] != 0)
                {
                    first = i;
                    break;
                }
            }
            for (int i = first; i >= 0; i--)
            {
                if (num[i] == 0) continue;
                u = num[i] % 10;              // ones
                t = num[i] / 10;
                h = num[i] / 100;             // hundreds
                t = t - 10 * h;               // tens
                if (h > 0) sb.Append(words0[h] + "Hundred ");
                if (u > 0 || t > 0)
                {
                    if (h > 0 || i < first) sb.Append(and);
                    if (t == 0)
                        sb.Append(words0[u]);
                    else if (t == 1)
                        sb.Append(words1[u]);
                    else
                        sb.Append(words2[t - 2] + words0[u]);
                }
                if (i != 0) sb.Append(words3[i - 1]);
            }
            return sb.ToString().TrimEnd();
        }


        #region for bulk upload
        public static class Utility
        {
            public static DataTable ConvertCSVtoDataTable(string strFilePath)
            {
                DataTable dt = new DataTable();
                using (StreamReader sr = new StreamReader(strFilePath))
                {
                    string[] headers = sr.ReadLine().Split(',');
                    foreach (string header in headers)
                    {
                        dt.Columns.Add(header);
                    }

                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        if (rows.Length > 1)
                        {
                            DataRow dr = dt.NewRow();
                            for (int i = 0; i < headers.Length; i++)
                            {
                                dr[i] = rows[i].Trim();
                            }
                            dt.Rows.Add(dr);
                        }
                    }

                }


                return dt;
            }

            public static DataTable ConvertXSLXtoDataTable(string strFilePath, string connString)
            {
                OleDbConnection oledbConn = new OleDbConnection(connString);
                DataTable dt = new DataTable();
                DataSet ds = new DataSet();
                //try
                //{

                oledbConn.Open();
                using (DataTable Sheets = oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null))
                {
                    if(Sheets.Rows.Count>0)
                    {
                        var i = 0;
                        string worksheets = Sheets.Rows[i]["TABLE_NAME"].ToString();
                        OleDbCommand cmd = new OleDbCommand(String.Format("SELECT * FROM [{0}]", worksheets), oledbConn);
                        OleDbDataAdapter oleda = new OleDbDataAdapter();
                        oleda.SelectCommand = cmd;

                        oleda.Fill(ds);
                    }

                    //for (int i = 0; i < Sheets.Rows.Count; i++)
                    //{
                    //    string worksheets = Sheets.Rows[i]["TABLE_NAME"].ToString();
                    //    OleDbCommand cmd = new OleDbCommand(String.Format("SELECT * FROM [{0}]", worksheets), oledbConn);
                    //    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    //    oleda.SelectCommand = cmd;

                    //    oleda.Fill(ds);
                    //}

                    dt = ds.Tables[0];
                }

                //}
                //catch (Exception ex)
                //{
                //}
                //finally
                //{

                oledbConn.Close();
                //}

                return dt;

            }

        }
        #endregion 

    }

}