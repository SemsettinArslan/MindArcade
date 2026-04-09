using System.Collections.Generic;

namespace SequenceCubes.Rules
{
    /// <summary>
    /// OCP: Yeni kural eklemek için switch/if değiştirmek yerine
    /// bu interface'i implement eden yeni bir sınıf yazılır.
    /// </summary>
    public interface ISequenceRule
    {
        /// <summary>
        /// Kural açıklaması (UI'da gösterilecek).
        /// </summary>
        string GetDescription();

        /// <summary>
        /// Yukarı çıkan (valid) küp listesini kurala göre filtreler.
        /// Örn: Reverse kuralı listeyi ters çevirir, OddIndices sadece tek indexleri alır.
        /// </summary>
        List<SequenceCube> Filter(List<SequenceCube> validUpCubes);
    }
}
