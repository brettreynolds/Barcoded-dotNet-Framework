using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Barcoded
{
    /// <summary>
    /// Linear (one-dimensional) barcode.
    /// </summary>
    public class LinearBarcode
    {
        private bool _barcodeValueChanged;
        
        /// <summary>
        /// Barcode symbology.
        /// </summary>
        public Symbology Symbology { get; }

        private string _barcodeValue;
        /// <summary>
        /// Value to encode.
        /// </summary>
        public string BarcodeValue
        {
            get => _barcodeValue;
            set
            {
                _barcodeValue = value;
                _barcodeValueChanged = true;
            }
        }

        private Image _image;
        /// <summary>
        /// Barcode image.
        /// </summary>
        public Image Image
        {
            get
            {
                UpdateBarcode();
                return _image;
            }
        }

        private LinearVectors _vectors;
        /// <summary>
        /// Barcode vectors.
        /// </summary>
        public LinearVectors Vectors
        {
            get
            {
                UpdateBarcode();
                return _vectors;
            }
        }

        /// <summary>
        /// Minimum point width for the given encoding value.
        /// </summary>
        public int MinimumPointWidth
        {
            get
            {
                Encoder.Encode(BarcodeValue);
                return Encoder.LinearEncoding.MinimumWidth;
            }
        }

        public string ZplEncode
        {
            get
            {
                Encoder.Encode(BarcodeValue);
                return Encoder.ZplEncode;
            }
        }

        /// <summary>
        /// Barcode symbology encoder.
        /// </summary>
        public LinearEncoder Encoder { get; }

        /// <summary>
        /// Creates a barcode image from the declared value and desired symbology.
        /// </summary>
        /// <param name="barcodeValue">Barcode value string.</param>
        /// <param name="symbology">Barcode symbology string.</param>
        public LinearBarcode(string barcodeValue, string symbology) : this(barcodeValue, GetSymbology(symbology))
        {
        }

        /// <summary>
        /// Creates a barcode image from the declared value and desired symbology.
        /// </summary>
        /// <param name="barcodeValue">Barcode value string.</param>
        /// <param name="symbology">Barcode symbology</param>
        public LinearBarcode(string barcodeValue, Symbology symbology)
        {
            BarcodeValue = barcodeValue;
            Symbology = symbology;

            switch (symbology)
            {
                case Symbology.Code128ABC:
                    Encoder = new Code128Encoder(symbology);
                    break;
                case Symbology.Code128BAC:
                    Encoder = new Code128Encoder(symbology);
                    break;
                case Symbology.Code128AB:
                    Encoder = new Code128Encoder(symbology);
                    break;
                case Symbology.Code128BA:
                    Encoder = new Code128Encoder(symbology);
                    break;
                case Symbology.GS1128:
                    Encoder = new Code128Encoder(symbology);
                    break;
                case Symbology.Code39:
                    Encoder = new Code39Encoder(symbology);
                    break;
                case Symbology.Code39C:
                    Encoder = new Code39Encoder(symbology);
                    break;
                case Symbology.Code39Full:
                    Encoder = new Code39Encoder(symbology);
                    break;
                case Symbology.Code39FullC:
                    Encoder = new Code39Encoder(symbology);
                    break;
                case Symbology.I2of5:
                    Encoder = new Interleaved2Of5Encoder(symbology);
                    break;
                case Symbology.I2of5C:
                    Encoder = new Interleaved2Of5Encoder(symbology);
                    break;
                case Symbology.Ean13:
                    Encoder = new Ean138Encoder(symbology);
                    break;
                case Symbology.UpcA:
                    Encoder = new Ean138Encoder(symbology);
                    break;
                case Symbology.Ean8:
                    Encoder = new Ean138Encoder(symbology);
                    break;
                default:
                    Encoder = new Code128Encoder(Symbology.Code128BAC);
                    break;
            }
        }

        /// <summary>
        /// Get a list of available barcode symbologies.
        /// </summary>
        /// <returns>Returns barcode symbology text list.</returns>
        public static List<string> GetSymbologies()
        {
            List<string> symbologies = Enum.GetValues(typeof(Symbology))
                .Cast<Symbology>()
                .Select(v => v.ToString())
                .ToList();

            return symbologies;
        }

        /// <summary>
        /// Get a list of available human readable text positions.
        /// </summary>
        /// <returns>Returns human readable text list.</returns>
        public static List<string> GetHumanReadablePositions()
        {
            List<string> humanReadablePositions = Enum.GetValues(typeof(HumanReadablePosition))
                .Cast<HumanReadablePosition>()
                .Select(v => v.ToString())
                .ToList();

            return humanReadablePositions;
        }

        /// <summary>
        /// Returns the symbology from the given name.
        /// </summary>
        /// <param name="symbology">Symbology name.</param>
        /// <returns>Symbology.</returns>
        private static Symbology GetSymbology(string symbology)
        {
            switch (symbology.ToUpper())
            {
                case "CODE128ABC":
                    return Symbology.Code128ABC;
                case "CODE128BAC":
                    return Symbology.Code128BAC;
                case "CODE128AB":
                    return Symbology.Code128AB;
                case "CODE128BA":
                    return Symbology.Code128BA;
                case "GS1128":
                    return Symbology.GS1128;
                case "CODE39":
                    return Symbology.Code39;
                case "CODE39C":
                    return Symbology.Code39C;
                case "CODE39FULL":
                    return Symbology.Code39Full;
                case "CODE39FULLC":
                    return Symbology.Code39FullC;
                case "I2OF5":
                    return Symbology.I2of5;
                case "I2OF5C":
                    return Symbology.I2of5C;
                case "EAN13":
                    return Symbology.Ean13;
                case "UPCA":
                    return Symbology.UpcA;
                case "EAN8":
                    return Symbology.Ean8;
                default:
                    return Symbology.Code128BAC;
            }
        }

        /// <summary>
        /// Provides a byte array version of the barcode image in the declared format for saving to file.
        /// </summary>
        /// <param name="codec">Codec for the image format to be saved.</param>
        /// <returns>Byte array of the barcode image.</returns>
        public byte[] SaveImage(string codec)
        {
            Encoder.CodecName = codec;
            
            MemoryStream imageMemoryStream = Encoder.GetImage(BarcodeValue);
            _image = Image.FromStream(imageMemoryStream);
            _vectors = new LinearVectors(Encoder);
            _barcodeValueChanged = false;
            return imageMemoryStream.ToArray();

        }

        /// <summary>
        /// Checks if any barcode settings have changed since the last call and creates a new barcode if they have.
        /// </summary>
        private void UpdateBarcode()
        {
            //Check that barcode value is not an empty string.
            if (string.IsNullOrEmpty(BarcodeValue))
            {
                BarcodeValue = "EMPTY";
                Encoder.SetHumanReadablePosition("Below");
                Encoder.HumanReadableValue = "EMPTY";
            }

            if (_barcodeValueChanged | Encoder.PropertyChanged)
            {
                MemoryStream imageMemoryStream = Encoder.GetImage(BarcodeValue);
                _image = Image.FromStream(imageMemoryStream);
                _vectors = new LinearVectors(Encoder);
                _barcodeValueChanged = false;
            }
        }
    }
}
