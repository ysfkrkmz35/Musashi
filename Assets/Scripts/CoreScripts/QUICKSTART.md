# 🚀 Musashi - Hızlı Başlangıç Kılavuzu

## Unity'de İlk Adımlar

### 1️⃣ Unity'yi Aç

1. Unity Hub'dan **Musashi** projesini aç
2. Unity editörün açılmasını bekle (scriptler compile edilecek)
3. Console'da hata yoksa devam et

---

### 2️⃣ Otomatik Scene Oluştur

Unity menüsünden:

```
Musashi > Setup > Create MehmetTest01 Scene
```

Bu komut otomatik olarak oluşturur:
- ✅ Player (DirectionalCombatSystem ile)
- ✅ Enemy (AI ile)
- ✅ DirectionalUI Canvas
- ✅ CombatEffectsManager
- ✅ MusashiGameManager

**Scene kaydedildi**: `Assets/Scenes/MehmetTest01.unity`

---

### 3️⃣ Kamp Scene'i Oluştur

Unity menüsünden:

```
Musashi > Setup > Create MehmetTest02 (Camp) Scene
```

Bu komut oluşturur:
- ✅ Camp Canvas
- ✅ 3 Upgrade Buttons (Hız/Güç/Odak)
- ✅ CampUpgradeSystem

**Scene kaydedildi**: `Assets/Scenes/MehmetTest02.unity`

---

## 4️⃣ MehmetTest01 Scene'ini Tamamla

**Hierarchy'de Player'ı seç:**

### A) Animator Ekle
1. **Animator** component'i zaten var
2. **Controller** ata:
   - Mevcut animator controller'ınızı atayın
   - VEYA yeni bir tane oluşturun

### B) Animator Parameters Ekle
Animator window'da (Window > Animation > Animator):

**Parameters:**
```
lightAttack      (Trigger)
heavyAttack      (Trigger)
parry            (Trigger)
dodge            (Trigger)
isMeditating     (Bool)
attackDirection  (Int)
defenseDirection (Int)
hit              (Trigger)
die              (Trigger)
```

