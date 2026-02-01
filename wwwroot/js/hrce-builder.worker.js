self.onmessage = (event) => {
    const { type, payload } = event.data;
    if (type === 'page') {
        const request = {
            name: payload.name?.trim(),
            title: payload.title?.trim(),
            role: payload.role?.trim() || null,
            policy: payload.policy?.trim() || null
        };
        self.postMessage({ type: 'page', request });
    }

    if (type === 'rule') {
        const request = {
            scope: 'Navigation',
            controller: payload.controller?.trim() || null,
            action: payload.action?.trim() || null,
            role: payload.role?.trim() || null,
            claimType: payload.claimType?.trim() || null,
            claimValue: payload.claimValue?.trim() || null,
            isVisible: payload.isVisible === 'true',
            priority: 5
        };
        self.postMessage({ type: 'rule', request });
    }
};
