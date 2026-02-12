# 💻 مقتطفات الكود الأساسية - Code Snippets

## 1. الخدمة الرئيسية (`CapabilityCardsService.cs`)

### 1.1 تعريف الخدمة

```csharp
// Services/CapabilityCardsService.cs

using McpServerFull.Models;

namespace McpServerFull.Services;

/// <summary>
/// خدمة توفير البطاقات التحليلية الـ 20 لقدرات SARMAD
/// </summary>
public class CapabilityCardsService
{
    private readonly List<CapabilityCard> _cards;
    private readonly ILogger<CapabilityCardsService> _logger;
    private readonly Dictionary<string, UserInterestProfile> _userProfiles = new();

    public CapabilityCardsService(ILogger<CapabilityCardsService> logger)
    {
        _logger = logger;
        _cards = InitializeCards();
    }

    // ... methods
}
```

---

### 1.2 تتبع التفاعل (`TrackInteraction`)

```csharp
/// <summary>
/// تسجيل تفاعل مع بطاقة مع تحليل ذكي
/// </summary>
public void TrackInteraction(CardInteraction interaction)
{
    // إنشاء profile جديد إذا لم يكن موجوداً
    if (!_userProfiles.ContainsKey(interaction.SessionId))
    {
        _userProfiles[interaction.SessionId] = new UserInterestProfile
        {
            SessionId = interaction.SessionId,
            FirstInteraction = DateTime.UtcNow
        };
    }

    var profile = _userProfiles[interaction.SessionId];
    profile.LastInteraction = DateTime.UtcNow;

    // تسجيل المشاهدة
    if (!profile.ViewedCards.Contains(interaction.CardId))
    {
        profile.ViewedCards.Add(interaction.CardId);
    }
    
    // تتبع النشاط الأخير (آخر 10)
    profile.RecentActivity.Enqueue((interaction.CardId, DateTime.UtcNow));
    if (profile.RecentActivity.Count > 10)
        profile.RecentActivity.Dequeue();
    
    // تحديث سرعة التفاعل
    UpdateInteractionPace(profile);

    // معالجة التوسيع
    if (interaction.Expanded && !profile.ExpandedCards.Contains(interaction.CardId))
    {
        profile.ExpandedCards.Add(interaction.CardId);
        
        var card = GetCard(interaction.CardId);
        if (card != null)
        {
            // زيادة اهتمام الفئة
            if (!profile.CategoryInterest.ContainsKey(card.Category))
                profile.CategoryInterest[card.Category] = 0;
            
            profile.CategoryInterest[card.Category] += card.ImportanceWeight;
            
            // استنتاج الاهتمامات من المواضيع
            foreach (var topic in card.RelatedTopics)
            {
                if (!profile.InferredInterests.Contains(topic))
                    profile.InferredInterests.Add(topic);
            }
        }
    }

    // تحديث نقاط التفاعل
    profile.TotalEngagementScore += interaction.Expanded ? 10 : 2;
    
    // التحليل الذكي بعد كل بطاقتين
    if (profile.ViewedCards.Count % 2 == 0)
    {
        AnalyzeUserProfile(interaction.SessionId);
    }

    _logger.LogInformation(
        "🔖 Card interaction: {CardId} by {SessionId}, Expanded: {Expanded}, " +
        "Pace: {Pace}, Confidence: {Confidence}%",
        interaction.CardId, interaction.SessionId, interaction.Expanded, 
        profile.Pace, profile.ConfidenceLevel);
}
```

---

### 1.3 حساب مستوى الثقة (`CalculateConfidenceLevel`)

