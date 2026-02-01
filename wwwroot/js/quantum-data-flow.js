/**
 * Quantum Data Flow System - نظام تدفق البيانات الكمي
 * يعالج ويصنف تدفقات eBPF كجسيمات كمية في فضاء المعالج
 */

class QuantumDataFlowSystem {
    constructor(engine) {
        this.engine = engine;
        this.classifiers = new Map();
        this.neuralModels = new Map();
        this.flowPatterns = [];
        this.anomalyDetector = new AnomalyDetector();
        this.quantumStates = new QuantumStateManager();
        
        this.initializeClassifiers();
    }

    initializeClassifiers() {
        // مصنف حركة الشبكة
        this.classifiers.set('network', {
            patterns: [
                { name: 'TCP_HANDSHAKE', signature: [6, 'SYN'], color: 0x44ff88 },
                { name: 'TCP_DATA', signature: [6, 'ACK', 'PSH'], color: 0x44ff44 },
                { name: 'TCP_FIN', signature: [6, 'FIN'], color: 0xff8844 },
                { name: 'UDP_STREAM', signature: [17], color: 0xffaa44 },
                { name: 'DNS_QUERY', signature: [17, 53], color: 0x44aaff },
                { name: 'HTTPS', signature: [6, 443], color: 0x88ff44 }
            ],
            threshold: 0.7
        });

        // مصنف عمليات النواة
        this.classifiers.set('kernel', {
            patterns: [
                { name: 'SYSCALL_READ', id: 0, color: 0x4488ff },
                { name: 'SYSCALL_WRITE', id: 1, color: 0x8844ff },
                { name: 'SYSCALL_OPEN', id: 2, color: 0xff4488 },
                { name: 'SYSCALL_CLOSE', id: 3, color: 0xff8844 },
                { name: 'SYSCALL_MMAP', id: 9, color: 0x44ffaa },
                { name: 'SYSCALL_FORK', id: 57, color: 0xaaff44 }
            ],
            threshold: 0.8
        });

        // مصنف أنماط الذاكرة
        this.classifiers.set('memory', {
            patterns: [
                { name: 'ALLOCATION', type: 'alloc', color: 0x44ff44 },
                { name: 'DEALLOCATION', type: 'free', color: 0xff4444 },
                { name: 'PAGE_FAULT', type: 'fault', color: 0xffff44 },
                { name: 'CACHE_MISS', type: 'miss', color: 0xff44ff }
            ],
            threshold: 0.6
        });
    }

    processEvent(event) {
        // تصنيف الحدث
        const classification = this.classifyEvent(event);
        
        // تحويل إلى حالة كمية
        const quantumState = this.toQuantumState(event, classification);
        
        // كشف الشذوذ
        const anomaly = this.anomalyDetector.analyze(event, classification);
        
        // تحديث النماذج
        this.updateModels(event, classification);
        
        return {
            original: event,
            classification,
            quantumState,
            anomaly,
            visualParams: this.getVisualizationParams(classification, anomaly)
        };
    }

    classifyEvent(event) {
        const type = event.type || 'unknown';
        const classifier = this.classifiers.get(type);
        
        if (!classifier) {
            return { type: 'unknown', confidence: 0, pattern: null };
        }

        let bestMatch = { pattern: null, confidence: 0 };
        
        for (const pattern of classifier.patterns) {
            const confidence = this.matchPattern(event, pattern);
            if (confidence > bestMatch.confidence) {
                bestMatch = { pattern, confidence };
            }
        }

        return {
            type,
            pattern: bestMatch.pattern,
            confidence: bestMatch.confidence,
            isValid: bestMatch.confidence >= classifier.threshold
        };
    }

    matchPattern(event, pattern) {
        let score = 0;
        let maxScore = 0;

        if (pattern.signature) {
            for (const sig of pattern.signature) {
                maxScore++;
                if (event.protocol === sig || 
                    event.port === sig || 
                    event.flags?.includes(sig)) {
                    score++;
                }
            }
        }

        if (pattern.id !== undefined && event.syscall_id === pattern.id) {
            score++;
            maxScore++;
        }

        if (pattern.type && event.memType === pattern.type) {
            score++;
            maxScore++;
        }

        return maxScore > 0 ? score / maxScore : 0;
    }

    toQuantumState(event, classification) {
        // تحويل الحدث إلى تمثيل كمي
        const state = {
            // موقع في فضاء هيلبرت
            amplitude: this.calculateAmplitude(event),
            phase: this.calculatePhase(event),
            
            // التراكب
            superposition: classification.confidence < 0.9,
            
            // التشابك مع أحداث أخرى
            entanglement: this.findEntanglements(event),
            
            // الاحتمالات
            probabilities: this.calculateProbabilities(event, classification),
            
            // السبين (اتجاه التدفق)
            spin: event.bytes > 1000 ? 'up' : 'down',
            
            // مستوى الطاقة
            energy: this.calculateEnergy(event)
        };

        this.quantumStates.register(event.id || Date.now(), state);
        return state;
    }

