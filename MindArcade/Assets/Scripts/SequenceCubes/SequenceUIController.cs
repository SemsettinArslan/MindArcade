using System.Collections;
using UnityEngine;
using TMPro;

namespace SequenceCubes
{
    /// <summary>
    /// SRP: Sadece SequenceCubes oyununun UI feedback'inden sorumludur.
    /// Kural açıklaması, hata/başarı mesajları ve geri bildirim gösterir.
    /// </summary>
    public class SequenceUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text instructionText;

        /// <summary>
        /// Mesajı belirli bir süre gösterir, sonra gizler.
        /// </summary>
        public void ShowFeedback(string message, float duration)
        {
            if (instructionText == null) return;

            instructionText.gameObject.SetActive(true);
            instructionText.text = message;
            StartCoroutine(HideAfterDelay(duration));
        }

        /// <summary>
        /// Kural açıklamasını gösterir (belirli süre sonra otomatik gizlenir).
        /// </summary>
        public void ShowRuleDescription(string ruleDescription, float displayDuration)
        {
            if (instructionText == null) return;

            instructionText.gameObject.SetActive(true);
            instructionText.text = ruleDescription;
        }

        /// <summary>
        /// Instruction text'i hemen gizler.
        /// </summary>
        public void HideInstruction()
        {
            if (instructionText != null)
                instructionText.gameObject.SetActive(false);
        }

        public void HideIfNotAcceptingInput(bool isAcceptingInput)
        {
            if (instructionText != null && !isAcceptingInput)
                instructionText.gameObject.SetActive(false);
        }

        private IEnumerator HideAfterDelay(float duration)
        {
            yield return new WaitForSeconds(duration);
            // Eğer oynatım/input sürüyorsa saklamayız
            if (instructionText != null)
                instructionText.gameObject.SetActive(false);
        }
    }
}