```csharp
/// <summary>
/// حساب مستوى الثقة (0-100) بناءً على كمية وجودة البيانات
/// </summary>
private int CalculateConfidenceLevel(UserInterestProfile profile)
{
    int confidence = 0;
    
    // البطاقات المشاهدة (حتى 25 نقطة)
    // 4 نقاط لكل بطاقة
    confidence += Math.Min(profile.ViewedCards.Count * 4, 25);
    
    // البطاقات الموسعة (حتى 40 نقطة) - الأهم!
    // 13 نقطة لكل توسيع
    confidence += Math.Min(profile.ExpandedCards.Count * 13, 40);
    
    // تنوع الفئات (حتى 20 نقطة)
    // 7 نقاط لكل فئة مختلفة
    confidence += Math.Min(profile.CategoryInterest.Count * 7, 20);
    
    // الاهتمامات المستنتجة (حتى 15 نقطة)
    // 3 نقاط لكل موضوع
    confidence += Math.Min(profile.InferredInterests.Count * 3, 15);
    
    return Math.Min(confidence, 100);
}
```

---

### 1.4 تحديد الـ Persona (`DeterminePersona`)

```csharp
/// <summary>
/// تحديد نوع المستخدم بناءً على أنماط التفاعل
/// </summary>
private (UserPersona persona, int confidence) DeterminePersona(UserInterestProfile profile)
{
    var scores = new Dictionary<UserPersona, int>();
    
    // تحليل الفئات المفضلة
    var topCategories = profile.CategoryInterest
        .OrderByDescending(x => x.Value)
        .Take(3)
        .ToList();
    
    var interests = profile.InferredInterests;
    
    // === TechnicalExplorer ===
    int techScore = 0;
    if (interests.Contains("rag") || interests.Contains("embeddings") || 
        interests.Contains("semantic-search"))
        techScore += 35;
    if (interests.Contains("code-execution") || interests.Contains("ml-training"))
        techScore += 25;
    if (interests.Contains("semantic-kernel") || interests.Contains("vector-search"))
        techScore += 20;
    if (topCategories.Any(c => c.Key == "technology"))
        techScore += 20;
    scores[UserPersona.TechnicalExplorer] = techScore;
    
    // === BusinessDecisionMaker ===
    int businessScore = 0;
    if (interests.Contains("marketplace") || interests.Contains("monetization"))
        businessScore += 35;
    if (interests.Contains("scaling") || interests.Contains("performance"))
        businessScore += 25;
    if (profile.Pace == InteractionPace.VeryFast)
        businessScore += 20;
    else if (profile.Pace == InteractionPace.Fast)
        businessScore += 10;
    if (profile.ExpandedCards.Count >= 2 && profile.ExpandedCards.Count <= 5)
        businessScore += 20;
    scores[UserPersona.BusinessDecisionMaker] = businessScore;
    
    // === Learner ===
    int learnerScore = 0;
    if (profile.Pace == InteractionPace.Slow || profile.Pace == InteractionPace.VeryDeep)
        learnerScore += 30;
    if (profile.ExpandedCards.Count >= 5)
        learnerScore += 25;
    if (topCategories.Any(c => c.Key == "core"))
        learnerScore += 20;
    if (interests.Contains("nlp") || interests.Contains("conversation") || 
        interests.Contains("general-ai"))
        learnerScore += 15;
    scores[UserPersona.Learner] = learnerScore;
    
    // === Integrator ===
    int integratorScore = 0;
    if (interests.Contains("api") || interests.Contains("rest") || 
        interests.Contains("integration"))
        integratorScore += 40;
    if (interests.Contains("docker") || interests.Contains("deployment") || 
        interests.Contains("containerization"))
        integratorScore += 30;
    if (topCategories.Any(c => c.Key == "integration"))
        integratorScore += 20;
    scores[UserPersona.Integrator] = integratorScore;
    
    // === Researcher ===
    int researchScore = 0;
    if (profile.ExpandedCards.Count >= 8)
        researchScore += 35;
    if (profile.Pace == InteractionPace.VeryDeep)
        researchScore += 30;
    if (interests.Count >= 10)
        researchScore += 20;
    if (topCategories.Any(c => c.Key == "advanced"))
        researchScore += 15;
    scores[UserPersona.Researcher] = researchScore;
    
    // اختيار أعلى نتيجة
    var best = scores.OrderByDescending(x => x.Value).First();
    
    int maxPossibleScore = 100;
    int confidence = Math.Min((best.Value * 100) / maxPossibleScore, 100);
    
    // العتبة: 25% minimum
    return confidence >= 25 
        ? (best.Key, confidence)
        : (UserPersona.Unknown, confidence);
}
```

