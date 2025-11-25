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

// Chat System
let chatModal;
let chatRefreshInterval;

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
    
    chatModal = new bootstrap.Modal(modalElement);
    chatModal.show();
    
    loadMessages();
    
    // Refresh messages every 3 seconds
    chatRefreshInterval = setInterval(loadMessages, 3000);
    
    // Clear interval when modal closes
    modalElement.addEventListener('hidden.bs.modal', function() {
        if (chatRefreshInterval) {
            clearInterval(chatRefreshInterval);
        }
    });
    
    // Enter key to send
    const chatInput = document.getElementById('chatInput');
    if (chatInput) {
        chatInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });
    }
}

function loadMessages() {
    if (!window.ORDER_ID) return;
    
    fetch(`/Shipper/GetMessages?orderId=${window.ORDER_ID}`)
        .then(response => {
            if (!response.ok) throw new Error('Failed to load messages');
            return response.json();
        })
        .then(messages => {
            const container = document.getElementById('chatMessages');
            if (!container) return;
            
            if (!messages || messages.length === 0) {
                container.innerHTML = '<div class="text-center text-muted"><i class="bi bi-chat-dots fs-2"></i><p>No messages yet. Start the conversation!</p></div>';
                return;
            }
            
            container.innerHTML = '';
            messages.forEach(msg => {
                const div = document.createElement('div');
                div.className = `chat-message ${msg.isSent ? 'sent' : 'received'}`;
                div.innerHTML = `
                    <div>${escapeHtml(msg.message)}</div>
                    <div class="chat-message-time">${msg.time}</div>
                `;
                container.appendChild(div);
            });
            
            // Scroll to bottom
            container.scrollTop = container.scrollHeight;
        })
        .catch(error => {
            console.error('Error loading messages:', error);
            const container = document.getElementById('chatMessages');
            if (container) {
                container.innerHTML = '<div class="text-center text-danger"><p>Failed to load messages</p></div>';
            }
        });
}

function sendMessage() {
    const input = document.getElementById('chatInput');
    if (!input) return;
    
    const message = input.value.trim();
    if (!message) return;
    
    if (!window.ORDER_ID) {
        alert('Order ID not found');
        return;
    }
    
    // Get CSRF token
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    
    fetch(`/Shipper/SendMessage`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token ? token.value : ''
        },
        body: JSON.stringify({
            orderId: window.ORDER_ID,
            message: message
        })
    })
    .then(response => {
        if (!response.ok) throw new Error('Failed to send message');
        return response.json();
    })
    .then(result => {
        if (result.success) {
            input.value = '';
            loadMessages();
        } else {
            alert('Failed to send message: ' + (result.message || 'Unknown error'));
        }
    })
    .catch(error => {
        console.error('Error sending message:', error);
        alert('Error sending message. Please try again.');
    });
}

// Utility: Escape HTML to prevent XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Expose functions globally
window.copyAddress = copyAddress;
window.previewPhotos = previewPhotos;
window.removePhoto = removePhoto;
window.openChat = openChat;
window.sendMessage = sendMessage;
window.startDeliveryTimer = startDeliveryTimer;
window.startDeliveryProgressAnimation = startDeliveryProgressAnimation;
