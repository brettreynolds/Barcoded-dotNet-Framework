using System;
using System.Collections.Generic;
using System.Text;

namespace Barcoded
{
    class Code128Encoder : LinearEncoder
    {
        private Dictionary<string, int> _symbologyCharEncode;
        private Dictionary<int, LinearPattern> _patternDictionary;
        //private LinearEncoding _coded = new LinearEncoding();
        private int _checkDigit = 0;

        private bool SuppressSubsetC { get; set; } = false;
        private Code128Subset StartSubset { get; set; } = Code128Subset.B;

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

        internal override void Setup()
        {

        }

        internal override ILinearValidator BarcodeValidator { get; } = new Code128Validator();

        internal override void Encode(string barcodeValue)
        {
            LoadCode128Symbology();
            LoadSymbologyPattern();
            AnalyseSection(barcodeValue, 0, Code128Subset.Null);
        }

        private void AnalyseSection(string barcodeValue, int startPosition, Code128Subset lastSubset)
        {

            Code128Subset currentSubset = Code128Subset.Null;
            int ASCIICode;
            int sectionCount = 0;
            bool newSection = false;

            for (int position = startPosition; position <= barcodeValue.Length - 1; position++)
            {
                newSection = true;
                ASCIICode = barcodeValue[position];
                sectionCount += 1;

                switch (ASCIICode)
                {
                    case int code when code <= 31:

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

                    case int code when code >= 96:

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

                    case int code when code >= 48 && code <= 57:

                        if (SuppressSubsetC == true)
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

            if (newSection == true)
            {
                if (currentSubset == Code128Subset.Null)
                {
                    currentSubset = StartSubset;
                }
                EncodeSection(lastSubset, currentSubset, barcodeValue.Substring(startPosition, barcodeValue.Length - startPosition));
            }

            AddCheckDigit();
            AddStopSymbol();
            SetMinXdimension();
            SetMinBarcodeHeight();
        }

        private int GetSubsetCSequenceCount(string barcodeValue, int startPosition)
        {
            int numberSequenceCount = 0;
            int ASCIICode;
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

                ASCIICode = barcodeValue[position];

                if (ASCIICode >= 48 && ASCIICode <= 57)
                {
                    numberSequenceCount += 1;
                }
                else
                {
                    break;
                }
            }

            if (startSubsetC == true && endSubsetC == true && (numberSequenceCount % 2) == 0)            // The complete barcode should be encoded as subset C
            {
                returnCount = numberSequenceCount;
            }
            else if (startSubsetC == true && numberSequenceCount >= 4)                                   // The first subset to be used is subset C
            {
                returnCount = numberSequenceCount - (numberSequenceCount % 2);
            }
            else if (endSubsetC == true && numberSequenceCount >= 4 && (numberSequenceCount % 2) == 0)    // The last subset to be used is subset C
            {
                returnCount = numberSequenceCount;
            }
            else if (numberSequenceCount >= 6 && (numberSequenceCount % 2) == 0)                         // Subset C should be used inbetween A or B subsets
            {
                returnCount = numberSequenceCount;
            }

            return returnCount;
        }

        public void EncodeSection(Code128Subset lastSubset, Code128Subset sectionSubset, string sectionValue)
        {
            string character = GetSubsetAsString(sectionSubset);
            int symbol = _symbologyCharEncode[GetSubsetAsString(lastSubset) + GetSubsetAsString(sectionSubset)];
            LinearPattern SymbolPattern = _patternDictionary[symbol];

            AddSymbolToEncode(character, 1, symbol, SymbolPattern);

            if (sectionSubset == Code128Subset.C)    //Subset C encoder
            {
                for (int encodePosition = 0; encodePosition <= sectionValue.Length - 1; encodePosition += 2)
                {
                    // Encode the current numeric pair symbol
                    character = sectionValue.Substring(encodePosition, 2);
                    symbol = System.Convert.ToInt32(character);
                    SymbolPattern = _patternDictionary[symbol];
                    AddSymbolToEncode(character, 0, symbol, SymbolPattern);
                }
            }
            else
            {
                for (int encodePosition = 0; encodePosition <= sectionValue.Length - 1; encodePosition++)
                {
                    character = sectionValue.Substring(encodePosition, 1);
                    symbol = _symbologyCharEncode[((int)sectionValue[encodePosition]).ToString()];
                    SymbolPattern = _patternDictionary[symbol];
                    AddSymbolToEncode(character, 0, symbol, SymbolPattern);
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
        internal override void SetMinXdimension()
        {
            int xdimensionOriginal = Xdimension;
            int minXdimension = (int)Math.Ceiling(DPI * 0.0075);
            Xdimension = Math.Max(Xdimension, minXdimension);

            // Set flag to show xdimension was adjusted
            if(xdimensionOriginal != Xdimension)
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

            throw new ArgumentException(subset + " is not a recognised Code128 subset", "subset");
        }

        private void LoadCode128Symbology()
        {
            if (_symbologyCharEncode != null)
            {
                return;
            }

            _symbologyCharEncode = new Dictionary<string, int>();

            _symbologyCharEncode.Add("0", 64);
            _symbologyCharEncode.Add("1", 65);
            _symbologyCharEncode.Add("2", 66);
            _symbologyCharEncode.Add("3", 67);
            _symbologyCharEncode.Add("4", 68);
            _symbologyCharEncode.Add("5", 69);
            _symbologyCharEncode.Add("6", 70);
            _symbologyCharEncode.Add("7", 71);
            _symbologyCharEncode.Add("8", 72);
            _symbologyCharEncode.Add("9", 73);
            _symbologyCharEncode.Add("10", 74);
            _symbologyCharEncode.Add("11", 75);
            _symbologyCharEncode.Add("12", 76);
            _symbologyCharEncode.Add("13", 77);
            _symbologyCharEncode.Add("14", 78);
            _symbologyCharEncode.Add("15", 79);
            _symbologyCharEncode.Add("16", 80);
            _symbologyCharEncode.Add("17", 81);
            _symbologyCharEncode.Add("18", 82);
            _symbologyCharEncode.Add("19", 83);
            _symbologyCharEncode.Add("20", 84);
            _symbologyCharEncode.Add("21", 85);
            _symbologyCharEncode.Add("22", 86);
            _symbologyCharEncode.Add("23", 87);
            _symbologyCharEncode.Add("24", 88);
            _symbologyCharEncode.Add("25", 89);
            _symbologyCharEncode.Add("26", 90);
            _symbologyCharEncode.Add("27", 91);
            _symbologyCharEncode.Add("28", 92);
            _symbologyCharEncode.Add("29", 93);
            _symbologyCharEncode.Add("30", 94);
            _symbologyCharEncode.Add("31", 95);
            _symbologyCharEncode.Add("32", 0);
            _symbologyCharEncode.Add("33", 1);
            _symbologyCharEncode.Add("34", 2);
            _symbologyCharEncode.Add("35", 3);
            _symbologyCharEncode.Add("36", 4);
            _symbologyCharEncode.Add("37", 5);
            _symbologyCharEncode.Add("38", 6);
            _symbologyCharEncode.Add("39", 7);
            _symbologyCharEncode.Add("40", 8);
            _symbologyCharEncode.Add("41", 9);
            _symbologyCharEncode.Add("42", 10);
            _symbologyCharEncode.Add("43", 11);
            _symbologyCharEncode.Add("44", 12);
            _symbologyCharEncode.Add("45", 13);
            _symbologyCharEncode.Add("46", 14);
            _symbologyCharEncode.Add("47", 15);
            _symbologyCharEncode.Add("48", 16);
            _symbologyCharEncode.Add("49", 17);
            _symbologyCharEncode.Add("50", 18);
            _symbologyCharEncode.Add("51", 19);
            _symbologyCharEncode.Add("52", 20);
            _symbologyCharEncode.Add("53", 21);
            _symbologyCharEncode.Add("54", 22);
            _symbologyCharEncode.Add("55", 23);
            _symbologyCharEncode.Add("56", 24);
            _symbologyCharEncode.Add("57", 25);
            _symbologyCharEncode.Add("58", 26);
            _symbologyCharEncode.Add("59", 27);
            _symbologyCharEncode.Add("60", 28);
            _symbologyCharEncode.Add("61", 29);
            _symbologyCharEncode.Add("62", 30);
            _symbologyCharEncode.Add("63", 31);
            _symbologyCharEncode.Add("64", 32);
            _symbologyCharEncode.Add("65", 33);
            _symbologyCharEncode.Add("66", 34);
            _symbologyCharEncode.Add("67", 35);
            _symbologyCharEncode.Add("68", 36);
            _symbologyCharEncode.Add("69", 37);
            _symbologyCharEncode.Add("70", 38);
            _symbologyCharEncode.Add("71", 39);
            _symbologyCharEncode.Add("72", 40);
            _symbologyCharEncode.Add("73", 41);
            _symbologyCharEncode.Add("74", 42);
            _symbologyCharEncode.Add("75", 43);
            _symbologyCharEncode.Add("76", 44);
            _symbologyCharEncode.Add("77", 45);
            _symbologyCharEncode.Add("78", 46);
            _symbologyCharEncode.Add("79", 47);
            _symbologyCharEncode.Add("80", 48);
            _symbologyCharEncode.Add("81", 49);
            _symbologyCharEncode.Add("82", 50);
            _symbologyCharEncode.Add("83", 51);
            _symbologyCharEncode.Add("84", 52);
            _symbologyCharEncode.Add("85", 53);
            _symbologyCharEncode.Add("86", 54);
            _symbologyCharEncode.Add("87", 55);
            _symbologyCharEncode.Add("88", 56);
            _symbologyCharEncode.Add("89", 57);
            _symbologyCharEncode.Add("90", 58);
            _symbologyCharEncode.Add("91", 59);
            _symbologyCharEncode.Add("92", 60);
            _symbologyCharEncode.Add("93", 61);
            _symbologyCharEncode.Add("94", 62);
            _symbologyCharEncode.Add("95", 63);
            _symbologyCharEncode.Add("96", 64);
            _symbologyCharEncode.Add("97", 65);
            _symbologyCharEncode.Add("98", 66);
            _symbologyCharEncode.Add("99", 67);
            _symbologyCharEncode.Add("100", 68);
            _symbologyCharEncode.Add("101", 69);
            _symbologyCharEncode.Add("102", 70);
            _symbologyCharEncode.Add("103", 71);
            _symbologyCharEncode.Add("104", 72);
            _symbologyCharEncode.Add("105", 73);
            _symbologyCharEncode.Add("106", 74);
            _symbologyCharEncode.Add("107", 75);
            _symbologyCharEncode.Add("108", 76);
            _symbologyCharEncode.Add("109", 77);
            _symbologyCharEncode.Add("110", 78);
            _symbologyCharEncode.Add("111", 79);
            _symbologyCharEncode.Add("112", 80);
            _symbologyCharEncode.Add("113", 81);
            _symbologyCharEncode.Add("114", 82);
            _symbologyCharEncode.Add("115", 83);
            _symbologyCharEncode.Add("116", 84);
            _symbologyCharEncode.Add("117", 85);
            _symbologyCharEncode.Add("118", 86);
            _symbologyCharEncode.Add("119", 87);
            _symbologyCharEncode.Add("120", 88);
            _symbologyCharEncode.Add("121", 89);
            _symbologyCharEncode.Add("122", 90);
            _symbologyCharEncode.Add("123", 91);
            _symbologyCharEncode.Add("124", 92);
            _symbologyCharEncode.Add("125", 93);
            _symbologyCharEncode.Add("126", 94);
            _symbologyCharEncode.Add("127", 95);
            _symbologyCharEncode.Add("SAB", 98);
            _symbologyCharEncode.Add("SBA", 98);
            _symbologyCharEncode.Add("AC", 99);
            _symbologyCharEncode.Add("BC", 99);
            _symbologyCharEncode.Add("AB", 100);
            _symbologyCharEncode.Add("CB", 100);
            _symbologyCharEncode.Add("BA", 101);
            _symbologyCharEncode.Add("CA", 101);
            _symbologyCharEncode.Add("A", 103);
            _symbologyCharEncode.Add("B", 104);
            _symbologyCharEncode.Add("C", 105);
            _symbologyCharEncode.Add("S", 106);
        }

        private void LoadSymbologyPattern()
        {
            if (_patternDictionary != null)
            {
                return;
            }
            _patternDictionary = new Dictionary<int, LinearPattern>();

            _patternDictionary.Add(0, new LinearPattern("212222", ModuleType.Bar));
            _patternDictionary.Add(1, new LinearPattern("222122", ModuleType.Bar));
            _patternDictionary.Add(2, new LinearPattern("222221", ModuleType.Bar));
            _patternDictionary.Add(3, new LinearPattern("121223", ModuleType.Bar));
            _patternDictionary.Add(4, new LinearPattern("121322", ModuleType.Bar));
            _patternDictionary.Add(5, new LinearPattern("131222", ModuleType.Bar));
            _patternDictionary.Add(6, new LinearPattern("122213", ModuleType.Bar));
            _patternDictionary.Add(7, new LinearPattern("122312", ModuleType.Bar));
            _patternDictionary.Add(8, new LinearPattern("132212", ModuleType.Bar));
            _patternDictionary.Add(9, new LinearPattern("221213", ModuleType.Bar));
            _patternDictionary.Add(10, new LinearPattern("221312", ModuleType.Bar));
            _patternDictionary.Add(11, new LinearPattern("231212", ModuleType.Bar));
            _patternDictionary.Add(12, new LinearPattern("112232", ModuleType.Bar));
            _patternDictionary.Add(13, new LinearPattern("122132", ModuleType.Bar));
            _patternDictionary.Add(14, new LinearPattern("122231", ModuleType.Bar));
            _patternDictionary.Add(15, new LinearPattern("113222", ModuleType.Bar));
            _patternDictionary.Add(16, new LinearPattern("123122", ModuleType.Bar));
            _patternDictionary.Add(17, new LinearPattern("123221", ModuleType.Bar));
            _patternDictionary.Add(18, new LinearPattern("223211", ModuleType.Bar));
            _patternDictionary.Add(19, new LinearPattern("221132", ModuleType.Bar));
            _patternDictionary.Add(20, new LinearPattern("221231", ModuleType.Bar));
            _patternDictionary.Add(21, new LinearPattern("213212", ModuleType.Bar));
            _patternDictionary.Add(22, new LinearPattern("223112", ModuleType.Bar));
            _patternDictionary.Add(23, new LinearPattern("312131", ModuleType.Bar));
            _patternDictionary.Add(24, new LinearPattern("311222", ModuleType.Bar));
            _patternDictionary.Add(25, new LinearPattern("321122", ModuleType.Bar));
            _patternDictionary.Add(26, new LinearPattern("321221", ModuleType.Bar));
            _patternDictionary.Add(27, new LinearPattern("312212", ModuleType.Bar));
            _patternDictionary.Add(28, new LinearPattern("322112", ModuleType.Bar));
            _patternDictionary.Add(29, new LinearPattern("322211", ModuleType.Bar));
            _patternDictionary.Add(30, new LinearPattern("212123", ModuleType.Bar));
            _patternDictionary.Add(31, new LinearPattern("212321", ModuleType.Bar));
            _patternDictionary.Add(32, new LinearPattern("232121", ModuleType.Bar));
            _patternDictionary.Add(33, new LinearPattern("111323", ModuleType.Bar));
            _patternDictionary.Add(34, new LinearPattern("131123", ModuleType.Bar));
            _patternDictionary.Add(35, new LinearPattern("131321", ModuleType.Bar));
            _patternDictionary.Add(36, new LinearPattern("112313", ModuleType.Bar));
            _patternDictionary.Add(37, new LinearPattern("132113", ModuleType.Bar));
            _patternDictionary.Add(38, new LinearPattern("132311", ModuleType.Bar));
            _patternDictionary.Add(39, new LinearPattern("211313", ModuleType.Bar));
            _patternDictionary.Add(40, new LinearPattern("231113", ModuleType.Bar));
            _patternDictionary.Add(41, new LinearPattern("231311", ModuleType.Bar));
            _patternDictionary.Add(42, new LinearPattern("112133", ModuleType.Bar));
            _patternDictionary.Add(43, new LinearPattern("112331", ModuleType.Bar));
            _patternDictionary.Add(44, new LinearPattern("132131", ModuleType.Bar));
            _patternDictionary.Add(45, new LinearPattern("113123", ModuleType.Bar));
            _patternDictionary.Add(46, new LinearPattern("113321", ModuleType.Bar));
            _patternDictionary.Add(47, new LinearPattern("133121", ModuleType.Bar));
            _patternDictionary.Add(48, new LinearPattern("313121", ModuleType.Bar));
            _patternDictionary.Add(49, new LinearPattern("211331", ModuleType.Bar));
            _patternDictionary.Add(50, new LinearPattern("231131", ModuleType.Bar));
            _patternDictionary.Add(51, new LinearPattern("213113", ModuleType.Bar));
            _patternDictionary.Add(52, new LinearPattern("213311", ModuleType.Bar));
            _patternDictionary.Add(53, new LinearPattern("213131", ModuleType.Bar));
            _patternDictionary.Add(54, new LinearPattern("311123", ModuleType.Bar));
            _patternDictionary.Add(55, new LinearPattern("311321", ModuleType.Bar));
            _patternDictionary.Add(56, new LinearPattern("331121", ModuleType.Bar));
            _patternDictionary.Add(57, new LinearPattern("312113", ModuleType.Bar));
            _patternDictionary.Add(58, new LinearPattern("312311", ModuleType.Bar));
            _patternDictionary.Add(59, new LinearPattern("332111", ModuleType.Bar));
            _patternDictionary.Add(60, new LinearPattern("314111", ModuleType.Bar));
            _patternDictionary.Add(61, new LinearPattern("221411", ModuleType.Bar));
            _patternDictionary.Add(62, new LinearPattern("431111", ModuleType.Bar));
            _patternDictionary.Add(63, new LinearPattern("111224", ModuleType.Bar));
            _patternDictionary.Add(64, new LinearPattern("111422", ModuleType.Bar));
            _patternDictionary.Add(65, new LinearPattern("121124", ModuleType.Bar));
            _patternDictionary.Add(66, new LinearPattern("121421", ModuleType.Bar));
            _patternDictionary.Add(67, new LinearPattern("141122", ModuleType.Bar));
            _patternDictionary.Add(68, new LinearPattern("141221", ModuleType.Bar));
            _patternDictionary.Add(69, new LinearPattern("112214", ModuleType.Bar));
            _patternDictionary.Add(70, new LinearPattern("112412", ModuleType.Bar));
            _patternDictionary.Add(71, new LinearPattern("122114", ModuleType.Bar));
            _patternDictionary.Add(72, new LinearPattern("122411", ModuleType.Bar));
            _patternDictionary.Add(73, new LinearPattern("142112", ModuleType.Bar));
            _patternDictionary.Add(74, new LinearPattern("142211", ModuleType.Bar));
            _patternDictionary.Add(75, new LinearPattern("241211", ModuleType.Bar));
            _patternDictionary.Add(76, new LinearPattern("221114", ModuleType.Bar));
            _patternDictionary.Add(77, new LinearPattern("413111", ModuleType.Bar));
            _patternDictionary.Add(78, new LinearPattern("241112", ModuleType.Bar));
            _patternDictionary.Add(79, new LinearPattern("134111", ModuleType.Bar));
            _patternDictionary.Add(80, new LinearPattern("111242", ModuleType.Bar));
            _patternDictionary.Add(81, new LinearPattern("121142", ModuleType.Bar));
            _patternDictionary.Add(82, new LinearPattern("121241", ModuleType.Bar));
            _patternDictionary.Add(83, new LinearPattern("114212", ModuleType.Bar));
            _patternDictionary.Add(84, new LinearPattern("124112", ModuleType.Bar));
            _patternDictionary.Add(85, new LinearPattern("124211", ModuleType.Bar));
            _patternDictionary.Add(86, new LinearPattern("411212", ModuleType.Bar));
            _patternDictionary.Add(87, new LinearPattern("421112", ModuleType.Bar));
            _patternDictionary.Add(88, new LinearPattern("421211", ModuleType.Bar));
            _patternDictionary.Add(89, new LinearPattern("212141", ModuleType.Bar));
            _patternDictionary.Add(90, new LinearPattern("214121", ModuleType.Bar));
            _patternDictionary.Add(91, new LinearPattern("412121", ModuleType.Bar));
            _patternDictionary.Add(92, new LinearPattern("111143", ModuleType.Bar));
            _patternDictionary.Add(93, new LinearPattern("111341", ModuleType.Bar));
            _patternDictionary.Add(94, new LinearPattern("131141", ModuleType.Bar));
            _patternDictionary.Add(95, new LinearPattern("114113", ModuleType.Bar));
            _patternDictionary.Add(96, new LinearPattern("114311", ModuleType.Bar));
            _patternDictionary.Add(97, new LinearPattern("411113", ModuleType.Bar));
            _patternDictionary.Add(98, new LinearPattern("411311", ModuleType.Bar));
            _patternDictionary.Add(99, new LinearPattern("113141", ModuleType.Bar));
            _patternDictionary.Add(100, new LinearPattern("114131", ModuleType.Bar));
            _patternDictionary.Add(101, new LinearPattern("311141", ModuleType.Bar));
            _patternDictionary.Add(102, new LinearPattern("411131", ModuleType.Bar));
            _patternDictionary.Add(103, new LinearPattern("211412", ModuleType.Bar));
            _patternDictionary.Add(104, new LinearPattern("211214", ModuleType.Bar));
            _patternDictionary.Add(105, new LinearPattern("211232", ModuleType.Bar));
            _patternDictionary.Add(106, new LinearPattern("2331112", ModuleType.Bar));

        }
    }
}
