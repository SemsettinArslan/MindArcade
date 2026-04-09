namespace MindArcade.Core
{
    /// <summary>
    /// Skor takibi interface'i. Her oyunun kendi skor hesaplaması farklı olabilir.
    /// </summary>
    public interface IScoreTracker
    {
        int CurrentScore { get; }
        float Accuracy { get; }
        void ReportResult(bool isCorrect);
        void Reset();
    }
}
