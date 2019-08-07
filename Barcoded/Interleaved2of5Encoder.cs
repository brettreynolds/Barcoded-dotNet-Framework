using System;
using System.Collections.Generic;

namespace Barcoded
{
    /// <summary>
    /// Interleaved 2 of 5 barcode encoder.
    /// </summary>
    internal class Interleaved2Of5Encoder : LinearEncoder
    {
        private Dictionary<string, LinearPattern> _patternDictionary;
        private readonly bool _useCheckDigit;

        public Interleaved2Of5Encoder(Symbology symbology) : base(symbology)
        {
            switch (Symbology)
            {
                case Symbology.I2of5:
                    // Interleaved 2 of 5
                    _useCheckDigit = false;
                    Description = "Interleaved 2 of 5";
                    break;

                case Symbology.I2of5C:
                    // Interleaved 2 of 5 with check digit
                    _useCheckDigit = true;
                    Description = "Interleaved 2 of 5 With Check Digit";
                    break;

                default:
                    // Interleaved 2 of 5 with check digit
                    _useCheckDigit = true;
                    Description = "Interleaved 2 of 5 With Check Digit";
                    break;
            }
        }

        internal override ILinearValidator BarcodeValidator { get; } = new Interleaved2Of5Validator();

        internal override void Encode(string barcodeValue)
        {
            ZplEncode = "";
            LoadSymbologyPattern();

            // Add check digit to barcode value.
            if (_useCheckDigit)
            {
                int checkDigit = LinearHelpers.GetUpcCheckDigit(barcodeValue);
                barcodeValue += checkDigit;
            }
            EncodedValue = barcodeValue;

            for (int encodePosition = 0; encodePosition <= barcodeValue.Length - 1; encodePosition += 2)
            {
                // Check if first or last character in barcode and insert start/stop symbol
                if (encodePosition == 0)
                {
                    LinearEncoding.Add("*", 1, _patternDictionary["START"]);
                }

                string digitPair = barcodeValue.Substring(encodePosition, 2);
                LinearEncoding.Add(digitPair, 0, _patternDictionary[digitPair]);


                if (encodePosition == barcodeValue.Length - 2)
                {
                    LinearEncoding.Add("*", 1, _patternDictionary["STOP"]);
                }
            }

            SetMinXDimension();
            SetMinBarcodeHeight();

        }

        /// <summary>
        /// Increases the barcode Xdimension to minimum required by symbology, if currently set lower
        /// </summary>
        internal override void SetMinXDimension()
        {
            int xdimensionOriginal = XDimension;
            int minXdimension = (int)Math.Ceiling(Dpi * 0.0075);
            XDimension = Math.Max(XDimension, minXdimension);

            // Set flag to show xdimension was adjusted
            if (xdimensionOriginal != XDimension)
            {
                XDimensionChanged = true;
            }
        }

        /// <summary>
        /// Increases the barcode height to minimum required by symbology, if currently set lower
        /// </summary>
        internal override void SetMinBarcodeHeight()
        {
            int barcodeHeightOriginal = BarcodeHeight;
            int minBarcodeHeight = (int)Math.Ceiling(Math.Max(LinearEncoding.MinimumWidth * XDimension * 0.15, Dpi * 0.25));
            BarcodeHeight = Math.Max(BarcodeHeight, minBarcodeHeight);

            // Set flag to show barcode height was adjusted
            if (barcodeHeightOriginal != BarcodeHeight)
            {
                BarcodeHeightChanged = true;
            }
        }

