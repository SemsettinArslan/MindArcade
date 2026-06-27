using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace MemoryMatch
{
    /// <summary>
    /// SRP: Sadece tahta (board) yönetiminden sorumludur.
    /// Kartları oluşturur, temizler, görsel olarak karıştırır ve giriş animasyonu oynatır.
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private Transform cardGridParent;
        [SerializeField] private Card cardPrefab;

        [Header("Giriş Animasyonu Ayarları")]
        [Tooltip("Her kartın ekranın altından hedef konumuna kayma süresi")]
        [SerializeField] private float cardEntranceDuration = 0.4f;
        [Tooltip("Kartlar arasındaki gecikme (sıralı giriş etkisi)")]
        [SerializeField] private float cardStaggerDelay = 0.06f;
        [Tooltip("Kartların başlangıç konumu (ekranın ne kadar altından gelecek, piksel)")]
        [SerializeField] private float entryOffsetY = -1200f;

        private readonly List<Card> _cards = new List<Card>(16);
        private Sequence _entranceSequence;

        public IReadOnlyList<Card> Cards => _cards;

        /// <summary>
        /// Mevcut tahtayı temizler ve tüm kartları yok eder.
        /// Devam eden animasyonu da iptal eder.
        /// </summary>
        public void ClearBoard()
        {
            // Devam eden DOTween sekansını öldür
            _entranceSequence?.Kill();

            foreach (Transform child in cardGridParent)
            {
                Destroy(child.gameObject);
            }
            _cards.Clear();
        }

        /// <summary>
        /// Verilen deste verisinden kartları instantiate ederek tahtayı kurar.
        /// Kartlar başlangıçta tıklanamaz, animasyon bitince tıklanabilir hale gelir.
        /// </summary>
        /// <param name="deck">Kart verileri</param>
        /// <param name="onCardClicked">Kart tıklandığında çağrılacak callback</param>
        /// <param name="onEntranceComplete">Tüm giriş animasyonu bittiğinde çağrılır (nullable)</param>
        public void SetupBoard(List<CardData> deck, Action<Card> onCardClicked, Action onEntranceComplete = null)
        {
            AdjustGridLayout(deck.Count);

            foreach (var cardData in deck)
            {
                Card newCard = Instantiate(cardPrefab, cardGridParent);
                newCard.Initialize(cardData, onCardClicked);
                
                // Animasyon bitene kadar tıklanamaz
                newCard.SetInteractable(false);
                
                _cards.Add(newCard);
            }

            // Giriş animasyonunu başlat
            PlayEntranceAnimation(onEntranceComplete);
        }

        /// <summary>
        /// Kartların ekrandan taşmaması için ızgara düzenini dinamik olarak ayarlar.
        /// Kart sayısı arttıkça kolon sayısını artırır ve hücre boyutunu küçültür.
        /// </summary>
        private void AdjustGridLayout(int totalCards)
        {
            GridLayoutGroup gridLayout = cardGridParent.GetComponent<GridLayoutGroup>();
            if (gridLayout == null) return;

            // Hücre boyutunu her iki oyun modunda da ve tüm kart sayılarında 180x240 olarak sabitliyoruz
            gridLayout.cellSize = new Vector2(180f, 240f);

            if (totalCards <= 8)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 4;
            }
            else if (totalCards <= 12)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 4;
            }
            else if (totalCards <= 16)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 4;
            }
            else if (totalCards <= 20)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 5;
            }
            else // 24 kart veya üzeri
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 6;
            }
        }

        /// <summary>
        /// Kartların ekrandaki pozisyonlarını karıştırır (ceza mekanizması).
        /// DOTween ile yumuşak karıştırma animasyonu.
        /// </summary>
        public void ShuffleBoardVisuals()
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in cardGridParent)
            {
                children.Add(child);
            }

            for (int i = 0; i < children.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, children.Count);
                children[i].SetSiblingIndex(randomIndex);
            }

            // Karıştırma sonrası hafif bir sarsılma efekti
            foreach (var card in _cards)
            {
                if (!card.IsMatched)
                {
                    RectTransform rt = card.GetComponent<RectTransform>();
                    rt.DOShakeAnchorPos(0.3f, new Vector2(8f, 4f), 10, 90f, false, true, ShakeRandomnessMode.Harmonic);
                }
            }
        }

        // ─── Entrance Animation ─────────────────────────────────

        /// <summary>
        /// Kartları ekranın altından sırayla yukarı kaydırarak dizilme animasyonu oynatır.
        /// Her kart hafif gecikmeyle gelir → "dalga" efekti.
        /// Bir frame bekleyerek Grid Layout'un pozisyonları hesaplamasını sağlar.
        /// </summary>
        private void PlayEntranceAnimation(Action onComplete)
        {
            StartCoroutine(EntranceAnimationRoutine(onComplete));
        }

        private IEnumerator EntranceAnimationRoutine(Action onComplete)
        {
            // Kartları anlık olarak görünmez yap (layout hesaplanana kadar)
            foreach (var card in _cards)
            {
                CanvasGroup cg = card.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = card.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
            }

            // Grid Layout'un pozisyonları hesaplaması için 1 frame bekle
            yield return null;

            // Layout'u zorla güncelle
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardGridParent as RectTransform);

            // Şimdi pozisyonlar doğru — animasyonu başlat
            _entranceSequence?.Kill();
            _entranceSequence = DOTween.Sequence();

            for (int i = 0; i < _cards.Count; i++)
            {
                RectTransform rt = _cards[i].GetComponent<RectTransform>();
                CanvasGroup cg = _cards[i].GetComponent<CanvasGroup>();

                // Hedef konumu kaydet (Grid Layout tarafından ARTIK DOĞRU atanmış)
                Vector2 targetPos = rt.anchoredPosition;

                // Başlangıç: ekranın altında + tamamen saydam + küçük
                rt.anchoredPosition = new Vector2(targetPos.x, entryOffsetY);
                cg.alpha = 0f;
                rt.localScale = Vector3.one * 0.5f;

                float delay = i * cardStaggerDelay;

                // Pozisyon: aşağıdan yukarı elastik kayma
                _entranceSequence.Insert(delay,
                    rt.DOAnchorPos(targetPos, cardEntranceDuration)
                      .SetEase(Ease.OutBack)
                );

                // Opaklık: 0 → 1 (hızlı fade-in)
                _entranceSequence.Insert(delay,
                    cg.DOFade(1f, cardEntranceDuration * 0.5f)
                      .SetEase(Ease.OutQuad)
                );

                // Boyut: 0.5x → 1x (büyüyerek gelme)
                _entranceSequence.Insert(delay,
                    rt.DOScale(Vector3.one, cardEntranceDuration)
                      .SetEase(Ease.OutBack)
                );
            }

            // Animasyon bittiğinde: kartları tıklanabilir yap + onComplete çağır
            _entranceSequence.OnComplete(() =>
            {
                foreach (var card in _cards)
                {
                    card.SetInteractable(true);
                }
                onComplete?.Invoke();
            });
        }

        private void OnDestroy()
        {
            _entranceSequence?.Kill();
        }
    }
}
