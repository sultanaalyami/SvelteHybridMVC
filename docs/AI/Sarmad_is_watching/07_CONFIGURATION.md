# ⚙️ الثوابت والإعدادات - Configuration Reference

## 1. ملخص سريع

```yaml
# === SARMAD Recommendation Engine Configuration ===

# Confidence Calculation
confidence:
  viewed_cards:
    points_per_card: 4
    max_points: 25
  expanded_cards:
    points_per_card: 13
    max_points: 40
  categories:
    points_per_category: 7
    max_points: 20
  interests:
    points_per_interest: 3
    max_points: 15

# Thresholds
thresholds:
  confidence_for_persona: 40        # Start persona detection
  confidence_for_light_tracking: 50 # Disable advanced tracking
  persona_min_confidence: 25        # Accept persona if >= 25%
  persona_stable_confidence: 50     # Consider persona stable
  advanced_tracking_score: 30       # Enable if engagement < 30

# Recommendation Scoring
recommendation:
  category_multiplier: 2
  topic_match_points: 10
  max_recommendations: 8

# Interaction Tracking
tracking:
  analysis_frequency: 2             # Analyze every N views
  recent_activity_limit: 10         # Queue size
  max_gap_seconds: 300              # Ignore gaps > 5 min

# Engagement Scoring
engagement:
  view_points: 2
  expand_points: 10

# Pace Classification (seconds)
pace:
  very_fast: 3
  fast: 10
  moderate: 30
  slow: 60
  very_deep: 60+
```

---

## 2. أوزان البطاقات

```yaml
# Card Importance Weights (1-10)
cards:
  # Weight 10 (Highest)
  ask-rag: 10
  semantic-kernel: 10
  
  # Weight 9
  archive: 9
  assistants: 9
  app-builder: 9
  rag-memory: 9
  multi-agent-system: 9
  
  # Weight 8
  run-command: 8
  code-interpreter: 8
  signalr-realtime: 8
  ml-intelligence: 8
  vector-search: 8
  error-intelligence: 8
  scalability: 8
  
  # Weight 7
  file-explorer: 7
  logs-system: 7
  session-management: 7
  api-endpoints: 7
  telemetry: 7
  
  # Weight 6 (Lowest)
  marketplace: 6
```

---

## 3. نقاط الـ Persona

```yaml
# Persona Scoring Rules

TechnicalExplorer:
  interests:
    - rag, embeddings, semantic-search: +35
    - code-execution, ml-training: +25
    - semantic-kernel, vector-search: +20
  categories:
    - technology in top 3: +20
  max_score: 100

BusinessDecisionMaker:
  interests:
    - marketplace, monetization: +35
    - scaling, performance: +25
  pace:
    - VeryFast: +20
    - Fast: +10
  behavior:
    - ExpandedCards 2-5 (selective): +20
  max_score: 100

Learner:
  pace:
    - Slow or VeryDeep: +30
  behavior:
    - ExpandedCards >= 5: +25
  categories:
    - core in top 3: +20
  interests:
    - nlp, conversation, general-ai: +15
  max_score: 90

Integrator:
  interests:
    - api, rest, integration: +40
    - docker, deployment, containerization: +30
  categories:
    - integration in top 3: +20
  max_score: 90

Researcher:
  behavior:
    - ExpandedCards >= 8: +35
  pace:
    - VeryDeep: +30
  interests:
    - Count >= 10: +20
  categories:
    - advanced in top 3: +15
  max_score: 100
```

---

## 4. خريطة المواضيع → البطاقات

