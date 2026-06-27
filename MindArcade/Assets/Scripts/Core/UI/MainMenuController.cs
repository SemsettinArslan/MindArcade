using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MindArcade.Core;
using MindArcade.Core.Llm;

namespace MindArcade.Core.UI
{
    /// <summary>
    /// Ana menü animasyonları ve panel yönetimini üstlenen denetleyici.
    /// Title ve butonlar için premium giriş animasyonları ve döngüsel sallanma (floating/bobbing) hareketleri sağlar.
    /// Panellerin dışına tıklayarak kapatılabilmesini destekler.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Başlık Efekt Ayarları (Title)")]
        [SerializeField] private RectTransform titleRect;
        [SerializeField] private float titleSlideDuration = 1.0f;
        [SerializeField] private float titleSlideDistance = 500f;
        [SerializeField] private Ease titleSlideEase = Ease.OutBack;
        [SerializeField] private float titleBobAmount = 15f;
        [SerializeField] private float titleBobDuration = 2.0f;

        [Header("Buton Giriş Efektleri")]
        [SerializeField] private RectTransform[] menuButtons;
        [SerializeField] private CanvasGroup[] menuButtonCanvasGroups;
        [SerializeField] private float buttonsSlideDuration = 0.8f;
        [SerializeField] private float buttonsSlideDistance = 400f;
        [SerializeField] private Ease buttonsSlideEase = Ease.OutBack;
        [SerializeField] private float buttonsStaggerDelay = 0.15f;

