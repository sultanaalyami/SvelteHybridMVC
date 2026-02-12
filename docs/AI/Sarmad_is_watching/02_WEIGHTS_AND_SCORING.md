# 📊 الأوزان وخوارزميات التسجيل - Weights & Scoring

## 1. أوزان البطاقات (`ImportanceWeight`)

### 1.1 جدول الأوزان الكامل

| البطاقة | الفئة | الوزن | المواضيع المرتبطة |
|---------|-------|-------|-------------------|
| `ask-rag` | core | **10** | rag, semantic-kernel, vector-search, context-aware |
| `semantic-kernel` | technology | **10** | orchestration, plugins, agents, cognitive-architecture |
| `archive` | core | 9 | embeddings, semantic-search, rag, indexing |
| `assistants` | core | 9 | agents, tools, specialized-ai, multi-agent |
| `app-builder` | support | 9 | docker, containerization, microservices, deployment |
| `rag-memory` | technology | 9 | vector-db, embeddings, context, memory |
| `multi-agent-system` | advanced | 9 | agents, coordination, distributed-ai, collaboration |
| `run-command` | core | 8 | nlp, conversation, general-ai |
| `code-interpreter` | support | 8 | code-execution, sandbox, ml-training, debugging |
| `signalr-realtime` | technology | 8 | websockets, real-time, streaming, live-updates |
| `ml-intelligence` | technology | 8 | ml-net, reinforcement, error-analysis, auto-improvement |
| `vector-search` | advanced | 8 | embeddings, similarity-search, qdrant, knn |
| `error-intelligence` | advanced | 8 | debugging, ml-analysis, error-patterns, auto-fix |
| `scalability` | integration | 8 | scaling, performance, distributed, cloud |
| `file-explorer` | core | 7 | file-management, project-structure, code-organization |
| `logs-system` | support | 7 | logging, ml-training, monitoring, analytics |
| `session-management` | advanced | 7 | sessions, persistence, state-management, sync |
| `api-endpoints` | integration | 7 | api, rest, integration, webhooks |
| `telemetry` | integration | 7 | monitoring, analytics, insights, performance |
| `marketplace` | integration | 6 | marketplace, monetization, subscriptions |

### 1.2 التوزيع البصري

```
Weight Distribution:
═══════════════════════════════════════════════════════════════════

10 ████████████████████ 2 cards  (ask-rag, semantic-kernel)
 9 █████████████████    5 cards  (archive, assistants, app-builder, rag-memory, multi-agent)
 8 ██████████████       7 cards  (run-command, code-interpreter, signalr, ml-intelligence, 
                                  vector-search, error-intelligence, scalability)
 7 ████████████         5 cards  (file-explorer, logs-system, session-management, 
                                  api-endpoints, telemetry)
 6 ██████████           1 card   (marketplace)

═══════════════════════════════════════════════════════════════════
```

---

## 2. أوزان الفئات (`Categories`)

### 2.1 إحصائيات الفئات

| الفئة | عدد البطاقات | مجموع الأوزان | المتوسط |
|-------|--------------|---------------|---------|
| `core` | 5 | 43 | 8.6 |
| `support` | 3 | 24 | 8.0 |
| `technology` | 4 | 35 | 8.75 |
| `advanced` | 4 | 32 | 8.0 |
| `integration` | 4 | 28 | 7.0 |

### 2.2 كيف تتراكم نقاط الفئات

```csharp
// عند توسيع بطاقة
if (interaction.Expanded)
{
    var card = GetCard(interaction.CardId);
    profile.CategoryInterest[card.Category] += card.ImportanceWeight;
}
```

**مثال**:
```
المستخدم يوسع 3 بطاقات:
1. ask-rag (core, weight=10)      → CategoryInterest["core"] = 10
2. archive (core, weight=9)       → CategoryInterest["core"] = 19
3. signalr (technology, weight=8) → CategoryInterest["technology"] = 8

النتيجة:
{
    "core": 19,
    "technology": 8
}
```

---

## 3. خوارزمية حساب الثقة (`CalculateConfidenceLevel`)

### 3.1 المعادلة

```
ConfidenceLevel = min(100, 
    min(ViewedCards × 4, 25) +
    min(ExpandedCards × 13, 40) +
    min(CategoryCount × 7, 20) +
    min(InterestCount × 3, 15)
)
```