---

### 1.5 حساب سرعة التفاعل (`UpdateInteractionPace`)

```csharp
/// <summary>
/// حساب سرعة التفاعل بناءً على النشاط الأخير
/// </summary>
private void UpdateInteractionPace(UserInterestProfile profile)
{
    if (profile.RecentActivity.Count < 2)
    {
        profile.Pace = InteractionPace.Unknown;
        return;
    }
    
    // حساب متوسط الفترة بين البطاقات
    var activities = profile.RecentActivity.ToArray();
    double totalSeconds = 0;
    int count = 0;
    
    for (int i = 1; i < activities.Length; i++)
    {
        var diff = (activities[i].OpenedAt - activities[i - 1].OpenedAt).TotalSeconds;
        
        // تجاهل فترات أطول من 5 دقائق (المستخدم غادر وعاد)
        if (diff < 300)
        {
            totalSeconds += diff;
            count++;
        }
    }
    
    if (count == 0)
    {
        profile.Pace = InteractionPace.Unknown;
        return;
    }
    
    double avgSeconds = totalSeconds / count;
    
    // تصنيف السرعة
    profile.Pace = avgSeconds switch
    {
        < 3 => InteractionPace.VeryFast,   // قرارات سريعة
        < 10 => InteractionPace.Fast,      // نشط
        < 30 => InteractionPace.Moderate,  // عادي
        < 60 => InteractionPace.Slow,      // متأمل
        _ => InteractionPace.VeryDeep      // باحث متعمق
    };
}
```

---

### 1.6 البطاقات ذات الصلة (`GetRelatedCapabilities`)

```csharp
/// <summary>
/// الحصول على البطاقات ذات الصلة بناءً على المواضيع المشتركة
/// </summary>
public List<CapabilityCard> GetRelatedCapabilities(string cardId)
{
    var card = GetCard(cardId);
    if (card == null) return new List<CapabilityCard>();

    // البحث عن بطاقات ذات مواضيع مشتركة
    return _cards
        .Where(c => c.Id != cardId &&
                   c.RelatedTopics.Any(t => card.RelatedTopics.Contains(t)))
        .OrderByDescending(c => c.RelatedTopics.Count(t => card.RelatedTopics.Contains(t)))
        .Take(3)
        .ToList();
}
```

---

## 2. نماذج البيانات (`Models`)

### 2.1 ملف اهتمامات المستخدم

```csharp
/// <summary>
/// ملف اهتمامات المستخدم مع مستوى الثقة والمعايير الذكية
/// </summary>
public class UserInterestProfile
{
    public required string SessionId { get; set; }
    
    // === البيانات السلوكية ===
    public List<string> ViewedCards { get; set; } = new();
    public List<string> ExpandedCards { get; set; } = new();
    public Dictionary<string, int> CategoryInterest { get; set; } = new();
    public List<string> InferredInterests { get; set; } = new();
    public int TotalEngagementScore { get; set; }
    public DateTime FirstInteraction { get; set; }
    public DateTime LastInteraction { get; set; }
    
    // === مؤشرات الثقة الذكية ===
    
    /// <summary>
    /// مستوى الثقة في فهم اهتمامات المستخدم (0-100)
    /// </summary>
    public int ConfidenceLevel { get; set; }
    
    /// <summary>
    /// هل نحتاج لتتبع متقدم (ماوس، تموضع، إلخ)؟
    /// </summary>
    public bool NeedsAdvancedTracking { get; set; }
    
    /// <summary>
    /// نوع العميل المستنتج
    /// </summary>
    public UserPersona? IdentifiedPersona { get; set; }
    
    /// <summary>
    /// مستوى اليقين في تحديد النوع (0-100)
    /// </summary>
    public int PersonaConfidence { get; set; }
    
    /// <summary>
    /// البطاقات المفتوحة في آخر 10 تفاعلات
    /// </summary>
    public Queue<(string CardId, DateTime OpenedAt)> RecentActivity { get; set; } = new();
    
    /// <summary>
    /// سرعة التفاعل
    /// </summary>
    public InteractionPace Pace { get; set; } = InteractionPace.Unknown;
}
```

