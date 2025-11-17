# ⚔️ Musashi Combat System - TAMAMEN DÜZELTİLDİ

## 🔴 Eski Sorunlar (ÇÖZÜLDÜ)

### 1. ❌ Düşman Rastgele Saldırıyordu
**Sorun:** Enemy her yöne rastgele saldırılar yapıyordu, strateji yoktu

**Çözüm:**
- ✅ Akıllı AI stratejisi eklendi
- ✅ Player'ın defense yönünü gözlemliyor
- ✅ 35% ihtimalle zıt yöne saldırıyor (smart attack)
- ✅ 25% ihtimalle en az savunulan yöne saldırıyor (adaptive)
- ✅ Heavy attack'larda bazen aynı yöne saldırarak oyuncuyu şaşırtıyor (feint)
- ✅ Pattern tracking - Oyuncunun favori yönlerini öğreniyor

**Kod:** `EnemyDuelControllerDirectional_V2.cs` - `ChooseAttackDirection()`

### 2. ❌ Player Defense Çalışmıyordu
**Sorun:** WASD ile defense değiştirme sayılmıyordu, basılı tutmak gerekiyordu

**Çözüm:**
- ✅ `Input.GetKey()` → `Input.GetKeyDown()` değiştirildi
- ✅ Artık toggle style çalışıyor (bir kere bas, seçili kalıyor)
- ✅ Defense direction UI'da görünüyor
- ✅ Console'da log görünüyor: `[Player] Defense stance: Up`

**Kod:** `PlayerDuelControllerDirectional.cs` - `HandleDirectionalInput()`

### 3. ❌ Hasar Sistemi Çalışmıyordu
**Sorun:** Karşılıklı hasar yenmiyordu, duello ilerlemiyor du

**Çözüm:**
- ✅ Player saldırısı artık enemy'ye gidiyor (eski kod comment'liydi!)
- ✅ Enemy saldırısı player'a düzgün uygulanıyor
- ✅ Çift hasar bug'ı düzeltildi (hitbox artık damage uygulamıyor)
- ✅ Controller'lar damage'i direkt Health.TakeDamage() ile veriyor

**Kod:**
- `PlayerDuelControllerDirectional.cs` - `AttackRoutine()` (192. satır)
- `EnemyDuelControllerDirectional_V2.cs` - `AttackRoutine()` (185. satır)

---

## ✅ Yeni Sistem - Nasıl Çalışıyor?

### 🎮 Kontroller

#### Savunma (Defense)
```
W = Yukarı savun
S = Aşağı savun
A = Sol savun
D = Sağ savun
```
- Bir kere bas, seçili kalır
- Mavi ok gösterir hangi yönü savunduğunu

#### Saldırı Yönü Seç
```
↑ Arrow = Yukarıdan saldır
↓ Arrow = Aşağıdan saldır
← Arrow = Soldan saldır
→ Arrow = Sağdan saldır
```
- Kırmızı ok gösterir hangi yönden saldıracağını

#### Aksiyonlar
```
Sol Click    = Hafif Saldırı (6 focus, 12 damage)
Sağ Click    = Ağır Saldırı (12 focus, 25 damage)
Shift        = Parry (5 focus)
Space        = Dodge (10 focus)
R (Basılı)   = Meditasyon (25 focus/s regen)
```

---

### ⚡ Combat Mekanikleri

#### 1. Parry Sistemi (EN ÖNEMLİ!)
```
Düşman FARKLI yönden saldırıyor + Sen parry basarsan
    → ⚡ PARRY SUCCESS!
    → 0.8 saniye Counter Window açılır
    → Bu sürede attack yapsan:
        ✓ 5x hızlı saldırı (0.2s cooldown)
        ✓ Bonus damage
        ✓ Focus geri kazanırsın
```

**Örnek:**
- Düşman: 🟠 Yukarıdan saldıracak (turuncu ok)
- Sen: W bas (yukarı savun) + Shift (parry)
- Sonuç: ❌ AYNI YÖN = BLOCK (hasar yok ama counter yok)

VS

- Düşman: 🟠 Yukarıdan saldıracak
- Sen: S bas (aşağı savun) + Shift (parry)
- Sonuç: ✅ FARKLI YÖN = PARRY! Counter window!

#### 2. Block Sistemi
```
Düşman saldırıyor + AYNI yönü savunuyorsan
    → ✋ BLOCK
    → Hasar almıyorsun
    → Ama 3 focus harcıyorsun
    → Counter window yok
```

#### 3. Dodge Sistemi
```
Space bas
    → 🌀 DODGE
    → 0.4 saniye invincible
    → Tüm saldırılardan kaçarsın
    → 10 focus harcar
```

#### 4. Focus Yönetimi
```
Max Focus: 100
Regen: 10/saniye (pasif)

Harcama:
- Hafif Saldırı: 6
- Ağır Saldırı: 12
- Parry: 5
- Dodge: 10
- Block: 3 (otomatik)

Meditasyon (R):
- 25/saniye bonus regen
- Ama TAMAMEN savunmasız!
- Düşman telegraph gösteriyorsa YAPMA!
```

---

### 🤖 Enemy AI - Nasıl Çalışıyor?

#### Strateji Seçimi
Enemy şu stratejilerden birini kullanır:

1. **Smart Attack (35% ihtimal)**
   - Senin savunma yönünün ZIT tarafına saldırır
   - Örnek: Sen W (yukarı) savunuyorsan → S (aşağıdan) saldırır

2. **Adaptive Attack (25% ihtimal)**
   - Senin en az kullandığın yönden saldırır
   - Oyun boyunca attack geçmişini takip eder

