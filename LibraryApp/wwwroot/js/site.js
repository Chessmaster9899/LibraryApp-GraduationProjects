// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Toast Notification System
class ToastManager {
    constructor() {
        this.container = this.createContainer();
        this.toastId = 0;
    }

    createContainer() {
        let container = document.querySelector('.toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container';
            document.body.appendChild(container);
        }
        return container;
    }

    show(type, title, message, duration = 5000) {
        const toast = this.createToast(type, title, message);
        this.container.appendChild(toast);

        // Trigger animation
        setTimeout(() => toast.classList.add('showing'), 10);

        // Auto-hide
        if (duration > 0) {
            setTimeout(() => this.hide(toast), duration);
        }

        return toast;
    }

    createToast(type, title, message) {
        const toastId = ++this.toastId;
        const iconMap = {
            success: '✓',
            error: '✗',
            warning: '⚠',
            info: 'ℹ'
        };

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.setAttribute('data-toast-id', toastId);

        toast.innerHTML = `
            <div class="toast-header">
                <div class="toast-icon">${iconMap[type] || 'ℹ'}</div>
                <div class="toast-title">${title}</div>
                <button type="button" class="toast-close" onclick="toastManager.hide(this.closest('.toast'))">×</button>
            </div>
            <div class="toast-body">${message}</div>
        `;

        return toast;
    }

    hide(toast) {
        if (toast && toast.parentNode) {
            toast.classList.remove('showing');
            toast.classList.add('hide');
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }
    }

    success(title, message, duration) {
        return this.show('success', title, message, duration);
    }

    error(title, message, duration) {
        return this.show('error', title, message, duration);
    }

    warning(title, message, duration) {
        return this.show('warning', title, message, duration);
    }

    info(title, message, duration) {
        return this.show('info', title, message, duration);
    }

    // Clear all toasts
    clear() {
        const toasts = this.container.querySelectorAll('.toast');
        toasts.forEach(toast => this.hide(toast));
    }
}

// Initialize global toast manager
const toastManager = new ToastManager();

// Utility functions for easy access
function showToast(type, title, message, duration) {
    return toastManager.show(type, title, message, duration);
}

function showSuccess(title, message, duration = 5000) {
    return toastManager.success(title, message, duration);
}

function showError(title, message, duration = 7000) {
    return toastManager.error(title, message, duration);
}

function showWarning(title, message, duration = 6000) {
    return toastManager.warning(title, message, duration);
}

function showInfo(title, message, duration = 5000) {
    return toastManager.info(title, message, duration);
}

// Enhanced Loading State Management
class LoadingManager {
    constructor() {
        this.loadingStates = new Map();
        this.initializeGlobalLoaders();
    }

    initializeGlobalLoaders() {
        // Create global loading overlay
        const overlay = document.createElement('div');
        overlay.className = 'loading-overlay';
        overlay.innerHTML = `
            <div class="loading-spinner">
                <div class="spinner-ring"></div>
                <div class="loading-text">Loading...</div>
            </div>
        `;
        document.body.appendChild(overlay);

        // Create page loader for navigation
        const pageLoader = document.createElement('div');
        pageLoader.className = 'page-loading-bar';
        document.body.appendChild(pageLoader);
    }

    showGlobalLoading(message = 'Loading...') {
        const overlay = document.querySelector('.loading-overlay');
        const text = overlay.querySelector('.loading-text');
        text.textContent = message;
        overlay.classList.add('visible');
        document.body.classList.add('loading');
    }

    hideGlobalLoading() {
        const overlay = document.querySelector('.loading-overlay');
        overlay.classList.remove('visible');
        document.body.classList.remove('loading');
    }

    showPageProgress() {
        const bar = document.querySelector('.page-loading-bar');
        bar.classList.add('loading');
        bar.style.width = '30%';
        
        setTimeout(() => bar.style.width = '60%', 200);
        setTimeout(() => bar.style.width = '90%', 500);
    }

    hidePageProgress() {
        const bar = document.querySelector('.page-loading-bar');
        bar.style.width = '100%';
        setTimeout(() => {
            bar.classList.remove('loading');
            bar.style.width = '0%';
        }, 200);
    }

