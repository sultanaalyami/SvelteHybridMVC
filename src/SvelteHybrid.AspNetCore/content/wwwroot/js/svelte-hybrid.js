/**
 * SvelteHybrid Client Runtime
 * Handles client-side hydration of Svelte components
 */
(function (global) {
    'use strict';

    const SvelteHybrid = {
        components: new Map(),
        initialized: false,

        /**
         * Register a Svelte component
         * @param {string} name - Component name
         * @param {Function} component - Svelte component constructor
         */
        register(name, component) {
            this.components.set(name, component);
            console.debug(`[SvelteHybrid] Registered component: ${name}`);
        },

        /**
         * Initialize and hydrate all components on the page
         */
        init() {
            if (this.initialized) return;
            
            const elements = document.querySelectorAll('[data-svelte-component]');
            
            elements.forEach(el => {
                this.hydrateElement(el);
            });

            this.initialized = true;
            console.debug(`[SvelteHybrid] Initialized ${elements.length} components`);
        },

        /**
         * Hydrate a single element
         * @param {HTMLElement} element - Element to hydrate
         */
        hydrateElement(element) {
            const componentName = element.dataset.svelteComponent;
            const Component = this.components.get(componentName);

            if (!Component) {
                console.warn(`[SvelteHybrid] Component not found: ${componentName}`);
                return;
            }

            const propsScript = element.querySelector('[data-component-props]');
            let props = {};

            if (propsScript) {
                try {
                    props = JSON.parse(propsScript.textContent || '{}');
                } catch (e) {
                    console.error(`[SvelteHybrid] Failed to parse props for ${componentName}`, e);
                }
            }

            const isSSR = element.dataset.svelteSsr !== 'false';
            const isClientOnly = element.dataset.svelteClientOnly === 'true';

            try {
                const instance = new Component({
                    target: element,
                    props,
                    hydrate: isSSR && !isClientOnly
                });

                element._svelteInstance = instance;
                element.dataset.svelteHydrated = 'true';
                
                console.debug(`[SvelteHybrid] Hydrated: ${componentName}`);
            } catch (e) {
                console.error(`[SvelteHybrid] Failed to hydrate ${componentName}`, e);
            }
        },

        /**
         * Destroy a component instance
         * @param {HTMLElement} element - Element containing the component
         */
        destroy(element) {
            if (element._svelteInstance) {
                element._svelteInstance.$destroy();
                delete element._svelteInstance;
            }
        },

        /**
         * Update component props
         * @param {HTMLElement} element - Element containing the component
         * @param {Object} props - New props
         */
        updateProps(element, props) {
            if (element._svelteInstance) {
                element._svelteInstance.$set(props);
            }
        },

        /**
         * Re-hydrate components (useful after dynamic content load)
         */
        refresh() {
            const elements = document.querySelectorAll('[data-svelte-component]:not([data-svelte-hydrated])');
            elements.forEach(el => this.hydrateElement(el));
        }
    };

    // Auto-initialize on DOMContentLoaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => SvelteHybrid.init());
    } else {
        SvelteHybrid.init();
    }

    // Export to global
    global.SvelteHybrid = SvelteHybrid;

})(typeof window !== 'undefined' ? window : this);
