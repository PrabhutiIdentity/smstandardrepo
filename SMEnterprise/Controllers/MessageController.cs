using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMEnterprise.Models;
using SMEnterprise.Repository;
using System.IO;

namespace SMEnterprise.Controllers
{
    public class MessageController : Controller
    {
        // GET: Message
        MessageData objMessageData = new MessageData();
        public ActionResult SendMessage(MessageModel objData)
        {
            objData.SendDate = CommonUsage.GetCurrentDate();
            if (objData.Attachment != null)
            {
                objData.Attachments = objData.Attachment.FileName.Replace(" ", "_");
            }
                
           objData.MessageID= objMessageData.InsertUserMessage(objData);
            if(objData.Attachment!=null)
            {
                var path = Path.Combine(Server.MapPath(CommonUsage.MessageAttachmentUploadBasePath),
                 objData.MessageID + "_" + objData.Attachments);
                objData.Attachment.SaveAs(path);
            }
           return Redirect(objData.SenderUrl);
        }
    }
}