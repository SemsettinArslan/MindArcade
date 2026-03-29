using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

namespace SequenceCubes
{
    public enum RuleMode
    {
        Normal,      // Yukarı çıkan küpleri sırayla
        Reverse,     // Yukarı çıkanları TERS sırayla
        OddIndices,  // Yukarı çıkanların Sadece 1. 3. 5. adımlarını
        EvenIndices  // Yukarı çıkanların Sadece 2. 4. 6. adımlarını
    }

    /// <summary>
    /// Bilgisayarın dizilimi yaparken aklında tutacağı komut zarfı.
    /// Hem küpün kimliği hem de kandırmacı (Aşağı inme) özelliğini içerir.
    /// </summary>
    public struct SequenceAction
    {
        public SequenceCube Cube;
        public bool IsDecoy; 
    }

    public class SequenceGameManager : MonoBehaviour
    {
        [Header("Ekranda Bulunan Küpler")]
        [SerializeField] private SequenceCube[] availableCubes;
        
        [Header("Zorluk / Oyun Ayarları")]
        [Tooltip("Bilgisayar rastgele kaç kere zıplatsın? (Min 4 önerilir)")]
        [SerializeField] private int sequenceLength = 4;
        [Tooltip("Küpler kalkıp inerken aralarında ne kadar boşluk olsun?")]
        [SerializeField] private float delayBetweenCubes = 0.5f;
        [Tooltip("Kaç kez yanma hakkımız var?")]
        [SerializeField] private int maxLives = 3;
        [Tooltip("Sahte (Aşağı Yönde) Zıplama ihtimali (Örn: 0.3 = %30 İhtimal)")]
        [Range(0f, 1f)]
        [SerializeField] private float decoyChance = 0.35f;

        [Header("Olaylar (Eventler)")]
        public UnityEvent<int> OnLivesChanged; 
        public UnityEvent OnGameWin; 
        public UnityEvent OnGameOver; 
        
        [Header("UI Geri Bildirim")]
        [Tooltip("Kullanıcıyı oyun boyunca yönlendirecek olan (Kandırıcı Kural) Gösterge Text'i. mistakeWarningText ile AYNI Obje Olabilir.")]
        [SerializeField] private TMP_Text instructionText;

        // Bilgisayarın GÖSTERDİĞİ tüm liste (Aşağı ve Yukarı Zıplamalar)
        private readonly List<SequenceAction> _generatedActions = new List<SequenceAction>();
        
        // Oyuncunun BULMASI BEKLENEN doğru hedefler listesi (Sadece Yukarı çıkanlar ve Kurala göre Filtrelenenler)
        private readonly List<SequenceCube> _expectedClickSequence = new List<SequenceCube>();
        
        private int _currentInputIndex;
        private int _currentLives;
        private RuleMode _currentMode;
        
        private bool _isAcceptingInput = false;

        private void Start()
        {
            if (availableCubes == null || availableCubes.Length == 0) return;

            for (int i = 0; i < availableCubes.Length; i++)
            {
                availableCubes[i].Initialize(i, OnCubeClicked);
            }

            if (instructionText != null) instructionText.gameObject.SetActive(false);

            StartNewLevel();
        }

        private void Update()
        {
            if (_isAcceptingInput && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (Camera.main != null)
                {
                    Vector2 mousePos = Mouse.current.position.ReadValue();
                    Ray ray = Camera.main.ScreenPointToRay(mousePos);
                    
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        SequenceCube clickedCube = hit.collider.GetComponent<SequenceCube>();
                        if (clickedCube != null)
                        {
                            clickedCube.OnUserClicked(); 
                        }
                    }
                }
            }
        }

        public void StartNewLevel()
        {
            _currentLives = maxLives;
            OnLivesChanged?.Invoke(_currentLives); 
            
            GenerateNewSequence();
            StartCoroutine(PlaySequence());
        }

