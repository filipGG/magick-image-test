namespace MagickTest;

internal class Program
{
    public const string rootPath = "C:/Users/Filip.Gustavsson/Programming/web-images-performance-test/public/";
    public const string ångström1Png = "Ångström1.png";
    public const string kvarsiten = "A-plan1.png";

    public const string outputFolder = "C:/Users/Filip.Gustavsson/Programming/web-images-performance-test/public/out/";

    static void Main(string[] args)
    {
        Directory.CreateDirectory(outputFolder);
        var bytes = File.ReadAllBytes(rootPath + ångström1Png);

        //FlatGrid.Run(bytes, outputFolder, 512);
        LeveledGrid.Run(bytes, outputFolder);
    }
}
