using System;
using System.Collections.Generic;

namespace Barcoded
{
    /// <summary>
    /// EAN-13, EAN-8 & UPC-A barcode encoder.
    /// </summary>
    class Ean138Encoder : LinearEncoder
    {
        private Dictionary<string, LinearPattern> _patternDictionary;
        private Dictionary<int, string> _groupDictionary;

        public Ean138Encoder(Symbology symbology) : base(symbology)
        {
            switch (Symbology)
            {
                case Symbology.Ean13:
                    // EAN-13
                    Description = "EAN-13";
                    break;

                case Symbology.UpcA:
                    // UPC-A
                    Description = "UPC-A";
                    break;

                case Symbology.Ean8:
                    // EAN-13
                    Description = "EAN-8";
                    break;

                default:
                    // EAN-13
                    Description = "EAN-13";
                    break;
            }
        }

        internal override ILinearValidator BarcodeValidator { get; } = new Ean138Validator();

        internal override void Encode(string barcodeValue)
        {
            ZplEncode = "";
            LoadSymbologyPattern();
            LoadGroupPattern();

            int startPoint;
            int midPoint;
            int endPoint;
            string group;
            int encodeOffset;
            int checkDigit = LinearHelpers.GetUpcCheckDigit(barcodeValue);

            switch (Symbology)
            {
                case Symbology.Ean13:
                    startPoint = 1;
                    midPoint = 7;
                    endPoint = 12;
                    // Set the group pattern to use, based on the first digit of the barcode.
                    group = _groupDictionary[barcodeValue[0]];
                    // Set human readable prefix to the first digit of the barcode value. 
                    LinearEncoding.HumanReadablePrefix = barcodeValue.Substring(0, 1);
                    LinearEncoding.HumanReadableSuffix = ">";
                    encodeOffset = 1;
                    break;

                case Symbology.UpcA:
                    startPoint = 0;
                    midPoint = 6;
                    endPoint = 11;
                    // Set UPC-A group pattern
                    group = _groupDictionary[48];
                    // Set human readable prefix to the first digit of the barcode value. 
                    LinearEncoding.HumanReadablePrefix = barcodeValue.Substring(0, 1);
                    // Set human readable suffix to the check digit value of the barcode. 
                    LinearEncoding.HumanReadableSuffix = checkDigit.ToString();
                    encodeOffset = 0;
                    break;

                case Symbology.Ean8:
                    startPoint = 0;
                    midPoint = 4;
                    endPoint = 7;
                    // Set EAN-8 group pattern
                    group = _groupDictionary[8];
                    LinearEncoding.HumanReadablePrefix = "<";
                    LinearEncoding.HumanReadableSuffix = ">";
                    encodeOffset = 0;
                    break;

                default:
                    startPoint = 1;
                    midPoint = 7;
                    endPoint = 12;
                    // Set the group pattern to use, based on the first digit of the barcode.
                    group = _groupDictionary[barcodeValue[0]];
                    // Set human readable prefix to the first digit of the barcode value. 
                    LinearEncoding.HumanReadablePrefix = barcodeValue.Substring(0, 1);
                    LinearEncoding.HumanReadableSuffix = ">";
                    encodeOffset = 1;
                    break;
            }

            for (int encodePosition = startPoint; encodePosition <= endPoint - 1; encodePosition++)
            {
                // Check if first digit in barcode and insert start symbol.
                if (encodePosition == startPoint)
                {
                    LinearEncoding.Add("*", 1, _patternDictionary["START"]);
                }

                // Check if mid point and insert separator.
                if (encodePosition == midPoint)
                {
                    LinearEncoding.Add("*", 1, _patternDictionary["MID"]);
                }

                int characterType = LinearHelpers.GetCharacterType(Symbology, encodePosition, barcodeValue.Length);
                string digitEncode = barcodeValue.Substring(encodePosition, 1) + group.Substring(encodePosition - encodeOffset, 1);
                LinearEncoding.Add(digitEncode.Substring(0,1), characterType, _patternDictionary[digitEncode]);

                // Add check digit and stop symbol if last encoding character.
                if (encodePosition == endPoint - 1)
                {
                    characterType = LinearHelpers.GetCharacterType(Symbology, encodePosition + 1, barcodeValue.Length);
                    string checkDigitCode = checkDigit + group.Substring(encodePosition - encodeOffset, 1);
                    LinearEncoding.Add(checkDigitCode.Substring(0, 1), characterType, _patternDictionary[checkDigitCode]);
                    EncodedValue = barcodeValue + checkDigit;
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
                {"0L", new LinearPattern("3211", ModuleType.Space)},
                {"1L", new LinearPattern("2221", ModuleType.Space)},
                {"2L", new LinearPattern("2122", ModuleType.Space)},
                {"3L", new LinearPattern("1411", ModuleType.Space)},
                {"4L", new LinearPattern("1132", ModuleType.Space)},
                {"5L", new LinearPattern("1231", ModuleType.Space)},
                {"6L", new LinearPattern("1114", ModuleType.Space)},
                {"7L", new LinearPattern("1312", ModuleType.Space)},
                {"8L", new LinearPattern("1213", ModuleType.Space)},
                {"9L", new LinearPattern("3112", ModuleType.Space)},
                {"0G", new LinearPattern("1123", ModuleType.Space)},
                {"1G", new LinearPattern("1222", ModuleType.Space)},
                {"2G", new LinearPattern("2212", ModuleType.Space)},
                {"3G", new LinearPattern("1141", ModuleType.Space)},
                {"4G", new LinearPattern("2311", ModuleType.Space)},
                {"5G", new LinearPattern("1321", ModuleType.Space)},
                {"6G", new LinearPattern("4111", ModuleType.Space)},
                {"7G", new LinearPattern("2131", ModuleType.Space)},
                {"8G", new LinearPattern("3121", ModuleType.Space)},
                {"9G", new LinearPattern("2113", ModuleType.Space)},
                {"0R", new LinearPattern("3211", ModuleType.Bar)},
                {"1R", new LinearPattern("2221", ModuleType.Bar)},
                {"2R", new LinearPattern("2122", ModuleType.Bar)},
                {"3R", new LinearPattern("1411", ModuleType.Bar)},
                {"4R", new LinearPattern("1132", ModuleType.Bar)},
                {"5R", new LinearPattern("1231", ModuleType.Bar)},
                {"6R", new LinearPattern("1114", ModuleType.Bar)},
                {"7R", new LinearPattern("1312", ModuleType.Bar)},
                {"8R", new LinearPattern("1213", ModuleType.Bar)},
                {"9R", new LinearPattern("3112", ModuleType.Bar)},
                {"START", new LinearPattern("111", ModuleType.Bar)},
                {"MID", new LinearPattern("11111", ModuleType.Space)},
                {"STOP", new LinearPattern("111", ModuleType.Bar)}
            };
        }

        private void LoadGroupPattern()
        {
            if (_groupDictionary != null)
            {
                return;
            }

            _groupDictionary = new Dictionary<int, string>
            {
                {48, "LLLLLLRRRRRR"},   // 0 & UPC-A
                {49, "LLGLGGRRRRRR"},   // 1
                {50, "LLGGLGRRRRRR"},   // 2
                {51, "LLGGGLRRRRRR"},   // 3
                {52, "LGLLGGRRRRRR"},   // 4
                {53, "LGGLLGRRRRRR"},   // 5
                {54, "LGGGLLRRRRRR"},   // 6
                {55, "LGLGLGRRRRRR"},   // 7
                {56, "LGLGGLRRRRRR"},   // 8
                {57, "LGGLGLRRRRRR"},   // 9
                {8, "LLLLRRRR" }        // Ean-8
            };
        }
    }
}
