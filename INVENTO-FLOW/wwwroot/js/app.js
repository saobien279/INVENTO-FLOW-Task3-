// ==========================================
// INVENTO-FLOW CLIENT APPLICATION JS
// ==========================================

const API_BASE = '/api';

// Global application state
const state = {
    token: null,
    user: null, // { id, username, role }
    currentTab: 'dashboard-tab',
    products: {
        items: [],
        page: 1,
        pageSize: 8,
        totalPages: 1,
        total: 0,
        searchTerm: '',
        sortBy: '',
        categoryFilter: '',
        supplierFilter: ''
    },
    categories: [],
    suppliers: [],
    orders: [],
    cart: [] // { productId, name, price, quantity, maxStock }
};

// Initialize app when DOM is fully loaded
document.addEventListener('DOMContentLoaded', () => {
    // Check if token exists in localStorage
    const savedToken = localStorage.getItem('token');
    if (savedToken) {
        state.token = savedToken;
        state.user = parseJwtToken(savedToken);
        
        if (state.user) {
            showAppView();
        } else {
            handleLogout();
        }
    } else {
        showAuthView();
    }

    // Default Theme setup
    if (localStorage.getItem('theme') === 'light') {
        document.body.classList.remove('dark-theme');
    } else {
        document.body.classList.add('dark-theme');
        localStorage.setItem('theme', 'dark');
    }
});

// ==========================================
// AUTHENTICATION FUNCTIONS
// ==========================================

function switchAuthTab(type) {
    const loginForm = document.getElementById('login-form');
    const registerForm = document.getElementById('register-form');
    const loginTabBtn = document.getElementById('tab-login-btn');
    const registerTabBtn = document.getElementById('tab-register-btn');

    if (type === 'login') {
        loginForm.classList.remove('hidden');
        registerForm.classList.add('hidden');
        loginTabBtn.classList.add('active');
        registerTabBtn.classList.remove('active');
    } else {
        loginForm.classList.add('hidden');
        registerForm.classList.remove('hidden');
        loginTabBtn.classList.remove('active');
        registerTabBtn.classList.add('active');
    }
}

function togglePasswordVisibility(inputId, icon) {
    const input = document.getElementById(inputId);
    if (input.type === 'password') {
        input.type = 'text';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    } else {
        input.type = 'password';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    }
}

async function handleLogin(event) {
    event.preventDefault();
    const usernameInput = document.getElementById('login-username');
    const passwordInput = document.getElementById('login-password');
    const submitBtn = document.getElementById('btn-login-submit');

    const username = usernameInput.value.trim();
    const password = passwordInput.value;

    if (!username || !password) {
        showToast('Vui lòng điền đầy đủ thông tin đăng nhập', 'warning');
        return;
    }

    setBtnLoading(submitBtn, true, 'Đang đăng nhập...');

    try {
        const response = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Username: username, Password: password })
        });

        if (!response.ok) {
            const errData = await response.json().catch(() => ({}));
            throw new Error(errData.message || 'Sai tài khoản hoặc mật khẩu.');
        }

        const data = await response.json();
        
        if (data.token) {
            localStorage.setItem('token', data.token);
            state.token = data.token;
            state.user = parseJwtToken(data.token);

            showToast(`Chào mừng quay trở lại, ${state.user.username}!`, 'success');
            showAppView();
            
            // Reset form
            usernameInput.value = '';
            passwordInput.value = '';
        } else {
            throw new Error('Không nhận được token từ máy chủ.');
        }
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        setBtnLoading(submitBtn, false, 'Đang Đăng Nhập');
    }
}

