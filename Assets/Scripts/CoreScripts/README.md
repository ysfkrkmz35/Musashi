# Musashi Core Scripts - Setup Guide

## 📁 Klasör Yapısı

```
CoreScripts/
├── Combat/                    # Dövüş mekanikleri
│   ├── AttackDirection.cs            # Yön enum ve data sınıfları
│   ├── DirectionalCombatSystem.cs    # Ana yönlü saldırı/savunma sistemi
│   ├── PlayerDuelControllerDirectional.cs   # Oyuncu controller
│   ├── EnemyDuelControllerDirectional.cs    # Düşman AI
│   ├── CombatHitboxDirectional.cs           # Yönlü hasar sistemi
│   ├── CombatAudioManager.cs         # Ses efektleri
│   └── CombatEffectsManager.cs       # Görsel efektler (camera shake, slow-mo)
│
├── UI/                        # Kullanıcı arayüzü
│   └── DirectionalIndicatorUI.cs     # 4 yön göstergeleri
│
├── Progression/               # İlerleme sistemi
│   ├── PlayerStats.cs                # İstatistik yönetimi
│   └── CampUpgradeSystem.cs          # Kamp yükseltme ekranı
│
└── Managers/                  # Oyun yöneticileri
    └── MusashiGameManager.cs         # Ana oyun akışı
```

---

## 🎮 Sistem Özellikleri

### ✅ Yönlü Saldırı-Savunma (For Honor tarzı)
- **4 Yön**: Yukarı, Aşağı, Sol, Sağ
- **Input**: Arrow keys (saldırı) + WASD (savunma)
- **Block**: Aynı yöne savunma = blok
- **Parry**: Farklı yöne parry = karşı saldırı penceresi
- **UI**: Gerçek zamanlı yön göstergeleri

### ✅ Odak Barı Sistemi (Shadow Fight tarzı)
- Tüm hareketler odak harcar
- Pasif yenilenme + Meditasyon
- Bar biterse savunmasız kalırsın

### ✅ Düşman AI
- Yönlü saldırı/savunma yapabilir
- Oyuncu tercihlerini öğrenir
- Zorluk seviyesi artar

### ✅ Kamp Yükseltme (Roguelite)
- **Hız**: +%15 saldırı hızı, +0.05s dodge
- **Güç**: +5 hasar, +%10 stance break
- **Odak**: +2 odak/sn, +10 max odak

### ✅ Görsel/İşitsel Efektler
- Camera shake (vuruş, parry)
- Slow-motion (parry başarısı)
- Screen flash
- Partikül efektleri (sparks, trails)
- Ses efektleri (clash, parry, hit)

---

## 🛠️ Unity'de Setup

### 1. Player Setup

**Hierarchy'de:**
```
Player
├── DirectionalCombatSystem (component)
├── PlayerDuelControllerDirectional (component)
├── Animator
├── Health (HealthB)
├── WeaponPoint
│   └── CombatHitboxDirectional (component + Collider)
└── CombatAudioManager (component)
```

**Inspector Ayarları:**

**PlayerDuelControllerDirectional:**
- Focus Max: 100
- Focus Regen Rate: 6
- Meditate Bonus: 12
- Light Cost: 10, Heavy Cost: 20
- Parry Cost: 8, Dodge Cost: 12
- Attack Cooldown: 0.65
- Keys: Mouse0 (light), Mouse1 (heavy), Shift (parry), Space (dodge), R (meditate)
- Directional Keys: Arrow keys (attack), WASD (defense)

**DirectionalCombatSystem:**
- Parry Window: 0.25s
- Counter Window: 0.8s
- Dodge Window: 0.3s
- Block Focus Cost: 5
- Parry Focus Cost: 8
- Parry Fail Penalty: 15

**CombatHitboxDirectional:**
- Team: Player
- Base Damage: 15 (light), 30 (heavy)
- Hit Collider: BoxCollider (isTrigger = true)
- **ÖNEMLİ**: Animator events ekleyin:
  - Saldırı animasyonunda: `AttackStart()` ve `AttackEnd()`