    setButtonLoading(button, isLoading, originalText = null) {
        const btn = typeof button === 'string' ? document.querySelector(button) : button;
        if (!btn) return;

        if (isLoading) {
            const text = originalText || btn.textContent || btn.value;
            btn.dataset.originalText = text;
            btn.disabled = true;
            btn.classList.add('loading');
            
            // Add spinner
            if (!btn.querySelector('.btn-spinner')) {
                const spinner = document.createElement('span');
                spinner.className = 'btn-spinner';
                btn.insertBefore(spinner, btn.firstChild);
            }
            
            if (btn.textContent !== undefined) {
                btn.textContent = ' Processing...';
            } else {
                btn.value = 'Processing...';
            }
        } else {
            btn.disabled = false;
            btn.classList.remove('loading');
            
            // Remove spinner
            const spinner = btn.querySelector('.btn-spinner');
            if (spinner) spinner.remove();
            
            const originalText = btn.dataset.originalText;
            if (originalText) {
                if (btn.textContent !== undefined) {
                    btn.textContent = originalText;
                } else {
                    btn.value = originalText;
                }
            }
        }
    }

    showElementLoading(element, message = 'Loading...') {
        const el = typeof element === 'string' ? document.querySelector(element) : element;
        if (!el) return;

        const loaderId = 'loader_' + Date.now();
        const loader = document.createElement('div');
        loader.className = 'element-loader';
        loader.setAttribute('data-loader-id', loaderId);
        loader.innerHTML = `
            <div class="element-spinner">
                <div class="spinner-dots">
                    <div class="dot1"></div>
                    <div class="dot2"></div>
                    <div class="dot3"></div>
                </div>
                <div class="element-loading-text">${message}</div>
            </div>
        `;

        el.style.position = 'relative';
        el.appendChild(loader);
        el.classList.add('element-loading');
        
        return loaderId;
    }

    hideElementLoading(element, loaderId = null) {
        const el = typeof element === 'string' ? document.querySelector(element) : element;
        if (!el) return;

        if (loaderId) {
            const loader = el.querySelector(`[data-loader-id="${loaderId}"]`);
            if (loader) loader.remove();
        } else {
            const loaders = el.querySelectorAll('.element-loader');
            loaders.forEach(loader => loader.remove());
        }
        
        if (!el.querySelector('.element-loader')) {
            el.classList.remove('element-loading');
        }
    }

    showTableLoading(tableElement, rowCount = 5) {
        const table = typeof tableElement === 'string' ? document.querySelector(tableElement) : tableElement;
        if (!table) return;

        const tbody = table.querySelector('tbody');
        const colCount = table.querySelector('thead tr')?.children.length || 3;
        
        const loadingRow = document.createElement('tr');
        loadingRow.className = 'table-loading-row';
        loadingRow.innerHTML = `
            <td colspan="${colCount}" class="text-center p-4">
                <div class="table-loading-content">
                    <div class="spinner-border text-primary" role="status">
                        <span class="sr-only">Loading...</span>
                    </div>
                    <div class="mt-2">Loading data...</div>
                </div>
            </td>
        `;
        
        if (tbody) {
            tbody.innerHTML = '';
            tbody.appendChild(loadingRow);
        }
    }

    hideTableLoading(tableElement) {
        const table = typeof tableElement === 'string' ? document.querySelector(tableElement) : tableElement;
        if (!table) return;

        const loadingRows = table.querySelectorAll('.table-loading-row');
        loadingRows.forEach(row => row.remove());
    }
}

// Initialize global loading manager
const loadingManager = new LoadingManager();

// Form validation enhancement
// Real-Time Notification System with SignalR
class RealTimeNotificationManager {
    constructor() {
        this.connection = null;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 10;
        this.isConnected = false;
        this.initialize();
    }

