using System;
using System.Collections.Generic;

namespace Barcoded
{
    /// <summary>
    /// Code 128 & GS1-128 barcode encoder.
    /// </summary>
    internal class Code128Encoder : LinearEncoder
    {
        private Dictionary<string, int> _symbologyCharEncode;
        private Dictionary<int, LinearPattern> _patternDictionary;
        private int _checkDigit;
        private readonly bool _suppressSubsetC;
        private readonly bool _isGs1;

        private Code128Subset StartSubset { get; }

        public Code128Encoder(Symbology symbology) : base(symbology)
        {
            switch (Symbology)
            {
                case Symbology.Code128ABC:
                    _suppressSubsetC = false;
                    StartSubset = Code128Subset.A;
                    Description = "Code 128 - Subset A Prioritised & Auto Subset C Selected";
                    break;
                case Symbology.Code128BAC:
                    _suppressSubsetC = false;
                    StartSubset = Code128Subset.B;
                    Description = "Code 128 - Subset B Prioritised & Auto Subset C Selected";
                    break;
                case Symbology.Code128AB:
                    _suppressSubsetC = true;
                    StartSubset = Code128Subset.A;
                    Description = "Code 128 - Subset A Prioritised & Subset C Suppressed";
                    break;
                case Symbology.Code128BA:
                    _suppressSubsetC = true;
                    StartSubset = Code128Subset.B;
                    Description = "Code 128 - Subset B Prioritised & Subset C Suppressed";
                    break;
                case Symbology.GS1128:
                    _suppressSubsetC = false;
                    _isGs1 = true;
                    StartSubset = Code128Subset.B;
                    Description = "GS1-128 - Subset B Prioritised & Auto Subset C Selected";
                    break;
                default:
                    _suppressSubsetC = false;
                    StartSubset = Code128Subset.B;
                    Description = "Code 128 - Subset B Prioritised & Auto Subset C Selected";
                    break;
            }
        }

        internal override ILinearValidator BarcodeValidator { get; } = new Code128Validator();

        internal override void Encode(string barcodeValue)
        {
            EncodedValue = barcodeValue;
            ZplEncode = "";
            LoadCode128Symbology();
            LoadSymbologyPattern();
            AnalyseSection(barcodeValue, 0, Code128Subset.Null);
        }

        /// <summary>
        /// Analyse the supplied section and determine the appropriate encoding. 
        /// </summary>
        /// <param name="barcodeValue"></param>
        /// <param name="startPosition"></param>
        /// <param name="lastSubset"></param>
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
                    case var code when code <= 31: // Control characters that are only supported by subset A.

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

                    case var code when code >= 96: // Lower case characters that are only supported by subset C

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

                    case var code when code >= 48 && code <= 57: // Numeric characters, that may benefit from using subset C.

                        if (_suppressSubsetC)
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

        /// <summary>
        /// Gets the count of numeric pairs to be used in Subset-C encoding
        /// </summary>
        /// <param name="barcodeValue"></param>
        /// <param name="startPosition"></param>
        /// <returns>Number of concurrent digits.</returns>
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

