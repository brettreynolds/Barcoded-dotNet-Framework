namespace Barcoded
{
    internal class Interleaved2Of5Validator : ILinearValidator
    {
        public string Parse(string text, Symbology symbology)
        {
            switch (symbology)
            {
                case Symbology.I2of5:
                    return LinearHelpers.GetEvenNumeric(text);

                case Symbology.I2of5C:
                    return LinearHelpers.GetOddNumeric(text);

                default:
                    return LinearHelpers.GetOddNumeric(text);
            }
        }
    }
}
