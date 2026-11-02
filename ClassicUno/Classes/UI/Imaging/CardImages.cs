/* Classic Uno 2026
 * Version: 1.0.1
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;
using System.Drawing;
using ClassicUno.Classes.Logic;
using ClassicUno.Properties;

namespace ClassicUno.Classes.UI.Imaging
{
    public class CardImages : IDisposable
    {
        #region Card key pair class
        /* KeyPair class for Dictionary object */
        private class CardKey
        {
            private readonly CardColor _color;
            private readonly CardType _type;
            private readonly int _value;

            public CardKey(CardColor color, CardType type, int value)
            {
                _color = color;
                _type = type;
                _value = value;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CardKey other))
                {
                    return false;
                }
                return _color == other._color && _type == other._type && _value == other._value;
            }

            public override int GetHashCode()
            {
                return (int)_color * 10000 + (int)_type * 100 + _value;
            }
        }
        #endregion

        #region Fields
        private readonly Dictionary<CardKey, Bitmap> _cardImages = new Dictionary<CardKey, Bitmap>();

        private const int MaxBackImages = 9;

        private bool _disposed;

        /* UNO! card images - this separates current game play card data from the actual bitmaps. This makes it easier
         * to create a smaller "game-save" file without including the images in the binary data file. */
        public List<Image> CardBackImages = new List<Image>();
        #endregion

        #region Constructor
        public CardImages()
        {
            BuildCardImages();
            BuildCardBacks();
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            /* Dispose of all cropped card images. */
            foreach (var image in _cardImages.Values)
            {
                image.Dispose();
            }

            _cardImages.Clear();

            /* Dispose of all cropped card-back images. */
            foreach (var image in CardBackImages)
            {
                image.Dispose();
            }

            CardBackImages.Clear();

            _disposed = true;
        }
        #endregion

        #region Card image getting
        public Bitmap GetCardImage(Card card)
        {
            /* This returns the bitmap image related to the UNO! card */
            var key = new CardKey(card.Color, card.Type, card.Value);
            return _cardImages.TryGetValue(key, out var image) ? image : null;
        }
        #endregion

        #region Private methods
        private void BuildCardImages()
        {
            var image = Resources.cards;
            var startY = 0;

            /* Loop through all four colors (this is the "Y" direction */
            for (var color = 0; color <= 3; color++)
            {
                /* Loop though all 14 cards of the resource image in the "X" direction "cutting-out" the images,
                 * or cropping, into a single bitmap of each card - paying particular attention to "special" cards;
                 * such as Wild, Wild Draw Four, Skip, UNO! Reverse, etc. */

                var startX = 0; /* This must be reset each new "Y" iteration */

                for (var i = 0; i <= 13; i++)
                {
                    var rect = new Rectangle(startX, startY, 144, 216);
                    var cardImg = image.Clone(rect, image.PixelFormat);

                    /* Check which "X" position we are currently at for "special" cards, everything else is numeric */
                    switch (i)
                    {
                        case 10:
                            /* Skip */
                            AddCardImage((CardColor)color + 1, CardType.Skip, 0, cardImg);
                            break;

                        case 11:
                            /* Reverse */
                            AddCardImage((CardColor)color + 1, CardType.Reverse, 0, cardImg);
                            break;

                        case 12:
                            /* Draw two */
                            AddCardImage((CardColor)color + 1, CardType.DrawTwo, 0, cardImg);
                            break;

                        case 13:
                            switch (color)
                            {
                                /* Wild/wild draw four */
                                case 0:
                                    /* Wild */
                                    AddCardImage(CardColor.None, CardType.Wild, 0, cardImg);
                                    break;

                                case 1:
                                    /* Wild draw four */
                                    AddCardImage(CardColor.None, CardType.DrawFour, 0, cardImg);
                                    break;

                                default:
                                    cardImg.Dispose();
                                    break;
                            }
                            break;

                        default:
                            /* Numeric */
                            AddCardImage((CardColor)color + 1, CardType.Numeric, i, cardImg);
                            break;
                    }
                    startX += 144; /* Card image width */
                }
                startY += 216; /* Card image height */
            }
        }

        private void BuildCardBacks()
        {
            var image = Resources.cardBacks;
            var startX = 0;
            for (var i = 0; i < MaxBackImages; i++)
            {
                var rect = new Rectangle(startX, 0, 144, 216);
                var cardImg = image.Clone(rect, image.PixelFormat);
                CardBackImages.Add(cardImg);
                startX += 144;
            }
        }

        private void AddCardImage(CardColor color, CardType type, int value, Bitmap image)
        {
            /* Create a new dictionary entry in CardImages based on parameters */
            var key = new CardKey(color, type, value);
            _cardImages.Add(key, image);
        }
        #endregion
    }
}
