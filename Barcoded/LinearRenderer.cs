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

            // Create each of the image elements.
            ImageElement encodingTextImage = GetEncodingImage(linearEncoder);
            ImageElement barcodeImage = GetBarcodeImage(linearEncoder);

            linearEncoder.BarcodeWidth = barcodeImage.Size.Width;

            ImageElement labelTextImage = linearEncoder.HumanReadableSymbolAligned ? GetLabelValueImageSymbolAligned(linearEncoder) : GetLabelValueImageCentered(linearEncoder);

            // Adjust each element position, dependent on label text visibility and position.
            switch (linearEncoder.HumanReadablePosition)
            {
                case HumanReadablePosition.Above:     //Label above the barcode
                    barcodeImage.Position.YPosition += labelTextImage.Size.Height;
                    encodingTextImage.Position.YPosition += labelTextImage.Size.Height + barcodeImage.Size.Height;
                    break;
                case HumanReadablePosition.Embedded:     //Embedded the barcode
                    barcodeImage.Position.YPosition += encodingTextImage.Size.Height;
                    labelTextImage.Position.YPosition += encodingTextImage.Size.Height + barcodeImage.Size.Height - (labelTextImage.Size.Height / 2);
                    break;
                default:    //Label below the barcode
                    barcodeImage.Position.YPosition += encodingTextImage.Size.Height;
                    labelTextImage.Position.YPosition += encodingTextImage.Size.Height + barcodeImage.Size.Height;
                    break;
            }

            // Set the required image height by adding the barcode height to the encoding text or value label text heights (if visible)
            int imageHeight = barcodeImage.Size.Height + encodingTextImage.Size.Height + labelTextImage.Size.Height;

            // Reduce the image height if human readable position is embedded
            if (linearEncoder.HumanReadablePosition == HumanReadablePosition.Embedded)
            {
                imageHeight -= (labelTextImage.Size.Height / 2);
            }

            int quietzone = 0;
            if (linearEncoder.Quietzone)
            {
                quietzone = Math.Max(20 * linearEncoder.XDimension, linearEncoder.Dpi / 4);
            }

            // Set the required image width by taking the greater of the barcode width and value label text width (if used).
            int imageWidth = Math.Max(barcodeImage.Size.Width + quietzone, labelTextImage.Size.Width);

            // Align all elements to center of image.
            labelTextImage.Position.XPosition = (imageWidth - labelTextImage.Size.Width) / 2;
            barcodeImage.Position.XPosition = (imageWidth - barcodeImage.Size.Width) / 2;
            encodingTextImage.Position.XPosition = (imageWidth - barcodeImage.Size.Width) / 2;

            // Create the combined image.
            Bitmap combinedImage = new Bitmap(imageWidth, imageHeight);
            combinedImage.SetResolution(linearEncoder.Dpi, linearEncoder.Dpi);
            Graphics combinedGraphics = Graphics.FromImage(combinedImage);

            // Add each element to the combined image.
            combinedGraphics.FillRectangle(Brushes.White, 0, 0, combinedImage.Width, combinedImage.Height);
            combinedGraphics.DrawImageUnscaled(barcodeImage.Image, barcodeImage.Position.XPosition, barcodeImage.Position.YPosition);
            combinedGraphics.DrawImageUnscaled(encodingTextImage.Image, encodingTextImage.Position.XPosition, encodingTextImage.Position.YPosition);
            combinedGraphics.DrawImageUnscaled(labelTextImage.Image, labelTextImage.Position.XPosition, labelTextImage.Position.YPosition);

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
            labelTextImage.Image.Dispose();
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
        /// <returns>The generated label value image inside an ImageElement object.</returns>
        internal static ImageElement GetLabelValueImageCentered(LinearEncoder linearEncoder)
        {

            // Create an empty ImageElement
            ImageElement labelValueElement = new ImageElement();

            // If the label value is not visible or the label value text is not supplied, return the empty ImageElement
            if (linearEncoder.HumanReadablePosition == HumanReadablePosition.Hidden | string.IsNullOrWhiteSpace(linearEncoder.HumanReadableValue))
            {
                return labelValueElement;
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
            labelValueElement.UpdateImage(new Bitmap((int)Math.Ceiling(labelTextSize.Width), (int)Math.Ceiling(labelTextSize.Height)));
            labelValueElement.Image.SetResolution(linearEncoder.Dpi, linearEncoder.Dpi);

            // Create a new graphics to draw on the barcode image
            Graphics labelValueGraphics = Graphics.FromImage(labelValueElement.Image);

            labelValueGraphics.FillRectangle(Brushes.White, 1, 1, labelValueElement.Image.Width, labelValueElement.Image.Height);
            labelValueGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            labelValueGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            labelValueGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            labelValueGraphics.DrawString(linearEncoder.HumanReadableValue, linearEncoder.HumanReadableFont, Brushes.Black, 1, 1);
            labelValueGraphics.Flush();

            return labelValueElement;
        }

        /// <summary>
        /// Creates the label value image element.
        /// </summary>
        /// <param name="linearEncoder"></param>
        /// <returns>The generated label value image inside an ImageElement object.</returns>
        internal static ImageElement GetLabelValueImageSymbolAligned(LinearEncoder linearEncoder)
        {
            // Create an empty ImageElement
            ImageElement labelValueElement = new ImageElement();

            // If the label value is not visible or the label value text is not supplied, return the empty ImageElement
            if (linearEncoder.HumanReadablePosition == HumanReadablePosition.Hidden | string.IsNullOrWhiteSpace(linearEncoder.HumanReadableValue))
            {
                return labelValueElement;
            }

            // Setup the label font with initial value "A" to assist calculating label element height later
            Font labelValueFont = ImageHelpers.GetSizedFontForWidth(1, linearEncoder.LinearEncoding.GetWidestSymbol() * linearEncoder.XDimension, linearEncoder.Dpi, linearEncoder.HumanReadableFont);

            // Set the text size so that we can get the encoding element height later
            SizeF labelValueSize = ImageHelpers.GetStringElementSize("A", labelValueFont, linearEncoder.Dpi);

            // Set the encoding image width from the minimum width multiplied by the x-dimension
            int labelImageWidth = linearEncoder.LinearEncoding.MinimumWidth * linearEncoder.XDimension;

            //Create a new bitmap image for the encoding text based on the calculated dimensions
            labelValueElement.UpdateImage(new Bitmap(labelImageWidth, (int)Math.Ceiling(labelValueSize.Height)));
            labelValueElement.Image.SetResolution(linearEncoder.Dpi, linearEncoder.Dpi);

            // Create a new graphics to draw on the encoded text image
            Graphics labelValueGraphics = Graphics.FromImage(labelValueElement.Image);
            //LabelValueGraphics.FillRectangle(Brushes.White, 0, 0, LabelImageWidth, (int)LabelValueSize.Height);

            int xPosition = 0;
            int yPosition = 0;

            for (int symbol = 0; symbol <= linearEncoder.LinearEncoding.Symbols.Count - 1; symbol++)
            {
                string labelCharacter = linearEncoder.LinearEncoding.Symbols[symbol].Character;
                labelValueFont = ImageHelpers.GetSizedFontForWidth(labelCharacter.Length, linearEncoder.LinearEncoding.Symbols[symbol].Width * linearEncoder.XDimension, linearEncoder.Dpi, linearEncoder.HumanReadableFont);

                int symbolWidth = linearEncoder.LinearEncoding.Symbols[symbol].Width;

                if (linearEncoder.LinearEncoding.Symbols[symbol].CharacterType != 1)
                {
                    labelValueGraphics.FillRectangle(Brushes.White, xPosition, yPosition, symbolWidth * linearEncoder.XDimension, labelValueElement.Image.Height);
                    labelValueGraphics.DrawString(labelCharacter, labelValueFont, Brushes.Black, xPosition, yPosition);
                }

                xPosition += symbolWidth * linearEncoder.XDimension;
            }

            labelValueFont.Dispose();
            labelValueGraphics.Dispose();

            return labelValueElement;
        }
        
        /// <summary>
        /// Creates the barcode image element.
        /// </summary>
        /// <param name="linearEncoder"></param>
        /// <returns>The generated barcode image inside an ImageElement object.</returns>
        internal static ImageElement GetBarcodeImage(LinearEncoder linearEncoder)
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

            return barcodeElement;
        }

        /// <summary>
        /// Creates the encoding text image element.
        /// </summary>
        /// <param name="linearEncoder"></param>
        /// <returns>The generated encoding text image inside an ImageElement object.</returns>
        internal static ImageElement GetEncodingImage(LinearEncoder linearEncoder)
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