async function handleRegister(event) {
    event.preventDefault();
    const usernameInput = document.getElementById('register-username');
    const passwordInput = document.getElementById('register-password');
    const confirmPasswordInput = document.getElementById('register-confirm-password');
    const submitBtn = document.getElementById('btn-register-submit');

    const username = usernameInput.value.trim();
    const password = passwordInput.value;
    const confirmPassword = confirmPasswordInput.value;

    if (!username || !password || !confirmPassword) {
        showToast('Vui lòng điền đầy đủ thông tin đăng ký', 'warning');
        return;
    }

    if (password !== confirmPassword) {
        showToast('Mật khẩu xác nhận không khớp', 'warning');
        return;
    }

    setBtnLoading(submitBtn, true, 'Đang xử lý...');

    try {
        const response = await fetch(`${API_BASE}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Username: username, Password: password })
        });

        if (!response.ok) {
            const errData = await response.json().catch(() => ({}));
            throw new Error(errData.message || 'Đăng ký thất bại. Tên đăng nhập có thể đã tồn tại.');
        }

        showToast('Đăng ký tài khoản thành công! Hãy đăng nhập.', 'success');
        switchAuthTab('login');
        
        // Reset form
        usernameInput.value = '';
        passwordInput.value = '';
        confirmPasswordInput.value = '';
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        setBtnLoading(submitBtn, false, 'Tạo Tài Khoản');
    }
}

function handleLogout() {
    localStorage.removeItem('token');
    state.token = null;
    state.user = null;
    state.cart = [];
    showAuthView();
    showToast('Đã đăng xuất khỏi hệ thống', 'success');
}

function showAuthView() {
    document.getElementById('auth-section').classList.remove('hidden');
    document.getElementById('app-section').classList.add('hidden');
}

function showAppView() {
    document.getElementById('auth-section').classList.add('hidden');
    document.getElementById('app-section').classList.remove('hidden');
    
    // Display user profile info
    document.getElementById('user-display-name').textContent = state.user.username;
    
    const roleBadge = document.getElementById('user-display-role');
    roleBadge.textContent = state.user.role;
    if (state.user.role === 'Admin') {
        roleBadge.className = 'role-badge admin-role';
        document.querySelectorAll('.admin-only').forEach(el => el.classList.remove('hidden'));
    } else {
        roleBadge.className = 'role-badge user-role';
        document.querySelectorAll('.admin-only').forEach(el => el.classList.add('hidden'));
    }

    // Default to Dashboard tab
    switchTab('dashboard-tab');
}

// Parse JWT Token Client-side (no library required)
function parseJwtToken(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(c => {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        
        const payload = JSON.parse(jsonPayload);
        
        // Map claims keys correctly
        const id = payload["nameid"] || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
        const username = payload["unique_name"] || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
        const role = payload["role"] || payload["http://schemas.xmlsoap.org/ws/2008/06/identity/claims/role"] || "User";

        return { id: parseInt(id), username, role };
    } catch (e) {
        console.error("JWT token parsing error:", e);
        return null;
    }
}

// ==========================================
// CORE API CALLER
// ==========================================
async function callApi(endpoint, options = {}) {
    const headers = { ...options.headers };
    
    if (!(options.body instanceof FormData)) {
        headers['Content-Type'] = 'application/json';
    }

    if (state.token) {
        headers['Authorization'] = `Bearer ${state.token}`;
    }

    const fetchOptions = {
        ...options,
        headers
    };

    try {
        const response = await fetch(`${API_BASE}${endpoint}`, fetchOptions);
        
        if (response.status === 401) {
            handleLogout();
            throw new Error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
        }

        if (response.status === 403) {
            throw new Error('Bạn không có quyền thực hiện thao tác này (Yêu cầu quyền Admin).');
        }

        // Return empty body for 204 No Content
        if (response.status === 204) {
            return null;
        }

        const data = await response.json();
        if (!response.ok) {
            throw new Error(data.message || `Lỗi máy chủ: ${response.status}`);
        }

        return data;
    } catch (err) {
        console.error(`API Error for ${endpoint}:`, err);
        throw err;
    }
}

// ==========================================
// TABS & NAVIGATION
// ==========================================
function switchTab(tabId) {
    state.currentTab = tabId;
    
    // Update active nav button style
    document.querySelectorAll('.nav-btn').forEach(btn => {
        if (btn.getAttribute('data-tab') === tabId) {
            btn.classList.add('active');
        } else {
            btn.classList.remove('active');
        }
    });

    // Update main pane display
    document.querySelectorAll('.tab-pane').forEach(pane => {
        if (pane.id === tabId) {
            pane.classList.add('active');
        } else {
            pane.classList.remove('active');
        }
    });

    // Update topbar title
    const titles = {
        'dashboard-tab': 'Tổng Quan Hệ Thống',
        'products-tab': 'Quản Lý Sản Phẩm Kho',
        'orders-tab': 'Quản Lý Đơn Hàng',
        'categories-tab': 'Danh Mục Phân Loại',
        'suppliers-tab': 'Nhà Cung Cấp'
    };
    document.getElementById('current-tab-title').textContent = titles[tabId] || 'InventoFlow';

    // Trigger tab-specific loaders
    loadTabData(tabId);
}

function loadTabData(tabId) {
    switch (tabId) {
        case 'dashboard-tab':
            refreshDashboardData();
            break;
        case 'products-tab':
            state.products.page = 1;
            loadProducts();
            loadCategoriesOptions();
            loadSuppliersOptions();
            break;
        case 'orders-tab':
            loadOrders();
            break;
        case 'categories-tab':
            loadCategories();
            break;
        case 'suppliers-tab':
            loadSuppliers();
            break;
    }
}

// ==========================================
// DASHBOARD VIEW LOGIC
// ==========================================
async function refreshDashboardData() {
    try {
        // Fetch catalogs for statistics (Products with high page limit to count and check low stock)
        const productsRes = await callApi('/products?pageNumber=1&pageSize=1000');
        const categoriesRes = await callApi('/categories');
        const suppliersRes = await callApi('/suppliers');
        
        let ordersCount = 0;
        if (state.user.role === 'Admin') {
            const ordersRes = await callApi('/orders?pageNumber=1&pageSize=1000');
            ordersCount = ordersRes.items ? ordersRes.items.length : 0;
        } else {
            const ordersRes = await callApi(`/orders/user/${state.user.id}`);
            ordersCount = ordersRes ? ordersRes.length : 0;
        }

        const totalProducts = productsRes.totalCount || productsRes.items.length || 0;
        const totalCategories = categoriesRes.length || 0;
        const totalSuppliers = suppliersRes.length || 0;

        // Render counts
        document.getElementById('stat-products-count').textContent = totalProducts;
        document.getElementById('stat-orders-count').textContent = ordersCount;
        document.getElementById('stat-categories-count').textContent = totalCategories;
        document.getElementById('stat-suppliers-count').textContent = totalSuppliers;

        // Find products with stock < 5
        const lowStockItems = productsRes.items.filter(p => p.stockQuantity < 5);
        renderLowStockAlerts(lowStockItems);

    } catch (error) {
        showToast('Không thể tải dữ liệu dashboard: ' + error.message, 'error');
    }
}

function renderLowStockAlerts(items) {
    const tbody = document.querySelector('#low-stock-table tbody');
    if (!items || items.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="4" class="text-center text-muted" style="padding: 2rem;">
                    <i class="fa-solid fa-circle-check" style="font-size: 2rem; color: var(--emerald-500); margin-bottom: 0.5rem; display:block;"></i>
                    Mức tồn kho an toàn. Không có sản phẩm nào sắp hết hàng!
                </td>
            </tr>`;
        return;
    }

    tbody.innerHTML = items.map(item => `
        <tr>
            <td><code style="color:var(--indigo-400);">${item.sku}</code></td>
            <td><strong>${item.name}</strong></td>
            <td style="font-weight:700; color:var(--rose-500);">${item.stockQuantity}</td>
            <td><span class="stock-badge outofstock">${item.stockQuantity === 0 ? 'Hết hàng' : 'Cảnh báo tồn ít'}</span></td>
        </tr>
    `).join('');
}

// ==========================================
// PRODUCTS VIEW LOGIC
// ==========================================
async function loadProducts() {
    const tbody = document.querySelector('#products-table tbody');
    tbody.innerHTML = `<tr><td colspan="8" class="text-center">Đang tải sản phẩm...</td></tr>`;

    try {
        let url = `/products?pageNumber=${state.products.page}&pageSize=${state.products.pageSize}`;
        if (state.products.searchTerm) {
            url += `&searchTerm=${encodeURIComponent(state.products.searchTerm)}`;
        }
        if (state.products.sortBy) {
            url += `&sortBy=${encodeURIComponent(state.products.sortBy)}`;
        }

        const data = await callApi(url);
        state.products.items = data.items || [];
        state.products.total = data.totalCount || 0;
        state.products.totalPages = data.totalPages || 1;

        renderProductsTable();
        renderProductsPagination();
    } catch (error) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-rose-500">Lỗi: ${error.message}</td></tr>`;
    }
}

