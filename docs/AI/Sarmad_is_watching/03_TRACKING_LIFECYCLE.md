# 🔄 دورة حياة التتبع والتحليل - Tracking Lifecycle

## 1. المراحل الأساسية

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         TRACKING LIFECYCLE                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐              │
│  │   VIEW   │───▶│  TRACK   │───▶│ ANALYZE  │───▶│ RECOMMEND│              │
│  └──────────┘    └──────────┘    └──────────┘    └──────────┘              │
│       │               │               │               │                     │
│       ▼               ▼               ▼               ▼                     │
│   User sees      Record to      Calculate         Return                    │
│   card in UI     profile        confidence        personalized              │
│                                 & persona          content                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. مرحلة التتبع (`TrackInteraction`)

### 2.1 التدفق التفصيلي

```csharp
public void TrackInteraction(CardInteraction interaction)
{
    // ═══════════════════════════════════════════════════════════════
    // المرحلة 1: إنشاء أو جلب الـ Profile
    // ═══════════════════════════════════════════════════════════════
    if (!_userProfiles.ContainsKey(interaction.SessionId))
    {
        _userProfiles[interaction.SessionId] = new UserInterestProfile
        {
            SessionId = interaction.SessionId,
            FirstInteraction = DateTime.UtcNow
        };
    }
    var profile = _userProfiles[interaction.SessionId];
    
    // ═══════════════════════════════════════════════════════════════
    // المرحلة 2: تحديث البيانات الأساسية
    // ═══════════════════════════════════════════════════════════════
    profile.LastInteraction = DateTime.UtcNow;
    
    if (!profile.ViewedCards.Contains(interaction.CardId))
    {
        profile.ViewedCards.Add(interaction.CardId);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // المرحلة 3: تتبع النشاط الأخير (Queue)
    // ═══════════════════════════════════════════════════════════════
    profile.RecentActivity.Enqueue((interaction.CardId, DateTime.UtcNow));
    if (profile.RecentActivity.Count > 10)
        profile.RecentActivity.Dequeue();
    
    // ═══════════════════════════════════════════════════════════════
    // المرحلة 4: حساب سرعة التفاعل
    // ═══════════════════════════════════════════════════════════════
    UpdateInteractionPace(profile);
    
    // ═══════════════════════════════════════════════════════════════
    // المرحلة 5: معالجة التوسيع (إن وجد)
    // ═══════════════════════════════════════════════════════════════
    if (interaction.Expanded && !profile.ExpandedCards.Contains(interaction.CardId))
    {
        profile.ExpandedCards.Add(interaction.CardId);
        
        var card = GetCard(interaction.CardId);
        if (card != null)
        {
            // تحديث اهتمام الفئة
            profile.CategoryInterest[card.Category] += card.ImportanceWeight;
            
            // استنتاج الاهتمامات من المواضيع
            foreach (var topic in card.RelatedTopics)
            {
                if (!profile.InferredInterests.Contains(topic))
                    profile.InferredInterests.Add(topic);
            }
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // المرحلة 6: تحديث نقاط التفاعل
    // ═══════════════════════════════════════════════════════════════
    profile.TotalEngagementScore += interaction.Expanded ? 10 : 2;
    
    // ═══════════════════════════════════════════════════════════════
    // المرحلة 7: التحليل الدوري (كل بطاقتين)
    // ═══════════════════════════════════════════════════════════════
    if (profile.ViewedCards.Count % 2 == 0)
    {
        AnalyzeUserProfile(interaction.SessionId);
    }
}
```

### 2.2 مخطط التدفق

```
TrackInteraction(interaction)
          │
          ▼
    ┌───────────────────┐
    │ Profile exists?   │
    └─────────┬─────────┘
              │
    ┌─────────┴─────────┐
    │                   │
    ▼ No                ▼ Yes
┌──────────┐      ┌──────────┐
│ Create   │      │  Get     │
│ Profile  │      │ Profile  │
└────┬─────┘      └────┬─────┘
     │                 │
     └────────┬────────┘
              │
              ▼
    ┌───────────────────┐
    │ Update ViewedCards│
    │ Update RecentActiv│
    │ UpdatePace()      │
    └─────────┬─────────┘
              │
              ▼
    ┌───────────────────┐
    │ Expanded?         │
    └─────────┬─────────┘
              │
    ┌─────────┴─────────┐
    │                   │
    ▼ Yes               ▼ No
┌──────────────┐   ┌──────────────┐
│ Add Expanded │   │ Score += 2   │
│ CategoryInt++│   │              │
│ AddTopics    │   │              │
│ Score += 10  │   │              │
└──────┬───────┘   └──────┬───────┘
       │                  │
       └────────┬─────────┘
                │
                ▼
    ┌───────────────────┐
    │ ViewedCards % 2?  │
    └─────────┬─────────┘
              │
    ┌─────────┴─────────┐
    │ == 0              │ != 0
    ▼                   ▼
┌──────────────┐   ┌──────────────┐
│AnalyzeProfile│   │    Done      │
└──────────────┘   └──────────────┘
```

