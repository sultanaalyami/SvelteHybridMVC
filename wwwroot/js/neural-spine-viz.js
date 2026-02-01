/**
 * Neural Spine Visualization - تصور النخاع الشوكي الرقمي
 * نظام تفاعلي ثنائي الأبعاد لتصور بيانات eBPF كنخاع شوكي حي
 */

class NeuralSpineVisualization {
    constructor(containerId, options = {}) {
        this.container = document.getElementById(containerId);
        this.options = {
            spineSegments: options.spineSegments || 50,
            nervesPerSegment: options.nervesPerSegment || 8,
            maxActiveNerves: options.maxActiveNerves || 100,
            pulseSpeed: options.pulseSpeed || 2.0,
            glowIntensity: options.glowIntensity || 1.5,
            rotationSpeed: options.rotationSpeed || 0.2,
            ...options
        };

        this.activeNerves = new Map();
        this.dataQueue = [];
        this.stats = { total: 0, active: 0, peakRate: 0 };
        
        this.init();
    }

    init() {
        this.setupCanvas();
        this.setupScene();
        this.createSpinalCord();
        this.createNerveEndings();
        this.setupControls();
        this.animate();
    }

    setupCanvas() {
        this.canvas = document.createElement('canvas');
        this.container.appendChild(this.canvas);
        this.ctx = this.canvas.getContext('2d', { 
            alpha: true,
            willReadFrequently: false
        });

        const resize = () => {
            this.canvas.width = this.container.clientWidth;
            this.canvas.height = this.container.clientHeight;
        };
        
        window.addEventListener('resize', resize);
        resize();
    }

    setupScene() {
        this.scene = {
            rotation: 0,
            camera: {
                zoom: 1.0,
                offsetX: 0,
                offsetY: 0
            }
        };
    }

    createSpinalCord() {
        const segments = this.options.spineSegments;
        const radius = 0.3;
        const height = 20;
        const segmentHeight = height / segments;

        this.spinalCord = {
            segments: []
        };

        for (let i = 0; i < segments; i++) {
            const y = (i - segments / 2) * segmentHeight;
            const segment = {
                y: y,
                radius: radius * (1 - Math.abs(i - segments / 2) / segments * 0.3),
                activity: 0,
                pulsePhase: Math.random() * Math.PI * 2
            };
            
            this.spinalCord.segments.push(segment);
        }
    }

    createNerveEndings() {
        this.nerves = [];
        const segments = this.options.spineSegments;
        const nervesPerSeg = this.options.nervesPerSegment;

        for (let i = 0; i < segments; i++) {
            const segment = this.spinalCord.segments[i];
            
            for (let j = 0; j < nervesPerSeg; j++) {
                const angle = (j / nervesPerSeg) * Math.PI * 2;
                const side = j % 2 === 0 ? 1 : -1;
                
                const nerve = {
                    segmentIndex: i,
                    angle: angle,
                    baseRadius: segment.radius,
                    endRadius: (segment.radius + 3) * side,
                    baseY: segment.y,
                    endY: segment.y + (Math.random() - 0.5) * 0.5,
                    active: false,
                    intensity: 0,
                    pulsePhase: 0,
                    dataType: null
                };

                this.nerves.push(nerve);
            }
        }
    }

    setupControls() {
        let isDragging = false;
        let lastX = 0, lastY = 0;

        this.canvas.addEventListener('mousedown', (e) => {
            isDragging = true;
            lastX = e.clientX;
            lastY = e.clientY;
        });

        this.canvas.addEventListener('mousemove', (e) => {
            if (isDragging) {
                const deltaX = e.clientX - lastX;
                this.scene.rotation += deltaX * 0.005;
                lastX = e.clientX;
                lastY = e.clientY;
            }
        });

        this.canvas.addEventListener('mouseup', () => isDragging = false);
        this.canvas.addEventListener('mouseleave', () => isDragging = false);
        
        this.canvas.addEventListener('wheel', (e) => {
            e.preventDefault();
            this.scene.camera.zoom *= (1 - e.deltaY * 0.001);
            this.scene.camera.zoom = Math.max(0.5, Math.min(2.0, this.scene.camera.zoom));
        });
    }

    processEbpfData(data) {
        if (!data) return;

        const nerve = this.findAvailableNerve(data);
        if (nerve) {
            this.activateNerve(nerve, data);
        }

        this.dataQueue.push(data);
        if (this.dataQueue.length > 1000) {
            this.dataQueue.shift();
        }

        this.stats.total++;
        this.updateStats();
    }

    findAvailableNerve(data) {
        const segmentIndex = this.mapDataToSegment(data);
        const candidates = this.nerves.filter(n => 
            n.segmentIndex === segmentIndex && !n.active
        );
        
        return candidates.length > 0 
            ? candidates[Math.floor(Math.random() * candidates.length)]
            : null;
    }

    mapDataToSegment(data) {
        if (data.protocol === 6) return Math.floor(Math.random() * this.options.spineSegments * 0.3);
        if (data.protocol === 17) return Math.floor(this.options.spineSegments * 0.3 + Math.random() * this.options.spineSegments * 0.3);
        return Math.floor(this.options.spineSegments * 0.6 + Math.random() * this.options.spineSegments * 0.4);
    }

    activateNerve(nerve, data) {
        nerve.active = true;
        nerve.intensity = 1.0;
        nerve.pulsePhase = 0;
        nerve.dataType = data.protocol;
        nerve.dataSize = data.bytes || 0;

        this.activeNerves.set(nerve, Date.now());
        this.stats.active = this.activeNerves.size;

        const segment = this.spinalCord.segments[nerve.segmentIndex];
        segment.activity = Math.min(1.0, segment.activity + 0.3);

        setTimeout(() => this.deactivateNerve(nerve), 1000 + Math.random() * 2000);
    }