        private void LoadSymbologyPattern()
        {
            if (_patternDictionary != null)
            {
                return;
            }

            _patternDictionary = new Dictionary<string, LinearPattern>
            {
                {"00", new LinearPattern("NNNNWWWWNN", ModuleType.Bar, WideBarRatio)},
                {"01", new LinearPattern("NWNNWNWNNW", ModuleType.Bar, WideBarRatio)},
                {"02", new LinearPattern("NNNWWNWNNW", ModuleType.Bar, WideBarRatio)},
                {"03", new LinearPattern("NWNWWNWNNN", ModuleType.Bar, WideBarRatio)},
                {"04", new LinearPattern("NNNNWWWNNW", ModuleType.Bar, WideBarRatio)},
                {"05", new LinearPattern("NWNNWWWNNN", ModuleType.Bar, WideBarRatio)},
                {"06", new LinearPattern("NNNWWWWNNN", ModuleType.Bar, WideBarRatio)},
                {"07", new LinearPattern("NNNNWNWWNW", ModuleType.Bar, WideBarRatio)},
                {"08", new LinearPattern("NWNNWNWWNN", ModuleType.Bar, WideBarRatio)},
                {"09", new LinearPattern("NNNWWNWWNN", ModuleType.Bar, WideBarRatio)},
                {"10", new LinearPattern("WNNNNWNWWN", ModuleType.Bar, WideBarRatio)},
                {"11", new LinearPattern("WWNNNNNNWW", ModuleType.Bar, WideBarRatio)},
                {"12", new LinearPattern("WNNWNNNNWW", ModuleType.Bar, WideBarRatio)},
                {"13", new LinearPattern("WWNWNNNNWN", ModuleType.Bar, WideBarRatio)},
                {"14", new LinearPattern("WNNNNWNNWW", ModuleType.Bar, WideBarRatio)},
                {"15", new LinearPattern("WWNNNWNNWN", ModuleType.Bar, WideBarRatio)},
                {"16", new LinearPattern("WNNWNWNNWN", ModuleType.Bar, WideBarRatio)},
                {"17", new LinearPattern("WNNNNNNWWW", ModuleType.Bar, WideBarRatio)},
                {"18", new LinearPattern("WWNNNNNWWN", ModuleType.Bar, WideBarRatio)},
                {"19", new LinearPattern("WNNWNNNWWN", ModuleType.Bar, WideBarRatio)},
                {"20", new LinearPattern("NNWNNWNWWN", ModuleType.Bar, WideBarRatio)},
                {"21", new LinearPattern("NWWNNNNNWW", ModuleType.Bar, WideBarRatio)},
                {"22", new LinearPattern("NNWWNNNNWW", ModuleType.Bar, WideBarRatio)},
                {"23", new LinearPattern("NWWWNNNNWN", ModuleType.Bar, WideBarRatio)},
                {"24", new LinearPattern("NNWNNWNNWW", ModuleType.Bar, WideBarRatio)},
                {"25", new LinearPattern("NWWNNWNNWN", ModuleType.Bar, WideBarRatio)},
                {"26", new LinearPattern("NNWWNWNNWN", ModuleType.Bar, WideBarRatio)},
                {"27", new LinearPattern("NNWNNNNWWW", ModuleType.Bar, WideBarRatio)},
                {"28", new LinearPattern("NWWNNNNWWN", ModuleType.Bar, WideBarRatio)},
                {"29", new LinearPattern("NNWWNNNWWN", ModuleType.Bar, WideBarRatio)},
                {"30", new LinearPattern("WNWNNWNWNN", ModuleType.Bar, WideBarRatio)},
                {"31", new LinearPattern("WWWNNNNNNW", ModuleType.Bar, WideBarRatio)},
                {"32", new LinearPattern("WNWWNNNNNW", ModuleType.Bar, WideBarRatio)},
                {"33", new LinearPattern("WWWWNNNNNN", ModuleType.Bar, WideBarRatio)},
                {"34", new LinearPattern("WNWNNWNNNW", ModuleType.Bar, WideBarRatio)},
                {"35", new LinearPattern("WWWNNWNNNN", ModuleType.Bar, WideBarRatio)},
                {"36", new LinearPattern("WNWWNWNNNN", ModuleType.Bar, WideBarRatio)},
                {"37", new LinearPattern("WNWNNNNWNW", ModuleType.Bar, WideBarRatio)},
                {"38", new LinearPattern("WWWNNNNWNN", ModuleType.Bar, WideBarRatio)},
                {"39", new LinearPattern("WNWWNNNWNN", ModuleType.Bar, WideBarRatio)},
                {"40", new LinearPattern("NNNNWWNWWN", ModuleType.Bar, WideBarRatio)},
                {"41", new LinearPattern("NWNNWNNNWW", ModuleType.Bar, WideBarRatio)},
                {"42", new LinearPattern("NNNWWNNNWW", ModuleType.Bar, WideBarRatio)},
                {"43", new LinearPattern("NWNWWNNNWN", ModuleType.Bar, WideBarRatio)},
                {"44", new LinearPattern("NNNNWWNNWW", ModuleType.Bar, WideBarRatio)},
                {"45", new LinearPattern("NWNNWWNNWN", ModuleType.Bar, WideBarRatio)},
                {"46", new LinearPattern("NNNWWWNNWN", ModuleType.Bar, WideBarRatio)},
                {"47", new LinearPattern("NNNNWNNWWW", ModuleType.Bar, WideBarRatio)},
                {"48", new LinearPattern("NWNNWNNWWN", ModuleType.Bar, WideBarRatio)},
                {"49", new LinearPattern("NNNWWNNWWN", ModuleType.Bar, WideBarRatio)},
                {"50", new LinearPattern("WNNNWWNWNN", ModuleType.Bar, WideBarRatio)},
                {"51", new LinearPattern("WWNNWNNNNW", ModuleType.Bar, WideBarRatio)},
                {"52", new LinearPattern("WNNWWNNNNW", ModuleType.Bar, WideBarRatio)},
                {"53", new LinearPattern("WWNWWNNNNN", ModuleType.Bar, WideBarRatio)},
                {"54", new LinearPattern("WNNNWWNNNW", ModuleType.Bar, WideBarRatio)},
                {"55", new LinearPattern("WWNNWWNNNN", ModuleType.Bar, WideBarRatio)},
                {"56", new LinearPattern("WNNWWWNNNN", ModuleType.Bar, WideBarRatio)},
                {"57", new LinearPattern("WNNNWNNWNW", ModuleType.Bar, WideBarRatio)},
                {"58", new LinearPattern("WWNNWNNWNN", ModuleType.Bar, WideBarRatio)},
                {"59", new LinearPattern("WNNWWNNWNN", ModuleType.Bar, WideBarRatio)},
                {"60", new LinearPattern("NNWNWWNWNN", ModuleType.Bar, WideBarRatio)},
                {"61", new LinearPattern("NWWNWNNNNW", ModuleType.Bar, WideBarRatio)},
                {"62", new LinearPattern("NNWWWNNNNW", ModuleType.Bar, WideBarRatio)},
                {"63", new LinearPattern("NWWWWNNNNN", ModuleType.Bar, WideBarRatio)},
                {"64", new LinearPattern("NNWNWWNNNW", ModuleType.Bar, WideBarRatio)},
                {"65", new LinearPattern("NWWNWWNNNN", ModuleType.Bar, WideBarRatio)},
                {"66", new LinearPattern("NNWWWWNNNN", ModuleType.Bar, WideBarRatio)},
                {"67", new LinearPattern("NNWNWNNWNW", ModuleType.Bar, WideBarRatio)},
                {"68", new LinearPattern("NWWNWNNWNN", ModuleType.Bar, WideBarRatio)},
                {"69", new LinearPattern("NNWWWNNWNN", ModuleType.Bar, WideBarRatio)},
                {"70", new LinearPattern("NNNNNWWWWN", ModuleType.Bar, WideBarRatio)},
                {"71", new LinearPattern("NWNNNNWNWW", ModuleType.Bar, WideBarRatio)},
                {"72", new LinearPattern("NNNWNNWNWW", ModuleType.Bar, WideBarRatio)},
                {"73", new LinearPattern("NWNWNNWNWN", ModuleType.Bar, WideBarRatio)},
                {"74", new LinearPattern("NNNNNWWNWW", ModuleType.Bar, WideBarRatio)},
                {"75", new LinearPattern("NWNNNWWNWN", ModuleType.Bar, WideBarRatio)},
                {"76", new LinearPattern("NNNWNWWNWN", ModuleType.Bar, WideBarRatio)},
                {"77", new LinearPattern("NNNNNNWWWW", ModuleType.Bar, WideBarRatio)},
                {"78", new LinearPattern("NWNNNNWWWN", ModuleType.Bar, WideBarRatio)},
                {"79", new LinearPattern("NNNWNNWWWN", ModuleType.Bar, WideBarRatio)},
                {"80", new LinearPattern("WNNNNWWWNN", ModuleType.Bar, WideBarRatio)},
                {"81", new LinearPattern("WWNNNNWNNW", ModuleType.Bar, WideBarRatio)},
                {"82", new LinearPattern("WNNWNNWNNW", ModuleType.Bar, WideBarRatio)},
                {"83", new LinearPattern("WWNWNNWNNN", ModuleType.Bar, WideBarRatio)},
                {"84", new LinearPattern("WNNNNWWNNW", ModuleType.Bar, WideBarRatio)},
                {"85", new LinearPattern("WWNNNWWNNN", ModuleType.Bar, WideBarRatio)},
                {"86", new LinearPattern("WNNWNWWNNN", ModuleType.Bar, WideBarRatio)},
                {"87", new LinearPattern("WNNNNNWWNW", ModuleType.Bar, WideBarRatio)},
                {"88", new LinearPattern("WWNNNNWWNN", ModuleType.Bar, WideBarRatio)},
                {"89", new LinearPattern("WNNWNNWWNN", ModuleType.Bar, WideBarRatio)},
                {"90", new LinearPattern("NNWNNWWWNN", ModuleType.Bar, WideBarRatio)},
                {"91", new LinearPattern("NWWNNNWNNW", ModuleType.Bar, WideBarRatio)},
                {"92", new LinearPattern("NNWWNNWNNW", ModuleType.Bar, WideBarRatio)},
                {"93", new LinearPattern("NWWWNNWNNN", ModuleType.Bar, WideBarRatio)},
                {"94", new LinearPattern("NNWNNWWNNW", ModuleType.Bar, WideBarRatio)},
                {"95", new LinearPattern("NWWNNWWNNN", ModuleType.Bar, WideBarRatio)},
                {"96", new LinearPattern("NNWWNWWNNN", ModuleType.Bar, WideBarRatio)},
                {"97", new LinearPattern("NNWNNNWWNW", ModuleType.Bar, WideBarRatio)},
                {"98", new LinearPattern("NWWNNNWWNN", ModuleType.Bar, WideBarRatio)},
                {"99", new LinearPattern("NNWWNNWWNN", ModuleType.Bar, WideBarRatio)},
                {"START", new LinearPattern("NNNN", ModuleType.Bar, WideBarRatio)},
                {"STOP", new LinearPattern("WNN", ModuleType.Bar, WideBarRatio)}
            };
        }
    }
}