---

## 3. مرحلة التحليل (`AnalyzeUserProfile`)

### 3.1 التدفق

```csharp
public void AnalyzeUserProfile(string sessionId)
{
    if (!_userProfiles.TryGetValue(sessionId, out var profile))
        return;

    // ═══════════════════════════════════════════════════════════════
    // الخطوة 1: حساب مستوى الثقة
    // ═══════════════════════════════════════════════════════════════
    profile.ConfidenceLevel = CalculateConfidenceLevel(profile);
    
    // ═══════════════════════════════════════════════════════════════
    // الخطوة 2: تحديد الـ Persona (إذا كانت الثقة كافية)
    // ═══════════════════════════════════════════════════════════════
    if (profile.ConfidenceLevel >= 40)
    {
        var (persona, confidence) = DeterminePersona(profile);
        profile.IdentifiedPersona = persona;
        profile.PersonaConfidence = confidence;
        
        // ═══════════════════════════════════════════════════════════
        // الخطوة 3: تحديد الحاجة للتتبع المتقدم
        // ═══════════════════════════════════════════════════════════
        profile.NeedsAdvancedTracking = ShouldEnableAdvancedTracking(profile);
    }
    else
    {
        // ثقة منخفضة → تفعيل التتبع المتقدم
        profile.NeedsAdvancedTracking = true;
    }
}
```

### 3.2 قرار التتبع المتقدم

```
ShouldEnableAdvancedTracking(profile)
                │
                ▼
    ┌───────────────────────┐
    │ ConfidenceLevel < 50? │──── Yes ──▶ return true
    └───────────┬───────────┘
                │ No
                ▼
    ┌───────────────────────┐
    │ Persona == Unknown?   │──── Yes ──▶ return true
    └───────────┬───────────┘
                │ No
                ▼
    ┌───────────────────────┐
    │ PersonaConf < 50?     │──── Yes ──▶ return true
    └───────────┬───────────┘
                │ No
                ▼
    ┌───────────────────────┐
    │ Views > 8 && Expand<2?│──── Yes ──▶ return true
    └───────────┬───────────┘           (سلوك غير متسق)
                │ No
                ▼
    ┌───────────────────────┐
    │ Score<30 && Views>5?  │──── Yes ──▶ return true
    └───────────┬───────────┘           (تفاعل سطحي)
                │ No
                ▼
           return false
        (البيانات كافية)
```

---

## 4. مرحلة التوصية (`GetPersonalizedCapabilities`)

### 4.1 الخوارزمية

```csharp
// في CapabilitiesController.cs
[HttpGet("personalized/{sessionId}")]
public IActionResult GetPersonalizedCapabilities(string sessionId)
{
    var profile = _capabilityCards.GetUserProfile(sessionId);

    List<CapabilityCard> personalizedCards;

    // ═══════════════════════════════════════════════════════════════
    // الحالة 1: لا يوجد profile → البطاقات الأساسية
    // ═══════════════════════════════════════════════════════════════
    if (profile == null || profile.ViewedCards.Count == 0)
    {
        personalizedCards = _capabilityCards.GetCardsByCategory("core")
            .Take(8)
            .ToList();
    }
    else
    {
        // ═══════════════════════════════════════════════════════════
        // الحالة 2: توجد بيانات → حساب التوصيات
        // ═══════════════════════════════════════════════════════════
        var allCards = _capabilityCards.GetAllCards();
        var recommendations = new List<(CapabilityCard card, int score)>();

        foreach (var card in allCards)
        {
            // تجاهل البطاقات المشاهدة
            if (profile.ViewedCards.Contains(card.Id))
                continue;

            int score = 0;

            // نقاط الفئة
            if (profile.CategoryInterest.TryGetValue(card.Category, out var categoryScore))
            {
                score += categoryScore * 2;
            }

            // نقاط المواضيع المتطابقة
            var matchingTopics = card.RelatedTopics
                .Count(t => profile.InferredInterests.Contains(t));
            score += matchingTopics * 10;

            // وزن الأهمية
            score += card.ImportanceWeight;

            recommendations.Add((card, score));
        }

        // الترتيب واختيار أعلى 8
        personalizedCards = recommendations
            .OrderByDescending(r => r.score)
            .Take(8)
            .Select(r => r.card)
            .ToList();
    }

    return Ok(personalizedCards);
}
```

