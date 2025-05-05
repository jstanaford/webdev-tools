document.getElementById('analyzeForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const url = document.getElementById('targetUrl').value;
    const loadingIndicator = document.getElementById('loadingIndicator');
    const resultsContainer = document.getElementById('resultsContainer');
    
    try {
        loadingIndicator.classList.remove('hidden');
        resultsContainer.classList.add('hidden');
        
        const response = await fetch(`/api/analyzer/analyze?url=${encodeURIComponent(url)}`);
        
        if (!response.ok) {
            throw new Error('Analysis failed');
        }
        
        const data = await response.json();
        displayResults(data);
    } catch (error) {
        showError('An error occurred while analyzing the website. Please try again.');
    } finally {
        loadingIndicator.classList.add('hidden');
    }
});

function displayResults(profile) {
    const resultsContainer = document.getElementById('resultsContainer');
    resultsContainer.innerHTML = `
        <div class="p-8">
            <h2 class="text-2xl font-semibold text-gray-900 mb-8">Analysis Results</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div class="bg-gray-50 rounded-lg p-6">
                    <h3 class="text-lg font-semibold text-gray-900 mb-4">Server Information</h3>
                    <div class="space-y-2">
                        <p class="text-sm text-gray-600"><span class="font-medium text-gray-900">Server Type:</span> ${profile.serverType || 'Unknown'}</p>
                        <p class="text-sm text-gray-600"><span class="font-medium text-gray-900">Scanned At:</span> ${new Date(profile.scannedAt).toLocaleString()}</p>
                    </div>
                </div>

                <div class="bg-gray-50 rounded-lg p-6">
                    <h3 class="text-lg font-semibold text-gray-900 mb-4">JavaScript Libraries</h3>
                    <ul class="space-y-1">
                        ${profile.javaScriptLibraries.map(lib => `
                            <li class="text-sm text-gray-600 flex items-center">
                                <svg class="h-4 w-4 text-indigo-500 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                                </svg>
                                ${lib.name} ${lib.version ? `v${lib.version}` : ''} ${lib.confidence < 1 ? `(${Math.round(lib.confidence * 100)}% confidence)` : ''}
                            </li>
                        `).join('')}
                    </ul>
                </div>

                <div class="bg-gray-50 rounded-lg p-6">
                    <h3 class="text-lg font-semibold text-gray-900 mb-4">Meta Technologies</h3>
                    <ul class="space-y-1">
                        ${profile.metaTechnologies.map(tech => `
                            <li class="text-sm text-gray-600 flex items-center">
                                <svg class="h-4 w-4 text-indigo-500 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                ${tech}
                            </li>
                        `).join('')}
                    </ul>
                </div>

                <div class="bg-gray-50 rounded-lg p-6">
                    <h3 class="text-lg font-semibold text-gray-900 mb-4">Detected Technologies</h3>
                    <ul class="space-y-1">
                        ${profile.detectedTechnologies.map(tech => `
                            <li class="text-sm text-gray-600">
                                <div class="flex items-center justify-between">
                                    <div class="flex items-center">
                                        <svg class="h-4 w-4 text-indigo-500 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                        </svg>
                                        ${tech.name} (${tech.category})
                                    </div>
                                    <span class="text-xs text-gray-500">
                                        ${tech.version !== 'Unknown' ? `v${tech.version}` : ''} 
                                        ${tech.confidenceLevel ? `- ${tech.confidenceLevel} confidence` : ''}
                                    </span>
                                </div>
                            </li>
                        `).join('')}
                    </ul>
                </div>

                <div class="bg-gray-50 rounded-lg p-6">
                    <h3 class="text-lg font-semibold text-gray-900 mb-4">Response Headers</h3>
                    <div class="space-y-2">
                        ${Object.entries(profile.headers).map(([key, value]) => `
                            <div class="text-sm">
                                <span class="font-medium text-gray-900">${key}:</span>
                                <span class="text-gray-600">${value}</span>
                            </div>
                        `).join('')}
                    </div>
                </div>
            </div>
        </div>
    `;
    resultsContainer.classList.remove('hidden');
}

function showError(message) {
    const resultsContainer = document.getElementById('resultsContainer');
    resultsContainer.innerHTML = `
        <div class="p-8">
            <div class="rounded-lg bg-red-50 p-4">
                <div class="flex">
                    <div class="flex-shrink-0">
                        <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
                        </svg>
                    </div>
                    <div class="ml-3">
                        <h3 class="text-sm font-medium text-red-800">Error</h3>
                        <div class="mt-2 text-sm text-red-700">
                            <p>${message}</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
    resultsContainer.classList.remove('hidden');
} 