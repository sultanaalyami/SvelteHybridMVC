/**
 * SvelteHybrid Runtime v3.0
 * Reactive framework for ASP.NET Core
 */
(function() {
    'use strict';

    const VERSION = '3.0.0';
    
    window.SvelteHybrid = {
        version: VERSION,
        state: new Map(),
        
        /**
         * Initialize all reactive elements
         */
        init: function() {
            var self = this;
            document.querySelectorAll('[s-reactive]').forEach(function(el) {
                self.initReactive(el);
            });
            this.initToastContainer();
            console.log('SvelteHybrid v' + VERSION + ' ready!');
        },

        /**
         * Initialize reactive container
         */
        initReactive: function(container) {
            var self = this;
            var id = container.id || 's' + Date.now() + Math.random().toString(36).substr(2, 5);
            container.id = id;

            // Parse initial state
            var state = {};
            var dataAttr = container.getAttribute('s-data');
            if (dataAttr) {
                try { 
                    state = (new Function('return (' + dataAttr + ')'))(); 
                } catch(e) { 
                    console.error('s-data error:', e); 
                }
            }

            // Create reactive proxy
            var proxy = this.reactive(state, function() { 
                self.render(container); 
            });
            this.state.set(id, proxy);
            container._state = proxy;

            // Setup events
            this.setupEvents(container, proxy);

            // Initial render
            this.render(container);
        },

        /**
         * Create reactive proxy
         */
        reactive: function(obj, onChange) {
            var handler = {
                get: function(target, key) {
                    var val = target[key];
                    if (typeof val === 'object' && val !== null && !Array.isArray(val)) {
                        return new Proxy(val, handler);
                    }
                    return val;
                },
                set: function(target, key, value) {
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
        setupEvents: function(container, state) {
            var self = this;

            // s-click
            container.querySelectorAll('[s-click]').forEach(function(el) {
                var expr = el.getAttribute('s-click');
                el.addEventListener('click', function(e) {
                    e.preventDefault();
                    self.evalExpr(expr, state, container);
                });
            });

            // s-model
            container.querySelectorAll('[s-model]').forEach(function(el) {
                var key = el.getAttribute('s-model');
                el._sModelKey = key;
                
                el.addEventListener('input', function() {
                    var val = el.type === 'checkbox' ? el.checked : el.value;
                    self.setPath(state, key, val);
                });
            });

            // s-submit
            container.querySelectorAll('form[s-submit]').forEach(function(el) {
                var expr = el.getAttribute('s-submit');
                el.addEventListener('submit', function(e) {
                    e.preventDefault();
                    self.evalExpr(expr, state, container);
                });
            });
        },

        /**
         * Render container
         */
        render: function(container) {
            var state = this.state.get(container.id);
            if (!state) return;

            var self = this;

            // s-text
            container.querySelectorAll('[s-text]').forEach(function(el) {
                var expr = el.getAttribute('s-text');
                var val = self.evalExprSync(expr, state);
                el.textContent = val !== undefined && val !== null ? val : '';
            });

            // s-model values
            container.querySelectorAll('[s-model]').forEach(function(el) {
                var key = el._sModelKey || el.getAttribute('s-model');
                var val = self.getPath(state, key);
                if (el.type === 'checkbox') {
                    el.checked = !!val;
                } else if (el.value !== (val || '')) {
                    el.value = val || '';
                }
            });

            // s-show
            container.querySelectorAll('[s-show]').forEach(function(el) {
                var expr = el.getAttribute('s-show');
                var show = self.evalExprSync(expr, state);
                el.style.display = show ? '' : 'none';
            });

            // s-if
            container.querySelectorAll('[s-if]').forEach(function(el) {
                var expr = el.getAttribute('s-if');
                var show = self.evalExprSync(expr, state);
                el.style.display = show ? '' : 'none';
            });

            // s-class
            container.querySelectorAll('[s-class]').forEach(function(el) {
                var expr = el.getAttribute('s-class');
                var classes = self.evalExprSync(expr, state);
                if (typeof classes === 'object') {
                    Object.keys(classes).forEach(function(cls) {
                        el.classList.toggle(cls, !!classes[cls]);
                    });
                }
            });

            // s-disabled
            container.querySelectorAll('[s-disabled]').forEach(function(el) {
                var expr = el.getAttribute('s-disabled');
                el.disabled = !!self.evalExprSync(expr, state);
            });
        },

        /**
         * Evaluate expression synchronously
         */
        evalExprSync: function(expr, state) {
            try {
                var keys = Object.keys(state);
                var values = Object.values(state);
                var fn = new Function(keys.join(','), 'try { return (' + expr + '); } catch(e) { return undefined; }');
                return fn.apply(null, values);
            } catch(e) { 
                return undefined; 
            }
        },

        /**
         * Evaluate expression (for actions)
         */
        evalExpr: function(expr, state, container) {
            try {
                var keys = Object.keys(state);
                var values = keys.map(function(k) { return state[k]; });
                var fn = new Function(keys.join(','), expr + '; return {' + keys.map(function(k) { return k + ':' + k; }).join(',') + '};');
                var result = fn.apply(null, values);
                
                // Update state
                keys.forEach(function(k) {
                    if (result[k] !== state[k]) {
                        state[k] = result[k];
                    }
                });
            } catch(e) { 
                console.error('Expression error:', e); 
            }
        },

        /**
         * Get nested path value
         */
        getPath: function(obj, path) {
            return path.split('.').reduce(function(o, k) { return o ? o[k] : undefined; }, obj);
        },

        /**
         * Set nested path value
         */
        setPath: function(obj, path, value) {
            var keys = path.split('.');
            var last = keys.pop();
            var target = keys.reduce(function(o, k) { return o[k] = o[k] || {}; }, obj);
            target[last] = value;
        },

        /**
         * Toast notification
         */
        toast: function(message, type) {
            type = type || 'info';
            var container = document.querySelector('.s-toast-container');
            if (!container) {
                this.initToastContainer();
                container = document.querySelector('.s-toast-container');
            }
            
            var toast = document.createElement('div');
            toast.className = 's-toast s-toast-' + type;
            toast.innerHTML = '<span>' + message + '</span><button onclick="this.parentElement.remove()">&times;</button>';
            container.appendChild(toast);
            
            setTimeout(function() { toast.classList.add('s-show'); }, 10);
            setTimeout(function() {
                toast.classList.remove('s-show');
                setTimeout(function() { toast.remove(); }, 300);
            }, 4000);
        },

        /**
         * Initialize toast container
         */
        initToastContainer: function() {
            if (!document.querySelector('.s-toast-container')) {
                var container = document.createElement('div');
                container.className = 's-toast-container';
                document.body.appendChild(container);
            }
        },

        /**
         * Get state by container ID
         */
        getState: function(id) {
            return this.state.get(id);
        }
    };

    // Alias
    window.$s = window.SvelteHybrid;

    // Auto-init on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() { 
            SvelteHybrid.init(); 
        });
    } else {
        SvelteHybrid.init();
    }

})();
