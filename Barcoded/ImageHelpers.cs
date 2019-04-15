using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Barcoded
{
    class ImageHelpers
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
        static public Font GetSizedFontForWidth(int stringLength, int width, int dpi, Font font, bool limitSizeToFont = true)
        {
            return GetSizedFontForWidth(new string('\u0057', stringLength), width, dpi, font, limitSizeToFont);

        }

        static public Font GetSizedFontForWidth(string textToFit, int width, int dpi, Font font, bool limitSizeToFont = true)
        {
            Bitmap Image = new Bitmap(width, 100);
            Image.SetResolution(dpi, dpi);
            Graphics ImageMeasure = Graphics.FromImage(Image);
            SizeF ImageSize;
            Font SizedFont;
            //string text = new string('\u0057', stringLength);

            SizedFont = new Font(font.FontFamily, 1);

            for (int FontSize = 1; FontSize <= 500; FontSize++)
            {
                Font TryFont = new Font(font.FontFamily, FontSize);
                ImageSize = ImageMeasure.MeasureString(textToFit, TryFont);

                if (ImageSize.Width < width)
                {
                    SizedFont = TryFont;
                    if (FontSize == font.Size && limitSizeToFont == true)
                        goto ExitFor;
                }
                else
                    goto ExitFor;
            }

            ExitFor:
            Image.Dispose();
            ImageMeasure.Dispose();
            return SizedFont;
        }

        /// <summary>
        /// Returns the image width required for the given text drawn using the specified font.
        /// </summary>
        /// <param name="text">String to be measured.</param>
        /// <param name="font">Font to be used.</param>
        /// <param name="dpi">Image DPI.</param>
        /// <returns></returns>
        static public SizeF GetStringElementSize(string text, Font font, int dpi)
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
        /// <returns>Image codec.</returns>
        static public ImageCodecInfo FindCodecInfo(string codecName)
        {
            codecName = codecName.ToUpper();

            switch(codecName)
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

            return FindCodecInfo("PNG");
        }
    }
}
