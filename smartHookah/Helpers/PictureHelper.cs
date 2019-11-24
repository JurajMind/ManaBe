using System;
using System.Collections.Generic;
using System.Drawing;

namespace smartHookah.Helpers
{
    static public class PictureHelper
    {
        public static string XbmToWebString(byte[] converted)
        {
            return Convert.ToBase64String(converted);
        }

        public static Bitmap XbmToBmp(byte[] test_bits, int Height, int Width)
        {
            //Sample data from http://en.wikipedia.org/wiki/X_BitMap
            //int Width = 16;
            //int Height = 7;



            //Create our bitmap
            Bitmap B = new Bitmap(Width, Height);
            //Will hold our byte as a string of bits
            string Bits = null;

            //Current X,Y of the painting process
            int X = 0;
            int Y = 0;

            //Loop through all of the bits
            for (int i = 0; i < test_bits.Length; i++)
            {
                //Convert the current byte to a string of bits and pad with extra 0's if needed
                Bits = Convert.ToString(test_bits[i], 2).PadLeft(8, '0');

                //Bits are stored with the first pixel in the least signigicant bit so we need to work the string from right to left
                for (int j = 7; j >= 0; j--)
                {
                    //Set the pixel's color based on whether the current bit is a 0 or 1
                    B.SetPixel(X, Y, Bits[j] == '0' ? Color.White : Color.Black);
                    //Incremement our X position
                    X += 1;

                    if (X >= Width)
                    {
                        X = 0;
                        Y += 1;

                        if (Y >= Height)
                        {
                            return B;
                        }

                        break;

                    }



                }
            }

            //Output the bitmap to the desktop
            return B;

        }

        public static Bitmap TrimBitmap(Bitmap bmp)
        {
            int minX = int.MaxValue, maxX = int.MinValue,
        minY = int.MaxValue, maxY = int.MinValue;
            // Brute-force scan of the bitmap to find image boundary
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    if (bmp.GetPixel(x, y).Name != "ffffffff")
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            Bitmap newBitmap = new Bitmap(maxX - minX + 1, maxY - minY + 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < newBitmap.Height; y++)
            {
                for (int x = 0; x < newBitmap.Width; x++)
                {
                    newBitmap.SetPixel(x, y, bmp.GetPixel(x + minX, y + minY));
                }
            }
            return newBitmap;
        }

        public static byte[] BmpToXbm(Bitmap B)
        {
            char[] buffer = new char[8];
            int X = 0;
            int Y = 0;

            List<Byte> result = new List<byte>();
            int bufferIndex = 0;

            for (int i = 0; i < B.Height; i++)
            {
                for (int j = 0; j < B.Width; j++)
                {
                    buffer[bufferIndex] = B.GetPixel(j, i).Name == "ffffffff" ? '0' : '1';
                    bufferIndex++;

                    if (bufferIndex >= 8)
                    {
                        Array.Reverse(buffer);
                        var tmpByte = Convert.ToByte(new string(buffer), 2);
                        result.Add((byte)tmpByte);
                        bufferIndex = 0;
                    }
                }

                if (bufferIndex < 8 && bufferIndex != 0)
                {
                    for (int j = bufferIndex + 1; j < 8; j++)
                    {
                        buffer[bufferIndex] = '0';
                    }
                    Array.Reverse(buffer);
                    var tmpByte = Convert.ToByte(new string(buffer), 2);
                    result.Add((byte)tmpByte);
                    bufferIndex = 0;
                }
            }
            return result.ToArray();
        }
    }
}