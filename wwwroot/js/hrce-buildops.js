const commandsContainer = document.getElementById('buildops-commands');
const historyTable = document.getElementById('buildops-history');
const statusBox = document.getElementById('buildops-status');
const metaBox = document.getElementById('buildops-meta');
const outputBox = document.getElementById('buildops-output');
const refreshButton = document.getElementById('buildops-refresh');

const getAntiForgeryToken = () =>
    document.querySelector('input[name="__RequestVerificationToken"]')?.value;

let activeJobId = null;
let refreshTimer = null;

const formatTime = (value) => value ? new Date(value).toLocaleString() : '-';
const formatState = (state) => {
    switch (state) {
        case 0: return 'قيد الانتظار';
        case 1: return 'قيد التشغيل';
        case 2: return 'ناجح';
        case 3: return 'فشل';
        case 4: return 'موقوف';
        default: return 'غير معروف';
    }
};

const setStatus = (text, state) => {
    statusBox.textContent = text;
    statusBox.dataset.state = state ?? '';
};

const renderCommands = (commands) => {
    if (!commands.length) {
        commandsContainer.innerHTML = '<div class="hrce-muted">لا توجد أوامر معرفة بعد.</div>';
        return;
    }

    commandsContainer.innerHTML = commands.map(command => `
        <div class="hrce-buildops__command">
            <div>
                <strong>${command.label}</strong>
                <div class="hrce-muted">${command.description ?? ''}</div>
            </div>
            <button class="btn btn-primary" data-command-key="${command.key}">تشغيل</button>
        </div>
    `).join('');
};

const renderHistory = (jobs) => {
    if (!jobs.length) {
        historyTable.innerHTML = '<tr><td colspan="5">لا توجد عمليات حتى الآن.</td></tr>';
        return;
    }

    historyTable.innerHTML = jobs.map(job => `
        <tr data-job-id="${job.id}">
            <td>${formatState(job.state)}</td>
            <td>${job.label}</td>
            <td>${formatTime(job.startedAtUtc ?? job.createdAtUtc)}</td>
            <td>${formatTime(job.completedAtUtc)}</td>
            <td>
                <button class="btn btn-light" data-action="details" data-job-id="${job.id}">عرض</button>
                ${job.state === 1 ? `<button class="btn btn-secondary" data-action="cancel" data-job-id="${job.id}">إيقاف</button>` : ''}
            </td>
        </tr>
    `).join('');
};

const renderJobDetails = (job) => {
    activeJobId = job?.id ?? null;
    if (!job) {
        setStatus('لم يتم بدء أي عملية بعد.', null);
        metaBox.textContent = '';
        outputBox.textContent = '';
        return;
    }

    const stateLabel = formatState(job.state);
    setStatus(`${stateLabel} - ${job.label}`, job.state);
    metaBox.textContent = `بدأت: ${formatTime(job.startedAtUtc ?? job.createdAtUtc)} | انتهت: ${formatTime(job.completedAtUtc)} | Exit: ${job.exitCode ?? '-'}`;
    outputBox.innerHTML = (job.output?.length ? job.output : ['لا يوجد إخراج بعد.'])
        .map(line => `<div>${line}</div>`).join('');
};

const fetchCommands = async () => {
    const response = await fetch('/api/buildops/commands');
    if (!response.ok) {
        commandsContainer.innerHTML = '<div class="hrce-muted">تعذر تحميل الأوامر.</div>';
        return;
    }
    const data = await response.json();
    renderCommands(data);
};

const fetchJobs = async () => {
    const response = await fetch('/api/buildops/jobs');
    if (!response.ok) {
        historyTable.innerHTML = '<tr><td colspan="5">تعذر تحميل السجل.</td></tr>';
        return;
    }
    const jobs = await response.json();
    renderHistory(jobs);
    if (activeJobId) {
        const job = jobs.find(j => j.id === activeJobId);
        if (job) {
            renderJobDetails(job);
        }
    }
};

const fetchJob = async (jobId) => {
    const response = await fetch(`/api/buildops/jobs/${jobId}`);
    if (!response.ok) {
        return;
    }
    const job = await response.json();
    renderJobDetails(job);
};

const runCommand = async (commandKey) => {
    const token = getAntiForgeryToken();
    const response = await fetch('/api/buildops/run', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            ...(token ? { 'RequestVerificationToken': token } : {})
        },
        body: JSON.stringify({ commandKey })
    });

    if (!response.ok) {
        setStatus('فشل تشغيل الأمر.', null);
        return;
    }

    const job = await response.json();
    renderJobDetails(job);
    await fetchJobs();
    startPolling();
};

const cancelJob = async (jobId) => {
    const token = getAntiForgeryToken();
    const response = await fetch(`/api/buildops/cancel/${jobId}`, {
        method: 'POST',
        headers: {
            ...(token ? { 'RequestVerificationToken': token } : {})
        }
    });
    if (response.ok) {
        await fetchJobs();
        if (activeJobId === jobId) {
            await fetchJob(jobId);
        }
    }
};

const startPolling = () => {
    if (refreshTimer) {
        clearInterval(refreshTimer);
    }
    refreshTimer = setInterval(() => {
        fetchJobs();
        if (activeJobId) {
            fetchJob(activeJobId);
        }
    }, 2000);
};

commandsContainer?.addEventListener('click', (event) => {
    const button = event.target.closest('[data-command-key]');
    if (!button) {
        return;
    }
    runCommand(button.dataset.commandKey);
});

historyTable?.addEventListener('click', (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) {
        return;
    }
    const jobId = button.dataset.jobId;
    if (!jobId) {
        return;
    }
    if (button.dataset.action === 'details') {
        fetchJob(jobId);
    }
    if (button.dataset.action === 'cancel') {
        cancelJob(jobId);
    }
});

refreshButton?.addEventListener('click', () => {
    fetchJobs();
});

fetchCommands();
fetchJobs();
startPolling();
