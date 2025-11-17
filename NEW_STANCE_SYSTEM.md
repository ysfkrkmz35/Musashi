# ⚔️ Yeni Stance-Based Combat System

## 🎯 Konsept

**ESKI SİSTEM:** Telegraph + reaction
**YENİ SİSTEM:** Simultaneous stance selection + commitment + punishment

Her iki taraf da:
1. Attack stance seçer (turuncu ok)
2. Defense stance seçer (mavi ok)  
3. Commit eder
4. Karşılıklı çarpışır!

## 🔥 Ana Mekanikler

### 1. Stance Selection (Hamle Seçimi)

**Player:**
- Arrow Keys (↑↓←→) = Attack stance seç (TURUNCU ok)
- WASD = Defense stance seç (MAVİ ok)
- İstediğin kadar değiştirebilirsin (commit etmeden önce)

**Enemy:**
- AI stratejik seçim yapar
- Player'ın defense'ını gözlemler
- Karşı hamle hazırlar

### 2. Commitment (Taahhüt)

**Attack Commit:**
- Sol Click (hafif) veya Sağ Click (ağır)
- Artık stance değiştiremezsin!
- Kırmızı yanıp söner (committed!)

**Aynı anda her iki taraf commit edebilir!**

### 3. Resolution (Çözüm)

Attack vs Defense:
```
AYNI YÖN = BLOCK
→ Hasar yok
→ -3 focus (defender)

FARKLI YÖN = HIT!
→ Tam hasar
→ -20 FOCUS CEZASI! (defender)
→ Bu çok önemli!
```

## 💀 Execution System (İnfaz)

**Focus < 10 = EXECUTABLE!**

Eğer focus'un 10'un altına düşerse:
- ☠️ "EXECUTABLE" uyarısı
- Enemy **EXECUTION ATTACK** yapabilir!
- Execution damage = **80 hasar!** (normal 20)
- Neredeyse instant kill!

**Kaçınma:**
- R bas (meditasyon) - ama savunmasızmış!
- Dodge kullan (10 focus)
- Block'la (ama -3 focus)

## 🎮 Oynanış Akışı

### Başlangıç (İlk 30s)
```
1. Arrow key ile attack yönü seç (↑ örneğin)
   → Turuncu ok ↑ görünür
   
2. WASD ile defense yönü seç (W örneğin)
   → Mavi ok ↑ görünür
   
3. Sol Click (hafif saldırı)
   → Kırmızı yanıp söner (committed!)
   → -8 focus
   
4. Enemy aynı anda kendi hamlesini yapar
   
5. Çarpışma!
   - Eğer enemy başka yönden geliyorsa → HIT! → -20 focus!
   - Eğer aynı yönden geliyorsa → BLOCK! → -3 focus
```

### Strateji (30-60s)

**Focus Yönetimi:**
- Her saldırı: -8 to -15 focus
- Her successful defense: -3 focus
- Her FAILED defense: -20 focus!
- Regen: +8/s (pasif)
- Meditasyon: +20/s (ama savunmasız!)

**Stance Mind Game:**
- Enemy senin defense'ını gözlemliyor
- Eğer hep W (yukarı) savunuyorsan
- Enemy aşağıdan (↓) saldıracak!
- **Unpredictable ol!**

**Parry Mekanikleri:**
- Shift = Active parry (farklı yön)
- Eğer düşman FARKLI yönden saldırıyorsa
- PARRY SUCCESS! → Counter window (1.2s)
- Bu sürede çok hızlı saldır!

### İleri Seviye (60s+)

**Execution Taktikleri:**

*Offensive:*
1. Enemy focus'unu aşağı çek
2. Sürekli saldır (her hit -20!)
3. Enemy < 10 focus olunca
4. Ağır saldırı (execution damage!)

*Defensive:*
1. Focus'unu koru!
2. Block > Miss (3 vs 20 focus)
3. Eğer <10 düşersen → R BAS (meditasyon)
4. Dodge kullan (escape)

**AI Davranışı:**
- Player < 30 focus → Daha agresif
- Player < 10 focus → EXECUTION MODE!
- Enemy < 30 focus → Daha defansif

## 📊 Focus Ekonomisi

### Maliyetler
```
Light Attack:   -8 focus
Heavy Attack:  -15 focus
Parry:          -5 focus
Dodge:         -10 focus

Block (success):    -3 focus
Failed Defense:    -20 focus!! (ÇOK PAHALI!)

Regen:          +8/s
Meditation:    +20/s (R basılı)
```

### Örnekler

**Senaryo 1: Block Chain**
```
Focus: 100
Attack (light): -8 → 92
Enemy blocks: you gain nothing
Enemy attacks, you block: -3 → 89
You attack, enemy blocks: -8 → 81
...
After 5 exchanges: ~50 focus (safe)
```

**Senaryo 2: Failed Defense Chain**
```
Focus: 100
Attack (light): -8 → 92
Enemy hits you (failed defense): -20 → 72!!
Enemy attacks again, you miss again: -20 → 52!!
Enemy attacks, you miss: -20 → 32!!
Enemy attacks, you miss: -20 → 12
Enemy attacks, you miss: -20 → EXECUTABLE!
☠️ DEAD
```

**Focus kritik!**

## 🎯 Kontroller (Özet)

```
=== STANCE SELECTION ===
↑↓←→  = Attack stance (turuncu)
WASD  = Defense stance (mavi)

=== ACTIONS ===
Sol Click  = Light Attack (8 focus, 15 dmg)
Sağ Click  = Heavy Attack (15 focus, 30 dmg)
Shift      = Parry (5 focus, counter window)
Space      = Dodge (10 focus, invincible)
R (basılı) = Meditasyon (+20 focus/s)
```

## ⚡ Yeni Sistemin Avantajları

**1. Mind Games:**
- Stance selection adds prediction layer
- Enemy learns your patterns
- You must adapt!

**2. Punishment:**
- Failed defense = -20 focus!
- Mistakes are EXPENSIVE
- Skill-based, not spam

**3. Execution Drama:**
- Comeback mechanic
- High risk/reward
- Intense finish moments

**4. No Spam:**
- Eski sistem: sürekli saldırı
- Yeni sistem: her saldırı önemli
- Focus yönetimi kritik

## 🔄 Eski vs Yeni

| Özellik | Eski | Yeni |
|---------|------|------|
| Tempo | Hızlı, sürekli | Taktiksel, düşünülü |
| Savunma | Telegraph sonrası react | Önceden stance seç |
| Ceza | Minimal | -20 focus! |
| Execution | Yok | Var (focus < 10) |
| Mind Game | Az | Çok! |
| Skill Cap | Orta | Yüksek |

## 🎌 Sonuç

**Yeni sistem:**
- ✅ Daha stratejik
- ✅ Mind games
- ✅ Her hamle önemli
- ✅ Execution drama
- ✅ Focus yönetimi kritik
- ✅ Skill-based

**Eski sistemi koruduk:**
- Directional combat (4 yön)
- Focus bars
- Parry mekanikleri
- Dodge i-frames

**Eklediklerimiz:**
- Stance selection phase
- Commitment system
- Heavy focus penalties (-20!)
- Execution at low focus
- More strategic AI

---

**Oyunu test et ve görüşlerini söyle! 🎮⚔️**