        private void GenerateNewSequence()
        {
            _generatedActions.Clear();
            _expectedClickSequence.Clear();
            
            // 1. Yeni Kural Belirle (1. Turda basit olsun, diğer turlarda da tamamen rastgele beyin yakıcı gelsin)
            _currentMode = (RuleMode)Random.Range(0, 4);

            // 2. Aksiyonları Üret (Aşağı, Yukarı vs)
            for (int i = 0; i < sequenceLength; i++)
            {
                SequenceCube randomCube = availableCubes[Random.Range(0, availableCubes.Length)];
                bool isDecoy = Random.value < decoyChance;
                
                _generatedActions.Add(new SequenceAction { Cube = randomCube, IsDecoy = isDecoy });
            }
            
            // (Güvenlik Önlemi) Tüm seri şans eseri kandırmaca inerse, oyuncu oyunu bitiremez ve kitlenir. 
            // Bu yüzden en azından listedeki 2 eylemi zorla YUKARI (isDecoy=false) çıkarıyoruz.
            ForceValidActionsCount(2);

            // 3. OYUNCUDAN BEKLENECEK HEDEF LİSTESİNİ ÇIKAR.
            // A- Bütün kandırmaca AŞAĞI inişleri filtresinden geçir ve sadece temiz YUKARI çıkanları al.
            List<SequenceCube> validUpCubes = new List<SequenceCube>();
            foreach (var action in _generatedActions)
            {
                if (!action.IsDecoy)
                    validUpCubes.Add(action.Cube);
            }

            // B- Elde kalan net listeyi, Geçerli Oyun Kurallarına göre eğip büküp "Nihai Beklenen Listesine (Expected)" atalım.
            switch (_currentMode)
            {
                case RuleMode.Normal:
                    // Düz olarak akılda kalanları ekle.
                    _expectedClickSequence.AddRange(validUpCubes);
                    break;
                    
                case RuleMode.Reverse:
                    // TERSİNE ÇEVİR - Listeyi sondan başa doğru okutup hedef listesine at.
                    for (int i = validUpCubes.Count - 1; i >= 0; i--)
                    {
                        _expectedClickSequence.Add(validUpCubes[i]);
                    }
                    break;
                    
                case RuleMode.OddIndices:
                    // 1., 3., 5. Adımları bul. (C# da indeks 0, 2, 4 demektir)
                    for (int i = 0; i < validUpCubes.Count; i += 2)
                    {
                        _expectedClickSequence.Add(validUpCubes[i]);
                    }
                    break;
                    
                case RuleMode.EvenIndices:
                    // 2., 4., 6. Adımları bul. (C# da indeks 1, 3, 5 demektir)
                    for (int i = 1; i < validUpCubes.Count; i += 2)
                    {
                        _expectedClickSequence.Add(validUpCubes[i]);
                    }
                    break;
            }
        }
        
        private void ForceValidActionsCount(int minCount)
        {
            int currentValid = 0;
            foreach (var action in _generatedActions)
            {
                if (!action.IsDecoy) currentValid++;
            }
            
            // Eksikse sonuncu dizileri zorla up yapalım
            int idx = _generatedActions.Count - 1;
            while (currentValid < minCount && idx >= 0)
            {
                SequenceAction a = _generatedActions[idx];
                if (a.IsDecoy)
                {
                    a.IsDecoy = false;
                    _generatedActions[idx] = a;
                    currentValid++;
                }
                idx--;
            }
        }

        private string GetRuleDescription()
        {
            switch (_currentMode)
            {
                case RuleMode.Normal: return "<color=yellow>YUKARI ÇIKANLARI\nSırasıyla Tekrarla!</color>";
                case RuleMode.Reverse: return "<color=#FF5555>DİKKAT: TERS YÖN!\nYUKARI Çıkanları \nSondan Başa Doğru Tıkla!</color>";
                case RuleMode.OddIndices: return "<color=#FFAAFF>ATLAMALI HAMLE!\nSadece 1., 3. ve 5. \nYUKARI Çıkanları Tıkla!</color>";
                case RuleMode.EvenIndices: return "<color=#55FFFF>ATLAMALI HAMLE!\nSadece 2. ve 4. \nYUKARI Çıkanları Tıkla!</color>";
                default: return "";
            }
        }

