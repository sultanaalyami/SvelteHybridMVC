/**
 * SvelteHybrid Runtime v3.0
 * Auto-generated reactive framework for ASP.NET Core
 * Zero-config - Works instantly!
 */
(function() {
    'use strict';

    const VERSION = '3.0.0';
    
    const SvelteHybrid = {
        version: VERSION,
        state: new Map(),
        components: new Map(),
        
        // Global helpers
        $: {
            get: async (url) => (await fetch(url)).json(),
            post: async (url, data) => (await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]')?.value || '' },
                body: JSON.stringify(data)
            })).json(),
            put: async (url, data) => (await fetch(url, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]')?.value || '' },
                body: JSON.stringify(data)
            })).json(),
            delete: async (url) => (await fetch(url, { method: 'DELETE', headers: { 'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]')?.value || '' } })).ok,
            toast: {
                show: (msg, type = 'info') => SvelteHybrid.toast(msg, type),
                success: (msg) => SvelteHybrid.toast(msg, 'success'),
                error: (msg) => SvelteHybrid.toast(msg, 'error'),
                warning: (msg) => SvelteHybrid.toast(msg, 'warning'),
                info: (msg) => SvelteHybrid.toast(msg, 'info')
            },
            modal: {
                open: (id) => document.querySelector(`[s-modal="${id}"]`)?.classList.add('s-modal-open'),
                close: (id) => document.querySelector(`[s-modal="${id}"]`)?.classList.remove('s-modal-open')
            },
            navigate: (url) => { window.location.href = url; },
            reload: () => { window.location.reload(); },
            copy: async (text) => { await navigator.clipboard.writeText(text); SvelteHybrid.toast('Copied!', 'success'); }
        },

        /**
         * Initialize all reactive elements
         */
        init() {
            document.querySelectorAll('[s-reactive]').forEach(el => this.initReactive(el));
            this.initToastContainer();
            this.initModals();
            this.initTabs();
            this.observeDOM();
            console.log(`?? SvelteHybrid v${VERSION} ready!`);
        },

        /**
         * Initialize reactive container
         */
        initReactive(container) {
            const id = container.id || `s${Date.now()}${Math.random().toString(36).substr(2, 5)}`;
            container.id = id;

            // Parse state
            let state = {};
            const dataAttr = container.getAttribute('s-data');
            if (dataAttr) {
                try { state = new Function(`return (${dataAttr})`)(); } catch(e) { console.error('s-data error:', e); }
            }

            // Create reactive proxy
            const proxy = this.reactive(state, () => this.render(container));
            this.state.set(id, proxy);

            // Setup event handlers
            this.setupEvents(container, proxy);

            // Run init expression
            const initExpr = container.getAttribute('s-init');
            if (initExpr) {
                this.evalAsync(initExpr, proxy, container);
            }

            // Setup polling
            const pollUrl = container.getAttribute('s-poll');
            if (pollUrl) {
                const interval = parseInt(container.getAttribute('s-interval')) || 5000;
                setInterval(async () => {
                    try {
                        const data = await this.$.get(pollUrl);
                        Object.assign(proxy, data);
                    } catch(e) { console.error('Poll error:', e); }
                }, interval);
            }

            // Initial render
            this.render(container);
        },

        /**
         * Create reactive proxy
         */
        reactive(obj, onChange) {
            const handler = {
                get: (target, key) => {
                    const val = target[key];
                    return (typeof val === 'object' && val !== null) ? new Proxy(val, handler) : val;
                },
                set: (target, key, value) => {
                    target[key] = value;
                    onChange();
                    return true;
                }
            };
            return new Proxy(obj, handler);
        },

        /**
         * Setup event handlers
         */
        setupEvents(container, state) {
            // Click
            container.querySelectorAll('[s-click]').forEach(el => {
                el.addEventListener('click', async (e) => {
                    e.preventDefault();
                    await this.evalAsync(el.getAttribute('s-click'), state, el);
                });
            });

            // Submit
            container.querySelectorAll('[s-submit]').forEach(el => {
                el.addEventListener('submit', async (e) => {
                    e.preventDefault();
                    const formData = Object.fromEntries(new FormData(el));
                    await this.evalAsync(el.getAttribute('s-submit'), state, el, { $form: formData, $el: el });
                });
            });

            // Model (two-way binding)
            container.querySelectorAll('[s-model]').forEach(el => {
                const key = el.getAttribute('s-model');
                el._sModel = key;
                el.addEventListener('input', () => {
                    const val = el.type === 'checkbox' ? el.checked : el.value;
                    this.setPath(state, key, val);
                });
            });

            // Change
            container.querySelectorAll('[s-change]').forEach(el => {
                el.addEventListener('change', async () => {
                    await this.evalAsync(el.getAttribute('s-change'), state, el);
                });
            });

            // Keyup / Enter
            container.querySelectorAll('[s-keyup], [s-enter]').forEach(el => {
                el.addEventListener('keyup', async (e) => {
                    if (el.hasAttribute('s-enter') && e.key === 'Enter') {
                        await this.evalAsync(el.getAttribute('s-enter'), state, el, { $event: e });
                    } else if (el.hasAttribute('s-keyup')) {
                        await this.evalAsync(el.getAttribute('s-keyup'), state, el, { $event: e, $key: e.key });
                    }
                });
            });

            // Focus/Blur
            container.querySelectorAll('[s-focus]').forEach(el => {
                el.addEventListener('focus', async () => await this.evalAsync(el.getAttribute('s-focus'), state, el));
            });
            container.querySelectorAll('[s-blur]').forEach(el => {
                el.addEventListener('blur', async () => await this.evalAsync(el.getAttribute('s-blur'), state, el));
            });
        },

        /**
         * Render container
         */
        render(container) {
            const state = this.state.get(container.id);
            if (!state) return;

            // Update interpolation {expr}
            const walker = document.createTreeWalker(container, NodeFilter.SHOW_TEXT);
            const textNodes = [];
            while (walker.nextNode()) textNodes.push(walker.currentNode);
            
            textNodes.forEach(node => {
                const original = node._sOriginal || node.textContent;
                if (!node._sOriginal && original.includes('{')) node._sOriginal = original;
                if (node._sOriginal) {
                    node.textContent = node._sOriginal.replace(/{([^}]+)}/g, (_, expr) => 
                        this.evalSync(expr, state) ?? '');
                }
            });

            // s-model
            container.querySelectorAll('[s-model]').forEach(el => {
                const key = el._sModel || el.getAttribute('s-model');
                const val = this.getPath(state, key);
                if (el.type === 'checkbox') el.checked = !!val;
                else if (el.value !== (val ?? '')) el.value = val ?? '';
            });

            // s-text
            container.querySelectorAll('[s-text]').forEach(el => {
                el.textContent = this.evalSync(el.getAttribute('s-text'), state) ?? '';
            });

            // s-html
            container.querySelectorAll('[s-html]').forEach(el => {
                el.innerHTML = this.evalSync(el.getAttribute('s-html'), state) ?? '';
            });

            // s-if / s-else
            container.querySelectorAll('[s-if]').forEach(el => {
                const show = !!this.evalSync(el.getAttribute('s-if'), state);
                el.style.display = show ? '' : 'none';
                const next = el.nextElementSibling;
                if (next?.hasAttribute('s-else')) next.style.display = show ? 'none' : '';
            });

            // s-show
            container.querySelectorAll('[s-show]').forEach(el => {
                el.style.display = this.evalSync(el.getAttribute('s-show'), state) ? '' : 'none';
            });

            // s-class
            container.querySelectorAll('[s-class]').forEach(el => {
                const classes = this.evalSync(el.getAttribute('s-class'), state);
                if (typeof classes === 'object') {
                    Object.entries(classes).forEach(([cls, active]) => el.classList.toggle(cls, !!active));
                }
            });

            // s-style
            container.querySelectorAll('[s-style]').forEach(el => {
                const styles = this.evalSync(el.getAttribute('s-style'), state);
                if (typeof styles === 'object') Object.assign(el.style, styles);
            });

            // s-disabled
            container.querySelectorAll('[s-disabled]').forEach(el => {
                el.disabled = !!this.evalSync(el.getAttribute('s-disabled'), state);
            });

            // s-attr
            container.querySelectorAll('[s-attr]').forEach(el => {
                const attrs = this.evalSync(el.getAttribute('s-attr'), state);
                if (typeof attrs === 'object') {
                    Object.entries(attrs).forEach(([name, val]) => {
                        if (val === false || val == null) el.removeAttribute(name);
                        else el.setAttribute(name, val === true ? '' : val);
                    });
                }
            });

            // s-for (template loops)
            container.querySelectorAll('template[s-for]').forEach(tpl => {
                const expr = tpl.getAttribute('s-for');
                const match = expr.match(/(\w+)\s+in\s+(\w+)/);
                if (!match) return;
                
                const [, itemName, arrayName] = match;
                const array = state[arrayName] || [];
                
                // Remove old items
                let sib = tpl.nextElementSibling;
                while (sib?.hasAttribute('s-for-item')) {
                    const next = sib.nextElementSibling;
                    sib.remove();
                    sib = next;
                }
                
                // Create new items
                array.forEach((item, idx) => {
                    const clone = tpl.content.cloneNode(true);
                    const wrapper = document.createElement('div');
                    wrapper.innerHTML = clone.firstElementChild?.outerHTML || '';
                    wrapper.innerHTML = wrapper.innerHTML
                        .replace(new RegExp(`{${itemName}\\.([^}]+)}`, 'g'), (_, k) => item[k] ?? '')
                        .replace(new RegExp(`{${itemName}}`, 'g'), typeof item === 'object' ? JSON.stringify(item) : item)
                        .replace(/{index}/g, idx);
                    
                    const newEl = wrapper.firstElementChild;
                    if (newEl) {
                        newEl.setAttribute('s-for-item', '');
                        tpl.parentNode.insertBefore(newEl, tpl.nextSibling);
                    }
                });
            });
        },

        /**
         * Evaluate expression synchronously
         */
        evalSync(expr, state) {
            try {
                const fn = new Function(...Object.keys(state), ...Object.keys(this.$).map(k => '$' + k),
                    `try { return (${expr}); } catch(e) { return undefined; }`);
                return fn(...Object.values(state), ...Object.values(this.$));
            } catch(e) { return undefined; }
        },

        /**
         * Evaluate expression asynchronously
         */
        async evalAsync(expr, state, el, extra = {}) {
            try {
                const allVars = { ...state, ...Object.fromEntries(Object.entries(this.$).map(([k,v]) => ['$'+k, v])), ...extra, $el: el };
                const fn = new Function(...Object.keys(allVars), `return (async () => { ${expr} })()`);
                return await fn(...Object.values(allVars));
            } catch(e) { console.error('Eval error:', e); }
        },

        /**
         * Get nested path value
         */
        getPath(obj, path) {
            return path.split('.').reduce((o, k) => o?.[k], obj);
        },

        /**
         * Set nested path value
         */
        setPath(obj, path, value) {
            const keys = path.split('.');
            const last = keys.pop();
            const target = keys.reduce((o, k) => o[k] = o[k] || {}, obj);
            target[last] = value;
        },

        /**
         * Toast notification
         */
        toast(message, type = 'info') {
            const container = document.querySelector('.s-toast-container');
            if (!container) return;
            
            const toast = document.createElement('div');
            toast.className = `s-toast s-toast-${type}`;
            toast.innerHTML = `<span>${message}</span><button onclick="this.parentElement.remove()">×</button>`;
            container.appendChild(toast);
            
            requestAnimationFrame(() => toast.classList.add('s-show'));
            setTimeout(() => {
                toast.classList.remove('s-show');
                setTimeout(() => toast.remove(), 300);
            }, 4000);
        },

        /**
         * Initialize toast container
         */
        initToastContainer() {
            if (!document.querySelector('.s-toast-container')) {
                const container = document.createElement('div');
                container.className = 's-toast-container';
                document.body.appendChild(container);
            }
        },

        /**
         * Initialize modals
         */
        initModals() {
            document.querySelectorAll('[s-modal]').forEach(modal => {
                modal.addEventListener('click', (e) => {
                    if (e.target === modal) modal.classList.remove('s-modal-open');
                });
            });
        },

        /**
         * Initialize tabs
         */
        initTabs() {
            document.querySelectorAll('.s-tabs').forEach(tabs => {
                const btns = tabs.querySelectorAll('.s-tab-btn');
                const panels = tabs.querySelectorAll('.s-tab-panel');
                
                btns.forEach((btn, i) => {
                    btn.addEventListener('click', () => {
                        btns.forEach((b, j) => b.classList.toggle('active', i === j));
                        panels.forEach((p, j) => p.style.display = i === j ? '' : 'none');
                    });
                });
            });
        },

        /**
         * Observe DOM for dynamic content
         */
        observeDOM() {
            const observer = new MutationObserver(mutations => {
                mutations.forEach(m => {
                    m.addedNodes.forEach(node => {
                        if (node.nodeType === 1) {
                            if (node.hasAttribute?.('s-reactive') && !this.state.has(node.id)) {
                                this.initReactive(node);
                            }
                            node.querySelectorAll?.('[s-reactive]').forEach(el => {
                                if (!this.state.has(el.id)) this.initReactive(el);
                            });
                        }
                    });
                });
            });
            observer.observe(document.body, { childList: true, subtree: true });
        },

        /**
         * Public API
         */
        getState: (id) => SvelteHybrid.state.get(id),
        setState: (id, data) => { const s = SvelteHybrid.state.get(id); if(s) Object.assign(s, data); },
        refresh: (id) => { const el = document.getElementById(id); if(el) SvelteHybrid.render(el); }
    };

    // Auto-init
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => SvelteHybrid.init());
    } else {
        SvelteHybrid.init();
    }

    // Export
    window.SvelteHybrid = SvelteHybrid;
    window.$s = SvelteHybrid;
})();
