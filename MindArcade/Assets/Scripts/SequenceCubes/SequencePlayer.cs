using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SequenceCubes
{
    /// <summary>
    /// SRP: Sadece üretilmiş sekansı ekranda oynatmaktan sorumludur.
    /// Küplerin animasyonunu sıralı olarak tetikler.
    /// </summary>
    public class SequencePlayer : MonoBehaviour
    {
        [Tooltip("Küpler arasındaki bekleme süresi")]
        [SerializeField] private float delayBetweenCubes = 0.5f;

        private float _defaultDelayBetweenCubes;

        private void Awake()
        {
            _defaultDelayBetweenCubes = delayBetweenCubes;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            if (multiplier <= 0.1f) multiplier = 0.1f;
            delayBetweenCubes = _defaultDelayBetweenCubes / multiplier;
        }

        /// <summary>
        /// Verilen aksiyon listesini ekranda sıralı olarak oynatır.
        /// </summary>
        /// <param name="actions">Oynatılacak aksiyonlar</param>
        /// <param name="onComplete">Tüm oynatım bittiğinde çağrılır</param>
        public void PlaySequence(List<SequenceAction> actions, Action onComplete)
        {
            StartCoroutine(PlayRoutine(actions, onComplete));
        }

        public void StopPlaying()
        {
            StopAllCoroutines();
        }

        private IEnumerator PlayRoutine(List<SequenceAction> actions, Action onComplete)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                bool isAnimationFinished = false;
                
                actions[i].Cube.PlayAnimation(actions[i].IsDecoy, () => isAnimationFinished = true);

                while (!isAnimationFinished) yield return null;

                yield return new WaitForSeconds(delayBetweenCubes);
            }

            onComplete?.Invoke();
        }
    }
}
