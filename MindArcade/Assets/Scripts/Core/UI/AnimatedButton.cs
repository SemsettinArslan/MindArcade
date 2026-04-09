using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace MindArcade.Core.UI
{
    /// <summary>
    /// Herhangi bir Button'a eklendiğinde otomatik hover/click animasyonları sağlar.
    /// DOTween ile akıcı micro-interaction efektleri.
    /// 
    /// Kullanım: Button olan herhangi bir GameObject'e bu component'i ekle.
    /// Inspector'dan efekt parametrelerini ayarla.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Hover Efekti")]
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float hoverDuration = 0.2f;
        [SerializeField] private Ease hoverEase = Ease.OutQuad;

        [Header("Tıklama Efekti")]
        [SerializeField] private float clickScale = 0.92f;
        [SerializeField] private float clickDuration = 0.1f;
        [SerializeField] private float clickBounceDuration = 0.15f;

        [Header("Ses")]
        [SerializeField] private bool playClickSound = true;
        [SerializeField] private bool playHoverSound = true;

        private Vector3 _originalScale;
        private Tween _currentTween;
        private Button _button;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _button = GetComponent<Button>();
        }

        // ─── Pointer Events ─────────────────────────────────────

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            KillTween();
            _currentTween = transform
                .DOScale(_originalScale * hoverScale, hoverDuration)
                .SetEase(hoverEase)
                .SetUpdate(true); // Pause sırasında da çalışsın

            if (playHoverSound)
                GameEvents.RaiseSFXRequested("button_hover");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            KillTween();
            _currentTween = transform
                .DOScale(_originalScale, hoverDuration)
                .SetEase(hoverEase)
                .SetUpdate(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            KillTween();
            _currentTween = transform
                .DOScale(_originalScale * clickScale, clickDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true);

            if (playClickSound)
                GameEvents.RaiseSFXRequested("button_click");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            KillTween();
            _currentTween = transform
                .DOScale(_originalScale, clickBounceDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        // ─── Cleanup ────────────────────────────────────────────

        private void KillTween()
        {
            _currentTween?.Kill();
            _currentTween = null;
        }

        /// <summary>
        /// Scale'i sıfırlar (sahne geçişlerinde).
        /// </summary>
        public void ResetScale()
        {
            KillTween();
            transform.localScale = _originalScale;
        }

        private void OnDisable()
        {
            KillTween();
            transform.localScale = _originalScale;
        }

        private void OnDestroy()
        {
            KillTween();
        }
    }
}
