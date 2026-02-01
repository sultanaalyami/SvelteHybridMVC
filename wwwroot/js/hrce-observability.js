const table = document.getElementById('log-table');
const logCount = document.getElementById('log-count');
const errorRate = document.getElementById('error-rate');

const normalizeLevel = (level = '') => level.toUpperCase();

const render = (entries) => {
    if (!entries.length) {
        table.innerHTML = '<tr><td colspan="4">·«  ÊÃœ ”Ã·«  »⁄œ</td></tr>';
        logCount.textContent = '0';
        errorRate.textContent = '0%';
        return;
    }

    const errors = entries.filter(entry => ['ERROR', 'CRITICAL'].includes(normalizeLevel(entry.level))).length;
    const rate = Math.round((errors / entries.length) * 100);

    logCount.textContent = entries.length.toString();
    errorRate.textContent = `${rate}%`;

    table.innerHTML = entries.map(entry => {
        const timestamp = new Date(entry.timestamp).toLocaleTimeString('ar-SA');
        const level = normalizeLevel(entry.level);
        const message = entry.message ?? '';
        const path = entry.path ?? '-';
        return `
            <tr>
                <td>${timestamp}</td>
                <td>${level}</td>
                <td>${message}</td>
                <td>${path}</td>
            </tr>
        `;
    }).join('');
};

const fetchLogs = async () => {
    try {
        const response = await fetch('/api/logs/recent?take=200');
        const data = await response.json();
        render(data);
    } catch {
        table.innerHTML = '<tr><td colspan="4"> ⁄–—  Õ„Ì· «·”Ã·« </td></tr>';
    }
};

fetchLogs();
setInterval(fetchLogs, 4000);
