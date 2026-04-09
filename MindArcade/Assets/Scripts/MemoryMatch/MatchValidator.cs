using System;

namespace MemoryMatch
{
    /// <summary>
    /// SRP: Sadece iki kartın eşleşip eşleşmediğini doğrular.
    /// İlk kart ve ikinci kart seçimini takip eder.
    /// </summary>
    public class MatchValidator
    {
        public Card FirstCard { get; private set; }
        public Card SecondCard { get; private set; }

        public bool IsWaitingForSecondCard => FirstCard != null && SecondCard == null;

        public event Action<Card, Card, bool> OnMatchEvaluated; // card1, card2, isMatch

        /// <summary>
        /// Kart seçimini işler. İlk kartsa kaydeder, ikinciyse eşleştirme yapar.
        /// </summary>
        /// <returns>
        /// true: eşleştirme yapıldı (ikinci kart seçildi), false: ilk kart seçildi (bekleniyor)
        /// </returns>
        public bool ProcessCardSelection(Card clickedCard)
        {
            if (FirstCard == null)
            {
                FirstCard = clickedCard;
                FirstCard.SetSelectedStatus(true);
                return false; // İkinci kartı bekliyoruz
            }

            if (clickedCard == FirstCard) return false; // Aynı kart tekrar

            SecondCard = clickedCard;
            bool isMatch = FirstCard.Id == SecondCard.Id;

            if (isMatch)
            {
                FirstCard.SetSelectedStatus(false);
                FirstCard.SetMatched();
                SecondCard.SetSelectedStatus(false);
                SecondCard.SetMatched();
            }
            else
            {
                FirstCard.SetSelectedStatus(false);
                FirstCard.PlayErrorEffect();
                SecondCard.PlayErrorEffect();
            }

            OnMatchEvaluated?.Invoke(FirstCard, SecondCard, isMatch);
            return true;
        }

        /// <summary>
        /// Tur sıfırlama — seçilen kartları temizler.
        /// </summary>
        public void ResetTurn()
        {
            if (FirstCard != null)
                FirstCard.SetSelectedStatus(false);

            FirstCard = null;
            SecondCard = null;
        }
    }
}