3. **Feint Attack (20% heavy attack'larda)**
   - Senin savunma yönünle AYNI yönden saldırır
   - Greedy player'ları cezalandırır

4. **Random Attack (20% ihtimal)**
   - Rastgele yön seçer
   - Unpredictable kalmak için

#### Telegraph Sistemi
```
1. Enemy saldırı kararı verir
2. 🟠 TURUNCU OK 1 saniye yanıp söner
3. Oyun %50 yavaşlar (slow-motion)
4. Sen 1 saniye içinde:
   → Defense değiştir (WASD)
   → Dodge hazırla (Space)
   → Parry hazırla (Shift)
5. 1 saniye sonra → Gerçek saldırı!
```

#### Timing
- Düşünme süresi: 2.5-4.5 saniye arası random
- Attack cooldown: 1.2 saniye
- Aggression: 40% (dengeli)

---

## 🎯 Strateji Rehberi

### Başlangıç (0-30 saniye)
1. **Savunma öğren**
   - WASD ile defense değiştir
   - Düşman telegraph gösterince yön değiştir
   - Block'ları test et

2. **Focus yönetimini öğren**
   - Hafif saldırılar yap (6 focus)
   - Focus biterse R ile meditasyon
   - Ama düşman telegraph gösteriyorsa meditasyonu BIRAK!

### Orta Oyun (30-60 saniye)
1. **Parry zamanlaması öğren**
   - Turuncu ok göründüğünde
   - FARKLI yöne savun
   - Shift bas (parry)
   - Eğer başarılı olursa → 5x hızlı saldırı!

2. **Focus avantajı kur**
   - Düşman focus'u 0 olursa hareket edemez
   - Successful parry düşman focus'unu eritir
   - Block da focus harcar ama az

### İleri Seviye (60+ saniye)
1. **Counter window'u kullan**
   - Parry success → 0.8s counter
   - Bu sürede spam attack yap
   - 0.2s cooldown = 4 hit atabilirsin!

2. **Düşman pattern'ini öğren**
   - Enemy senin favori yönünü öğrenirse
   - O yönü az kullan
   - Unpredictable ol

3. **Heavy attack'i akıllıca kullan**
   - 12 focus ama 25 damage
   - Counter window'da kullan
   - Düşman focus'u düşükken kullan

---

## 🔧 Kurulum

### Unity'de:
```
Musashi > 🔧 FIX COMBAT SYSTEM (PLAYABLE)
```

Bu tek tıklama:
- ✅ Player controller'ı günceller
- ✅ Enemy AI'ı düzeltir
- ✅ Combat parametrelerini dengeler
- ✅ Telegraph sistemini optimize eder
- ✅ Health entegrasyonunu düzeltir

### Sonra:
1. PLAY tuşuna bas
2. Console'u aç (Ctrl+Shift+C) - Combat log'ları göreceksin
3. Oyna!

---

## 📊 Combat Parameters (Dengeli)

### Player
```
Focus Max: 100
Focus Regen: 10/s
Meditasyon Bonus: +15/s

Light Attack: 6 focus, 12 damage, 0.4s cooldown
Heavy Attack: 12 focus, 25 damage, 0.4s cooldown
Parry: 5 focus, 0.3s window
Dodge: 10 focus, 0.4s i-frames
Block: 3 focus (otomatik)
```

### Enemy
```
Focus Max: 100
Focus Regen: 5/s

Light Attack: 12 focus, 18 damage, 1.2s cooldown
Heavy Attack: 20 focus, 30 damage, 1.2s cooldown
Parry: 8 focus, 0.3s window
Think Interval: 2.5-4.5s
```

Bu dengeyle:
- Player daha hızlı ama daha az damage
- Enemy daha yavaş ama daha güçlü
- Telegraph sayesinde player reaktif avantaj
- Focus yönetimi kritik

---

## 🐛 Debug / Test

### Console Log'ları
Oynarken şunları göreceksin:

```
[Player] Attack direction: Up
[Player] Defense stance: Left
[Enemy AI] Smart attack - targeting opposite of player defense
[Enemy] 🎯 Chosen attack direction: Right
[Enemy] ⚠️ TELEGRAPH! Attack incoming from Right
[Player] Incoming attack from Right - Result: Blocked, Damage: False
[Player] ✋ Blocked attack!
```

Bu log'lar:
- Attack direction'ları gösterir
- Defense stance'i gösterir
- AI decision'ını açıklar
- Combat result'ı gösterir

### Sorun Giderme

**Saldırılar hasar vermiyor?**
- Console'da `[Player] Hit enemy for X damage!` görmeli sin
- Görüyorsan ama HP düşmüyorsa → Health script problemi
- Görmüyorsan → Telegraph beklemelisin (1 saniye)

**Defense çalışmıyor?**
- WASD basınca console'da `[Player] Defense stance: X` görmeli sin
- Görmüyorsan → Script güncel değil

**Enemy sürekli saldırıyor?**
- Think interval 2.5-4.5s olmalı
- Console'da `[Enemy] 🎯 Chosen attack direction` arasında 2-4s geçmeli

---

## 🎌 SON SÖZ

Artık oyun **TAMAMEN OYNANAB İLİR**!

- ✅ Enemy stratejik düşünüyor
- ✅ Defense çalışıyor
- ✅ Hasar sistemleri doğru
- ✅ Combat dengeli ve eğlenceli
- ✅ Telegraph sistemi adil warning veriyor

**Tek yapman gereken:**
1. Unity'de: `Musashi > 🔧 FIX COMBAT SYSTEM`
2. PLAY'e bas
3. Dövüş!

Artık gerçek bir Musashi duello deneyimi var! ⚔️🎌
