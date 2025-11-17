# MUSASHI ULTIMATE COMBAT SYSTEM - FINAL

## 🎮 OYUN MEKANİĞİ (GAMEPLAY MECHANICS)

### Temel Kontroller (Basic Controls)
- **WASD** = Stance seçimi (Select stance)
- **Çift tuş basma** = Saldırıyı gerçekleştir (Commit attack)
  - Örnek: W → W = Üstten saldır (Attack from up)

### ⚡ SÜRPRİZ SALDIRI (SURPRISE ATTACK)
**EN ÖNEMLİ MEKANİK!**

1. Stance değiştir (WASD)
2. 0.4 saniye içinde aynı tuşa **iki kez** bas
3. **2x HASAR!** (Double damage!)

**Maliyetler:**
- Başarılı: -30 focus
- Başarısız (bloklandı): -30 focus + -20 ekstra ceza = -50 focus!

### 🛡️ BLOK SİSTEMİ - ODAK KAZANMA (BLOCK REWARDS)
**Savunma artık ÖDÜL veriyor!**

- **Normal blok:** +15 focus
- **Perfect blok** (sürpriz saldırı blokla): +25 focus!
- **Başarısız savunma:** -35 focus (AĞIR CEZA!)

### 💰 FOCUS EKONOMİSİ (FOCUS ECONOMY)

#### Maliyetler (Costs):
- Stance değişimi: -5 focus
- Normal saldırı: -20 focus
- Sürpriz saldırı: -30 focus
- Başarısız savunma: -35 focus

#### Kazanç (Gains):
- Normal blok: +15 focus ⭐
- Perfect blok: +25 focus ⭐⭐
- Pasif regen: +0.5/saniye (ÇOK YAVAŞ!)

#### Kritik Eşikler:
- **< 20 focus** = **EXECUTABLE!** ☠️
  - Savunma yok!
  - Bir sonraki vuruş = ölüm!

## 🤖 AKILLI DÜŞMAN AI (SMART ENEMY AI)

### Pattern Learning (Desen Öğrenme)
- Oyuncunun saldırı yönlerini takip eder
- Oyuncunun savunma alışkanlıklarını analiz eder
- En az savunulan yöne saldırır!

### Taktikler (Tactics)
- **Prediction:** %70 tahmin başarısı
- **Feint Attack:** Hızlı stance değişimleri ile oyuncuyu şaşırtma
- **Counter-Attack:** Vurulduktan sonra %60 karşı saldırı şansı
- **Aggression:** Dinamik saldırganlık seviyesi

## 🎯 STRATEJİ REHBERİ (STRATEGY GUIDE)

### Focus Yönetimi
1. **ASLA meditation yok!** Focus kazanmanın tek yolu BLOKLAmak!
2. Pasif regen ÇOK YAVAŞ (0.5/sn) - BLOĞA güven!
3. Her hareket focus tüketir - dikkatli ol!

### Sürpriz Saldırı Stratejisi
1. Stance değiştir → Düşmanın savunmasını oku
2. 0.4 saniye içinde çift tuş → 2x hasar!
3. RİSKLİ ama ödüllendirici!

### Savunma Stratejisi
1. Düşmanın saldırı desenlerini öğren
2. Doğru yönü savun → +15 focus kazan
3. Sürpriz saldırıları blokla → +25 focus!

### Executable Durumu
- Focus < 20 olduğunda SAVUNMAyı KAYBET!
- Tek kurtuluş: Saldırıyı BLOKLA ve focus kazan!
- Ya da ölüm... ☠️

## 🔧 TEKNİK DETAYLAR

### Setup
1. Unity'de: **Musashi → ⚔️ ULTIMATE COMBAT SETUP**
2. Oyuncuyu ve düşmanı sahneye ekle
3. Play!

### Önemli Değerler
```csharp
// Player & Enemy
focusMax = 100f
passiveRegenRate = 0.5f
attackCost = 20f
stanceChangeCost = 5f
failedDefenseCost = 35f
surpriseAttackCost = 30f
successfulBlockReward = 15f
perfectBlockReward = 25f
executionThreshold = 20f

// Surprise Attack
surpriseAttackWindow = 0.4f
surpriseAttackDamageMultiplier = 2.0f
surpriseAttackPenalty = 20f

// Enemy AI
predictionSkill = 0.7f
feintChance = 0.4f
counterAttackChance = 0.6f
```

## 📊 COMBAT FLOW

```
BAŞLANGIÇ: 100 focus
    ↓
Stance değiştir (-5 focus) = 95 focus
    ↓
0.4 saniye içinde çift tuş bas
    ↓
Sürpriz saldırı (-30 focus) = 65 focus
    ↓
[BAŞARILI] → Düşmana 40 hasar (2x)
[BAŞARISIZ] → Ekstra -20 ceza = 45 focus kaldı
    ↓
Düşman saldırır
    ↓
[BLOK] → +15 focus = 60 focus ⭐
[BAŞARISIZ] → -35 focus + hasar = 10 focus ☠️
    ↓
10 focus < 20 → EXECUTABLE!
```

## ⚠️ REMOVED FEATURES

### Kaldırılan Sistemler:
- ❌ **Meditation (R tuşu)** - Artık yok!
- ❌ **Dodge (Space)** - Kaldırıldı!
- ❌ **Block maliyeti** - Blok artık ödül veriyor!

### Yeni Paradigma:
- **ESKI:** Meditation ile focus kazan
- **YENİ:** Block ile focus kazan!
- **Sonuç:** Agresif savunma = kazanma stratejisi!

## 🏆 KAZANMA STRATEJİSİ

1. **Early Game:** Düşmanın desenlerini öğren
2. **Mid Game:** Sürpriz saldırılarla baskı yap
3. **Late Game:** Perfect bloklarla focus avantajı kazan
4. **Finish:** Düşmanı executable yap (< 20 focus) → Final vuruş!

---

**HAZIR! SAVAŞ BAŞLASIN! ⚔️**

**Not:** Tüm eski setup scriptleri kaldırıldı. Tek setup: `MusashiCombatSetup.cs`