function renderProductsTable() {
    const tbody = document.querySelector('#products-table tbody');
    if (state.products.items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Không tìm thấy sản phẩm nào.</td></tr>`;
        return;
    }

    // Apply category & supplier client side filtering if selected, since API query only supports searchTerm & sortBy
    let itemsToRender = state.products.items;
    
    if (state.products.categoryFilter) {
        itemsToRender = itemsToRender.filter(item => item.categoryId == state.products.categoryFilter);
    }
    if (state.products.supplierFilter) {
        itemsToRender = itemsToRender.filter(item => item.supplierId == state.products.supplierFilter);
    }

    tbody.innerHTML = itemsToRender.map(item => {
        const priceFormatted = formatCurrency(item.price);
        
        let stockBadgeClass = 'instock';
        let stockText = `${item.stockQuantity} chiếc`;
        if (item.stockQuantity === 0) {
            stockBadgeClass = 'outofstock';
            stockText = 'Hết Hàng';
        } else if (item.stockQuantity < 5) {
            stockBadgeClass = 'lowstock';
            stockText = `Tồn ít (${item.stockQuantity})`;
        }

        const isAdmin = state.user.role === 'Admin';
        const actionButtons = isAdmin ? `
            <div class="actions-cell">
                <button class="btn-icon edit-btn" onclick="openEditProductModal(${item.id})" title="Chỉnh sửa">
                    <i class="fa-solid fa-pen"></i>
                </button>
                <button class="btn-icon delete-btn" onclick="deleteProduct(${item.id})" title="Xóa">
                    <i class="fa-solid fa-trash-can"></i>
                </button>
            </div>` : `<span class="text-muted"><i class="fa-solid fa-lock" title="Chỉ đọc"></i></span>`;

        return `
            <tr>
                <td>${item.id}</td>
                <td><code>${item.sku}</code></td>
                <td><strong>${item.name}</strong></td>
                <td style="font-weight:700;">${priceFormatted}</td>
                <td><span class="stock-badge ${stockBadgeClass}">${stockText}</span></td>
                <td>${item.categoryName || '<span class="text-muted">Chưa có</span>'}</td>
                <td>${item.supplierName || '<span class="text-muted">Chưa có</span>'}</td>
                <td class="actions-col">${actionButtons}</td>
            </tr>
        `;
    }).join('');
}

function renderProductsPagination() {
    document.getElementById('current-page-num').textContent = state.products.page;
    
    const start = state.products.total === 0 ? 0 : (state.products.page - 1) * state.products.pageSize + 1;
    let end = state.products.page * state.products.pageSize;
    if (end > state.products.total) end = state.products.total;

    document.getElementById('pagination-start').textContent = start;
    document.getElementById('pagination-end').textContent = end;
    document.getElementById('pagination-total').textContent = state.products.total;

    document.getElementById('prev-page-btn').disabled = state.products.page <= 1;
    document.getElementById('next-page-btn').disabled = state.products.page >= state.products.totalPages;
}

function changeProductPage(direction) {
    const newPage = state.products.page + direction;
    if (newPage >= 1 && newPage <= state.products.totalPages) {
        state.products.page = newPage;
        loadProducts();
    }
}

function handleProductSearch() {
    state.products.searchTerm = document.getElementById('product-search-input').value;
    state.products.sortBy = document.getElementById('product-sort-by').value;
    state.products.categoryFilter = document.getElementById('product-filter-category').value;
    state.products.supplierFilter = document.getElementById('product-filter-supplier').value;
    
    // Reset back to page 1 for new search
    state.products.page = 1;
    loadProducts();
}

async function loadCategoriesOptions() {
    try {
        const data = await callApi('/categories');
        state.categories = data || [];
        
        // Fill search category dropdown
        const filterSelect = document.getElementById('product-filter-category');
        const formSelect = document.getElementById('product-category');
        
        const htmlOptions = ['<option value="">Tất cả Danh Mục</option>']
            .concat(state.categories.map(c => `<option value="${c.id}">${c.name}</option>`));
        
        filterSelect.innerHTML = htmlOptions.join('');
        
        const formOptions = ['<option value="">Chọn danh mục</option>']
            .concat(state.categories.map(c => `<option value="${c.id}">${c.name}</option>`));
        
        formSelect.innerHTML = formOptions.join('');
    } catch (e) {
        console.error("Lỗi tải option danh mục:", e);
    }
}

async function loadSuppliersOptions() {
    try {
        const data = await callApi('/suppliers');
        state.suppliers = data || [];
        
        // Fill search supplier dropdown
        const filterSelect = document.getElementById('product-filter-supplier');
        const formSelect = document.getElementById('product-supplier');
        
        const htmlOptions = ['<option value="">Tất cả Nhà Cung Cấp</option>']
            .concat(state.suppliers.map(s => `<option value="${s.id}">${s.name}</option>`));
        
        filterSelect.innerHTML = htmlOptions.join('');
        
        const formOptions = ['<option value="">Chọn nhà cung cấp</option>']
            .concat(state.suppliers.map(s => `<option value="${s.id}">${s.name}</option>`));
        
        formSelect.innerHTML = formOptions.join('');
    } catch (e) {
        console.error("Lỗi tải option nhà cung cấp:", e);
    }
}

// Open modals for Products
function openAddProductModal() {
    if (state.user.role !== 'Admin') {
        showToast('Bạn không có quyền thực hiện chức năng này', 'error');
        return;
    }
    document.getElementById('product-modal-title').textContent = 'Thêm Sản Phẩm Mới';
    document.getElementById('product-id').value = '';
    document.getElementById('product-form').reset();
    openModal('product-modal');
}

async function openEditProductModal(id) {
    if (state.user.role !== 'Admin') {
        showToast('Bạn không có quyền thực hiện chức năng này', 'error');
        return;
    }
    try {
        const product = await callApi(`/products/${id}`);
        document.getElementById('product-modal-title').textContent = 'Chỉnh Sửa Sản Phẩm';
        document.getElementById('product-id').value = product.id;
        document.getElementById('product-name').value = product.name;
        document.getElementById('product-sku').value = product.sku;
        document.getElementById('product-price').value = product.price;
        document.getElementById('product-stock').value = product.stockQuantity;
        document.getElementById('product-category').value = product.categoryId || '';
        document.getElementById('product-supplier').value = product.supplierId || '';
        
        openModal('product-modal');
    } catch (error) {
        showToast('Không tải được thông tin sản phẩm: ' + error.message, 'error');
    }
}

async function handleProductSubmit(event) {
    event.preventDefault();
    const id = document.getElementById('product-id').value;
    const name = document.getElementById('product-name').value.trim();
    const sku = document.getElementById('product-sku').value.trim();
    const price = parseFloat(document.getElementById('product-price').value);
    const stock = parseInt(document.getElementById('product-stock').value);
    const categoryId = document.getElementById('product-category').value;
    const supplierId = document.getElementById('product-supplier').value;

    const payload = {
        Name: name,
        SKU: sku,
        Price: price,
        StockQuantity: stock,
        CategoryId: categoryId ? parseInt(categoryId) : null,
        SupplierId: supplierId ? parseInt(supplierId) : null
    };

    try {
        if (id) {
            // Update
            payload.Id = parseInt(id);
            await callApi(`/products/${id}`, {
                method: 'PUT',
                body: JSON.stringify(payload)
            });
            showToast('Cập nhật sản phẩm thành công', 'success');
        } else {
            // Add new
            await callApi('/products', {
                method: 'POST',
                body: JSON.stringify(payload)
            });
            showToast('Thêm sản phẩm mới thành công', 'success');
        }
        closeModal('product-modal');
        loadProducts();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

async function deleteProduct(id) {
    if (state.user.role !== 'Admin') {
        showToast('Bạn không có quyền thực hiện chức năng này', 'error');
        return;
    }
    if (confirm('Bạn chắc chắn muốn xóa sản phẩm này? Thao tác này không thể hoàn tác.')) {
        try {
            await callApi(`/products/${id}`, { method: 'DELETE' });
            showToast('Xóa sản phẩm thành công', 'success');
            loadProducts();
        } catch (error) {
            showToast(error.message, 'error');
        }
    }
}

// ==========================================
// ORDERS & CHECKOUT VIEW LOGIC
// ==========================================
async function loadOrders() {
    const tbody = document.querySelector('#orders-table tbody');
    tbody.innerHTML = `<tr><td colspan="5" class="text-center">Đang tải danh sách đơn hàng...</td></tr>`;

    try {
        let orders = [];
        if (state.user.role === 'Admin') {
            const data = await callApi('/orders?pageNumber=1&pageSize=50');
            orders = data.items || [];
        } else {
            // Standard users only get their own history
            orders = await callApi(`/orders/user/${state.user.id}`);
        }

        state.orders = orders;
        renderOrdersTable();
    } catch (error) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center text-rose-500">Lỗi: ${error.message}</td></tr>`;
    }
}

