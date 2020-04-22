using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Barcoded
{

    public abstract class LinearEncoder
    {
        protected LinearEncoder(Symbology symbology)
        {
            Symbology = symbology;
        }

        /// <summary>
        /// Encode the value using the barcode symbology selected.
        /// </summary>
        /// <param name="barcodeValue"></param>
        protected abstract void Encode(string barcodeValue);

        internal void Generate(string barcodeValue)
        {
            EncodedValue = BarcodeValidator.Parse(barcodeValue, Symbology);
            Encode(EncodedValue);
        }

        /// <summary>
        /// Set the minimum barcode height for the barcode symbology used.
        /// </summary>
        internal abstract void SetMinBarcodeHeight();

        /// <summary>
        /// Set the minimum XDimension for the barcode symbology used.
        /// </summary>
        internal abstract void SetMinXDimension();

        /// <summary>
        /// Get the generated barcode as a bitmap image.
        /// </summary>
        /// <param name="barcodeValue"></param>
        /// <returns>Barcode image</returns>
        internal MemoryStream GetImage(string barcodeValue)
        {
            LinearEncoding.Clear();
            Generate(barcodeValue);
            return LinearRenderer.DrawImageMemoryStream(this);
        }


        /// <summary>
        /// Reset the Property Changed flag to false.
        /// </summary>
        internal void ResetPropertyChanged()
        {
            PropertyChanged = false;
        }

        /// <summary>
        /// Internal Barcode Value that can be reached by the renderer.
        /// </summary>
        public string EncodedValue { get; internal set; }

        /// <summary>
        /// Barcode symbology.
        /// </summary>
        public Symbology Symbology { get; set; }

        /// <summary>
        /// Encoder description.
        /// </summary>
        public string Description { get; internal set; }

        internal LinearEncoding LinearEncoding { get; } = new LinearEncoding();

        /// <summary>
        /// ZPL encoded string.
        /// </summary>
        internal string ZplEncode { get; set; }
        
        /// <summary>
        /// Used to detect if any properties have changed.
        /// </summary>
        public bool PropertyChanged { get; internal set; }

        /// <summary>
        /// Symbology specific validator.
        /// </summary>
        internal abstract ILinearValidator BarcodeValidator { get; }

        private string _humanReadableValue = "";
        /// <summary>
        /// The human readable value.
        /// </summary>
        public string HumanReadableValue
        {
            get => _humanReadableValue;
            set
            {
                _humanReadableValue = value;
                PropertyChanged = true;
            }
        }

        private bool _humanReadableSymbolAligned;
        /// <summary>
        /// Sets the human readable label to align with associated barcode symbol
        /// </summary>
        public bool HumanReadableSymbolAligned
        {
            get => _humanReadableSymbolAligned;
            set
            {
                _humanReadableSymbolAligned = value;
                PropertyChanged = true;
            }
        }

        /// <summary>
        /// Sets the visibility and position of the human readable value.
        /// Hidden, Above, Below, Embedded
        /// </summary>
        public HumanReadablePosition HumanReadablePosition { get; private set; }
        public void SetHumanReadablePosition(string position)
        {
            switch(position.ToUpper())
            {
                case "ABOVE":
                    HumanReadablePosition = HumanReadablePosition.Above;
                    break;
                case "TOP":
                    HumanReadablePosition = HumanReadablePosition.Above;
                    break;
                case "BELOW":
                    HumanReadablePosition = HumanReadablePosition.Below;
                    break;
                case "BOTTOM":
                    HumanReadablePosition = HumanReadablePosition.Below;
                    break;
                case "HIDDEN":
                    HumanReadablePosition = HumanReadablePosition.Hidden;
                    break;
                case "EMBEDDED":
                    HumanReadablePosition = HumanReadablePosition.Embedded;
                    break;
                default:
                    HumanReadablePosition = HumanReadablePosition.Hidden;
                    break;
            }
            PropertyChanged = true;
        }

        /// <summary>
        /// The font to be used for the human readable value, if shown.
        /// Will default to the system default font if not set.
        /// </summary>
        public Font HumanReadableFont { get; internal set; } = SystemFonts.DefaultFont;

        /// <summary>
        /// Set the font to be used for the human readable label.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="pointSize"></param>
        public void SetHumanReadableFont(string fontFamily, int pointSize)
        {
            if(pointSize < 1)
            {
                pointSize = (int)SystemFonts.DefaultFont.Size;
            }

            try
            {
                HumanReadableFont = new Font(new FontFamily(fontFamily), pointSize);
            }
            catch (Exception)
            {
                HumanReadableFont = SystemFonts.DefaultFont;
            }

            PropertyChanged = true;
        }

        private const int BarcodeHeightMin = 1;
        private const int BarcodeHeightMax = 2400;

        private int _barcodeHeight = 1;
        /// <summary>
        /// Sets the desired height in pixels for the barcode element of the image.
        /// </summary>
        public int BarcodeHeight
        {
            get => _barcodeHeight;
            set
            {
                int barcodeHeightOriginal = value;
                _barcodeHeight = (value < BarcodeHeightMin) ? BarcodeHeightMin : (value > BarcodeHeightMax) ? BarcodeHeightMax : value;
                if(barcodeHeightOriginal != _barcodeHeight)
                {
                    BarcodeHeightChanged = true;
                }
                PropertyChanged = true;
            }
        }
        
        private int _xDimension = 1;
        /// <summary>
        /// X-dimension is the width of the narrowest bar element in the barcode.
        /// All other bar and spaces widths in the barcode are a multiple of this value.
        /// </summary>
        public int XDimension
        {
            get => _xDimension;
            set
            {
                _xDimension = value;
                PropertyChanged = true;
            }
        }

        private int _wideBarRatio = 3;
        /// <summary>
        /// X-dimension is the width of the narrowest bar element in the barcode.
        /// All other bar and spaces widths in the barcode are a multiple of this value.
        /// </summary>
        public int WideBarRatio
        {
            get => _wideBarRatio;
            set
            {
                _wideBarRatio = value > 2? 3 : 2;
                PropertyChanged = true;
            }
        }

        private const int DpiMin = 1;
        private const int DpiMax = 600;

        private int _dpi = 300;
        /// <summary>
        /// Sets the desired image dpi.
        /// </summary>
        public int Dpi
        {
            get => _dpi;
            set
            {
                int dpiOriginal = value;
                _dpi = (value < DpiMin) ? DpiMin : (value > DpiMax) ? DpiMax : value;
                if(dpiOriginal != _dpi)
                {
                    DpiChanged = true;
                }
                PropertyChanged = true;
            }
        }

        private int _targetWidth;
        /// <summary>
        /// Target pixel width for the barcode.
        /// If set, the encoder will attempt to get as close to this value without exceeding, when generating the barcode
        /// </summary>
        public int TargetWidth
        {
            get => _targetWidth;
            set
            {
                _targetWidth = value;
                PropertyChanged = true;
            }
        }

        private bool _showEncoding;
        /// <summary>
        /// When true, will include a human readable label of the barcode encoding values
        /// for the corresponding part of the barcode. The position of this label will adjust
        /// top or bottom, dependent on the use of a human readable value and its position. Default is top.
        /// </summary>
        public bool ShowEncoding
        {
            get => _showEncoding;
            set
            {
                _showEncoding = value;
                PropertyChanged = true;
            }
        }

        /// <summary>
        /// The font to be used for the encoding label if shown.
        /// Will default to the system default font, if not set.
        /// </summary>
        public FontFamily EncodingFontFamily { get; private set; } = SystemFonts.DefaultFont.FontFamily;

        public void SetEncodingFontFamily(string fontFamily)
        {
            try
            {
                EncodingFontFamily = new FontFamily(fontFamily);
            }
            catch (Exception)
            {
                EncodingFontFamily = SystemFonts.DefaultFont.FontFamily;
            }

            PropertyChanged = true;
        }

        private bool _quietzone;
        /// <summary>
        /// Sets the starting subset to "A" or "B", where an explicit subset is not required.
        /// Will default to "A" if not set.
        /// </summary>
        public bool Quietzone
        {
            get => _quietzone;
            set
            {
                _quietzone = value;
                PropertyChanged = true;
            }
        }

        public ImageCodecInfo ImageCodec { get; private set; } = ImageHelpers.FindCodecInfo("PNG");
        public string CodecName
        {
            get => ImageCodec.FormatDescription;
            set
            {
                ImageCodec = ImageHelpers.FindCodecInfo(value);
                PropertyChanged = true;
            }
        }

        /// <summary>
        /// Width of the generated barcode.
        /// </summary>
        public int BarcodeWidth { get; internal set; }

        /// <summary>
        /// Indicates if the X-dimension was altered to accomodate symbology constraints.
        /// </summary>
        public bool XDimensionChanged { get; internal set; }

        /// <summary>
        /// Indicates if the barcode height was altered to accomodate symbology constraints.
        /// </summary>
        public bool BarcodeHeightChanged { get; internal set; }

        /// <summary>
        /// Indicates if DPI was adjusted to fit system set constraints.
        /// </summary>
        public bool DpiChanged { get; internal set; }

        /// <summary>
        /// Indicates if the human readable font size was changed to prevent text exceeding barcode width.
        /// </summary>
        public bool HumanReadableFontSizeChanged { get; internal set; }

    }
}
