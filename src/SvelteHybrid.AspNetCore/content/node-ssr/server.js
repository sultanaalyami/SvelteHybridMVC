/**
 * SvelteHybrid Node.js SSR Server
 * Express server for server-side rendering Svelte components
 */

const express = require('express');
const { readFileSync, existsSync } = require('fs');
const { join } = require('path');

const app = express();
app.use(express.json());

const PORT = process.env.PORT || 3000;
const COMPONENTS_PATH = process.env.COMPONENTS_PATH || './components';

// Component cache
const componentCache = new Map();
let startTime = Date.now();

/**
 * Load and compile a Svelte component
 */
function loadComponent(name) {
    if (componentCache.has(name)) {
        return componentCache.get(name);
    }

    const componentPath = join(COMPONENTS_PATH, name, `${name}.js`);
    
    if (!existsSync(componentPath)) {
        throw new Error(`Component not found: ${name}`);
    }

    // In production, components should be pre-compiled
    const Component = require(componentPath).default;
    componentCache.set(name, Component);
    
    return Component;
}

/**
 * Render endpoint
 */
app.post('/render', async (req, res) => {
    const { component: componentName, props = {} } = req.body;

    if (!componentName) {
        return res.status(400).json({ error: 'Component name required' });
    }

    try {
        const Component = loadComponent(componentName);
        
        const startRender = performance.now();
        const { html, css, head } = Component.render(props);
        const renderTime = performance.now() - startRender;

        res.json({
            html,
            css: { code: css?.code || '', map: css?.map },
            head: head || '',
            renderTime
        });
    } catch (error) {
        console.error(`Error rendering ${componentName}:`, error);
        res.status(500).json({ error: error.message });
    }
});

/**
 * Health check endpoint
 */
app.get('/health', (req, res) => {
    res.json({ status: 'healthy' });
});

/**
 * Status endpoint
 */
app.get('/status', (req, res) => {
    res.json({
        version: process.version,
        uptimeSeconds: (Date.now() - startTime) / 1000,
        componentsLoaded: componentCache.size
    });
});

/**
 * Clear cache endpoint
 */
app.post('/cache/clear', (req, res) => {
    componentCache.clear();
    res.json({ message: 'Cache cleared' });
});

app.listen(PORT, () => {
    console.log(`SvelteHybrid SSR server running on port ${PORT}`);
    console.log(`Components path: ${COMPONENTS_PATH}`);
});
