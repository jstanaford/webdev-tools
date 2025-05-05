document.addEventListener('DOMContentLoaded', function() {
    // Tab switching functionality
    window.switchTab = function(tabName) {
        // Hide all tab contents
        document.querySelectorAll('.tab-content').forEach(content => {
            content.classList.add('hidden');
        });
        
        // Show selected tab content
        document.getElementById(`${tabName}-content`).classList.remove('hidden');
        
        // Update tab button styles
        document.querySelectorAll('.tab-button').forEach(button => {
            button.classList.remove('border-indigo-500', 'text-indigo-600');
            button.classList.add('border-transparent', 'text-gray-500');
        });
        
        // Highlight active tab
        const activeButton = document.getElementById(`${tabName}-tab`);
        activeButton.classList.remove('border-transparent', 'text-gray-500');
        activeButton.classList.add('border-indigo-500', 'text-indigo-600');
    };

    // Common Port Scanner Form
    const form = document.getElementById('portScannerForm');
    if (form) {
        const resultsContainer = document.getElementById('scanResults');
        
        form.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const url = document.getElementById('url').value;
            const submitButton = form.querySelector('button[type="submit"]');
            
            try {
                submitButton.disabled = true;
                submitButton.innerHTML = '<span class="inline-flex items-center"><svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>Scanning...</span>';
                
                const response = await fetch('/api/portscanner/scan', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ url })
                });
                
                if (!response.ok) {
                    throw new Error('Scan failed');
                }
                
                const results = await response.json();
                resultsContainer.innerHTML = generateResultsTable(results, url);

                // Add event listener for export button
                document.getElementById('exportButton')?.addEventListener('click', () => {
                    exportResults(results, url);
                });
                
            } catch (error) {
                resultsContainer.innerHTML = `
                    <div class="mt-8 bg-red-50 p-4 rounded-lg">
                        <p class="text-red-700">Error: ${error.message}</p>
                    </div>`;
            } finally {
                submitButton.disabled = false;
                submitButton.textContent = 'Start Scan';
            }
        });
    }

    // Direct Port Scanner Form
    const directForm = document.getElementById('directScanForm');
    if (directForm) {
        const directResultsContainer = document.getElementById('directScanResults');
        
        directForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const url = document.getElementById('directUrl').value;
            const port = parseInt(document.getElementById('port').value);
            const submitButton = directForm.querySelector('button[type="submit"]');
            
            try {
                submitButton.disabled = true;
                submitButton.innerHTML = '<span class="inline-flex items-center"><svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>Scanning...</span>';
                
                const response = await fetch('/api/portscanner/direct-scan', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ url, port })
                });
                
                if (!response.ok) {
                    throw new Error('Scan failed');
                }
                
                const result = await response.json();
                directResultsContainer.innerHTML = generateDirectScanResult(result, url);
                
            } catch (error) {
                directResultsContainer.innerHTML = `
                    <div class="mt-8 bg-red-50 p-4 rounded-lg">
                        <p class="text-red-700">Error: ${error.message}</p>
                    </div>`;
            } finally {
                submitButton.disabled = false;
                submitButton.textContent = 'Scan Port';
            }
        });
    }
});

function generateResultsTable(results, url) {
    const openPorts = results.filter(r => r.isOpen).length;
    const closedPorts = results.length - openPorts;

    return `
        <div class="mt-8 bg-white rounded-xl ring-1 ring-gray-200 shadow-sm">
            <div class="p-6 border-b border-gray-200">
                <div class="flex items-center justify-between">
                    <h3 class="text-lg font-semibold text-gray-900">Scan Results for ${url}</h3>
                    <button id="exportButton" 
                            class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                        <svg class="h-4 w-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>
                        Export Results
                    </button>
                </div>
                <div class="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
                    <div class="bg-green-50 rounded-lg p-4">
                        <div class="flex items-center">
                            <div class="flex-shrink-0">
                                <svg class="h-6 w-6 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                            </div>
                            <div class="ml-3">
                                <h3 class="text-sm font-medium text-green-800">Open Ports</h3>
                                <div class="mt-1 text-lg font-semibold text-green-900">${openPorts}</div>
                            </div>
                        </div>
                    </div>
                    <div class="bg-red-50 rounded-lg p-4">
                        <div class="flex items-center">
                            <div class="flex-shrink-0">
                                <svg class="h-6 w-6 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                            </div>
                            <div class="ml-3">
                                <h3 class="text-sm font-medium text-red-800">Closed Ports</h3>
                                <div class="mt-1 text-lg font-semibold text-red-900">${closedPorts}</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="overflow-x-auto">
                <table class="min-w-full divide-y divide-gray-200">
                    <thead class="bg-gray-50">
                        <tr>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Port</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Service</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                        </tr>
                    </thead>
                    <tbody class="bg-white divide-y divide-gray-200">
                        ${results.map(result => `
                            <tr class="hover:bg-gray-50">
                                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">${result.port}</td>
                                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${result.service}</td>
                                <td class="px-6 py-4 whitespace-nowrap text-sm">
                                    ${result.isOpen 
                                        ? '<span class="inline-flex items-center rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800">Open</span>'
                                        : '<span class="inline-flex items-center rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800">Closed</span>'}
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        </div>`;
}

function exportResults(results, url) {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const filename = `port-scan-${url}-${timestamp}.csv`;
    
    const csvContent = [
        ['Port', 'Service', 'Status'],
        ...results.map(r => [r.port, r.service, r.isOpen ? 'Open' : 'Closed'])
    ].map(row => row.join(',')).join('\n');
    
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function generateDirectScanResult(result, url) {
    return `
        <div class="mt-8 bg-white rounded-xl ring-1 ring-gray-200 shadow-sm">
            <div class="p-6">
                <h3 class="text-lg font-semibold text-gray-900 mb-4">Scan Result for ${url}:${result.port}</h3>
                <div class="bg-gray-50 rounded-lg p-6">
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div>
                            <label class="text-sm font-medium text-gray-500">Port</label>
                            <p class="mt-1 text-lg font-semibold text-gray-900">${result.port}</p>
                        </div>
                        <div>
                            <label class="text-sm font-medium text-gray-500">Service</label>
                            <p class="mt-1 text-lg font-semibold text-gray-900">${result.service}</p>
                        </div>
                        <div>
                            <label class="text-sm font-medium text-gray-500">Status</label>
                            <p class="mt-1">
                                ${result.isOpen 
                                    ? '<span class="inline-flex items-center rounded-full bg-green-100 px-2.5 py-0.5 text-sm font-medium text-green-800">Open</span>'
                                    : '<span class="inline-flex items-center rounded-full bg-red-100 px-2.5 py-0.5 text-sm font-medium text-red-800">Closed</span>'}
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>`;
} 