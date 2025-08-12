// Advanced Table Manager for enhanced table functionality
class AdvancedTableManager {
    constructor() {
        this.tables = new Map();
        this.initializeAllTables();
    }

    initializeAllTables() {
        document.querySelectorAll('[data-enhanced-table]').forEach(table => {
            this.enhanceTable(table);
        });
    }

    enhanceTable(tableElement) {
        const tableId = tableElement.id || 'table_' + Date.now();
        if (!tableElement.id) {
            tableElement.id = tableId;
        }

        const config = {
            sortable: tableElement.hasAttribute('data-sortable'),
            filterable: tableElement.hasAttribute('data-filterable'),
            pageable: tableElement.hasAttribute('data-pageable'),
            pageSize: parseInt(tableElement.getAttribute('data-page-size')) || 10,
            searchable: tableElement.hasAttribute('data-searchable')
        };

        const tableManager = new TableInstance(tableElement, config);
        this.tables.set(tableId, tableManager);

        return tableManager;
    }

    getTable(tableId) {
        return this.tables.get(tableId);
    }
}

class TableInstance {
    constructor(tableElement, config) {
        this.table = tableElement;
        this.config = config;
        this.data = [];
        this.filteredData = [];
        this.currentPage = 1;
        this.sortColumn = null;
        this.sortDirection = 'asc';
        this.filters = new Map();

        this.init();
    }

    init() {
        this.extractData();
        this.createControls();
        this.bindEvents();
        this.applyEnhancements();
    }

    extractData() {
        const tbody = this.table.querySelector('tbody');
        if (!tbody) return;

        this.data = Array.from(tbody.querySelectorAll('tr')).map((row, index) => {
            const cells = Array.from(row.querySelectorAll('td')).map(cell => ({
                text: cell.textContent.trim(),
                html: cell.innerHTML,
                element: cell
            }));
            return {
                index: index,
                element: row,
                cells: cells,
                visible: true
            };
        });

        this.filteredData = [...this.data];
    }

    createControls() {
        const controlsContainer = document.createElement('div');
        controlsContainer.className = 'table-controls mb-3';
        controlsContainer.innerHTML = this.generateControlsHTML();
        
        this.table.parentNode.insertBefore(controlsContainer, this.table);
        this.controlsContainer = controlsContainer;
    }

    generateControlsHTML() {
        let html = '<div class="row align-items-center">';
        
        // Search
        if (this.config.searchable) {
            html += `
                <div class="col-md-4">
                    <div class="input-group">
                        <span class="input-group-text"><i class="fas fa-search"></i></span>
                        <input type="text" class="form-control" id="${this.table.id}_search" placeholder="Search table...">
                    </div>
                </div>
            `;
        }

        // Page size selector
        if (this.config.pageable) {
            html += `
                <div class="col-md-2">
                    <select class="form-select" id="${this.table.id}_pagesize">
                        <option value="5">5 per page</option>
                        <option value="10" ${this.config.pageSize === 10 ? 'selected' : ''}>10 per page</option>
                        <option value="25" ${this.config.pageSize === 25 ? 'selected' : ''}>25 per page</option>
                        <option value="50" ${this.config.pageSize === 50 ? 'selected' : ''}>50 per page</option>
                        <option value="100" ${this.config.pageSize === 100 ? 'selected' : ''}>100 per page</option>
                    </select>
                </div>
            `;
        }

        // Export buttons
        html += `
            <div class="col-md-4 ms-auto">
                <div class="btn-group" role="group">
                    <button type="button" class="btn btn-outline-primary btn-sm" id="${this.table.id}_export_csv">
                        <i class="fas fa-file-csv me-1"></i>CSV
                    </button>
                    <button type="button" class="btn btn-outline-primary btn-sm" id="${this.table.id}_export_excel">
                        <i class="fas fa-file-excel me-1"></i>Excel
                    </button>
                    <button type="button" class="btn btn-outline-primary btn-sm" id="${this.table.id}_print">
                        <i class="fas fa-print me-1"></i>Print
                    </button>
                </div>
            </div>
        `;

        html += '</div>';

        // Pagination
        if (this.config.pageable) {
            html += `
                <div class="row mt-2">
                    <div class="col-md-6">
                        <div class="table-info">
                            Showing <span id="${this.table.id}_start">1</span> to <span id="${this.table.id}_end">10</span> 
                            of <span id="${this.table.id}_total">${this.data.length}</span> entries
                        </div>
                    </div>
                    <div class="col-md-6">
                        <nav>
                            <ul class="pagination pagination-sm justify-content-end" id="${this.table.id}_pagination">
                            </ul>
                        </nav>
                    </div>
                </div>
            `;
        }

        return html;
    }

