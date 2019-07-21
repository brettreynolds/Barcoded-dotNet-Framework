using System.Text.RegularExpressions;

namespace Barcoded
{
    internal class Code39Validator : ILinearValidator
    {
        /// <summary>
        /// Returns a string compatible with the given Code39 symbology.
        /// </summary>
        /// <param name="text">Text to convert from.</param>
        /// <param name="symbology">Code39 symbology to be used.</param>
        /// <returns>Code39 supported string.</returns>
        public string Parse(string text, Symbology symbology)
        {
            switch (symbology)
            {
                case Symbology.Code39Full:
                    return LinearHelpers.GetOnlyAscii(text);
                case Symbology.Code39FullC:
                    return LinearHelpers.GetOnlyAscii(text);
                default:
                    return GetOnlyCode39(text);
            }
        }

        /// <summary>
        /// Returns a string containing only ASCII characters.
        /// Non ASCII are converted to space character
        /// </summary>
        /// <param name="text">Text to remove non ASCII from.</param>
        /// <returns>ASCII only version of input text.</returns>
        private static string GetOnlyCode39(string text)
        {
            string returnString = text.ToUpper();
            Regex code39Regex = new Regex("[^-0-9A-Z.$/+% ]");
            returnString = code39Regex.Replace(returnString, " ");

            return returnString;
        }
    }
}
