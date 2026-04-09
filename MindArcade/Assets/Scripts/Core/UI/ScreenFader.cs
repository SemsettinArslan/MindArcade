using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace MindArcade.Core.UI
{
    /// <summary>
    /// Sahne geçiş efekti.
    /// Tam ekran siyah overlay ile fade in/out.
    /// Sahne yüklemeden önce FadeOut, yüklendikten sonra FadeIn çağrılır.
    /// 
    /// Kurulum:
    /// 1. Ayrı bir Canvas oluştur (Sort Order: 999)
    /// 2. Canvas'a tam ekran Image ekle (Stretch, siyah renk)
    /// 3. Bu component'i Image objesine ekle
    /// 4. Canvas'ı prefab yap ve ilk sahneye koy
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [Header("Ayarlar")]
        [Tooltip("Kararma süresi (saniye). 2-3 arası ideal.")]
        [SerializeField] private float fadeDuration = 2.5f;
        [SerializeField] private Color fadeColor = Color.black;

        [Header("Otomatik Sahne FadeIn")]
        [Tooltip("Her sahne yüklendiğinde otomatik FadeIn yapılsın mı?")]
        [SerializeField] private bool autoFadeInOnSceneLoad = true;

        private Image _fadeImage;
        private CanvasGroup _canvasGroup;
        private Tween _currentTween;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _fadeImage = GetComponent<Image>();
            _canvasGroup = GetComponent<CanvasGroup>();

            _fadeImage.color = fadeColor;
            _fadeImage.raycastTarget = true;
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Sahne yüklendiğinde otomatik FadeIn.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (autoFadeInOnSceneLoad)
            {
                // Bir frame bekle ki sahne tam yüklensin
                StartCoroutine(FadeInNextFrame());
            }
        }

        private IEnumerator FadeInNextFrame()
        {
            yield return null;
            FadeIn();
        }

        // ─── Public API ─────────────────────────────────────────

        /// <summary>
        /// Ekranı koyulaştırır (sahne değişmeden önce).
        /// </summary>
        public void FadeOut(Action onComplete = null)
        {
            KillCurrent();

            _canvasGroup.blocksRaycasts = true;
            _currentTween = _canvasGroup
                .DOFade(1f, fadeDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Ekranı açar (sahne yüklendikten sonra).
        /// </summary>
        public void FadeIn(Action onComplete = null)
        {
            KillCurrent();

            _canvasGroup.alpha = 1f; // Tam siyahtan başla
            _canvasGroup.blocksRaycasts = true;

            _currentTween = _canvasGroup
                .DOFade(0f, fadeDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _canvasGroup.blocksRaycasts = false;
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// Fade out → sahne yükle → otomatik fade in (OnSceneLoaded ile).
        /// </summary>
        public void TransitionToScene(string sceneName)
        {
            FadeOut(() =>
            {
                SceneManager.LoadScene(sceneName);
                // FadeIn otomatik olacak → OnSceneLoaded callback'i
            });
        }

        private void KillCurrent()
        {
            _currentTween?.Kill();
            _currentTween = null;
        }

        private void OnDestroy()
        {
            KillCurrent();
        }
    }
}