function renderOrdersTable() {
    const tbody = document.querySelector('#orders-table tbody');
    if (state.orders.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">Không tìm thấy đơn hàng nào.</td></tr>`;
        return;
    }

    tbody.innerHTML = state.orders.map(order => {
        const orderDate = new Date(order.orderDate || order.ngayTao || new Date()).toLocaleString('vi-VN');
        const total = formatCurrency(order.totalAmount || order.tongTien || 0);
        const customerVal = order.userId || order.maKH || order.maNV || 'User';

        return `
            <tr>
                <td><strong>#${order.id}</strong></td>
                <td>${orderDate}</td>
                <td style="font-weight:700; color:var(--indigo-400);">${total}</td>
                <td><code>ID: ${customerVal}</code></td>
                <td>
                    <button class="btn-secondary" style="padding:0.4rem 0.8rem; font-size:0.8rem;" onclick="viewOrderDetail(${order.id})">
                        <i class="fa-solid fa-eye"></i> Xem Chi Tiết
                    </button>
                </td>
            </tr>
        `;
    }).join('');
}

async function viewOrderDetail(id) {
    try {
        const order = await callApi(`/orders/${id}`);
        document.getElementById('detail-order-id').textContent = order.id;
        
        const dateVal = order.orderDate || order.ngayTao || new Date();
        document.getElementById('detail-order-date').textContent = new Date(dateVal).toLocaleString('vi-VN');
        document.getElementById('detail-order-user').textContent = `Khách hàng ID: ${order.userId}`;
        document.getElementById('detail-order-total').textContent = formatCurrency(order.totalAmount);

        const itemsTableBody = document.querySelector('#order-detail-table tbody');
        const itemsList = order.orderItems || order.items || [];
        
        if (itemsList.length === 0) {
            itemsTableBody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">Không có chi tiết mặt hàng</td></tr>`;
        } else {
            itemsTableBody.innerHTML = itemsList.map(item => {
                const subtotal = item.quantity * item.price;
                return `
                    <tr>
                        <td><code>#${item.productId}</code></td>
                        <td><strong>${item.productName || 'Sản phẩm ' + item.productId}</strong></td>
                        <td>${item.quantity}</td>
                        <td>${formatCurrency(item.price)}</td>
                        <td style="font-weight:700;">${formatCurrency(subtotal)}</td>
                    </tr>
                `;
            }).join('');
        }

        openModal('order-detail-modal');
    } catch (error) {
        showToast('Không tải được chi tiết đơn hàng: ' + error.message, 'error');
    }
}

