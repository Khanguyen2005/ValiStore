// Category Products Page JavaScript
// Handles filtering and pagination for category products

function applyFilters() {
    var form = document.getElementById('filterForm');
    var formData = new FormData(form);

    // Get selected color IDs
    var colorIds = [];
    document.querySelectorAll('.color-checkbox:checked').forEach(function (checkbox) {
        colorIds.push(checkbox.value);
    });

    // Get selected size IDs
    var sizeIds = [];
    document.querySelectorAll('.size-checkbox:checked').forEach(function (checkbox) {
        sizeIds.push(checkbox.value);
    });

    // Build URL
    var baseUrl = document.getElementById('filterForm').getAttribute('data-url');
    var params = [];

    var sort = formData.get('sort');
    if (sort) params.push('sort=' + encodeURIComponent(sort));

    var minPrice = formData.get('minPrice');
    if (minPrice) params.push('minPrice=' + encodeURIComponent(minPrice));

    var maxPrice = formData.get('maxPrice');
    if (maxPrice) params.push('maxPrice=' + encodeURIComponent(maxPrice));

    if (colorIds.length > 0) params.push('colors=' + colorIds.join(','));
    if (sizeIds.length > 0) params.push('sizes=' + sizeIds.join(','));

    if (params.length > 0) {
        baseUrl += '?' + params.join('&');
    }

    window.location.href = baseUrl;
}

function clearFilters() {
    var baseUrl = document.getElementById('filterForm').getAttribute('data-url');
    window.location.href = baseUrl;
}

function changePage(page) {
    var form = document.getElementById('filterForm');
    var formData = new FormData(form);

    // Get selected color IDs
    var colorIds = [];
    document.querySelectorAll('.color-checkbox:checked').forEach(function (checkbox) {
        colorIds.push(checkbox.value);
    });

    // Get selected size IDs
    var sizeIds = [];
    document.querySelectorAll('.size-checkbox:checked').forEach(function (checkbox) {
        sizeIds.push(checkbox.value);
    });

    // Build URL with page parameter
    var baseUrl = document.getElementById('filterForm').getAttribute('data-url');
    var params = ['page=' + page];

    var sort = formData.get('sort');
    if (sort) params.push('sort=' + encodeURIComponent(sort));

    var minPrice = formData.get('minPrice');
    if (minPrice) params.push('minPrice=' + encodeURIComponent(minPrice));

    var maxPrice = formData.get('maxPrice');
    if (maxPrice) params.push('maxPrice=' + encodeURIComponent(maxPrice));

    if (colorIds.length > 0) params.push('colors=' + colorIds.join(','));
    if (sizeIds.length > 0) params.push('sizes=' + sizeIds.join(','));

    var url = baseUrl + '?' + params.join('&');
    window.location.href = url;
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('Category Products filters initialized');
});
