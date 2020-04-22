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

        protected override void Encode(string barcodeValue)
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
                    ZplEncode += asciiChar.ToString();
                    checkDigitRunning += _checkDigitDictionary[fullAsciiCode];
                }

                // Check if last encoding character.
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
                    // Add stop symbol.
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
                {0, new List<int>() {37, 85}},      // NUL   =>  %U
                {1, new List<int>() {36, 65}},      // SOH   =>  $A
                {2, new List<int>() {36, 66}},      // STX   =>  $B
                {3, new List<int>() {36, 67}},      // ETX   =>  $C
                {4, new List<int>() {36, 68}},      // EOT   =>  $D
                {5, new List<int>() {36, 69}},      // ENQ   =>  $E
                {6, new List<int>() {36, 70}},      // ACK   =>  $F
                {7, new List<int>() {36, 71}},      // BEL   =>  $G
                {8, new List<int>() {36, 72}},      // BS    =>  $H
                {9, new List<int>() {36, 73}},      // HT    =>  $I
                {10, new List<int>() {36, 74}},     // LF    =>  $J
                {11, new List<int>() {36, 75}},     // VT    =>  $K
                {12, new List<int>() {36, 76}},     // FF    =>  $L
                {13, new List<int>() {36, 77}},     // CR    =>  $M
                {14, new List<int>() {36, 78}},     // SO    =>  $N
                {15, new List<int>() {36, 79}},     // SI    =>  $O
                {16, new List<int>() {36, 80}},     // DLE   =>  $P
                {17, new List<int>() {36, 81}},     // DC1   =>  $Q
                {18, new List<int>() {36, 82}},     // DC2   =>  $R
                {19, new List<int>() {36, 83}},     // DC3   =>  $S
                {20, new List<int>() {36, 84}},     // DC4   =>  $T
                {21, new List<int>() {36, 85}},     // NAK   =>  $U
                {22, new List<int>() {36, 86}},     // SYN   =>  $V
                {23, new List<int>() {36, 87}},     // ETB   =>  $W
                {24, new List<int>() {36, 88}},     // CAN   =>  $X
                {25, new List<int>() {36, 89}},     // EM    =>  $Y
                {26, new List<int>() {36, 90}},     // SUB   =>  $Z
                {27, new List<int>() {37, 65}},     // ESC   =>  %A
                {28, new List<int>() {37, 66}},     // FS    =>  %B
                {29, new List<int>() {37, 67}},     // GS    =>  %C
                {30, new List<int>() {37, 68}},     // RS    =>  %D
                {31, new List<int>() {37, 69}},     // US    =>  %E
                {32, new List<int>() {32}},         // SPACE =>  SPACE
                {33, new List<int>() {47, 65}},     // !     =>  /A
                {34, new List<int>() {47, 66}},     // "     =>  /B
                {35, new List<int>() {47, 67}},     // #     =>  /C
                {36, new List<int>() {47, 68}},     // $     =>  /D
                {37, new List<int>() {47, 69}},     // %     =>  /E
                {38, new List<int>() {47, 70}},     // &     =>  /F
                {39, new List<int>() {47, 71}},     // '     =>  /G
                {40, new List<int>() {47, 72}},     // (     =>  /H
                {41, new List<int>() {47, 73}},     // )     =>  /I
                {42, new List<int>() {47, 74}},     // *     =>  /J
                {43, new List<int>() {47, 75}},     // +     =>  /K
                {44, new List<int>() {47, 76}},     // ,     =>  /L
                {45, new List<int>() {45}},         // -     =>  -
                {46, new List<int>() {46}},         // .     =>  .
                {47, new List<int>() {47, 79}},     // /     =>  /O
                {48, new List<int>() {48}},         // 0     =>  0
                {49, new List<int>() {49}},         // 1     =>  1
                {50, new List<int>() {50}},         // 2     =>  2
                {51, new List<int>() {51}},         // 3     =>  3
                {52, new List<int>() {52}},         // 4     =>  4
                {53, new List<int>() {53}},         // 5     =>  5
                {54, new List<int>() {54}},         // 6     =>  6
                {55, new List<int>() {55}},         // 7     =>  7
                {56, new List<int>() {56}},         // 8     =>  8
                {57, new List<int>() {57}},         // 9     =>  9
                {58, new List<int>() {47, 90}},     // :     =>  /Z
                {59, new List<int>() {37, 70}},     // ;     =>  %F
                {60, new List<int>() {37, 71}},     // <     =>  %G
                {61, new List<int>() {37, 72}},     // =     =>  %H
                {62, new List<int>() {37, 73}},     // >     =>  %I
                {63, new List<int>() {37, 74}},     // ?     =>  %J
                {64, new List<int>() {37, 86}},     // @     =>  %V
                {65, new List<int>() {65}},         // A     =>  A
                {66, new List<int>() {66}},         // B     =>  B
                {67, new List<int>() {67}},         // C     =>  C
                {68, new List<int>() {68}},         // D     =>  D
                {69, new List<int>() {69}},         // E     =>  E
                {70, new List<int>() {70}},         // F     =>  F
                {71, new List<int>() {71}},         // G     =>  G
                {72, new List<int>() {72}},         // H     =>  H
                {73, new List<int>() {73}},         // I     =>  I
                {74, new List<int>() {74}},         // J     =>  J
                {75, new List<int>() {75}},         // K     =>  K
                {76, new List<int>() {76}},         // L     =>  L
                {77, new List<int>() {77}},         // M     =>  M
                {78, new List<int>() {78}},         // N     =>  N
                {79, new List<int>() {79}},         // O     =>  O
                {80, new List<int>() {80}},         // P     =>  P
                {81, new List<int>() {81}},         // Q     =>  Q
                {82, new List<int>() {82}},         // R     =>  R
                {83, new List<int>() {83}},         // S     =>  S
                {84, new List<int>() {84}},         // T     =>  T
                {85, new List<int>() {85}},         // U     =>  U
                {86, new List<int>() {86}},         // V     =>  V
                {87, new List<int>() {87}},         // W     =>  W
                {88, new List<int>() {88}},         // X     =>  X
                {89, new List<int>() {89}},         // Y     =>  Y
                {90, new List<int>() {90}},         // Z     =>  Z
                {91, new List<int>() {37, 75}},     // [     =>  %K
                {92, new List<int>() {37, 76}},     // \     =>  %L
                {93, new List<int>() {37, 77}},     // ]     =>  %M
                {94, new List<int>() {37, 78}},     // ^     =>  %N
                {95, new List<int>() {37, 79}},     // _     =>  %O
                {96, new List<int>() {37, 87}},     // `     =>  %W
                {97, new List<int>() {43, 65}},     // a     =>  +A
                {98, new List<int>() {43, 66}},     // b     =>  +B
                {99, new List<int>() {43, 67}},     // c     =>  +C
                {100, new List<int>() {43, 68}},    // d     =>  +D
                {101, new List<int>() {43, 69}},    // e     =>  +E
                {102, new List<int>() {43, 70}},    // f     =>  +F
                {103, new List<int>() {43, 71}},    // g     =>  +G
                {104, new List<int>() {43, 72}},    // h     =>  +H
                {105, new List<int>() {43, 73}},    // i     =>  +I
                {106, new List<int>() {43, 74}},    // j     =>  +J
                {107, new List<int>() {43, 75}},    // k     =>  +K
                {108, new List<int>() {43, 76}},    // l     =>  +L
                {109, new List<int>() {43, 77}},    // m     =>  +M
                {110, new List<int>() {43, 78}},    // n     =>  +N
                {111, new List<int>() {43, 79}},    // o     =>  +O
                {112, new List<int>() {43, 80}},    // p     =>  +P
                {113, new List<int>() {43, 81}},    // q     =>  +Q
                {114, new List<int>() {43, 82}},    // r     =>  +R
                {115, new List<int>() {43, 83}},    // s     =>  +S
                {116, new List<int>() {43, 84}},    // t     =>  +T
                {117, new List<int>() {43, 85}},    // u     =>  +U
                {118, new List<int>() {43, 86}},    // v     =>  +V
                {119, new List<int>() {43, 87}},    // w     =>  +W
                {120, new List<int>() {43, 88}},    // x     =>  +X
                {121, new List<int>() {43, 89}},    // y     =>  +Y
                {122, new List<int>() {43, 90}},    // z     =>  +Z
                {123, new List<int>() {37, 80}},    // {     =>  %P
                {124, new List<int>() {37, 81}},    // |     =>  %Q
                {125, new List<int>() {37, 82}},    // }     =>  %R
                {126, new List<int>() {37, 83}},    // ~     =>  %S
                {127, new List<int>() {37, 84}}     // DEL   =>  %T
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
                {32, new List<int>() {32}},      // SPACE
                {36, new List<int>() {36}},      // $
                {37, new List<int>() {37}},      // %
                {43, new List<int>() {43}},      // +
                {45, new List<int>() {45}},      // -
                {46, new List<int>() {46}},      // .
                {47, new List<int>() {47}},      // /
                {48, new List<int>() {48}},      // 0
                {49, new List<int>() {49}},      // 1
                {50, new List<int>() {50}},      // 2
                {51, new List<int>() {51}},      // 3
                {52, new List<int>() {52}},      // 4
                {53, new List<int>() {53}},      // 5
                {54, new List<int>() {54}},      // 6
                {55, new List<int>() {55}},      // 7
                {56, new List<int>() {56}},      // 8
                {57, new List<int>() {57}},      // 9
                {65, new List<int>() {65}},      // A
                {66, new List<int>() {66}},      // B
                {67, new List<int>() {67}},      // C
                {68, new List<int>() {68}},      // D
                {69, new List<int>() {69}},      // E
                {70, new List<int>() {70}},      // F
                {71, new List<int>() {71}},      // G
                {72, new List<int>() {72}},      // H
                {73, new List<int>() {73}},      // I
                {74, new List<int>() {74}},      // J
                {75, new List<int>() {75}},      // K
                {76, new List<int>() {76}},      // L
                {77, new List<int>() {77}},      // M
                {78, new List<int>() {78}},      // N
                {79, new List<int>() {79}},      // O
                {80, new List<int>() {80}},      // P
                {81, new List<int>() {81}},      // Q
                {82, new List<int>() {82}},      // R
                {83, new List<int>() {83}},      // S
                {84, new List<int>() {84}},      // T
                {85, new List<int>() {85}},      // U
                {86, new List<int>() {86}},      // V
                {87, new List<int>() {87}},      // W
                {88, new List<int>() {88}},      // X
                {89, new List<int>() {89}},      // Y
                {90, new List<int>() {90}}       // Z
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
                {0, new LinearPattern("NWNNWNWNN", ModuleType.Bar, WideBarRatio)},       // START/STOP
                {1, new LinearPattern("N", ModuleType.Space, WideBarRatio)},             // SPACER
                {32, new LinearPattern("NWWNNNWNN", ModuleType.Bar, WideBarRatio)},      // SPACE
                {36, new LinearPattern("NWNWNWNNN", ModuleType.Bar, WideBarRatio)},      // $
                {37, new LinearPattern("NNNWNWNWN", ModuleType.Bar, WideBarRatio)},      // %
                {43, new LinearPattern("NWNNNWNWN", ModuleType.Bar, WideBarRatio)},      // +
                {45, new LinearPattern("NWNNNNWNW", ModuleType.Bar, WideBarRatio)},      // -
                {46, new LinearPattern("WWNNNNWNN", ModuleType.Bar, WideBarRatio)},      // .
                {47, new LinearPattern("NWNWNNNWN", ModuleType.Bar, WideBarRatio)},      // /
                {48, new LinearPattern("NNNWWNWNN", ModuleType.Bar, WideBarRatio)},      // 0
                {49, new LinearPattern("WNNWNNNNW", ModuleType.Bar, WideBarRatio)},      // 1
                {50, new LinearPattern("NNWWNNNNW", ModuleType.Bar, WideBarRatio)},      // 2
                {51, new LinearPattern("WNWWNNNNN", ModuleType.Bar, WideBarRatio)},      // 3
                {52, new LinearPattern("NNNWWNNNW", ModuleType.Bar, WideBarRatio)},      // 4
                {53, new LinearPattern("WNNWWNNNN", ModuleType.Bar, WideBarRatio)},      // 5
                {54, new LinearPattern("NNWWWNNNN", ModuleType.Bar, WideBarRatio)},      // 6
                {55, new LinearPattern("NNNWNNWNW", ModuleType.Bar, WideBarRatio)},      // 7
                {56, new LinearPattern("WNNWNNWNN", ModuleType.Bar, WideBarRatio)},      // 8
                {57, new LinearPattern("NNWWNNWNN", ModuleType.Bar, WideBarRatio)},      // 9
                {65, new LinearPattern("WNNNNWNNW", ModuleType.Bar, WideBarRatio)},      // A
                {66, new LinearPattern("NNWNNWNNW", ModuleType.Bar, WideBarRatio)},      // B
                {67, new LinearPattern("WNWNNWNNN", ModuleType.Bar, WideBarRatio)},      // C
                {68, new LinearPattern("NNNNWWNNW", ModuleType.Bar, WideBarRatio)},      // D
                {69, new LinearPattern("WNNNWWNNN", ModuleType.Bar, WideBarRatio)},      // E
                {70, new LinearPattern("NNWNWWNNN", ModuleType.Bar, WideBarRatio)},      // F
                {71, new LinearPattern("NNNNNWWNW", ModuleType.Bar, WideBarRatio)},      // G
                {72, new LinearPattern("WNNNNWWNN", ModuleType.Bar, WideBarRatio)},      // H
                {73, new LinearPattern("NNWNNWWNN", ModuleType.Bar, WideBarRatio)},      // I
                {74, new LinearPattern("NNNNWWWNN", ModuleType.Bar, WideBarRatio)},      // J
                {75, new LinearPattern("WNNNNNNWW", ModuleType.Bar, WideBarRatio)},      // K
                {76, new LinearPattern("NNWNNNNWW", ModuleType.Bar, WideBarRatio)},      // L
                {77, new LinearPattern("WNWNNNNWN", ModuleType.Bar, WideBarRatio)},      // M
                {78, new LinearPattern("NNNNWNNWW", ModuleType.Bar, WideBarRatio)},      // N
                {79, new LinearPattern("WNNNWNNWN", ModuleType.Bar, WideBarRatio)},      // O
                {80, new LinearPattern("NNWNWNNWN", ModuleType.Bar, WideBarRatio)},      // P
                {81, new LinearPattern("NNNNNNWWW", ModuleType.Bar, WideBarRatio)},      // Q
                {82, new LinearPattern("WNNNNNWWN", ModuleType.Bar, WideBarRatio)},      // R
                {83, new LinearPattern("NNWNNNWWN", ModuleType.Bar, WideBarRatio)},      // S
                {84, new LinearPattern("NNNNWNWWN", ModuleType.Bar, WideBarRatio)},      // T
                {85, new LinearPattern("WWNNNNNNW", ModuleType.Bar, WideBarRatio)},      // U
                {86, new LinearPattern("NWWNNNNNW", ModuleType.Bar, WideBarRatio)},      // V
                {87, new LinearPattern("WWWNNNNNN", ModuleType.Bar, WideBarRatio)},      // W
                {88, new LinearPattern("NWNNWNNNW", ModuleType.Bar, WideBarRatio)},      // X
                {89, new LinearPattern("WWNNWNNNN", ModuleType.Bar, WideBarRatio)},      // Y
                {90, new LinearPattern("NWWNWNNNN", ModuleType.Bar, WideBarRatio)}       // Z
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
                {32, 38},      // SPACE
                {36, 39},      // $
                {37, 42},      // %
                {43, 41},      // +
                {45, 36},      // -
                {46, 37},      // .
                {47, 40},      // /
                {48, 0},       // 0
                {49, 1},       // 1
                {50, 2},       // 2
                {51, 3},       // 3
                {52, 4},       // 4
                {53, 5},       // 5
                {54, 6},       // 6
                {55, 7},       // 7
                {56, 8},       // 8
                {57, 9},       // 9
                {65, 10},      // A
                {66, 11},      // B
                {67, 12},      // C
                {68, 13},      // D
                {69, 14},      // E
                {70, 15},      // F
                {71, 16},      // G
                {72, 17},      // H
                {73, 18},      // I
                {74, 19},      // J
                {75, 20},      // K
                {76, 21},      // L
                {77, 22},      // M
                {78, 23},      // N
                {79, 24},      // O
                {80, 25},      // P
                {81, 26},      // Q
                {82, 27},      // R
                {83, 28},      // S
                {84, 29},      // T
                {85, 30},      // U
                {86, 31},      // V
                {87, 32},      // W
                {88, 33},      // X
                {89, 34},      // Y
                {90, 35}       // Z
            };
        }
    }
}