        private IEnumerator PlaySequence()
        {
            _isAcceptingInput = false;
            SetCubesInteractable(false);
            
            // Oyuncunun beynini yakacak olan tur kuralını ekranda büyükçe göster
            if (instructionText != null)
            {
                instructionText.gameObject.SetActive(true);
                instructionText.text = GetRuleDescription();
            }
            
            // Kuralı okuması ve hazırlanması için 3 saniye süre aşılayalım.
            yield return new WaitForSeconds(3.0f);
            
            if (instructionText != null) instructionText.gameObject.SetActive(false);

            // Aksiyonları ekranda tek tek izletelim (Kandırmacalar Aşağı inecektir)
            for (int i = 0; i < _generatedActions.Count; i++)
            {
                bool isAnimationFinished = false;
                
                // Animasyona decoy olup olmadığını param olarak sunuyoruz.
                _generatedActions[i].Cube.PlayAnimation(_generatedActions[i].IsDecoy, () => isAnimationFinished = true);
                
                while (!isAnimationFinished) yield return null;
                
                yield return new WaitForSeconds(delayBetweenCubes);
            }

            // Oynatım bitti, artık klavye/fare özgür!
            _currentInputIndex = 0;
            
            // (Güvenlik Önlemi) Sıralama çok şanssız/imkansız süzgeçli geldiyse otomatik sonlandır.
            if (_expectedClickSequence.Count == 0)
            {
                HandleSuccess();
                yield break;
            }

            _isAcceptingInput = true;
            SetCubesInteractable(true);
            
            // Oyuncuya sıranın onda geçtiğini göster.
            ShowFeedback("<color=green>SIRA SENDE!</color>", 1f);
        }

        private void SetCubesInteractable(bool state)
        {
            foreach (var cube in availableCubes) cube.SetInteractable(state);
        }

        private void OnCubeClicked(SequenceCube clickedCube)
        {
            if (!_isAcceptingInput) return;

            // Target (Aradığımız küp) hedef listede bu tur eşleşiyor olmalıdır
            SequenceCube expectedCube = _expectedClickSequence[_currentInputIndex];
            
            if (clickedCube.Id != expectedCube.Id)
            {
                HandleMistake();
                return;
            }

            _currentInputIndex++;

            if (_currentInputIndex >= _expectedClickSequence.Count)
            {
                HandleSuccess();
            }
        }

        private void HandleMistake()
        {
            _isAcceptingInput = false;
            SetCubesInteractable(false);
            
            _currentLives--;
            OnLivesChanged?.Invoke(_currentLives);

            if (_currentLives > 0)
                ShowFeedback($"HATA!\nKalan Hak: {_currentLives}\n(Tekrar İzle)", 2f);
            else
                ShowFeedback("Hak Bitti!\nSistem Yenileniyor...", 2f);

            if (_currentLives <= 0)
            {
                OnGameOver?.Invoke();
                StartCoroutine(RestartWithDelay(2.0f)); 
            }
            else
            {
                // Kandırmaca animasyon dizilimini ezberlerken yandı, tekrar aynı izlenceyi aynı kuralla sunuyoruz.
                StartCoroutine(PlaySequence());
            }
        }

        private void HandleSuccess()
        {
            _isAcceptingInput = false;
            SetCubesInteractable(false);
            
            OnGameWin?.Invoke();
            ShowFeedback("<color=green>BRAVO!\nDizi Çözüldü.</color>", 1.5f);
            
            StartCoroutine(StartNextRoundWithDelay());
        }
        
        private void ShowFeedback(string msg, float duration)
        {
            if (instructionText != null)
            {
                instructionText.gameObject.SetActive(true);
                instructionText.text = msg;
                StartCoroutine(HideInstructionAfter(duration));
            }
        }
        
        private IEnumerator HideInstructionAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (instructionText != null && !_isAcceptingInput) // Eğer yeni oyun başlamadıysa sakla
            {
                instructionText.gameObject.SetActive(false);
            }
        }

        private IEnumerator RestartWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartNewLevel();
        }

        private IEnumerator StartNextRoundWithDelay()
        {
            // İsterseniz burada zorluğu "sequenceLength++;" yaparak 1 artırabilirsiniz.
            yield return new WaitForSeconds(2f);
            StartNewLevel();
        }
    }
}
