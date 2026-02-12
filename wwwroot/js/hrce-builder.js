const pageForm = document.getElementById('page-form');
const ruleForm = document.getElementById('rule-form');
const pageResult = document.getElementById('page-result');
const ruleTable = document.getElementById('rule-table');
const ruleTableMeta = ruleTable?.closest('table');

const layoutCanvas = document.getElementById('layout-canvas');
const layoutPalette = document.getElementById('layout-palette');
const layoutLinks = document.getElementById('layout-links');
const layoutSaveButton = document.getElementById('layout-save');
const layoutSaveQuickButton = document.getElementById('layout-save-quick');
const layoutVersions = document.getElementById('layout-versions');
const layoutRestoreButton = document.getElementById('layout-restore');
const layoutVersionName = document.getElementById('layout-version-name');
const layoutStatus = document.getElementById('layout-status');

const worker = new Worker('/js/hrce-builder.worker.js');
const getAntiForgeryToken = () =>
    document.querySelector('input[name="__RequestVerificationToken"]')?.value;

const fetchRules = async () => {
    try {
        const response = await fetch('/api/builder/rules');
        const showIdentityColumns = ruleTableMeta?.dataset.identityColumns === 'true';
        const colSpan = showIdentityColumns ? 5 : 3;
        if (!response.ok) {
            ruleTable.innerHTML = `<tr><td colspan="${colSpan}">€Ì— „ «Õ</td></tr>`;
            return;
        }
        const data = await response.json();
        const identityCells = (rule) => {
            if (!showIdentityColumns) {
                return '';
            }
            const claim = rule.claimType ? `${rule.claimType}:${rule.claimValue ?? ''}` : '-';
            return `<td>${rule.role ?? '-'}</td><td>${claim}</td>`;
        };
        ruleTable.innerHTML = data.map(rule => {
            const target = rule.controller ?? rule.page ?? rule.area ?? '-';
            return `
                <tr>
                    <td>${rule.scope}</td>
                    <td>${target}</td>
                    ${identityCells(rule)}
                    <td>${rule.isVisible ?? '-'}</td>
                </tr>
            `;
        }).join('');

        renderCapturedLinks(data);
    } catch {
        const showIdentityColumns = ruleTableMeta?.dataset.identityColumns === 'true';
        const colSpan = showIdentityColumns ? 5 : 3;
        ruleTable.innerHTML = `<tr><td colspan="${colSpan}"> ⁄–—  Õ„Ì· «·ﬁÊ«⁄œ</td></tr>`;
    }
};

const renderCapturedLinks = (links) => {
    if (!layoutLinks) {
        return;
    }

    layoutLinks.innerHTML = links.length
        ? links.map(link => `
            <button class="btn btn-light" type="button" draggable="true" data-builder-block data-type="link" data-label="${link.label}" data-url="${link.url}">
                ${link.label}
            </button>
        `).join('')
        : '<div class="hrce-muted">·«  ÊÃœ —Ê«»ÿ »⁄œ.</div>';
};

