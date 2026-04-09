using System.Collections.Generic;
using UnityEngine;
using SequenceCubes.Rules;

namespace SequenceCubes
{
    /// <summary>
    /// SRP: Sadece dizi üretiminden sorumludur.
    /// Aksiyonları oluşturur, decoy ekler ve kurala göre beklenen listeyi çıkarır.
    /// </summary>
    public class SequenceGenerator
    {
        private readonly SequenceCube[] _availableCubes;
        
        // Mevcut kurallar havuzu (OCP: Yeni kural eklemek → listeye yeni eleman ekle)
        private readonly ISequenceRule[] _rules;

        public SequenceGenerator(SequenceCube[] availableCubes)
        {
            _availableCubes = availableCubes;
            
            _rules = new ISequenceRule[]
            {
                new NormalRule(),
                new ReverseRule(),
                new OddIndicesRule(),
                new EvenIndicesRule()
            };
        }

        /// <summary>
        /// Yeni bir dizi üretir ve beklenen tıklama sırasını döndürür.
        /// </summary>
        /// <param name="sequenceLength">Bilgisayarın göstereceği toplam adım sayısı</param>
        /// <param name="decoyChance">Sahte (aşağı inen) aksiyon ihtimali (0-1)</param>
        /// <param name="generatedActions">Üretilen tüm aksiyonlar (output)</param>
        /// <param name="expectedSequence">Oyuncudan beklenen tıklama sırası (output)</param>
        /// <param name="selectedRule">Seçilen kural (output)</param>
        public void Generate(
            int sequenceLength,
            float decoyChance,
            out List<SequenceAction> generatedActions,
            out List<SequenceCube> expectedSequence,
            out ISequenceRule selectedRule)
        {
            generatedActions = new List<SequenceAction>();
            expectedSequence = new List<SequenceCube>();

            // 1. Rastgele kural seç
            selectedRule = _rules[Random.Range(0, _rules.Length)];

            // 2. Aksiyonları üret
            for (int i = 0; i < sequenceLength; i++)
            {
                SequenceCube randomCube = _availableCubes[Random.Range(0, _availableCubes.Length)];
                bool isDecoy = Random.value < decoyChance;
                generatedActions.Add(new SequenceAction { Cube = randomCube, IsDecoy = isDecoy });
            }

            // 3. En az 2 geçerli (yukarı çıkan) aksiyon olduğundan emin ol
            ForceValidActionsCount(generatedActions, 2);

            // 4. Sadece yukarı çıkanları filtrele
            List<SequenceCube> validUpCubes = new List<SequenceCube>();
            foreach (var action in generatedActions)
            {
                if (!action.IsDecoy)
                    validUpCubes.Add(action.Cube);
            }

            // 5. Kurala göre nihai beklenen listeyi oluştur
            expectedSequence = selectedRule.Filter(validUpCubes);
        }

        private void ForceValidActionsCount(List<SequenceAction> actions, int minCount)
        {
            int currentValid = 0;
            foreach (var action in actions)
            {
                if (!action.IsDecoy) currentValid++;
            }

            int idx = actions.Count - 1;
            while (currentValid < minCount && idx >= 0)
            {
                SequenceAction a = actions[idx];
                if (a.IsDecoy)
                {
                    a.IsDecoy = false;
                    actions[idx] = a;
                    currentValid++;
                }
                idx--;
            }
        }
    }
}
