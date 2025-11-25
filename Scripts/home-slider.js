/**
 * Product Slider - Smooth sliding with one product per click
 * ValiModern E-commerce
 */

(function () {
    'use strict';

    class ProductSlider {
        constructor(wrapper) {
            this.wrapper = wrapper;
            this.track = wrapper.querySelector('.product-slider-track');
            this.items = Array.from(wrapper.querySelectorAll('.product-slider-item'));
            this.prevBtn = wrapper.querySelector('.product-slider-prev');
            this.nextBtn = wrapper.querySelector('.product-slider-next');
            
            this.currentIndex = 0;
            this.itemsPerView = this.getItemsPerView();
            this.totalItems = this.items.length;
            this.isAnimating = false;
            
            this.init();
        }

        init() {
            // Bind events
            if (this.prevBtn) {
                this.prevBtn.addEventListener('click', () => this.prev());
            }
            if (this.nextBtn) {
                this.nextBtn.addEventListener('click', () => this.next());
            }

            // Handle window resize
            let resizeTimeout;
            window.addEventListener('resize', () => {
                clearTimeout(resizeTimeout);
                resizeTimeout = setTimeout(() => {
                    this.itemsPerView = this.getItemsPerView();
                    this.updateSlider();
                }, 150);
            });

            // Touch/swipe support for mobile
            this.addTouchSupport();

            // Initial update
            this.updateSlider();
        }

        getItemsPerView() {
            const width = window.innerWidth;
            if (width <= 576) return 1;      // Mobile: 1 item
            if (width <= 991) return 2;      // Tablet: 2 items
            return 4;                         // Desktop: 4 items
        }

        prev() {
            if (this.isAnimating) return;
            
            this.isAnimating = true;
            
            if (this.currentIndex > 0) {
                this.currentIndex--;
            } else {
                // Infinite loop: Jump to end
                this.currentIndex = this.totalItems - this.itemsPerView;
            }
            
            this.updateSlider(() => {
                this.isAnimating = false;
            });
        }

        next() {
            if (this.isAnimating) return;
            
            this.isAnimating = true;
            const maxIndex = this.totalItems - this.itemsPerView;
            
            if (this.currentIndex < maxIndex) {
                this.currentIndex++;
            } else {
                // Infinite loop: Jump to start
                this.currentIndex = 0;
            }
            
            this.updateSlider(() => {
                this.isAnimating = false;
            });
        }

        updateSlider(callback) {
            // Calculate transform
            const itemWidth = this.items[0].offsetWidth;
            const gap = 16; // Match CSS gap
            const gapAdjustment = window.innerWidth <= 576 ? 8 : (window.innerWidth <= 991 ? 12 : 16);
            const offset = this.currentIndex * (itemWidth + gapAdjustment);
            
            // Apply transform
            this.track.style.transform = `translateX(-${offset}px)`;

            // Update button states (always enabled for infinite loop)
            this.updateButtons();
            
            // Call callback after animation
            if (callback) {
                setTimeout(callback, 400); // Match transition duration
            }
        }

        updateButtons() {
            // For infinite loop, buttons are always enabled
            // But we can still show visual feedback
            
            if (this.prevBtn) {
                this.prevBtn.removeAttribute('disabled');
            }

            if (this.nextBtn) {
                this.nextBtn.removeAttribute('disabled');
            }
        }

        addTouchSupport() {
            let touchStartX = 0;
            let touchEndX = 0;

            this.track.addEventListener('touchstart', (e) => {
                touchStartX = e.changedTouches[0].screenX;
            }, { passive: true });

            this.track.addEventListener('touchend', (e) => {
                touchEndX = e.changedTouches[0].screenX;
                this.handleSwipe();
            }, { passive: true });

            const handleSwipe = () => {
                const swipeThreshold = 50; // Minimum swipe distance
                const diff = touchStartX - touchEndX;

                if (Math.abs(diff) > swipeThreshold) {
                    if (diff > 0) {
                        // Swipe left - next
                        this.next();
                    } else {
                        // Swipe right - prev
                        this.prev();
                    }
                }
            };

            this.handleSwipe = handleSwipe;
        }
    }

    // Initialize all sliders when DOM is ready
    function initSliders() {
        const sliderWrappers = document.querySelectorAll('.product-slider-wrapper');
        
        sliderWrappers.forEach(wrapper => {
            new ProductSlider(wrapper);
        });
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSliders);
    } else {
        initSliders();
    }

})();
