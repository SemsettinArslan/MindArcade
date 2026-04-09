using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace MindArcade.Core.UI
{
    /// <summary>
    /// Ortak Pause paneli. UIPanel'den türer.
    /// Devam Et, Ana Menü ve Ayarlar butonları.
    /// Time.timeScale ile oyunu durdurur/devam ettirir.
    /// Ana Menüye dönüşte ScreenFader ile fade geçişi yapar.
    /// </summary>
    public class PausePanel : UIPanel
    {
        [Header("Pause — Butonlar")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button settingsButton;

        [Header("Sahne Geçişi")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        protected override void Awake()
        {
            base.Awake();

            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        /// <summary>
        /// Paneli açar ve oyunu durdurur.
        /// </summary>
        public void Pause()
        {
            Show();
            Time.timeScale = 0f;
            GameEvents.RaiseGamePaused();
        }

        /// <summary>
        /// Paneli kapatır ve oyuna devam eder.
        /// </summary>
        public void Resume()
        {
            Hide();
            Time.timeScale = 1f;
            GameEvents.RaiseGameResumed();
        }

        private void GoToMainMenu()
        {
            // Önce time scale'i düzelt
            Time.timeScale = 1f;
            GameEvents.RaiseGameResumed();

            // ScreenFader varsa fade geçişi yap
            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.TransitionToScene(mainMenuSceneName);
            }
            else
            {
                // ScreenFader yoksa direkt yükle
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Time.timeScale = 1f;
        }
    }
}