    calculateAmplitude(event) {
        // سعة الموجة بناءً على حجم البيانات
        const bytes = event.bytes || 100;
        return Math.min(1, Math.log10(bytes + 1) / 5);
    }

    calculatePhase(event) {
        // الطور بناءً على التوقيت
        const timestamp = event.timestamp || Date.now();
        return (timestamp % 6283) / 1000; // 0 to 2π
    }

    findEntanglements(event) {
        // البحث عن أحداث متشابكة (نفس المصدر/الوجهة، تدفقات مرتبطة)
        const entangled = [];
        
        // محاكاة التشابك الكمي
        if (event.src_ip && event.dst_ip) {
            entangled.push({
                type: 'network_pair',
                strength: 0.8,
                partner: `${event.dst_ip}:${event.src_ip}`
            });
        }

        if (event.process_id) {
            entangled.push({
                type: 'process_thread',
                strength: 0.6,
                partner: `thread_${event.thread_id || 0}`
            });
        }

        return entangled;
    }

    calculateProbabilities(event, classification) {
        // احتمالات الانهيار لحالات مختلفة
        const probs = {};
        const classifier = this.classifiers.get(classification.type);
        
        if (classifier) {
            for (const pattern of classifier.patterns) {
                const conf = this.matchPattern(event, pattern);
                probs[pattern.name] = conf;
            }
        }

        // تطبيع الاحتمالات
        const sum = Object.values(probs).reduce((a, b) => a + b, 0);
        if (sum > 0) {
            for (const key in probs) {
                probs[key] /= sum;
            }
        }

        return probs;
    }

    calculateEnergy(event) {
        // مستوى الطاقة الكمي
        const bytes = event.bytes || 0;
        const rate = event.rate || 1;
        
        // معادلة E = hf (تبسيط)
        const planck = 0.001;
        const frequency = rate * 1000;
        
        return Math.min(1, planck * frequency + Math.log10(bytes + 1) / 10);
    }

    getVisualizationParams(classification, anomaly) {
        const params = {
            // اللون الأساسي
            color: classification.pattern?.color || 0x4488ff,
            
            // الشدة
            intensity: classification.confidence,
            
            // الحجم
            size: 1 + classification.confidence * 2,
            
            // نوع الجسيم
            particleType: this.getParticleType(classification),
            
            // تأثيرات خاصة
            effects: []
        };

        // إضافة تأثيرات الشذوذ
        if (anomaly.isAnomaly) {
            params.effects.push({
                type: 'pulse',
                color: 0xff0000,
                intensity: anomaly.score
            });
        }

        // تأثير التراكب
        if (classification.confidence < 0.7) {
            params.effects.push({
                type: 'blur',
                amount: 1 - classification.confidence
            });
        }

        return params;
    }

    getParticleType(classification) {
        const typeMap = {
            'network': 'photon',
            'kernel': 'electron',
            'memory': 'quark',
            'process': 'neutrino',
            'syscall': 'boson'
        };
        return typeMap[classification.type] || 'fermion';
    }

    updateModels(event, classification) {
        // تحديث النماذج التعلمية
        const modelKey = classification.type;
        
        if (!this.neuralModels.has(modelKey)) {
            this.neuralModels.set(modelKey, new SimpleNeuralModel(modelKey));
        }

        const model = this.neuralModels.get(modelKey);
        model.train(event, classification);
    }

    getStatistics() {
        return {
            totalClassified: this.quantumStates.count,
            classifierStats: this.getClassifierStats(),
            anomalyRate: this.anomalyDetector.getRate(),
            entanglementDensity: this.quantumStates.getEntanglementDensity()
        };
    }

    getClassifierStats() {
        const stats = {};
        for (const [key, classifier] of this.classifiers) {
            stats[key] = {
                patterns: classifier.patterns.length,
                threshold: classifier.threshold
            };
        }
        return stats;
    }
}

/**
 * كاشف الشذوذ - يكتشف الأنماط غير الطبيعية في التدفقات
 */
class AnomalyDetector {
    constructor() {
        this.baseline = new Map();
        this.windowSize = 100;
        this.sensitivity = 2.5; // standard deviations
        this.history = [];
        this.anomalyCount = 0;
    }

    analyze(event, classification) {
        const key = classification.type + '_' + (classification.pattern?.name || 'unknown');
        const value = this.extractValue(event);
        
        // تحديث الخط الأساسي
        this.updateBaseline(key, value);
        
        // حساب الشذوذ
        const stats = this.baseline.get(key);
        if (!stats || stats.count < 10) {
            return { isAnomaly: false, score: 0 };
        }

        const zScore = Math.abs((value - stats.mean) / (stats.std || 1));
        const isAnomaly = zScore > this.sensitivity;
        
        if (isAnomaly) {
            this.anomalyCount++;
        }

        return {
            isAnomaly,
            score: Math.min(1, zScore / (this.sensitivity * 2)),
            zScore,
            expected: stats.mean,
            actual: value
        };
    }

