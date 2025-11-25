// Shipper Details Page JavaScript

// Delivery Timer
function startDeliveryTimer(startTimeStr) {
    const startTime = new Date(startTimeStr);
    const timerElement = document.getElementById('deliveryTimer');
    
    if (!timerElement) return;
    
    function updateTimer() {
        const now = new Date();
        const diff = now - startTime;
        
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((diff % (1000 * 60)) / 1000);
        
        timerElement.textContent = 
            hours.toString().padStart(2, '0') + ':' +
            minutes.toString().padStart(2, '0') + ':' +
            seconds.toString().padStart(2, '0');
    }
    
    updateTimer();
    setInterval(updateTimer, 1000);
}

// Delivery Progress Animation (3 seconds loop)
function startDeliveryProgressAnimation() {
    const progressFill = document.getElementById('progressFill');
    const progressTruck = document.getElementById('progressTruck');
    const statusText = document.getElementById('statusText');
    
    if (!progressFill || !progressTruck || !statusText) return;
    
    const stages = [
        { progress: 0, status: 'Preparing delivery...', duration: 0 },
        { progress: 25, status: 'Picked up from warehouse', duration: 500 },
        { progress: 50, status: 'In transit to customer', duration: 1000 },
        { progress: 75, status: 'Approaching destination', duration: 1500 },
        { progress: 100, status: 'Arriving soon...', duration: 2000 }
    ];
    
    let currentStage = 0;
    
    function animateProgress() {
        if (currentStage >= stages.length) {
            currentStage = 0; // Loop back
        }
        
        const stage = stages[currentStage];
        
        setTimeout(() => {
            progressFill.style.width = stage.progress + '%';
            progressTruck.style.left = stage.progress + '%';
            statusText.textContent = stage.status;
            
            currentStage++;
            animateProgress();
        }, stage.duration);
    }
    
    animateProgress();
}

// Copy Address
function copyAddress() {
    if (!window.DELIVERY_ADDRESS) {
        console.error('Delivery address not defined');
        return;
    }
    
    navigator.clipboard.writeText(window.DELIVERY_ADDRESS).then(() => {
        if (window.ShipperLayout && window.ShipperLayout.showNotification) {
            window.ShipperLayout.showNotification('Address copied to clipboard!', 'success');
        } else {
            alert('Address copied!');
        }
    }).catch(err => {
        console.error('Failed to copy:', err);
        alert('Failed to copy address');
    });
}

// Photo Preview
let selectedFiles = [];

function previewPhotos(event) {
    const files = Array.from(event.target.files);
    selectedFiles = files;
    const container = document.getElementById('photoPreview');
    
    if (!container) return;
    
    container.innerHTML = '';
    
    files.forEach((file, index) => {
        if (!file.type.startsWith('image/')) return;
        
        const reader = new FileReader();
        reader.onload = function(e) {
            const div = document.createElement('div');
            div.className = 'photo-upload-preview';
            div.innerHTML = `
                <img src="${e.target.result}" alt="Preview ${index + 1}" />
                <button type="button" class="remove-photo" onclick="removePhoto(${index})" title="Remove">
                    ×
                </button>
            `;
            container.appendChild(div);
        };
        reader.readAsDataURL(file);
    });
}

function removePhoto(index) {
    selectedFiles.splice(index, 1);
    
    // Update file input
    const dataTransfer = new DataTransfer();
    selectedFiles.forEach(file => dataTransfer.items.add(file));
    const fileInput = document.querySelector('input[name="deliveryPhotos"]');
    if (fileInput) {
        fileInput.files = dataTransfer.files;
        previewPhotos({ target: fileInput });
    }
}

// ==============================================================
// REALTIME CHAT WITH SIGNALR (No more polling!)
// ==============================================================

let chatModal;
let messagesContainer;

