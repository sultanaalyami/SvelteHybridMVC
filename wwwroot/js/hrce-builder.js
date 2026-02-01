const pageForm = document.getElementById('page-form');
const ruleForm = document.getElementById('rule-form');
const pageResult = document.getElementById('page-result');
const ruleTable = document.getElementById('rule-table');

const worker = new Worker('/js/hrce-builder.worker.js');
const getAntiForgeryToken = () =>
    document.querySelector('input[name="__RequestVerificationToken"]')?.value;

const fetchRules = async () => {
    try {
        const response = await fetch('/api/builder/rules');
        if (!response.ok) {
            ruleTable.innerHTML = '<tr><td colspan="5">€Ì— „ «Õ</td></tr>';
            return;
        }
        const data = await response.json();
        ruleTable.innerHTML = data.map(rule => {
            const target = rule.controller ?? rule.page ?? rule.area ?? '-';
            const claim = rule.claimType ? `${rule.claimType}:${rule.claimValue ?? ''}` : '-';
            return `
                <tr>
                    <td>${rule.scope}</td>
                    <td>${target}</td>
                    <td>${rule.role ?? '-'}</td>
                    <td>${claim}</td>
                    <td>${rule.isVisible ?? '-'}</td>
                </tr>
            `;
        }).join('');
    } catch {
        ruleTable.innerHTML = '<tr><td colspan="5"> ⁄–—  Õ„Ì· «·ﬁÊ«⁄œ</td></tr>';
    }
};

pageForm?.addEventListener('submit', (event) => {
    event.preventDefault();
    const payload = Object.fromEntries(new FormData(pageForm));
    worker.postMessage({ type: 'page', payload });
});

ruleForm?.addEventListener('submit', (event) => {
    event.preventDefault();
    const payload = Object.fromEntries(new FormData(ruleForm));
    worker.postMessage({ type: 'rule', payload });
});

worker.onmessage = async (event) => {
    const { type, request } = event.data;
    if (type === 'page') {
        const antiForgeryToken = getAntiForgeryToken();
        const response = await fetch('/api/builder/page', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...(antiForgeryToken ? { 'RequestVerificationToken': antiForgeryToken } : {})
            },
            body: JSON.stringify(request)
        });

        if (response.ok) {
            const result = await response.json();
            pageResult.textContent = result.created
                ? ` „ ≈‰‘«¡ ${result.controllerPath} + ${result.viewPath}`
                : `„ÊÃÊœ »«·›⁄·: ${result.controllerPath}`;

            const autoRefresh = pageForm?.querySelector('[name="autoRefresh"]')?.value === 'true';
            if (autoRefresh && result.created) {
                setTimeout(() => window.location.reload(), 600);
            }
        } else {
            pageResult.textContent = ' ⁄–— ≈‰‘«¡ «·’›Õ…';
        }
    }

    if (type === 'rule') {
        const antiForgeryToken = getAntiForgeryToken();
        const response = await fetch('/api/builder/rule', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...(antiForgeryToken ? { 'RequestVerificationToken': antiForgeryToken } : {})
            },
            body: JSON.stringify(request)
        });

        if (response.ok) {
            await fetchRules();
        }
    }
};

fetchRules();
