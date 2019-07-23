using System;
using System.Collections.Generic;
using System.Linq;

namespace Barcoded
{
    /// <summary>
    /// Code 39 barcode encoder.
    /// </summary>
    internal class Code39Encoder : LinearEncoder
    {
        private Dictionary<int, List<int>> _symbolDictionary;
        private Dictionary<int, LinearPattern> _patternDictionary;
        private Dictionary<int, int> _checkDigitDictionary;

        private readonly bool _useCheckDigit;

        public Code39Encoder(Symbology symbology) : base(symbology)
        {
            switch (Symbology)
            {
                case Symbology.Code39:
                    // Code39
                    _useCheckDigit = false;
                    Description = "Code 39";
                    break;
                case Symbology.Code39C:
                    // Code39 with check digit
                    _useCheckDigit = true;
                    Description = "Code 39 With Check Digit";
                    break;
                case Symbology.Code39Full:
                    // Code39 full ASCII
                    _useCheckDigit = false;
                    Description = "Code 39 - Full ASCII";
                    break;
                case Symbology.Code39FullC:
                    // Code39 full ASCII with check digit
                    _useCheckDigit = true;
                    Description = "Code 39 - Full ASCII With Check Digit";
                    break;
                default:
                    // Code39
                    _useCheckDigit = false;
                    Description = "Code 39";
                    break;
            }
        }

        internal override ILinearValidator BarcodeValidator { get; } = new Code39Validator();

