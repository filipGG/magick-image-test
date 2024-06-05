using ImageMagick;

using System.Text.Json;

namespace MagickTest;

public record LevelImageDef(List<LevelDef> Levels);

public record LevelDef(List<LevelTile> Tiles);

public record LevelTile(double X, double Y, double Width, double Height, string? dataUrl);

public class LeveledGrid
{
    public static void Run(byte[] bytes, string outputFolder)
    {
        var settings = new MagickReadSettings();
        settings.Format = MagickFormat.Png32;
        settings.ColorSpace = ColorSpace.sRGB;
        using var fullImage = new MagickImage(bytes, settings);

        var fullWidth = fullImage.Width;
        var fullHeight = fullImage.Height;

        var fullTiles = GenerateTiles(fullImage, 256, 256, fullWidth, fullHeight);
        ResizeToHalf(fullImage);

        var halfTiles = GenerateTiles(fullImage, 256, 512, fullWidth, fullHeight);
        ResizeToHalf(fullImage);

        var quarterTiles = GenerateTiles(fullImage, 256, 1024, fullWidth, fullHeight);
        ResizeToHalf(fullImage);

        var eightTiles = GenerateTiles(fullImage, 256, 2048, fullWidth, fullHeight);
        ResizeToHalf(fullImage);
        var sixteenthTiles = GenerateTiles(fullImage, 256, 4096, fullWidth, fullHeight);

        var result = new LevelImageDef([fullTiles, halfTiles, quarterTiles, eightTiles, sixteenthTiles]);

        var json = JsonSerializer.Serialize(result);
        File.WriteAllText(outputFolder + "leveled_image_def.json", json);
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

    private static LevelDef GenerateTiles(
        IMagickImage<ushort> fullImage,
        int tileTargetSize,
        int tileGeometrySize,
        int fullWidth,
        int fullHeight
    )
    {
        var croppedImages = fullImage.CropToTiles(tileTargetSize, tileTargetSize).ToList();
        int noOfImgX = (int)Math.Ceiling((double)fullImage.Width / tileTargetSize);
        int noOfImgY = (int)Math.Ceiling((double)fullImage.Height / tileTargetSize);

        var geometryAdjustedWidth = noOfImgX * tileGeometrySize;
        var geometryAdjustedHeight = noOfImgY * tileGeometrySize;

        var tiles = croppedImages.Select((croppedImage, i) =>
        {
            PadImage(croppedImage);

            var tileWidth = tileGeometrySize;
            var tileHeight = tileGeometrySize;
            var halfWidth = (double)tileWidth / 2;
            var halfHeight = (double)tileHeight / 2;
            double x = ((i % noOfImgX) * tileGeometrySize) + halfWidth;
            double y = fullHeight - ((i / noOfImgX) * tileGeometrySize) - (tileGeometrySize / 2);

            string? imageData = null;

            if (!IsPureTransparent(croppedImage))
            {
                imageData = GetDataUrl(croppedImage);
            }

            return new LevelTile(x, y, tileWidth, tileHeight, imageData);
        }).ToList();

        foreach (var croppedImage in croppedImages)
        {
            croppedImage.Dispose();
        }

        return new LevelDef(tiles);
    }

    private static string GetDataUrl(IMagickImage<ushort> image)
    {
        var imageBytes = image.ToByteArray();
        var asBase64 = Convert.ToBase64String(imageBytes);
        return $"data:image/png;base64,{asBase64}";
    }

    private static void PadImage(IMagickImage<ushort> image)
    {
        //int paddedWidth = PadToBeDivisibleByFour(image.Width);
        //int paddedHeight = PadToBeDivisibleByFour(image.Height);
        //image.Extent(paddedWidth, paddedHeight, MagickColors.Transparent);
        image.Extent(256, 256, MagickColors.Transparent);
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
}