    deactivateNerve(nerve) {
        nerve.active = false;
        this.activeNerves.delete(nerve);
        this.stats.active = this.activeNerves.size;
    }

    updateStats() {
        const now = Date.now();
        if (!this.lastStatsUpdate) this.lastStatsUpdate = now;
        
        const elapsed = (now - this.lastStatsUpdate) / 1000;
        if (elapsed > 1) {
            const rate = this.stats.total / elapsed;
            this.stats.peakRate = Math.max(this.stats.peakRate, rate);
            this.lastStatsUpdate = now;
        }
    }

    animate(time = 0) {
        this.animationFrame = requestAnimationFrame((t) => this.animate(t));

        const dt = time - (this.lastTime || time);
        this.lastTime = time;

        this.scene.rotation += this.options.rotationSpeed * 0.001;

        this.spinalCord.segments.forEach(seg => {
            seg.pulsePhase += 0.05;
            seg.activity *= 0.95;
        });

        this.nerves.forEach(nerve => {
            if (nerve.active) {
                nerve.pulsePhase += this.options.pulseSpeed * 0.1;
                nerve.intensity *= 0.98;
                
                if (nerve.intensity < 0.05) {
                    this.deactivateNerve(nerve);
                }
            }
        });

        this.render();
    }

    render() {
        const ctx = this.ctx;
        if (!ctx) return;

        ctx.fillStyle = 'rgba(2, 2, 5, 1.0)';
        ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

        this.renderSpinalCord();
        this.renderNerves();
    }

    renderSpinalCord() {
        const ctx = this.ctx;
        if (!ctx) return;

        const centerX = this.canvas.width / 2;
        const centerY = this.canvas.height / 2;
        const scale = 20 * this.scene.camera.zoom;

        this.spinalCord.segments.forEach((seg, i) => {
            const y = centerY + seg.y * scale;
            const radius = seg.radius * scale;
            
            const pulse = Math.sin(seg.pulsePhase) * 0.2 + 0.8;
            const activity = seg.activity;

            const gradient = ctx.createRadialGradient(centerX, y, 0, centerX, y, radius * (1 + activity));
            gradient.addColorStop(0, `rgba(100, 150, 255, ${0.8 * pulse})`);
            gradient.addColorStop(0.5, `rgba(50, 100, 200, ${0.4 * pulse})`);
            gradient.addColorStop(1, `rgba(20, 50, 150, 0)`);

            ctx.fillStyle = gradient;
            ctx.beginPath();
            ctx.arc(centerX, y, radius * (1 + activity * 0.5), 0, Math.PI * 2);
            ctx.fill();

            if (activity > 0.1) {
                ctx.strokeStyle = `rgba(150, 200, 255, ${activity})`;
                ctx.lineWidth = 2;
                ctx.stroke();
            }
        });
    }

    renderNerves() {
        const ctx = this.ctx;
        if (!ctx) return;

        const centerX = this.canvas.width / 2;
        const centerY = this.canvas.height / 2;
        const scale = 20 * this.scene.camera.zoom;

        this.nerves.forEach(nerve => {
            if (!nerve.active && nerve.intensity < 0.01) return;

            const segment = this.spinalCord.segments[nerve.segmentIndex];
            const baseY = centerY + nerve.baseY * scale;

            const colorMap = {
                6: [100, 255, 100],
                17: [255, 200, 100],
                default: [200, 100, 255]
            };

            const color = colorMap[nerve.dataType] || colorMap.default;
            const alpha = nerve.intensity * (Math.sin(nerve.pulsePhase) * 0.3 + 0.7);

            ctx.save();
            ctx.translate(centerX, baseY);
            ctx.rotate(this.scene.rotation + nerve.angle);

            ctx.strokeStyle = `rgba(${color[0]}, ${color[1]}, ${color[2]}, ${alpha})`;
            ctx.lineWidth = 2 + nerve.intensity * 2;
            ctx.shadowBlur = 10 * nerve.intensity * this.options.glowIntensity;
            ctx.shadowColor = `rgba(${color[0]}, ${color[1]}, ${color[2]}, ${alpha})`;

            ctx.beginPath();
            ctx.moveTo(0, 0);
            
            const endX = nerve.endRadius * scale;
            const endY = (nerve.endY - nerve.baseY) * scale;
            
            const cp1x = endX * 0.3;
            const cp1y = endY * 0.3;
            const cp2x = endX * 0.7;
            const cp2y = endY * 0.7;
            
            ctx.bezierCurveTo(cp1x, cp1y, cp2x, cp2y, endX, endY);
            ctx.stroke();

            if (nerve.intensity > 0.5) {
                const gradient = ctx.createRadialGradient(endX, endY, 0, endX, endY, 10);
                gradient.addColorStop(0, `rgba(${color[0]}, ${color[1]}, ${color[2]}, ${alpha})`);
                gradient.addColorStop(1, `rgba(${color[0]}, ${color[1]}, ${color[2]}, 0)`);

                ctx.fillStyle = gradient;
                ctx.beginPath();
                ctx.arc(endX, endY, 5 + nerve.intensity * 5, 0, Math.PI * 2);
                ctx.fill();
            }

            ctx.restore();
        });
    }

    getStats() {
        return {
            ...this.stats,
            activeNerves: this.activeNerves.size,
            queueSize: this.dataQueue.length
        };
    }

    destroy() {
        if (this.animationFrame) {
            cancelAnimationFrame(this.animationFrame);
        }
        this.container.removeChild(this.canvas);
    }
}

window.NeuralSpineVisualization = NeuralSpineVisualization;
