using ImageMagick;

using System.Text.Json;

namespace MagickTest;

public class FlatGrid
{

    public static void Run(byte[] bytes, string outputFolder, int tileSize)
    {
        var imageDef = CropImage(bytes, tileSize);
        var json = JsonSerializer.Serialize(imageDef);
        File.WriteAllText(outputFolder + "image_def.json", json);
    }

    private static ImageDef CropImage(byte[] pngImageBytes, int tileSize)
    {
        var settings = new MagickReadSettings();
        settings.Format = MagickFormat.Png32;
        settings.ColorSpace = ColorSpace.sRGB;

        using var fullImage = new MagickImage(pngImageBytes, settings);

        var fullWidth = fullImage.Width;
        var fullHeight = fullImage.Height;
        var tileSizeTarget = tileSize;

        var croppedImages = fullImage.CropToTiles(tileSizeTarget, tileSizeTarget);

        int noOfImgX = (int)Math.Ceiling((double)fullWidth / tileSizeTarget);

        var tiles = croppedImages.ToList().Select((croppedImage, i) =>
        {
            PadImage(croppedImage);

            var tileWidth = croppedImage.Width;
            var tileHeight = croppedImage.Height;
            var halfWidth = (double)croppedImage.Width / 2;
            var halfHeight = (double)croppedImage.Height / 2;
            double x = ((i % noOfImgX) * tileSizeTarget) + halfWidth;
            double y = (fullHeight - (i / noOfImgX) * tileSizeTarget) - halfHeight;

            ImageData? imageData = null;

            if (!IsPureTransparent(croppedImage))
            {
                imageData = ResizeAndGetDataUrls(croppedImage);
            }

            return new ImageDefTile(x, y, tileWidth, tileHeight, imageData);
        }).ToList();

        foreach (var croppedImage in croppedImages)
        {
            croppedImage.Dispose();
        }

        return new ImageDef(tiles);
    }

    private static ImageData ResizeAndGetDataUrls(IMagickImage<ushort> image)
    {
        var full = new ImageDataUrl("full", GetDataUrl(image));

        ResizeToHalf(image);
        var half = new ImageDataUrl("half", GetDataUrl(image));

        ResizeToHalf(image);
        var quater = new ImageDataUrl("quater", GetDataUrl(image));

        return new ImageData(full, half, quater);
    }

    private static void ResizeToHalf(IMagickImage<ushort> image)
    {
        int newWidth = image.Width / 2;
        int newHeight = image.Height / 2;

        int newWidthCeiled = (int)Math.Ceiling(image.Width / 2.0);
        int newHeightCeiled = (int)Math.Ceiling(image.Height / 2.0);

        image.Resize(newWidth, newHeight);

        if (newWidthCeiled != newWidth || newHeightCeiled != newHeight)
        {
            image.Extent(newWidthCeiled, newHeightCeiled, MagickColors.Transparent);
        }
    }

    private static string GetDataUrl(IMagickImage<ushort> image)
    {
        image.Write("tmp.png");
        var settings = new MagickReadSettings();
        settings.Format = MagickFormat.Png32;
        settings.ColorSpace = ColorSpace.sRGB;
        using var loadedImage = new MagickImage("tmp.png", settings);

        var imageBytes = loadedImage.ToByteArray();
        var asBase64 = Convert.ToBase64String(imageBytes);
        return $"data:image/png;base64,{asBase64}";
    }

    private static bool IsPureTransparent(IMagickImage<ushort> image)
    {
        var pixels = image.GetPixels();
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = pixels[x, y];
                var alpha = pixel[3];

                if (alpha > 1000)
                {
                    pixels.Dispose();
                    return false;
                }
            }
        }

        pixels.Dispose();
        return true;
    }

    /// <summary>
    /// Pads the Image so that it is divisible by 4.
    /// This is required when converting the image to .ktx2 format
    /// </summary>
    /// <param name="image"></param>
    private static void PadImage(IMagickImage<ushort> image)
    {
        int paddedWidth = PadToBeDivisibleByFour(image.Width);
        int paddedHeight = PadToBeDivisibleByFour(image.Height);
        image.Extent(paddedWidth, paddedHeight, MagickColors.Transparent);
    }

    private static int PadToBeDivisibleByFour(int value)
    {
        if (value % 4 != 0)
        {
            var increaseBy = 4 - (value % 4);
            return value + increaseBy;
        }

        return value;
    }
}
