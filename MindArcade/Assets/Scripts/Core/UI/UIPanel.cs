using System;
using UnityEngine;
using DG.Tweening;

namespace MindArcade.Core.UI
{
    /// <summary>
    /// Tüm panellerin base class'ı.
    /// DOTween ile açılma/kapanma animasyonları sağlar.
    /// 
    /// ÖNEMLİ: Panel gizleme GameObject.SetActive DEĞİL, 
    /// CanvasGroup (alpha=0, interactable=false) ile yapılır.
    /// Bu sayede Inspector'da paneli gizleyip açmak sorun yaratmaz.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        [Header("Panel Animasyon Ayarları")]
        [SerializeField] protected float animationDuration = 0.35f;
        [SerializeField] protected Ease showEase = Ease.OutBack;
        [SerializeField] protected Ease hideEase = Ease.InBack;

        [Header("Animasyon Tipi")]
        [SerializeField] protected PanelAnimationType animationType = PanelAnimationType.ScaleAndFade;

        [Header("Başlangıç Durumu")]
        [SerializeField] protected bool startHidden = true;

        // ─── Events ─────────────────────────────────────────────
        public event Action OnPanelShown;
        public event Action OnPanelHidden;

        // ─── State ──────────────────────────────────────────────
        public bool IsVisible { get; private set; }

        protected CanvasGroup _canvasGroup;
        private Sequence _currentTween;
        private RectTransform _rectTransform;
        private bool _initialized;

        protected virtual void Awake()
        {
            EnsureInitialized();

            if (startHidden)
                HideInstant();
        }

        /// <summary>
        /// Awake'den önce çağrılabilir durumlar için güvenli initialization.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();

            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // ─── Public API ─────────────────────────────────────────

        public virtual void Show()
        {
            EnsureInitialized();

            if (IsVisible) return;

            // GameObject MUTLAKA aktif olmalı
            gameObject.SetActive(true);
            IsVisible = true;

            KillCurrentTween();
            _currentTween = DOTween.Sequence().SetUpdate(true);

            switch (animationType)
            {
                case PanelAnimationType.ScaleAndFade:
                    AnimateScaleAndFade(true);
                    break;
                case PanelAnimationType.SlideFromBottom:
                    AnimateSlide(true, Vector2.down);
                    break;
                case PanelAnimationType.SlideFromTop:
                    AnimateSlide(true, Vector2.up);
                    break;
                case PanelAnimationType.FadeOnly:
                    AnimateFadeOnly(true);
                    break;
            }

            _currentTween.OnComplete(() =>
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                OnPanelShown?.Invoke();
                OnShowComplete();
            });

            GameEvents.RaiseSFXRequested("panel_open");
        }

        public virtual void Hide()
        {
            if (!IsVisible) return;

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            KillCurrentTween();
            _currentTween = DOTween.Sequence().SetUpdate(true);

            switch (animationType)
            {
                case PanelAnimationType.ScaleAndFade:
                    AnimateScaleAndFade(false);
                    break;
                case PanelAnimationType.SlideFromBottom:
                    AnimateSlide(false, Vector2.down);
                    break;
                case PanelAnimationType.SlideFromTop:
                    AnimateSlide(false, Vector2.up);
                    break;
                case PanelAnimationType.FadeOnly:
                    AnimateFadeOnly(false);
                    break;
            }

            _currentTween.OnComplete(() =>
            {
                IsVisible = false;
                // SetActive(false) KULLANMIYORUZ — CanvasGroup ile gizliyoruz
                // Bu sayede Awake tekrar çalışmıyor
                OnPanelHidden?.Invoke();
                OnHideComplete();
            });

            GameEvents.RaiseSFXRequested("panel_close");
        }

        public void ShowInstant()
        {
            EnsureInitialized();
            KillCurrentTween();

            gameObject.SetActive(true);
            IsVisible = true;

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            transform.localScale = Vector3.one;
        }

        public void HideInstant()
        {
            EnsureInitialized();
            KillCurrentTween();

            IsVisible = false;

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            transform.localScale = Vector3.one;
            // SetActive(false) KULLANMIYORUZ
        }

        public void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        // ─── Override Hooks ─────────────────────────────────────

        protected virtual void OnShowComplete() { }
        protected virtual void OnHideComplete() { }

        // ─── Animation Implementations ──────────────────────────

        private void AnimateScaleAndFade(bool showing)
        {
            if (showing)
            {
                transform.localScale = Vector3.one * 0.6f;
                _canvasGroup.alpha = 0f;

                _currentTween.Append(
                    transform.DOScale(Vector3.one, animationDuration).SetEase(showEase)
                );
                _currentTween.Join(
                    _canvasGroup.DOFade(1f, animationDuration * 0.6f).SetEase(Ease.OutQuad)
                );
            }
            else
            {
                _currentTween.Append(
                    transform.DOScale(Vector3.one * 0.6f, animationDuration * 0.7f).SetEase(hideEase)
                );
                _currentTween.Join(
                    _canvasGroup.DOFade(0f, animationDuration * 0.7f).SetEase(Ease.InQuad)
                );
            }
        }

        private void AnimateSlide(bool showing, Vector2 direction)
        {
            float slideDistance = 800f;
            Vector2 hiddenPos = direction * slideDistance;

            if (showing)
            {
                _rectTransform.anchoredPosition = hiddenPos;
                _canvasGroup.alpha = 0f;

                _currentTween.Append(
                    _rectTransform.DOAnchorPos(Vector2.zero, animationDuration).SetEase(showEase)
                );
                _currentTween.Join(
                    _canvasGroup.DOFade(1f, animationDuration * 0.5f)
                );
            }
            else
            {
                _currentTween.Append(
                    _rectTransform.DOAnchorPos(hiddenPos, animationDuration * 0.7f).SetEase(hideEase)
                );
                _currentTween.Join(
                    _canvasGroup.DOFade(0f, animationDuration * 0.7f)
                );
            }
        }

        private void AnimateFadeOnly(bool showing)
        {
            if (showing)
            {
                _canvasGroup.alpha = 0f;
                transform.localScale = Vector3.one;
                _currentTween.Append(
                    _canvasGroup.DOFade(1f, animationDuration).SetEase(Ease.OutQuad)
                );
            }
            else
            {
                _currentTween.Append(
                    _canvasGroup.DOFade(0f, animationDuration * 0.7f).SetEase(Ease.InQuad)
                );
            }
        }

        private void KillCurrentTween()
        {
            _currentTween?.Kill();
            _currentTween = null;
        }

        protected virtual void OnDestroy()
        {
            KillCurrentTween();
        }
    }

    public enum PanelAnimationType
    {
        ScaleAndFade,
        SlideFromBottom,
        SlideFromTop,
        FadeOnly
    }
}