### 2.2 الـ Enums

```csharp
/// <summary>
/// أنواع العملاء المستنتجة من السلوك
/// </summary>
public enum UserPersona
{
    Unknown,              // لم يتم التحديد بعد
    TechnicalExplorer,    // مطور تقني يستكشف القدرات
    BusinessDecisionMaker,// صاحب قرار يبحث عن حلول
    Learner,              // متعلم يريد فهم التكنولوجيا
    Integrator,           // مهندس يريد التكامل
    Researcher            // باحث يريد معلومات مفصلة
}

/// <summary>
/// سرعة التفاعل
/// </summary>
public enum InteractionPace
{
    Unknown,
    VeryFast,    // < 3 ثواني بين البطاقات
    Fast,        // 3-10 ثواني
    Moderate,    // 10-30 ثانية
    Slow,        // 30-60 ثانية
    VeryDeep     // > 60 ثانية - قراءة متعمقة
}
```

---

## 3. مثال على بطاقة (`CapabilityCard`)

```csharp
new CapabilityCard
{
    Id = "ask-rag",
    Title = "Ask (RAG) - الاستدلال المعزز",
    ShortDescription = "أقوى قدرات سرمد - أسئلة ذكية بناءً على مشاريعك",
    IconClass = "fas fa-brain",
    Category = "core",
    DisplayOrder = 4,
    ImportanceWeight = 10,  // الأعلى!
    ExpandedContent = """
        <h4>القدرة الأقوى في SARMAD</h4>
        <p><strong>Ask (RAG)</strong> يجمع بين قوة النماذج اللغوية 
        والبحث الدلالي المتقدم.</p>
        """,
    KeyFeatures = new List<string>
    {
        "Retrieval-Augmented Generation",
        "ذاكرة دلالية متقدمة",
        "Semantic Kernel Integration",
        "دقة عالية في الإجابات"
    },
    UseCases = new List<string>
    {
        "شرح كيف يعمل كود معين",
        "إيجاد أمثلة من مشاريعك",
        "تحليل البنية المعمارية",
        "اكتشاف Bugs محتملة"
    },
    CodeExample = """
        POST /mcp/ask
        Content-Type: text/plain
        
        كيف يعمل نظام الذاكرة في هذا المشروع؟
        """,
    RelatedTopics = new List<string> 
    { 
        "rag", 
        "semantic-kernel", 
        "vector-search", 
        "context-aware" 
    }
}
```

---

## 4. كود الـ Frontend (JavaScript)

### 4.1 إنشاء Session وتتبع

