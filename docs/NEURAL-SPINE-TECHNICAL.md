# التكامل التقني: نظام التصور النخاعي
## Technical Integration: Neural Spine Visualization

<div dir="rtl">

## 🔌 نقاط التكامل (Integration Points)

### 1. API Endpoint

```csharp
// Controllers/MonitoringController.cs
[HttpGet("events")]
public async Task StreamEventsForVisualization(CancellationToken cancellationToken)
{
    Response.Headers.Append("Content-Type", "text/event-stream");
    Response.Headers.Append("Cache-Control", "no-cache");
    Response.Headers.Append("Connection", "keep-alive");

    await foreach (var evt in _kernelMonitor.StreamEventsAsync(cancellationToken))
    {
        var json = System.Text.Json.JsonSerializer.Serialize(evt);
        await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
```

**URL**: `/api/monitoring/events`  
**Type**: Server-Sent Events (SSE)  
**Format**: JSON

### 2. تنسيق البيانات المتوقع (Data Format)

```json
{
    "ticketId": "00000000-0000-0000-0000-000000000000",
    "processId": 1234,
    "timestamp": "2026-01-01T02:00:00Z",
    "eventType": "network_packet",
    "data": {
        "protocol": 6,        // 6=TCP, 17=UDP, 1=ICMP
        "bytes": 1460,        // حجم الحزمة
        "src_ip": "192.168.1.100",
        "dst_ip": "10.0.0.1",
        "src_port": 443,
        "dst_port": 52341
    }
}
```

### 3. JavaScript API

```javascript
// تهيئة التصور
const viz = new NeuralSpineVisualization('containerId', {
    spineSegments: 60,         // عدد قطع النخاع
    nervesPerSegment: 12,      // عدد الأعصاب لكل قطعة
    maxActiveNerves: 100,      // الحد الأقصى للأعصاب النشطة
    pulseSpeed: 2.5,           // سرعة النبض (1.0 = عادي)
    glowIntensity: 2.0,        // شدة التوهج (1.0 = عادي)
    rotationSpeed: 0.15        // سرعة الدوران (راديان/ثانية)
});

// معالجة حدث
viz.processEbpfData({
    protocol: 6,
    bytes: 1460,
    timestamp: Date.now()
});

// الحصول على الإحصائيات
const stats = viz.getStats();
console.log(stats);
// {
//     total: 1234,
//     active: 15,
//     peakRate: 87,
//     activeNerves: 15,
//     queueSize: 3
// }

// تنظيف
viz.destroy();
```

## 🎨 خوارزمية التصور

### 1. تعيين البيانات للأعصاب (Data-to-Nerve Mapping)

```javascript
mapDataToSegment(data) {
    // TCP → الثلث العلوي (0-30%)
    if (data.protocol === 6) {
        return Math.floor(Math.random() * segments * 0.3);
    }
    
    // UDP → الثلث الأوسط (30-60%)
    if (data.protocol === 17) {
        return Math.floor(segments * 0.3 + Math.random() * segments * 0.3);
    }
    
    // Other → الثلث السفلي (60-100%)
    return Math.floor(segments * 0.6 + Math.random() * segments * 0.4);
}
```

### 2. تنشيط العصب (Nerve Activation)

```javascript
activateNerve(nerve, data) {
    nerve.active = true;
    nerve.intensity = 1.0;              // شدة كاملة
    nerve.pulsePhase = 0;               // بداية النبضة
    nerve.dataType = data.protocol;     // نوع البيانات
    
    // تحديث نشاط القطعة
    segment.activity += 0.3;
    
    // إطفاء تلقائي بعد 1-3 ثواني
    setTimeout(() => deactivateNerve(nerve), 1000 + random(2000));
}
```

### 3. دورة الرسم (Render Loop)

```javascript
animate(time) {
    // تحديث حالة النخاع الشوكي
    spinalCord.segments.forEach(seg => {
        seg.pulsePhase += 0.05;          // تقدم النبض
        seg.activity *= 0.95;             // تلاشي النشاط
    });
    
    // تحديث الأعصاب
    nerves.forEach(nerve => {
        if (nerve.active) {
            nerve.pulsePhase += pulseSpeed * 0.1;
            nerve.intensity *= 0.98;      // تلاشي تدريجي
        }
    });
    
    render();
    requestAnimationFrame(animate);
}
```

## 🏗️ بنية الملفات

```
HRCE/
├── Controllers/
│   └── VisualizationController.cs          # Controller منفصل
│       ├── GET /Visualization/NeuralSpine  # الصفحة الرئيسية
│       └── GET /Visualization/FullScreen   # عرض ملء الشاشة
│
├── Views/
│   └── Visualization/
│       └── NeuralSpine.cshtml              # واجهة التصور
│           ├── HTML Structure
│           ├── CSS Styles (Inline)
│           └── JavaScript Integration
│
└── wwwroot/
    └── js/
        └── neural-spine-viz.js             # محرك التصور
            ├── class NeuralSpineVisualization
            ├── createSpinalCord()
            ├── createNerveEndings()
            ├── processEbpfData()
            ├── animate()
            └── render()
```

