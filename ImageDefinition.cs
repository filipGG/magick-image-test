namespace MagickTest;

public record ImageDefTile(double X, double Y, double Width, double Height, ImageData? dataUrl);

public record ImageDef(List<ImageDefTile> Tiles);

public record ImageData(ImageDataUrl full, ImageDataUrl half, ImageDataUrl quarter);

public record ImageDataUrl(string Quality, string dataUrl);