    async initialize() {
        // Only initialize if user is logged in
        const userId = document.querySelector('[data-user-id]')?.getAttribute('data-user-id');
        if (!userId) return;

        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/notificationHub")
                .withAutomaticReconnect([0, 2000, 10000, 30000])
                .build();

            this.setupEventHandlers();
            await this.startConnection();
        } catch (error) {
            console.warn('SignalR initialization failed:', error);
        }
    }

    setupEventHandlers() {
        // Connection events
        this.connection.onreconnecting(() => {
            this.isConnected = false;
            console.log('SignalR reconnecting...');
        });

        this.connection.onreconnected(() => {
            this.isConnected = true;
            this.reconnectAttempts = 0;
            console.log('SignalR reconnected');
            showSuccess('Connection Restored', 'Real-time notifications are working again');
        });

        this.connection.onclose(() => {
            this.isConnected = false;
            console.log('SignalR connection closed');
        });

        // Message handlers
        this.connection.on('Connected', (data) => {
            this.isConnected = true;
            console.log('SignalR connected:', data.message);
        });

        this.connection.on('ReceiveNotification', (notification) => {
            this.handleNotification(notification);
        });

        this.connection.on('ReceiveProjectUpdate', (update) => {
            this.handleProjectUpdate(update);
        });

        this.connection.on('UserStatusChanged', (data) => {
            this.handleUserStatusChange(data);
        });
    }

    async startConnection() {
        try {
            await this.connection.start();
            this.isConnected = true;
            console.log('SignalR connection established');
        } catch (error) {
            console.error('Failed to start SignalR connection:', error);
            this.scheduleReconnect();
        }
    }

    scheduleReconnect() {
        if (this.reconnectAttempts < this.maxReconnectAttempts) {
            const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 30000);
            setTimeout(() => {
                this.reconnectAttempts++;
                this.startConnection();
            }, delay);
        }
    }

    handleNotification(notification) {
        // Show toast notification
        showToast(notification.type, notification.title, notification.message);

        // Update notification badge if present
        this.updateNotificationBadge();

        // Play notification sound (optional)
        this.playNotificationSound();

        // Store notification for later viewing
        this.storeNotification(notification);
    }

    handleProjectUpdate(update) {
        // Handle project-specific updates
        showToast(update.type, update.title, update.message);
        
        // Update project page if currently viewing this project
        const currentProjectId = this.getCurrentProjectId();
        if (currentProjectId && currentProjectId == update.projectId) {
            this.refreshProjectData();
        }
    }

    handleUserStatusChange(data) {
        // Update user status indicators
        const userElements = document.querySelectorAll(`[data-user-id="${data.userId}"]`);
        userElements.forEach(element => {
            const statusIndicator = element.querySelector('.user-status');
            if (statusIndicator) {
                statusIndicator.textContent = data.status;
                statusIndicator.className = `user-status status-${data.status.toLowerCase()}`;
            }
        });
    }

    updateNotificationBadge() {
        const badge = document.querySelector('.notification-badge');
        if (badge) {
            const currentCount = parseInt(badge.textContent) || 0;
            badge.textContent = currentCount + 1;
            badge.classList.add('badge-pulse');
            setTimeout(() => badge.classList.remove('badge-pulse'), 1000);
        }
    }

    playNotificationSound() {
        // Optional: Play notification sound
        try {
            const audio = new Audio('/sounds/notification.mp3');
            audio.volume = 0.3;
            audio.play().catch(() => {
                // Ignore audio play errors (autoplay policy)
            });
        } catch (error) {
            // Audio not available, ignore
        }
    }

    storeNotification(notification) {
        // Store in local storage for offline viewing
        try {
            const notifications = JSON.parse(localStorage.getItem('recentNotifications') || '[]');
            notifications.unshift(notification);
            notifications.splice(50); // Keep only latest 50
            localStorage.setItem('recentNotifications', JSON.stringify(notifications));
        } catch (error) {
            console.warn('Failed to store notification:', error);
        }
    }

    getCurrentProjectId() {
        // Extract project ID from current page
        const match = window.location.pathname.match(/\/Projects\/Details\/(\d+)/);
        return match ? parseInt(match[1]) : null;
    }

    refreshProjectData() {
        // Refresh project data if on project details page
        const refreshBtn = document.querySelector('.refresh-project-data');
        if (refreshBtn) {
            refreshBtn.click();
        }
    }

    // Public methods for other parts of the application
    async joinProjectGroup(projectId) {
        if (this.isConnected) {
            try {
                await this.connection.invoke('JoinProjectGroup', projectId);
            } catch (error) {
                console.warn('Failed to join project group:', error);
            }
        }
    }

    async leaveProjectGroup(projectId) {
        if (this.isConnected) {
            try {
                await this.connection.invoke('LeaveProjectGroup', projectId);
            } catch (error) {
                console.warn('Failed to leave project group:', error);
            }
        }
    }

    async updateUserStatus(status) {
        if (this.isConnected) {
            try {
                await this.connection.invoke('UpdateUserStatus', status);
            } catch (error) {
                console.warn('Failed to update user status:', error);
            }
        }
    }
}

// Initialize real-time notifications
let realTimeNotifications = null;