## 🔧 التخصيص المتقدم

### تعديل خريطة الألوان

```javascript
// في neural-spine-viz.js، دالة renderNerves()
const colorMap = {
    6: [100, 255, 100],      // TCP → أخضر
    17: [255, 200, 100],     // UDP → برتقالي
    1: [100, 200, 255],      // ICMP → أزرق فاتح
    47: [255, 100, 200],     // GRE → وردي
    default: [200, 100, 255] // أخرى → بنفسجي
};
```

### إضافة مؤثرات بصرية

```javascript
// تأثير "انفجار" عند حزمة كبيرة
if (data.bytes > 1000) {
    createExplosionEffect(nerve.endPos);
}

// خط متصل للاتصالات المستمرة
if (isConnectionPersistent(data)) {
    drawPersistentLine(nerve);
}
```

### تكامل مع مصادر بيانات أخرى

```javascript
// WebSocket بدلاً من SSE
const ws = new WebSocket('wss://localhost:61520/monitoring/ws');
ws.onmessage = (msg) => {
    const data = JSON.parse(msg.data);
    viz.processEbpfData(data);
};

// Polling بدلاً من البث
setInterval(async () => {
    const response = await fetch('/api/monitoring/latest');
    const events = await response.json();
    events.forEach(evt => viz.processEbpfData(evt));
}, 100);
```

## 📊 مقاييس الأداء

### معايير الأداء المستهدفة

| المقياس | القيمة المستهدفة | الحد الأقصى |
|---------|-------------------|-------------|
| FPS | 60 | 30 (مقبول) |
| Event Processing | 100/sec | 200/sec |
| Memory Usage | < 100MB | < 200MB |
| CPU Usage | < 10% | < 25% |
| Latency | < 50ms | < 100ms |

### مراقبة الأداء

```javascript
// في Console
performance.mark('viz-start');
viz.processEbpfData(data);
performance.mark('viz-end');
performance.measure('viz-process', 'viz-start', 'viz-end');

// عرض النتائج
performance.getEntriesByType('measure').forEach(m => {
    console.log(`${m.name}: ${m.duration}ms`);
});
```

## 🔒 الأمان والأداء

### تحديد معدل البيانات (Rate Limiting)

```javascript
class RateLimiter {
    constructor(maxRate = 100) {
        this.maxRate = maxRate;
        this.count = 0;
        this.resetInterval = setInterval(() => this.count = 0, 1000);
    }
    
    allow() {
        if (this.count < this.maxRate) {
            this.count++;
            return true;
        }
        return false;
    }
}

const limiter = new RateLimiter(100);

eventSource.onmessage = (event) => {
    if (limiter.allow()) {
        viz.processEbpfData(JSON.parse(event.data));
    }
};
```

### تنظيف الذاكرة

```javascript
// تنظيف تلقائي للبيانات القديمة
setInterval(() => {
    const now = Date.now();
    dataQueue = dataQueue.filter(d => now - d.timestamp < 60000); // حذف > دقيقة
    
    activeNerves.forEach((timestamp, nerve) => {
        if (now - timestamp > 5000) {  // حذف > 5 ثواني
            activeNerves.delete(nerve);
        }
    });
}, 5000);
```

## 🧪 الاختبار

### اختبار الوحدة (Unit Testing)

```javascript
// test/neural-spine-viz.test.js
describe('NeuralSpineVisualization', () => {
    test('يجب إنشاء العدد الصحيح من الأعصاب', () => {
        const viz = new NeuralSpineVisualization('test', {
            spineSegments: 10,
            nervesPerSegment: 5
        });
        
        expect(viz.nerves.length).toBe(50);
    });
    
    test('يجب معالجة البيانات بشكل صحيح', () => {
        const viz = new NeuralSpineVisualization('test');
        const data = { protocol: 6, bytes: 1000 };
        
        viz.processEbpfData(data);
        
        expect(viz.stats.total).toBe(1);
        expect(viz.stats.active).toBeGreaterThan(0);
    });
});
```

### اختبار الأداء (Performance Testing)

```javascript
// قياس معدل المعالجة
const startTime = Date.now();
const testCount = 10000;

for (let i = 0; i < testCount; i++) {
    viz.processEbpfData({
        protocol: [6, 17, 1][i % 3],
        bytes: Math.random() * 1500
    });
}

const elapsed = Date.now() - startTime;
const rate = testCount / (elapsed / 1000);

console.log(`معدل المعالجة: ${rate.toFixed(2)} حدث/ثانية`);
```

## 📚 مراجع إضافية

- [Canvas 2D API](https://developer.mozilla.org/en-US/docs/Web/API/Canvas_API)
- [Server-Sent Events](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events)
- [eBPF Documentation](https://ebpf.io/what-is-ebpf/)
- [ASP.NET Core SSE](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)

---

**للمطورين: الكود مُعلَّق بالتفصيل ومُهيأ للتوسع والتخصيص**

</div>