const renderPaletteBlocks = (blocks) => {
    if (!layoutPalette) {
        return;
    }

    layoutPalette.innerHTML = blocks.map(block => `
        <button class="btn btn-light" type="button" draggable="true" data-builder-block data-type="${block.type}" data-label="${block.label}">
            ${block.label}
        </button>
    `).join('');
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
loadPalette();

const getLayoutBlocks = () => {
    if (!layoutCanvas) {
        return [];
    }

    return Array.from(layoutCanvas.querySelectorAll('[data-layout-block]')).map(block => ({
        id: block.dataset.blockId,
        type: block.dataset.type,
        label: block.dataset.label,
        url: block.dataset.url || null,
        cssClass: block.dataset.cssClass || null
    }));
};

const renderLayout = (layout) => {
    if (!layoutCanvas) {
        return;
    }

    layoutCanvas.innerHTML = '';
    layout.blocks.forEach(block => {
        layoutCanvas.appendChild(createLayoutBlock(block));
    });
};

const createLayoutBlock = (block) => {
    const item = document.createElement('div');
    item.className = 'hrce-card';
    item.draggable = true;
    item.dataset.layoutBlock = 'true';
    item.dataset.blockId = block.id;
    item.dataset.type = block.type;
    item.dataset.label = block.label;
    item.dataset.url = block.url ?? '';
    item.dataset.cssClass = block.cssClass ?? '';
    item.innerHTML = `
        <div class="hrce-card__label">${block.type}</div>
        <div class="hrce-card__value">${block.label}</div>
        ${block.url ? `<div class="hrce-muted">${block.url}</div>` : ''}
    `;

    item.addEventListener('dragstart', (event) => {
        event.dataTransfer.setData('text/plain', JSON.stringify({
            source: 'canvas',
            blockId: block.id
        }));
    });

    item.addEventListener('click', () => {
        layoutCanvas.querySelectorAll('[data-layout-block]').forEach(el => el.classList.remove('is-selected'));
        item.classList.add('is-selected');
    });

    return item;
};

const handleDrop = (event) => {
    event.preventDefault();
    if (!layoutCanvas) {
        return;
    }

    const payload = JSON.parse(event.dataTransfer.getData('text/plain'));
    if (payload.source === 'palette') {
        const block = {
            id: crypto.randomUUID(),
            type: payload.type,
            label: payload.label,
            url: payload.url || null,
            cssClass: null
        };
        layoutCanvas.appendChild(createLayoutBlock(block));
        return;
    }

    if (payload.source === 'canvas') {
        const dragged = layoutCanvas.querySelector(`[data-block-id="${payload.blockId}"]`);
        if (dragged && event.target.closest('[data-layout-block]')) {
            layoutCanvas.insertBefore(dragged, event.target.closest('[data-layout-block]'));
        }
    }
};

const initLayoutDragDrop = () => {
    if (!layoutCanvas || !layoutPalette) {
        return;
    }

    layoutPalette.addEventListener('dragstart', (event) => {
        const target = event.target.closest('[data-builder-block]');
        if (!target) {
            return;
        }

        event.dataTransfer.setData('text/plain', JSON.stringify({
            source: 'palette',
            type: target.dataset.type,
            label: target.dataset.label,
            url: target.dataset.url
        }));
    });

    layoutLinks?.addEventListener('dragstart', (event) => {
        const target = event.target.closest('[data-builder-block]');
        if (!target) {
            return;
        }

        event.dataTransfer.setData('text/plain', JSON.stringify({
            source: 'palette',
            type: target.dataset.type,
            label: target.dataset.label,
            url: target.dataset.url
        }));
    });

    layoutCanvas.addEventListener('dragover', (event) => {
        event.preventDefault();
        layoutCanvas.classList.add('drag-over');
    });
    layoutCanvas.addEventListener('dragleave', () => {
        layoutCanvas.classList.remove('drag-over');
    });
    layoutCanvas.addEventListener('drop', (event) => {
        layoutCanvas.classList.remove('drag-over');
        handleDrop(event);
    });
};

const loadLayout = async () => {
    if (!layoutCanvas) {
        return;
    }

    const response = await fetch('/api/builder/layout');
    if (!response.ok) {
        layoutCanvas.innerHTML = '<div class="hrce-muted"> ⁄–—  Õ„Ì· «· ŒÿÌÿ.</div>';
        return;
    }

    const data = await response.json();
    renderLayout(data);
};

const loadLayoutVersions = async () => {
    if (!layoutVersions) {
        return;
    }

    const response = await fetch('/api/builder/layout/versions');
    if (!response.ok) {
        layoutVersions.innerHTML = '';
        return;
    }

    const versions = await response.json();
    layoutVersions.innerHTML = versions.length
        ? versions.map(version => `
            <option value="${version.id}" ${version.isCurrent ? 'selected' : ''}>
                ${new Date(version.createdAt).toLocaleString()} - ${version.name}
            </option>
        `).join('')
        : '<option value="">·«  ÊÃœ ‰”Œ „Õ›ÊŸ…</option>';
};

const saveLayout = async (nameOverride) => {
    if (!layoutCanvas) {
        return;
    }

    const layout = { blocks: getLayoutBlocks() };
    const payload = {
        name: nameOverride || layoutVersionName?.value || 'Auto',
        layout
    };
    const antiForgeryToken = getAntiForgeryToken();
    const response = await fetch('/api/builder/layout', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            ...(antiForgeryToken ? { 'RequestVerificationToken': antiForgeryToken } : {})
        },
        body: JSON.stringify(payload)
    });

    if (response.ok) {
        layoutStatus.textContent = ' „ Õ›Ÿ «·‰”Œ… »‰Ã«Õ.';
        await loadLayoutVersions();
    } else {
        layoutStatus.textContent = ' ⁄–— Õ›Ÿ «·‰”Œ….';
    }
};

const restoreLayout = async () => {
    const versionId = layoutVersions?.value;
    if (!versionId) {
        return;
    }

    const antiForgeryToken = getAntiForgeryToken();
    const response = await fetch('/api/builder/layout/restore', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            ...(antiForgeryToken ? { 'RequestVerificationToken': antiForgeryToken } : {})
        },
        body: JSON.stringify({ versionId })
    });

    if (response.ok) {
        layoutStatus.textContent = ' „  «” ⁄«œ… «·‰”Œ… »‰Ã«Õ.';
        await loadLayout();
        await loadLayoutVersions();
    } else {
        layoutStatus.textContent = ' ⁄–— «” ⁄«œ… «·‰”Œ….';
    }
};

layoutSaveButton?.addEventListener('click', () => saveLayout());
layoutSaveQuickButton?.addEventListener('click', () => saveLayout('Quick Save'));
layoutRestoreButton?.addEventListener('click', restoreLayout);

initLayoutDragDrop();
loadLayout();
loadLayoutVersions();
