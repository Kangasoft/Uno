/* Classic Uno 2026
 * Version: 1.0
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
            if (selectedCard == null || topCard == null)
            {
                return false;
            }

            switch (topCard.Type)
            {
                /* Offical UNO stacking rules */
                case CardType.DrawTwo:
                    switch (selectedCard.Type)
                    {
                        case CardType.DrawTwo:
                        case CardType.DrawFour:
                            /* Draw Two cards cannot be stacked neither can Draw Four's (on a Draw Two). */
                            return false;
                    }
                    break;

                case CardType.DrawFour when selectedCard.Type == CardType.DrawTwo:
                    return false;
            }

            switch (selectedCard.Type)
            {
                case CardType.DrawFour:
                {
                    /* Wild Draw Four can only be played if the player has no cards matching the current colour. */
                    var hasMatchingColor = playerHand.Any(card =>
                        card != null && card.Color == currentColor && card.Type != CardType.Wild &&
                        card.Type != CardType.DrawFour);
                    return !hasMatchingColor;
                }

                case CardType.Wild:
                    /* A regular Wild can always be played. */
                    return true;
            }

            /* Match the current colour. */
            if (selectedCard.Color == currentColor)
            {
                return true;
            }

            /* Match the number on numeric cards. */
            if (selectedCard.Type == CardType.Numeric && topCard.Type == CardType.Numeric && selectedCard.Value == topCard.Value)
            {
                return true;
            }

            /* Match the action type. */
            return selectedCard.Type == topCard.Type && selectedCard.Type != CardType.Numeric;
        }
    }
}
