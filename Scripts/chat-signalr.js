/**
 * Realtime Chat with SignalR
 * Replaces polling with push-based realtime messaging
 */

var ChatSignalR = (function() {
    var chatHub = null;
    var currentOrderId = null;
    var currentUserId = null;
    var isConnected = false;
    var messageCallback = null;
    
    /**
     * Initialize SignalR connection
     */
    function init(userId, onMessageReceived) {
        currentUserId = userId;
        messageCallback = onMessageReceived;
        
        // Create hub proxy
        chatHub = $.connection.chatHub;
        
        // Set up client-side methods that server can call
        setupClientMethods();
        
        // Start connection
        $.connection.hub.start()
            .done(function() {
                isConnected = true;
                console.log('[SignalR] Connected to ChatHub');
                console.log('[SignalR] Connection ID: ' + $.connection.hub.id);
            })
            .fail(function(error) {
                console.error('[SignalR] Connection failed:', error);
                isConnected = false;
            });
        
        // Handle disconnection
        $.connection.hub.disconnected(function() {
            isConnected = false;
            console.log('[SignalR] Disconnected. Attempting to reconnect...');
            
            // Auto-reconnect after 5 seconds
            setTimeout(function() {
                $.connection.hub.start();
            }, 5000);
        });
        
        // Handle reconnection
        $.connection.hub.reconnected(function() {
            isConnected = true;
            console.log('[SignalR] Reconnected');
            
            // Rejoin current room if any
            if (currentOrderId) {
                joinOrderChat(currentOrderId);
            }
        });
    }
    
    /**
     * Setup client-side methods called by server
     */
    function setupClientMethods() {
        // Receive new message
        chatHub.client.onNewMessage = function(data) {
            console.log('[SignalR] New message received:', data);
            
            // Check if message is from current user
            var isSent = (data.senderId === currentUserId);
            
            var messageData = {
                isSent: isSent,
                message: data.message,
                time: data.time
            };
            
            // Call callback to update UI
            if (messageCallback && typeof messageCallback === 'function') {
                messageCallback(messageData);
            }
        };
        
        // Handle errors from server
        chatHub.client.onError = function(errorMessage) {
            console.error('[SignalR] Server error:', errorMessage);
            if (window.alert) {
                alert('Chat error: ' + errorMessage);
            }
        };
    }
    
    /**
     * Join order chat room
     */
    function joinOrderChat(orderId) {
        if (!isConnected) {
            console.warn('[SignalR] Not connected yet. Will join when connected.');
            currentOrderId = orderId;
            return;
        }
        
        currentOrderId = orderId;
        
        chatHub.server.joinOrderChat(orderId)
            .done(function() {
                console.log('[SignalR] Joined order chat:', orderId);
                
                // Mark messages as read
                chatHub.server.markMessagesAsRead(orderId);
            })
            .fail(function(error) {
                console.error('[SignalR] Failed to join chat:', error);
            });
    }
    
    /**
     * Leave order chat room
     */
    function leaveOrderChat(orderId) {
        if (!isConnected || !chatHub) {
            return;
        }
        
        chatHub.server.leaveOrderChat(orderId)
            .done(function() {
                console.log('[SignalR] Left order chat:', orderId);
                currentOrderId = null;
            })
            .fail(function(error) {
                console.error('[SignalR] Failed to leave chat:', error);
            });
    }
    
    /**
     * Send message
     */
    function sendMessage(orderId, message) {
        if (!isConnected) {
            console.error('[SignalR] Cannot send message: not connected');
            return Promise.reject('Not connected');
        }
        
        return chatHub.server.sendMessage(orderId, message);
    }
    
    /**
     * Check if connected
     */
    function isConnectionActive() {
        return isConnected;
    }
    
    /**
     * Get connection state
     */
    function getConnectionState() {
        if (!$.connection || !$.connection.hub) {
            return 'disconnected';
        }
        
        var state = $.connection.hub.state;
        var stateNames = ['connecting', 'connected', 'reconnecting', 'disconnected'];
        return stateNames[state] || 'unknown';
    }
    
    // Public API
    return {
        init: init,
        joinOrderChat: joinOrderChat,
        leaveOrderChat: leaveOrderChat,
        sendMessage: sendMessage,
        isConnected: isConnectionActive,
        getState: getConnectionState
    };
})();

// Auto-initialize if user is logged in
$(document).ready(function() {
    // Check if USER_ID is defined (set in layout/view)
    if (typeof USER_ID !== 'undefined' && USER_ID) {
        // Define global message handler
        window.onChatMessageReceived = function(messageData) {
            // This will be overridden by specific page implementations
            console.log('[Chat] Message received (no handler):', messageData);
        };
        
        // Initialize SignalR
        ChatSignalR.init(USER_ID, function(messageData) {
            if (window.onChatMessageReceived) {
                window.onChatMessageReceived(messageData);
            }
        });
    }
});

// Expose globally
window.ChatSignalR = ChatSignalR;
