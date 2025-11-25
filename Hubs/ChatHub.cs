using Microsoft.AspNet.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using ValiModern.Models.EF;

namespace ValiModern.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time chat between customers and shippers
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        /// <summary>
        /// Get current user ID from authentication
        /// </summary>
        private int? GetCurrentUserId()
        {
            if (int.TryParse(Context.User.Identity.Name, out int userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Join a specific order chat room
        /// Called automatically when user opens chat modal
        /// </summary>
        public async Task JoinOrderChat(int orderId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return;
            }

            // Verify user has access to this order
            var order = _db.Orders.Find(orderId);
            if (order == null)
            {
                return;
            }

            // Check if user is customer or shipper for this order
            bool hasAccess = (order.user_id == userId.Value) || (order.shipper_id == userId.Value);
            
            if (!hasAccess)
            {
                return; // Unauthorized
            }

            // Join the room
            string roomName = "order_" + orderId;
            await Groups.Add(Context.ConnectionId, roomName);

            System.Diagnostics.Debug.WriteLine($"[ChatHub] User {userId} joined room {roomName}");
        }

        /// <summary>
        /// Leave order chat room
        /// Called when modal closes
        /// </summary>
        public async Task LeaveOrderChat(int orderId)
        {
            string roomName = "order_" + orderId;
            await Groups.Remove(Context.ConnectionId, roomName);

            var userId = GetCurrentUserId();
            System.Diagnostics.Debug.WriteLine($"[ChatHub] User {userId} left room {roomName}");
        }

        /// <summary>
        /// Send message to order chat room
        /// </summary>
        public async Task SendMessage(int orderId, string message)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            // Validate message length
            if (message.Length > 500)
            {
                message = message.Substring(0, 500);
            }

            try
            {
                // Get order and validate access
                var order = _db.Orders.Find(orderId);
                if (order == null)
                {
                    await Clients.Caller.onError("Order not found");
                    return;
                }

                // CHECK: Prevent sending messages if order is completed
                if (order.status == "Completed")
                {
                    await Clients.Caller.onError("This order is completed. Chat is closed.");
                    System.Diagnostics.Debug.WriteLine($"[ChatHub] Rejected message for completed order #{orderId}");
                    return;
                }

                bool isCustomer = order.user_id == userId.Value;
                bool isShipper = order.shipper_id == userId.Value;

                if (!isCustomer && !isShipper)
                {
                    await Clients.Caller.onError("Unauthorized");
                    return;
                }

                // Determine receiver
                int receiverId = isCustomer ? (order.shipper_id ?? 0) : order.user_id;

                if (receiverId == 0)
                {
                    await Clients.Caller.onError("Receiver not found");
                    return;
                }

                // Save message to database
                var newMessage = new Message
                {
                    order_id = orderId,
                    sender_id = userId.Value,
                    receiver_id = receiverId,
                    message1 = message.Trim(),
                    created_at = DateTime.Now,
                    is_read = false
                };

                _db.Messages.Add(newMessage);
                _db.SaveChanges();

                // Update unread message counters on order
                if (isCustomer)
                {
                    // Customer sent, increment shipper's unread count
                    order.shipper_unread_messages = (order.shipper_unread_messages ?? 0) + 1;
                }
                else
                {
                    // Shipper sent, increment customer's unread count
                    order.customer_unread_messages = (order.customer_unread_messages ?? 0) + 1;
                }
                _db.SaveChanges();

                // Broadcast to all users in the room (including sender for confirmation)
                string roomName = "order_" + orderId;
                await Clients.Group(roomName).onNewMessage(new
                {
                    isSent = false, // Will be true for sender on client side
                    message = message.Trim(),
                    time = DateTime.Now.ToString("HH:mm"),
                    senderId = userId.Value
                });

                System.Diagnostics.Debug.WriteLine($"[ChatHub] Message sent to room {roomName} by user {userId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatHub] Error: {ex.Message}");
                await Clients.Caller.onError("Failed to send message");
            }
        }

        /// <summary>
        /// Mark messages as read when user opens/views chat
        /// </summary>
        public void MarkMessagesAsRead(int orderId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return;
            }

            try
            {
                // Mark all messages from the other user as read
                var messages = _db.Messages
                    .Where(m => m.order_id == orderId && m.receiver_id == userId.Value && (m.is_read == false || !m.is_read.HasValue))
                    .ToList();

                foreach (var msg in messages)
                {
                    msg.is_read = true;
                }

                if (messages.Any())
                {
                    // Reset unread counter
                    var order = _db.Orders.Find(orderId);
                    if (order != null)
                    {
                        if (order.user_id == userId.Value)
                        {
                            order.customer_unread_messages = 0;
                        }
                        else if (order.shipper_id == userId.Value)
                        {
                            order.shipper_unread_messages = 0;
                        }
                    }

                    _db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"[ChatHub] Marked {messages.Count} messages as read for order {orderId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatHub] Error marking messages as read: {ex.Message}");
            }
        }

        /// <summary>
        /// Connection lifecycle
        /// </summary>
        public override Task OnConnected()
        {
            var userId = GetCurrentUserId();
            System.Diagnostics.Debug.WriteLine($"[ChatHub] User {userId} connected (ConnectionId: {Context.ConnectionId})");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userId = GetCurrentUserId();
            System.Diagnostics.Debug.WriteLine($"[ChatHub] User {userId} disconnected (ConnectionId: {Context.ConnectionId})");
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userId = GetCurrentUserId();
            System.Diagnostics.Debug.WriteLine($"[ChatHub] User {userId} reconnected (ConnectionId: {Context.ConnectionId})");
            return base.OnReconnected();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
