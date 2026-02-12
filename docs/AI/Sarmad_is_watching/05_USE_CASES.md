# ✅❌ حالات الاستخدام - Use Cases

## 1. نظرة عامة

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         USE CASES CLASSIFICATION                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ✅ SUPPORTED                    ⚠️ LIMITED                ❌ UNSUPPORTED   │
│  ───────────────                 ──────────               ──────────────    │
│  • Card recommendations          • Multi-device           • User auth       │
│  • Persona detection             • Long-term memory       • Persistence     │
│  • Interest inference            • A/B testing            • ML training     │
│  • Pace analysis                 • External data          • Cross-session   │
│  • Category scoring              • Real-time collab       • Analytics       │
│  • Related content               • Offline mode           • Export/Import   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. ✅ حالات الاستخدام المدعومة بالكامل

### 2.1 توصية البطاقات المخصصة

**السيناريو**: عرض محتوى مخصص بناءً على اهتمامات المستخدم

**كيف يعمل**:
```
1. المستخدم يتصفح البطاقات
2. النظام يسجل المشاهدات والتوسعات
3. يحسب نقاط لكل بطاقة غير مشاهدة
4. يعرض أعلى 8 بطاقات
```

**الكود المسؤول**:
```csharp
// CapabilitiesController.cs
[HttpGet("personalized/{sessionId}")]
public IActionResult GetPersonalizedCapabilities(string sessionId)
```

**الحدود**:
- ✅ يعمل بشكل ممتاز للـ 20 بطاقة الحالية
- ✅ سريع (O(n) حيث n = عدد البطاقات)
- ✅ لا يحتاج قاعدة بيانات

---

### 2.2 تحديد نوع المستخدم (Persona Detection)

**السيناريو**: معرفة ما إذا كان المستخدم مطور، صاحب قرار، متعلم، إلخ

**كيف يعمل**:
```
1. تحليل المواضيع المهتم بها
2. تحليل سرعة التفاعل
3. تحليل عدد وعمق التوسعات
4. حساب نقاط لكل persona
5. اختيار الأعلى
```

**الكود المسؤول**:
```csharp
// CapabilityCardsService.cs
private (UserPersona persona, int confidence) DeterminePersona(UserInterestProfile profile)
```

**الحدود**:
- ✅ 5 أنواع مدعومة
- ✅ ثقة محسوبة (0-100%)
- ✅ يتحسن مع المزيد من البيانات

---

### 2.3 استنتاج الاهتمامات

**السيناريو**: معرفة ما يهتم به المستخدم دون سؤاله مباشرة

**كيف يعمل**:
```
1. المستخدم يوسع بطاقة "Ask (RAG)"
2. البطاقة تحتوي على: RelatedTopics = ["rag", "semantic-kernel", "vector-search"]
3. تُضاف هذه المواضيع إلى InferredInterests
4. تُستخدم لاحقاً في التوصية والـ persona
```

**الكود المسؤول**:
```csharp
// CapabilityCardsService.cs - TrackInteraction()
foreach (var topic in card.RelatedTopics)
{
    if (!profile.InferredInterests.Contains(topic))
        profile.InferredInterests.Add(topic);
}
```

**الحدود**:
- ✅ تراكمي (يُضاف ولا يُزال)
- ✅ يُستخدم في التوصية والـ persona

---

### 2.4 تحليل سرعة التفاعل

**السيناريو**: فهم أسلوب المستخدم (متسرع؟ متأمل؟ باحث؟)

**كيف يعمل**:
```
1. تسجيل آخر 10 تفاعلات في Queue
2. حساب الفرق الزمني بين كل تفاعلين
3. حساب المتوسط
4. تصنيف: VeryFast / Fast / Moderate / Slow / VeryDeep
```

**الكود المسؤول**:
```csharp
// CapabilityCardsService.cs
private void UpdateInteractionPace(UserInterestProfile profile)
```

**الحدود**:
- ✅ يتجاهل الفترات > 5 دقائق (المستخدم غادر وعاد)
- ✅ يتحدث باستمرار

---

### 2.5 البطاقات ذات الصلة

**السيناريو**: "قد يعجبك أيضاً" بعد قراءة بطاقة

**كيف يعمل**:
```
1. جلب RelatedTopics للبطاقة الحالية
2. البحث عن بطاقات تشترك في مواضيع
3. ترتيب حسب عدد المواضيع المشتركة
4. إرجاع أعلى 3
```

**الكود المسؤول**:
```csharp
// CapabilityCardsService.cs
public List<CapabilityCard> GetRelatedCapabilities(string cardId)
```

