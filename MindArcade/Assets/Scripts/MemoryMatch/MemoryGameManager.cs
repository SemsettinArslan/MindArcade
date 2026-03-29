using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MemoryMatch
{
    public enum StroopMatchMode
    {
        TextVsColor,   // Sahnede 8 metin, 8 blok renk arkaplanı olur. Metnin anlattığıyla bloğu eşleştirirsiniz.
        TextVsText     // Ekranda 16 kartın 16'sı da metindir (Ama hepsi sahte farklı renge boyalıdır). Aynı KELİMELERİ eşleştirirsiniz.
    }

    public class MemoryGameManager : MonoBehaviour
    {
        [Header("Oyun Modu")]
        public StroopMatchMode currentMatchMode = StroopMatchMode.TextVsColor;

        [Header("Referanslar")]
        [SerializeField] private Transform cardGridParent;
        [SerializeField] private Card cardPrefab;
        
        [Header("UI & Kural Referansları")]
        [SerializeField] private TMP_Text instructionText; 
        [SerializeField] private TMP_Text turnTimerText; 
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text gameOverTimeText;
        [SerializeField] private Button playAgainButton;

        [Header("Oyun Zorluğu")]
        [Tooltip("İlk kart seçildikten sonra ikinciyi bulmak için kaç saniye verilecek? (Örn: 5 saniye)")]
        [SerializeField] private float turnTimeLimit = 5.0f;
        
        // --- 24 FARKLI RENK HAVUZU (Tamamen Keskin Zıtlıklar ve Koyu/Açık Tonlar) ---
        private readonly string[] _colorNames = {
            "SİYAH", "KOYU GRİ", "AÇIK GRİ", "KIRMIZI", "BORDO", "PEMBE", "AÇIK PEMBE", "TURUNCU",
            "KAHVERENGİ", "SARI", "KREM", "YEŞİL", "KOYU YEŞİL", "AÇIK YEŞİL", 
            "MAVİ", "LACİVERT", "AÇIK MAVİ", "MOR", "KOYU MOR", "CAMGÖBEĞİ", 
            "TEN RENGİ", "HAKİ", "TURKUAZ", "BEJ"
        };
        
        private readonly Color[] _colorValues = {
            Color.black, new Color(0.3f, 0.3f, 0.3f), new Color(0.7f, 0.7f, 0.7f), Color.red, new Color(0.5f, 0f, 0f), new Color(1f, 0.1f, 0.6f), new Color(1f, 0.7f, 0.8f), new Color(1f, 0.5f, 0f),
            new Color(0.4f, 0.2f, 0f), Color.yellow, new Color(1f, 0.95f, 0.8f), Color.green, new Color(0f, 0.4f, 0f), new Color(0.6f, 1f, 0.2f),
            Color.blue, new Color(0f, 0f, 0.4f), new Color(0f, 0.8f, 1f), new Color(0.6f, 0f, 1f), new Color(0.3f, 0f, 0.3f), new Color(0f, 0.6f, 0.6f),
            new Color(1f, 0.8f, 0.6f), new Color(0.5f, 0.5f, 0f), new Color(0.2f, 1f, 0.8f), new Color(0.8f, 0.7f, 0.5f)
        };
        
        private readonly List<Card> _cards = new List<Card>(16); 
        
        private Card _firstCard;
        private Card _secondCard;
        
        private int _totalMatches;
        private int _currentMatches;
        private float _totalGameTime;
        
        private float _remainingTime;
        private bool _isTurnTimerRunning;

        private void Start()
        {
            if (playAgainButton != null) playAgainButton.onClick.AddListener(StartGame);
            
            StartGame();
        }

        private void Update()
        {
            if (gameOverPanel != null && !gameOverPanel.activeSelf)
            {
                _totalGameTime += Time.deltaTime;
            }

            if (_isTurnTimerRunning)
            {
                _remainingTime -= Time.deltaTime;
                
                if (turnTimerText != null)
                {
                    turnTimerText.text = "Kalan: " + _remainingTime.ToString("F1") + "s";
                }

                if (_remainingTime <= 0)
                {
                    HandleTimeOut(); // Süre Dolarsa Kartlar Karışır!
                }
            }
        }

        public void StartGame()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
                
            _totalGameTime = 0f;
            _totalMatches = 8;
            
            if (instructionText != null)
            {
                instructionText.text = currentMatchMode == StroopMatchMode.TextVsText 
                    ? "Aynı 'KELİME' Yazan Kartları Eşleştir!" 
                    : "Renk İsmi İle 'DÜZ RENK' Kartını Eşleştir!";
            }

            ClearBoard();
            SetupBoard();
        }

        private void ClearBoard()
        {
            foreach (Transform child in cardGridParent)
            {
                Destroy(child.gameObject);
            }
            _cards.Clear();
            _firstCard = null;
            _secondCard = null;
            _currentMatches = 0;
            _isTurnTimerRunning = false;
            
            if (turnTimerText != null) turnTimerText.text = "Hazır!";
        }

        private void SetupBoard()
        {
            List<CardData> deck = GenerateRandomDeck();
            
            ShuffleDeck(deck);

            foreach (var cardData in deck)
            {
                // Ekrandaki Gridden Kart Instancelarını oluştur
                Card newCard = Instantiate(cardPrefab, cardGridParent);
                newCard.Initialize(cardData, OnCardClicked);
                _cards.Add(newCard);
            }
        }

        /// <summary>
        /// 24 renk havuzundan rastgele 8 tanesini çeker. 
        /// Seçilen moda göre desteyi (16 kart) üretir.
        /// </summary>
        private List<CardData> GenerateRandomDeck()
        {
            List<CardData> deck = new List<CardData>(16);
            
            // 24 Havuzdan rastgele 8 Index seçimi çantası (Tekrarsız)
            List<int> selectedIndices = new List<int>();
            while (selectedIndices.Count < 8)
            {
                int r = Random.Range(0, _colorValues.Length);
                if (!selectedIndices.Contains(r))
                {
                    selectedIndices.Add(r);
                }
            }

            if (currentMatchMode == StroopMatchMode.TextVsColor)
            {
                for (int i = 0; i < 8; i++)
                {
                    int colorIndex = selectedIndices[i];
                    string colorName = _colorNames[colorIndex];
                    Color actualColor = _colorValues[colorIndex];
                    
                    int fakeIndex = Random.Range(0, _colorValues.Length);
                    while (fakeIndex == colorIndex) fakeIndex = Random.Range(0, _colorValues.Length);
                    Color fakeInkColor = _colorValues[fakeIndex];

                    float luminance = (0.299f * fakeInkColor.r) + (0.587f * fakeInkColor.g) + (0.114f * fakeInkColor.b);
                    Color safeBgColor = luminance > 0.55f ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.98f, 0.98f, 0.98f);

                    // 1. KART -> ÜZERİNDE SADECE YAZI YAZAN
                    deck.Add(new CardData { MatchId = colorIndex, HasText = true, DisplayText = colorName, TextColor = fakeInkColor, BackgroundColor = safeBgColor });

                    // 2. KART -> ÜZERİNDE HİÇBİR ŞEY YAZMAYAN, KLASİK BİR DÜZ RENK TABLOSU (Blok)
                    deck.Add(new CardData { MatchId = colorIndex, HasText = false, DisplayText = "", TextColor = Color.clear, BackgroundColor = actualColor });
                }
            }
            else // TextVsText MODU! (16 tane yazılı kart olur. Amaç AYNI METİNLERİ bulmaktır)
            {
                for (int i = 0; i < 8; i++)
                {
                    int wordIndex = selectedIndices[i];
                    string wordName = _colorNames[wordIndex];
                    
                    // İki kart için iki farklı şaşırtıcı mürekkep rengi oluştur
                    int fakeIndex1 = Random.Range(0, _colorValues.Length);
                    while (fakeIndex1 == wordIndex) fakeIndex1 = Random.Range(0, _colorValues.Length);
                    Color fakeInkColor1 = _colorValues[fakeIndex1];
                    
                    int fakeIndex2 = Random.Range(0, _colorValues.Length);
                    while (fakeIndex2 == wordIndex) fakeIndex2 = Random.Range(0, _colorValues.Length);
                    Color fakeInkColor2 = _colorValues[fakeIndex2];

                    float luma1 = (0.299f * fakeInkColor1.r) + (0.587f * fakeInkColor1.g) + (0.114f * fakeInkColor1.b);
                    Color safeBg1 = luma1 > 0.55f ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.98f, 0.98f, 0.98f);
                    
                    float luma2 = (0.299f * fakeInkColor2.r) + (0.587f * fakeInkColor2.g) + (0.114f * fakeInkColor2.b);
                    Color safeBg2 = luma2 > 0.55f ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.98f, 0.98f, 0.98f);

                    // 1. KART -> Aynı Kelime (Renk 1)
                    deck.Add(new CardData { MatchId = wordIndex, HasText = true, DisplayText = wordName, TextColor = fakeInkColor1, BackgroundColor = safeBg1 });
    
                    // 2. KART -> Aynı Kelime (Renk 2 - Farklı bir renge boyalı!)
                    deck.Add(new CardData { MatchId = wordIndex, HasText = true, DisplayText = wordName, TextColor = fakeInkColor2, BackgroundColor = safeBg2 });
                }
            }

            return deck;
        }

        private void ShuffleDeck(List<CardData> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                CardData temp = deck[i];
                int randomIndex = Random.Range(i, deck.Count);
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Kartların ekrandaki yerlerini karıştırıp beyni sıfırlar! (Time Out Durumu İçin)
        /// </summary>
        private void ShuffleBoardVisuals()
        {
            // Sadece hiyerarşik listeyi karıştır
            List<Transform> children = new List<Transform>();
            foreach (Transform child in cardGridParent)
            {
                children.Add(child);
            }
            
            for (int i = 0; i < children.Count; i++)
            {
                int randomIndex = Random.Range(0, children.Count);
                // Unity UI grid sistemi Child Index'lerine göre çalışır
                children[i].SetSiblingIndex(randomIndex);
            }
        }

        private void OnCardClicked(Card clickedCard)
        {
            if (_firstCard == null)
            {
                _firstCard = clickedCard;
                _firstCard.SetSelectedStatus(true); // Büyütülür
                
                // Bomba (Zamanlayıcı) Başlıyor
                _remainingTime = turnTimeLimit;
                _isTurnTimerRunning = true;
            }
            else if (_secondCard == null && clickedCard != _firstCard)
            {
                _secondCard = clickedCard;
                _isTurnTimerRunning = false; // Zamanlayıcı donduruldu
                
                CheckMatch();
            }
        }

        private void CheckMatch()
        {
            if (_firstCard.Id == _secondCard.Id)
            {
                // Doğru Karar Çıktı!
                _firstCard.SetSelectedStatus(false);
                _firstCard.SetMatched();
                
                _secondCard.SetSelectedStatus(false);
                _secondCard.SetMatched();
                
                _currentMatches++;
                
                ResetTurn();
                
                if (turnTimerText != null) turnTimerText.text = "<color=green>Bildiniz!</color>";

                if (_currentMatches >= _totalMatches)
                {
                    GameFinished();
                }
            }
            else
            {
                // Yanlış bildiler
                _firstCard.SetSelectedStatus(false);
                _firstCard.PlayErrorEffect();
                
                _secondCard.PlayErrorEffect();
                
                ResetTurn();
                if (turnTimerText != null) turnTimerText.text = "<color=red>Yanlış Eşleşme!</color>";
            }
        }

        private void HandleTimeOut()
        {
            _isTurnTimerRunning = false;
            
            if (turnTimerText != null)
                turnTimerText.text = "<color=red>SÜRE DOLDU! KARTLAR KARIŞTI...</color>";
                
            if (_firstCard != null)
            {
                _firstCard.PlayErrorEffect(); 
            }
            
            ResetTurn(); // İlk seçimi sil gitsin
            
            // Kullanıcı oyalandığı ve süresi bittiği için ceza olarak masadaki tüm kartların yerini değiştirip kafasını karıştırıyoruz!
            ShuffleBoardVisuals(); 
        }

        private void ResetTurn()
        {
            if (_firstCard != null)
                _firstCard.SetSelectedStatus(false); // Büyüklüğü geri 1x1 yap
                
            _firstCard = null;
            _secondCard = null;
        }

        private void GameFinished()
        {
            _isTurnTimerRunning = false; 
            
            if (turnTimerText != null) turnTimerText.text = "OYUN BİTTİ!";
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                
                if (gameOverTimeText != null)
                {
                    gameOverTimeText.text = "Toplam Tamamlama Süresi:\n" + _totalGameTime.ToString("F1") + "s";
                }
            }
        }
    }
}