    bindEvents() {
        // Search functionality
        if (this.config.searchable) {
            const searchInput = document.getElementById(`${this.table.id}_search`);
            searchInput.addEventListener('input', (e) => {
                this.search(e.target.value);
            });
        }

        // Page size change
        if (this.config.pageable) {
            const pageSizeSelect = document.getElementById(`${this.table.id}_pagesize`);
            pageSizeSelect.addEventListener('change', (e) => {
                this.config.pageSize = parseInt(e.target.value);
                this.currentPage = 1;
                this.render();
            });
        }

        // Export buttons
        document.getElementById(`${this.table.id}_export_csv`)?.addEventListener('click', () => {
            this.exportToCSV();
        });

        document.getElementById(`${this.table.id}_export_excel`)?.addEventListener('click', () => {
            this.exportToExcel();
        });

        document.getElementById(`${this.table.id}_print`)?.addEventListener('click', () => {
            this.printTable();
        });

        // Sorting
        if (this.config.sortable) {
            const headers = this.table.querySelectorAll('thead th[data-sortable]');
            headers.forEach((header, index) => {
                header.style.cursor = 'pointer';
                header.innerHTML += ' <i class="fas fa-sort text-muted"></i>';
                
                header.addEventListener('click', () => {
                    this.sort(index, header.getAttribute('data-sort-type') || 'string');
                });
            });
        }
    }

    applyEnhancements() {
        // Add hover effects
        this.table.classList.add('table-hover');
        
        // Add responsive wrapper if not present
        if (!this.table.closest('.table-responsive')) {
            const wrapper = document.createElement('div');
            wrapper.className = 'table-responsive';
            this.table.parentNode.insertBefore(wrapper, this.table);
            wrapper.appendChild(this.table);
        }

        this.render();
    }

    search(query) {
        if (!query.trim()) {
            this.filteredData = [...this.data];
        } else {
            const searchTerm = query.toLowerCase();
            this.filteredData = this.data.filter(row => {
                return row.cells.some(cell => 
                    cell.text.toLowerCase().includes(searchTerm)
                );
            });
        }
        
        this.currentPage = 1;
        this.render();
    }

    sort(columnIndex, sortType = 'string') {
        if (this.sortColumn === columnIndex) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = columnIndex;
            this.sortDirection = 'asc';
        }

        this.filteredData.sort((a, b) => {
            let valueA = a.cells[columnIndex]?.text || '';
            let valueB = b.cells[columnIndex]?.text || '';

            // Type-specific sorting
            switch (sortType) {
                case 'number':
                    valueA = parseFloat(valueA) || 0;
                    valueB = parseFloat(valueB) || 0;
                    break;
                case 'date':
                    valueA = new Date(valueA);
                    valueB = new Date(valueB);
                    break;
                default:
                    valueA = valueA.toLowerCase();
                    valueB = valueB.toLowerCase();
            }

            let result = 0;
            if (valueA < valueB) result = -1;
            if (valueA > valueB) result = 1;

            return this.sortDirection === 'desc' ? -result : result;
        });