### 3.2 جدول النقاط

| المؤشر | النقاط/وحدة | الحد الأقصى | الوزن النسبي |
|--------|-------------|-------------|--------------|
| البطاقات المشاهدة | 4 | 25 | 25% |
| البطاقات الموسعة | 13 | 40 | **40%** |
| تنوع الفئات | 7 | 20 | 20% |
| الاهتمامات المستنتجة | 3 | 15 | 15% |

### 3.3 أمثلة حساب

```
═══════════════════════════════════════════════════════════════════
السيناريو 1: مستخدم جديد
───────────────────────────────────────────────────────────────────
ViewedCards: 3, ExpandedCards: 1, Categories: 1, Interests: 3

الحساب:
- مشاهدات: 3 × 4 = 12 (< 25) → 12
- توسعات:  1 × 13 = 13 (< 40) → 13
- فئات:    1 × 7 = 7 (< 20) → 7
- اهتمامات: 3 × 3 = 9 (< 15) → 9

المجموع: 12 + 13 + 7 + 9 = 41%
═══════════════════════════════════════════════════════════════════

السيناريو 2: مستخدم نشط
───────────────────────────────────────────────────────────────────
ViewedCards: 6, ExpandedCards: 3, Categories: 3, Interests: 8

الحساب:
- مشاهدات: 6 × 4 = 24 (< 25) → 24
- توسعات:  3 × 13 = 39 (< 40) → 39
- فئات:    3 × 7 = 21 (> 20) → 20
- اهتمامات: 8 × 3 = 24 (> 15) → 15

المجموع: 24 + 39 + 20 + 15 = 98%
═══════════════════════════════════════════════════════════════════

السيناريو 3: متصفح سطحي
───────────────────────────────────────────────────────────────────
ViewedCards: 10, ExpandedCards: 0, Categories: 1, Interests: 0

الحساب:
- مشاهدات: 10 × 4 = 40 (> 25) → 25
- توسعات:  0 × 13 = 0 → 0
- فئات:    1 × 7 = 7 → 7
- اهتمامات: 0 × 3 = 0 → 0

المجموع: 25 + 0 + 7 + 0 = 32%
═══════════════════════════════════════════════════════════════════
```

---

## 4. خوارزمية تحديد الـ Persona (`DeterminePersona`)

### 4.1 مصفوفة النقاط الكاملة

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                        PERSONA SCORING MATRIX                                │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  TechnicalExplorer (الحد الأقصى: 100)                                       │
│  ─────────────────────────────────────────                                   │
│  ├── اهتمامات [rag, embeddings, semantic-search]     → +35                  │
│  ├── اهتمامات [code-execution, ml-training]          → +25                  │
│  ├── اهتمامات [semantic-kernel, vector-search]       → +20                  │
│  └── فئة "technology" ضمن أعلى 3                      → +20                  │
│                                                                              │
│  BusinessDecisionMaker (الحد الأقصى: 100)                                   │
│  ─────────────────────────────────────────                                   │
│  ├── اهتمامات [marketplace, monetization]            → +35                  │
│  ├── اهتمامات [scaling, performance]                 → +25                  │
│  ├── Pace == VeryFast                                 → +20                  │
│  ├── Pace == Fast                                     → +10                  │
│  └── ExpandedCards بين 2-5 (انتقائي)                  → +20                  │
│                                                                              │
│  Learner (الحد الأقصى: 90)                                                  │
│  ─────────────────────────────────────────                                   │
│  ├── Pace == Slow أو VeryDeep                         → +30                  │
│  ├── ExpandedCards >= 5                               → +25                  │
│  ├── فئة "core" ضمن أعلى 3                            → +20                  │
│  └── اهتمامات [nlp, conversation, general-ai]        → +15                  │
│                                                                              │
│  Integrator (الحد الأقصى: 90)                                               │
│  ─────────────────────────────────────────                                   │
│  ├── اهتمامات [api, rest, integration]               → +40 ⭐ الأهم         │
│  ├── اهتمامات [docker, deployment, containerization] → +30                  │
│  └── فئة "integration" ضمن أعلى 3                     → +20                  │
│                                                                              │
│  Researcher (الحد الأقصى: 100)                                              │
│  ─────────────────────────────────────────                                   │
│  ├── ExpandedCards >= 8                               → +35                  │
│  ├── Pace == VeryDeep                                 → +30                  │
│  ├── InferredInterests.Count >= 10                    → +20                  │
│  └── فئة "advanced" ضمن أعلى 3                        → +15                  │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘

