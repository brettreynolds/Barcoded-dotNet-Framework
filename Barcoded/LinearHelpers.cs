using System;

namespace Barcoded
{
    internal static class LinearHelpers
    {
        /// <summary>
        /// Returns a string containing only ASCII characters.
        /// Non ASCII are converted to space character
        /// </summary>
        /// <param name="text">Text to remove non ASCII from.</param>
        /// <returns>ASCII only version of input text.</returns>
        internal static string GetOnlyAscii(string text)
        {
            string returnString = "";

            if(string.IsNullOrEmpty(text))
            {
                return returnString;
            }

            for (int position = 0; position <= text.Length - 1; position++)
            {
                int asciiCode = text[position];
                if (asciiCode > 127)
                {
                    returnString += " ";
                }
                else
                {
                    returnString += text.Substring(position, 1);
                }
            }
            return returnString;
        }

        /// <summary>
        /// Returns a string containing only numeric (0-9) characters.
        /// Non numeric characters are removed from the string
        /// </summary>
        /// <param name="text">Text to remove non numeric from.</param>
        /// <param name="length">Required string length</param>
        /// <returns>Numeric only version of input text.</returns>
        internal static string GetOnlyNumeric(string text, int length = 0)
        {
            string returnString = "";

            if (string.IsNullOrEmpty(text))
            {
                return returnString;
            }

            for (int position = 0; position <= text.Length - 1; position++)
            {
                int asciiCode = text[position];
                if (asciiCode >= 48 && asciiCode <= 57)
                {
                    returnString += text.Substring(position, 1);
                }
            }

            if (length > 0) returnString = returnString.Length > length? returnString.Substring(0, length) : returnString.PadLeft(length, '0');

            return returnString;
        }

        /// <summary>
        /// Returns a string containing only an even number of 
        /// numeric (0-9) characters.
        /// Non numeric characters are removed from the string
        /// and "0" prefixed to odd numbered numeric strings.
        /// </summary>
        /// <param name="text">Text to remove non numeric from and make even character count.</param>
        /// <returns>Even numbered character count, numeric only version of input text.</returns>
        internal static string GetEvenNumeric(string text)
        {
            string returnString = GetOnlyNumeric(text);

            if (returnString.Length % 2 == 1)
            {
                returnString = "0" + returnString;
            }

            return returnString;
        }

        /// <summary>
        /// Returns a string containing only an odd number of 
        /// numeric (0-9) characters.
        /// Non numeric characters are removed from the string
        /// and "0" prefixed to even numbered numeric strings.
        /// </summary>
        /// <param name="text">Text to remove non numeric from and make odd character count.</param>
        /// <returns>Odd numbered character count, numeric only version of input text.</returns>
        internal static string GetOddNumeric(string text)
        {
            string returnString = GetOnlyNumeric(text);

            if (returnString.Length % 2 != 1)
            {
                returnString = "0" + returnString;
            }

            return returnString;
        }

        internal static int GetUpcCheckDigit(string text)
        {
            string digits = GetOnlyNumeric(text);
            int checkDigitRunning = 0;
            int oddEven = text.Length % 2;

            for (int digitPosition = 0; digitPosition <= digits.Length - 1; digitPosition++)
            {
                if (digitPosition % 2 == oddEven)
                {
                    checkDigitRunning += Convert.ToInt32(digits.Substring(digitPosition, 1));
                }
                else
                {
                    checkDigitRunning += (Convert.ToInt32(digits.Substring(digitPosition, 1))) * 3;
                }
            }
            return (10 - checkDigitRunning % 10) % 10;
        }

        /// <summary>
        /// Returns the character type based on barcode symbology and character position.
        /// </summary>
        /// <param name="symbology"></param>
        /// <param name="characterPosition"></param>
        /// <param name="barcodeLength"></param>
        /// <remarks>Character Types: 0 = Encoded value & eye-readable, 1 = Control character, 3 = Encoded value & not eye-readable.</remarks>
        /// <returns>Character type as int</returns>
        internal static int GetCharacterType(Symbology symbology, int characterPosition, int barcodeLength)
        {
            int charPosAdjust = characterPosition > barcodeLength ? -1 : characterPosition;

            switch (symbology)
            {
                case Symbology.UpcA:

                    switch (charPosAdjust)
                    {
                        case -1:
                            return 2;

                        case 0:
                            return 2;

                        case 11:
                            return 2;

                        default:
                            return 0;
                    }

                case Symbology.Code128BAC:

                    switch (charPosAdjust)
                    {
                        case -1:
                            return 1;

                        case 0:
                            return 1;

                        default:
                            return 0;
                    }

                case Symbology.Code128ABC:

                    switch (charPosAdjust)
                    {
                        case -1:
                            return 1;

                        case 0:
                            return 1;

                        default:
                            return 0;
                    }

                case Symbology.Code128BA:

                    switch (charPosAdjust)
                    {
                        case -1:
                            return 1;

                        case 0:
                            return 1;

                        default:
                            return 0;
                    }

                case Symbology.Code128AB:

                    switch (charPosAdjust)
                    {
                        case -1:
                            return 1;

                        case 0:
                            return 1;

                        default:
                            return 0;
                    }

                case Symbology.GS1128:

                    switch (charPosAdjust)
                    {
                        case -1:
                            return 1;

                        case 0:
                            return 1;

                        case 1:
                            return 1;

                        default:
                            return 0;
                    }

                default:
                    return 0;
            }
        }
    }
}
