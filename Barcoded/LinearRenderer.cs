using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Barcoded
{
    internal static class LinearRenderer
    {
        private const int _maximumPixelWidth = 12000; // 20" at maximum DPI of 600

        internal class Position
        {
            public int XPosition { get; set; }
            public int YPosition { get; set; }

            public Position(int xPosition = 0, int yPosition = 0)
            {
                XPosition = xPosition;
                YPosition = yPosition;
            }
        }

        internal class Size
        {
            public int Width { get; set; }
            public int Height { get; set; }

            public Size(int width = 0, int height = 0)
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
            public Bitmap Image { get; set; }
            public Position Position { get; set; }
            public Size Size { get; set; }

            public ImageElement()
            {
                Image = new Bitmap(1, 1);
                Position = new Position();
                Size = new Size();
            }

            public ImageElement(Bitmap image)
            {
                Image = image;
                Position = new Position();
                Size = new Size(image.Width, image.Height);
            }

            public void UpdateImage(Bitmap image)
            {
                Image = image;
                Size.Width = image.Width;
                Size.Height = image.Height;
            }
        }

        private static void Draw(ref MemoryStream memoryStream, LinearEncoder linearEncoder)
        {
            int XdimensionOriginal = linearEncoder.Xdimension;

            // Adjust x-dimension to match target width (if set)
            linearEncoder.Xdimension = Math.Max(linearEncoder.Xdimension, GetXDimensionForTargetWidth(linearEncoder.TargetWidth, linearEncoder.LinearEncoding.MinimumWidth));

            // Ensure the x-dimension selected doesn't result in a width that exceeds maximum allowable
            linearEncoder.Xdimension = Math.Min(linearEncoder.Xdimension, GetXDimensionForTargetWidth(_maximumPixelWidth, linearEncoder.LinearEncoding.MinimumWidth));

            // Create each of the image elements
            ImageElement encodingTextImage = GetEncodingImage(linearEncoder);
            ImageElement barcodeImage = GetBarcodeImage(linearEncoder);

            linearEncoder.BarcodeWidth = barcodeImage.Size.Width;

            ImageElement labelTextImage;
            if (linearEncoder.HumanReadableSymbolAligned == true)
            {
                labelTextImage = GetLabelValueImageSymbolAligned(linearEncoder);
            }
            else
            {
                labelTextImage = GetLabelValueImageCentered(linearEncoder);
            }

            // Adjust each element position, dependent on label text visibility and position
            switch (linearEncoder.HumanReadablePosition)
            {
                case HumanReadablePosition.Above:     //Label above the barcode
                    barcodeImage.Position.YPosition += labelTextImage.Size.Height;
                    encodingTextImage.Position.YPosition += labelTextImage.Size.Height + barcodeImage.Size.Height;
                    break;
                case HumanReadablePosition.Embeded:     //Embeded the barcode
                    barcodeImage.Position.YPosition += (int)encodingTextImage.Size.Height;
                    labelTextImage.Position.YPosition += (int)encodingTextImage.Size.Height + barcodeImage.Size.Height - (labelTextImage.Size.Height / 2);
                    break;
                default:    //Label below the barcode
                    barcodeImage.Position.YPosition += (int)encodingTextImage.Size.Height;
                    labelTextImage.Position.YPosition += (int)encodingTextImage.Size.Height + barcodeImage.Size.Height;
                    break;
            }

            // Set the required image height by adding the barcode height to the encodeing text or value label text heights (if visible)
            int imageHeight = barcodeImage.Size.Height + encodingTextImage.Size.Height + labelTextImage.Size.Height;

            // Reduce the image height if human readable position is embeded
            if (linearEncoder.HumanReadablePosition == HumanReadablePosition.Embeded)
            {
                imageHeight -= (labelTextImage.Size.Height / 2);
            }

            int quietzone = 0;
            if (linearEncoder.Quietzone)
            {
                quietzone = Math.Max(20 * linearEncoder.Xdimension, linearEncoder.DPI / 4);
            }

            // Set the required image width by taking the greater of the barcode width and value label text width (if used)
            int imageWidth = Math.Max(barcodeImage.Size.Width + quietzone, labelTextImage.Size.Width);

            // Align all elements to center of image
            labelTextImage.Position.XPosition = (imageWidth - labelTextImage.Size.Width) / 2;
            barcodeImage.Position.XPosition = (imageWidth - barcodeImage.Size.Width) / 2;
            encodingTextImage.Position.XPosition = (imageWidth - barcodeImage.Size.Width) / 2;

            // Create the combined image
            Bitmap combinedImage = new Bitmap(imageWidth, imageHeight);
            combinedImage.SetResolution(linearEncoder.DPI, linearEncoder.DPI);
            Graphics combinedGraphics = Graphics.FromImage(combinedImage);

            // Add each element to the combined image
            combinedGraphics.FillRectangle(Brushes.White, 0, 0, combinedImage.Width, combinedImage.Height);
            combinedGraphics.DrawImageUnscaled(barcodeImage.Image, barcodeImage.Position.XPosition, barcodeImage.Position.YPosition);
            combinedGraphics.DrawImageUnscaled(encodingTextImage.Image, encodingTextImage.Position.XPosition, encodingTextImage.Position.YPosition);
            combinedGraphics.DrawImageUnscaled(labelTextImage.Image, labelTextImage.Position.XPosition, labelTextImage.Position.YPosition);

            // Save the image to the memory stream
            EncoderParameters encodingParams = new EncoderParameters(1);
            encodingParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            combinedImage.Save(memoryStream, linearEncoder.ImageCodec, encodingParams);

            // Set flag if xdimension was changed
            if(linearEncoder.Xdimension != XdimensionOriginal)
            {
                linearEncoder.XdimensionChanged = true;
            }
            
            // Dispose of the objects we won't need any more
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
        /// <param name="settings">The barcode settings as a Code128Settings object.</param>
        /// <param name="dimensions">The barcode dimensions as a Code128Dimensions object.</param>
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
            int barcodeWidth = linearEncoder.LinearEncoding.MinimumWidth * linearEncoder.Xdimension;

            // Get the original human readable font size, so we can compare with the size after adjusting to fir barcode width
            int humanReadableFontSizeOriginal = (int)linearEncoder.HumanReadableFont.Size;

            // Adjust the human readable font size so that the value does not exceed the width of the barcode image
            linearEncoder.HumanReadableFont = ImageHelpers.GetSizedFontForWidth(linearEncoder.HumanReadableValue, barcodeWidth, linearEncoder.DPI, linearEncoder.HumanReadableFont, true);

            // Set the human readable font size changed flag, if size different from original
            if(humanReadableFontSizeOriginal != (int)linearEncoder.HumanReadableFont.Size)
            {
                linearEncoder.HumanReadabaleFontSizeChanged = true;
            }

            // Measure the value label text size, based on the font provided
            SizeF labelTextSize = ImageHelpers.GetStringElementSize(linearEncoder.HumanReadableValue, linearEncoder.HumanReadableFont, linearEncoder.DPI);

            // Create a new bitmap image for the label value text based on the calculated dimensions
            labelValueElement.UpdateImage(new Bitmap((int)Math.Ceiling(labelTextSize.Width), (int)Math.Ceiling(labelTextSize.Height)));
            labelValueElement.Image.SetResolution(linearEncoder.DPI, linearEncoder.DPI);

            // Create a new graphics to draw on the barcode image
            Graphics labelValueGraphics = Graphics.FromImage(labelValueElement.Image);

            int xPosition = 1;
            int yPosition = 1;

            labelValueGraphics.FillRectangle(Brushes.White, xPosition, yPosition, labelValueElement.Image.Width, labelValueElement.Image.Height);
            labelValueGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            labelValueGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            labelValueGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            labelValueGraphics.DrawString(linearEncoder.HumanReadableValue, linearEncoder.HumanReadableFont, Brushes.Black, xPosition, yPosition);
            labelValueGraphics.Flush();

            return labelValueElement;
        }

        /// <summary>
        /// Creates the label value image element.
        /// </summary>
        /// <param name="settings">The barcode settings as a Code128Settings object.</param>
        /// <param name="dimensions">The barcode dimensions as a Code128Dimensions object.</param>
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
            Font labelValueFont = ImageHelpers.GetSizedFontForWidth(1, linearEncoder.LinearEncoding.GetWidestSymbol() * linearEncoder.Xdimension, linearEncoder.DPI, linearEncoder.HumanReadableFont);

            // Set the text size so that we can get the encoding element height later
            SizeF labelValueSize = ImageHelpers.GetStringElementSize("A", labelValueFont, linearEncoder.DPI);

            // Set the encoding image width from the minimum width multiplied by the x-dimension
            int labelImageWidth = linearEncoder.LinearEncoding.MinimumWidth * linearEncoder.Xdimension;

            //Create a new bitmap image for the encoding text based on the calculated dimensions
            labelValueElement.UpdateImage(new Bitmap(labelImageWidth, (int)Math.Ceiling(labelValueSize.Height)));
            labelValueElement.Image.SetResolution(linearEncoder.DPI, linearEncoder.DPI);

            // Create a new graphics to draw on the encoded text image
            Graphics LabelValueGraphics = Graphics.FromImage(labelValueElement.Image);
            //LabelValueGraphics.FillRectangle(Brushes.White, 0, 0, LabelImageWidth, (int)LabelValueSize.Height);

            string labelCharacter;
            int symbolWidth;
            int xPosition = 0;
            int yPosition = 0;

            for (int symbol = 0; symbol <= linearEncoder.LinearEncoding.Symbols.Count - 1; symbol++)
            {
                labelCharacter = linearEncoder.LinearEncoding.Symbols[symbol].Character;
                labelValueFont = ImageHelpers.GetSizedFontForWidth(labelCharacter.Length, linearEncoder.LinearEncoding.Symbols[symbol].Width * linearEncoder.Xdimension, linearEncoder.DPI, linearEncoder.HumanReadableFont);

                symbolWidth = linearEncoder.LinearEncoding.Symbols[symbol].Width;

                if (linearEncoder.LinearEncoding.Symbols[symbol].CharacterType != 1)
                {
                    LabelValueGraphics.FillRectangle(Brushes.White, xPosition, yPosition, symbolWidth * linearEncoder.Xdimension, labelValueElement.Image.Height);
                    LabelValueGraphics.DrawString(labelCharacter, labelValueFont, Brushes.Black, xPosition, yPosition);
                }

                xPosition += symbolWidth * linearEncoder.Xdimension;
            }

            labelValueFont.Dispose();
            LabelValueGraphics.Dispose();

            return labelValueElement;
        }

        /// <summary>
        /// Creates the barcode image element.
        /// </summary>
        /// <param name="coded">The encoded barcode as a Code128Coded object.</param>
        /// <param name="dimensions">The barcode dimensions as a Code128Dimensions object.</param>
        /// <returns>The generated barcode image inside an ImageElement object.</returns>
        internal static ImageElement GetBarcodeImage(LinearEncoder linearEncoder)
        {

            // Set the encoding image width from the minimum width multiplied by the x-dimension
            int barcodeImageWidth = linearEncoder.LinearEncoding.MinimumWidth * linearEncoder.Xdimension;

            // Create a new bitmap image for the barcode based on the calculated dimensions
            ImageElement barcodeElement = new ImageElement(new Bitmap(barcodeImageWidth, linearEncoder.BarcodeHeight));
            barcodeElement.Image.SetResolution(linearEncoder.DPI, linearEncoder.DPI);

            //Create a new graphics to draw on the barcode image
            Graphics BarcodeGraphics = Graphics.FromImage(barcodeElement.Image);

            int barWidth;
            int spaceWidth;
            int xPosition = 0;
            int yPosition = 0;

            BarcodeGraphics.FillRectangle(Brushes.White, xPosition, yPosition, barcodeElement.Image.Width, barcodeElement.Image.Height);

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
                            barWidth = symbolPattern[module].Width * linearEncoder.Xdimension;
                            BarcodeGraphics.FillRectangle(Brushes.Black, xPosition, yPosition, barWidth, linearEncoder.BarcodeHeight);
                            xPosition += barWidth;
                            BarcodeGraphics.Flush();
                            break;
                        case ModuleType.Space: // Space
                            spaceWidth = symbolPattern[module].Width * linearEncoder.Xdimension;
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
        /// <param name="coded">The encoded barcode as a Code128Coded object.</param>
        /// <param name="dimensions">The barcode dimensions as a Code128Dimensions object.</param>
        /// <param name="settings">The barcode settings as a Code128Settings object.</param>
        /// <returns>The generated encoding text image inside an ImageElement object.</returns>
        internal static ImageElement GetEncodingImage(LinearEncoder linearEncoder)
        {
            // Create an empty ImageElement
            ImageElement encodingTextElement = new ImageElement();

            // Retrun the empty ImageElement if encoding not visible
            if (linearEncoder.ShowEncoding == false)
            {
                return encodingTextElement;
            }

            // Setup the font for encoding with initial value "A" to assist calculating encoding element height later
            Font encodeCharFont = ImageHelpers.GetSizedFontForWidth(1, linearEncoder.LinearEncoding.GetWidestSymbol() * linearEncoder.Xdimension, linearEncoder.DPI, new Font(linearEncoder.EncodingFontFamily, 8), false);

            // Set the text size so that we can get the encoding element height later
            SizeF encodingTextSize = ImageHelpers.GetStringElementSize("A", encodeCharFont, linearEncoder.DPI);

            // Set the encoding image width from the minimum width multiplied by the x-dimension
            int encodingImageWidth = linearEncoder.LinearEncoding.MinimumWidth * linearEncoder.Xdimension;

            //Create a new bitmap image for the encoding text based on the calculated dimensions
            encodingTextElement.UpdateImage(new Bitmap(encodingImageWidth, (int)Math.Ceiling(encodingTextSize.Height)));
            encodingTextElement.Image.SetResolution(linearEncoder.DPI, linearEncoder.DPI);

            // Create a new graphics to draw on the encoded text image
            Graphics encodingTextGraphics = Graphics.FromImage(encodingTextElement.Image);
            encodingTextGraphics.FillRectangle(Brushes.White, 0, 0, encodingImageWidth, (int)Math.Ceiling(encodingTextSize.Height));

            string encodeCharacter;
            int symbolWidth;
            int xPosition = 0;
            int yPosition = 0;
            Brush encodeBrush;
            Pen encodePen = new Pen(Brushes.Black, 1);

            for (int symbol = 0; symbol <= linearEncoder.LinearEncoding.Symbols.Count - 1; symbol++)
            {
                encodeCharacter = linearEncoder.LinearEncoding.Symbols[symbol].Character;
                encodeCharFont = ImageHelpers.GetSizedFontForWidth(encodeCharacter.Length, linearEncoder.LinearEncoding.Symbols[symbol].Width * linearEncoder.Xdimension, linearEncoder.DPI, new Font(linearEncoder.EncodingFontFamily, 8), false);

                symbolWidth = linearEncoder.LinearEncoding.Symbols[symbol].Width;
                //string SymbolPattern = coded.SymbolPattern[symbol];

                if (linearEncoder.LinearEncoding.Symbols[symbol].CharacterType == 1)
                {
                    encodingTextGraphics.FillRectangle(Brushes.Black, xPosition, yPosition, symbolWidth * linearEncoder.Xdimension, encodingTextElement.Image.Height);
                    encodeBrush = Brushes.White;
                }
                else
                {
                    encodingTextGraphics.DrawRectangle(encodePen, xPosition, yPosition, (symbolWidth * linearEncoder.Xdimension) - 1, encodingTextElement.Image.Height - 1);
                    encodeBrush = Brushes.Black;
                }

                encodingTextGraphics.DrawString(encodeCharacter, encodeCharFont, encodeBrush, xPosition, yPosition);

                xPosition += symbolWidth * linearEncoder.Xdimension;
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