### 4.2 مخطط التدفق

```
GET /api/capabilities/personalized/{sessionId}
                    │
                    ▼
          ┌─────────────────┐
          │ GetUserProfile  │
          └────────┬────────┘
                   │
        ┌──────────┴──────────┐
        │                     │
        ▼ null/empty          ▼ has data
┌───────────────┐      ┌───────────────┐
│ Return core   │      │ For each card │
│ cards (8)     │      │ not in viewed │
└───────────────┘      └───────┬───────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Calculate Score:    │
                    │ + CategoryInt × 2   │
                    │ + MatchTopics × 10  │
                    │ + ImportanceWeight  │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Sort by Score DESC  │
                    │ Take Top 8          │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Return JSON Array   │
                    └─────────────────────┘
```

---

## 5. تكامل الـ Frontend

### 5.1 JavaScript Tracking (من `Index.cshtml`)

```javascript
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
        
        // إرسال إشعار لسرمد
        if (window.intelligentHub) {
            await window.intelligentHub.invoke('CardExpanded', id);
        }
        
        // أو عبر REST API
        await fetch('/api/capabilities/track', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                sessionId: sessionId,
                cardId: id,
                expanded: true
            })
        });
    }
}

// تحميل البطاقات المخصصة
async function loadInitialCapabilities() {
    try {
        // محاولة جلب توصيات مخصصة
        const res = await fetch('/api/capabilities/personalized/' + sessionId);
        let list = await res.json();
        
        // fallback للبطاقات الأساسية
        if (!Array.isArray(list) || list.length === 0) {
            const r2 = await fetch('/api/capabilities/initial');
            list = await r2.json();
        }
        
        // عرض البطاقات
        const grid = document.getElementById('capabilitiesGrid');
        grid.innerHTML = '';
        list.forEach(addCapabilityCard);
    } catch (e) {
        console.error('فشل تحميل القدرات', e);
    }
}
```

### 5.2 SignalR Real-time Updates

```javascript
// الاستماع لقرارات التتبع من سرمد
function setupIntelligentListeners() {
    const hub = window.intelligentHub;
    
    // قرار تفعيل/تعطيل التتبع المتقدم
    hub.on('TrackingDecision', (decision) => {
        console.log('🤖 SARMAD Decision:', {
            mode: decision.needsAdvancedTracking ? 'Full' : 'Light',
            confidence: decision.confidenceLevel + '%',
            persona: decision.persona
        });
        
        if (decision.needsAdvancedTracking) {
            enableAdvancedTracking();  // تتبع الماوس، التمرير، إلخ
        } else {
            disableAdvancedTracking(); // تتبع أساسي فقط
        }
    });
}
```

---

## 6. ملخص دورة الحياة

```
═══════════════════════════════════════════════════════════════════════════════
                           COMPLETE LIFECYCLE
═══════════════════════════════════════════════════════════════════════════════

[1] USER ARRIVES
    └── sessionId = crypto.randomUUID()
    └── Profile created (empty)
    └── Load initial cards (core category)

[2] USER VIEWS CARD
    └── ViewedCards.Add(cardId)
    └── RecentActivity.Enqueue()
    └── UpdateInteractionPace()
    └── EngagementScore += 2

[3] USER EXPANDS CARD
    └── ExpandedCards.Add(cardId)
    └── CategoryInterest[cat] += weight
    └── InferredInterests += topics
    └── EngagementScore += 10

[4] EVERY 2 VIEWS: ANALYZE
    └── CalculateConfidenceLevel()
    └── if (confidence >= 40):
        └── DeterminePersona()
        └── ShouldEnableAdvancedTracking()

[5] USER REQUESTS RECOMMENDATIONS
    └── Calculate scores for unseen cards
    └── Sort by score DESC
    └── Return top 8

[6] CYCLE REPEATS
    └── More interactions → Higher confidence
    └── Better recommendations → More engagement
    └── Virtuous cycle

═══════════════════════════════════════════════════════════════════════════════
```
