namespace MindArcade.Core
{
    /// <summary>
    /// Tüm oyunların ortak yaşam döngüsü interface'i.
    /// Her oyunun GameManager'ı bu interface'i implement etmeli.
    /// </summary>
    public interface IGameManager
    {
        bool IsGameRunning { get; }
        void StartGame();
        void EndGame();
    }
}
