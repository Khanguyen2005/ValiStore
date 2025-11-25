/**
 * Customer Chat with SignalR (for Order Details page)
 * Realtime messaging between customer and shipper
 */

var CustomerChat = (function() {
    var messagesContainer;
    var inputField;
    var isCompleted = false;
    
    /**
     * Initialize chat when modal opens
     */
    function init(orderId, orderStatus, userId) {
        messagesContainer = document.getElementById('chatMessages');
        inputField = document.getElementById('chatInput');
        isCompleted = (orderStatus === 'Completed');
        
        // Load initial messages
        loadInitialMessages(orderId);
        
        // Join SignalR room
        if (window.ChatSignalR && ChatSignalR.isConnected()) {
            ChatSignalR.joinOrderChat(orderId);
        }
        
        // Setup message received handler
        window.onChatMessageReceived = function(messageData) {
            // Only update if modal is open
            if (messagesContainer && $('#chatModal').hasClass('show')) {
                addMessageToUI(messageData);
            }
        };
        
        // Enter key to send
        if (inputField && !isCompleted) {
            inputField.addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    sendMessage(orderId);
                }
            });
        }
    }
    
    /**
     * Cleanup when modal closes
     */
    function cleanup(orderId) {
        if (window.ChatSignalR) {
            ChatSignalR.leaveOrderChat(orderId);
        }
        window.onChatMessageReceived = null;
        messagesContainer = null;
        inputField = null;
    }
    
    /**
     * Load initial messages from server
     */
    function loadInitialMessages(orderId) {
        if (!messagesContainer) return;
        
        // Show loading
        messagesContainer.innerHTML = '<div class="text-center text-muted py-4"><div class="spinner-border spinner-border-sm me-2"></div>Loading messages...</div>';
        
        $.get('/Order/GetCustomerMessages', { orderId: orderId })
            .done(function(messages) {
                if (!messagesContainer) return;
                
                messagesContainer.innerHTML = '';
                
                if (messages && messages.length > 0) {
                    messages.forEach(function(msg) {
                        addMessageToUI(msg);
                    });
                    scrollToBottom();
                } else {
                    var emptyMessage = isCompleted ? 
                        '<div class="text-center text-muted py-4"><i class="bi bi-chat-dots fs-2 mb-2"></i><p>No messages in this conversation</p></div>' :
                        '<div class="text-center text-muted py-4"><i class="bi bi-chat-dots fs-2 mb-2"></i><p>No messages yet. Start a conversation!</p></div>';
                    messagesContainer.innerHTML = emptyMessage;
                }
            })
            .fail(function() {
                if (messagesContainer) {
                    messagesContainer.innerHTML = '<div class="text-center text-danger py-4"><i class="bi bi-exclamation-triangle fs-2 mb-2"></i><p>Failed to load messages</p></div>';
                }
            });
    }
    
    /**
     * Add message to UI
     */
    function addMessageToUI(msg) {
        if (!messagesContainer) return;
        
        // Remove empty state if exists
        var emptyState = messagesContainer.querySelector('.text-center');
        if (emptyState) {
            messagesContainer.innerHTML = '';
        }
        
        var messageClass = msg.isSent ? 'customer-message' : 'shipper-message';
        var alignClass = msg.isSent ? 'text-end' : 'text-start';
        var bgColor = msg.isSent ? 'background: #0dcaf0; color: white;' : 'background: #e9ecef;';
        
        var messageHtml = '<div class="mb-2 ' + alignClass + '">';
        messageHtml += '<div class="d-inline-block p-2 rounded ' + messageClass + '" style="max-width: 70%; ' + bgColor + '">';
        messageHtml += '<div>' + escapeHtml(msg.message) + '</div>';
        messageHtml += '<small class="d-block" style="font-size: 0.75rem; opacity: 0.8;">' + msg.time + '</small>';
        messageHtml += '</div></div>';
        
        // Check if user was at bottom before adding
        var wasAtBottom = isScrolledToBottom();
        
        messagesContainer.insertAdjacentHTML('beforeend', messageHtml);
        
        // Auto-scroll if user was at bottom
        if (wasAtBottom) {
            scrollToBottom();
        }
    }
    
    /**
     * Send message via SignalR
     */
    function sendMessage(orderId) {
        if (isCompleted) {
            alert('This order is completed. You cannot send new messages.');
            return;
        }
        
        if (!inputField) return;
        
        var message = inputField.value.trim();
        if (!message) return;
        
        if (!window.ChatSignalR || !ChatSignalR.isConnected()) {
            alert('Chat is not connected. Please refresh the page.');
            return;
        }
        
        // Disable input during send
        inputField.disabled = true;
        
        // Send via SignalR
        ChatSignalR.sendMessage(orderId, message)
            .done(function() {
                inputField.value = '';
                inputField.disabled = false;
                inputField.focus();
                
                // Message will be added via SignalR callback
            })
            .fail(function(error) {
                console.error('Failed to send message:', error);
                alert('Failed to send message. Please try again.');
                inputField.disabled = false;
            });
    }
    
    /**
     * Helper functions
     */
    function isScrolledToBottom() {
        if (!messagesContainer) return true;
        return messagesContainer.scrollHeight - messagesContainer.scrollTop <= messagesContainer.clientHeight + 50;
    }
    
    function scrollToBottom() {
        if (messagesContainer) {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
    }
    
    function escapeHtml(text) {
        var map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return String(text).replace(/[&<>"']/g, function(m) { return map[m]; });
    }
    
    // Public API
    return {
        init: init,
        cleanup: cleanup,
        sendMessage: sendMessage
    };
})();

// Expose globally
window.CustomerChat = CustomerChat;