// Checkout flow
async function openCreateOrderModal() {
    state.cart = [];
    updateCartUi();
    
    // Fetch products catalog
    const catalogContainer = document.getElementById('catalog-products-list');
    catalogContainer.innerHTML = '<div class="text-center py-4">Đang tải danh sách sản phẩm...</div>';
    
    try {
        const res = await callApi('/products?pageNumber=1&pageSize=1000');
        const products = res.items || [];
        
        if (products.length === 0) {
            catalogContainer.innerHTML = '<div class="text-center py-4 text-muted">Kho rỗng. Vui lòng thêm sản phẩm trước.</div>';
            return;
        }

        // Store catalog temporarily on catalog global to filter
        window.orderCatalogList = products;
        renderCatalogList(products);
        openModal('order-modal');
    } catch (e) {
        showToast('Không tải được catalog sản phẩm: ' + e.message, 'error');
    }
}

function renderCatalogList(products) {
    const catalogContainer = document.getElementById('catalog-products-list');
    
    catalogContainer.innerHTML = products.map(p => {
        const isOutOfStock = p.stockQuantity <= 0;
        const addBtn = isOutOfStock 
            ? `<button class="btn-secondary btn-block" disabled style="padding:0.4rem; font-size:0.8rem;">Hết Hàng</button>`
            : `<button class="btn-primary btn-block" style="padding:0.4rem; font-size:0.8rem;" onclick="addToCart(${p.id}, '${p.name.replace(/'/g, "\\'")}', ${p.price}, ${p.stockQuantity})">
                <i class="fa-solid fa-plus"></i> Chọn Mua
               </button>`;
        
        return `
            <div class="catalog-item-card">
                <div class="catalog-item-info">
                    <h5>${p.name}</h5>
                    <span class="catalog-item-sku">${p.sku}</span>
                    <p class="catalog-item-price">${formatCurrency(p.price)}</p>
                    <p class="catalog-item-stock text-muted">Kho: ${p.stockQuantity} cái</p>
                </div>
                <div style="margin-top:0.75rem;">
                    ${addBtn}
                </div>
            </div>
        `;
    }).join('');
}

