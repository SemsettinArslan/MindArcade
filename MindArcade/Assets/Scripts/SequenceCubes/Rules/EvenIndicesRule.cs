using System.Collections.Generic;

namespace SequenceCubes.Rules
{
    /// <summary>
    /// Çift indeks kuralı: Sadece 2., 4., 6. yukarı çıkanları tıkla.
    /// (C# index: 1, 3, 5)
    /// </summary>
    public class EvenIndicesRule : ISequenceRule
    {
        public string GetDescription()
        {
            return "<color=#55FFFF>ATLAMALI HAMLE!\nSadece 2. ve 4. \nYUKARI Çıkanları Tıkla!</color>";
        }

        public List<SequenceCube> Filter(List<SequenceCube> validUpCubes)
        {
            var result = new List<SequenceCube>();
            for (int i = 1; i < validUpCubes.Count; i += 2)
            {
                result.Add(validUpCubes[i]);
            }
            return result;
        }
    }
}
