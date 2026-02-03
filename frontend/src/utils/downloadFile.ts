// File Download Utility
// This utility provides a bulletproof way to download files with correct extensions

export const downloadFile = (blob: Blob, filename: string) => {
    // Try multiple methods in order of preference

    // Method 1: Try modern showSaveFilePicker API (Chrome 86+, Edge 86+)
    if ('showSaveFilePicker' in window) {
        downloadWithFilePicker(blob, filename);
        return;
    }

    // Method 2: Try msSaveBlob for IE/Edge Legacy
    if (typeof (navigator as any).msSaveBlob !== 'undefined') {
        (navigator as any).msSaveBlob(blob, filename);
        return;
    }

    // Method 3: Standard download with multiple fallbacks
    downloadWithLink(blob, filename);
};

const downloadWithFilePicker = async (blob: Blob, filename: string) => {
    try {
        const ext = filename.split('.').pop() || 'xlsx';
        const handle = await (window as any).showSaveFilePicker({
            suggestedName: filename,
            types: [{
                description: 'Excel Spreadsheet',
                accept: { 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': [`.${ext}`] },
            }],
        });
        const writable = await handle.createWritable();
        await writable.write(blob);
        await writable.close();
    } catch (err) {
        // User cancelled or API not supported, fall back
        downloadWithLink(blob, filename);
    }
};

const downloadWithLink = (blob: Blob, filename: string) => {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');

    // Set all attributes before adding to DOM
    link.style.display = 'none';
    link.download = filename;
    link.href = url;
    link.rel = 'noopener';

    // Add to DOM
    document.body.appendChild(link);

    // Trigger click after a microtask to ensure attributes are processed
    requestAnimationFrame(() => {
        link.dispatchEvent(new MouseEvent('click', {
            bubbles: true,
            cancelable: true,
            view: window,
        }));

        // Cleanup
        setTimeout(() => {
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
        }, 200);
    });
};
