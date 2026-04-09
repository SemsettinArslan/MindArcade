using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MemoryMatch
{
    public class Card : MonoBehaviour
    {
        [Header("Bileşenler")]
        [SerializeField] private TMP_Text wordText;
        [SerializeField] private Button cardButton;
        [SerializeField] private Image background;

        [Header("Efekt Renkleri (Sadece Yazılı Kart İçin)")]
        [SerializeField] private Color defaultBgColor = Color.white;
        [SerializeField] private Color errorBgColor = new Color(1f, 0.5f, 0.5f);

        public int Id { get; private set; }
        public bool IsMatched { get; private set; }

        private Action<Card> _onCardClicked;
        private Coroutine _errorRoutine;
        
        private Color _baseBackgroundColor;
        private bool _isSolidColorCard;

        public void Initialize(CardData data, Action<Card> onCardClicked)
        {
            Id = data.MatchId;
            _onCardClicked = onCardClicked;
            _isSolidColorCard = !data.HasText;
            
            if (data.HasText)
            {
                wordText.text = data.DisplayText;
                wordText.color = data.TextColor;
                _baseBackgroundColor = defaultBgColor; // Örn: Beyaz
            }
            else
            {
                wordText.text = ""; // Yazısız kart!
                _baseBackgroundColor = data.BackgroundColor; // Dümdüz renk
            }
            
            IsMatched = false;
            background.color = _baseBackgroundColor;
            transform.localScale = Vector3.one;
            
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnClicked);
            cardButton.interactable = true;
        }

        private void OnClicked()
        {
            if (IsMatched) return;
            _onCardClicked?.Invoke(this);
        }

        /// <summary>
        /// Kartın tıklanabilirliğini dışarıdan kontrol eder.
        /// Giriş animasyonu sırasında false yapılır.
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (cardButton != null)
                cardButton.interactable = interactable;
        }

        /// <summary>
        /// Seçildiğinde Büyü (Rengi değiştirmek yerine Boyutu değiştiriyoruz ki asıl renk kaybolmasın)
        /// </summary>
        public void SetSelectedStatus(bool isSelected)
        {
            if (IsMatched) return;
            transform.localScale = isSelected ? new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
        }

        public void PlayErrorEffect()
        {
            if (IsMatched) return;
            if (_errorRoutine != null) StopCoroutine(_errorRoutine);
            _errorRoutine = StartCoroutine(ErrorRoutine());
        }

        private System.Collections.IEnumerator ErrorRoutine()
        {
            // Renkli kartsak (Yazısızsa) rengini kaybetmemek için hatayı sağ-sol sallanarak ver
            if (_isSolidColorCard)
            {
                RectTransform rt = GetComponent<RectTransform>();
                Vector2 orig = rt.anchoredPosition;
                
                // Hızlı sağ sol titreşimi (Mini Tween Alternatifi)
                rt.anchoredPosition = orig + new Vector2(10, 0);
                yield return new WaitForSeconds(0.1f);
                rt.anchoredPosition = orig - new Vector2(10, 0);
                yield return new WaitForSeconds(0.1f);
                rt.anchoredPosition = orig + new Vector2(5, 0);
                yield return new WaitForSeconds(0.1f);
                rt.anchoredPosition = orig;
            }
            else
            {
                background.color = errorBgColor;
                yield return new WaitForSeconds(0.5f);
                if (!IsMatched)
                    background.color = _baseBackgroundColor;
            }
        }

        public void SetMatched()
        {
            IsMatched = true;
            cardButton.interactable = false;
            
            // Küçültüp yarı saydamlaştıralım (Çözüldüğünü belirtsin)
            transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            
            // Renkleri yarı karartalım (RGB çarpımı 0.5)
            background.color = new Color(_baseBackgroundColor.r * 0.5f, _baseBackgroundColor.g * 0.5f, _baseBackgroundColor.b * 0.5f, 1f);
        }
    }
}
