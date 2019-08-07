using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Barcoded
{
    internal static class LinearRenderer
    {
        private const int MaximumPixelWidth = 12000; // 20" at maximum DPI of 600
        
        /// <summary>
        /// Holds the x & y position of the ImageElement.
        /// </summary>
        internal class Position
        {
            internal int XPosition { get; set; }
            internal int YPosition { get; set; }

            internal Position(int xPosition = 0, int yPosition = 0)
            {
                XPosition = xPosition;
                YPosition = yPosition;
            }
        }

        /// <summary>
        /// Holds the width and height of the ImageElement.
        /// </summary>
        internal class Size
        {
            internal int Width { get; set; }
            internal int Height { get; set; }

            internal Size(int width = 0, int height = 0)
            {
                Width = width;
                Height = height;
            }
        }

        /// <summary>
        /// Holds the element image, dimensions and position.
        /// </summary>
        internal class ImageElement
        {
            internal Bitmap Image { get; set; }
            internal Position Position { get; set; }
            internal Size Size { get; set; }

            internal ImageElement()
            {
                Image = new Bitmap(1, 1);
                Position = new Position();
                Size = new Size();
            }

            internal ImageElement(Bitmap image)
            {
                Image = image;
                Position = new Position();
                Size = new Size(image.Width, image.Height);
            }

            internal void UpdateImage(Bitmap image)
            {
                Image = image;
                Size.Width = image.Width;
                Size.Height = image.Height;
            }
        }

        private static void Draw(ref MemoryStream memoryStream, LinearEncoder linearEncoder)
        {
            int xDimensionOriginal = linearEncoder.XDimension;

            // Adjust x-dimension to match target width (if set)
            linearEncoder.XDimension = Math.Max(linearEncoder.XDimension, GetXDimensionForTargetWidth(linearEncoder.TargetWidth, linearEncoder.LinearEncoding.MinimumWidth));

            // Ensure the x-dimension selected doesn't result in a width that exceeds maximum allowable.
            linearEncoder.XDimension = Math.Min(linearEncoder.XDimension, GetXDimensionForTargetWidth(MaximumPixelWidth, linearEncoder.LinearEncoding.MinimumWidth));

            switch (linearEncoder.Symbology)
            {
                case Symbology.Ean13:
                    linearEncoder.Quietzone = true;
                    linearEncoder.SetHumanReadablePosition("Embedded");
                    linearEncoder.HumanReadableSymbolAligned = true;
                    break;

                case Symbology.UpcA:
                    linearEncoder.Quietzone = true;
                    linearEncoder.SetHumanReadablePosition("Embedded");
                    linearEncoder.HumanReadableSymbolAligned = true;
                    break;

                case Symbology.Ean8:
                    linearEncoder.Quietzone = true;
                    linearEncoder.SetHumanReadablePosition("Embedded");
                    linearEncoder.HumanReadableSymbolAligned = true;
                    break;
            }

            int quietzone = 0;
            if (linearEncoder.Quietzone)
            {
                quietzone = Math.Max(20 * linearEncoder.XDimension, linearEncoder.Dpi / 4) / 2;
            }

            // Create each of the image elements.
            ImageElement encodingTextImage = GetEncodingImage(linearEncoder, quietzone);
            ImageElement barcodeImage = GetBarcodeImage(linearEncoder, quietzone);

            linearEncoder.BarcodeWidth = barcodeImage.Size.Width;

            ImageElement humanReadableImage = linearEncoder.HumanReadableSymbolAligned ? GetHumanReadableImageSymbolAligned(linearEncoder, quietzone) : GetHumanReadableImageCentered(linearEncoder, quietzone);

            // Adjust each element position, dependent on label text visibility and position.
            switch (linearEncoder.HumanReadablePosition)
            {
                case HumanReadablePosition.Above:     //Label above the barcode
                    barcodeImage.Position.YPosition += humanReadableImage.Size.Height;
                    encodingTextImage.Position.YPosition += humanReadableImage.Size.Height + barcodeImage.Size.Height;
                    break;
                case HumanReadablePosition.Embedded:     //Embedded the barcode
                    barcodeImage.Position.YPosition += encodingTextImage.Size.Height;
                    humanReadableImage.Position.YPosition += encodingTextImage.Size.Height + barcodeImage.Size.Height - (humanReadableImage.Size.Height / 2);
                    break;
                default:    //Label below the barcode
                    barcodeImage.Position.YPosition += encodingTextImage.Size.Height;
                    humanReadableImage.Position.YPosition += encodingTextImage.Size.Height + barcodeImage.Size.Height;
                    break;
            }

            // Set the required image height by adding the barcode height to the encoding text or value label text heights (if visible)
            int imageHeight = barcodeImage.Size.Height + encodingTextImage.Size.Height + humanReadableImage.Size.Height;

            // Reduce the image height if human readable position is embedded
            if (linearEncoder.HumanReadablePosition == HumanReadablePosition.Embedded)
            {
                imageHeight -= (humanReadableImage.Size.Height / 2);
            }

            // Set the required image width by taking the greater of the barcode width and value label text width (if used).
            int imageWidth = Math.Max(barcodeImage.Size.Width + (quietzone * 2), humanReadableImage.Size.Width);

            // Create the combined image.
            Bitmap combinedImage = new Bitmap(imageWidth, imageHeight);
            combinedImage.SetResolution(linearEncoder.Dpi, linearEncoder.Dpi);
            Graphics combinedGraphics = Graphics.FromImage(combinedImage);

            // Add each element to the combined image.
            combinedGraphics.FillRectangle(Brushes.White, 0, 0, combinedImage.Width, combinedImage.Height);
            combinedGraphics.DrawImageUnscaled(barcodeImage.Image, barcodeImage.Position.XPosition, barcodeImage.Position.YPosition);
            combinedGraphics.DrawImageUnscaled(encodingTextImage.Image, encodingTextImage.Position.XPosition, encodingTextImage.Position.YPosition);
            combinedGraphics.DrawImageUnscaled(humanReadableImage.Image, humanReadableImage.Position.XPosition, humanReadableImage.Position.YPosition);

            // Save the image to the memory stream.
            EncoderParameters encodingParams = new EncoderParameters(1)
            {
                Param = {[0] = new EncoderParameter(Encoder.Quality, 100L)}
            };
            combinedImage.Save(memoryStream, linearEncoder.ImageCodec, encodingParams);

            // Set flag if xdimension was changed.
            if(linearEncoder.XDimension != xDimensionOriginal)
            {
                linearEncoder.XDimensionChanged = true;
            }

            // Dispose of the objects we won't need any more.
            barcodeImage.Image.Dispose();
            humanReadableImage.Image.Dispose();
            encodingTextImage.Image.Dispose();
            combinedGraphics.Dispose();
            combinedImage.Dispose();

            linearEncoder.ResetPropertyChanged();
        }

        internal static MemoryStream DrawImageMemoryStream(LinearEncoder linearEncoder)
        {
            MemoryStream memoryStream = new MemoryStream();
            Draw(ref memoryStream, linearEncoder);
            return memoryStream;
        }

        /// <summary>
        /// Creates the label value image element.
        /// </summary>
        /// <param name="linearEncoder"></param>
        /// <param name="quietzone"></param>
        /// <returns>The generated label value image inside an ImageElement object.</returns>
        internal static ImageElement GetHumanReadableImageCentered(LinearEncoder linearEncoder, int quietzone)
        {
            // Create an empty ImageElement
            ImageElement humanReadableElement = new ImageElement();

            // If the human readable position is set to hidden, return the empty ImageElement
            if (linearEncoder.HumanReadablePosition == HumanReadablePosition.Hidden)
            {
                return humanReadableElement;
            }

            // If the human readable is set to a visible position, but has no value, take the barcode value
            if (string.IsNullOrWhiteSpace(linearEncoder.HumanReadableValue))
            {
                linearEncoder.HumanReadableValue = linearEncoder.EncodedValue;
            }

            // Calculate the barcode image width from the minimum width multiplied by the x-dimension
            int barcodeWidth = linearEncoder.LinearEncoding.MinimumWidth * linearEncoder.XDimension;

            // Get the original human readable font size, so we can compare with the size after adjusting to fir barcode width
            int humanReadableFontSizeOriginal = (int)linearEncoder.HumanReadableFont.Size;

            // Adjust the human readable font size so that the value does not exceed the width of the barcode image
            linearEncoder.HumanReadableFont = ImageHelpers.GetSizedFontForWidth(linearEncoder.HumanReadableValue, barcodeWidth, linearEncoder.Dpi, linearEncoder.HumanReadableFont);

            // Set the human readable font size changed flag, if size different from original
            if(humanReadableFontSizeOriginal != (int)linearEncoder.HumanReadableFont.Size)
            {
                linearEncoder.HumanReadableFontSizeChanged = true;
            }

            // Measure the value label text size, based on the font provided
            SizeF labelTextSize = ImageHelpers.GetStringElementSize(linearEncoder.HumanReadableValue, linearEncoder.HumanReadableFont, linearEncoder.Dpi);

            // Create a new bitmap image for the label value text based on the calculated dimensions
            humanReadableElement.UpdateImage(new Bitmap((int)Math.Ceiling(labelTextSize.Width), (int)Math.Ceiling(labelTextSize.Height)));
            humanReadableElement.Image.SetResolution(linearEncoder.Dpi, linearEncoder.Dpi);

            // Create a new graphics to draw on the barcode image
            Graphics labelValueGraphics = Graphics.FromImage(humanReadableElement.Image);

            labelValueGraphics.FillRectangle(Brushes.White, 1, 1, humanReadableElement.Image.Width, humanReadableElement.Image.Height);
            labelValueGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            labelValueGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            labelValueGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            labelValueGraphics.DrawString(linearEncoder.HumanReadableValue, linearEncoder.HumanReadableFont, Brushes.Black, 1, 1);
            labelValueGraphics.Flush();
            humanReadableElement.Position.XPosition = quietzone + (barcodeWidth - (int)labelTextSize.Width) / 2;

            return humanReadableElement;
        }

        /// <summary>
        /// Creates the label value image element.
        /// </summary>
        /// <param name="linearEncoder"></param>
        /// <param name="quietzone"></param>
        /// <returns>The generated label value image inside an ImageElement object.</returns>
        internal static ImageElement GetHumanReadableImageSymbolAligned(LinearEncoder linearEncoder, int quietzone)
        {
            // Create an empty ImageElement
            ImageElement humanReadableElement = new ImageElement();

            // If the human readable position is set to hidden, return the empty ImageElement
            if (linearEncoder.HumanReadablePosition == HumanReadablePosition.Hidden)
            {
                return humanReadableElement;
            }

            // Setup the label font with initial value "A" to assist calculating label element height later
            Font humanReadableFont = ImageHelpers.GetSizedFontForWidth(1, linearEncoder.LinearEncoding.GetWidestSymbol() * linearEncoder.XDimension, linearEncoder.Dpi, linearEncoder.HumanReadableFont);

            // Set the text size so that we can get the encoding element height later
            SizeF humanReadableSize = ImageHelpers.GetStringElementSize("W", humanReadableFont, linearEncoder.Dpi);

            int prefixWidth = linearEncoder.LinearEncoding.HumanReadablePrefix?.Length * (int)humanReadableSize.Width ?? 0;
            int suffixWidth = linearEncoder.LinearEncoding.HumanReadableSuffix?.Length * (int)humanReadableSize.Width ?? 0;

            // Set the encoding image width from the minimum width multiplied by the x-dimension
            int humanReadableImageWidth = linearEncoder.LinearEncoding.MinimumWidth * linearEncoder.XDimension + prefixWidth + suffixWidth;

            //Create a new bitmap image for the encoding text based on the calculated dimensions
            humanReadableElement.UpdateImage(new Bitmap(humanReadableImageWidth, (int)Math.Ceiling(humanReadableSize.Height)));
            humanReadableElement.Image.SetResolution(linearEncoder.Dpi, linearEncoder.Dpi);

            // Create a new graphics to draw on the encoded text image
            Graphics humanReadableGraphics = Graphics.FromImage(humanReadableElement.Image);

            int xPosition = 0;
            int yPosition = 0;

            StringFormat humanReadableFormat = new StringFormat
            {
                Alignment = StringAlignment.Center
            };

            RectangleF humanReadableRectangle;

            // Add any human readable prefix
            if (linearEncoder.LinearEncoding.HumanReadablePrefix != null)
            {
                humanReadableRectangle = new RectangleF(xPosition, yPosition, prefixWidth, humanReadableElement.Image.Height);
                humanReadableGraphics.FillRectangle(Brushes.White, humanReadableRectangle);
                humanReadableGraphics.DrawString(linearEncoder.LinearEncoding.HumanReadablePrefix, humanReadableFont, Brushes.Black, humanReadableRectangle, humanReadableFormat);
                xPosition += prefixWidth;
            }

            for (int symbol = 0; symbol <= linearEncoder.LinearEncoding.Symbols.Count - 1; symbol++)
            {
                string humanReadableCharacter = linearEncoder.LinearEncoding.Symbols[symbol].Character;
                Font humanReadableCharacterFont = ImageHelpers.GetSizedFontForWidth(humanReadableCharacter.Length, linearEncoder.LinearEncoding.Symbols[symbol].Width * linearEncoder.XDimension, linearEncoder.Dpi, linearEncoder.HumanReadableFont);

                int symbolWidth = linearEncoder.LinearEncoding.Symbols[symbol].Width;

                if (linearEncoder.LinearEncoding.Symbols[symbol].CharacterType == 0)
                {
                    humanReadableRectangle = new RectangleF(xPosition, yPosition, symbolWidth * linearEncoder.XDimension, humanReadableElement.Image.Height);
                    humanReadableGraphics.FillRectangle(Brushes.White, humanReadableRectangle);
                    humanReadableGraphics.DrawString(humanReadableCharacter, humanReadableCharacterFont, Brushes.Black, humanReadableRectangle, humanReadableFormat);
                }

                xPosition += symbolWidth * linearEncoder.XDimension;
            }

            // Add any human readable suffix
            if (linearEncoder.LinearEncoding.HumanReadableSuffix != null)
            {
                humanReadableRectangle = new RectangleF(xPosition, yPosition, suffixWidth, humanReadableElement.Image.Height);
                humanReadableGraphics.FillRectangle(Brushes.White, humanReadableRectangle);
                humanReadableGraphics.DrawString(linearEncoder.LinearEncoding.HumanReadableSuffix, humanReadableFont, Brushes.Black, humanReadableRectangle, humanReadableFormat);
            }

            humanReadableFont.Dispose();
            humanReadableGraphics.Dispose();
            humanReadableElement.Position.XPosition = quietzone - prefixWidth;

            return humanReadableElement;
        }

        /// <summary>
        /// Creates the barcode image element.
        /// </summary>
        /// <param name="linearEncoder"></param>
        /// <param name="quietzone"></param>
        /// <returns>The generated barcode image inside an ImageElement object.</returns>
        internal static ImageElement GetBarcodeImage(LinearEncoder linearEncoder, int quietzone)
        {

            // Set the encoding image width from the minimum width multiplied by the x-dimension
            int barcodeImageWidth = linearEncoder.LinearEncoding.MinimumWidth * linearEncoder.XDimension;

            // Create a new bitmap image for the barcode based on the calculated dimensions
            ImageElement barcodeElement = new ImageElement(new Bitmap(barcodeImageWidth, linearEncoder.BarcodeHeight));
            barcodeElement.Image.SetResolution(linearEncoder.Dpi, linearEncoder.Dpi);

            //Create a new graphics to draw on the barcode image
            Graphics barcodeGraphics = Graphics.FromImage(barcodeElement.Image);

            int xPosition = 0;
            int yPosition = 0;

            barcodeGraphics.FillRectangle(Brushes.White, xPosition, yPosition, barcodeElement.Image.Width, barcodeElement.Image.Height);

            // Loop through each encoded symbol and convert to bars based on selected symbology
            for (int symbol = 0; symbol <= linearEncoder.LinearEncoding.Symbols.Count - 1; symbol++)
            {
                LinearPattern symbolPattern = linearEncoder.LinearEncoding.Symbols[symbol].Pattern;

                // Build the barcode symbol and insert
                for (int module = 0; module <= symbolPattern.Count - 1; module++)
                {
                    switch (symbolPattern[module].ModuleType)
                    {
                        case ModuleType.Bar: // Bar
                            int barWidth = symbolPattern[module].Width * linearEncoder.XDimension;
                            barcodeGraphics.FillRectangle(Brushes.Black, xPosition, yPosition, barWidth, linearEncoder.BarcodeHeight);
                            xPosition += barWidth;
                            barcodeGraphics.Flush();
                            break;
                        case ModuleType.Space: // Space
                            int spaceWidth = symbolPattern[module].Width * linearEncoder.XDimension;
                            xPosition += spaceWidth;
                            break;
                    }
                }
            }

            barcodeElement.Position.XPosition = quietzone;
            return barcodeElement;
        }

        /// <summary>
        /// Creates the encoding text image element.
        /// </summary>
        /// <param name="linearEncoder"></param>
        /// <param name="quietzone"></param>
        /// <returns>The generated encoding text image inside an ImageElement object.</returns>
        internal static ImageElement GetEncodingImage(LinearEncoder linearEncoder, int quietzone)
        {
            // Create an empty ImageElement
            ImageElement encodingTextElement = new ImageElement();

            // Return the empty ImageElement if encoding not visible
            if (linearEncoder.ShowEncoding == false)
            {
                return encodingTextElement;
            }

            // Setup the font for encoding with initial value "A" to assist calculating encoding element height later.
            Font encodeCharFont = ImageHelpers.GetSizedFontForWidth(1, linearEncoder.LinearEncoding.GetWidestSymbol() * linearEncoder.XDimension, linearEncoder.Dpi, new Font(linearEncoder.EncodingFontFamily, 8), false);

            // Set the text size so that we can get the encoding element height later.
            SizeF encodingTextSize = ImageHelpers.GetStringElementSize("A", encodeCharFont, linearEncoder.Dpi);

            // Set the encoding image width from the minimum width multiplied by the x-dimension.
            int encodingImageWidth = linearEncoder.LinearEncoding.MinimumWidth * linearEncoder.XDimension;

            //Create a new bitmap image for the encoding text based on the calculated dimensions.
            encodingTextElement.UpdateImage(new Bitmap(encodingImageWidth, (int)Math.Ceiling(encodingTextSize.Height)));
            encodingTextElement.Image.SetResolution(linearEncoder.Dpi, linearEncoder.Dpi);

            // Create a new graphics to draw on the encoded text image.
            Graphics encodingTextGraphics = Graphics.FromImage(encodingTextElement.Image);
            encodingTextGraphics.FillRectangle(Brushes.White, 0, 0, encodingImageWidth, (int)Math.Ceiling(encodingTextSize.Height));

            int xPosition = 0;
            int yPosition = 0;

            Pen encodePen = new Pen(Brushes.Black, 1);

            for (int symbol = 0; symbol <= linearEncoder.LinearEncoding.Symbols.Count - 1; symbol++)
            {
                string encodeCharacter = linearEncoder.LinearEncoding.Symbols[symbol].Character;
                encodeCharFont = ImageHelpers.GetSizedFontForWidth(encodeCharacter.Length, linearEncoder.LinearEncoding.Symbols[symbol].Width * linearEncoder.XDimension, linearEncoder.Dpi, new Font(linearEncoder.EncodingFontFamily, 8), false);

                int symbolWidth = linearEncoder.LinearEncoding.Symbols[symbol].Width;
                //string SymbolPattern = coded.SymbolPattern[symbol];

                Brush encodeBrush;
                if (linearEncoder.LinearEncoding.Symbols[symbol].CharacterType == 1)
                {
                    encodingTextGraphics.FillRectangle(Brushes.Black, xPosition, yPosition, symbolWidth * linearEncoder.XDimension, encodingTextElement.Image.Height);
                    encodeBrush = Brushes.White;
                }
                else
                {
                    encodingTextGraphics.DrawRectangle(encodePen, xPosition, yPosition, (symbolWidth * linearEncoder.XDimension) - 1, encodingTextElement.Image.Height - 1);
                    encodeBrush = Brushes.Black;
                }

                encodingTextGraphics.DrawString(encodeCharacter, encodeCharFont, encodeBrush, xPosition, yPosition);

                xPosition += symbolWidth * linearEncoder.XDimension;
            }

            encodeCharFont.Dispose();
            encodingTextGraphics.Dispose();
            encodePen.Dispose();
            encodingTextElement.Position.XPosition = quietzone;

            return encodingTextElement;
        }

        /// <summary>
        /// Calculates the maximum x-dimension of a barcode for a given width.
        /// </summary>
        /// <param name="targetWidth">The target pixel width.</param>
        /// <param name="minimumWidth">The minimum barcode pixel width.</param>
        /// <returns>Maximum achievable x-dimension.</returns>
        internal static int GetXDimensionForTargetWidth(int targetWidth, int minimumWidth)
        {
            int xDimension = 1;
            if (targetWidth > 0)
            {
                while (!(minimumWidth * (xDimension + 1) > targetWidth))
                {
                    xDimension += 1;
                }
            }
            return xDimension;
        }
    }
}