function filterCatalog() {
    const keyword = document.getElementById('catalog-search-input').value.trim().toLowerCase();
    if (!window.orderCatalogList) return;
    
    const filtered = window.orderCatalogList.filter(p => 
        p.name.toLowerCase().includes(keyword) || 
        p.sku.toLowerCase().includes(keyword)
    );
    renderCatalogList(filtered);
}

function addToCart(id, name, price, maxStock) {
    const existing = state.cart.find(item => item.productId === id);
    
    if (existing) {
        if (existing.quantity >= maxStock) {
            showToast(`Vượt quá số lượng tồn kho của ${name} (${maxStock} cái)`, 'warning');
            return;
        }
        existing.quantity++;
    } else {
        state.cart.push({
            productId: id,
            name: name,
            price: price,
            quantity: 1,
            maxStock: maxStock
        });
    }
    
    updateCartUi();
    showToast(`Đã thêm ${name} vào giỏ`, 'success', 1000);
}

function changeCartQty(id, delta) {
    const item = state.cart.find(item => item.productId === id);
    if (!item) return;

    const newQty = item.quantity + delta;
    if (newQty <= 0) {
        state.cart = state.cart.filter(i => i.productId !== id);
    } else if (newQty > item.maxStock) {
        showToast(`Không đủ số lượng tồn kho. Tối đa: ${item.maxStock}`, 'warning');
        return;
    } else {
        item.quantity = newQty;
    }
    updateCartUi();
}

function clearCart() {
    state.cart = [];
    updateCartUi();
}

function updateCartUi() {
    const cartContainer = document.getElementById('cart-items-list');
    
    if (state.cart.length === 0) {
        cartContainer.innerHTML = '<div class="empty-cart-message">Chưa có sản phẩm nào được chọn.</div>';
        document.getElementById('cart-total-qty').textContent = 0;
        document.getElementById('cart-total-price').textContent = '0 ₫';
        return;
    }

    cartContainer.innerHTML = state.cart.map(item => `
        <div class="cart-item">
            <div class="cart-item-details">
                <h5>${item.name}</h5>
                <p>${formatCurrency(item.price)} x ${item.quantity}</p>
            </div>
            <div class="cart-item-controls">
                <button class="cart-item-btn" onclick="changeCartQty(${item.productId}, -1)">-</button>
                <span class="cart-item-qty">${item.quantity}</span>
                <button class="cart-item-btn" onclick="changeCartQty(${item.productId}, 1)">+</button>
            </div>
        </div>
    `).join('');

    const totalQty = state.cart.reduce((sum, item) => sum + item.quantity, 0);
    const totalPrice = state.cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);

    document.getElementById('cart-total-qty').textContent = totalQty;
    document.getElementById('cart-total-price').textContent = formatCurrency(totalPrice);
}

async function submitOrder() {
    if (state.cart.length === 0) {
        showToast('Giỏ hàng trống. Hãy chọn sản phẩm trước.', 'warning');
        return;
    }

    const payload = {
        UserId: state.user.id,
        Items: state.cart.map(item => ({
            ProductId: item.productId,
            Quantity: item.quantity
        }))
    };

    try {
        const response = await callApi('/orders', {
            method: 'POST',
            body: JSON.stringify(payload)
        });

        showToast('Đặt hàng thành công!', 'success');
        closeModal('order-modal');
        
        // Refresh appropriate view
        if (state.currentTab === 'orders-tab') {
            loadOrders();
        } else if (state.currentTab === 'dashboard-tab') {
            refreshDashboardData();
        }
    } catch (e) {
        showToast(e.message, 'error');
    }
}

// ==========================================
// CATEGORIES CRUD LOGIC (Admin Only)
// ==========================================
async function loadCategories() {
    const tbody = document.querySelector('#categories-table tbody');
    tbody.innerHTML = `<tr><td colspan="3" class="text-center">Đang tải danh mục...</td></tr>`;

    try {
        const data = await callApi('/categories');
        state.categories = data || [];
        renderCategoriesTable();
    } catch (e) {
        tbody.innerHTML = `<tr><td colspan="3" class="text-center text-rose-500">Lỗi: ${e.message}</td></tr>`;
    }
}

