<script>
    /** @type {string} */
    export let title = 'Hello';
    
    /** @type {string} */
    export let message = 'World';
</script>

<div class="example-component">
    <h2>{title}</h2>
    <p>{message}</p>
    <button on:click={() => alert('Clicked!')}>
        Click Me
    </button>
</div>

<style>
    .example-component {
        padding: 1rem;
        border: 1px solid #e5e7eb;
        border-radius: 8px;
        background: #f9fafb;
    }
    
    h2 {
        margin: 0 0 0.5rem;
        color: #1f2937;
    }
    
    p {
        color: #6b7280;
    }
    
    button {
        background: #3b82f6;
        color: white;
        border: none;
        padding: 0.5rem 1rem;
        border-radius: 4px;
        cursor: pointer;
        transition: background 0.2s;
    }
    
    button:hover {
        background: #2563eb;
    }
</style>
