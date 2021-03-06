﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace UB.Utils
{
    public static class BM
    {
        public static Bitmap CropBitmap(Bitmap bitmap, int cropX, int cropY, int cropWidth, int cropHeight)
        {
            Rectangle rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap cropped = bitmap.Clone(rect, bitmap.PixelFormat);
            return cropped;
        }

        public static Stream CroppedBitmapToPngStream( this CroppedBitmap image )
        {
            MemoryStream memStream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            var frame = encoder.With(x => image)
                .With(x => BitmapFrame.Create(x));
            if (frame == null)
                return null;

            encoder.Frames.Add(frame);
            encoder.Save(memStream);
            return memStream;

        }
        public static Stream ToPngStream(this System.Windows.Controls.Image image)
        {
            if (image == null || image.Source == null)
                return null;

            MemoryStream memStream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            var frame = encoder.With( x => image.Source as CroppedBitmap)
                .With( x => BitmapFrame.Create(x));
            if (frame == null)
                return null;

            encoder.Frames.Add(frame);
            encoder.Save(memStream);
            return memStream;
        }

        public static byte[] ConvertToByteArray(this RenderTargetBitmap renderTarget)
        {
            if (renderTarget == null || renderTarget.PixelHeight == 0 || renderTarget.PixelWidth == 0)
                return null;

            int stride = renderTarget.PixelWidth * renderTarget.Format.BitsPerPixel / 8;
            int size = stride * renderTarget.PixelHeight;

            byte[] buffer = new byte[size];

            renderTarget.CopyPixels(buffer, stride, 0);

            return buffer;
        }
        public static byte[] ConvertToByteArray(this WriteableBitmap renderTarget)
        {
            if (renderTarget == null || renderTarget.PixelHeight == 0 || renderTarget.PixelWidth == 0)
                return null;

            int stride = renderTarget.PixelWidth * renderTarget.Format.BitsPerPixel / 8;
            int size = stride * renderTarget.PixelHeight;

            byte[] buffer = new byte[size];

            renderTarget.CopyPixels(buffer, stride, 0);

            return buffer;
        }
    }
}
