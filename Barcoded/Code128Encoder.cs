using System;
using System.Collections.Generic;

namespace Barcoded
{
    /// <summary>
    /// Code 128 barcode encoder.
    /// </summary>
    internal class Code128Encoder : LinearEncoder
    {
        private Dictionary<string, int> _symbologyCharEncode;
        private Dictionary<int, LinearPattern> _patternDictionary;

        private int _checkDigit;

        private bool SuppressSubsetC { get; }

        private Code128Subset StartSubset { get; }

        public Code128Encoder(Symbology symbology) : base(symbology)
        {
            switch (Symbology)
            {
                case Symbology.Code128ABC:
                    SuppressSubsetC = false;
                    StartSubset = Code128Subset.A;
                    Description = "Code 128 - Subset A Prioritised";
                    break;
                case Symbology.Code128BAC:
                    SuppressSubsetC = false;
                    StartSubset = Code128Subset.B;
                    Description = "Code 128 - Subset B Prioritised";
                    break;
                case Symbology.Code128AB:
                    SuppressSubsetC = true;
                    StartSubset = Code128Subset.A;
                    Description = "Code 128 - Subset A Prioritised & Subset C Suppressed";
                    break;
                case Symbology.Code128BA:
                    SuppressSubsetC = true;
                    StartSubset = Code128Subset.B;
                    Description = "Code 128 - Subset B Prioritised & Subset C Suppressed";
                    break;
                default:
                    SuppressSubsetC = false;
                    StartSubset = Code128Subset.B;
                    Description = "Code 128 - Subset B Prioritised";
                    break;
            }
        }

        internal override ILinearValidator BarcodeValidator { get; } = new Code128Validator();

        internal override void Encode(string barcodeValue)
        {
            ZplEncode = "";
            LoadCode128Symbology();
            LoadSymbologyPattern();
            AnalyseSection(barcodeValue, 0, Code128Subset.Null);
        }

        private void AnalyseSection(string barcodeValue, int startPosition, Code128Subset lastSubset)
        {

            Code128Subset currentSubset = Code128Subset.Null;
            bool isNewSection = false;

            for (int position = startPosition; position <= barcodeValue.Length - 1; position++)
            {
                isNewSection = true;
                int asciiCode = barcodeValue[position];

                switch (asciiCode)
                {
                    case var code when code <= 31:

                        if (currentSubset == Code128Subset.Null | currentSubset == Code128Subset.A)
                        {
                            currentSubset = Code128Subset.A;
                        }
                        else
                        {
                            EncodeSection(lastSubset, currentSubset, barcodeValue.Substring(startPosition, position - startPosition));
                            AnalyseSection(barcodeValue, position, currentSubset);
                            return;
                        }
                        break;

                    case var code when code >= 96:

                        if (currentSubset == Code128Subset.Null | currentSubset == Code128Subset.B)
                        {
                            currentSubset = Code128Subset.B;
                        }
                        else
                        {
                            EncodeSection(lastSubset, currentSubset, barcodeValue.Substring(startPosition, position - startPosition));
                            AnalyseSection(barcodeValue, position, currentSubset);
                            return;
                        }
                        break;

                    case var code when code >= 48 && code <= 57:

                        if (SuppressSubsetC)
                        {
                            break;
                        }

                        int subsetCSequence = GetSubsetCSequenceCount(barcodeValue, position);

                        if (subsetCSequence >= 1)
                        {
                            if (position >= 1)
                            {
                                if (currentSubset == Code128Subset.Null)
                                {
                                    currentSubset = StartSubset;
                                }
                                EncodeSection(lastSubset, currentSubset, barcodeValue.Substring(startPosition, position - startPosition));
                                lastSubset = currentSubset;
                            }

                            currentSubset = Code128Subset.C;
                            EncodeSection(lastSubset, currentSubset, barcodeValue.Substring(position, subsetCSequence));
                            AnalyseSection(barcodeValue, position + subsetCSequence, currentSubset);
                            return;
                        }

                        break;
                }
            }

            if (isNewSection)
            {
                if (currentSubset == Code128Subset.Null)
                {
                    currentSubset = StartSubset;
                }
                EncodeSection(lastSubset, currentSubset, barcodeValue.Substring(startPosition, barcodeValue.Length - startPosition));
            }

            AddCheckDigit();
            AddStopSymbol();
            SetMinXDimension();
            SetMinBarcodeHeight();
        }

