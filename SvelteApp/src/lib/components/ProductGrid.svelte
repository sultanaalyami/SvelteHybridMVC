<script>
    /** @type {import('./$types').PageLoad} */
    export let data;
</script>

<div class="product-grid">
    {#each data.products as product (product.id)}
        <div class="product-card">
            <h3>{product.name}</h3>
            <p class="price">{product.price}</p>
            <span class="stock" class:low={product.stock < 10}>
                المخزون: {product.stock}
            </span>
        </div>
    {/each}
</div>

<style>
    .product-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
        gap: 1.5rem;
    }
    
    .product-card {
        background: white;
        padding: 1.5rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    
    .price { color: #ff3e00; font-weight: bold; }
    .stock { font-size: 0.875rem; }
    .stock.low { color: #dc3545; }
</style>
