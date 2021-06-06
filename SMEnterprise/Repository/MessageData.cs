using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using SMEnterprise.Models;
using System.Data.SqlClient;
using System.Data;

namespace SMEnterprise.Repository
{
    public class MessageData
    {
        #region MailBox
        public int InsertUserMessage(MessageModel objData)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@MessageBody", objData.MessageBody);
                paramater.Add("@SenderID", objData.SenderID);
                paramater.Add("@SenderType", objData.SenderType);
                paramater.Add("@RecieverID", objData.RecieverID);
                paramater.Add("@RecieverType", objData.RecieverType);
                paramater.Add("@SendDate", objData.SendDate);
                paramater.Add("@ParentMessageID", objData.ParentMessageID);
                paramater.Add("@MainMailThreadID", objData.MainMailThreadID);
                paramater.Add("@Attachments", objData.Attachments);
                paramater.Add("@MessageTitle", objData.MessageTitle);

               return con.Query<int>("spn_AddUserMessage", paramater, null, true, 0, commandType: CommandType.StoredProcedure).SingleOrDefault();
             
            }
        }
        public MailBoxModel GetMailBoxModel(int RecieverType, int ID,int ListType)
        {
            MailBoxModel objModel = new MailBoxModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RecieverType", RecieverType);
                paramater.Add("@ID", ID);
                paramater.Add("@ListType", ListType);
                using (var multi = con.QueryMultiple("spn_GetMessages", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.UnreadCount = multi.Read<int>().SingleOrDefault();
                    objModel.Mails = multi.Read<MessageModel>().ToList();
                    objModel.Mail = multi.Read<MessageModel>().SingleOrDefault();
                    objModel.RecieverList = multi.Read<NameIDModel>().ToList();
                    if (objModel.Mail == null)
                    {
                        objModel.Mail = new MessageModel();
                        objModel.Mail.CurrUserID = ID;
                        objModel.Mail.CurrUserType = RecieverType;
                    }
                }
            }
            return objModel;
        }
        public MailBoxModel GetSentMailBoxModel(int SenderType, int ID, int ListType)
        {
            MailBoxModel objModel = new MailBoxModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@SenderType", SenderType);
                paramater.Add("@ID", ID);
                paramater.Add("@ListType", ListType);
                using (var multi = con.QueryMultiple("spn_GetSentMessages", paramater, null, 0, commandType: CommandType.StoredProcedure))
                {
                    objModel.UnreadCount = multi.Read<int>().SingleOrDefault();
                    objModel.Mails = multi.Read<MessageModel>().ToList();
                    objModel.Mail = multi.Read<MessageModel>().SingleOrDefault();
                    objModel.RecieverList = multi.Read<NameIDModel>().ToList();
                    if(objModel.Mail==null)
                    {
                        objModel.Mail = new MessageModel();
                        objModel.Mail.CurrUserID = ID;
                        objModel.Mail.CurrUserType = SenderType;
                    }
                }
            }
            return objModel;
        }
        public MessageModel GetMessageDetails(int MessageID,int IsSent)
        {
            MessageModel objModel = new MessageModel();
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@ID", MessageID);
                paramater.Add("@CurrDate", CommonUsage.GetCurrentDate());
                paramater.Add("@IsSent", IsSent);
                objModel = con.Query<MessageModel>("spn_GetMessageDetails", paramater, null, true, 0, CommandType.StoredProcedure).SingleOrDefault();
            }

            return objModel;
        }
        public List<NameIDModel> GetRecieverList(int RecieverType, int SenderID, int SenderType)
        {
            using (SqlConnection con = new SqlConnection(CommonUsage.ConnectionString))
            {
                var paramater = new DynamicParameters();
                paramater.Add("@RecieverType", RecieverType);
                paramater.Add("@ID", SenderID);
                paramater.Add("@SenderType", SenderType);
                return con.Query<NameIDModel>("spn_GetMessageRecieverList", paramater, null, true, 0, CommandType.StoredProcedure).ToList();
            }
        }
        #endregion
    }
}