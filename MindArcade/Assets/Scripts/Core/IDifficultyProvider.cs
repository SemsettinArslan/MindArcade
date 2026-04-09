namespace MindArcade.Core
{
    /// <summary>
    /// Zorluk sağlayıcı interface'i. Oyunlara göre seed, hız, spawn frekansı
    /// gibi parametreleri soyutlar.
    /// </summary>
    public interface IDifficultyProvider
    {
        int CurrentLevel { get; }
        void ReportPlayerAction(bool isCorrect);
    }
}
