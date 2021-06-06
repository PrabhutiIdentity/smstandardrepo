using Microsoft.AspNet.SignalR;

namespace SMEnterprise.Repository
{
    public class SignalrHub
    {
    }
    public class MyHub : Hub
    {
        public MyHub()
        {

        }
        public void SendStatus(string id, int Status)
        {
            Clients.All.GetStatus(id, Status);
        }
        public void SendCanceledStatus(int id, int Status, int DeligateID)
        {
            Clients.All.GetCanceledStatus(id, Status, DeligateID);
        }
    }
}