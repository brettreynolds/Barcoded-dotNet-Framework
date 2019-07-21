namespace Barcoded
{
    internal class Code128Validator : ILinearValidator
    {
        public string Parse(string text, Symbology symbology)
        {
            return LinearHelpers.GetOnlyAscii(text);
        }
    }
}