        this.updateSortIndicators();
        this.render();
    }

    updateSortIndicators() {
        // Clear all sort indicators
        const headers = this.table.querySelectorAll('thead th i.fas');
        headers.forEach(icon => {
            icon.className = 'fas fa-sort text-muted';
        });

        // Set current sort indicator
        if (this.sortColumn !== null) {
            const currentHeader = this.table.querySelectorAll('thead th')[this.sortColumn];
            const icon = currentHeader.querySelector('i.fas');
            if (icon) {
                icon.className = `fas fa-sort-${this.sortDirection === 'asc' ? 'up' : 'down'} text-primary`;
            }
        }
    }

    render() {
        if (this.config.pageable) {
            this.renderPaginated();
        } else {
            this.renderAll();
        }
        
        this.updateInfo();
    }

    renderPaginated() {
        const startIndex = (this.currentPage - 1) * this.config.pageSize;
        const endIndex = startIndex + this.config.pageSize;
        const pageData = this.filteredData.slice(startIndex, endIndex);

        this.renderRows(pageData);
        this.renderPagination();
    }

    renderAll() {
        this.renderRows(this.filteredData);
    }

    renderRows(data) {
        const tbody = this.table.querySelector('tbody');
        if (!tbody) return;

        // Hide all rows first
        this.data.forEach(row => {
            row.element.style.display = 'none';
        });

        // Show filtered rows
        data.forEach(row => {
            row.element.style.display = '';
        });

        // Show "no results" message if empty
        if (data.length === 0) {
            if (!tbody.querySelector('.no-results-row')) {
                const noResultsRow = document.createElement('tr');
                noResultsRow.className = 'no-results-row';
                noResultsRow.innerHTML = `
                    <td colspan="${this.table.querySelectorAll('thead th').length}" class="text-center p-4">
                        <div class="text-muted">
                            <i class="fas fa-search fa-2x mb-2"></i>
                            <p>No matching records found</p>
                        </div>
                    </td>
                `;
                tbody.appendChild(noResultsRow);
            }
        } else {
            const noResultsRow = tbody.querySelector('.no-results-row');
            if (noResultsRow) {
                noResultsRow.remove();
            }
        }
    }

    renderPagination() {
        const totalPages = Math.ceil(this.filteredData.length / this.config.pageSize);
        const pagination = document.getElementById(`${this.table.id}_pagination`);
        
        if (!pagination) return;

        let html = '';

        // Previous button
        html += `
            <li class="page-item ${this.currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${this.currentPage - 1}">
                    <i class="fas fa-chevron-left"></i>
                </a>
            </li>
        `;

        // Page numbers
        const maxVisiblePages = 5;
        let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
        let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

        if (endPage - startPage + 1 < maxVisiblePages) {
            startPage = Math.max(1, endPage - maxVisiblePages + 1);
        }

        for (let i = startPage; i <= endPage; i++) {
            html += `
                <li class="page-item ${i === this.currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `;
        }

        // Next button
        html += `
            <li class="page-item ${this.currentPage === totalPages ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${this.currentPage + 1}">
                    <i class="fas fa-chevron-right"></i>
                </a>
            </li>
        `;

        pagination.innerHTML = html;

        // Bind pagination events
        pagination.addEventListener('click', (e) => {
            e.preventDefault();
            const link = e.target.closest('.page-link');
            if (link && !link.closest('.page-item').classList.contains('disabled')) {
                const page = parseInt(link.getAttribute('data-page'));
                this.goToPage(page);
            }
        });
    }

    goToPage(page) {
        const totalPages = Math.ceil(this.filteredData.length / this.config.pageSize);
        if (page >= 1 && page <= totalPages) {
            this.currentPage = page;
            this.render();
        }
    }

    updateInfo() {
        if (!this.config.pageable) return;

        const startElement = document.getElementById(`${this.table.id}_start`);
        const endElement = document.getElementById(`${this.table.id}_end`);
        const totalElement = document.getElementById(`${this.table.id}_total`);

        if (startElement && endElement && totalElement) {
            const start = (this.currentPage - 1) * this.config.pageSize + 1;
            const end = Math.min(this.currentPage * this.config.pageSize, this.filteredData.length);
            
            startElement.textContent = this.filteredData.length > 0 ? start : 0;
            endElement.textContent = end;
            totalElement.textContent = this.filteredData.length;
        }
    }

    exportToCSV() {
        const headers = Array.from(this.table.querySelectorAll('thead th')).map(th => 
            th.textContent.trim().replace(/\s+/g, ' ')
        );
        
        const rows = this.filteredData.map(row => 
            row.cells.map(cell => `"${cell.text.replace(/"/g, '""')}"`).join(',')
        );

        const csv = [headers.join(','), ...rows].join('\n');
        this.downloadFile(csv, 'table-export.csv', 'text/csv');
    }

    exportToExcel() {
        // Simple Excel export using HTML table
        const table = this.table.cloneNode(true);
        const tbody = table.querySelector('tbody');
        
        // Clear tbody and add only filtered data
        tbody.innerHTML = '';
        this.filteredData.forEach(row => {
            tbody.appendChild(row.element.cloneNode(true));
        });

        const html = `
            <html>
            <head>
                <meta charset="utf-8">
                <style>table { border-collapse: collapse; } th, td { border: 1px solid #ddd; padding: 8px; }</style>
            </head>
            <body>${table.outerHTML}</body>
            </html>
        `;
        
        this.downloadFile(html, 'table-export.xls', 'application/vnd.ms-excel');
    }

    printTable() {
        const table = this.table.cloneNode(true);
        const tbody = table.querySelector('tbody');
        
        // Clear tbody and add only filtered data
        tbody.innerHTML = '';
        this.filteredData.forEach(row => {
            tbody.appendChild(row.element.cloneNode(true));
        });

        const printWindow = window.open('', '_blank');
        printWindow.document.write(`
            <html>
            <head>
                <title>Table Print</title>
                <style>
                    body { font-family: Arial, sans-serif; }
                    table { border-collapse: collapse; width: 100%; }
                    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                    th { background-color: #f2f2f2; }
                </style>
            </head>
            <body>
                <h2>Table Export</h2>
                ${table.outerHTML}
            </body>
            </html>
        `);
        printWindow.document.close();
        printWindow.print();
    }

    downloadFile(content, filename, contentType) {
        const blob = new Blob([content], { type: contentType });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
    }
}

// Initialize advanced table manager
let advancedTableManager;

document.addEventListener('DOMContentLoaded', function() {
    advancedTableManager = new AdvancedTableManager();
});

// Export for global access
window.advancedTableManager = advancedTableManager;