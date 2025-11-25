// ============================================
// ValiModern Shipper Layout JavaScript
// Interactive features for delivery portal
// ============================================

(function() {
    'use strict';

    // ============================================
    // Initialize on DOM Ready
    // ============================================
    document.addEventListener('DOMContentLoaded', function() {
        initializeNavbarEffects();
        initializeBellNotification();
        initializeDropdownEffects();
        initializeAlertAnimations();
    });

    // ============================================
    // Navbar Scroll Effect
    // ============================================
    function initializeNavbarEffects() {
        const navbar = document.querySelector('.shipper-navbar');
        if (!navbar) return;

        let lastScroll = 0;
        
        window.addEventListener('scroll', function() {
            const currentScroll = window.pageYOffset;
            
            if (currentScroll <= 0) {
                navbar.style.boxShadow = '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)';
            } else {
                navbar.style.boxShadow = '0 20px 25px -5px rgba(0, 0, 0, 0.15), 0 10px 10px -5px rgba(0, 0, 0, 0.04)';
            }
            
            lastScroll = currentScroll;
        });
    }

    // ============================================
    // Bell Notification Interactive
    // ============================================
    function initializeBellNotification() {
        const bellWrapper = document.querySelector('.bell-wrapper');
        if (!bellWrapper) return;

        bellWrapper.addEventListener('click', function(e) {
            const badge = this.querySelector('.bell-badge');
            if (badge) {
                // Add a click animation
                badge.style.animation = 'none';
                setTimeout(() => {
                    badge.style.animation = 'bounce 0.5s ease';
                }, 10);
            }
        });

        // Periodic attention animation
        const badge = bellWrapper.querySelector('.bell-badge');
        if (badge) {
            setInterval(function() {
                const bellIcon = bellWrapper.querySelector('i');
                if (bellIcon) {
                    bellIcon.style.animation = 'ring 0.5s ease';
                    setTimeout(() => {
                        bellIcon.style.animation = '';
                    }, 500);
                }
            }, 30000); // Every 30 seconds
        }
    }

    // ============================================
    // Dropdown Smooth Effects
    // ============================================
    function initializeDropdownEffects() {
        const dropdownToggle = document.querySelector('#shipperMenu');
        if (!dropdownToggle) return;

        dropdownToggle.addEventListener('show.bs.dropdown', function() {
            const dropdown = this.nextElementSibling;
            if (dropdown) {
                dropdown.style.animation = 'dropdownSlide 0.3s ease';
            }
        });

        // Add ripple effect to dropdown items
        const dropdownItems = document.querySelectorAll('.dropdown-item');
        dropdownItems.forEach(function(item) {
            item.addEventListener('click', function(e) {
                const ripple = document.createElement('span');
                ripple.style.position = 'absolute';
                ripple.style.borderRadius = '50%';
                ripple.style.background = 'rgba(16, 185, 129, 0.3)';
                ripple.style.width = '20px';
                ripple.style.height = '20px';
                ripple.style.marginLeft = '-10px';
                ripple.style.marginTop = '-10px';
                ripple.style.left = e.clientX - this.offsetLeft + 'px';
                ripple.style.top = e.clientY - this.offsetTop + 'px';
                ripple.style.animation = 'ripple 0.6s ease-out';
                
                this.style.position = 'relative';
                this.appendChild(ripple);
                
                setTimeout(() => {
                    ripple.remove();
                }, 600);
            });
        });

        // Add ripple animation
        if (!document.querySelector('#ripple-style')) {
            const style = document.createElement('style');
            style.id = 'ripple-style';
            style.textContent = `
                @keyframes ripple {
                    from {
                        opacity: 1;
                        transform: scale(0);
                    }
                    to {
                        opacity: 0;
                        transform: scale(4);
                    }
                }
            `;
            document.head.appendChild(style);
        }
    }

    // ============================================
    // Alert Auto-dismiss with Progress
    // ============================================
    function initializeAlertAnimations() {
        const alerts = document.querySelectorAll('.alert:not(.alert-static)');
        
        alerts.forEach(function(alert) {
            // Add progress bar
            const progressBar = document.createElement('div');
            progressBar.style.cssText = `
                position: absolute;
                bottom: 0;
                left: 0;
                height: 3px;
                background: currentColor;
                opacity: 0.3;
                width: 100%;
                transform-origin: left;
                animation: alertProgress 5s linear;
            `;
            alert.style.position = 'relative';
            alert.style.overflow = 'hidden';
            alert.appendChild(progressBar);

            // Add progress animation
            if (!document.querySelector('#alert-progress-style')) {
                const style = document.createElement('style');
                style.id = 'alert-progress-style';
                style.textContent = `
                    @keyframes alertProgress {
                        from { transform: scaleX(1); }
                        to { transform: scaleX(0); }
                    }
                `;
                document.head.appendChild(style);
            }

            // Auto dismiss after 5 seconds
            setTimeout(function() {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }, 5000);
        });
    }

    // ============================================
    // Active Link Highlight
    // ============================================
    function highlightActiveLink() {
        const currentPath = window.location.pathname;
        const navLinks = document.querySelectorAll('.shipper-navbar .nav-link');
        
        navLinks.forEach(function(link) {
            const href = link.getAttribute('href');
            if (href && currentPath.includes(href)) {
                link.classList.add('active');
            }
        });
    }

    // Call highlight on load
    highlightActiveLink();

    // ============================================
    // Smooth Scroll for Internal Links
    // ============================================
    document.querySelectorAll('a[href^="#"]').forEach(function(anchor) {
        anchor.addEventListener('click', function(e) {
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // ============================================
    // Mobile Menu Auto-close
    // ============================================
    const navbarCollapse = document.querySelector('.navbar-collapse');
    if (navbarCollapse) {
        const navLinks = navbarCollapse.querySelectorAll('.nav-link:not(.dropdown-toggle)');
        navLinks.forEach(function(link) {
            link.addEventListener('click', function() {
                if (window.innerWidth < 992) {
                    const bsCollapse = new bootstrap.Collapse(navbarCollapse, {
                        toggle: false
                    });
                    bsCollapse.hide();
                }
            });
        });
    }

    // ============================================
    // Badge Count Animation
    // ============================================
    function animateBadgeCount(badge, newCount) {
        if (!badge) return;
        
        const currentCount = parseInt(badge.textContent) || 0;
        if (newCount === currentCount) return;
        
        // Animate the change
        badge.style.animation = 'none';
        setTimeout(() => {
            badge.textContent = newCount > 99 ? '99+' : newCount;
            badge.style.animation = 'bounce 0.5s ease';
        }, 10);
        
        // Show notification
        if (newCount > currentCount) {
            showToastNotification('New delivery assigned!', 'info');
        }
    }

    // ============================================
    // Toast Notification Helper
    // ============================================
    function showToastNotification(message, type = 'info') {
        // Create toast container if it doesn't exist
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }

        // Create toast
        const toast = document.createElement('div');
        toast.className = 'toast align-items-center border-0';
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');
        
        const bgClass = type === 'success' ? 'bg-success' : 
                       type === 'danger' ? 'bg-danger' : 
                       type === 'warning' ? 'bg-warning' : 'bg-info';
        
        toast.classList.add(bgClass, 'text-white');
        
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi bi-info-circle me-2"></i>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        
        toastContainer.appendChild(toast);
        
        const bsToast = new bootstrap.Toast(toast, {
            autohide: true,
            delay: 3000
        });
        bsToast.show();
        
        toast.addEventListener('hidden.bs.toast', function() {
            toast.remove();
        });
    }

    // ============================================
    // Export functions for external use
    // ============================================
    window.ShipperLayout = {
        animateBadgeCount: animateBadgeCount,
        showNotification: showToastNotification
    };

})();
