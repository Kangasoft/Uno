/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System.Collections.Generic;
using System.Linq;

namespace ClassicUno.Classes.Logic
{
    public static class Validation
    {
        public static bool IsCardPlayable(Card topCard, Card selectedCard, CardColor currentColor, IEnumerable<Card> playerHand)
        {
            if (topCard == null || selectedCard == null)
            {
                return false;
            }

            /* No stacking of Draw Two or Wild Draw Four cards. */
            switch (topCard.Type)
            {
                case CardType.DrawTwo:
                case CardType.DrawFour:
                    switch (selectedCard.Type)
                    {
                        case CardType.DrawTwo:
                        case CardType.DrawFour:
                            return false;
                    }
                    break;
            }

            switch (selectedCard.Type)
            {
                case CardType.Wild:
                    /* Wild cards can always be played. */
                    return true;

                case CardType.DrawFour:
                {
                    /* Wild Draw Four can only be played when the player
                     * has no other card matching the current colour. */
                    var hasMatchingColor = playerHand?.Any(card =>
                        card != null &&
                        card.Color == currentColor &&
                        card.Type != CardType.Wild &&
                        card.Type != CardType.DrawFour) == true;

                    return !hasMatchingColor;
                }
            }

            /* Match the current colour. */
            if (selectedCard.Color == currentColor)
            {
                return true;
            }

            /* Match the number. */
            if (selectedCard.Type == CardType.Numeric && topCard.Type == CardType.Numeric && selectedCard.Value == topCard.Value)
            {
                return true;
            }

            /* Match the action-card type. */
            return selectedCard.Type == topCard.Type && selectedCard.Type != CardType.Numeric;
        }
    }
}