document.addEventListener('DOMContentLoaded', function() {
    // Initialize SignalR if user is logged in
    const userId = document.querySelector('[data-user-id]')?.getAttribute('data-user-id');
    if (userId) {
        realTimeNotifications = new RealTimeNotificationManager();
    }

    // Enhanced form submissions with better loading states
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const submitBtn = form.querySelector('button[type="submit"], input[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                loadingManager.setButtonLoading(submitBtn, true);

                // Re-enable after reasonable timeout as fallback
                setTimeout(() => {
                    loadingManager.setButtonLoading(submitBtn, false);
                }, 10000);
            }
        });
    });

    // Page navigation loading
    document.addEventListener('click', function(e) {
        const link = e.target.closest('a[href]');
        if (link && !link.href.includes('#') && !link.href.includes('javascript:') && 
            !link.hasAttribute('download') && !link.target === '_blank') {
            loadingManager.showPageProgress();
        }
    });

    // Hide page loader when page loads
    window.addEventListener('load', function() {
        loadingManager.hidePageProgress();
    });

    // Auto-join project group when viewing project details
    const projectId = realTimeNotifications?.getCurrentProjectId();
    if (projectId && realTimeNotifications) {
        realTimeNotifications.joinProjectGroup(projectId);
    }

    // Update user status on activity
    let statusTimeout;
    document.addEventListener('click', function() {
        if (realTimeNotifications) {
            realTimeNotifications.updateUserStatus('active');
            clearTimeout(statusTimeout);
            statusTimeout = setTimeout(() => {
                realTimeNotifications.updateUserStatus('idle');
            }, 300000); // 5 minutes
        }
    });

    // Check for server-side messages in TempData and show as toasts
    const serverMessages = document.querySelector('[data-server-messages]');
    if (serverMessages) {
        try {
            const messages = JSON.parse(serverMessages.textContent);
            messages.forEach(msg => {
                showToast(msg.Type || msg.type || 'info', msg.Title || msg.title || 'Notification', msg.Message || msg.message, msg.Duration || msg.duration);
            });
        } catch (e) {
            console.warn('Failed to parse server messages for toast notifications');
        }
    }
});

// File upload progress enhancement
function enhanceFileUpload(inputElement) {
    if (!inputElement || inputElement.type !== 'file') return;

    inputElement.addEventListener('change', function(e) {
        const files = e.target.files;
        if (files.length > 0) {
            const fileNames = Array.from(files).map(f => f.name).join(', ');
            showInfo('Files Selected', `Selected: ${fileNames}`, 3000);
        }
    });
}

// Auto-enhance all file inputs on page load
document.addEventListener('DOMContentLoaded', function() {
    const fileInputs = document.querySelectorAll('input[type="file"]');
    fileInputs.forEach(enhanceFileUpload);
});

// AJAX helper with toast notifications
function ajaxRequest(url, options = {}) {
    const defaultOptions = {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    };

    const finalOptions = { ...defaultOptions, ...options };

    return fetch(url, finalOptions)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .catch(error => {
            showError('Request Failed', error.message || 'An unexpected error occurred');
            throw error;
        });
}

// Role Management Functions
function confirmDeleteRole(roleId, roleName) {
    if (confirm(`Are you sure you want to delete the role "${roleName}"? This action cannot be undone.`)) {
        deleteRole(roleId, roleName);
    }
}

function deleteRole(roleId, roleName) {
    if (confirm(`This will permanently delete the role "${roleName}". Continue?`)) {
        window.location.href = `/RoleManagement/DeleteRole/${roleId}`;
    }
}

function assignRole(userId) {
    const roleSelect = document.querySelector(`#roleSelect${userId}`);
    if (roleSelect && roleSelect.value) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/RoleManagement/AssignRole';
        
        const userIdInput = document.createElement('input');
        userIdInput.type = 'hidden';
        userIdInput.name = 'userId';
        userIdInput.value = userId;
        
        const roleIdInput = document.createElement('input');
        roleIdInput.type = 'hidden';
        roleIdInput.name = 'roleId';
        roleIdInput.value = roleSelect.value;
        
        form.appendChild(userIdInput);
        form.appendChild(roleIdInput);
        document.body.appendChild(form);
        form.submit();
    }
}

function removeRole(userId, roleId, roleName) {
    if (confirm(`Remove the role "${roleName}" from this user?`)) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/RoleManagement/RemoveRole';
        
        const userIdInput = document.createElement('input');
        userIdInput.type = 'hidden';
        userIdInput.name = 'userId';
        userIdInput.value = userId;
        
        const roleIdInput = document.createElement('input');
        roleIdInput.type = 'hidden';
        roleIdInput.name = 'roleId';
        roleIdInput.value = roleId;
        
        form.appendChild(userIdInput);
        form.appendChild(roleIdInput);
        document.body.appendChild(form);
        form.submit();
    }
}
