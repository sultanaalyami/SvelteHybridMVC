/**
 * SvelteHybrid Runtime v2.0
 * Zero-config reactive framework for ASP.NET Core
 * Works instantly - no build step required!
 */
(function() {
    'use strict';

    const SvelteHybrid = {
        version: '2.0.0',
        components: new Map(),
        state: new Map(),
        watchers: new Map(),
        
        // Global functions available in expressions
        globals: {
            $get: async (url) => {
                const res = await fetch(url);
                return res.json();
            },
            $post: async (url, data) => {
                const res = await fetch(url, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });
                return res.json();
            },
            $put: async (url, data) => {
                const res = await fetch(url, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });
                return res.json();
            },
            $delete: async (url) => {
                const res = await fetch(url, { method: 'DELETE' });
                return res.ok;
            },
            $toast: {
                success: (msg) => SvelteHybrid.toast(msg, 'success'),
                error: (msg) => SvelteHybrid.toast(msg, 'error'),
                info: (msg) => SvelteHybrid.toast(msg, 'info'),
                warning: (msg) => SvelteHybrid.toast(msg, 'warning')
            },
            $modal: {
                open: (id) => {
                    const el = document.querySelector(`s-modal#${id}, [s-modal="${id}"]`);
                    if (el) el.classList.add('s-modal-open');
                },
                close: (id) => {
                    const el = document.querySelector(`s-modal#${id}, [s-modal="${id}"]`);
                    if (el) el.classList.remove('s-modal-open');
                }
            }
        },

        /**
         * Initialize all reactive elements
         */
        init() {
            // Find all reactive containers
            document.querySelectorAll('[s-reactive]').forEach(el => {
                this.initReactive(el);
            });

            // Initialize built-in components
            this.initComponents();

            // Setup mutation observer for dynamic content
            this.observeDOM();

            console.log(`?? SvelteHybrid v${this.version} initialized`);
        },

        /**
         * Initialize a reactive container
         */
        initReactive(container) {
            const id = container.id || `s-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
            container.id = id;

            // Parse initial state
            const dataAttr = container.getAttribute('s-data');
            let state = {};
            if (dataAttr) {
                try {
                    state = new Function(`return (${dataAttr})`)();
                } catch (e) {
                    console.error('Invalid s-data:', e);
                }
            }

            // Create reactive proxy
            const proxy = this.createReactiveProxy(state, () => this.update(container));
            this.state.set(id, proxy);

            // Process all directives
            this.processDirectives(container, proxy);

            // Run init if present
            const initExpr = container.getAttribute('s-init');
            if (initExpr) {
                this.evalExpr(initExpr, proxy, container);
            }

            // Setup polling if present
            const pollUrl = container.getAttribute('s-poll');
            if (pollUrl) {
                const interval = parseInt(container.getAttribute('s-interval')) || 5000;
                setInterval(async () => {
                    const data = await this.globals.$get(pollUrl);
                    Object.assign(proxy, data);
                }, interval);
            }

            // Initial render
            this.update(container);
        },

        /**
         * Create a reactive proxy for state
         */
        createReactiveProxy(obj, onChange) {
            const handler = {
                get: (target, key) => {
                    const value = target[key];
                    if (typeof value === 'object' && value !== null) {
                        return new Proxy(value, handler);
                    }
                    return value;
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
         * Process all directives in a container
         */
        processDirectives(container, state) {
            // s-click
            container.querySelectorAll('[s-click]').forEach(el => {
                const expr = el.getAttribute('s-click');
                el.addEventListener('click', async (e) => {
                    e.preventDefault();
                    await this.evalExpr(expr, state, el);
                });
            });

            // s-submit
            container.querySelectorAll('[s-submit]').forEach(el => {
                const expr = el.getAttribute('s-submit');
                el.addEventListener('submit', async (e) => {
                    e.preventDefault();
                    const formData = Object.fromEntries(new FormData(el));
                    await this.evalExpr(expr, state, el, { $form: formData });
                });
            });

            // s-model (two-way binding)
            container.querySelectorAll('[s-model]').forEach(el => {
                const key = el.getAttribute('s-model');
                el.addEventListener('input', (e) => {
                    const value = el.type === 'checkbox' ? el.checked : el.value;
                    this.setNestedValue(state, key, value);
                });
                // Store reference for updates
                el._sModel = key;
            });

            // s-change
            container.querySelectorAll('[s-change]').forEach(el => {
                const expr = el.getAttribute('s-change');
                el.addEventListener('change', async () => {
                    await this.evalExpr(expr, state, el);
                });
            });

            // s-keyup
            container.querySelectorAll('[s-keyup]').forEach(el => {
                const expr = el.getAttribute('s-keyup');
                el.addEventListener('keyup', async (e) => {
                    await this.evalExpr(expr, state, el, { $event: e, $key: e.key });
                });
            });

            // s-focus
            container.querySelectorAll('[s-focus]').forEach(el => {
                const expr = el.getAttribute('s-focus');
                el.addEventListener('focus', async () => {
                    await this.evalExpr(expr, state, el);
                });
            });

            // s-blur
            container.querySelectorAll('[s-blur]').forEach(el => {
                const expr = el.getAttribute('s-blur');
                el.addEventListener('blur', async () => {
                    await this.evalExpr(expr, state, el);
                });
            });
        },

        /**
         * Update DOM based on state
         */
        update(container) {
            const state = this.state.get(container.id);
            if (!state) return;

            // Update text interpolation {expression}
            this.updateInterpolation(container, state);

            // Update s-model values
            container.querySelectorAll('[s-model]').forEach(el => {
                const key = el._sModel || el.getAttribute('s-model');
                const value = this.getNestedValue(state, key);
                if (el.type === 'checkbox') {
                    el.checked = !!value;
                } else if (el.value !== value) {
                    el.value = value ?? '';
                }
            });

            // Update s-if
            container.querySelectorAll('[s-if]').forEach(el => {
                const expr = el.getAttribute('s-if');
                const show = this.evalExprSync(expr, state);
                el.style.display = show ? '' : 'none';
                
                // Handle s-else sibling
                const next = el.nextElementSibling;
                if (next && next.hasAttribute('s-else')) {
                    next.style.display = show ? 'none' : '';
                }
            });

            // Update s-show
            container.querySelectorAll('[s-show]').forEach(el => {
                const expr = el.getAttribute('s-show');
                const show = this.evalExprSync(expr, state);
                el.style.display = show ? '' : 'none';
            });

            // Update s-class
            container.querySelectorAll('[s-class]').forEach(el => {
                const expr = el.getAttribute('s-class');
                const classes = this.evalExprSync(expr, state);
                if (typeof classes === 'object') {
                    Object.entries(classes).forEach(([cls, active]) => {
                        el.classList.toggle(cls, !!active);
                    });
                }
            });

            // Update s-style
            container.querySelectorAll('[s-style]').forEach(el => {
                const expr = el.getAttribute('s-style');
                const styles = this.evalExprSync(expr, state);
                if (typeof styles === 'object') {
                    Object.assign(el.style, styles);
                }
            });

            // Update s-disabled
            container.querySelectorAll('[s-disabled]').forEach(el => {
                const expr = el.getAttribute('s-disabled');
                el.disabled = !!this.evalExprSync(expr, state);
            });

            // Update s-for
            container.querySelectorAll('template[s-for]').forEach(template => {
                const expr = template.getAttribute('s-for');
                const match = expr.match(/(\w+)\s+in\s+(\w+)/);
                if (!match) return;

                const [, itemName, arrayName] = match;
                const array = state[arrayName] || [];
                
                // Remove old items
                let sibling = template.nextElementSibling;
                while (sibling && sibling.hasAttribute('s-for-item')) {
                    const next = sibling.nextElementSibling;
                    sibling.remove();
                    sibling = next;
                }

                // Add new items
                array.forEach((item, index) => {
                    const clone = template.content.cloneNode(true);
                    const wrapper = document.createElement('div');
                    wrapper.setAttribute('s-for-item', '');
                    wrapper.innerHTML = clone.firstElementChild?.outerHTML || '';
                    
                    // Replace item references
                    wrapper.innerHTML = wrapper.innerHTML
                        .replace(new RegExp(`{${itemName}\\.([^}]+)}`, 'g'), (_, key) => item[key] ?? '')
                        .replace(new RegExp(`{${itemName}}`, 'g'), item)
                        .replace(/{index}/g, index);
                    
                    template.parentNode.insertBefore(wrapper.firstElementChild || wrapper, template.nextSibling);
                });
            });

            // Update s-text
            container.querySelectorAll('[s-text]').forEach(el => {
                const expr = el.getAttribute('s-text');
                el.textContent = this.evalExprSync(expr, state) ?? '';
            });

            // Update s-html
            container.querySelectorAll('[s-html]').forEach(el => {
                const expr = el.getAttribute('s-html');
                el.innerHTML = this.evalExprSync(expr, state) ?? '';
            });

            // Update s-attr
            container.querySelectorAll('[s-attr]').forEach(el => {
                const expr = el.getAttribute('s-attr');
                const attrs = this.evalExprSync(expr, state);
                if (typeof attrs === 'object') {
                    Object.entries(attrs).forEach(([name, value]) => {
                        if (value === false || value === null || value === undefined) {
                            el.removeAttribute(name);
                        } else {
                            el.setAttribute(name, value === true ? '' : value);
                        }
                    });
                }
            });
        },

        /**
         * Update text interpolation
         */
        updateInterpolation(container, state) {
            const walker = document.createTreeWalker(container, NodeFilter.SHOW_TEXT);
            const nodes = [];
            while (walker.nextNode()) nodes.push(walker.currentNode);

            nodes.forEach(node => {
                const original = node._sOriginal || node.textContent;
                if (!node._sOriginal && original.includes('{')) {
                    node._sOriginal = original;
                }
                if (node._sOriginal) {
                    node.textContent = node._sOriginal.replace(/{([^}]+)}/g, (_, expr) => {
                        return this.evalExprSync(expr, state) ?? '';
                    });
                }
            });
        },

        /**
         * Evaluate expression synchronously
         */
        evalExprSync(expr, state) {
            try {
                const fn = new Function(...Object.keys(state), ...Object.keys(this.globals), 
                    `return (${expr})`);
                return fn(...Object.values(state), ...Object.values(this.globals));
            } catch (e) {
                return undefined;
            }
        },

        /**
         * Evaluate expression asynchronously
         */
        async evalExpr(expr, state, el, extra = {}) {
            try {
                const allVars = { ...state, ...this.globals, ...extra, $el: el };
                const fn = new Function(...Object.keys(allVars), 
                    `return (async () => { ${expr} })()`);
                const result = await fn(...Object.values(allVars));
                return result;
            } catch (e) {
                console.error('Expression error:', e);
                return undefined;
            }
        },

        /**
         * Get nested object value
         */
        getNestedValue(obj, path) {
            return path.split('.').reduce((o, k) => o?.[k], obj);
        },

        /**
         * Set nested object value
         */
        setNestedValue(obj, path, value) {
            const keys = path.split('.');
            const last = keys.pop();
            const target = keys.reduce((o, k) => o[k] = o[k] || {}, obj);
            target[last] = value;
        },

        /**
         * Initialize built-in components
         */
        initComponents() {
            // Toast container
            if (!document.querySelector('.s-toast-container')) {
                const container = document.createElement('div');
                container.className = 's-toast-container';
                document.body.appendChild(container);
            }

            // Modal backdrop
            document.querySelectorAll('s-modal, [s-modal]').forEach(modal => {
                modal.addEventListener('click', (e) => {
                    if (e.target === modal) {
                        modal.classList.remove('s-modal-open');
                    }
                });
            });

            // Tabs
            document.querySelectorAll('s-tabs').forEach(tabs => {
                const tabButtons = tabs.querySelectorAll('s-tab');
                tabButtons.forEach((tab, i) => {
                    const btn = document.createElement('button');
                    btn.textContent = tab.getAttribute('title') || `Tab ${i + 1}`;
                    btn.className = 's-tab-btn' + (i === 0 ? ' active' : '');
                    btn.onclick = () => {
                        tabButtons.forEach((t, j) => {
                            t.style.display = j === i ? '' : 'none';
                            tabs.querySelectorAll('.s-tab-btn')[j]?.classList.toggle('active', j === i);
                        });
                    };
                    tabs.insertBefore(btn, tabs.firstChild);
                    tab.style.display = i === 0 ? '' : 'none';
                });
            });
        },

        /**
         * Show toast notification
         */
        toast(message, type = 'info') {
            const container = document.querySelector('.s-toast-container');
            if (!container) return;

            const toast = document.createElement('div');
            toast.className = `s-toast s-toast-${type}`;
            toast.textContent = message;
            container.appendChild(toast);

            setTimeout(() => toast.classList.add('s-toast-show'), 10);
            setTimeout(() => {
                toast.classList.remove('s-toast-show');
                setTimeout(() => toast.remove(), 300);
            }, 3000);
        },

        /**
         * Observe DOM for dynamic content
         */
        observeDOM() {
            const observer = new MutationObserver(mutations => {
                mutations.forEach(mutation => {
                    mutation.addedNodes.forEach(node => {
                        if (node.nodeType === 1) {
                            if (node.hasAttribute?.('s-reactive')) {
                                this.initReactive(node);
                            }
                            node.querySelectorAll?.('[s-reactive]').forEach(el => {
                                if (!this.state.has(el.id)) {
                                    this.initReactive(el);
                                }
                            });
                        }
                    });
                });
            });

            observer.observe(document.body, { childList: true, subtree: true });
        },

        /**
         * Refresh a specific reactive container
         */
        refresh(idOrElement) {
            const el = typeof idOrElement === 'string' 
                ? document.getElementById(idOrElement)
                : idOrElement;
            if (el) this.update(el);
        },

        /**
         * Get state of a reactive container
         */
        getState(id) {
            return this.state.get(id);
        },

        /**
         * Set state of a reactive container
         */
        setState(id, newState) {
            const state = this.state.get(id);
            if (state) {
                Object.assign(state, newState);
            }
        }
    };

    // Auto-initialize
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => SvelteHybrid.init());
    } else {
        SvelteHybrid.init();
    }

    // Export
    window.SvelteHybrid = SvelteHybrid;
    window.$s = SvelteHybrid; // Short alias

})();
