using UnityEngine;
using UnityEngine.InputSystem;

namespace MindArcade.Core.UI
{
    /// <summary>
    /// Pause mantığını yönetir.
    /// ESC / Back tuşunu dinler ve PausePanel'i açıp kapar.
    /// Her sahneye bir tane eklenir.
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [SerializeField] private PausePanel pausePanel;

        private bool _isPaused;

        private void Update()
        {
            // ESC tuşu kontrolü
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        /// <summary>
        /// Pause durumunu değiştirir.
        /// </summary>
        public void TogglePause()
        {
            if (pausePanel == null) return;

            if (_isPaused)
            {
                pausePanel.Resume();
                _isPaused = false;
            }
            else
            {
                pausePanel.Pause();
                _isPaused = true;
            }
        }

        /// <summary>
        /// Dışarıdan pause butonuyla çağrılabilir.
        /// </summary>
        public void PauseGame()
        {
            if (_isPaused || pausePanel == null) return;
            pausePanel.Pause();
            _isPaused = true;
        }

        /// <summary>
        /// Dışarıdan resume çağrılabilir.
        /// </summary>
        public void ResumeGame()
        {
            if (!_isPaused || pausePanel == null) return;
            pausePanel.Resume();
            _isPaused = false;
        }
    }
}
