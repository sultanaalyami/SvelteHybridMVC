const express = require('express');
const app = express();
const port = process.env.PORT || 3000;

app.use(express.json());

// محاكاة تسجيل المكونات (في الواقع سيتم تحميلها ديناميكياً)
const components = {
    'ProductCard': {
        render: (props) => {
            // محاكاة مخرجات Svelte SSR
            // في الإنتاج: require(`./Components/${name}.svelte`).default.render(props)
            const { model } = props;
            const stockBadge = model.IsInStock 
                ? '<span class="badge badge-success">متوفر</span>' 
                : '<span class="badge badge-danger">نفذ</span>';
                
            return {
                html: `
                    <div class="svelte-component-root">
                        <div class="product-card-svelte">
                            <img src="${model.ImageUrl}" alt="${model.Title}" loading="lazy" />
                            <h3>${model.Title} (Svelte SSR)</h3>
                            <p>${model.Description}</p>
                            <div class="price">${model.PriceDisplay}</div>
                            ${stockBadge}
                            <button class="btn-favorite">❤️</button>
                        </div>
                    </div>
                `,
                css: { code: '.product-card-svelte { border: 1px solid #ccc; }' },
                head: ''
            };
        }
    }
};

app.post('/render', (req, res) => {
    const { componentName, props } = req.body;
    
    console.log(`Rendering component: ${componentName}`);

    try {
        const component = components[componentName];
        
        if (!component) {
            return res.status(404).json({ error: `Component '${componentName}' not found` });
        }

        // استدعاء دالة العرض للمكون
        const result = component.render(props);
        
        res.json(result);
    } catch (error) {
        console.error('SSR Error:', error);
        res.status(500).json({ error: error.message });
    }
});

app.listen(port, () => {
    console.log(`HRCE SSR Server listening on port ${port}`);
});