function renderCategoriesTable() {
    const tbody = document.querySelector('#categories-table tbody');
    if (state.categories.length === 0) {
        tbody.innerHTML = `<tr><td colspan="3" class="text-center text-muted">Chưa có danh mục nào.</td></tr>`;
        return;
    }

    tbody.innerHTML = state.categories.map(c => {
        const isAdmin = state.user.role === 'Admin';
        const actions = isAdmin ? `
            <div class="actions-cell">
                <button class="btn-icon edit-btn" onclick="openEditCategoryModal(${c.id}, '${c.name.replace(/'/g, "\\'")}')" title="Sửa">
                    <i class="fa-solid fa-pen"></i>
                </button>
                <button class="btn-icon delete-btn" onclick="deleteCategory(${c.id})" title="Xóa">
                    <i class="fa-solid fa-trash-can"></i>
                </button>
            </div>` : `<span class="text-muted"><i class="fa-solid fa-lock" title="Chỉ đọc"></i></span>`;

        return `
            <tr>
                <td>${c.id}</td>
                <td><strong>${c.name}</strong></td>
                <td class="actions-col">${actions}</td>
            </tr>
        `;
    }).join('');
}

function openAddCategoryModal() {
    if (state.user.role !== 'Admin') {
        showToast('Bạn không có quyền thực hiện chức năng này', 'error');
        return;
    }
    document.getElementById('category-modal-title').textContent = 'Thêm Danh Mục Mới';
    document.getElementById('category-id').value = '';
    document.getElementById('category-name').value = '';
    openModal('category-modal');
}

function openEditCategoryModal(id, name) {
    if (state.user.role !== 'Admin') {
        showToast('Bạn không có quyền thực hiện chức năng này', 'error');
        return;
    }
    document.getElementById('category-modal-title').textContent = 'Chỉnh Sửa Danh Mục';
    document.getElementById('category-id').value = id;
    document.getElementById('category-name').value = name;
    openModal('category-modal');
}

async function handleCategorySubmit(event) {
    event.preventDefault();
    const id = document.getElementById('category-id').value;
    const name = document.getElementById('category-name').value.trim();

    if (!name) {
        showToast('Tên danh mục không được để trống', 'warning');
        return;
    }

    const payload = { Name: name };

    try {
        if (id) {
            payload.Id = parseInt(id);
            await callApi(`/categories/${id}`, {
                method: 'PUT',
                body: JSON.stringify(payload)
            });
            showToast('Cập nhật danh mục thành công', 'success');
        } else {
            await callApi('/categories', {
                method: 'POST',
                body: JSON.stringify(payload)
            });
            showToast('Tạo danh mục mới thành công', 'success');
        }
        closeModal('category-modal');
        loadCategories();
    } catch (e) {
        showToast(e.message, 'error');
    }
}

async function deleteCategory(id) {
    if (state.user.role !== 'Admin') {
        showToast('Bạn không có quyền thực hiện chức năng này', 'error');
        return;
    }
    if (confirm('Xác nhận xóa danh mục này?')) {
        try {
            await callApi(`/categories/${id}`, { method: 'DELETE' });
            showToast('Xóa danh mục thành công', 'success');
            loadCategories();
        } catch (e) {
            showToast(e.message, 'error');
        }
    }
}

// ==========================================
// SUPPLIERS CRUD LOGIC (Admin Only)
// ==========================================
async function loadSuppliers() {
    const tbody = document.querySelector('#suppliers-table tbody');
    tbody.innerHTML = `<tr><td colspan="5" class="text-center">Đang tải nhà cung cấp...</td></tr>`;

    try {
        const data = await callApi('/suppliers');
        state.suppliers = data || [];
        renderSuppliersTable();
    } catch (e) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center text-rose-500">Lỗi: ${e.message}</td></tr>`;
    }
}

function renderSuppliersTable() {
    const tbody = document.querySelector('#suppliers-table tbody');
    if (state.suppliers.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">Chưa có nhà cung cấp nào.</td></tr>`;
        return;
    }

    tbody.innerHTML = state.suppliers.map(s => {
        const address = s.address || s.diaChi || '<span class="text-muted">Chưa cập nhật</span>';
        const phone = s.phoneNumber || s.sdt || s.phone || '<span class="text-muted">Chưa cập nhật</span>';
        
        const isAdmin = state.user.role === 'Admin';
        const actions = isAdmin ? `
            <div class="actions-cell">
                <button class="btn-icon edit-btn" onclick="openEditSupplierModal(${s.id})" title="Sửa">
                    <i class="fa-solid fa-pen"></i>
                </button>
                <button class="btn-icon delete-btn" onclick="deleteSupplier(${s.id})" title="Xóa">
                    <i class="fa-solid fa-trash-can"></i>
                </button>
            </div>` : `<span class="text-muted"><i class="fa-solid fa-lock" title="Chỉ đọc"></i></span>`;

        return `
            <tr>
                <td>${s.id}</td>
                <td><strong>${s.name}</strong></td>
                <td>${address}</td>
                <td><code>${phone}</code></td>
                <td class="actions-col">${actions}</td>
            </tr>
        `;
    }).join('');
}