---

### 2. Enemy Setup

**Hierarchy'de:**
```
Enemy
├── DirectionalCombatSystem (component)
├── EnemyDuelControllerDirectional (component)
├── Animator
├── Health (HealthB)
└── WeaponPoint
    └── CombatHitboxDirectional (component + Collider)
```

**Inspector Ayarları:**

**EnemyDuelControllerDirectional:**
- Player: [Drag Player transform]
- Focus Max: 100
- Focus Regen Rate: 5
- Think Every: (1.2, 2.0)
- Aggression Level: 0.5
- Prediction Skill: 0.3
- Adaptation Rate: 0.2
- Locked X: 2.5

**CombatHitboxDirectional:**
- Team: Enemy

---

### 3. UI Setup

**Canvas Hierarchy:**
```
Canvas
├── FocusBarPanel
│   ├── PlayerFocusBar (Slider)
│   └── EnemyFocusBar (Slider)
├── DirectionalIndicatorUI
│   ├── PlayerAttackIndicator
│   │   ├── UpArrow (Image)
│   │   ├── DownArrow (Image)
│   │   ├── LeftArrow (Image)
│   │   └── RightArrow (Image)
│   ├── PlayerDefenseIndicator (same structure)
│   ├── EnemyAttackIndicator (same structure)
│   └── EnemyDefenseIndicator (same structure)
└── FlashImage (full screen Image, disabled by default)
```

**DirectionalIndicatorUI:**
- Attack Color: Red (1, 0.2, 0.2, 0.8)
- Defense Color: Blue (0.2, 0.5, 1, 0.8)
- Parry Color: Gold (1, 0.8, 0.2, 1)
- Counter Color: Green (0, 1, 0.5, 1)

---

### 4. Game Manager Setup

**Hierarchy'de:**
```
GameManager (DontDestroyOnLoad)
├── MusashiGameManager (component)
└── CampUpgradeSystem (component)
```

**MusashiGameManager:**
- Total Duels: 7
- Duel Scene Name: "baris"
- Camp Scene Name: "Camp"

---

### 5. Effects Setup

**Scene'de:**
```
CombatEffects
├── CombatEffectsManager (component)
├── Main Camera (reference)
└── Prefabs folder references
```

**Prefab Referansları:**
- Sword Trail Prefab: [Trail renderer prefab]
- Parry Sparks Prefab: [Particle system]
- Hit Sparks Prefab: [Particle system]
- Block Sparks Prefab: [Particle system]

---

## 🎯 Kullanım

### Oyuncu Kontrolleri

**Saldırı:**
- **Arrow Keys** (↑↓←→): Saldırı yönü seç
- **Mouse 0**: Hafif saldırı
- **Mouse 1**: Ağır saldırı

**Savunma:**
- **WASD**: Savunma yönü seç
- **Shift**: Parry
- **Space**: Dodge

**Odak:**
- **R (hold)**: Meditasyon (odak doldurma)

### Parry Mekaniği

1. Düşman saldırısı geldiğinde **Shift** bas (parry aktivasyonu)
2. **Farklı yöne** bak (örnek: düşman yukarıdan saldırıyorsa, sen aşağı/sol/sağ)
3. Başarılı parry → Counter window açılır (0.8s)
4. Counter window'da saldırı = bonus damage

### Kamp Sistemi

1. Düello kazanıldıktan 2 saniye sonra kamp ekranı açılır
2. 3 kart sunulur: **Hız**, **Güç**, **Odak**
3. Bir kart seç → İstatistikler güncellenir
4. Continue → Sonraki düello

---

## 🔧 Entegrasyon Kılavuzu

### Eski Controllerlara Geçiş

Eğer mevcut `PlayerDuelController` veya `EnemyDuelController` kullanıyorsanız:

