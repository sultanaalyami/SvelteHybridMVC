# 🔍 SARMAD is Watching - نظام التوصية الذكي

> **"سرمد يراقبك... ليفهمك أفضل"**

نظام توصية ذكي يعتمد على تحليل سلوك المستخدم في الوقت الفعلي لتقديم محتوى مخصص.

---

## 📁 هيكل التوثيق

| الملف | الوصف |
|-------|-------|
| [01_ARCHITECTURE.md](./01_ARCHITECTURE.md) | البنية العامة ودورة حياة البيانات |
| [02_WEIGHTS_AND_SCORING.md](./02_WEIGHTS_AND_SCORING.md) | أوزان الفئات والمواضيع وخوارزميات التسجيل |
| [03_TRACKING_LIFECYCLE.md](./03_TRACKING_LIFECYCLE.md) | دورة حياة التتبع والتحليل |
| [04_API_REFERENCE.md](./04_API_REFERENCE.md) | مرجع واجهات API |
| [05_USE_CASES.md](./05_USE_CASES.md) | حالات الاستخدام المدعومة والتي تتطلب تعديل |
| [06_CODE_SNIPPETS.md](./06_CODE_SNIPPETS.md) | مقتطفات الكود الأساسية |

---

## 🎯 نظرة عامة سريعة

### ما الذي يراقبه سرمد؟

```
┌─────────────────────────────────────────────────────────────────┐
│                    SARMAD Tracking Signals                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  📊 Primary Signals (Always Tracked)                           │
│  ├── Card Views (which cards user sees)                        │
│  ├── Card Expansions (which cards user reads fully)            │
│  ├── Time Between Actions (interaction pace)                   │
│  └── Session Duration                                          │
│                                                                 │
│  🔬 Secondary Signals (When Confidence < 50%)                  │
│  ├── Mouse Movements (hover patterns)                          │
│  ├── Scroll Depth                                              │
│  ├── Focus Areas (IntersectionObserver)                        │
│  └── Click Patterns                                            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### كيف يستنتج سرمد اهتماماتك؟

```
User Expands "Ask (RAG)" Card
         │
         ▼
┌─────────────────────────────────────┐
│ Card Properties:                    │
│ - Category: "core"                  │
│ - ImportanceWeight: 10              │
│ - RelatedTopics: [rag, semantic-    │
│   kernel, vector-search, context]   │
└─────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│ Profile Updates:                    │
│ - CategoryInterest["core"] += 10    │
│ - InferredInterests += topics       │
│ - EngagementScore += 10             │
└─────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│ Analysis (every 2 views):           │
│ - ConfidenceLevel = 53%             │
│ - Persona = TechnicalExplorer       │
│ - PersonaConfidence = 55%           │
└─────────────────────────────────────┘
```

---

## 🚀 البدء السريع

### 1. تتبع تفاعل مستخدم

```http
POST /api/capabilities/track
Content-Type: application/json

{
    "sessionId": "user-session-uuid",
    "cardId": "ask-rag",
    "expanded": true
}
```

### 2. جلب توصيات مخصصة

```http
GET /api/capabilities/personalized/user-session-uuid
```

### 3. Response

```json
[
    {
        "id": "rag-memory",
        "title": "RAG Memory System",
        "category": "technology",
        "displayOrder": 11
    },
    // ... 7 more personalized cards
]
```

---

## 📈 مؤشرات الأداء

| المؤشر | القيمة المثلى | الوصف |
|--------|---------------|-------|
| Time to First Persona | < 4 interactions | الوقت لتحديد نوع المستخدم |
| Confidence @ 6 cards | > 60% | مستوى الثقة بعد 6 بطاقات |
| Recommendation Accuracy | > 75% | دقة التوصيات (based on subsequent interactions) |

---

## 🔗 الملفات المصدرية الأساسية

```
MCPServerMvc/
├── Services/
│   └── CapabilityCardsService.cs      # 🎯 محرك التوصية الرئيسي
├── Controllers/
│   └── CapabilitiesController.cs      # 🌐 API endpoints
├── Models/
│   └── CapabilityCard.cs              # 📦 نماذج البيانات
└── Pages/
    └── Index.cshtml                   # 🖥️ Frontend tracking code
```

---

## ⚠️ ملاحظات مهمة

1. **الخصوصية**: جميع البيانات مرتبطة بـ `SessionId` فقط، لا يوجد تتبع للهوية الحقيقية
2. **التخزين**: البيانات في الذاكرة فقط (`Dictionary<string, UserInterestProfile>`)
3. **الحدود**: لا يوجد persistence عبر إعادة تشغيل الخادم

---

## 📅 آخر تحديث

- **التاريخ**: 2026
- **الإصدار**: 1.0
- **المؤلف**: SARMAD Development Team
