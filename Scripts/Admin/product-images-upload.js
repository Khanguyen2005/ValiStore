/**
 * Product Images Upload - Admin JavaScript
 * Handles multi-image upload with drag & drop functionality
 */

(function () {
    'use strict';

    // Multi-image dropzone functionality
    const ProductImagesUpload = {
        // DOM Elements
        dropZone: null,
        fileInput: null,
        previewContainer: null,
        uploadBtn: null,
        selectedFiles: [],

        // Initialize
        init: function () {
            this.dropZone = document.getElementById('dropZone');
            this.fileInput = document.getElementById('imageFiles');
            this.previewContainer = document.getElementById('previewContainer');
            this.uploadBtn = document.getElementById('uploadBtn');

            if (!this.dropZone || !this.fileInput) {
                console.error('Required elements not found');
                return;
            }

            this.bindEvents();
        },

        // Bind all events
        bindEvents: function () {
            const self = this;

            // Click on dropzone to trigger file input
            this.dropZone.addEventListener('click', function (e) {
                if (e.target.tagName !== 'BUTTON' && e.target.tagName !== 'INPUT') {
                    self.fileInput.click();
                }
            });

            // Handle file selection
            this.fileInput.addEventListener('change', function () {
                if (self.fileInput.files && self.fileInput.files.length > 0) {
                    const newFiles = Array.from(self.fileInput.files);
                    self.selectedFiles = [...self.selectedFiles, ...newFiles];
                    self.updateFileInput();
                    self.displayPreviews();
                    self.uploadBtn.style.display = 'inline-block';
                }
            });

            // Drag and drop events
            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
                self.dropZone.addEventListener(eventName, self.preventDefaults, false);
            });

            ['dragenter', 'dragover'].forEach(eventName => {
                self.dropZone.addEventListener(eventName, function () {
                    self.dropZone.classList.add('border-primary', 'bg-primary', 'bg-opacity-10');
                });
            });

            ['dragleave', 'drop'].forEach(eventName => {
                self.dropZone.addEventListener(eventName, function () {
                    self.dropZone.classList.remove('border-primary', 'bg-primary', 'bg-opacity-10');
                });
            });

            this.dropZone.addEventListener('drop', function (e) {
                self.handleDrop(e);
            });

            // Form validation before submit
            const uploadForm = document.getElementById('uploadForm');
            if (uploadForm) {
                uploadForm.addEventListener('submit', function (e) {
                    if (self.selectedFiles.length === 0) {
                        e.preventDefault();
                        alert('Please select at least one image to upload.');
                        return false;
                    }
                });
            }
        },

        // Prevent default drag behaviors
        preventDefaults: function (e) {
            e.preventDefault();
            e.stopPropagation();
        },

        // Handle file drop
        handleDrop: function (e) {
            const dt = e.dataTransfer;
            const droppedFiles = Array.from(dt.files);

            // Add dropped files to existing files
            this.selectedFiles = [...this.selectedFiles, ...droppedFiles];
            this.updateFileInput();
            this.displayPreviews();
            this.uploadBtn.style.display = 'inline-block';
        },

        // Update file input with all selected files
        updateFileInput: function () {
            const dt = new DataTransfer();
            this.selectedFiles.forEach(file => {
                dt.items.add(file);
            });
            this.fileInput.files = dt.files;
        },

        // Display preview for all selected files
        displayPreviews: function () {
            const self = this;
            this.previewContainer.innerHTML = '';

            if (this.selectedFiles.length === 0) {
                this.previewContainer.style.display = 'none';
                this.uploadBtn.style.display = 'none';
                return;
            }

            this.previewContainer.style.display = 'flex';

            this.selectedFiles.forEach((file, index) => {
                const reader = new FileReader();
                reader.onload = function (e) {
                    const col = document.createElement('div');
                    col.className = 'col-6 col-md-3';
                    col.innerHTML = `
    <div class="card shadow-sm preview-card">
       <img src="${e.target.result}" class="card-img-top" style="height:150px;object-fit:cover;" />
             <div class="card-body p-2 text-center">
        <small class="text-muted d-block text-truncate" title="${self.escapeHtml(file.name)}">${self.escapeHtml(file.name)}</small>
     <small class="badge bg-info">${self.formatFileSize(file.size)}</small>
   <br>
      <button type="button" class="btn btn-sm btn-outline-danger mt-2" 
    onclick="ProductImagesUpload.removeFile(${index})">
        <i class="bi bi-x-circle"></i> Remove
       </button>
            </div>
       </div>
        `;
                    self.previewContainer.appendChild(col);
                };
                reader.readAsDataURL(file);
            });
        },

        // Remove file at index
        removeFile: function (index) {
            this.selectedFiles.splice(index, 1);
            this.updateFileInput();
            this.displayPreviews();
        },

        // Format file size
        formatFileSize: function (bytes) {
            if (bytes === 0) return '0 Bytes';
            const k = 1024;
            const sizes = ['Bytes', 'KB', 'MB', 'GB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
        },

        // Escape HTML to prevent XSS
        escapeHtml: function (text) {
            const map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, function (m) { return map[m]; });
        }
    };

    // Expose to window for onclick handlers
    window.ProductImagesUpload = ProductImagesUpload;

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            ProductImagesUpload.init();
        });
    } else {
        ProductImagesUpload.init();
    }

})();

/**
 * Set Main Image
 * @param {number} id - Image ID
 */
function setMainImage(id) {
    if (confirm('Set this image as the main product image?')) {
        document.getElementById('setMainId').value = id;
        document.getElementById('setMainForm').submit();
    }
}

/**
 * Delete Image
 * @param {number} id - Image ID
 * @param {string} url - Image URL
 */
function deleteImage(id, url) {
    const fileName = url.split('/').pop();
    if (confirm('Delete this image?\n\n' + fileName)) {
        document.getElementById('deleteId').value = id;
        document.getElementById('deleteForm').submit();
    }
}