---

## 3. ⚠️ حالات استخدام محدودة (تحتاج تعديل بسيط)

### 3.1 دعم الأجهزة المتعددة

**المشكلة**: الـ Profile مرتبط بـ `sessionId` الذي يتغير مع كل جلسة متصفح

**الحل المقترح**:
```csharp
// إضافة userId اختياري
public class UserInterestProfile
{
    public string? UserId { get; set; }  // 🆕 للمستخدمين المسجلين
    public required string SessionId { get; set; }
}

// دمج profiles عند تسجيل الدخول
public void MergeProfiles(string userId, string sessionId)
{
    var sessionProfile = _userProfiles.GetValueOrDefault(sessionId);
    var userProfile = _userProfiles.Values.FirstOrDefault(p => p.UserId == userId);
    
    if (sessionProfile != null && userProfile != null)
    {
        // دمج البيانات
        userProfile.ViewedCards.AddRange(sessionProfile.ViewedCards.Except(userProfile.ViewedCards));
        // ... merge other fields
    }
}
```

**التعديل المطلوب**: ~50 سطر كود

---

### 3.2 الذاكرة طويلة المدى

**المشكلة**: البيانات تُفقد عند إعادة تشغيل الخادم

**الحل المقترح**:
```csharp
// إضافة persistence layer
public interface IProfileStorage
{
    Task SaveProfileAsync(UserInterestProfile profile);
    Task<UserInterestProfile?> LoadProfileAsync(string sessionId);
}

// تنفيذ بسيط باستخدام JSON files
public class FileProfileStorage : IProfileStorage
{
    private readonly string _storagePath = "profiles/";
    
    public async Task SaveProfileAsync(UserInterestProfile profile)
    {
        var json = JsonSerializer.Serialize(profile);
        await File.WriteAllTextAsync(
            Path.Combine(_storagePath, $"{profile.SessionId}.json"), 
            json);
    }
}
```

**التعديل المطلوب**: ~100 سطر كود + تعديل في `TrackInteraction`

---

### 3.3 A/B Testing للتوصيات

**المشكلة**: لا يوجد آلية لمقارنة خوارزميات مختلفة

**الحل المقترح**:
```csharp
public interface IRecommendationStrategy
{
    List<CapabilityCard> GetRecommendations(UserInterestProfile profile, List<CapabilityCard> allCards);
}

public class ABTestingService
{
    private readonly Dictionary<string, IRecommendationStrategy> _strategies;
    
    public IRecommendationStrategy GetStrategyForSession(string sessionId)
    {
        // 50% للخوارزمية A, 50% للخوارزمية B
        var hash = sessionId.GetHashCode();
        return hash % 2 == 0 ? _strategies["A"] : _strategies["B"];
    }
}
```

**التعديل المطلوب**: ~150 سطر كود

---

## 4. ❌ حالات استخدام غير مدعومة (تحتاج تعديل جذري)

### 4.1 مصادقة المستخدمين

**لماذا غير مدعوم**:
- النظام الحالي يعتمد على `sessionId` مجهول
- لا يوجد ربط مع هوية حقيقية
- لا يوجد تشفير أو أمان

**ما يتطلبه الدعم**:
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ التعديلات المطلوبة:                                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│ 1. إضافة ASP.NET Identity أو خدمة مصادقة خارجية                             │
│ 2. تعديل جميع الـ API endpoints لتتطلب [Authorize]                          │
│ 3. ربط Profile بـ UserId بدلاً من SessionId                                 │
│ 4. إضافة GDPR compliance (حذف البيانات، تصدير، إلخ)                         │
│ 5. تعديل الـ Frontend للتعامل مع tokens                                      │
│                                                                             │
│ الجهد المقدر: 500+ سطر كود، 2-3 أيام عمل                                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

### 4.2 Persistence دائم (قاعدة بيانات)

**لماذا غير مدعوم**:
- التخزين الحالي في `Dictionary` في الذاكرة فقط
- يُفقد عند restart
- لا يدعم scale-out (multiple server instances)

**ما يتطلبه الدعم**:
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ الخيارات:                                                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│ الخيار 1: SQL Database                                                      │
│   - إضافة Entity Framework Core                                             │
│   - إنشاء tables: UserProfiles, CardInteractions, InferredInterests        │
│   - تعديل CapabilityCardsService للقراءة/الكتابة من DB                      │
│   - الجهد: 400+ سطر، 2 أيام                                                 │
│                                                                             │
│ الخيار 2: Redis Cache                                                       │
│   - إضافة IDistributedCache                                                 │
│   - تخزين profiles كـ JSON                                                  │
│   - أسرع لكن بدون query capabilities                                        │
│   - الجهد: 200 سطر، 1 يوم                                                   │
│                                                                             │
│ الخيار 3: Qdrant (Vector DB) - موجود بالفعل في المشروع!                     │
│   - استخدام RagMemoryService لتخزين profiles                               │
│   - ميزة: يمكن البحث الدلالي في اهتمامات المستخدمين                          │
│   - الجهد: 150 سطر، 1 يوم                                                   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