```javascript
// من Index.cshtml

// إنشاء session جديد
const sessionId = crypto.randomUUID();

// كائن تتبع السلوك
const behavior = { 
    MouseTrack: [], 
    FocusAreas: [], 
    ClickedElements: [], 
    ScrollDepth: 0, 
    TimeOnPage: 0, 
    Interests: [], 
    SessionId: sessionId 
};

// تتبع توسيع البطاقة
async function expandCard(btn, id) {
    const card = btn.closest('.capability-card');
    const full = card.querySelector('.card-full-content');
    
    if (full.style.display === 'none') {
        full.style.display = 'block';
        btn.innerHTML = '<i class="fas fa-chevron-up"></i> إخفاء';
        
        // إرسال إشعار التوسيع
        await trackInteraction(id, true);
    } else {
        full.style.display = 'none';
        btn.innerHTML = '<i class="fas fa-chevron-down"></i> اقرأ المزيد';
    }
}

// إرسال التفاعل للخادم
async function trackInteraction(cardId, expanded) {
    try {
        const response = await fetch('/api/capabilities/track', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                sessionId: sessionId,
                cardId: cardId,
                expanded: expanded
            })
        });
        
        const result = await response.json();
        
        // تحديث الواجهة بناءً على قرارات سرمد
        if (result.profile) {
            console.log('🤖 SARMAD Analysis:', {
                confidence: result.profile.confidenceLevel + '%',
                persona: result.profile.identifiedPersona,
                needsTracking: result.profile.needsAdvancedTracking
            });
            
            if (result.profile.needsAdvancedTracking) {
                enableAdvancedTracking();
            }
        }
        
        return result;
    } catch (error) {
        console.error('Track error:', error);
    }
}
```

### 4.2 تحميل التوصيات المخصصة

```javascript
// تحميل البطاقات المخصصة عند بدء الصفحة
async function loadInitialCapabilities() {
    try {
        // محاولة جلب توصيات مخصصة أولاً
        const res = await fetch('/api/capabilities/personalized/' + sessionId);
        let list = await res.json();
        
        // fallback للبطاقات الأساسية إذا لم توجد توصيات
        if (!Array.isArray(list) || list.length === 0) {
            const r2 = await fetch('/api/capabilities/initial');
            list = await r2.json();
        }
        
        // عرض البطاقات
        const grid = document.getElementById('capabilitiesGrid');
        grid.innerHTML = '';
        
        list.forEach(card => {
            addCapabilityCard(card);
        });
        
    } catch (e) {
        console.error('فشل تحميل القدرات', e);
        // fallback نهائي
        loadStaticCards();
    }
}

// تشغيل عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', loadInitialCapabilities);
```

### 4.3 التتبع المتقدم (عند الحاجة)

```javascript
// تفعيل التتبع المتقدم عندما يطلبه سرمد
function enableAdvancedTracking() {
    console.log('🔬 Enabling advanced tracking...');
    
    // تتبع حركة الماوس
    document.addEventListener('mousemove', throttle((e) => {
        behavior.MouseTrack.push({
            x: e.clientX,
            y: e.clientY,
            t: Date.now()
        });
        
        // الاحتفاظ بآخر 100 نقطة فقط
        if (behavior.MouseTrack.length > 100) {
            behavior.MouseTrack.shift();
        }
    }, 100));
    
    // تتبع التمرير
    document.addEventListener('scroll', throttle(() => {
        const scrollTop = window.scrollY;
        const docHeight = document.documentElement.scrollHeight - window.innerHeight;
        behavior.ScrollDepth = Math.round((scrollTop / docHeight) * 100);
    }, 200));
    
    // تتبع التركيز على العناصر
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const cardId = entry.target.dataset.cardId;
                if (cardId && !behavior.FocusAreas.includes(cardId)) {
                    behavior.FocusAreas.push(cardId);
                }
            }
        });
    }, { threshold: 0.5 });
    
    document.querySelectorAll('.capability-card').forEach(card => {
        observer.observe(card);
    });
}

// دالة throttle لتقليل عدد الأحداث
function throttle(func, limit) {
    let inThrottle;
    return function(...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}
```

---

## 5. تسجيل الخدمة (`Program.cs`)

```csharp
// Program.cs

var builder = WebApplication.CreateBuilder(args);

// تسجيل الخدمة كـ Singleton (للحفاظ على الـ profiles في الذاكرة)
builder.Services.AddSingleton<CapabilityCardsService>();

// ... باقي التسجيلات

var app = builder.Build();

// ... باقي الإعدادات

app.Run();
```