        internal override void Encode(string barcodeValue)
        {

            switch(Symbology)
            {
                case Symbology.Code39Full:
                    LoadCode39FullAsciiDictionary();
                    break;
                case Symbology.Code39FullC:
                    LoadCode39FullAsciiDictionary();
                    break;
                default:
                    LoadCode39SymbolDictionary();
                    break;
            }

            ZplEncode = "";
            LoadSymbologyPattern();
            LoadCheckDigitDictionary();
            int checkDigitRunning = 0;

            for (int position = 0; position <= barcodeValue.Length - 1; position++)
            {
                // Check if first or last character in barcode and insert start/stop symbol
                if (position == 0)
                {
                    LinearEncoding.Add("*", 1, _patternDictionary[0]);
                    LinearEncoding.Add(" ", 1, _patternDictionary[1]);
                }

                int asciiCode = barcodeValue[position];
                List<int> fullAsciiCharacter = _symbolDictionary[asciiCode];

                for (int character = 0; character <= fullAsciiCharacter.Count - 1; character++)
                {
                    int fullAsciiCode = fullAsciiCharacter[character];
                    char asciiChar = (char)fullAsciiCode;
                    LinearEncoding.Add(asciiChar.ToString(), 0, _patternDictionary[fullAsciiCode]);
                    LinearEncoding.Add(" ", 1, _patternDictionary[1]);
                    checkDigitRunning += _checkDigitDictionary[fullAsciiCode];
                }

                if (position == barcodeValue.Length - 1)
                {
                    if (_useCheckDigit)
                    {
                        int checkDigit = checkDigitRunning % 43;
                        asciiCode = _checkDigitDictionary.FirstOrDefault(x => x.Value == checkDigit).Key;
                        char character = (char)asciiCode;
                        LinearEncoding.Add(character.ToString(), 1, _patternDictionary[asciiCode]);
                        LinearEncoding.Add(" ", 1, _patternDictionary[1]);
                    }
                    LinearEncoding.Add("*", 1, _patternDictionary[0]);
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

        /// <summary>
        /// Load the Full ASCII 
        /// </summary>
        private void LoadCode39FullAsciiDictionary()
        {
            if (_symbolDictionary != null)
            {
                return;
            }

            _symbolDictionary = new Dictionary<int, List<int>>
            {
                {0, new List<int>() {37, 85}},
                {1, new List<int>() {36, 65}},
                {2, new List<int>() {36, 66}},
                {3, new List<int>() {36, 67}},
                {4, new List<int>() {36, 68}},
                {5, new List<int>() {36, 69}},
                {6, new List<int>() {36, 70}},
                {7, new List<int>() {36, 71}},
                {8, new List<int>() {36, 72}},
                {9, new List<int>() {36, 73}},
                {10, new List<int>() {36, 74}},
                {11, new List<int>() {36, 75}},
                {12, new List<int>() {36, 76}},
                {13, new List<int>() {36, 77}},
                {14, new List<int>() {36, 78}},
                {15, new List<int>() {36, 79}},
                {16, new List<int>() {36, 80}},
                {17, new List<int>() {36, 81}},
                {18, new List<int>() {36, 82}},
                {19, new List<int>() {36, 83}},
                {20, new List<int>() {36, 84}},
                {21, new List<int>() {36, 85}},
                {22, new List<int>() {36, 86}},
                {23, new List<int>() {36, 87}},
                {24, new List<int>() {36, 88}},
                {25, new List<int>() {36, 89}},
                {26, new List<int>() {36, 90}},
                {27, new List<int>() {37, 65}},
                {28, new List<int>() {37, 66}},
                {29, new List<int>() {37, 67}},
                {30, new List<int>() {37, 68}},
                {31, new List<int>() {37, 69}},
                {32, new List<int>() {32}},
                {33, new List<int>() {47, 65}},
                {34, new List<int>() {47, 66}},
                {35, new List<int>() {47, 67}},
                {36, new List<int>() {47, 68}},
                {37, new List<int>() {47, 69}},
                {38, new List<int>() {47, 70}},
                {39, new List<int>() {47, 71}},
                {40, new List<int>() {47, 72}},
                {41, new List<int>() {47, 73}},
                {42, new List<int>() {47, 74}},
                {43, new List<int>() {47, 75}},
                {44, new List<int>() {47, 76}},
                {45, new List<int>() {45}},
                {46, new List<int>() {46}},
                {47, new List<int>() {47, 79}},
                {48, new List<int>() {48}},
                {49, new List<int>() {49}},
                {50, new List<int>() {50}},
                {51, new List<int>() {51}},
                {52, new List<int>() {52}},
                {53, new List<int>() {53}},
                {54, new List<int>() {54}},
                {55, new List<int>() {55}},
                {56, new List<int>() {56}},
                {57, new List<int>() {57}},
                {58, new List<int>() {47, 90}},
                {59, new List<int>() {37, 70}},
                {60, new List<int>() {37, 71}},
                {61, new List<int>() {37, 72}},
                {62, new List<int>() {37, 73}},
                {63, new List<int>() {37, 74}},
                {64, new List<int>() {37, 86}},
                {65, new List<int>() {65}},
                {66, new List<int>() {66}},
                {67, new List<int>() {67}},
                {68, new List<int>() {68}},
                {69, new List<int>() {69}},
                {70, new List<int>() {70}},
                {71, new List<int>() {71}},
                {72, new List<int>() {72}},
                {73, new List<int>() {73}},
                {74, new List<int>() {74}},
                {75, new List<int>() {75}},
                {76, new List<int>() {76}},
                {77, new List<int>() {77}},
                {78, new List<int>() {78}},
                {79, new List<int>() {79}},
                {80, new List<int>() {80}},
                {81, new List<int>() {81}},
                {82, new List<int>() {82}},
                {83, new List<int>() {83}},
                {84, new List<int>() {84}},
                {85, new List<int>() {85}},
                {86, new List<int>() {86}},
                {87, new List<int>() {87}},
                {88, new List<int>() {88}},
                {89, new List<int>() {89}},
                {90, new List<int>() {90}},
                {91, new List<int>() {37, 75}},
                {92, new List<int>() {37, 76}},
                {93, new List<int>() {37, 77}},
                {94, new List<int>() {37, 78}},
                {95, new List<int>() {37, 79}},
                {96, new List<int>() {37, 87}},
                {97, new List<int>() {43, 65}},
                {98, new List<int>() {43, 66}},
                {99, new List<int>() {43, 67}},
                {100, new List<int>() {43, 68}},
                {101, new List<int>() {43, 69}},
                {102, new List<int>() {43, 70}},
                {103, new List<int>() {43, 71}},
                {104, new List<int>() {43, 72}},
                {105, new List<int>() {43, 73}},
                {106, new List<int>() {43, 74}},
                {107, new List<int>() {43, 75}},
                {108, new List<int>() {43, 76}},
                {109, new List<int>() {43, 77}},
                {110, new List<int>() {43, 78}},
                {111, new List<int>() {43, 79}},
                {112, new List<int>() {43, 80}},
                {113, new List<int>() {43, 81}},
                {114, new List<int>() {43, 82}},
                {115, new List<int>() {43, 83}},
                {116, new List<int>() {43, 84}},
                {117, new List<int>() {43, 85}},
                {118, new List<int>() {43, 86}},
                {119, new List<int>() {43, 87}},
                {120, new List<int>() {43, 88}},
                {121, new List<int>() {43, 89}},
                {122, new List<int>() {43, 90}},
                {123, new List<int>() {37, 80}},
                {124, new List<int>() {37, 81}},
                {125, new List<int>() {37, 82}},
                {126, new List<int>() {37, 83}},
                {127, new List<int>() {37, 84}}
            };
        }

        private void LoadCode39SymbolDictionary()
        {
            if (_symbolDictionary != null)
            {
                return;
            }

            _symbolDictionary = new Dictionary<int, List<int>>
            {
                {32, new List<int>() {32}},
                {36, new List<int>() {36}},
                {37, new List<int>() {37}},
                {43, new List<int>() {43}},
                {45, new List<int>() {45}},
                {46, new List<int>() {46}},
                {47, new List<int>() {47}},
                {48, new List<int>() {48}},
                {49, new List<int>() {49}},
                {50, new List<int>() {50}},
                {51, new List<int>() {51}},
                {52, new List<int>() {52}},
                {53, new List<int>() {53}},
                {54, new List<int>() {54}},
                {55, new List<int>() {55}},
                {56, new List<int>() {56}},
                {57, new List<int>() {57}},
                {65, new List<int>() {65}},
                {66, new List<int>() {66}},
                {67, new List<int>() {67}},
                {68, new List<int>() {68}},
                {69, new List<int>() {69}},
                {70, new List<int>() {70}},
                {71, new List<int>() {71}},
                {72, new List<int>() {72}},
                {73, new List<int>() {73}},
                {74, new List<int>() {74}},
                {75, new List<int>() {75}},
                {76, new List<int>() {76}},
                {77, new List<int>() {77}},
                {78, new List<int>() {78}},
                {79, new List<int>() {79}},
                {80, new List<int>() {80}},
                {81, new List<int>() {81}},
                {82, new List<int>() {82}},
                {83, new List<int>() {83}},
                {84, new List<int>() {84}},
                {85, new List<int>() {85}},
                {86, new List<int>() {86}},
                {87, new List<int>() {87}},
                {88, new List<int>() {88}},
                {89, new List<int>() {89}},
                {90, new List<int>() {90}}
            };
        }

        private void LoadSymbologyPattern()
        {
            if (_patternDictionary != null)
            {
                return;
            }

            _patternDictionary = new Dictionary<int, LinearPattern>
            {
                {0, new LinearPattern("131131311", ModuleType.Bar)},
                {1, new LinearPattern("1", ModuleType.Space)},
                {32, new LinearPattern("133111311", ModuleType.Bar)},
                {36, new LinearPattern("131313111", ModuleType.Bar)},
                {37, new LinearPattern("111313131", ModuleType.Bar)},
                {43, new LinearPattern("131113131", ModuleType.Bar)},
                {45, new LinearPattern("131111313", ModuleType.Bar)},
                {46, new LinearPattern("331111311", ModuleType.Bar)},
                {47, new LinearPattern("131311131", ModuleType.Bar)},
                {48, new LinearPattern("111331311", ModuleType.Bar)},
                {49, new LinearPattern("311311113", ModuleType.Bar)},
                {50, new LinearPattern("113311113", ModuleType.Bar)},
                {51, new LinearPattern("313311111", ModuleType.Bar)},
                {52, new LinearPattern("111331113", ModuleType.Bar)},
                {53, new LinearPattern("311331111", ModuleType.Bar)},
                {54, new LinearPattern("113331111", ModuleType.Bar)},
                {55, new LinearPattern("111311313", ModuleType.Bar)},
                {56, new LinearPattern("311311311", ModuleType.Bar)},
                {57, new LinearPattern("113311311", ModuleType.Bar)},
                {65, new LinearPattern("311113113", ModuleType.Bar)},
                {66, new LinearPattern("113113113", ModuleType.Bar)},
                {67, new LinearPattern("313113111", ModuleType.Bar)},
                {68, new LinearPattern("111133113", ModuleType.Bar)},
                {69, new LinearPattern("311133111", ModuleType.Bar)},
                {70, new LinearPattern("113133111", ModuleType.Bar)},
                {71, new LinearPattern("111113313", ModuleType.Bar)},
                {72, new LinearPattern("311113311", ModuleType.Bar)},
                {73, new LinearPattern("113113311", ModuleType.Bar)},
                {74, new LinearPattern("111133311", ModuleType.Bar)},
                {75, new LinearPattern("311111133", ModuleType.Bar)},
                {76, new LinearPattern("113111133", ModuleType.Bar)},
                {77, new LinearPattern("313111131", ModuleType.Bar)},
                {78, new LinearPattern("111131133", ModuleType.Bar)},
                {79, new LinearPattern("311131131", ModuleType.Bar)},
                {80, new LinearPattern("113131131", ModuleType.Bar)},
                {81, new LinearPattern("111111333", ModuleType.Bar)},
                {82, new LinearPattern("311111331", ModuleType.Bar)},
                {83, new LinearPattern("113111331", ModuleType.Bar)},
                {84, new LinearPattern("111131331", ModuleType.Bar)},
                {85, new LinearPattern("331111113", ModuleType.Bar)},
                {86, new LinearPattern("133111113", ModuleType.Bar)},
                {87, new LinearPattern("333111111", ModuleType.Bar)},
                {88, new LinearPattern("131131113", ModuleType.Bar)},
                {89, new LinearPattern("331131111", ModuleType.Bar)},
                {90, new LinearPattern("133131111", ModuleType.Bar)}
            };


        }

        private void LoadCheckDigitDictionary()
        {
            if (_checkDigitDictionary != null)
            {
                return;
            }

            _checkDigitDictionary = new Dictionary<int, int>
            {
                {32, 38},
                {36, 39},
                {37, 42},
                {43, 41},
                {45, 36},
                {46, 37},
                {47, 40},
                {48, 0},
                {49, 1},
                {50, 2},
                {51, 3},
                {52, 4},
                {53, 5},
                {54, 6},
                {55, 7},
                {56, 8},
                {57, 9},
                {65, 10},
                {66, 11},
                {67, 12},
                {68, 13},
                {69, 14},
                {70, 15},
                {71, 16},
                {72, 17},
                {73, 18},
                {74, 19},
                {75, 20},
                {76, 21},
                {77, 22},
                {78, 23},
                {79, 24},
                {80, 25},
                {81, 26},
                {82, 27},
                {83, 28},
                {84, 29},
                {85, 30},
                {86, 31},
                {87, 32},
                {88, 33},
                {89, 34},
                {90, 35}
            };
        }
    }
}