### C) Animation Events Ekle
Saldırı animasyonlarına:
1. Animation window'u aç
2. Saldırı animasyonunu seç
3. **Event** ekle (timeline'da click):
   - Animasyonun %20'sinde: `AttackStart`
   - Animasyonun %60'ında: `AttackEnd`

### D) Hitbox Collider Ayarla
**Player > WeaponPoint > Hitbox:**
- BoxCollider zaten var
- **Is Trigger**: ✅ True
- **Size**: (1, 0.2, 0.2) - kılıç boyutuna göre ayarla

---

**Hierarchy'de Enemy'yi Seç:**

Aynı adımları tekrarla:
- ✅ Animator + Controller
- ✅ Parameters
- ✅ Animation Events
- ✅ Hitbox Collider

---

### 5️⃣ UI'ı Tamamla

**Hierarchy'de DirectionalUI Canvas'ı seç:**

#### A) Arrow Images Ata

**PlayerPanel > UpArrow:**
1. Image component'i var
2. **Sprite**: Ok sprite'ı ata (yukarı bakan)
3. Renk: Gri (zaten ayarlı)

**Aynısını şunlar için tekrarla:**
- PlayerPanel > DownArrow
- PlayerPanel > LeftArrow
- PlayerPanel > RightArrow
- EnemyPanel > UpArrow, DownArrow, LeftArrow, RightArrow

#### B) DirectionalIndicatorUI Component'i Ayarla

**DirectionalUI Canvas** GameObject'inde:
1. **DirectionalIndicatorUI** component'i seç
2. **Inspector'da drag & drop**:

```
Player Up Arrow    → PlayerPanel/UpArrow (Image)
Player Down Arrow  → PlayerPanel/DownArrow (Image)
Player Left Arrow  → PlayerPanel/LeftArrow (Image)
Player Right Arrow → PlayerPanel/RightArrow (Image)

Enemy Up Arrow     → EnemyPanel/UpArrow (Image)
Enemy Down Arrow   → EnemyPanel/DownArrow (Image)
Enemy Left Arrow   → EnemyPanel/LeftArrow (Image)
Enemy Right Arrow  → EnemyPanel/RightArrow (Image)
```

---

### 6️⃣ Camera Ayarla

**Main Camera'yı seç:**

**CombatEffectsManager** GameObject'inde:
- **Main Camera**: Main Camera'yı drag & drop

**Pozisyon ayarla:**
```
Position: (1.25, 2, -10)
Rotation: (0, 0, 0)
```

Bu hem Player'ı hem Enemy'yi görür.

---

### 7️⃣ FocusBar Ekle (Opsiyonel)

Eğer odak barı göstermek istiyorsan:

**Canvas'a sağ tık > UI > Slider**

**İki slider oluştur:**
1. **PlayerFocusBar**: Sol üstte
2. **EnemyFocusBar**: Sağ üstte

**Inspector'da:**
- Min Value: 0
- Max Value: 100
- Value: 100

**FocusBar.cs script'i zaten mevcut** (eski sistemden), otomatik çalışacak.

---

## 🎮 TEST ET!

### Play Butonuna Bas!

**Kontroller:**

**Saldırı:**
- **Arrow Keys** (↑↓←→): Saldırı yönü seç
- **Mouse Sol**: Hafif saldırı
- **Mouse Sağ**: Ağır saldırı

**Savunma:**
- **WASD**: Savunma yönü seç
- **Shift**: Parry
- **Space**: Dodge

**Odak:**
- **R (basılı tut)**: Meditasyon

---

### ✅ Çalışıyor mu Kontrol Et

1. **Arrow tuşlarına** bas → Console'da `[Player] Attack direction: Up` görmeli
2. **WASD** bas → Console'da `[Player] Defense direction: Left` görmeli
3. **UI'da oklar** renk değiştirmeli (Kırmızı = saldırı, Mavi = savunma)
4. **Enemy** otomatik saldırmalı → Console'da `[Enemy] Attacking from Down`
5. **Parry** dene → `[Player] PARRY SUCCESS!` görmelisin

---

## 🐛 Sorun Giderme

### "DirectionalCombatSystem not found"
**Çözüm**: Unity'yi yeniden başlat (scriptleri compile etmesi için)

### UI okları görünmüyor
**Çözüm**:
1. Canvas > Render Mode: Screen Space - Overlay
2. Arrow Image'lara sprite atamayı unutma

### Enemy saldırmıyor
**Çözüm**:
1. Enemy > Player referansını kontrol et
2. Enemy > Think Every: (1.2, 2.0) olmalı

### Animation çalışmıyor
**Çözüm**:
1. Animator Controller atandı mı?
2. Parameters eklenmiş mi?
3. Animation Events eklenmiş mi?

### Collision çalışmıyor
**Çözüm**:
1. Hitbox > BoxCollider > Is Trigger ✅
2. Player ve Enemy'de Rigidbody olmalı (opsiyonel ama önerilir)

---

## 🎯 Sonraki Adımlar

### Kamp Sistemini Test Et

1. **MehmetTest02** scene'ini aç
2. **Camp Canvas > CampUpgradeSystem** component'inde:
   - Speed Button → SpeedButton drag & drop
   - Power Button → PowerButton drag & drop
   - Focus Button → FocusButton drag & drop
   - Continue Button → ContinueButton drag & drop
3. Play'e bas
4. Kartlardan birine tıkla
5. "DEVAM ET" butonu görünmeli

### Build Settings Ekle

**File > Build Settings:**
1. **Add Open Scenes**: MehmetTest01
2. **Add Open Scenes**: MehmetTest02
3. MehmetTest01'i ilk sıraya koy (index 0)

### GameManager'ı Ayarla

**MehmetTest01** scene'inde:

**MusashiGameManager** GameObject'inde:
- Duel Scene Name: `MehmetTest01`
- Camp Scene Name: `MehmetTest02`
- Total Duels: 7

---

## 🎨 Görsel İyileştirmeler

### Sprite'lar Ekle
Arrow Image'lar için:
- Kendin çiz VEYA
- Unity Asset Store'dan ücretsiz UI pack indir

### Particle Effects
CombatEffectsManager'a ata:
- Parry Sparks Prefab
- Hit Sparks Prefab
- Sword Trail Prefab

### Audio Clips
CombatAudioManager'a ata:
- Sword swing sounds
- Clash sounds
- Parry sounds

---

## 📖 Detaylı Dokümantasyon

Tam setup ve kod açıklaması için:
- [README.md](README.md) - Sistem detayları
- [CoreScripts/Combat/](Combat/) - Dövüş sistemi kodları
- [CoreScripts/Progression/](Progression/) - Yükseltme sistemi kodları

---

## 💬 Debug Console

Play mode'da göreceğin log'lar:

```
[Player] Attack direction: Up
[Player] Defense direction: Left
[Enemy] Attacking from Down
[Player] PARRY SUCCESS! Counter window opened!
[Enemy] Hit player!
[Stats] Speed upgraded! Level: 2
[GameManager] Duel 3/7
```

---

**Hazır! Artık Musashi'nin yönlü dövüş sistemi çalışıyor! ⚔️🎌**

Sorularınız varsa:
- Console log'ları kontrol edin
- README.md'ye bakın
- Script içindeki comment'leri okuyun
