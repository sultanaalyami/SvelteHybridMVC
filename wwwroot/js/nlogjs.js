window.nlogjs = (() => {
    const endpoint = '/api/logs';

    const send = (level, message) => {
        const payload = {
            level,
            message: typeof message === 'string' ? message : JSON.stringify(message)
        };

        if (navigator.sendBeacon) {
            const blob = new Blob([JSON.stringify(payload)], { type: 'application/json' });
            navigator.sendBeacon(endpoint, blob);
            return;
        }

        fetch(endpoint, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        }).catch(() => {});
    };

    return {
        trace: (message) => send('TRACE', message),
        debug: (message) => send('DEBUG', message),
        info: (message) => send('INFO', message),
        warn: (message) => send('WARN', message),
        error: (message) => send('ERROR', message),
        critical: (message) => send('CRITICAL', message)
    };
})();