        private static int GetSubsetCSequenceCount(string barcodeValue, int startPosition)
        {
            int numberSequenceCount = 0;
            bool startSubsetC = false;
            bool endSubsetC = false;
            int returnCount = 0;

            for (int position = startPosition; position <= barcodeValue.Length - 1; position++)
            {
                if (position == 0)
                {
                    startSubsetC = true;
                }
                if (position == barcodeValue.Length - 1)
                {
                    endSubsetC = true;
                }

                int asciiCode = barcodeValue[position];

                if (asciiCode >= 48 && asciiCode <= 57)
                {
                    numberSequenceCount += 1;
                }
                else
                {
                    break;
                }
            }

            if (startSubsetC && endSubsetC && (numberSequenceCount % 2) == 0)                       // The complete barcode should be encoded as subset C
            {
                returnCount = numberSequenceCount;
            }
            else if (startSubsetC && numberSequenceCount >= 4)                                      // The first subset to be used is subset C
            {
                returnCount = numberSequenceCount - (numberSequenceCount % 2);
            }
            else if (endSubsetC && numberSequenceCount >= 4 && (numberSequenceCount % 2) == 0)      // The last subset to be used is subset C
            {
                returnCount = numberSequenceCount;
            }
            else if (numberSequenceCount >= 6 && (numberSequenceCount % 2) == 0)                    // Subset C should be used in-between A or B subsets
            {
                returnCount = numberSequenceCount;
            }

            return returnCount;
        }

        public void EncodeSection(Code128Subset lastSubset, Code128Subset sectionSubset, string sectionValue)
        {
            string character = GetSubsetAsString(sectionSubset);
            int symbol = _symbologyCharEncode[GetSubsetAsString(lastSubset) + GetSubsetAsString(sectionSubset)];
            LinearPattern symbolPattern = _patternDictionary[symbol];

            AddSymbolToEncode(character, 1, symbol, symbolPattern);
            ZplEncode += GetZplFunction(GetSubsetAsString(lastSubset) + GetSubsetAsString(sectionSubset));

            if (sectionSubset == Code128Subset.C)    //Subset C encoder
            {
                for (int encodePosition = 0; encodePosition <= sectionValue.Length - 1; encodePosition += 2)
                {
                    // Encode the current numeric pair symbol
                    character = sectionValue.Substring(encodePosition, 2);
                    symbol = Convert.ToInt32(character);
                    symbolPattern = _patternDictionary[symbol];
                    AddSymbolToEncode(character, 0, symbol, symbolPattern);
                    ZplEncode += character;
                }
            }
            else
            {
                for (int encodePosition = 0; encodePosition <= sectionValue.Length - 1; encodePosition++)
                {
                    character = sectionValue.Substring(encodePosition, 1);
                    symbol = _symbologyCharEncode[((int)sectionValue[encodePosition]).ToString()];
                    symbolPattern = _patternDictionary[symbol];
                    AddSymbolToEncode(character, 0, symbol, symbolPattern);
                    if (sectionSubset == Code128Subset.A)
                    {
                        string subsetACode = "0" + symbol.ToString();
                        ZplEncode += subsetACode.Substring(subsetACode.Length - 2, 2);
                    }
                    else
                    {
                        ZplEncode += character;
                    }
                    
                }
            }
        }

        private void AddCheckDigit()
        {
            // Return if no symbols have been encoded.
            if (LinearEncoding.Symbols.Count == 0)
            {
                return;
            }

            _checkDigit = _checkDigit % 103;
            LinearPattern symbolPattern = _patternDictionary[_checkDigit];

            AddSymbolToEncode(_checkDigit.ToString(), 1, _checkDigit, symbolPattern);
        }

        private void AddStopSymbol()
        {
            // Return if no symbols have been encoded.
            if (LinearEncoding.Symbols.Count == 0)
            {
                return;
            }

            string character = "S";
            int symbol = _symbologyCharEncode[character];
            LinearPattern symbolPattern = _patternDictionary[symbol];

            AddSymbolToEncode(character, 1, symbol, symbolPattern);
        }