### 4.3 تدريب نماذج ML من البيانات

**لماذا غير مدعوم**:
- الخوارزميات الحالية rule-based (قواعد ثابتة)
- لا يوجد جمع بيانات للتدريب
- لا يوجد feedback loop

**ما يتطلبه الدعم**:
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ المتطلبات:                                                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│ 1. جمع البيانات:                                                            │
│    - تسجيل كل interaction مع timestamp                                      │
│    - تسجيل ما إذا نقر المستخدم على التوصية أم لا (feedback)                 │
│    - تخزين في قاعدة بيانات                                                  │
│                                                                             │
│ 2. Pipeline التدريب:                                                        │
│    - إعداد بيانات التدريب (features + labels)                               │
│    - اختيار نموذج (Collaborative Filtering, Content-Based, Hybrid)         │
│    - تدريب باستخدام ML.NET أو خدمة خارجية                                   │
│                                                                             │
│ 3. الاستدلال:                                                               │
│    - استبدال الخوارزمية الحالية بنموذج ML                                   │
│    - A/B testing بين القديم والجديد                                        │
│                                                                             │
│ الجهد: 1000+ سطر، أسبوع+ من العمل                                           │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

### 4.4 التتبع عبر الجلسات

**لماذا غير مدعوم**:
- كل جلسة لها `sessionId` جديد
- لا يوجد ربط بين الجلسات
- لا يوجد fingerprinting

**ما يتطلبه الدعم**:
```
خياران:

1. User Authentication (الأفضل):
   - المستخدم يسجل دخول
   - جميع جلساته مرتبطة بـ UserId

2. Device Fingerprinting (أقل موثوقية):
   - جمع معلومات الجهاز (user agent, screen, fonts, etc.)
   - إنشاء hash فريد
   - ربط الجلسات بنفس الـ fingerprint
   
   ⚠️ تحذير: قد يخالف GDPR/قوانين الخصوصية
```

---

### 4.5 تحليلات وتقارير (Analytics Dashboard)

**لماذا غير مدعوم**:
- لا يوجد تجميع للإحصائيات
- لا يوجد dashboard
- لا يوجد export

**ما يتطلبه الدعم**:
```csharp
// خدمة إحصائيات جديدة
public class AnalyticsService
{
    public Task<DashboardStats> GetGlobalStatsAsync()
    {
        return new DashboardStats
        {
            TotalSessions = _profiles.Count,
            AverageConfidence = _profiles.Values.Average(p => p.ConfidenceLevel),
            PersonaDistribution = _profiles.Values
                .GroupBy(p => p.IdentifiedPersona)
                .ToDictionary(g => g.Key, g => g.Count()),
            TopInterests = GetTopInterests(),
            MostViewedCards = GetMostViewedCards(),
            ConversionFunnel = GetFunnelData()
        };
    }
}

// صفحة Razor للعرض
// Pages/Analytics/Dashboard.cshtml
```

**الجهد**: ~300 سطر كود + صفحة UI

---

## 5. ملخص التوافق

| حالة الاستخدام | الحالة | الجهد للدعم |
|----------------|--------|-------------|
| توصية البطاقات | ✅ مدعوم | - |
| تحديد Persona | ✅ مدعوم | - |
| استنتاج الاهتمامات | ✅ مدعوم | - |
| تحليل سرعة التفاعل | ✅ مدعوم | - |
| البطاقات ذات الصلة | ✅ مدعوم | - |
| أجهزة متعددة | ⚠️ محدود | ~50 سطر |
| ذاكرة طويلة المدى | ⚠️ محدود | ~100 سطر |
| A/B Testing | ⚠️ محدود | ~150 سطر |
| مصادقة المستخدمين | ❌ غير مدعوم | 500+ سطر |
| قاعدة بيانات | ❌ غير مدعوم | 200-400 سطر |
| تدريب ML | ❌ غير مدعوم | 1000+ سطر |
| تتبع عبر الجلسات | ❌ غير مدعوم | يتطلب auth |
| Analytics Dashboard | ❌ غير مدعوم | ~300 سطر |
