using UnityEngine;
using TMPro;
using DG.Tweening;
using MindArcade.Core;

namespace SequenceCubes
{
    /// <summary>
    /// SRP: Sadece SequenceCubes oyununun UI feedback'inden sorumludur.
    /// DOTween animasyonları ile akıcı gösterim.
    /// Kural açıklaması, hata/başarı mesajları ve geri bildirim gösterir.
    /// </summary>
    public class SequenceUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text instructionText;

        [Header("Animasyon Ayarları")]
        [SerializeField] private float showDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.2f;

        private Sequence _currentSequence;

        /// <summary>
        /// Mesajı DOTween animasyonuyla gösterir, belirli süre sonra gizler.
        /// </summary>
        public void ShowFeedback(string message, float duration)
        {
            if (instructionText == null) return;

            KillCurrent();

            instructionText.gameObject.SetActive(true);
            instructionText.text = message;

            // Giriş animasyonu: Scale bounce + fade
            instructionText.transform.localScale = Vector3.one * 0.5f;
            instructionText.alpha = 0f;

            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(
                instructionText.transform.DOScale(Vector3.one, showDuration).SetEase(Ease.OutBack)
            );
            _currentSequence.Join(
                instructionText.DOFade(1f, showDuration * 0.6f)
            );

            // Bekle + çıkış animasyonu
            _currentSequence.AppendInterval(duration);
            _currentSequence.Append(
                instructionText.transform.DOScale(Vector3.zero, hideDuration).SetEase(Ease.InBack)
            );
            _currentSequence.Join(
                instructionText.DOFade(0f, hideDuration)
            );
            _currentSequence.OnComplete(() =>
            {
                if (instructionText != null)
                    instructionText.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// Kural açıklamasını gösterir (otomatik gizlenmez).
        /// </summary>
        public void ShowRuleDescription(string ruleDescription, float displayDuration)
        {
            if (instructionText == null) return;

            KillCurrent();

            instructionText.gameObject.SetActive(true);
            instructionText.text = ruleDescription;
            instructionText.alpha = 0f;
            instructionText.transform.localScale = Vector3.one * 1.2f;

            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(
                instructionText.DOFade(1f, 0.4f).SetEase(Ease.OutQuad)
            );
            _currentSequence.Join(
                instructionText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack)
            );
        }

        /// <summary>
        /// Instruction text'i animasyonla gizler.
        /// </summary>
        public void HideInstruction()
        {
            if (instructionText == null) return;

            KillCurrent();

            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(
                instructionText.DOFade(0f, hideDuration).SetEase(Ease.InQuad)
            );
            _currentSequence.Join(
                instructionText.transform.DOScale(Vector3.one * 0.5f, hideDuration).SetEase(Ease.InBack)
            );
            _currentSequence.OnComplete(() =>
            {
                if (instructionText != null)
                    instructionText.gameObject.SetActive(false);
            });
        }

        public void HideIfNotAcceptingInput(bool isAcceptingInput)
        {
            if (!isAcceptingInput)
                HideInstruction();
        }

        private void KillCurrent()
        {
            _currentSequence?.Kill();
            _currentSequence = null;
        }

        private void OnDestroy()
        {
            KillCurrent();
        }
    }
}
