using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Hubs;
using Data.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Hubs
{
    public interface IChatHub
    {
        Task SendMessage(string userId, string senderId, MessageViewModel message);
        Task ConversationPushing(string firstUser, string secondUser, ConversationPushingViewModel model);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub, IChatHub
    {
        public static ConcurrentDictionary<string, List<string>> ConnectedUsers = new ConcurrentDictionary<string, List<string>>();
        public IHubContext<ChatHub> Current { get; set; }

        public ChatHub(IHubContext<ChatHub> current)
        {
            Current = current;
        }

        public async Task SendMessage(string userId, string senderId, MessageViewModel message)
        {
            List<string> ReceiverConnectionids;
            ConnectedUsers.TryGetValue(userId, out ReceiverConnectionids);

            List<string> SenderConnectionids;
            ConnectedUsers.TryGetValue(senderId, out SenderConnectionids);

            try
            {
                await Current.Clients.Clients(ReceiverConnectionids).SendAsync("ReceiveMessage", message);
            }
            catch (Exception) { }

            try
            {
                message.YourMessage = true;
                await Current.Clients.Clients(SenderConnectionids).SendAsync("ReceiveMessage", message);
            }
            catch (Exception) { }

        }

        public async Task ConversationPushing(string sender, string reciever, ConversationPushingViewModel model)
        {
            List<string> SenderConnectionids;
            ConnectedUsers.TryGetValue(sender, out SenderConnectionids);

            if (SenderConnectionids != null)
            {
                try
                {
                    model.YourMessage = true;
                    await Current.Clients.Clients(SenderConnectionids).SendAsync("ConversationPushing", model);
                }
                catch (Exception) { }
            }

            List<string> ReceiverConnectionids;
            ConnectedUsers.TryGetValue(reciever, out ReceiverConnectionids);

            if (ReceiverConnectionids != null)
            {
                try
                {
                    model.YourMessage = false;
                    await Current.Clients.Clients(ReceiverConnectionids).SendAsync("ConversationPushing", model);
                }
                catch (Exception) { }
            }

        }

        public override Task OnConnectedAsync()
        {
            Trace.TraceInformation("MapHub started. ID: {0}", Context.ConnectionId);

            // Try to get a List of existing user connections from the cache
            List<string> existingUserConnectionIds;
            ConnectedUsers.TryGetValue(Context.User.GetId(), out existingUserConnectionIds);

            // happens on the very first connection from the user
            if (existingUserConnectionIds == null)
            {
                existingUserConnectionIds = new List<string>();
            }

            // First add to a List of existing user connections (i.e. multiple web browser tabs)
            existingUserConnectionIds.Add(Context.ConnectionId);

            // Add to the global dictionary of connected users
            ConnectedUsers.TryAdd(Context.User.GetId(), existingUserConnectionIds);
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            List<string> existingUserConnectionIds;
            ConnectedUsers.TryGetValue(Context.User.GetId(), out existingUserConnectionIds);

            // remove the connection id from the List 
            existingUserConnectionIds.Remove(Context.ConnectionId);

            // If there are no connection ids in the List, delete the user from the global cache (ConnectedUsers).
            if (existingUserConnectionIds.Count == 0)
            {
                // if there are no connections for the user,
                // just delete the userName key from the ConnectedUsers concurent dictionary
                List<string> garbage; // to be collected by the Garbage Collector
                ConnectedUsers.TryRemove(Context.User.GetId(), out garbage);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
