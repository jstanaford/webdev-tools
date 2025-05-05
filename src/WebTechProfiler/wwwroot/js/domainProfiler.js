document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('domainProfilerForm');
    const resultsContainer = document.getElementById('profileResults');
    
    if (form) {
        form.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const domain = document.getElementById('domain').value;
            const submitButton = form.querySelector('button[type="submit"]');
            
            try {
                submitButton.disabled = true;
                submitButton.innerHTML = '<span class="inline-flex items-center"><svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>Analyzing...</span>';
                
                const response = await fetch('/api/domainprofiler/analyze', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ domain })
                });
                
                if (!response.ok) {
                    throw new Error('Analysis failed');
                }
                
                const data = await response.json();
                resultsContainer.innerHTML = generateResultsHtml(data);
                
                // Add click handlers for copy buttons after rendering results
                setupCopyButtons();
                
            } catch (error) {
                resultsContainer.innerHTML = `
                    <div class="mt-8 bg-red-50 p-4 rounded-lg">
                        <p class="text-red-700">Error: ${error.message}</p>
                    </div>`;
            } finally {
                submitButton.disabled = false;
                submitButton.textContent = 'Analyze Domain';
            }
        });
    }
});

function setupCopyButtons() {
    document.querySelectorAll('.copy-button').forEach(button => {
        button.addEventListener('click', function() {
            const textToCopy = this.closest('.record-item').querySelector('.record-content').textContent;
            navigator.clipboard.writeText(textToCopy.trim())
                .then(() => {
                    const originalHTML = this.innerHTML;
                    this.innerHTML = `
                        <svg class="h-4 w-4 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                        </svg>`;
                    setTimeout(() => {
                        this.innerHTML = originalHTML;
                    }, 2000);
                });
        });
    });
}

function generateResultsHtml(data) {
    // Store the data in a global variable for the export function to access
    window.dnsRecordsData = data;
    
    return `
        <div class="mt-8 bg-white rounded-xl ring-1 ring-gray-200 shadow-sm">
            <div class="p-6 border-b border-gray-200">
                <div class="flex items-center justify-between">
                    <h2 class="text-xl font-semibold text-gray-900">DNS Records for ${data.domain}</h2>
                    <button onclick="exportResults()" 
                            class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                        <svg class="h-4 w-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>
                        Export Results
                    </button>
                </div>
            </div>
            <div class="p-6">
                <div class="grid grid-cols-1 gap-6">
                    ${Object.entries(data.records).map(([type, records]) => `
                        <div class="bg-gray-50 rounded-lg p-4">
                            <h3 class="text-sm font-medium text-gray-900 mb-2">${type} Records</h3>
                            ${records.length > 0 
                                ? records.map(record => `
                                    <div class="record-item group relative bg-white rounded-lg p-3 mb-2 ring-1 ring-gray-200">
                                        <button class="copy-button absolute right-2 top-2 p-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <svg class="h-4 w-4 text-gray-500 hover:text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5H6a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2v-1M8 5a2 2 0 002 2h2a2 2 0 002-2M8 5a2 2 0 012-2h2a2 2 0 012 2m0 0h2a2 2 0 012 2v3m2 4H10m0 0l3-3m-3 3l3 3" />
                                            </svg>
                                        </button>
                                        <pre class="record-content text-sm text-gray-600 whitespace-pre-wrap">${record}</pre>
                                    </div>
                                `).join('')
                                : '<p class="text-sm text-gray-500">No records found</p>'
                            }
                        </div>
                    `).join('')}
                </div>
            </div>
        </div>`;
}

window.exportResults = function() {
    const data = window.dnsRecordsData;
    if (!data) return;
    
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const filename = `dns-records-${data.domain}-${timestamp}.json`;
    
    const exportData = {
        domain: data.domain,
        timestamp: new Date().toISOString(),
        records: data.records
    };
    
    const jsonString = JSON.stringify(exportData, null, 2);
    const blob = new Blob([jsonString], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}; 