        private void AddSymbolToEncode(string character, int characterType, int symbol, LinearPattern symbolPattern)
        {
            int barcodePosition = LinearEncoding.Symbols.Count;

            LinearEncoding.Add(character, characterType, symbolPattern);

            if (barcodePosition == 0)
            {
                _checkDigit = symbol;
            }
            else
            {
                _checkDigit += (barcodePosition * symbol);
            }
        }

        /// <summary>
        /// Increases the barcode Xdimension to minimum required by symbology, if currently set lower
        /// </summary>
        internal override void SetMinXDimension()
        {
            int xDimensionOriginal = XDimension;
            int minXDimension = (int)Math.Ceiling(Dpi * 0.0075);
            XDimension = Math.Max(XDimension, minXDimension);

            // Set flag to show xdimension was adjusted
            if(xDimensionOriginal != XDimension)
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

        /// <summary>
        /// Get the Code 128 subset as a string
        /// </summary>
        /// <param name="subset"></param>
        /// <returns>Single character subset string</returns>
        private string GetSubsetAsString(Code128Subset subset)
        {
            switch (subset)
            {
                case Code128Subset.A:
                    return "A";
                case Code128Subset.B:
                    return "B";
                case Code128Subset.C:
                    return "C";
                case Code128Subset.Null:
                    return "";
            }

            throw new ArgumentException(subset + " is not a recognised Code128 subset", nameof(subset));
        }

        /// <summary>
        /// Load the Code 128 symbology ASCII mapping into memory.
        /// </summary>
        private void LoadCode128Symbology()
        {
            if (_symbologyCharEncode != null)
            {
                return;
            }

            _symbologyCharEncode = new Dictionary<string, int>
            {
                {"0", 64},
                {"1", 65},
                {"2", 66},
                {"3", 67},
                {"4", 68},
                {"5", 69},
                {"6", 70},
                {"7", 71},
                {"8", 72},
                {"9", 73},
                {"10", 74},
                {"11", 75},
                {"12", 76},
                {"13", 77},
                {"14", 78},
                {"15", 79},
                {"16", 80},
                {"17", 81},
                {"18", 82},
                {"19", 83},
                {"20", 84},
                {"21", 85},
                {"22", 86},
                {"23", 87},
                {"24", 88},
                {"25", 89},
                {"26", 90},
                {"27", 91},
                {"28", 92},
                {"29", 93},
                {"30", 94},
                {"31", 95},
                {"32", 0},
                {"33", 1},
                {"34", 2},
                {"35", 3},
                {"36", 4},
                {"37", 5},
                {"38", 6},
                {"39", 7},
                {"40", 8},
                {"41", 9},
                {"42", 10},
                {"43", 11},
                {"44", 12},
                {"45", 13},
                {"46", 14},
                {"47", 15},
                {"48", 16},
                {"49", 17},
                {"50", 18},
                {"51", 19},
                {"52", 20},
                {"53", 21},
                {"54", 22},
                {"55", 23},
                {"56", 24},
                {"57", 25},
                {"58", 26},
                {"59", 27},
                {"60", 28},
                {"61", 29},
                {"62", 30},
                {"63", 31},
                {"64", 32},
                {"65", 33},
                {"66", 34},
                {"67", 35},
                {"68", 36},
                {"69", 37},
                {"70", 38},
                {"71", 39},
                {"72", 40},
                {"73", 41},
                {"74", 42},
                {"75", 43},
                {"76", 44},
                {"77", 45},
                {"78", 46},
                {"79", 47},
                {"80", 48},
                {"81", 49},
                {"82", 50},
                {"83", 51},
                {"84", 52},
                {"85", 53},
                {"86", 54},
                {"87", 55},
                {"88", 56},
                {"89", 57},
                {"90", 58},
                {"91", 59},
                {"92", 60},
                {"93", 61},
                {"94", 62},
                {"95", 63},
                {"96", 64},
                {"97", 65},
                {"98", 66},
                {"99", 67},
                {"100", 68},
                {"101", 69},
                {"102", 70},
                {"103", 71},
                {"104", 72},
                {"105", 73},
                {"106", 74},
                {"107", 75},
                {"108", 76},
                {"109", 77},
                {"110", 78},
                {"111", 79},
                {"112", 80},
                {"113", 81},
                {"114", 82},
                {"115", 83},
                {"116", 84},
                {"117", 85},
                {"118", 86},
                {"119", 87},
                {"120", 88},
                {"121", 89},
                {"122", 90},
                {"123", 91},
                {"124", 92},
                {"125", 93},
                {"126", 94},
                {"127", 95},
                {"SAB", 98},
                {"SBA", 98},
                {"AC", 99},
                {"BC", 99},
                {"AB", 100},
                {"CB", 100},
                {"BA", 101},
                {"CA", 101},
                {"A", 103},
                {"B", 104},
                {"C", 105},
                {"S", 106}
            };

        }

        /// <summary>
        /// Load the Code 128 character to pattern mapping into memory.
        /// </summary>
        private void LoadSymbologyPattern()
        {
            if (_patternDictionary != null)
            {
                return;
            }

            _patternDictionary = new Dictionary<int, LinearPattern>
            {
                {0, new LinearPattern("212222", ModuleType.Bar)},
                {1, new LinearPattern("222122", ModuleType.Bar)},
                {2, new LinearPattern("222221", ModuleType.Bar)},
                {3, new LinearPattern("121223", ModuleType.Bar)},
                {4, new LinearPattern("121322", ModuleType.Bar)},
                {5, new LinearPattern("131222", ModuleType.Bar)},
                {6, new LinearPattern("122213", ModuleType.Bar)},
                {7, new LinearPattern("122312", ModuleType.Bar)},
                {8, new LinearPattern("132212", ModuleType.Bar)},
                {9, new LinearPattern("221213", ModuleType.Bar)},
                {10, new LinearPattern("221312", ModuleType.Bar)},
                {11, new LinearPattern("231212", ModuleType.Bar)},
                {12, new LinearPattern("112232", ModuleType.Bar)},
                {13, new LinearPattern("122132", ModuleType.Bar)},
                {14, new LinearPattern("122231", ModuleType.Bar)},
                {15, new LinearPattern("113222", ModuleType.Bar)},
                {16, new LinearPattern("123122", ModuleType.Bar)},
                {17, new LinearPattern("123221", ModuleType.Bar)},
                {18, new LinearPattern("223211", ModuleType.Bar)},
                {19, new LinearPattern("221132", ModuleType.Bar)},
                {20, new LinearPattern("221231", ModuleType.Bar)},
                {21, new LinearPattern("213212", ModuleType.Bar)},
                {22, new LinearPattern("223112", ModuleType.Bar)},
                {23, new LinearPattern("312131", ModuleType.Bar)},
                {24, new LinearPattern("311222", ModuleType.Bar)},
                {25, new LinearPattern("321122", ModuleType.Bar)},
                {26, new LinearPattern("321221", ModuleType.Bar)},
                {27, new LinearPattern("312212", ModuleType.Bar)},
                {28, new LinearPattern("322112", ModuleType.Bar)},
                {29, new LinearPattern("322211", ModuleType.Bar)},
                {30, new LinearPattern("212123", ModuleType.Bar)},
                {31, new LinearPattern("212321", ModuleType.Bar)},
                {32, new LinearPattern("232121", ModuleType.Bar)},
                {33, new LinearPattern("111323", ModuleType.Bar)},
                {34, new LinearPattern("131123", ModuleType.Bar)},
                {35, new LinearPattern("131321", ModuleType.Bar)},
                {36, new LinearPattern("112313", ModuleType.Bar)},
                {37, new LinearPattern("132113", ModuleType.Bar)},
                {38, new LinearPattern("132311", ModuleType.Bar)},
                {39, new LinearPattern("211313", ModuleType.Bar)},
                {40, new LinearPattern("231113", ModuleType.Bar)},
                {41, new LinearPattern("231311", ModuleType.Bar)},
                {42, new LinearPattern("112133", ModuleType.Bar)},
                {43, new LinearPattern("112331", ModuleType.Bar)},
                {44, new LinearPattern("132131", ModuleType.Bar)},
                {45, new LinearPattern("113123", ModuleType.Bar)},
                {46, new LinearPattern("113321", ModuleType.Bar)},
                {47, new LinearPattern("133121", ModuleType.Bar)},
                {48, new LinearPattern("313121", ModuleType.Bar)},
                {49, new LinearPattern("211331", ModuleType.Bar)},
                {50, new LinearPattern("231131", ModuleType.Bar)},
                {51, new LinearPattern("213113", ModuleType.Bar)},
                {52, new LinearPattern("213311", ModuleType.Bar)},
                {53, new LinearPattern("213131", ModuleType.Bar)},
                {54, new LinearPattern("311123", ModuleType.Bar)},
                {55, new LinearPattern("311321", ModuleType.Bar)},
                {56, new LinearPattern("331121", ModuleType.Bar)},
                {57, new LinearPattern("312113", ModuleType.Bar)},
                {58, new LinearPattern("312311", ModuleType.Bar)},
                {59, new LinearPattern("332111", ModuleType.Bar)},
                {60, new LinearPattern("314111", ModuleType.Bar)},
                {61, new LinearPattern("221411", ModuleType.Bar)},
                {62, new LinearPattern("431111", ModuleType.Bar)},
                {63, new LinearPattern("111224", ModuleType.Bar)},
                {64, new LinearPattern("111422", ModuleType.Bar)},
                {65, new LinearPattern("121124", ModuleType.Bar)},
                {66, new LinearPattern("121421", ModuleType.Bar)},
                {67, new LinearPattern("141122", ModuleType.Bar)},
                {68, new LinearPattern("141221", ModuleType.Bar)},
                {69, new LinearPattern("112214", ModuleType.Bar)},
                {70, new LinearPattern("112412", ModuleType.Bar)},
                {71, new LinearPattern("122114", ModuleType.Bar)},
                {72, new LinearPattern("122411", ModuleType.Bar)},
                {73, new LinearPattern("142112", ModuleType.Bar)},
                {74, new LinearPattern("142211", ModuleType.Bar)},
                {75, new LinearPattern("241211", ModuleType.Bar)},
                {76, new LinearPattern("221114", ModuleType.Bar)},
                {77, new LinearPattern("413111", ModuleType.Bar)},
                {78, new LinearPattern("241112", ModuleType.Bar)},
                {79, new LinearPattern("134111", ModuleType.Bar)},
                {80, new LinearPattern("111242", ModuleType.Bar)},
                {81, new LinearPattern("121142", ModuleType.Bar)},
                {82, new LinearPattern("121241", ModuleType.Bar)},
                {83, new LinearPattern("114212", ModuleType.Bar)},
                {84, new LinearPattern("124112", ModuleType.Bar)},
                {85, new LinearPattern("124211", ModuleType.Bar)},
                {86, new LinearPattern("411212", ModuleType.Bar)},
                {87, new LinearPattern("421112", ModuleType.Bar)},
                {88, new LinearPattern("421211", ModuleType.Bar)},
                {89, new LinearPattern("212141", ModuleType.Bar)},
                {90, new LinearPattern("214121", ModuleType.Bar)},
                {91, new LinearPattern("412121", ModuleType.Bar)},
                {92, new LinearPattern("111143", ModuleType.Bar)},
                {93, new LinearPattern("111341", ModuleType.Bar)},
                {94, new LinearPattern("131141", ModuleType.Bar)},
                {95, new LinearPattern("114113", ModuleType.Bar)},
                {96, new LinearPattern("114311", ModuleType.Bar)},
                {97, new LinearPattern("411113", ModuleType.Bar)},
                {98, new LinearPattern("411311", ModuleType.Bar)},
                {99, new LinearPattern("113141", ModuleType.Bar)},
                {100, new LinearPattern("114131", ModuleType.Bar)},
                {101, new LinearPattern("311141", ModuleType.Bar)},
                {102, new LinearPattern("411131", ModuleType.Bar)},
                {103, new LinearPattern("211412", ModuleType.Bar)},
                {104, new LinearPattern("211214", ModuleType.Bar)},
                {105, new LinearPattern("211232", ModuleType.Bar)},
                {106, new LinearPattern("2331112", ModuleType.Bar)}
            };
        }

        private static string GetZplFunction(string controlCharacter)
        {
            switch (controlCharacter)
            {
                case "SAB":
                    return ">9";

                case "SBA":
                    return ">:";

                case "AC":
                    return ">5";

                case "BC":
                    return ">5";

                case "AB":
                    return ">6";

                case "CB":
                    return ">6";

                case "BA":
                    return ">7";

                case "CA":
                    return ">7";

                case "A":
                    return ">9";

                case "B":
                    return ">:";

                case "C":
                    return ">;";

                default:
                    return "";

            }
        }
    }
}
