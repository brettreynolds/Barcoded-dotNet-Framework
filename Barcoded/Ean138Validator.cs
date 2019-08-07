namespace Barcoded
{
    internal class Ean138Validator : ILinearValidator
    {
        public string Parse(string text, Symbology symbology)
        {
            switch (symbology)
            {
                case Symbology.Ean13:
                    return LinearHelpers.GetOnlyNumeric(text, 12);

                case Symbology.UpcA:
                    return LinearHelpers.GetOnlyNumeric(text, 11);

                case Symbology.Ean8:
                    return LinearHelpers.GetOnlyNumeric(text, 7);

                default:
                    return LinearHelpers.GetOnlyNumeric(text, 12);
            }
        }
    }
}
