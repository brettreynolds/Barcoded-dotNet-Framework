namespace Barcoded
{
    class Code128Validator : ILinearValidator
    {
        public string Parse(string text, Symbology symbology)
        {
            return LinearHelpers.GetOnlyASCII(text);
        }
    }
}