function openChat() {
    if (!window.ORDER_ID) {
        alert('Order ID not found');
        return;
    }
    
    const modalElement = document.getElementById('chatModal');
    if (!modalElement) {
        alert('Chat modal not found');
        return;
    }
    
    messagesContainer = document.getElementById('chatMessages');
    
    chatModal = new bootstrap.Modal(modalElement);
    chatModal.show();
    
    // Load initial messages from server
    loadMessages();
    
    // Join SignalR chat room
    if (window.ChatSignalR && ChatSignalR.isConnected()) {
        ChatSignalR.joinOrderChat(window.ORDER_ID);
    }
    
    // Setup message received handler
    window.onChatMessageReceived = function(messageData) {
        // Only update if this is for current order
        if (messagesContainer && $('#chatModal').hasClass('show')) {
            addMessageToUI(messageData);
        }
    };
    
    // Cleanup when modal closes
    modalElement.addEventListener('hidden.bs.modal', function() {
        // Leave SignalR room
        if (window.ChatSignalR) {
            ChatSignalR.leaveOrderChat(window.ORDER_ID);
        }
        window.onChatMessageReceived = null;
        messagesContainer = null;
    }, { once: true });
    
    // Enter key to send
    const chatInput = document.getElementById('chatInput');
    if (chatInput) {
        chatInput.removeEventListener('keypress', handleChatKeypress);
        chatInput.addEventListener('keypress', handleChatKeypress);
    }
}

function handleChatKeypress(e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        sendMessage();
    }
}

// Load initial messages (called once when modal opens)
function loadMessages() {
    if (!window.ORDER_ID) return;
    
    fetch('/Shipper/GetMessages?orderId=' + window.ORDER_ID)
        .then(response => {
            if (!response.ok) throw new Error('Failed to load messages');
            return response.json();
        })
        .then(messages => {
            if (!messagesContainer) return;
            
            messagesContainer.innerHTML = '';
            
            if (!messages || messages.length === 0) {
                messagesContainer.innerHTML = '<div class="text-center text-muted"><i class="bi bi-chat-dots fs-2"></i><p>No messages yet. Start the conversation!</p></div>';
            } else {
                messages.forEach(msg => {
                    addMessageToUI(msg);
                });
                scrollToBottom(messagesContainer);
            }
        })
        .catch(error => {
            console.error('Error loading messages:', error);
            if (messagesContainer) {
                messagesContainer.innerHTML = '<div class="text-center text-danger"><p>Failed to load messages</p></div>';
            }
        });
}

// Add message to UI (called for both initial load and realtime updates)
function addMessageToUI(msg) {
    if (!messagesContainer) return;
    
    var messageClass = msg.isSent ? 'sent' : 'received';
    var messageHtml = '<div class="chat-message ' + messageClass + '">' +
                      '<div>' + escapeHtml(msg.message) + '</div>' +
                      '<div class="chat-message-time">' + msg.time + '</div>' +
                      '</div>';
    
    // Check if user was at bottom before adding
    var wasAtBottom = isScrolledToBottom(messagesContainer);
    
    // Remove empty state if exists
    var emptyState = messagesContainer.querySelector('.text-center');
    if (emptyState) {
        messagesContainer.innerHTML = '';
    }
    
    // Add new message
    messagesContainer.insertAdjacentHTML('beforeend', messageHtml);
    
    // Auto-scroll if user was at bottom
    if (wasAtBottom) {
        scrollToBottom(messagesContainer);
    }
}

// Helper: Check if scrolled to bottom
function isScrolledToBottom(element) {
    if (!element) return true;
    return element.scrollHeight - element.scrollTop <= element.clientHeight + 50;
}

// Helper: Scroll to bottom
function scrollToBottom(element) {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
}

// Send message via SignalR
function sendMessage() {
    const input = document.getElementById('chatInput');
    if (!input) return;
    
    const message = input.value.trim();
    if (!message) return;
    
    if (!window.ORDER_ID) {
        alert('Order ID not found');
        return;
    }
    
    // Check SignalR connection
    if (!window.ChatSignalR || !ChatSignalR.isConnected()) {
        alert('Chat is not connected. Please refresh the page.');
        return;
    }
    
    // Disable input during send
    input.disabled = true;
    
    // Send via SignalR
    ChatSignalR.sendMessage(window.ORDER_ID, message)
        .done(function() {
            // Success
            input.value = '';
            input.disabled = false;
            input.focus();
            
            // Message will be added to UI via SignalR callback
        })
        .fail(function(error) {
            console.error('Failed to send message:', error);
            alert('Failed to send message. Please try again.');
            input.disabled = false;
        });
}

// Utility: Escape HTML to prevent XSS
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

// Expose functions globally
window.copyAddress = copyAddress;
window.previewPhotos = previewPhotos;
window.removePhoto = removePhoto;
window.openChat = openChat;
window.sendMessage = sendMessage;
window.startDeliveryTimer = startDeliveryTimer;
window.startDeliveryProgressAnimation = startDeliveryProgressAnimation;
