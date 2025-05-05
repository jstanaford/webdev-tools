document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('hostViewerForm');
    const resultsContainer = document.getElementById('resultsContainer');
    
    form.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        const url = document.getElementById('url').value;
        const ipAddress = document.getElementById('ipAddress').value;
        
        try {
            const response = await fetch('/api/hostviewer/analyze', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ url, ipAddress })
            });
            
            const data = await response.json();
            
            resultsContainer.innerHTML = `
                <div class="mt-16 bg-white rounded-xl ring-1 ring-gray-200 shadow-sm p-8">
                    <h2 class="text-2xl font-semibold text-gray-900 mb-6">Host Information</h2>
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                        <div class="text-left">
                            <h3 class="font-medium text-gray-900 mb-2">Original DNS</h3>
                            <pre class="bg-gray-50 p-4 rounded-lg text-sm">${data.originalDnsInfo}</pre>
                        </div>
                        <div class="text-left">
                            <h3 class="font-medium text-gray-900 mb-2">Modified Host</h3>
                            <pre class="bg-gray-50 p-4 rounded-lg text-sm">${data.modifiedHostInfo}</pre>
                        </div>
                    </div>
                    <div class="mt-8">
                        <a href="/host-viewer/${encodeURIComponent(ipAddress)}?url=${encodeURIComponent(url)}" 
                           class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                            View in Browser
                        </a>
                    </div>
                </div>`;
        } catch (error) {
            resultsContainer.innerHTML = `
                <div class="mt-8 text-red-600">
                    Error analyzing host information. Please try again.
                </div>`;
        }
    });
}); 