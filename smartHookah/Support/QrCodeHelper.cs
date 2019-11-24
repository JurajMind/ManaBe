using QRCoder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace smartHookah.Support
{
    public static class QrCodeHelper
    {
        /// <summary>
        /// Produces the markup for an image element that displays a QR Code image, as provided by Google's chart API.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="data">The data to be encoded, as a string.</param>
        /// <param name="size">The square length of the resulting image, in pixels.</param>
        /// <param name="margin">The width of the border that surrounds the image, measured in rows (not pixels).</param>
        /// <param name="errorCorrectionLevel">The amount of error correction to build into the image.  Higher error correction comes at the expense of reduced space for data.</param>
        /// <param name="htmlAttributes">Optional HTML attributes to include on the image element.</param>
        /// <returns></returns>
        public static MvcHtmlString QRCode(this HtmlHelper htmlHelper, string data, int size = 80, int margin = 4, QRCodeErrorCorrectionLevel errorCorrectionLevel = QRCodeErrorCorrectionLevel.Low, object htmlAttributes = null)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (size < 1)
                throw new ArgumentOutOfRangeException("size", size, "Must be greater than zero.");
            if (margin < 0)
                throw new ArgumentOutOfRangeException("margin", margin, "Must be greater than or equal to zero.");
            if (!Enum.IsDefined(typeof(QRCodeErrorCorrectionLevel), errorCorrectionLevel))
                throw new InvalidEnumArgumentException("errorCorrectionLevel", (int)errorCorrectionLevel, typeof(QRCodeErrorCorrectionLevel));

            var url = string.Format("https://chart.apis.google.com/chart?cht=qr&chld={2}|{3}&chs={0}x{0}&chl={1}", size, HttpUtility.UrlEncode(data), errorCorrectionLevel.ToString()[0], margin);

            var tag = new TagBuilder("img");
            if (htmlAttributes != null)
                tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            tag.Attributes.Add("src", url);
            tag.Attributes.Add("width", size.ToString());
            tag.Attributes.Add("height", size.ToString());

            return new MvcHtmlString(tag.ToString(TagRenderMode.SelfClosing));
        }

        public enum QRCodeErrorCorrectionLevel
        {
            /// <summary>Recovers from up to 7% erroneous data.</summary>
            Low,
            /// <summary>Recovers from up to 15% erroneous data.</summary>
            Medium,
            /// <summary>Recovers from up to 25% erroneous data.</summary>
            QuiteGood,
            /// <summary>Recovers from up to 30% erroneous data.</summary>
            High
        }

        public static string GetBase64QrCode(string text)
        {
            var byteArray = GenerateQrCode(text.PadRight(42));
            return Convert.ToBase64String(byteArray);
        }

        private static byte[] GenerateQrCode(string text)
        {
            var result = new List<byte>();
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.L);
            var buffer = new BitArray(32);
            foreach (var line in qrCodeData.ModuleMatrix.Skip(2).Take(32).ToArray())
            {

                var sliced = line.Slice(2, 32);
                Debug.WriteLine(sliced.ToZeroOneString());
                var a = sliced.ToByteArray();
                result.AddRange(a);
            }
            return result.ToArray();
        }

        public static BitArray Slice(this BitArray from, int start, int count)
        {
            var result = new BitArray(count);

            for (int i = 0; i < count; i++)
            {

                result[i] = @from[i + start];
            }

            return result;
        }

        public static string ToZeroOneString(this BitArray from)
        {
            StringBuilder result = new StringBuilder(@from.Length);
            foreach (bool b in @from)
            {
                result.Append(b ? '#' : ' ');
            }

            return result.ToString();
        }

        public static byte[] ToByteArray(this BitArray bits)
        {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }
    }
}