```yaml
# Topic to Cards Mapping

topics:
  # RAG & AI Core
  rag:
    cards: [archive, ask-rag]
    persona_boost: TechnicalExplorer +35
  
  embeddings:
    cards: [archive, vector-search, rag-memory]
    persona_boost: TechnicalExplorer +35
  
  semantic-search:
    cards: [archive, ask-rag]
    persona_boost: TechnicalExplorer +35
  
  semantic-kernel:
    cards: [ask-rag, semantic-kernel]
    persona_boost: TechnicalExplorer +20
  
  vector-search:
    cards: [ask-rag, vector-search]
    persona_boost: TechnicalExplorer +20
  
  # Development
  code-execution:
    cards: [code-interpreter]
    persona_boost: TechnicalExplorer +25
  
  ml-training:
    cards: [code-interpreter, logs-system]
    persona_boost: TechnicalExplorer +25
  
  debugging:
    cards: [code-interpreter, error-intelligence]
    persona_boost: TechnicalExplorer +25
  
  # Integration
  api:
    cards: [api-endpoints]
    persona_boost: Integrator +40
  
  rest:
    cards: [api-endpoints]
    persona_boost: Integrator +40
  
  integration:
    cards: [api-endpoints]
    persona_boost: Integrator +40
  
  docker:
    cards: [app-builder]
    persona_boost: Integrator +30
  
  deployment:
    cards: [app-builder]
    persona_boost: Integrator +30
  
  containerization:
    cards: [app-builder]
    persona_boost: Integrator +30
  
  # Business
  marketplace:
    cards: [marketplace]
    persona_boost: BusinessDecisionMaker +35
  
  monetization:
    cards: [marketplace]
    persona_boost: BusinessDecisionMaker +35
  
  scaling:
    cards: [scalability]
    persona_boost: BusinessDecisionMaker +25
  
  performance:
    cards: [scalability, telemetry]
    persona_boost: BusinessDecisionMaker +25
  
  # General AI
  nlp:
    cards: [run-command]
    persona_boost: Learner +15
  
  conversation:
    cards: [run-command]
    persona_boost: Learner +15
  
  general-ai:
    cards: [run-command]
    persona_boost: Learner +15
  
  # Agents
  agents:
    cards: [assistants, multi-agent-system, semantic-kernel]
    persona_boost: TechnicalExplorer +20
  
  tools:
    cards: [assistants]
    persona_boost: TechnicalExplorer +20
  
  multi-agent:
    cards: [assistants, multi-agent-system]
    persona_boost: TechnicalExplorer +20
```

---

## 5. قرارات التتبع المتقدم

```yaml
# When to enable advanced tracking

enable_advanced_tracking_when:
  - ConfidenceLevel < 50
  - IdentifiedPersona == Unknown
  - PersonaConfidence < 50
  - ViewedCards > 8 AND ExpandedCards < 2  # Inconsistent
  - TotalEngagementScore < 30 AND ViewedCards > 5  # Shallow

advanced_tracking_signals:
  - Mouse movements (throttled 100ms)
  - Scroll depth (throttled 200ms)
  - IntersectionObserver (threshold 0.5)
  - Focus/blur events
  - Time on card (for expanded cards)
```

---

## 6. الفئات

```yaml
categories:
  core:
    description: "المنتجات الأساسية"
    display_order: 1
    cards: [run-command, file-explorer, archive, ask-rag, assistants]
    avg_weight: 8.6
  
  support:
    description: "وظائف الدعم"
    display_order: 2
    cards: [code-interpreter, logs-system, app-builder]
    avg_weight: 8.0
  
  technology:
    description: "التقنيات"
    display_order: 3
    cards: [signalr-realtime, semantic-kernel, rag-memory, ml-intelligence]
    avg_weight: 8.75
  
  advanced:
    description: "القدرات المتقدمة"
    display_order: 4
    cards: [multi-agent-system, vector-search, session-management, error-intelligence]
    avg_weight: 8.0
  
  integration:
    description: "التكامل والنشر"
    display_order: 5
    cards: [api-endpoints, marketplace, telemetry, scalability]
    avg_weight: 7.0
```

---

## 7. مقارنة سريعة

| الثابت | القيمة | الغرض |
|--------|--------|-------|
| `ANALYSIS_FREQUENCY` | 2 | تحليل كل بطاقتين |
| `RECENT_QUEUE_SIZE` | 10 | حجم قائمة النشاط الأخير |
| `MAX_GAP_SECONDS` | 300 | تجاهل فترات > 5 دقائق |
| `VIEW_POINTS` | 2 | نقاط مشاهدة بدون توسيع |
| `EXPAND_POINTS` | 10 | نقاط التوسيع |
| `CATEGORY_MULTIPLIER` | 2 | مضاعف اهتمام الفئة |
| `TOPIC_MATCH_POINTS` | 10 | نقاط كل موضوع متطابق |
| `MAX_RECOMMENDATIONS` | 8 | عدد التوصيات المرجعة |
| `MIN_CONFIDENCE_PERSONA` | 40 | بدء تحديد الـ persona |
| `MIN_PERSONA_CONFIDENCE` | 25 | قبول الـ persona |
