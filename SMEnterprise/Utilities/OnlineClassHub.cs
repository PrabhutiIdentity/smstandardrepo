using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMEnterprise.Utilities
{
    public class OnlineClassHub : Hub
    {
        public async Task OnlineClassEnded(string group, string MeetingID)
        {
            await Clients.Group(group).SendAsync("OnlineClassEnded", MeetingID);
        }
        public async Task OnlineClassStarted(string group, string MeetingID)
        {
            await Clients.Group(group).SendAsync("OnlineClassStarted", MeetingID);
        }
        public async Task RecordingReady(string user, string MeetingID)
        {
            await Clients.All.SendAsync("OnlineClassRecordingReady", user, MeetingID);
        }
        public async Task AddToGroup(string groupName)
        {
            await Groups.Add(Context.ConnectionId, groupName);

            //await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.Remove(Context.ConnectionId, groupName);

            //await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }
        public void SendToGroup(GroupMessage message)
        {
            // Call the addMessage method on all clients            
            //Clients.All.addMessage(message.Message);
            Clients.Group(message.Group).addMessage("Group Message " + message.Message);
           
        }
    }
    public class GroupMessage
    {
        public string Message { get; set; }
        public string Group { get; set; }
    }
}