        [Header("Ana Menü Buton Bağlantıları")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button doctorLoginMenuBtn;
 
        [Header("Alt Paneller")]
        [SerializeField] private UIPanel playPanel;
        [SerializeField] private UIPanel settingsPanel;
        [SerializeField] private UIPanel loginPanel;
        [SerializeField] private UIPanel doctorLoginPanel;
        [SerializeField] private UIPanel doctorPortalPanel;
 
        [Header("Panel Dışı Kapatma Tetikleyicileri (Blocker Overlays)")]
        [SerializeField] private Button playPanelCloseBlocker;
        [SerializeField] private Button settingsPanelCloseBlocker;
        [SerializeField] private Button loginPanelCloseBlocker;
        [SerializeField] private Button doctorLoginCloseBlocker;
 
        [Header("Oyun Seçim Butonları (Play Panel)")]
        [SerializeField] private Button memoryMatchBtn;
        [SerializeField] private Button focusRunnerBtn;
        [SerializeField] private Button sequenceCubesBtn;

        private Vector2 _originalTitlePosition;
        private Vector2[] _originalButtonPositions;
        private Tween _titleBobTween;

        private void Awake()
        {
            // Orijinal pozisyonları kaydet
            if (titleRect != null)
            {
                _originalTitlePosition = titleRect.anchoredPosition;
            }

            if (menuButtons != null && menuButtons.Length > 0)
            {
                _originalButtonPositions = new Vector2[menuButtons.Length];
                for (int i = 0; i < menuButtons.Length; i++)
                {
                    if (menuButtons[i] != null)
                    {
                        _originalButtonPositions[i] = menuButtons[i].anchoredPosition;
                    }
                }
            }

            // Başlangıç durumunu hazırla (Gizli/Offset)
            PrepareInitialState();
        }

        private void Start()
        {
            // Event dinleyicilerini bağla
            SetupButtonListeners();

            // Giriş animasyonlarını başlat
            PlayIntroAnimations();
        }

        /// <summary>
        /// Nesneleri animasyon öncesindeki başlangıç konumlarına ve saydamlıklarına (alpha) ayarlar.
        /// </summary>
        private void PrepareInitialState()
        {
            // Title'ı yukarıya taşı
            if (titleRect != null)
            {
                titleRect.anchoredPosition = new Vector2(_originalTitlePosition.x, _originalTitlePosition.y + titleSlideDistance);
            }

            // Butonları sola kaydır ve saydamlaştır
            if (menuButtons != null)
            {
                for (int i = 0; i < menuButtons.Length; i++)
                {
                    if (menuButtons[i] != null)
                    {
                        menuButtons[i].anchoredPosition = new Vector2(_originalButtonPositions[i].x - buttonsSlideDistance, _originalButtonPositions[i].y);
                    }

                    if (menuButtonCanvasGroups != null && i < menuButtonCanvasGroups.Length && menuButtonCanvasGroups[i] != null)
                    {
                        menuButtonCanvasGroups[i].alpha = 0f;
                        menuButtonCanvasGroups[i].interactable = false;
                        menuButtonCanvasGroups[i].blocksRaycasts = false;
                    }
                }
            }

            // Panellerin başlangıçta kapalı olmasını garanti et
            if (playPanel != null) playPanel.HideInstant();
            if (settingsPanel != null) settingsPanel.HideInstant();
            if (loginPanel != null) loginPanel.HideInstant();
            if (doctorLoginPanel != null) doctorLoginPanel.HideInstant();
            if (doctorPortalPanel != null) doctorPortalPanel.HideInstant();
        }

        /// <summary>
        /// Giriş animasyonlarını (Title yukarıdan aşağıya, Butonlar soldan sağa stagger ile) tetikler.
        /// </summary>
        private void PlayIntroAnimations()
        {
            // Title Yukarıdan Aşağı Kayma Animasyonu
            if (titleRect != null)
            {
                titleRect.DOAnchorPosY(_originalTitlePosition.y, titleSlideDuration)
                    .SetEase(titleSlideEase)
                    .OnComplete(StartTitleBobbing);
            }

            // Butonlar Soldan Sağa Kayma ve Fade-In Animasyonu
            if (menuButtons != null)
            {
                for (int i = 0; i < menuButtons.Length; i++)
                {
                    int index = i; // Closure için
                    if (menuButtons[index] == null) continue;

                    float delay = index * buttonsStaggerDelay;

                    // Pozisyon kaydırma
                    menuButtons[index].DOAnchorPosX(_originalButtonPositions[index].x, buttonsSlideDuration)
                        .SetEase(buttonsSlideEase)
                        .SetDelay(delay);

                    // Fade-In
                    if (menuButtonCanvasGroups != null && index < menuButtonCanvasGroups.Length && menuButtonCanvasGroups[index] != null)
                    {
                        menuButtonCanvasGroups[index].DOFade(1f, buttonsSlideDuration * 0.8f)
                            .SetDelay(delay)
                            .OnComplete(() =>
                            {
                                menuButtonCanvasGroups[index].interactable = true;
                                menuButtonCanvasGroups[index].blocksRaycasts = true;
                            });
                    }
                }
            }
        }

        /// <summary>
        /// Başlık ulaştığı noktada sürekli yukarı aşağı yumuşak salınım (bobbing) yapar.
        /// </summary>
        private void StartTitleBobbing()
        {
            if (titleRect == null) return;

            // Yukarı aşağı yumuşak süzülme hareketi (Looping Yoyo)
            _titleBobTween = titleRect.DOAnchorPosY(_originalTitlePosition.y - titleBobAmount, titleBobDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Tüm alt panelleri gizler.
        /// </summary>
        public void HideAllPanels()
        {
            if (playPanel != null && playPanel.IsVisible) playPanel.Hide();
            if (settingsPanel != null && settingsPanel.IsVisible) settingsPanel.Hide();
            if (loginPanel != null && loginPanel.IsVisible) loginPanel.Hide();
            if (doctorLoginPanel != null && doctorLoginPanel.IsVisible) doctorLoginPanel.Hide();
            if (doctorPortalPanel != null && doctorPortalPanel.IsVisible) doctorPortalPanel.Hide();
        }

        public void ShowPlayPanel()
        {
            HideAllPanels();
            if (playPanel != null) playPanel.Show();
        }

        public void ShowSettingsPanel()
        {
            HideAllPanels();
            if (settingsPanel != null) settingsPanel.Show();
        }

        public void ShowLoginPanel()
        {
            HideAllPanels();
            if (loginPanel != null) loginPanel.Show();
        }

        public void ShowDoctorLoginPanel()
        {
            HideAllPanels();
            if (doctorLoginPanel != null) doctorLoginPanel.Show();
        }

        public void ShowDoctorPortalPanel()
        {
            HideAllPanels();
            if (doctorPortalPanel != null)
            {
                doctorPortalPanel.Show();
                var portal = doctorPortalPanel.GetComponent<DoctorPortalPanel>();
                if (portal != null)
                {
                    portal.OpenPortal();
                }
            }
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
        }

        private void SetupButtonListeners()
        {
            // Play -> Giriş Yapılmışsa Oyun Seçimini Aç, Değilse Hasta Girişini Aç
            if (playButton != null)
            {
                playButton.onClick.AddListener(() =>
                {
                    if (!string.IsNullOrEmpty(PlayerProfileManager.ActivePatientId))
                    {
                        ShowPlayPanel();
                    }
                    else
                    {
                        ShowLoginPanel();
                    }
                });
            }
 
            // Settings -> Ayarlar Panelini Aç
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(ShowSettingsPanel);
            }

            // Doctor Login -> Doktor Giriş Panelini Aç
            if (doctorLoginMenuBtn != null)
            {
                doctorLoginMenuBtn.onClick.AddListener(ShowDoctorLoginPanel);
            }
 
            // Quit -> Oyunu Kapat
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(() =>
                {
                    if (SceneNavigator.Instance != null)
                    {
                        SceneNavigator.Instance.QuitGame();
                    }
                    else
                    {
                        Debug.Log("QuitGame tetiklendi (SceneNavigator eksik).");
                        Application.Quit();
                    }
                });
            }

            // Panel dışı tıklanma blockers
            if (playPanelCloseBlocker != null && playPanel != null)
            {
                playPanelCloseBlocker.onClick.AddListener(() => playPanel.Hide());
            }

            if (settingsPanelCloseBlocker != null && settingsPanel != null)
            {
                settingsPanelCloseBlocker.onClick.AddListener(() => settingsPanel.Hide());
            }

            if (loginPanelCloseBlocker != null && loginPanel != null)
            {
                loginPanelCloseBlocker.onClick.AddListener(() => loginPanel.Hide());
            }

            if (doctorLoginCloseBlocker != null && doctorLoginPanel != null)
            {
                doctorLoginCloseBlocker.onClick.AddListener(() => doctorLoginPanel.Hide());
            }

            // Oyun Seçim Butonları Dinleyicileri (Play Panel)
            if (memoryMatchBtn != null)
            {
                memoryMatchBtn.onClick.AddListener(() => LoadGame(SceneType.MemoryMatch));
            }

            if (focusRunnerBtn != null)
            {
                focusRunnerBtn.onClick.AddListener(() => LoadGame(SceneType.FocusRunner));
            }

            if (sequenceCubesBtn != null)
            {
                sequenceCubesBtn.onClick.AddListener(() => LoadGame(SceneType.SequenceCubes));
            }
        }

        private void LoadGame(SceneType targetScene)
        {
            if (SceneNavigator.Instance != null)
            {
                SceneNavigator.Instance.LoadScene(targetScene);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene.ToString());
            }
        }

        private void OnDestroy()
        {
            _titleBobTween?.Kill();
        }
    }
}
