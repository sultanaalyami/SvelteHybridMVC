# 🌐 مرجع واجهات API - API Reference

## 1. نقاط النهاية (Endpoints)

### 1.1 ملخص

| Method | Endpoint | الوصف |
|--------|----------|-------|
| `GET` | `/api/capabilities/all` | جميع البطاقات |
| `GET` | `/api/capabilities/initial` | البطاقات الأولية (8 من core) |
| `GET` | `/api/capabilities/more` | البطاقات الإضافية |
| `GET` | `/api/capabilities/personalized/{sessionId}` | التوصيات المخصصة |
| `GET` | `/api/capabilities/{id}` | بطاقة واحدة |
| `POST` | `/api/capabilities/track` | تسجيل تفاعل |
| `GET` | `/api/capabilities/{id}/related` | البطاقات ذات الصلة |

---

## 2. تفاصيل كل Endpoint

### 2.1 جلب جميع البطاقات

```http
GET /api/capabilities/all
```

**Response**: `200 OK`

```json
[
    {
        "id": "run-command",
        "title": "Run Command - المحادثة العامة",
        "excerpt": "نموذج دردشة ذكي يجيب على أسئلتك بلغة طبيعية",
        "fullContent": "<h4>محرك المحادثة الأساسي</h4>...",
        "icon": "fas fa-comments",
        "category": "core",
        "displayOrder": 1
    },
    // ... 19 more cards
]
```

---

### 2.2 البطاقات الأولية

```http
GET /api/capabilities/initial
```

**Response**: `200 OK`

```json
[
    // أول 8 بطاقات من فئة core
    {
        "id": "run-command",
        "title": "Run Command - المحادثة العامة",
        "excerpt": "نموذج دردشة ذكي يجيب على أسئلتك بلغة طبيعية",
        "icon": "fas fa-comments",
        "category": "core"
    },
    // ... 7 more
]
```

---

### 2.3 التوصيات المخصصة ⭐

```http
GET /api/capabilities/personalized/{sessionId}
```

**Parameters**:
- `sessionId` (path) - معرف الجلسة (UUID)

**Response**: `200 OK`

**حالة 1: لا يوجد profile أو فارغ**
```json
[
    // البطاقات الأساسية (core)
    {
        "id": "run-command",
        "title": "Run Command - المحادثة العامة",
        "excerpt": "...",
        "icon": "fas fa-comments",
        "category": "core"
    }
]
```

**حالة 2: يوجد profile مع بيانات**
```json
[
    // مرتبة حسب Score تنازلياً
    {
        "id": "rag-memory",           // Score: 55
        "title": "RAG Memory System",
        "excerpt": "ذاكرة طويلة وقصيرة للسياق الكامل",
        "icon": "fas fa-database",
        "category": "technology"
    },
    {
        "id": "semantic-kernel",      // Score: 46
        "title": "Semantic Kernel",
        "excerpt": "ما يجعل سرمد ذكياً حقاً",
        "icon": "fas fa-atom",
        "category": "technology"
    }
    // ... 6 more
]
```

---

### 2.4 تسجيل تفاعل ⭐

```http
POST /api/capabilities/track
Content-Type: application/json
```

**Request Body**:
```json
{
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "cardId": "ask-rag",
    "expanded": true
}
```

**Response**: `200 OK`

```json
{
    "success": true,
    "profile": {
        "confidenceLevel": 65,
        "identifiedPersona": "TechnicalExplorer",
        "personaConfidence": 55,
        "needsAdvancedTracking": false,
        "pace": "Moderate",
        "viewedCards": 6,
        "expandedCards": 3
    }
}
```

**Response Schema**:

| الحقل | النوع | الوصف |
|-------|-------|-------|
| `success` | boolean | حالة العملية |
| `profile.confidenceLevel` | int (0-100) | مستوى الثقة في فهم المستخدم |
| `profile.identifiedPersona` | string | نوع المستخدم المستنتج |
| `profile.personaConfidence` | int (0-100) | ثقة تحديد النوع |
| `profile.needsAdvancedTracking` | boolean | هل نحتاج تتبع متقدم؟ |
| `profile.pace` | string | سرعة التفاعل |
| `profile.viewedCards` | int | عدد البطاقات المشاهدة |
| `profile.expandedCards` | int | عدد البطاقات الموسعة |

---

### 2.5 بطاقة واحدة

```http
GET /api/capabilities/{id}
```

**Parameters**:
- `id` (path) - معرف البطاقة

**Response**: `200 OK`

```json
{
    "id": "ask-rag",
    "title": "Ask (RAG) - الاستدلال المعزز",
    "shortDescription": "أقوى قدرات سرمد",
    "iconClass": "fas fa-brain",
    "category": "core",
    "displayOrder": 4,
    "importanceWeight": 10,
    "expandedContent": "<h4>القدرة الأقوى...</h4>",
    "keyFeatures": [
        "Retrieval-Augmented Generation",
        "ذاكرة دلالية متقدمة"
    ],
    "useCases": [
        "شرح كيف يعمل كود معين",
        "إيجاد أمثلة من مشاريعك"
    ],
    "relatedTopics": [
        "rag",
        "semantic-kernel",
        "vector-search"
    ]
}
```