Confidence = (BestScore × 100) / 100
عتبة التحديد: >= 25%
```

### 4.2 مثال حساب Persona

```
═══════════════════════════════════════════════════════════════════
المستخدم:
- InferredInterests: [rag, embeddings, semantic-kernel, api]
- Pace: Moderate
- ExpandedCards: 4
- TopCategories: [{technology: 27}, {core: 19}]
───────────────────────────────────────────────────────────────────

حساب النقاط:

TechnicalExplorer:
  + 35 (rag, embeddings)
  + 20 (semantic-kernel)
  + 20 (technology in top)
  = 75 ✅

BusinessDecisionMaker:
  + 0  (no marketplace/monetization)
  + 0  (Pace != Fast/VeryFast)
  + 20 (2 <= 4 <= 5)
  = 20

Learner:
  + 0  (Pace != Slow/VeryDeep)
  + 0  (ExpandedCards < 5)
  + 0  (core not in top)
  = 0

Integrator:
  + 40 (api interest)
  + 0  (no docker/deployment)
  + 0  (integration not in top)
  = 40

Researcher:
  + 0  (ExpandedCards < 8)
  + 0  (Pace != VeryDeep)
  + 0  (interests < 10)
  = 0

النتيجة:
- Persona: TechnicalExplorer
- PersonaConfidence: 75%
═══════════════════════════════════════════════════════════════════
```

---

## 5. خوارزمية التوصية (`GetPersonalizedCapabilities`)

### 5.1 المعادلة

```
Score = (CategoryInterest[card.Category] × 2)
      + (MatchingTopics × 10)
      + ImportanceWeight
```

### 5.2 مثال حساب

```
═══════════════════════════════════════════════════════════════════
المستخدم:
- CategoryInterest: { "technology": 18, "core": 10 }
- InferredInterests: ["rag", "embeddings", "vector-search"]
- ViewedCards: ["archive", "ask-rag"]
───────────────────────────────────────────────────────────────────

حساب نقاط البطاقات غير المشاهدة:

1. rag-memory (technology)
   - Category: 18 × 2 = 36
   - Topics match: ["embeddings"] = 1 × 10 = 10
   - Weight: 9
   - Total: 55 ✅

2. vector-search (advanced)
   - Category: 0 × 2 = 0
   - Topics match: ["embeddings"] = 1 × 10 = 10
   - Weight: 8
   - Total: 18

3. signalr-realtime (technology)
   - Category: 18 × 2 = 36
   - Topics match: [] = 0 × 10 = 0
   - Weight: 8
   - Total: 44

4. semantic-kernel (technology)
   - Category: 18 × 2 = 36
   - Topics match: [] = 0 × 10 = 0
   - Weight: 10
   - Total: 46

الترتيب النهائي:
1. rag-memory: 55
2. semantic-kernel: 46
3. signalr-realtime: 44
...
═══════════════════════════════════════════════════════════════════
```

---

## 6. تصنيف سرعة التفاعل (`InteractionPace`)

```csharp
profile.Pace = avgSeconds switch
{
    < 3   => InteractionPace.VeryFast,   // 🏃 قرارات سريعة
    < 10  => InteractionPace.Fast,       // 🚶 نشط
    < 30  => InteractionPace.Moderate,   // 👤 عادي
    < 60  => InteractionPace.Slow,       // 🔍 متأمل
    _     => InteractionPace.VeryDeep    // 📖 قارئ متعمق
};
```

| السرعة | الفترة | الدلالة |
|--------|--------|---------|
| `VeryFast` | < 3s | صاحب قرار، يبحث عن شيء محدد |
| `Fast` | 3-10s | نشط، يعرف ما يريد |
| `Moderate` | 10-30s | مستخدم عادي، يستكشف |
| `Slow` | 30-60s | متعلم، يقرأ بعناية |
| `VeryDeep` | > 60s | باحث، يقرأ كل التفاصيل |