function openAddSupplierModal() {
    if (state.user.role !== 'Admin') {
        showToast('Bạn không có quyền thực hiện chức năng này', 'error');
        return;
    }
    document.getElementById('supplier-modal-title').textContent = 'Thêm Nhà Cung Cấp Mới';
    document.getElementById('supplier-id').value = '';
    document.getElementById('supplier-form').reset();
    openModal('supplier-modal');
}

async function openEditSupplierModal(id) {
    if (state.user.role !== 'Admin') {
        showToast('Bạn không có quyền thực hiện chức năng này', 'error');
        return;
    }
    try {
        const s = await callApi(`/suppliers/${id}`);
        document.getElementById('supplier-modal-title').textContent = 'Chỉnh Sửa Nhà Cung Cấp';
        document.getElementById('supplier-id').value = s.id;
        document.getElementById('supplier-name').value = s.name;
        document.getElementById('supplier-address').value = s.address || s.diaChi || '';
        document.getElementById('supplier-phone').value = s.phoneNumber || s.sdt || s.phone || '';
        
        openModal('supplier-modal');
    } catch (e) {
        showToast('Lỗi tải dữ liệu nhà cung cấp: ' + e.message, 'error');
    }
}

async function handleSupplierSubmit(event) {
    event.preventDefault();
    const id = document.getElementById('supplier-id').value;
    const name = document.getElementById('supplier-name').value.trim();
    const address = document.getElementById('supplier-address').value.trim();
    const phone = document.getElementById('supplier-phone').value.trim();

    if (!name) {
        showToast('Tên nhà cung cấp không được để trống', 'warning');
        return;
    }

    const payload = {
        Name: name,
        Address: address,
        PhoneNumber: phone
    };

    try {
        if (id) {
            payload.Id = parseInt(id);
            await callApi(`/suppliers/${id}`, {
                method: 'PUT',
                body: JSON.stringify(payload)
            });
            showToast('Cập nhật nhà cung cấp thành công', 'success');
        } else {
            await callApi('/suppliers', {
                method: 'POST',
                body: JSON.stringify(payload)
            });
            showToast('Tạo nhà cung cấp mới thành công', 'success');
        }
        closeModal('supplier-modal');
        loadSuppliers();
    } catch (e) {
        showToast(e.message, 'error');
    }
}

async function deleteSupplier(id) {
    if (state.user.role !== 'Admin') {
        showToast('Bạn không có quyền thực hiện chức năng này', 'error');
        return;
    }
    if (confirm('Xác nhận xóa nhà cung cấp này?')) {
        try {
            await callApi(`/suppliers/${id}`, { method: 'DELETE' });
            showToast('Xóa nhà cung cấp thành công', 'success');
            loadSuppliers();
        } catch (e) {
            showToast(e.message, 'error');
        }
    }
}

// ==========================================
// CLIENT UI UTILITIES
// ==========================================

// Modal management
function openModal(modalId) {
    document.getElementById(modalId).classList.add('active');
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('active');
}

// Close modals when clicking outside content area
window.onclick = function(event) {
    if (event.target.classList.contains('modal')) {
        event.target.classList.remove('active');
    }
};

// Theme switcher
function toggleTheme() {
    const body = document.body;
    if (body.classList.contains('dark-theme')) {
        body.classList.remove('dark-theme');
        localStorage.setItem('theme', 'light');
    } else {
        body.classList.add('dark-theme');
        localStorage.setItem('theme', 'dark');
    }
}

// Toast notification helper
function showToast(message, type = 'info', duration = 3500) {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    
    let icon = 'fa-circle-info';
    if (type === 'success') icon = 'fa-circle-check';
    if (type === 'error') icon = 'fa-circle-xmark';
    if (type === 'warning') icon = 'fa-triangle-exclamation';

    toast.innerHTML = `
        <i class="fa-solid ${icon}"></i>
        <div class="toast-message">${message}</div>
    `;
    
    container.appendChild(toast);
    
    setTimeout(() => {
        toast.style.animation = 'slideIn 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275) reverse forwards';
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

// Button loading state helper
function setBtnLoading(btn, isLoading, text) {
    const span = btn.querySelector('span');
    const icon = btn.querySelector('i');
    
    if (isLoading) {
        btn.disabled = true;
        if (span) btn.dataset.originalText = span.textContent;
        if (span) span.textContent = text;
        if (icon) {
            btn.dataset.originalIconClass = icon.className;
            icon.className = 'fa-solid fa-circle-notch fa-spin';
        }
    } else {
        btn.disabled = false;
        if (span && btn.dataset.originalText) span.textContent = btn.dataset.originalText;
        if (icon && btn.dataset.originalIconClass) icon.className = btn.dataset.originalIconClass;
    }
}

// Format currency in VNĐ
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}