        /// <summary>
        /// Adds the supplied section and control characters to the encode.
        /// </summary>
        /// <param name="lastSubset"></param>
        /// <param name="sectionSubset"></param>
        /// <param name="sectionValue"></param>
        public void EncodeSection(Code128Subset lastSubset, Code128Subset sectionSubset, string sectionValue)
        {
            // Add the subset select or change control symbol to the encode
            string character = GetSubsetAsString(sectionSubset);
            int symbol = _symbologyCharEncode[GetSubsetAsString(lastSubset) + GetSubsetAsString(sectionSubset)];
            LinearPattern symbolPattern = _patternDictionary[symbol];
            AddSymbolToEncode(character, 1, symbol, symbolPattern);
            ZplEncode += GetZplFunction(GetSubsetAsString(lastSubset) + GetSubsetAsString(sectionSubset));
            
            // Add FNC1 symbol if GS1-128 Barcode
            if (lastSubset == Code128Subset.Null && _isGs1)
            {
                symbol = _symbologyCharEncode["FNC1"];
                symbolPattern = _patternDictionary[symbol];
                AddSymbolToEncode("F1", 1, symbol, symbolPattern);
                ZplEncode += GetZplFunction("FNC1");
            }

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

        /// <summary>
        /// Adds the check digit to the encode.
        /// </summary>
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

        /// <summary>
        /// Adds the stop symbol to the encode.
        /// </summary>
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

        /// <summary>
        /// Adds the given symbol to the encode.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="characterType"></param>
        /// <param name="symbol"></param>
        /// <param name="symbolPattern"></param>
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
                {"0", 64},      // NUL
                {"1", 65},      // SOH
                {"2", 66},      // STX
                {"3", 67},      // ETX
                {"4", 68},      // EOT
                {"5", 69},      // ENQ
                {"6", 70},      // ACK
                {"7", 71},      // BEL
                {"8", 72},      // BS
                {"9", 73},      // TAB
                {"10", 74},     // LF
                {"11", 75},     // VT
                {"12", 76},     // FF
                {"13", 77},     // CR
                {"14", 78},     // SOH
                {"15", 79},     // SI
                {"16", 80},     // DLE
                {"17", 81},     // DC1
                {"18", 82},     // DC2
                {"19", 83},     // DC3
                {"20", 84},     // DC4
                {"21", 85},     // NAK
                {"22", 86},     // SYN
                {"23", 87},     // ETB
                {"24", 88},     // CAN
                {"25", 89},     // EM
                {"26", 90},     // SUB
                {"27", 91},     // ESC
                {"28", 92},     // FS
                {"29", 93},     // GS
                {"30", 94},     // RS
                {"31", 95},     // US
                {"32", 0},      // SPACE
                {"33", 1},      // !
                {"34", 2},      // "
                {"35", 3},      // #
                {"36", 4},      // $
                {"37", 5},      // %
                {"38", 6},      // &
                {"39", 7},      // '
                {"40", 8},      // (
                {"41", 9},      // )
                {"42", 10},     // *
                {"43", 11},     // +
                {"44", 12},     // ,
                {"45", 13},     // -
                {"46", 14},     // .
                {"47", 15},     // /
                {"48", 16},     // 0
                {"49", 17},     // 1
                {"50", 18},     // 2
                {"51", 19},     // 3
                {"52", 20},     // 4
                {"53", 21},     // 5
                {"54", 22},     // 6
                {"55", 23},     // 7
                {"56", 24},     // 8
                {"57", 25},     // 9
                {"58", 26},     // :
                {"59", 27},     // ;
                {"60", 28},     // <
                {"61", 29},     // =
                {"62", 30},     // >
                {"63", 31},     // ?
                {"64", 32},     // @
                {"65", 33},     // A
                {"66", 34},     // B
                {"67", 35},     // C
                {"68", 36},     // D
                {"69", 37},     // E
                {"70", 38},     // F
                {"71", 39},     // G
                {"72", 40},     // H
                {"73", 41},     // I
                {"74", 42},     // J
                {"75", 43},     // K
                {"76", 44},     // L
                {"77", 45},     // M
                {"78", 46},     // N
                {"79", 47},     // O
                {"80", 48},     // P
                {"81", 49},     // Q
                {"82", 50},     // R
                {"83", 51},     // S
                {"84", 52},     // T
                {"85", 53},     // U
                {"86", 54},     // V
                {"87", 55},     // W
                {"88", 56},     // X
                {"89", 57},     // Y
                {"90", 58},     // Z
                {"91", 59},     // [
                {"92", 60},     // \
                {"93", 61},     // ]
                {"94", 62},     // ^
                {"95", 63},     // _
                {"96", 64},     // `
                {"97", 65},     // a
                {"98", 66},     // b
                {"99", 67},     // c
                {"100", 68},    // d
                {"101", 69},    // e
                {"102", 70},    // f
                {"103", 71},    // g
                {"104", 72},    // h
                {"105", 73},    // i
                {"106", 74},    // j
                {"107", 75},    // k
                {"108", 76},    // l
                {"109", 77},    // m
                {"110", 78},    // n
                {"111", 79},    // o
                {"112", 80},    // p
                {"113", 81},    // q
                {"114", 82},    // r
                {"115", 83},    // s
                {"116", 84},    // t
                {"117", 85},    // u
                {"118", 86},    // v
                {"119", 87},    // w
                {"120", 88},    // x
                {"121", 89},    // y
                {"122", 90},    // z
                {"123", 91},    // {
                {"124", 92},    // |
                {"125", 93},    // }
                {"126", 94},    // ~
                {"127", 95},    // DEL
                {"SAB", 98},    // SHIFT B
                {"SBA", 98},    // SHIFT A
                {"AC", 99},     // SHIFT C
                {"BC", 99},     // SHIFT C
                {"AB", 100},    // SHIFT B
                {"CB", 100},    // SHIFT B
                {"BA", 101},    // SHIFT A
                {"CA", 101},    // SHIFT A
                {"FNC1", 102},  // FNC1
                {"A", 103},     // START A
                {"B", 104},     // START B
                {"C", 105},     // START C
                {"S", 106}      // STOP
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
            {                                                                    //   A     |    B    | C
                {0, new LinearPattern("212222", ModuleType.Bar)},       // SPACE   | SPACE   | 00
                {1, new LinearPattern("222122", ModuleType.Bar)},       // !       | !       | 01
                {2, new LinearPattern("222221", ModuleType.Bar)},       // "       | "       | 02
                {3, new LinearPattern("121223", ModuleType.Bar)},       // #       | #       | 03
                {4, new LinearPattern("121322", ModuleType.Bar)},       // $       | $       | 04
                {5, new LinearPattern("131222", ModuleType.Bar)},       // %       | %       | 05
                {6, new LinearPattern("122213", ModuleType.Bar)},       // &       | &       | 06
                {7, new LinearPattern("122312", ModuleType.Bar)},       // '       | '       | 07
                {8, new LinearPattern("132212", ModuleType.Bar)},       // (       | (       | 08
                {9, new LinearPattern("221213", ModuleType.Bar)},       // )       | )       | 09
                {10, new LinearPattern("221312", ModuleType.Bar)},      // *       | *       | 10
                {11, new LinearPattern("231212", ModuleType.Bar)},      // +       | +       | 11
                {12, new LinearPattern("112232", ModuleType.Bar)},      // ,       | ,       | 12
                {13, new LinearPattern("122132", ModuleType.Bar)},      // -       | -       | 13
                {14, new LinearPattern("122231", ModuleType.Bar)},      // .       | .       | 14
                {15, new LinearPattern("113222", ModuleType.Bar)},      // /       | /       | 15
                {16, new LinearPattern("123122", ModuleType.Bar)},      // 0       | 0       | 16
                {17, new LinearPattern("123221", ModuleType.Bar)},      // 1       | 1       | 17
                {18, new LinearPattern("223211", ModuleType.Bar)},      // 2       | 2       | 18
                {19, new LinearPattern("221132", ModuleType.Bar)},      // 3       | 3       | 19
                {20, new LinearPattern("221231", ModuleType.Bar)},      // 4       | 4       | 20
                {21, new LinearPattern("213212", ModuleType.Bar)},      // 5       | 5       | 21
                {22, new LinearPattern("223112", ModuleType.Bar)},      // 6       | 6       | 22
                {23, new LinearPattern("312131", ModuleType.Bar)},      // 7       | 7       | 23
                {24, new LinearPattern("311222", ModuleType.Bar)},      // 8       | 8       | 24
                {25, new LinearPattern("321122", ModuleType.Bar)},      // 9       | 9       | 25
                {26, new LinearPattern("321221", ModuleType.Bar)},      // :       | :       | 26
                {27, new LinearPattern("312212", ModuleType.Bar)},      // ;       | ;       | 27
                {28, new LinearPattern("322112", ModuleType.Bar)},      // <       | <       | 28
                {29, new LinearPattern("322211", ModuleType.Bar)},      // =       | =       | 29
                {30, new LinearPattern("212123", ModuleType.Bar)},      // >       | >       | 30
                {31, new LinearPattern("212321", ModuleType.Bar)},      // ?       | ?       | 31
                {32, new LinearPattern("232121", ModuleType.Bar)},      // @       | @       | 32
                {33, new LinearPattern("111323", ModuleType.Bar)},      // A       | A       | 33
                {34, new LinearPattern("131123", ModuleType.Bar)},      // B       | B       | 34
                {35, new LinearPattern("131321", ModuleType.Bar)},      // C       | C       | 35
                {36, new LinearPattern("112313", ModuleType.Bar)},      // D       | D       | 36
                {37, new LinearPattern("132113", ModuleType.Bar)},      // E       | E       | 37
                {38, new LinearPattern("132311", ModuleType.Bar)},      // F       | F       | 38
                {39, new LinearPattern("211313", ModuleType.Bar)},      // G       | G       | 39
                {40, new LinearPattern("231113", ModuleType.Bar)},      // H       | H       | 40
                {41, new LinearPattern("231311", ModuleType.Bar)},      // I       | I       | 41
                {42, new LinearPattern("112133", ModuleType.Bar)},      // J       | J       | 42
                {43, new LinearPattern("112331", ModuleType.Bar)},      // K       | K       | 43
                {44, new LinearPattern("132131", ModuleType.Bar)},      // L       | L       | 44
                {45, new LinearPattern("113123", ModuleType.Bar)},      // M       | M       | 45
                {46, new LinearPattern("113321", ModuleType.Bar)},      // N       | N       | 46
                {47, new LinearPattern("133121", ModuleType.Bar)},      // O       | O       | 47
                {48, new LinearPattern("313121", ModuleType.Bar)},      // P       | P       | 48
                {49, new LinearPattern("211331", ModuleType.Bar)},      // Q       | Q       | 49
                {50, new LinearPattern("231131", ModuleType.Bar)},      // R       | R       | 50
                {51, new LinearPattern("213113", ModuleType.Bar)},      // S       | S       | 51
                {52, new LinearPattern("213311", ModuleType.Bar)},      // T       | T       | 52
                {53, new LinearPattern("213131", ModuleType.Bar)},      // U       | U       | 53
                {54, new LinearPattern("311123", ModuleType.Bar)},      // V       | V       | 54
                {55, new LinearPattern("311321", ModuleType.Bar)},      // W       | W       | 55
                {56, new LinearPattern("331121", ModuleType.Bar)},      // X       | X       | 56
                {57, new LinearPattern("312113", ModuleType.Bar)},      // Y       | Y       | 57
                {58, new LinearPattern("312311", ModuleType.Bar)},      // Z       | Z       | 58
                {59, new LinearPattern("332111", ModuleType.Bar)},      // [       | [       | 59
                {60, new LinearPattern("314111", ModuleType.Bar)},      // \       | \       | 60
                {61, new LinearPattern("221411", ModuleType.Bar)},      // ]       | ]       | 61
                {62, new LinearPattern("431111", ModuleType.Bar)},      // ^       | ^       | 62
                {63, new LinearPattern("111224", ModuleType.Bar)},      // _       | _       | 63
                {64, new LinearPattern("111422", ModuleType.Bar)},      // NUL     | `       | 64
                {65, new LinearPattern("121124", ModuleType.Bar)},      // SOH     | a       | 65
                {66, new LinearPattern("121421", ModuleType.Bar)},      // STX     | b       | 66
                {67, new LinearPattern("141122", ModuleType.Bar)},      // ETX     | c       | 67
                {68, new LinearPattern("141221", ModuleType.Bar)},      // EOT     | d       | 68
                {69, new LinearPattern("112214", ModuleType.Bar)},      // ENQ     | e       | 69
                {70, new LinearPattern("112412", ModuleType.Bar)},      // ACK     | f       | 70
                {71, new LinearPattern("122114", ModuleType.Bar)},      // BEL     | g       | 71
                {72, new LinearPattern("122411", ModuleType.Bar)},      // BS      | h       | 72
                {73, new LinearPattern("142112", ModuleType.Bar)},      // HT      | i       | 73
                {74, new LinearPattern("142211", ModuleType.Bar)},      // LF      | j       | 74
                {75, new LinearPattern("241211", ModuleType.Bar)},      // VT      | k       | 75
                {76, new LinearPattern("221114", ModuleType.Bar)},      // FF      | l       | 76
                {77, new LinearPattern("413111", ModuleType.Bar)},      // CR      | m       | 77
                {78, new LinearPattern("241112", ModuleType.Bar)},      // SO      | n       | 78
                {79, new LinearPattern("134111", ModuleType.Bar)},      // SI      | o       | 79
                {80, new LinearPattern("111242", ModuleType.Bar)},      // DLE     | p       | 80
                {81, new LinearPattern("121142", ModuleType.Bar)},      // DC1     | q       | 81
                {82, new LinearPattern("121241", ModuleType.Bar)},      // DC2     | r       | 82
                {83, new LinearPattern("114212", ModuleType.Bar)},      // DC3     | s       | 83
                {84, new LinearPattern("124112", ModuleType.Bar)},      // DC4     | t       | 84
                {85, new LinearPattern("124211", ModuleType.Bar)},      // NAK     | u       | 85
                {86, new LinearPattern("411212", ModuleType.Bar)},      // SYN     | v       | 86
                {87, new LinearPattern("421112", ModuleType.Bar)},      // ETB     | w       | 87
                {88, new LinearPattern("421211", ModuleType.Bar)},      // CAN     | x       | 88
                {89, new LinearPattern("212141", ModuleType.Bar)},      // EM      | y       | 89
                {90, new LinearPattern("214121", ModuleType.Bar)},      // SUB     | z       | 90
                {91, new LinearPattern("412121", ModuleType.Bar)},      // ESC     | {       | 91
                {92, new LinearPattern("111143", ModuleType.Bar)},      // FS      | |       | 92
                {93, new LinearPattern("111341", ModuleType.Bar)},      // GS      | }       | 93
                {94, new LinearPattern("131141", ModuleType.Bar)},      // RS      | ~       | 94
                {95, new LinearPattern("114113", ModuleType.Bar)},      // US      | DEL     | 95
                {96, new LinearPattern("114311", ModuleType.Bar)},      // FNC 3   | FNC 3   | 96
                {97, new LinearPattern("411113", ModuleType.Bar)},      // FNC 2   | FNC 2   | 97
                {98, new LinearPattern("411311", ModuleType.Bar)},      // SHIFT B | SHIFT A | 98
                {99, new LinearPattern("113141", ModuleType.Bar)},      // CODE C  | CODE C  | 99
                {100, new LinearPattern("114131", ModuleType.Bar)},     // CODE B  | FNC 4   | CODE B
                {101, new LinearPattern("311141", ModuleType.Bar)},     // FNC 4   | CODE A  | CODE A
                {102, new LinearPattern("411131", ModuleType.Bar)},     // FNC 1   | FNC 1   | FNC 1
                {103, new LinearPattern("211412", ModuleType.Bar)},     // START CODE A
                {104, new LinearPattern("211214", ModuleType.Bar)},     // START CODE B
                {105, new LinearPattern("211232", ModuleType.Bar)},     // START CODE C
                {106, new LinearPattern("2331112", ModuleType.Bar)}     // STOP
            };
        }

        /// <summary>
        /// Gets the ZPL string for the given control character.
        /// </summary>
        /// <param name="controlCharacter"></param>
        /// <returns></returns>
        private static string GetZplFunction(string controlCharacter)
        {
            switch (controlCharacter)
            {
                case "SAB":           // START A
                    return ">9";

                case "SBA":           // START B
                    return ">:";

                case "AC":           // SHIFT A to C
                    return ">5";

                case "BC":           // SHIFT B to C
                    return ">5";

                case "AB":           // SHIFT A to B
                    return ">6";

                case "CB":           // SHIFT C to B
                    return ">6";

                case "BA":           // SHIFT B to A
                    return ">7";

                case "CA":           // SHIFT C to A
                    return ">7";

                case "A":           // START A
                    return ">9";

                case "B":           // START B
                    return ">:";

                case "C":           // START C
                    return ">;";

                case "FNC1":        // FNC1 - GS1
                    return ">8";

                default:
                    return "";

            }
        }
    }
}
