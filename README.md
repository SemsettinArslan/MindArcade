# 🧠 MindArcade

**Yapay Zeka Destekli Bilişsel Oyun Platformu Bitirme Projesi**

MindArcade, bilişsel beceri gelişimini hedefleyen, **Gemini AI** tabanlı adaptif zorluk sistemi ve **Firebase** altyapılı klinik takip özellikleriyle donatılmış bir Unity oyun platformudur. Stroop Etkisi gibi nöropsikolojik prensipler üzerine kurulu üç farklı mini oyun içerir.

> 🎓 **Bitirme Projesi** — Unity 6 (6000.3.7f1) · C# · Gemini 2.5 Flash · Firebase Realtime Database

---
**Video Linki**
https://www.youtube.com/watch?v=ZzdYfuTI9BA

## 📑 İçindekiler

- [Özellikler](#-özellikler)
- [Oyunlar](#-oyunlar)
  - [FocusRunner — Seçici Dikkat](#-focusrunner--seçici-dikkat)
  - [MemoryMatch — Görsel Bellek](#-memorymatch--görsel-bellek)
  - [SequenceCubes — Ardışıl Hafıza](#-sequencecubes--ardışıl-hafıza)
- [Mimari](#-mimari)
- [Teknoloji Yığını](#-teknoloji-yığını)
- [Proje Yapısı](#-proje-yapısı)
- [Kurulum](#-kurulum)
- [Yapılandırma](#-yapılandırma)

---

## ✨ Özellikler

| Özellik | Açıklama |
|---|---|
| 🤖 **LLM Tabanlı Adaptif Zorluk** | Gemini 2.5 Flash API ile oyuncu performansına göre gerçek zamanlı zorluk ayarlama |
| 🧪 **Stroop Etkisi Entegrasyonu** | Tüm oyunlarda bilişsel yükü artırmak için renk-kelime uyumsuzluğu mekaniği |
| 🔥 **Firebase Bulut Altyapısı** | Hasta/oyuncu verilerini Firebase Realtime Database üzerinde saklama ve takip |
| 👨‍⚕️ **Doktor Portalı** | Klinisyenler için hasta yönetimi, performans izleme ve HTML rapor dışa aktarma |
| 🔐 **Hasta Giriş Sistemi** | Firebase destekli kimlik doğrulama ile hasta bazlı profil yönetimi |
| 📊 **Oyuncu Profil Sistemi** | Oyun bazlı kümülatif istatistikler (doğruluk, skor, seviye, süre, hata) |
| 🎨 **Low-Poly 3D Sanat Stili** | Medieval Market ortamı, Wizard karakter ve stilize UI tasarımı |
| 🔊 **Ses Yönetimi** | ScriptableObject tabanlı SoundLibrary ile merkezi ses efekti yönetimi |
| 🎬 **Sahne Geçiş Efektleri** | ScreenFader ile fade-in/fade-out sahne geçişleri ve LLM pre-fetch desteği |
| 📱 **Çapraz Platform** | Mobil (Android) ve PC (Windows) render pipeline desteği (URP) |

---

## 🎮 Oyunlar

### 🏃 FocusRunner — Seçici Dikkat

Stroop Etkisi tabanlı bir **sonsuz koşu** (endless runner) oyunudur. Oyuncu bir koridorda koşarken, önüne çıkan **Stroop nesnelerinin** (renk yazıları üzerinde farklı renklerle yazılmış kelimeler) doğru olanlarını toplar, yanlış olanlardan kaçınır.

**Mekanikler:**
- 🎯 **Kural Sistemi** — Her turda dinamik olarak değişen kurallar (örn: "Sadece kırmızı yazılı olanları topla")
- ⚡ **Hız & Zorluk Artışı** — LLM tarafından belirlenen `speedMultiplier` ile koşu hızı ve engel sıklığı ayarlanır
- ❤️ **Can Sistemi** — Yanlış toplamalar can kaybettirir; canlar bitince oyun sona erer
- 🧩 **Stroop Nesneleri** — Renk adı ve görüntülenen rengi farklı olan uyaranlar (bilişsel inhibisyon testi)
- 📏 **Level Generator** — Prosedürel olarak engel ve Stroop nesneleri yerleştiren seviye üreteci

**Anahtar Scriptler:** `FocusRunnerManager`, `PlayerController`, `DifficultyManager`, `LevelGenerator`, `RuleManager`, `StroopObject`

---

### 🃏 MemoryMatch — Görsel Bellek

Klasik hafıza kartı eşleştirme oyununun **Stroop varyasyonu**. Kartlar renkli semboller taşır ve Stroop moduna göre eşleştirme kriteri değişir.

**Mekanikler:**
- 🎴 **Kart Sistemi** — `CardData` (ScriptableObject tabanlı) ile tanımlanan renkli sembol kartları
- 🔄 **Stroop Eşleştirme Modları** — Renk-renk, kelime-kelime veya çapraz eşleştirme kuralları
- ⏱️ **Tur Zamanlayıcı** — Her açılmış kart çifti için geri sayım; süre dolunca yanlış sayılır
- 🎨 **Renk Paleti** — `MemoryColorPalette` ScriptableObject ile özelleştirilebilir renk setleri
- 📐 **Board Manager** — Grid boyutunu zorluk seviyesine göre dinamik olarak ayarlar
- 🃏 **Deck Generator** — Stroop moduna uygun kart destesi oluşturan algoritmik üreteci
- ✅ **Match Validator** — Aktif Stroop kuralına göre doğru/yanlış eşleşme kontrolü

**Anahtar Scriptler:** `MemoryGameManager`, `BoardManager`, `Card`, `DeckGenerator`, `MatchValidator`, `MemoryColorPalette`, `TurnTimer`, `StroopMatchMode`

---

### 🟦 SequenceCubes — Ardışıl Hafıza

**Simon Says** tarzında bir ardışıl hafıza oyunudur. Renkli küpler belirli bir sırayla yanar ve oyuncu bu sırayı tekrarlamalıdır. Stroop kuralları sırayı karmaşıklaştırır.

**Mekanikler:**
- 🔢 **Sıra Uzunluğu** — Her turda artan küp sırası (LLM tarafından ayarlanabilir)
- 🎲 **Kural Çeşitliliği** — Strategy Pattern ile uygulanan sıra kuralları:
  - `NormalRule` — Sıra olduğu gibi tekrarlanır
  - `ReverseRule` — Sıra ters çevrilerek tekrarlanır
  - `EvenIndicesRule` — Sadece çift indeksli elemanlar tekrarlanır
  - `OddIndicesRule` — Sadece tek indeksli elemanlar tekrarlanır
- ❤️ **Can Sistemi** — `LivesManager` ile sınırlı hata hakkı
- 💡 **Animasyonlu Küpler** — `SequenceCube` ile görsel yanma/sönme animasyonları
- 🎵 **Sequence Player** — Küp sırasını görsel + işitsel olarak oynatan bileşen

**Anahtar Scriptler:** `SequenceGameManager`, `SequenceGenerator`, `SequenceCube`, `SequencePlayer`, `SequenceInputHandler`, `LivesManager`, `ISequenceRule`

---

## 🏗 Mimari

```
┌────────────────────────────────────────────────────┐
│                    CORE LAYER                       │
│  GameEvents (Event Bus) · SceneNavigator · Interfaces│
├────────────┬────────────────┬──────────────────────┤
│   LLM      │    Firebase    │        UI            │
│ GeminiClient│ FirebaseClient │ MainMenuController   │
│ LlmDifficulty│ (REST API)  │ LoginUIController    │
│ Manager    │               │ DoctorPortalPanel     │
│ PlayerProfile│              │ UIPanel · ScreenFader │
│ Manager    │               │ GameOverPanel · Pause │
├────────────┴────────────────┴──────────────────────┤
│                  GAME MODULES                       │
│  ┌──────────┐  ┌───────────┐  ┌────────────────┐  │
│  │FocusRunner│  │MemoryMatch│  │SequenceCubes   │  │
│  │ Manager  │  │ Manager   │  │ Manager        │  │
│  │ Difficulty│  │ Board     │  │ Generator      │  │
│  │ LevelGen │  │ DeckGen   │  │ Rules(Strategy)│  │
│  │ Player   │  │ Card      │  │ Cube · Player  │  │
│  │ Stroop   │  │ Validator │  │ InputHandler   │  │
│  └──────────┘  └───────────┘  └────────────────┘  │
├────────────────────────────────────────────────────┤
│                SUPPORT SYSTEMS                      │
│  AudioManager · SoundLibrary · ReportExporter       │
│  AnimatedButton · UISoundBinder · SettingsPanel      │
└────────────────────────────────────────────────────┘
```

### Tasarım Desenleri

| Desen | Kullanım Yeri |
|---|---|
| **Singleton** | `SceneNavigator`, `LlmDifficultyManager`, `FirebaseClient`, `AudioManager`, `ScreenFader` |
| **Event Bus** | `GameEvents` — Tüm modüller arası gevşek bağlı (loosely coupled) iletişim |
| **Strategy Pattern** | `ISequenceRule` — SequenceCubes kural varyasyonları (`NormalRule`, `ReverseRule`, vb.) |
| **Interface Segregation** | `IGameManager`, `IDifficultyProvider`, `IScoreTracker` — Ortak kontratlar |
| **ScriptableObject Data** | `CardData`, `MemoryColorPalette`, `SoundLibrary` — Veri odaklı tasarım |
| **Observer Pattern** | `GameEvents` — C# event/delegate tabanlı bildirim sistemi |
| **MVC-benzeri** | Her oyun: Manager (Controller) + UI (View) + Data (Model) ayrımı |

---

## 🛠 Teknoloji Yığını

| Katman | Teknoloji | Versiyon |
|---|---|---|
| **Oyun Motoru** | Unity 6 (URP) | 6000.3.7f1 |
| **Yapay Zeka** | Google Gemini API | 2.5 Flash |
| **Veritabanı** | Firebase Realtime Database | REST API |
---

## 📂 Proje Yapısı

```
MindArcade/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/                          # Çekirdek sistemler
│   │   │   ├── Audio/                     # AudioManager, SoundLibrary, UISoundBinder
│   │   │   ├── Firebase/                  # FirebaseClient (REST)
│   │   │   ├── Llm/                       # GeminiClient, LlmDifficultyManager
│   │   │   │                              # PlayerProfileManager, LlmDataTypes
│   │   │   ├── UI/                        # MainMenuController, LoginUIController
│   │   │   │                              # DoctorPortalPanel, UIPanel, ScreenFader
│   │   │   │                              # GameOverPanel, PausePanel, AnimatedButton
│   │   │   │                              # SettingsPanelController, SceneLoadButton
│   │   │   ├── Utils/                     # ReportExporter (HTML rapor oluşturma)
│   │   │   ├── GameEvents.cs              # Merkezi event bus
│   │   │   ├── SceneNavigator.cs          # Sahne geçiş yöneticisi
│   │   │   ├── IDifficultyProvider.cs     # Zorluk sağlayıcı arayüzü
│   │   │   ├── IGameManager.cs            # Oyun yöneticisi arayüzü
│   │   │   └── IScoreTracker.cs           # Skor takip arayüzü
│   │   ├── FocusRunner/                   # Seçici dikkat oyunu
│   │   ├── MemoryMatch/                   # Görsel bellek oyunu
│   │   └── SequenceCubes/                 # Ardışıl hafıza oyunu
│   │       └── Rules/                     # ISequenceRule implementasyonları
│   ├── Scenes/
│   │   ├── MainMenu.unity                 # Ana menü sahnesi
│   │   ├── FocusRunner.unity              # FocusRunner oyun sahnesi
│   │   ├── MemoryMatch.unity              # MemoryMatch oyun sahnesi
│   │   └── SequenceCubes.unity            # SequenceCubes oyun sahnesi
│   ├── Prefabs/                           # UI ve oyun prefab'ları
│   ├── Art/                               # Görseller, fontlar, sesler
│   ├── SObject/                           # ScriptableObject asset'leri
│   ├── Resources/                         # Runtime yüklenen kaynaklar (API key vb.)
│   ├── Settings/                          # URP render pipeline ayarları
│   └── Plugins/                           # DOTween (Demigiant)
├── Packages/                              # Unity paket bağımlılıkları
└── ProjectSettings/                       # Unity proje ayarları
```

---

## 🚀 Kurulum

### Gereksinimler

- **Unity 6** (6000.3.7f1 veya üzeri)
- **Gemini API Anahtarı** (Google AI Studio'dan alınabilir)
- **Firebase Realtime Database** projesi (opsiyonel — hasta takibi için)
- İnternet bağlantısı (LLM ve Firebase istekleri için)

### Adımlar

1. **Repoyu klonlayın:**
   ```bash
   git clone https://github.com/SemsettinArslan/MindArcade.git
   ```

2. **Unity Hub'da açın:**
   - Unity Hub → `Open` → `MindArcade` klasörünü seçin
   - Unity 6 (6000.3.7f1) sürümünün yüklü olduğundan emin olun

3. **Gemini API Anahtarını yapılandırın:**
   - `Assets/Resources/` klasörüne `gemini_key.txt` dosyası oluşturun
   - API anahtarınızı bu dosyaya yapıştırın
   - ⚠️ Bu dosya `.gitignore` ile korunmaktadır, repoya pushlanmaz

4. **Sahneyi çalıştırın:**
   - `Assets/Scenes/MainMenu.unity` sahnesini açın
   - ▶️ Play butonuna basın

---

## ⚙️ Yapılandırma

### Gemini API

API anahtarı iki şekilde atanabilir:

| Yöntem | Açıklama |
|---|---|
| **Dosya (Önerilen)** | `Assets/Resources/gemini_key.txt` dosyasına API anahtarını yazın |
| **Inspector** | `LlmDifficultyManager` bileşeninin `Api Key` alanına doğrudan girin |

> **Not:** `gemini_key.txt` dosyası `.gitignore` içinde tanımlıdır, güvenle kullanabilirsiniz.

### Firebase

`FirebaseClient` bileşeninde Inspector üzerinden `Database URL` alanını kendi Firebase Realtime Database URL'nizle değiştirin:

```
https://proje-id-default-rtdb.europe-west1.firebasedatabase.app/
```

### Doktor Portalı Girişi

Varsayılan doktor giriş bilgileri:
- **Kullanıcı Adı:** `test`
- **Şifre:** `test123`

---

## ⚠️ Yasal Uyarı / Disclaimer
Bu proje, tıbbi teşhis doğruluğu taahhüt etmeyen bir çalışmasıdır. Sunulan analizler ve öneriler hekim muayenesi yerine geçmez; sadece bilgilendirme amaçlıdır. Sağlık problemleriniz için her zaman uzman bir doktora başvurmalısınız.

---
