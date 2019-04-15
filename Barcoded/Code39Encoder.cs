using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barcoded
{
    class Code39Encoder : LinearEncoder
    {
        private Dictionary<int, List<int>> _symbolDictionary;
        private Dictionary<int, LinearPattern> _patternDictionary;
        private Dictionary<int, int> _checkDigitDictionary;

        private bool _useCheckDigit = false;

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

        internal override void Setup()
        {

        }

        internal override ILinearValidator BarcodeValidator { get; } = new Code39Validator();

        internal override void Encode(string barcodeValue)
        {

            switch(Symbology)
            {
                case Symbology.Code39Full:
                    LoadCode39FullASCIIDictionary();
                    break;
                case Symbology.Code39FullC:
                    LoadCode39FullASCIIDictionary();
                    break;
                default:
                    LoadCode39SymbolDictionary();
                    break;
            }

            LoadSymbologyPattern();
            LoadCheckDigitDictionary();
            int ASCICode;
            int fullASCIICode;
            char ASCIIChar;
            List<int> fullASCIICharacter;
            int checkDigitRunning = 0;

            for (int position = 0; position <= barcodeValue.Length - 1; position++)
            {
                // Check if first or last character in barcode and insert start/stop symbol
                if (position == 0)
                {
                    LinearEncoding.Add("*", 1, _patternDictionary[0]);
                    LinearEncoding.Add(" ", 1, _patternDictionary[1]);
                }

                ASCICode = barcodeValue[position];
                fullASCIICharacter = _symbolDictionary[ASCICode];


                for (int character = 0; character <= fullASCIICharacter.Count - 1; character++)
                {
                    fullASCIICode = fullASCIICharacter[character];
                    ASCIIChar = (char)fullASCIICode;
                    LinearEncoding.Add(ASCIIChar.ToString(), 0, _patternDictionary[fullASCIICode]);
                    LinearEncoding.Add(" ", 1, _patternDictionary[1]);
                    checkDigitRunning += _checkDigitDictionary[fullASCIICode];
                }

                if (position == barcodeValue.Length - 1)
                {
                    if (_useCheckDigit == true)
                    {
                        int checkDigit = checkDigitRunning % 43;
                        ASCICode = _checkDigitDictionary.FirstOrDefault(x => x.Value == checkDigit).Key;
                        char character = (char)ASCICode;
                        LinearEncoding.Add(character.ToString(), 1, _patternDictionary[ASCICode]);
                        LinearEncoding.Add(" ", 1, _patternDictionary[1]);
                    }
                    LinearEncoding.Add("*", 1, _patternDictionary[0]);
                }
            }

            SetMinXdimension();
            SetMinBarcodeHeight();

        }

        /// <summary>
        /// Increases the barcode Xdimension to minimum required by symbology, if currently set lower
        /// </summary>
        internal override void SetMinXdimension()
        {
            int xdimensionOriginal = Xdimension;
            int minXdimension = (int)Math.Ceiling(DPI * 0.0075);
            Xdimension = Math.Max(Xdimension, minXdimension);

            // Set flag to show xdimension was adjusted
            if (xdimensionOriginal != Xdimension)
            {
                XdimensionChanged = true;
            }
        }

        /// <summary>
        /// Increases the barcode height to minimum required by symbology, if currently set lower
        /// </summary>
        internal override void SetMinBarcodeHeight()
        {
            int barcodeHeightOriginal = BarcodeHeight;
            int minBarcodeHeight = (int)Math.Ceiling(Math.Max(LinearEncoding.MinimumWidth * Xdimension * 0.15, DPI * 0.25));
            BarcodeHeight = Math.Max(BarcodeHeight, minBarcodeHeight);

            // Set flag to show barcode height was adjusted
            if (barcodeHeightOriginal != BarcodeHeight)
            {
                BarcodeHeightChanged = true;
            }
        }

        private void LoadCode39FullASCIIDictionary()
        {
            if (_symbolDictionary != null)
            {
                return;
            }
            _symbolDictionary = new Dictionary<int, List<int>>();

            _symbolDictionary.Add(0, new List<int>() { 37, 85 });
            _symbolDictionary.Add(1, new List<int>() { 36, 65 });
            _symbolDictionary.Add(2, new List<int>() { 36, 66 });
            _symbolDictionary.Add(3, new List<int>() { 36, 67 });
            _symbolDictionary.Add(4, new List<int>() { 36, 68 });
            _symbolDictionary.Add(5, new List<int>() { 36, 69 });
            _symbolDictionary.Add(6, new List<int>() { 36, 70 });
            _symbolDictionary.Add(7, new List<int>() { 36, 71 });
            _symbolDictionary.Add(8, new List<int>() { 36, 72 });
            _symbolDictionary.Add(9, new List<int>() { 36, 73 });
            _symbolDictionary.Add(10, new List<int>() { 36, 74 });
            _symbolDictionary.Add(11, new List<int>() { 36, 75 });
            _symbolDictionary.Add(12, new List<int>() { 36, 76 });
            _symbolDictionary.Add(13, new List<int>() { 36, 77 });
            _symbolDictionary.Add(14, new List<int>() { 36, 78 });
            _symbolDictionary.Add(15, new List<int>() { 36, 79 });
            _symbolDictionary.Add(16, new List<int>() { 36, 80 });
            _symbolDictionary.Add(17, new List<int>() { 36, 81 });
            _symbolDictionary.Add(18, new List<int>() { 36, 82 });
            _symbolDictionary.Add(19, new List<int>() { 36, 83 });
            _symbolDictionary.Add(20, new List<int>() { 36, 84 });
            _symbolDictionary.Add(21, new List<int>() { 36, 85 });
            _symbolDictionary.Add(22, new List<int>() { 36, 86 });
            _symbolDictionary.Add(23, new List<int>() { 36, 87 });
            _symbolDictionary.Add(24, new List<int>() { 36, 88 });
            _symbolDictionary.Add(25, new List<int>() { 36, 89 });
            _symbolDictionary.Add(26, new List<int>() { 36, 90 });
            _symbolDictionary.Add(27, new List<int>() { 37, 65 });
            _symbolDictionary.Add(28, new List<int>() { 37, 66 });
            _symbolDictionary.Add(29, new List<int>() { 37, 67 });
            _symbolDictionary.Add(30, new List<int>() { 37, 68 });
            _symbolDictionary.Add(31, new List<int>() { 37, 69 });
            _symbolDictionary.Add(32, new List<int>() { 32 });
            _symbolDictionary.Add(33, new List<int>() { 47, 65 });
            _symbolDictionary.Add(34, new List<int>() { 47, 66 });
            _symbolDictionary.Add(35, new List<int>() { 47, 67 });
            _symbolDictionary.Add(36, new List<int>() { 47, 68 });
            _symbolDictionary.Add(37, new List<int>() { 47, 69 });
            _symbolDictionary.Add(38, new List<int>() { 47, 70 });
            _symbolDictionary.Add(39, new List<int>() { 47, 71 });
            _symbolDictionary.Add(40, new List<int>() { 47, 72 });
            _symbolDictionary.Add(41, new List<int>() { 47, 73 });
            _symbolDictionary.Add(42, new List<int>() { 47, 74 });
            _symbolDictionary.Add(43, new List<int>() { 47, 75 });
            _symbolDictionary.Add(44, new List<int>() { 47, 76 });
            _symbolDictionary.Add(45, new List<int>() { 45 });
            _symbolDictionary.Add(46, new List<int>() { 46 });
            _symbolDictionary.Add(47, new List<int>() { 47, 79 });
            _symbolDictionary.Add(48, new List<int>() { 48 });
            _symbolDictionary.Add(49, new List<int>() { 49 });
            _symbolDictionary.Add(50, new List<int>() { 50 });
            _symbolDictionary.Add(51, new List<int>() { 51 });
            _symbolDictionary.Add(52, new List<int>() { 52 });
            _symbolDictionary.Add(53, new List<int>() { 53 });
            _symbolDictionary.Add(54, new List<int>() { 54 });
            _symbolDictionary.Add(55, new List<int>() { 55 });
            _symbolDictionary.Add(56, new List<int>() { 56 });
            _symbolDictionary.Add(57, new List<int>() { 57 });
            _symbolDictionary.Add(58, new List<int>() { 47, 90 });
            _symbolDictionary.Add(59, new List<int>() { 37, 70 });
            _symbolDictionary.Add(60, new List<int>() { 37, 71 });
            _symbolDictionary.Add(61, new List<int>() { 37, 72 });
            _symbolDictionary.Add(62, new List<int>() { 37, 73 });
            _symbolDictionary.Add(63, new List<int>() { 37, 74 });
            _symbolDictionary.Add(64, new List<int>() { 37, 86 });
            _symbolDictionary.Add(65, new List<int>() { 65 });
            _symbolDictionary.Add(66, new List<int>() { 66 });
            _symbolDictionary.Add(67, new List<int>() { 67 });
            _symbolDictionary.Add(68, new List<int>() { 68 });
            _symbolDictionary.Add(69, new List<int>() { 69 });
            _symbolDictionary.Add(70, new List<int>() { 70 });
            _symbolDictionary.Add(71, new List<int>() { 71 });
            _symbolDictionary.Add(72, new List<int>() { 72 });
            _symbolDictionary.Add(73, new List<int>() { 73 });
            _symbolDictionary.Add(74, new List<int>() { 74 });
            _symbolDictionary.Add(75, new List<int>() { 75 });
            _symbolDictionary.Add(76, new List<int>() { 76 });
            _symbolDictionary.Add(77, new List<int>() { 77 });
            _symbolDictionary.Add(78, new List<int>() { 78 });
            _symbolDictionary.Add(79, new List<int>() { 79 });
            _symbolDictionary.Add(80, new List<int>() { 80 });
            _symbolDictionary.Add(81, new List<int>() { 81 });
            _symbolDictionary.Add(82, new List<int>() { 82 });
            _symbolDictionary.Add(83, new List<int>() { 83 });
            _symbolDictionary.Add(84, new List<int>() { 84 });
            _symbolDictionary.Add(85, new List<int>() { 85 });
            _symbolDictionary.Add(86, new List<int>() { 86 });
            _symbolDictionary.Add(87, new List<int>() { 87 });
            _symbolDictionary.Add(88, new List<int>() { 88 });
            _symbolDictionary.Add(89, new List<int>() { 89 });
            _symbolDictionary.Add(90, new List<int>() { 90 });
            _symbolDictionary.Add(91, new List<int>() { 37, 75 });
            _symbolDictionary.Add(92, new List<int>() { 37, 76 });
            _symbolDictionary.Add(93, new List<int>() { 37, 77 });
            _symbolDictionary.Add(94, new List<int>() { 37, 78 });
            _symbolDictionary.Add(95, new List<int>() { 37, 79 });
            _symbolDictionary.Add(96, new List<int>() { 37, 87 });
            _symbolDictionary.Add(97, new List<int>() { 43, 65 });
            _symbolDictionary.Add(98, new List<int>() { 43, 66 });
            _symbolDictionary.Add(99, new List<int>() { 43, 67 });
            _symbolDictionary.Add(100, new List<int>() { 43, 68 });
            _symbolDictionary.Add(101, new List<int>() { 43, 69 });
            _symbolDictionary.Add(102, new List<int>() { 43, 70 });
            _symbolDictionary.Add(103, new List<int>() { 43, 71 });
            _symbolDictionary.Add(104, new List<int>() { 43, 72 });
            _symbolDictionary.Add(105, new List<int>() { 43, 73 });
            _symbolDictionary.Add(106, new List<int>() { 43, 74 });
            _symbolDictionary.Add(107, new List<int>() { 43, 75 });
            _symbolDictionary.Add(108, new List<int>() { 43, 76 });
            _symbolDictionary.Add(109, new List<int>() { 43, 77 });
            _symbolDictionary.Add(110, new List<int>() { 43, 78 });
            _symbolDictionary.Add(111, new List<int>() { 43, 79 });
            _symbolDictionary.Add(112, new List<int>() { 43, 80 });
            _symbolDictionary.Add(113, new List<int>() { 43, 81 });
            _symbolDictionary.Add(114, new List<int>() { 43, 82 });
            _symbolDictionary.Add(115, new List<int>() { 43, 83 });
            _symbolDictionary.Add(116, new List<int>() { 43, 84 });
            _symbolDictionary.Add(117, new List<int>() { 43, 85 });
            _symbolDictionary.Add(118, new List<int>() { 43, 86 });
            _symbolDictionary.Add(119, new List<int>() { 43, 87 });
            _symbolDictionary.Add(120, new List<int>() { 43, 88 });
            _symbolDictionary.Add(121, new List<int>() { 43, 89 });
            _symbolDictionary.Add(122, new List<int>() { 43, 90 });
            _symbolDictionary.Add(123, new List<int>() { 37, 80 });
            _symbolDictionary.Add(124, new List<int>() { 37, 81 });
            _symbolDictionary.Add(125, new List<int>() { 37, 82 });
            _symbolDictionary.Add(126, new List<int>() { 37, 83 });
            _symbolDictionary.Add(127, new List<int>() { 37, 84 });

        }

        private void LoadCode39SymbolDictionary()
        {
            if (_symbolDictionary != null)
            {
                return;
            }
            _symbolDictionary = new Dictionary<int, List<int>>();

            _symbolDictionary.Add(32, new List<int>() { 32 });
            _symbolDictionary.Add(36, new List<int>() { 36 });
            _symbolDictionary.Add(37, new List<int>() { 37 });
            _symbolDictionary.Add(43, new List<int>() { 43 });
            _symbolDictionary.Add(45, new List<int>() { 45 });
            _symbolDictionary.Add(46, new List<int>() { 46 });
            _symbolDictionary.Add(47, new List<int>() { 47 });
            _symbolDictionary.Add(48, new List<int>() { 48 });
            _symbolDictionary.Add(49, new List<int>() { 49 });
            _symbolDictionary.Add(50, new List<int>() { 50 });
            _symbolDictionary.Add(51, new List<int>() { 51 });
            _symbolDictionary.Add(52, new List<int>() { 52 });
            _symbolDictionary.Add(53, new List<int>() { 53 });
            _symbolDictionary.Add(54, new List<int>() { 54 });
            _symbolDictionary.Add(55, new List<int>() { 55 });
            _symbolDictionary.Add(56, new List<int>() { 56 });
            _symbolDictionary.Add(57, new List<int>() { 57 });
            _symbolDictionary.Add(65, new List<int>() { 65 });
            _symbolDictionary.Add(66, new List<int>() { 66 });
            _symbolDictionary.Add(67, new List<int>() { 67 });
            _symbolDictionary.Add(68, new List<int>() { 68 });
            _symbolDictionary.Add(69, new List<int>() { 69 });
            _symbolDictionary.Add(70, new List<int>() { 70 });
            _symbolDictionary.Add(71, new List<int>() { 71 });
            _symbolDictionary.Add(72, new List<int>() { 72 });
            _symbolDictionary.Add(73, new List<int>() { 73 });
            _symbolDictionary.Add(74, new List<int>() { 74 });
            _symbolDictionary.Add(75, new List<int>() { 75 });
            _symbolDictionary.Add(76, new List<int>() { 76 });
            _symbolDictionary.Add(77, new List<int>() { 77 });
            _symbolDictionary.Add(78, new List<int>() { 78 });
            _symbolDictionary.Add(79, new List<int>() { 79 });
            _symbolDictionary.Add(80, new List<int>() { 80 });
            _symbolDictionary.Add(81, new List<int>() { 81 });
            _symbolDictionary.Add(82, new List<int>() { 82 });
            _symbolDictionary.Add(83, new List<int>() { 83 });
            _symbolDictionary.Add(84, new List<int>() { 84 });
            _symbolDictionary.Add(85, new List<int>() { 85 });
            _symbolDictionary.Add(86, new List<int>() { 86 });
            _symbolDictionary.Add(87, new List<int>() { 87 });
            _symbolDictionary.Add(88, new List<int>() { 88 });
            _symbolDictionary.Add(89, new List<int>() { 89 });
            _symbolDictionary.Add(90, new List<int>() { 90 });
        }

        private void LoadSymbologyPattern()
        {
            if (_patternDictionary != null)
            {
                return;
            }
            _patternDictionary = new Dictionary<int, LinearPattern>();

            _patternDictionary.Add(0, new LinearPattern("131131311", ModuleType.Bar));
            _patternDictionary.Add(1, new LinearPattern("1", ModuleType.Space));
            _patternDictionary.Add(32, new LinearPattern("133111311", ModuleType.Bar));
            _patternDictionary.Add(36, new LinearPattern("131313111", ModuleType.Bar));
            _patternDictionary.Add(37, new LinearPattern("111313131", ModuleType.Bar));
            _patternDictionary.Add(43, new LinearPattern("131113131", ModuleType.Bar));
            _patternDictionary.Add(45, new LinearPattern("131111313", ModuleType.Bar));
            _patternDictionary.Add(46, new LinearPattern("331111311", ModuleType.Bar));
            _patternDictionary.Add(47, new LinearPattern("131311131", ModuleType.Bar));
            _patternDictionary.Add(48, new LinearPattern("111331311", ModuleType.Bar));
            _patternDictionary.Add(49, new LinearPattern("311311113", ModuleType.Bar));
            _patternDictionary.Add(50, new LinearPattern("113311113", ModuleType.Bar));
            _patternDictionary.Add(51, new LinearPattern("313311111", ModuleType.Bar));
            _patternDictionary.Add(52, new LinearPattern("111331113", ModuleType.Bar));
            _patternDictionary.Add(53, new LinearPattern("311331111", ModuleType.Bar));
            _patternDictionary.Add(54, new LinearPattern("113331111", ModuleType.Bar));
            _patternDictionary.Add(55, new LinearPattern("111311313", ModuleType.Bar));
            _patternDictionary.Add(56, new LinearPattern("311311311", ModuleType.Bar));
            _patternDictionary.Add(57, new LinearPattern("113311311", ModuleType.Bar));
            _patternDictionary.Add(65, new LinearPattern("311113113", ModuleType.Bar));
            _patternDictionary.Add(66, new LinearPattern("113113113", ModuleType.Bar));
            _patternDictionary.Add(67, new LinearPattern("313113111", ModuleType.Bar));
            _patternDictionary.Add(68, new LinearPattern("111133113", ModuleType.Bar));
            _patternDictionary.Add(69, new LinearPattern("311133111", ModuleType.Bar));
            _patternDictionary.Add(70, new LinearPattern("113133111", ModuleType.Bar));
            _patternDictionary.Add(71, new LinearPattern("111113313", ModuleType.Bar));
            _patternDictionary.Add(72, new LinearPattern("311113311", ModuleType.Bar));
            _patternDictionary.Add(73, new LinearPattern("113113311", ModuleType.Bar));
            _patternDictionary.Add(74, new LinearPattern("111133311", ModuleType.Bar));
            _patternDictionary.Add(75, new LinearPattern("311111133", ModuleType.Bar));
            _patternDictionary.Add(76, new LinearPattern("113111133", ModuleType.Bar));
            _patternDictionary.Add(77, new LinearPattern("313111131", ModuleType.Bar));
            _patternDictionary.Add(78, new LinearPattern("111131133", ModuleType.Bar));
            _patternDictionary.Add(79, new LinearPattern("311131131", ModuleType.Bar));
            _patternDictionary.Add(80, new LinearPattern("113131131", ModuleType.Bar));
            _patternDictionary.Add(81, new LinearPattern("111111333", ModuleType.Bar));
            _patternDictionary.Add(82, new LinearPattern("311111331", ModuleType.Bar));
            _patternDictionary.Add(83, new LinearPattern("113111331", ModuleType.Bar));
            _patternDictionary.Add(84, new LinearPattern("111131331", ModuleType.Bar));
            _patternDictionary.Add(85, new LinearPattern("331111113", ModuleType.Bar));
            _patternDictionary.Add(86, new LinearPattern("133111113", ModuleType.Bar));
            _patternDictionary.Add(87, new LinearPattern("333111111", ModuleType.Bar));
            _patternDictionary.Add(88, new LinearPattern("131131113", ModuleType.Bar));
            _patternDictionary.Add(89, new LinearPattern("331131111", ModuleType.Bar));
            _patternDictionary.Add(90, new LinearPattern("133131111", ModuleType.Bar));

        }

        private void LoadCheckDigitDictionary()
        {
            if (_checkDigitDictionary != null)
            {
                return;
            }
            _checkDigitDictionary = new Dictionary<int, int>();

            _checkDigitDictionary.Add(32, 38);
            _checkDigitDictionary.Add(36, 39);
            _checkDigitDictionary.Add(37, 42);
            _checkDigitDictionary.Add(43, 41);
            _checkDigitDictionary.Add(45, 36);
            _checkDigitDictionary.Add(46, 37);
            _checkDigitDictionary.Add(47, 40);
            _checkDigitDictionary.Add(48, 0);
            _checkDigitDictionary.Add(49, 1);
            _checkDigitDictionary.Add(50, 2);
            _checkDigitDictionary.Add(51, 3);
            _checkDigitDictionary.Add(52, 4);
            _checkDigitDictionary.Add(53, 5);
            _checkDigitDictionary.Add(54, 6);
            _checkDigitDictionary.Add(55, 7);
            _checkDigitDictionary.Add(56, 8);
            _checkDigitDictionary.Add(57, 9);
            _checkDigitDictionary.Add(65, 10);
            _checkDigitDictionary.Add(66, 11);
            _checkDigitDictionary.Add(67, 12);
            _checkDigitDictionary.Add(68, 13);
            _checkDigitDictionary.Add(69, 14);
            _checkDigitDictionary.Add(70, 15);
            _checkDigitDictionary.Add(71, 16);
            _checkDigitDictionary.Add(72, 17);
            _checkDigitDictionary.Add(73, 18);
            _checkDigitDictionary.Add(74, 19);
            _checkDigitDictionary.Add(75, 20);
            _checkDigitDictionary.Add(76, 21);
            _checkDigitDictionary.Add(77, 22);
            _checkDigitDictionary.Add(78, 23);
            _checkDigitDictionary.Add(79, 24);
            _checkDigitDictionary.Add(80, 25);
            _checkDigitDictionary.Add(81, 26);
            _checkDigitDictionary.Add(82, 27);
            _checkDigitDictionary.Add(83, 28);
            _checkDigitDictionary.Add(84, 29);
            _checkDigitDictionary.Add(85, 30);
            _checkDigitDictionary.Add(86, 31);
            _checkDigitDictionary.Add(87, 32);
            _checkDigitDictionary.Add(88, 33);
            _checkDigitDictionary.Add(89, 34);
            _checkDigitDictionary.Add(90, 35);

        }
    }
}
