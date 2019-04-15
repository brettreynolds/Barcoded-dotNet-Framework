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

        internal abstract void Setup();

        internal abstract void Encode(string barcodeValue);

        internal abstract void SetMinBarcodeHeight();

        internal abstract void SetMinXdimension();

        internal MemoryStream GetImage(string barcodeValue)
        {
            LinearEncoding.Clear();
            Encode(BarcodeValidator.Parse(barcodeValue, Symbology));
            return LinearRenderer.DrawImageMemoryStream(this);

        }

        internal void ResetPropertyChanged()
        {
            PropertyChanged = false;
        }

        /// <summary>
        /// Barcode symbology
        /// </summary>
        public Symbology Symbology { get; set; }

        public string Description { get; internal set; }

        internal LinearEncoding LinearEncoding { get; private set; } = new LinearEncoding();

        /// <summary>
        /// Used to detect if any properties have changed
        /// </summary>
        public bool PropertyChanged { get; internal set; }

        /// <summary>
        /// Symbology specific validator
        /// </summary>
        internal abstract ILinearValidator BarcodeValidator { get; }

        private string _humanReadableValue = "";
        /// <summary>
        /// The human readable value.
        /// </summary>
        public string HumanReadableValue
        {
            get
            {
                return _humanReadableValue;
            }
            set
            {
                _humanReadableValue = value;
                PropertyChanged = true;
            }
        }

        private bool _humanReadableSymbolAligned = false;
        /// <summary>
        /// Sets the human readable label to align with associated barcode symbol
        /// </summary>
        public bool HumanReadableSymbolAligned
        {
            get
            {
                return _humanReadableSymbolAligned;
            }
            set
            {
                _humanReadableSymbolAligned = value;
                PropertyChanged = true;
            }
        }

        /// <summary>
        /// Sets the visibility and position of the human readable value.
        /// Hidden, Above, Below, Embeded
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
                case "EMBEDED":
                    HumanReadablePosition = HumanReadablePosition.Embeded;
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

        public void SetHumanReadableFont(string fontFamily, int pointSize)
        {
            if(pointSize < 1)
            {
                pointSize = (int)SystemFonts.DefaultFont.Size;
            }
            HumanReadableFont = new Font(new FontFamily(fontFamily) ?? SystemFonts.DefaultFont.FontFamily, pointSize) ?? SystemFonts.DefaultFont;
            PropertyChanged = true;
        }

        private const int _barcodeHeightMin = 1;
        private const int _barcodeHeightMax = 2400;

        private int _barcodeHeight = 1;
        /// <summary>
        /// Sets the desired height in pixels for the barcode element of the image.
        /// </summary>
        public int BarcodeHeight
        {
            get
            {
                return _barcodeHeight;
            }
            set
            {
                int barcodeHeightOriginal = value;
                _barcodeHeight = (value < _barcodeHeightMin) ? _barcodeHeightMin : (value > _barcodeHeightMax) ? _barcodeHeightMax : value;
                if(barcodeHeightOriginal != _barcodeHeight)
                {
                    BarcodeHeightChanged = true;
                }
                PropertyChanged = true;
            }
        }
        
        private int _xdimension = 1;
        /// <summary>
        /// X-dimension is the width of the narrowest bar element in the barcode.
        /// All other bar and spaces widths in the barcode are a multiple of this value.
        /// </summary>
        public int Xdimension
        {
            get
            {
                return _xdimension;
            }
            set
            {
                _xdimension = value;
                PropertyChanged = true;
            }
        }

        private const int _dpiMin = 1;
        private const int _dpiMax = 600;

        private int _dpi = 300;
        /// <summary>
        /// Sets the desired image dpi.
        /// </summary>
        public int DPI
        {
            get
            {
                return _dpi;
            }
            set
            {
                int dpiOriginal = value;
                _dpi = (value < _dpiMin) ? _dpiMin : (value > _dpiMax) ? _dpiMax : value;
                if(dpiOriginal != _dpi)
                {
                    DPIChanged = true;
                }
                PropertyChanged = true;
            }
        }

        private int _targetWidth = 0;
        /// <summary>
        /// Target pixel width for the barcode.
        /// If set, the encoder will attempt to get as close to this value without exceeding, when generating the barcode
        /// </summary>
        public int TargetWidth
        {
            get
            {
                return _targetWidth;
            }
            set
            {
                _targetWidth = value;
                PropertyChanged = true;
            }
        }

        private bool _showEncoding = false;
        /// <summary>
        /// When true, will include a human readable label of the barcode encoding values
        /// for the coresponding part of the barcode. The postition of this label will adjust
        /// top or bottom, dependent on the use of a human readable value and its position. Default is top.
        /// </summary>
        public bool ShowEncoding
        {
            get
            {
                return _showEncoding;
            }
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
            EncodingFontFamily = new FontFamily(fontFamily) ?? SystemFonts.DefaultFont.FontFamily;
            PropertyChanged = true;
        }

        private bool _quietzone = false;
        /// <summary>
        /// Sets the starting subset to "A" or "B", where an explicit subset is not required.
        /// Will default to "A" if not set.
        /// </summary>
        public bool Quietzone
        {
            get
            {
                return _quietzone;
            }
            set
            {
                _quietzone = value;
                PropertyChanged = true;
            }
        }

        public ImageCodecInfo ImageCodec { get; private set; } = ImageHelpers.FindCodecInfo("JPEG");
        public string CodecName
        {
            get
            {
                return ImageCodec.CodecName;
            }
            set
            {
                ImageCodec = ImageHelpers.FindCodecInfo(value);
            }
        }

        public int BarcodeWidth { get; internal set; }

        public bool XdimensionChanged { get; internal set; }

        public bool BarcodeHeightChanged { get; internal set; }

        public bool DPIChanged { get; internal set; }

        public bool HumanReadabaleFontSizeChanged { get; internal set; }

    }
}
