# 🎯 Musashi V2 - Telegraph System Update

## 📋 Yeni Özellikler

### ⚠️ Attack Telegraph System
Enemy saldırı yapmadan **1 saniye önce** saldırı yönünü gösterir!

**Nasıl Çalışır:**
1. Enemy saldırı kararı verir
2. ⚠️ **TELEGRAPH PHASE** başlar (1 saniye)
   - Enemy'nin saldırı oku **turuncu** renkte **parlar**
   - Zaman **%50 yavaşlar** (slow-motion)
   - Oyuncu hangi yönden saldırı geleceğini görür
   - **Hazırlanma zamanı!**
3. ⚔️ **ATTACK PHASE** başlar
   - Zaman normale döner
   - Enemy gerçek saldırıyı yapar

**Avantajlar:**
- ✅ Oyuncu tepki verebilir
- ✅ Doğru yöne savunma yapabilir
- ✅ Parry için hazırlanabilir
- ✅ Daha adil ve skill-based dövüş

---

## 🚀 OTOMATIK GÜNCELLEME - TEK TIK!

### Unity'de Menüden:
```
Musashi > Update to V2 (Telegraph System)
```

**Bu komut otomatik yapar:**
- ✅ Enemy controller'ı V2'ye günceller
- ✅ AttackTelegraphSystem component'i ekler
- ✅ UI'ı V2'ye günceller (telegraph renkleri)
- ✅ Tüm ayarları korur

**Süre:** 1 saniyeden az! ⚡

---

## 🎮 Oynanış Değişiklikleri

### Öncesi (V1):
```
Enemy → Aniden saldırır
Player → Refleksle karşılar (zor!)
```

### Sonrası (V2):
```
Enemy → Yön seçer
        ↓
⚠️ TELEGRAPH (1 saniye, slow-mo)
   Enemy oku turuncu parlar
   Player: "Yukarıdan geliyor!"
        ↓
Player → Yukarı savunma yapar
        ↓
⚔️ ATTACK
Enemy → Saldırır
Player → ✅ BLOCK / PARRY SUCCESS!
```

---

## 📊 Teknik Detaylar

### Yeni Script'ler (4 dosya)

1. **AttackTelegraphSystem.cs**
   - Telegraph mekanizmasını yönetir
   - Slow-motion efekti
   - Ses efektleri (opsiyonel)

2. **EnemyDuelControllerDirectional_V2.cs**
   - Eski controller'ın güncellenmiş versiyonu
   - Telegraph sistemini kullanır
   - `useTelegraph = true` (default)

3. **DirectionalIndicatorUI_V2.cs**
   - Telegraph görsellerini gösterir
   - Enemy okları turuncu renkte parlar
   - Pulse animasyonu

4. **MusashiSystemUpdate.cs** (Editor tool)
   - Otomatik güncelleme aracı
   - Tek tıkla V2'ye geçiş

---

## ⚙️ Ayarlar (Inspector'da)

### Enemy > AttackTelegraphSystem

**Telegraph Settings:**
- `Telegraph Duration`: **1.0s** (uyarı süresi)
- `Slow Motion Scale`: **0.5** (zaman %50 yavaşlar)
- `Enable Slow Motion`: **TRUE** (açık)

**Visual Feedback:**
- `Telegraph Color`: **Turuncu** (uyarı rengi)
- `Pulse Speed`: **8** (yanıp sönme hızı)

**Audio:**
- `Telegraph Sound`: Ses clip'i (opsiyonel)
- `Telegraph Volume`: **0.6**

### Enemy > EnemyDuelControllerDirectional_V2

**Telegraph Settings:**
- `Use Telegraph`: **TRUE** (açık/kapalı)

---

## 🎯 Kullanım

### Telegraph'ı Kapat (Harder Mode)
Daha zorluk istiyorsan:

1. **Enemy** GameObject'i seç
2. **EnemyDuelControllerDirectional_V2** component
3. **Use Telegraph** = **FALSE**

→ Eski gibi aniden saldırır!

### Telegraph Süresini Ayarla
Daha kısa/uzun uyarı istiyorsan:

1. **Enemy > AttackTelegraphSystem**
2. **Telegraph Duration** = **0.5** (daha kısa) veya **1.5** (daha uzun)

### Slow-Motion'ı Kapat
Sadece görsel uyarı, zaman yavaşlamasın:

1. **Enemy > AttackTelegraphSystem**
2. **Enable Slow Motion** = **FALSE**

---

## 🐛 Sorun Giderme

**"Update bulunamadı" hatası**
→ Unity'yi yeniden başlat (scriptler compile olsun)

