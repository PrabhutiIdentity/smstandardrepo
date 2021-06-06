using SMEnterprise.Models;
using SMEnterprise.Repository;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SMEnterprise.Controllers
{
    public class FileUploadApiController : ApiController
    {
        // GET: FileUpload
        [HttpPost]
        public async Task<CommonApiWraperModel> UploadAttachment()
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            // Check if the request contains multipart/form-data.  
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());
                //access form data  
                NameValueCollection formData = provider.FormData;
                //access files  
                IList<HttpContent> files = provider.Files;

                if (formData["ID"] != null)
                {
                    HttpContent file1 = files[0];

                    string filename = String.Empty;
                    Stream input = await file1.ReadAsStreamAsync();
                    string directoryName = String.Empty;
                    string URL = String.Empty;
                    var thisFileName = file1.Headers.ContentDisposition.FileName.Trim('\"');
                    string ID = formData["ID"];
                    int Type = CommonUsage.ConvertToInt(formData["Type"]);
                    var path = HttpRuntime.AppDomainAppPath;
                    if (Type == 0)
                    {
                        directoryName = System.IO.Path.Combine(path, "Attachments\\Assignments");
                    }
                    else if (Type == 1)
                    {
                        directoryName = System.IO.Path.Combine(path, "Images\\BlackBoard");
                    }
                    else
                    {
                        directoryName = System.IO.Path.Combine(path, "Attachments");
                    }

                    filename = System.IO.Path.Combine(directoryName, ID + "_" + thisFileName);

                    //Deletion exists file  
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }

                    //Directory.CreateDirectory(@directoryName);  
                    using (Stream file = File.OpenWrite(filename))
                    {
                        input.CopyTo(file);
                        //close file  
                        file.Close();
                    }
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    objWraper.Code = 200;
                    objWraper.Message = "Success";
                    objWraper.Data = directoryName + "\\" + ID + "_" + thisFileName;
                }
                else
                {
                    objWraper.Code = 404;
                    objWraper.Message = "Not Uploaded " + formData["ID"];
                }
            }
            catch (Exception ex)
            {
                objWraper.Code = 1;
                objWraper.Message = ex.ToString();

            }
            return objWraper;
        }
        [HttpPost]
        [Route("api/ParentApi/UploadAttachment")]
        public async Task<CommonApiWraperModel> UploadAttachmentParent()
        {
            CommonApiWraperModel objWraper = new CommonApiWraperModel();
            // Check if the request contains multipart/form-data.  
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());
                //access form data  
                NameValueCollection formData = provider.FormData;
                //access files  
                IList<HttpContent> files = provider.Files;

                if (formData["ID"] != null)
                {
                    HttpContent file1 = files[0];

                    string filename = String.Empty;
                    Stream input = await file1.ReadAsStreamAsync();
                    string directoryName = String.Empty;
                    string URL = String.Empty;
                    var thisFileName = file1.Headers.ContentDisposition.FileName.Trim('\"');
                    string ID = formData["ID"];
                    var path = HttpRuntime.AppDomainAppPath;
                    directoryName = System.IO.Path.Combine(path, "Attachments\\AssignmentSubmissions");

                    thisFileName = CommonUsage.GetValidFileName(thisFileName);
                    filename = System.IO.Path.Combine(directoryName, ID + "_" + thisFileName);

                    //Deletion exists file  
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }

                    //Directory.CreateDirectory(@directoryName);  
                    using (Stream file = File.OpenWrite(filename))
                    {
                        input.CopyTo(file);
                        //close file  
                        file.Close();
                    }
                    (new ParentData()).UpdateAssignmentResponseAttachment(ID, thisFileName);
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    objWraper.Code = 200;
                    objWraper.Message = "Success";
                    objWraper.Data = "Attachments/AssignmentSubmissions/" + ID + "_" + thisFileName;
                }
                else
                {
                    objWraper.Code = 404;
                    objWraper.Message = "Not Uploaded " + formData["ID"];
                }
            }
            catch (Exception ex)
            {
                objWraper.Code = 1;
                objWraper.Message = ex.ToString();

            }
            return objWraper;
        }
    }
}