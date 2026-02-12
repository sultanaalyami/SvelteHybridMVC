<script>
    /** @type {{ signedIn: boolean, userName: string }} */
    let authState = { signedIn: false, userName: '' };
    let loading = true;

    // SSE: real-time auth state from server
    function connectAuthStream() {
        const source = new EventSource('/api/auth/state');

        source.onmessage = (event) => {
            try {
                authState = JSON.parse(event.data);
                loading = false;
            } catch { /* ignore parse errors */ }
        };

        source.onerror = () => {
            source.close();
            // Fallback: one-time fetch
            fetchAuthState();
        };

        return source;
    }

    async function fetchAuthState() {
        try {
            const res = await fetch('/api/auth/state/check');
            authState = await res.json();
        } catch { /* offline fallback */ }
        loading = false;
    }

    async function logout() {
        const token = document.querySelector('[name="__RequestVerificationToken"]')?.value || '';
        await fetch('/Identity/Account/Logout', {
            method: 'POST',
            headers: { 'RequestVerificationToken': token }
        });
        window.location.href = '/';
    }

    import { onMount, onDestroy } from 'svelte';
    let source;
    onMount(() => { source = connectAuthStream(); });
    onDestroy(() => { source?.close(); });
</script>

<div class="auth-widget">
    {#if loading}
        <span class="auth-loading"></span>
    {:else if authState.signedIn}
        <a class="auth-link" href="/Identity/Account/Manage">
            <span class="auth-avatar">{authState.userName?.charAt(0)?.toUpperCase() || '?'}</span>
            {authState.userName}
        </a>
        <button class="auth-btn auth-btn--logout" on:click={logout}>Logout</button>
    {:else}
        <a class="auth-link" href="/Identity/Account/Register">Register</a>
        <a class="auth-btn auth-btn--login" href="/Identity/Account/Login">Login</a>
    {/if}
</div>

<style>
    .auth-widget {
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }
    .auth-loading {
        width: 1rem;
        height: 1rem;
        border: 2px solid rgba(255,255,255,0.2);
        border-top-color: #38bdf8;
        border-radius: 50%;
        animation: spin 0.6s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .auth-avatar {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 1.6rem;
        height: 1.6rem;
        border-radius: 50%;
        background: linear-gradient(135deg, #38bdf8, #a78bfa);
        color: #0f172a;
        font-weight: 700;
        font-size: 0.75rem;
        margin-inline-end: 0.3rem;
    }
    .auth-link {
        text-decoration: none;
        color: #e2e8f0;
        padding: 0.4rem 0.7rem;
        border-radius: 999px;
        transition: background 150ms;
    }
    .auth-link:hover { background: rgba(148,163,184,0.12); }
    .auth-btn {
        padding: 0.4rem 0.8rem;
        border-radius: 999px;
        border: none;
        cursor: pointer;
        font: inherit;
        font-size: 0.85rem;
        text-decoration: none;
        transition: all 150ms;
    }
    .auth-btn--login {
        background: linear-gradient(135deg, #38bdf8, #22d3ee);
        color: #0f172a;
        font-weight: 600;
    }
    .auth-btn--logout {
        background: transparent;
        border: 1px solid rgba(239,68,68,0.4);
        color: #fca5a5;
    }
    .auth-btn--logout:hover {
        background: rgba(239,68,68,0.16);
        color: #fff;
    }
</style>