1. **Yeni component ekle**:
   - `PlayerDuelControllerDirectional` ekle
   - `DirectionalCombatSystem` ekle
   - Eski controller'ı devre dışı bırak (disable, silme!)

2. **Referansları taşı**:
   - Focus ayarları → Yeni controller'a kopyala
   - Animator → Aynı kalır
   - Health → Aynı kalır

3. **Test**:
   - Arrow keys ile yön değiştirmeyi test et
   - UI'da yön göstergelerini kontrol et

### Animator Setup

**Gerekli Parameters:**
- `lightAttack` (Trigger)
- `heavyAttack` (Trigger)
- `parry` (Trigger)
- `dodge` (Trigger)
- `isMeditating` (Bool)
- `attackDirection` (Int) - 1=Up, 2=Down, 3=Left, 4=Right
- `defenseDirection` (Int)
- `hit` (Trigger)
- `die` (Trigger)

**Animation Events:**
- Attack animasyonları: `AttackStart()`, `AttackEnd()`

---

## 📊 Debug

**Console Logs:**
- `[Player] Attack direction: Up` - Yön değişiklikleri
- `[Enemy] Attacking from Down` - Düşman saldırı yönü
- `[Player] PARRY SUCCESS!` - Parry başarısı
- `[Stats] Speed upgraded! Level: 2` - Yükseltme uygulandı

**On-Screen Debug (F12 ile toggle):**
- Directional indicators (real-time)
- Combat system state (parrying, dodging, counter)
- Journey progress (duel 3/7)
- Upgrade levels

---

## 🐛 Troubleshooting

**Problem**: Yön değişmiyor
- **Çözüm**: `DirectionalCombatSystem` component'i eklemeyi unutmuş olabilirsiniz

**Problem**: Parry çalışmıyor
- **Çözüm**: Parry window çok kısa olabilir (0.25s → 0.35s deneyin)

**Problem**: Kamp ekranı açılmıyor
- **Çözüm**: `MusashiGameManager.OnDuelVictory()` metodunu düello bittiğinde çağırın

**Problem**: Ses yok
- **Çözüm**: `CombatAudioManager` component'i ekleyin ve audio clip'leri atayın

**Problem**: Slow-motion takılıyor
- **Çözüm**: `CombatEffectsManager.enableSlowMotion = false` yapın

---

## 📝 Gelecek Geliştirmeler

- [ ] Farklı düşman tipleri (agresif, defansif, balansed)
- [ ] Combo sistemi (3 saldırı → özel hareket)
- [ ] Special moves (odak tüketip güçlü saldırı)
- [ ] Stance system (farklı duruşlar farklı avantajlar)
- [ ] Final boss: Gölge Musashi (mirror match)
- [ ] Story elements (diyalog, cutscene'ler)
- [ ] Leaderboard (en hızlı tırmanış)

---

## 💡 İpuçları

1. **Odak Yönetimi**: Meditasyon risklidir! Sadece düşman yorgunken kullan
2. **Parry Timing**: Düşmanın saldırı animasyonunu ezberle
3. **Yön Okuma**: Düşmanın kılıç pozisyonuna bak
4. **Upgrade Stratejisi**: İlk 2 düello → Focus, sonra → Güç/Hız
5. **Counter Window**: Parry başarısında mutlaka saldır, bu pencere kısa!

---

## 🎨 Asset İhtiyaçları

### Audio:
- Sword swing sounds (light + heavy)
- Sword clash sounds
- Parry impact sound
- Dodge whoosh sound
- Hit grunt sounds
- Death sounds
- Meditation ambient

### Particles:
- Sword trail (Trail Renderer)
- Parry sparks (Particle System - gold)
- Hit sparks (Particle System - red)
- Block sparks (Particle System - blue)

### UI:
- Arrow sprites (4 yön)
- Card background (camp upgrade)
- Frame sprites (focus bars)

---

**Oluşturan**: Musashi Development Team
**Versiyon**: 1.0
**Tarih**: 2025
