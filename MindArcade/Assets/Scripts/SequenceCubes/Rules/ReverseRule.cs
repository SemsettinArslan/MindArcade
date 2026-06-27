using System.Collections.Generic;

namespace SequenceCubes.Rules
{
    /// <summary>
    /// Ters kural: Yukarı çıkanları sondan başa doğru tıkla.
    /// </summary>
    public class ReverseRule : ISequenceRule
    {
        public string GetDescription()
        {
            return "<color=#FF5555>DIKKAT: TERS YON!\nYUKARI Cikanlari \nSondan Basa Dogru Tikla!</color>";
        }

        public List<SequenceCube> Filter(List<SequenceCube> validUpCubes)
        {
            var result = new List<SequenceCube>(validUpCubes);
            result.Reverse();
            return result;
        }
    }
}