**Telegraph çalışmıyor**
→ Enemy > EnemyDuelControllerDirectional_V2 > Use Telegraph = TRUE kontrol et

**UI okları parlamıyor**
→ DirectionalUI_Canvas > DirectionalIndicatorUI_V2 component var mı kontrol et

**Zaman donuyor**
→ AttackTelegraphSystem > Enable Slow Motion = FALSE yap

---

## 📝 Eski Sisteme Geri Dön (Rollback)

V2'yi beğenmediysen geri dönebilirsin:

1. **Enemy** GameObject'i seç
2. **EnemyDuelControllerDirectional_V2** component'ini **Remove**
3. **AttackTelegraphSystem** component'ini **Remove**
4. **Add Component** → **EnemyDuelControllerDirectional** (eski)
5. Ayarları tekrar gir

---

## 🎮 Test Etme

### PLAY'e Bas

**Ne göreceksin:**

1. Enemy düşünüyor... (1-2 saniye)
2. ⚠️ **TELEGRAPH!**
   - Enemy oku **turuncu** renkte **yanıp sönüyor**
   - Zaman **yavaşladı**
   - Console: `[Enemy] ⚠️ TELEGRAPH! Saldırı geliyor: Up`
   - **1 saniye** hazırlanma zamanın var!
3. ⚔️ **ATTACK!**
   - Zaman normale döndü
   - Enemy saldırdı
   - Sen hazırdın → **BLOCK** veya **PARRY SUCCESS!**

**Console Log'ları:**
```
[Enemy] Preparing attack from Up
[Enemy] ⚠️ TELEGRAPH! Saldırı geliyor: Up
[Player] Defense direction: Up
[Player] Blocked attack!
```

---

## 💡 İpuçları

1. **Telegraph sırasında savunma yönünü değiştir**
   - WASD ile enemy'nin gösterdiği yöne git

2. **Parry denemek için:**
   - Enemy'nin saldırı yönünü gör
   - **Farklı yön**e savunma yap
   - Shift bas (parry)
   - Telegraph bitince → **PARRY SUCCESS!**

3. **Dodge kullan:**
   - Telegraph gördün ama yön seçmek istemiyorsan
   - **Space** (dodge) bas
   - i-frame ile kaçın

4. **Meditasyon yapmak için ideal:**
   - Enemy telegraph'dayken
   - Zaman yavaş, hazırsın
   - **R** bas (meditasyon)
   - Odak doldur!

---

## 🎨 Görsel Feedback

### UI'da Göreceklerin:

**Normal Durum:**
- Player okları: Gri (pasif)
- Enemy okları: Gri (pasif)

**Telegraph Sırasında:**
- Player okları: Mavi (savunma yönün)
- Enemy okları: **TURUNCU PARLAMA** (saldırı yönü) ⚠️

**Parry Sırasında:**
- Player okları: Altın sarısı (parry aktif)

**Counter Window:**
- Player okları: Yeşil parlama (karşı saldırı zamanı!)

---

## 📈 Zorluk Seviyeleri

### Easy Mode (Başlangıç):
```
Telegraph Duration: 1.5s
Slow Motion Scale: 0.3 (zaman %70 yavaş)
Use Telegraph: TRUE
```

### Normal Mode (Varsayılan):
```
Telegraph Duration: 1.0s
Slow Motion Scale: 0.5 (zaman %50 yavaş)
Use Telegraph: TRUE
```

### Hard Mode (Zorluk):
```
Telegraph Duration: 0.5s
Slow Motion Scale: 0.7 (zaman %30 yavaş)
Use Telegraph: TRUE
```

### Expert Mode (Uzman):
```
Telegraph Duration: 0.5s
Slow Motion: FALSE (yok)
Use Telegraph: TRUE (sadece görsel uyarı)
```

### Hardcore Mode (Sekiro tarzı):
```
Use Telegraph: FALSE
(Hiç uyarı yok, saf refleks!)
```

---

## 🎯 Sonraki Özellikler (Gelecek)

V2'den sonra eklenebilecek:

- [ ] Combo telegraph (3 saldırı zinciri)
- [ ] Feint system (sahte telegraph)
- [ ] Telegraph'ı görmezden gelme cezası
- [ ] Perfect parry bonus (tam zamanında)
- [ ] Telegraph rengi düşman tipine göre değişir

---

**🎌 V2 Telegraph sistemi hazır! Güncellemeyi dene ve daha adil dövüşlerin tadını çıkar! ⚔️**

**Hemen güncelle:**
```
Musashi > Update to V2 (Telegraph System)
```
