namespace SequenceCubes
{
    /// <summary>
    /// Bilgisayarın dizilimi yaparken aklında tutacağı komut zarfı.
    /// Hem küpün kimliği hem de kandırmacı (aşağı inme) özelliğini içerir.
    /// Eskiden SequenceGameManager içindeydi, SRP gereği ayrı dosyaya taşındı.
    /// </summary>
    public struct SequenceAction
    {
        public SequenceCube Cube;
        public bool IsDecoy;
    }
}
