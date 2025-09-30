using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    internal class ConvMatrix
    {
        public int TopLeft = 0, TopMid = 0, TopRight = 0;
        public int MidLeft = 0, Pixel = 1, MidRight = 0;
        public int BottomLeft = 0, BottomMid = 0, BottomRight = 0;
        public int Factor = 1;
        public int Offset = 0;

        public void SetAll(int nVal)
        {
            TopLeft = TopMid = TopRight = MidLeft = Pixel = MidRight = BottomLeft = BottomMid = BottomRight = nVal;
        }
        public void SetManual(int[] arr)
        {
            if (arr.Length != 9) throw new ArgumentException("Array must have exactly 9 elements.");
            TopLeft = arr[0];
            TopMid = arr[1];
            TopRight = arr[2];
            MidLeft = arr[3];
            Pixel = arr[4];
            MidRight = arr[5];
            BottomLeft = arr[6];
            BottomMid = arr[7];
            BottomRight = arr[8];
        }

        public static Bitmap Conv3x3(Bitmap src, ConvMatrix m)
        {
            // Avoid divide by zero errors 
            if (0 == m.Factor) return null;

            // Clone the source for reading
            Bitmap bSrc = (Bitmap)src.Clone();
            // Create a new bitmap for output
            Bitmap result = new Bitmap(src.Width, src.Height);

            BitmapData bmData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height),
                                                ImageLockMode.ReadWrite,
                                                PixelFormat.Format24bppRgb);
            BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height),
                                             ImageLockMode.ReadWrite,
                                             PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            int stride2 = stride * 2;

            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr SrcScan0 = bmSrc.Scan0;

            unsafe
            {
                byte* p = (byte*)Scan0.ToPointer();
                byte* pSrc = (byte*)SrcScan0.ToPointer();
                int nOffset = stride - src.Width * 3;
                int nWidth = src.Width - 2;
                int nHeight = src.Height - 2;

                int nPixel;

                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        nPixel = ((((pSrc[2] * m.TopLeft) +
                                    (pSrc[5] * m.TopMid) +
                                    (pSrc[8] * m.TopRight) +
                                    (pSrc[2 + stride] * m.MidLeft) +
                                    (pSrc[5 + stride] * m.Pixel) +
                                    (pSrc[8 + stride] * m.MidRight) +
                                    (pSrc[2 + stride2] * m.BottomLeft) +
                                    (pSrc[5 + stride2] * m.BottomMid) +
                                    (pSrc[8 + stride2] * m.BottomRight))
                                    / m.Factor) + m.Offset);
                        nPixel = Math.Max(0, Math.Min(255, nPixel));
                        p[5 + stride] = (byte)nPixel;

                        nPixel = ((((pSrc[1] * m.TopLeft) +
                                    (pSrc[4] * m.TopMid) +
                                    (pSrc[7] * m.TopRight) +
                                    (pSrc[1 + stride] * m.MidLeft) +
                                    (pSrc[4 + stride] * m.Pixel) +
                                    (pSrc[7 + stride] * m.MidRight) +
                                    (pSrc[1 + stride2] * m.BottomLeft) +
                                    (pSrc[4 + stride2] * m.BottomMid) +
                                    (pSrc[7 + stride2] * m.BottomRight))
                                    / m.Factor) + m.Offset);
                        nPixel = Math.Max(0, Math.Min(255, nPixel));
                        p[4 + stride] = (byte)nPixel;

                        nPixel = ((((pSrc[0] * m.TopLeft) +
                                    (pSrc[3] * m.TopMid) +
                                    (pSrc[6] * m.TopRight) +
                                    (pSrc[0 + stride] * m.MidLeft) +
                                    (pSrc[3 + stride] * m.Pixel) +
                                    (pSrc[6 + stride] * m.MidRight) +
                                    (pSrc[0 + stride2] * m.BottomLeft) +
                                    (pSrc[3 + stride2] * m.BottomMid) +
                                    (pSrc[6 + stride2] * m.BottomRight))
                                    / m.Factor) + m.Offset);
                        nPixel = Math.Max(0, Math.Min(255, nPixel));
                        p[3 + stride] = (byte)nPixel;

                        p += 3;
                        pSrc += 3;
                    }
                    p += nOffset;
                    pSrc += nOffset;
                }
            }

            result.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);

            return result;
        }

        //public static Bitmap Conv3x3(Bitmap src, ConvMatrix m)
        //{
        //    // Avoid divide by zero errors 
        //    if (0 == m.Factor) return null;

        //    // GDI+ still lies to us - the return format is BGR, NOT RGB.  
        //    Bitmap bSrc = (Bitmap)src.Clone();
        //    BitmapData bmData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        //    BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //    int stride = bmData.Stride;
        //    int stride2 = stride * 2;

        //    System.IntPtr Scan0 = bmData.Scan0;
        //    System.IntPtr SrcScan0 = bmSrc.Scan0;

        //    unsafe
        //    {
        //        byte* p = (byte*)(void*)Scan0;
        //        byte* pSrc = (byte*)(void*)SrcScan0;
        //        int nOffset = stride - src.Width * 3;
        //        int nWidth = src.Width - 2;
        //        int nHeight = src.Height - 2;

        //        int nPixel;

        //        for (int y = 0; y < nHeight; ++y)
        //        {
        //            for (int x = 0; x < nWidth; ++x)
        //            {
        //                nPixel = ((((pSrc[2] * m.TopLeft) +

        //                    (pSrc[5] * m.TopMid) +
        //                    (pSrc[8] * m.TopRight) +
        //                    (pSrc[2 + stride] * m.MidLeft) +
        //                    (pSrc[5 + stride] * m.Pixel) +
        //                    (pSrc[8 + stride] * m.MidRight) +
        //                    (pSrc[2 + stride2] * m.BottomLeft) +
        //                    (pSrc[5 + stride2] * m.BottomMid) +
        //                    (pSrc[8 + stride2] * m.BottomRight))
        //                    / m.Factor) + m.Offset);

        //                if (nPixel < 0) nPixel = 0;
        //                if (nPixel > 255) nPixel = 255;
        //                p[5 + stride] = (byte)nPixel;

        //                nPixel = ((((pSrc[1] * m.TopLeft) +
        //                    (pSrc[4] * m.TopMid) +
        //                    (pSrc[7] * m.TopRight) +
        //                    (pSrc[1 + stride] * m.MidLeft) +
        //                    (pSrc[4 + stride] * m.Pixel) +
        //                    (pSrc[7 + stride] * m.MidRight) +
        //                    (pSrc[1 + stride2] * m.BottomLeft) +
        //                    (pSrc[4 + stride2] * m.BottomMid) +
        //                    (pSrc[7 + stride2] * m.BottomRight))
        //                    / m.Factor) + m.Offset);

        //                if (nPixel < 0) nPixel = 0;
        //                if (nPixel > 255) nPixel = 255;
        //                p[4 + stride] = (byte)nPixel;

        //                nPixel = ((((pSrc[0] * m.TopLeft) +

        //                               (pSrc[3] * m.TopMid) +
        //                               (pSrc[6] * m.TopRight) +
        //                               (pSrc[0 + stride] * m.MidLeft) +
        //                               (pSrc[3 + stride] * m.Pixel) +
        //                               (pSrc[6 + stride] * m.MidRight) +
        //                               (pSrc[0 + stride2] * m.BottomLeft) +
        //                               (pSrc[3 + stride2] * m.BottomMid) +
        //                               (pSrc[6 + stride2] * m.BottomRight))
        //                    / m.Factor) + m.Offset);

        //                if (nPixel < 0) nPixel = 0;
        //                if (nPixel > 255) nPixel = 255;
        //                p[3 + stride] = (byte)nPixel;
        //                p += 3;
        //                pSrc += 3;
        //            }

        //            p += nOffset;
        //            pSrc += nOffset;
        //        }
        //    }

        //    src.UnlockBits(bmData);
        //    bSrc.UnlockBits(bmSrc);
        //    return src;
        //}
    }
}
