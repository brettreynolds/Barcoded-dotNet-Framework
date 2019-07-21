using System.Drawing;
using System.Drawing.Imaging;

namespace Barcoded
{
    internal class ImageHelpers
    {
        /// <summary>
        /// Resizes the given font to fit within the specified width.
        /// </summary>
        /// <param name="stringLength">String to be measured.</param>
        /// <param name="width">Available width.</param>
        /// <param name="dpi">Image DPI.</param>
        /// <param name="font">Font to be resized.</param>
        /// <param name="limitSizeToFont">Limit maximum font size to the original font provided.</param>
        /// <returns>Font, size adjusted to fir width.</returns>
        internal static Font GetSizedFontForWidth(int stringLength, int width, int dpi, Font font, bool limitSizeToFont = true)
        {
            return GetSizedFontForWidth(new string('\u0057', stringLength), width, dpi, font, limitSizeToFont);

        }

        /// <summary>
        /// Gets the font adjusted to the maximum size that will allow the given text to fit the given width.
        /// </summary>
        /// <param name="textToFit">Text that needs to fit</param>
        /// <param name="width">Available width</param>
        /// <param name="dpi">Image DPI</param>
        /// <param name="font">Font to be measured</param>
        /// <param name="limitSizeToFont">Limit maximum font size returned to the font size provided</param>
        /// <returns>Font set to the maximum size that will fit</returns>
        internal static Font GetSizedFontForWidth(string textToFit, int width, int dpi, Font font, bool limitSizeToFont = true)
        {
            Bitmap image = new Bitmap(width, 100);
            image.SetResolution(dpi, dpi);
            Graphics imageMeasure = Graphics.FromImage(image);

            Font sizedFont = new Font(font.FontFamily, 1);

            for (int fontSize = 1; fontSize <= 500; fontSize++)
            {
                Font tryFont = new Font(font.FontFamily, fontSize);
                SizeF imageSize = imageMeasure.MeasureString(textToFit, tryFont);

                if (imageSize.Width < width)
                {
                    sizedFont = tryFont;
                    if (fontSize == (int)font.Size && limitSizeToFont) goto ExitFor;
                }
                else
                {
                    goto ExitFor;
                }
                    
            }

            ExitFor:
            image.Dispose();
            imageMeasure.Dispose();
            return sizedFont;
        }

        /// <summary>
        /// Returns the image width required for the given text drawn using the specified font.
        /// </summary>
        /// <param name="text">String to be measured</param>
        /// <param name="font">Font to be used</param>
        /// <param name="dpi">Image DPI</param>
        /// <returns>Measured image size</returns>
        internal static SizeF GetStringElementSize(string text, Font font, int dpi)
        {
            Bitmap bitmap = new Bitmap(1, 1);
            bitmap.SetResolution(dpi, dpi);
            Graphics graphics = Graphics.FromImage(bitmap);
            SizeF size = graphics.MeasureString(text, font);
            graphics.Dispose();
            bitmap.Dispose();
            return size;
        }

        /// <summary>
        /// Returns the image codec for the given codec name.
        /// </summary>
        /// <param name="codecName">Codec name.</param>
        /// <remarks>Will return PNG, if specified codec cannot be found.</remarks>
        /// <returns>Image codec.</returns>
        internal static ImageCodecInfo FindCodecInfo(string codecName)
        {
            while (true)
            {
                codecName = codecName.ToUpper();

                switch (codecName)
                {
                    case "JPG":
                        codecName = "JPEG";
                        break;
                    case "TIF":
                        codecName = "TIFF";
                        break;
                }

                ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

                foreach (ImageCodecInfo encoder in encoders)
                {
                    if (encoder.FormatDescription.Equals(codecName))
                    {
                        return encoder;
                    }
                }

                codecName = "PNG";
            }
        }
    }
}