    extractValue(event) {
        return event.bytes || event.duration || event.count || 1;
    }

    updateBaseline(key, value) {
        if (!this.baseline.has(key)) {
            this.baseline.set(key, {
                values: [],
                mean: 0,
                std: 0,
                count: 0
            });
        }

        const stats = this.baseline.get(key);
        stats.values.push(value);
        
        // الحفاظ على حجم النافذة
        if (stats.values.length > this.windowSize) {
            stats.values.shift();
        }

        // إعادة حساب الإحصائيات
        stats.count = stats.values.length;
        stats.mean = stats.values.reduce((a, b) => a + b, 0) / stats.count;
        stats.std = Math.sqrt(
            stats.values.reduce((sum, v) => sum + Math.pow(v - stats.mean, 2), 0) / stats.count
        );
    }

    getRate() {
        return this.history.length > 0 ? this.anomalyCount / this.history.length : 0;
    }
}

/**
 * مدير الحالات الكمية - يتتبع ويدير الحالات الكمية للأحداث
 */
class QuantumStateManager {
    constructor() {
        this.states = new Map();
        this.count = 0;
        this.maxStates = 10000;
    }

    register(id, state) {
        if (this.states.size >= this.maxStates) {
            // إزالة أقدم حالة
            const oldestKey = this.states.keys().next().value;
            this.states.delete(oldestKey);
        }

        this.states.set(id, {
            ...state,
            createdAt: Date.now()
        });
        this.count++;
    }

    get(id) {
        return this.states.get(id);
    }

    collapse(id) {
        const state = this.states.get(id);
        if (state && state.superposition) {
            // انهيار إلى حالة محددة
            const probs = state.probabilities;
            const rand = Math.random();
            let cumulative = 0;
            
            for (const [key, prob] of Object.entries(probs)) {
                cumulative += prob;
                if (rand <= cumulative) {
                    state.collapsedTo = key;
                    state.superposition = false;
                    break;
                }
            }
        }
        return state;
    }

    getEntanglementDensity() {
        let totalEntanglements = 0;
        for (const state of this.states.values()) {
            totalEntanglements += state.entanglement?.length || 0;
        }
        return this.states.size > 0 ? totalEntanglements / this.states.size : 0;
    }

    getStateSummary() {
        let superposed = 0;
        let collapsed = 0;
        let entangled = 0;

        for (const state of this.states.values()) {
            if (state.superposition) superposed++;
            else collapsed++;
            if (state.entanglement?.length > 0) entangled++;
        }

        return { superposed, collapsed, entangled, total: this.states.size };
    }
}

/**
 * نموذج عصبي بسيط - للتعلم من الأنماط
 */
class SimpleNeuralModel {
    constructor(type) {
        this.type = type;
        this.weights = new Map();
        this.learningRate = 0.01;
        this.trainCount = 0;
    }

    train(event, classification) {
        const features = this.extractFeatures(event);
        const target = classification.pattern?.name || 'unknown';

        // تحديث الأوزان (Hebbian learning مبسط)
        for (const [key, value] of Object.entries(features)) {
            const weightKey = `${key}_${target}`;
            const current = this.weights.get(weightKey) || 0;
            this.weights.set(weightKey, current + this.learningRate * value);
        }

        this.trainCount++;
    }

    predict(event) {
        const features = this.extractFeatures(event);
        const scores = new Map();

        for (const [weightKey, weight] of this.weights) {
            const [featureKey, target] = weightKey.split('_');
            const featureValue = features[featureKey] || 0;
            
            if (!scores.has(target)) {
                scores.set(target, 0);
            }
            scores.set(target, scores.get(target) + weight * featureValue);
        }

        // إيجاد أعلى درجة
        let bestTarget = null;
        let bestScore = -Infinity;
        
        for (const [target, score] of scores) {
            if (score > bestScore) {
                bestScore = score;
                bestTarget = target;
            }
        }

        return { prediction: bestTarget, confidence: this.sigmoid(bestScore) };
    }

    extractFeatures(event) {
        return {
            bytes_log: Math.log10((event.bytes || 1) + 1),
            protocol: event.protocol || 0,
            port_high: (event.port || 0) > 1024 ? 1 : 0,
            port_low: (event.port || 0) <= 1024 ? 1 : 0,
            has_src: event.src_ip ? 1 : 0,
            has_dst: event.dst_ip ? 1 : 0
        };
    }

    sigmoid(x) {
        return 1 / (1 + Math.exp(-x));
    }
}

// تصدير للاستخدام العام
window.QuantumDataFlowSystem = QuantumDataFlowSystem;
window.AnomalyDetector = AnomalyDetector;
window.QuantumStateManager = QuantumStateManager;
