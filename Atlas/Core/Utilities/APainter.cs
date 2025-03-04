using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Atlas.Core.Utilities
{
    public static class APainter
    {
        public static BitmapSource ProcessImage(BitmapSource originalImage, int GridHeight, int GridWidth, int FeatherWidth, int BlurRadius)
        {
            // Determine scaling based on aspect ratio
            double imgAspectRatio = (double)originalImage.PixelWidth / originalImage.PixelHeight;
            double gridAspectRatio = (double)GridWidth / GridHeight;

            int scaledWidth, scaledHeight;
            if (imgAspectRatio < gridAspectRatio) // Taller than wide relative to grid
            {
                scaledHeight = GridHeight;
                scaledWidth = (int)(originalImage.PixelWidth * ((double)GridHeight / originalImage.PixelHeight));
            }
            else // Wider than tall relative to grid
            {
                scaledWidth = GridWidth;
                scaledHeight = (int)(originalImage.PixelHeight * ((double)GridWidth / originalImage.PixelWidth));
            }

            // Scale the main image
            TransformedBitmap mainImage = new TransformedBitmap(originalImage, new ScaleTransform(
                (double)scaledWidth / originalImage.PixelWidth,
                (double)scaledHeight / originalImage.PixelHeight));

            // Create a blurred background stretched to grid size
            TransformedBitmap stretchedImage = new TransformedBitmap(originalImage, new ScaleTransform(
                (double)GridWidth / originalImage.PixelWidth,
                (double)GridHeight / originalImage.PixelHeight));
            BlurEffect blur = new BlurEffect { Radius = BlurRadius };
            VisualBrush visualBrush = new VisualBrush(new Image { Source = stretchedImage });
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(visualBrush, null, new Rect(0, 0, GridWidth, GridHeight));
            }
            RenderTargetBitmap blurredBackground = new RenderTargetBitmap(GridWidth, GridHeight, 96, 96, PixelFormats.Pbgra32);
            blurredBackground.Render(drawingVisual);
            blurredBackground.Freeze();

            // Combine into a single output bitmap
            WriteableBitmap output = new WriteableBitmap(GridWidth, GridHeight, 96, 96, PixelFormats.Bgra32, null);
            byte[] mainPixels = new byte[scaledWidth * scaledHeight * 4];
            mainImage.CopyPixels(mainPixels, scaledWidth * 4, 0);
            byte[] backgroundPixels = new byte[GridWidth * GridHeight * 4];
            blurredBackground.CopyPixels(backgroundPixels, GridWidth * 4, 0);

            // Fill the output bitmap
            byte[] outputPixels = new byte[GridWidth * GridHeight * 4];
            int offsetX = (GridWidth - scaledWidth) / 2;
            int offsetY = (GridHeight - scaledHeight) / 2;

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    int index = (y * GridWidth + x) * 4;
                    byte bgB = backgroundPixels[index];
                    byte bgG = backgroundPixels[index + 1];
                    byte bgR = backgroundPixels[index + 2];
                    byte bgA = (byte)(backgroundPixels[index + 3] * 0.8); // 80% opacity for background

                    if (x >= offsetX && x < offsetX + scaledWidth && y >= offsetY && y < offsetY + scaledHeight)
                    {
                        // Inside the main image area
                        int imgX = x - offsetX;
                        int imgY = y - offsetY;
                        int imgIndex = (imgY * scaledWidth + imgX) * 4;
                        byte fgB = mainPixels[imgIndex];
                        byte fgG = mainPixels[imgIndex + 1];
                        byte fgR = mainPixels[imgIndex + 2];
                        byte fgA = mainPixels[imgIndex + 3];

                        // Apply feathering based on orientation
                        double alphaFactor = 1.0;
                        if (imgAspectRatio < gridAspectRatio) // Taller: feather left/right
                        {
                            if (x < offsetX + FeatherWidth) // Left feather
                                alphaFactor = (x - offsetX) / (double)FeatherWidth;
                            else if (x >= offsetX + scaledWidth - FeatherWidth) // Right feather
                                alphaFactor = (offsetX + scaledWidth - x) / (double)FeatherWidth;
                        }
                        else // Wider: feather top/bottom
                        {
                            if (y < offsetY + FeatherWidth) // Top feather
                                alphaFactor = (y - offsetY) / (double)FeatherWidth;
                            else if (y >= offsetY + scaledHeight - FeatherWidth) // Bottom feather
                                alphaFactor = (offsetY + scaledHeight - y) / (double)FeatherWidth;
                        }

                        // Blend foreground and background
                        byte a = (byte)(fgA * alphaFactor);
                        outputPixels[index] = (byte)((fgB * a + bgB * (255 - a)) / 255);
                        outputPixels[index + 1] = (byte)((fgG * a + bgG * (255 - a)) / 255);
                        outputPixels[index + 2] = (byte)((fgR * a + bgR * (255 - a)) / 255);
                        outputPixels[index + 3] = (byte)(a + (255 - a) * bgA / 255);
                    }
                    else
                    {
                        // Outside main image: use blurred background with 80% opacity
                        outputPixels[index] = bgB;
                        outputPixels[index + 1] = bgG;
                        outputPixels[index + 2] = bgR;
                        outputPixels[index + 3] = bgA;
                    }
                }
            }

            output.WritePixels(new Int32Rect(0, 0, GridWidth, GridHeight), outputPixels, GridWidth * 4, 0);
            return output;

        }
    }
}

