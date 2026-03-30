using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace FocusRunner
{
    public class FocusRunnerUI : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text _ruleText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _statsText; // Level, Accuracy
        [SerializeField] private TMP_Text _levelNotificationText;

        [Header("Feedback Effects")]
        [SerializeField] private Image _flashOverlay; // Ekranda tam boy bir Image olmalı (Alpha 0)
        [SerializeField] private Color _correctFlashColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Color _wrongFlashColor = new Color(1, 0, 0, 0.3f);

        [Header("Panels")]
        [SerializeField] private GameObject _gameOverPanel;

        private void Start()
        {
            if (RuleManager.Instance != null)
                RuleManager.Instance.OnRuleChanged += UpdateRuleUI;

            if (_levelNotificationText != null) 
                _levelNotificationText.gameObject.SetActive(false);

            if (_flashOverlay != null)
                _flashOverlay.color = Color.clear;
        }

        private void OnDestroy()
        {
            if (RuleManager.Instance != null)
                RuleManager.Instance.OnRuleChanged -= UpdateRuleUI;
        }

        private void Update()
        {
            if (FocusRunnerManager.Instance == null) return;

            if (_scoreText != null) 
                _scoreText.text = $"Puan: {FocusRunnerManager.Instance.Score}";

            if (_statsText != null)
                _statsText.text = $"Lvl: {DifficultyManager.Instance.currentLevel} | Doğruluk: {FocusRunnerManager.Instance.Accuracy:P0}";
        }

        public void TriggerFlash(bool isCorrect)
        {
            if (_flashOverlay == null) return;
            StopAllCoroutines();
            StartCoroutine(FlashRoutine(isCorrect ? _correctFlashColor : _wrongFlashColor));
        }

        private IEnumerator FlashRoutine(Color targetColor)
        {
            float duration = 0.25f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _flashOverlay.color = Color.Lerp(targetColor, Color.clear, elapsed / duration);
                yield return null;
            }
            _flashOverlay.color = Color.clear;
        }

        private void UpdateRuleUI(GameRule rule, string description)
        {
            if (_ruleText != null)
            {
                _ruleText.text = description;
            }

            ShowLevelUpNotification("HEDEF DEĞİŞTİ!");
        }

        public void ShowLevelUpNotification(string text)
        {
            if (_levelNotificationText == null) return;
            
            _levelNotificationText.text = text;
            _levelNotificationText.gameObject.SetActive(true);
            
            CancelInvoke(nameof(HideNotification));
            Invoke(nameof(HideNotification), 1.5f);
        }

        private void HideNotification()
        {
            if (_levelNotificationText != null) 
                _levelNotificationText.gameObject.SetActive(false);
        }

        public void ToggleGameOver(bool active)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(active);
        }
    }
}
