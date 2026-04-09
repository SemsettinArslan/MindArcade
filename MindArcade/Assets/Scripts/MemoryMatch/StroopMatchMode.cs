namespace MemoryMatch
{
    /// <summary>
    /// Memory Match oyununun eşleştirme modları.
    /// Eskiden MemoryGameManager içindeydi, SRP gereği ayrı dosyaya taşındı.
    /// </summary>
    public enum StroopMatchMode
    {
        TextVsColor,   // Sahnede 8 metin, 8 blok renk arkaplanı olur. Metnin anlattığıyla bloğu eşleştirirsiniz.
        TextVsText     // Ekranda 16 kartın 16'sı da metindir. Aynı KELİMELERİ eşleştirirsiniz.
    }
}
