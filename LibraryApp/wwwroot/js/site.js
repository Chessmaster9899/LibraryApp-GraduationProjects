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

// Form validation enhancement
document.addEventListener('DOMContentLoaded', function() {
    // Enhance form submissions with toast notifications
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const submitBtn = form.querySelector('button[type="submit"], input[type="submit"]');
            if (submitBtn) {
                const originalText = submitBtn.textContent || submitBtn.value;
                submitBtn.disabled = true;
                if (submitBtn.textContent !== undefined) {
                    submitBtn.textContent = 'Processing...';
                } else {
                    submitBtn.value = 'Processing...';
                }

                // Re-enable after 3 seconds as fallback
                setTimeout(() => {
                    submitBtn.disabled = false;
                    if (submitBtn.textContent !== undefined) {
                        submitBtn.textContent = originalText;
                    } else {
                        submitBtn.value = originalText;
                    }
                }, 3000);
            }
        });
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