**Response**: `404 Not Found`
```json
{
    "error": "Card not found"
}
```

---

### 2.6 البطاقات ذات الصلة

```http
GET /api/capabilities/{id}/related
```

**Parameters**:
- `id` (path) - معرف البطاقة

**Response**: `200 OK`

```json
[
    {
        "id": "archive",
        "title": "Archive - الأرشفة الدلالية",
        "excerpt": "فهرسة ذكية لمحتويات مشاريعك",
        "icon": "fas fa-archive",
        "category": "core"
    },
    {
        "id": "rag-memory",
        "title": "RAG Memory System",
        "excerpt": "ذاكرة طويلة وقصيرة للسياق الكامل",
        "icon": "fas fa-database",
        "category": "technology"
    },
    {
        "id": "vector-search",
        "title": "Vector Search",
        "excerpt": "بحث دلالي فائق السرعة",
        "icon": "fas fa-search-plus",
        "category": "advanced"
    }
]
```

---

## 3. نماذج البيانات (Models)

### 3.1 CardInteractionRequest

```csharp
public class CardInteractionRequest
{
    public required string SessionId { get; set; }
    public required string CardId { get; set; }
    public bool Expanded { get; set; }
}
```

### 3.2 CapabilityCard (مختصر)

```csharp
public class CapabilityCard
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string ShortDescription { get; set; }
    public required string IconClass { get; set; }
    public required string Category { get; set; }
    public int DisplayOrder { get; set; }
    public int ImportanceWeight { get; set; }
    public required string ExpandedContent { get; set; }
    public List<string> KeyFeatures { get; set; }
    public List<string> UseCases { get; set; }
    public List<string> RelatedTopics { get; set; }
}
```

### 3.3 UserPersona

```csharp
public enum UserPersona
{
    Unknown,              // لم يتم التحديد
    TechnicalExplorer,    // مطور تقني
    BusinessDecisionMaker,// صاحب قرار
    Learner,              // متعلم
    Integrator,           // مهندس تكامل
    Researcher            // باحث
}
```

### 3.4 InteractionPace

```csharp
public enum InteractionPace
{
    Unknown,    // غير محدد
    VeryFast,   // < 3 ثواني
    Fast,       // 3-10 ثواني
    Moderate,   // 10-30 ثانية
    Slow,       // 30-60 ثانية
    VeryDeep    // > 60 ثانية
}
```

---

## 4. أمثلة استخدام

### 4.1 cURL

```bash
# 1. تسجيل تفاعل
curl -X POST http://localhost:5000/api/capabilities/track \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "test-123",
    "cardId": "ask-rag",
    "expanded": true
  }'

# 2. جلب توصيات
curl http://localhost:5000/api/capabilities/personalized/test-123

# 3. جلب بطاقات ذات صلة
curl http://localhost:5000/api/capabilities/ask-rag/related
```

### 4.2 JavaScript (Fetch)

```javascript
// تسجيل تفاعل
async function trackCardInteraction(sessionId, cardId, expanded) {
    const response = await fetch('/api/capabilities/track', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sessionId, cardId, expanded })
    });
    return response.json();
}

// جلب توصيات
async function getPersonalizedCards(sessionId) {
    const response = await fetch(`/api/capabilities/personalized/${sessionId}`);
    return response.json();
}

// مثال استخدام
const sessionId = crypto.randomUUID();

// المستخدم يوسع بطاقة
const result = await trackCardInteraction(sessionId, 'ask-rag', true);
console.log('Profile:', result.profile);
// { confidenceLevel: 41, identifiedPersona: "TechnicalExplorer", ... }

// جلب توصيات
const recommendations = await getPersonalizedCards(sessionId);
console.log('Recommendations:', recommendations.map(c => c.id));
// ["rag-memory", "semantic-kernel", "archive", ...]
```

### 4.3 C# (HttpClient)

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// تسجيل تفاعل
var interaction = new { SessionId = "test-123", CardId = "ask-rag", Expanded = true };
var response = await client.PostAsJsonAsync("/api/capabilities/track", interaction);
var result = await response.Content.ReadFromJsonAsync<TrackResponse>();

// جلب توصيات
var cards = await client.GetFromJsonAsync<List<CapabilityCard>>(
    $"/api/capabilities/personalized/{interaction.SessionId}");
```

---

## 5. رموز الحالة (Status Codes)

| الرمز | الوصف | متى يُرجع |
|-------|-------|-----------|
| `200 OK` | نجاح | العمليات الناجحة |
| `404 Not Found` | غير موجود | البطاقة غير موجودة |
| `400 Bad Request` | طلب خاطئ | بيانات ناقصة أو غير صالحة |
| `500 Internal Error` | خطأ داخلي | استثناء غير متوقع |
