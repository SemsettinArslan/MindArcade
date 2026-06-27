using System.Collections.Generic;

namespace SequenceCubes.Rules
{
    /// <summary>
    /// Tek indeks kuralı: Sadece 1., 3., 5. yukarı çıkanları tıkla.
    /// (C# index: 0, 2, 4)
    /// </summary>
    public class OddIndicesRule : ISequenceRule
    {
        public string GetDescription()
        {
            return "<color=#FFAAFF>ATLAMALI HAMLE!\nSadece 1., 3. ve 5. \nYUKARI Cikanlari Tikla!</color>";
        }

        public List<SequenceCube> Filter(List<SequenceCube> validUpCubes)
        {
            var result = new List<SequenceCube>();
            for (int i = 0; i < validUpCubes.Count; i += 2)
            {
                result.Add(validUpCubes[i]);
            }
            return result;
        }
    }
}
