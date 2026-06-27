using System.Collections.Generic;

namespace SequenceCubes.Rules
{
    /// <summary>
    /// Normal kural: Yukarı çıkan küpleri sırasıyla tekrarla.
    /// </summary>
    public class NormalRule : ISequenceRule
    {
        public string GetDescription()
        {
            return "<color=yellow>YUKARI CIKANLARI\nSirasiyla Tekrarla!</color>";
        }

        public List<SequenceCube> Filter(List<SequenceCube> validUpCubes)
        {
            return new List<SequenceCube>(validUpCubes);
        }
    }
}
