/*
    private static void Run()
    {
        string inputFilePath = "C:/Users/Filip.Gustavsson/Programming/TestToKtx/bilder/Ångström1-resized.png";
        string outputFilePath = "C:/Users/Filip.Gustavsson/Programming/TestToKtx/bilder/out.ktx2";

        byte[] pngBytes = File.ReadAllBytes(inputFilePath);
        var image = new MagickImage(pngBytes, MagickFormat.Png32);
        PadImage(image);
        var ktx2Bytes = EncodeImage(image);
        SaveKtx2Image(ktx2Bytes, outputFilePath);
    }

    private static void SaveKtx2Image(byte[] ktx2Bytes, string path)
    {
        File.WriteAllBytes(path, ktx2Bytes);
    }

    private static void PadImage(IMagickImage<ushort> image)
    {
        var max = Math.Max(image.Width, image.Height);
        image.Extent(max, max, MagickColors.Transparent);
    }

    private static byte[] EncodeImage(IMagickImage<ushort> image)
    {
        string toktxPath = "toktx";
        string arguments = "--genmipmap --t2 --encode uastc --uastc_quality 4 --uastc_rdo_l .25 --uastc_rdo_d 65536 --zcmp 22 --assign_oetf srgb -";

        var pngBytes = image.ToByteArray();

        using (var process = new Process())
        {
            process.StartInfo.FileName = toktxPath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.ErrorDialog = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;

            process.Start();
            process.StandardInput.BaseStream.Write(pngBytes, 0, pngBytes.Length);
            process.StandardInput.Close();

            byte[] ktx2Bytes;
            using (var ms = new MemoryStream())
            {
                process.StandardOutput.BaseStream.CopyTo(ms);
                ktx2Bytes = ms.ToArray();
            }

            string errorOutput = process.StandardError.ReadToEnd();

            if (errorOutput.Length > 0)
            {
                throw new Exception(errorOutput);
            }

            process.WaitForExit();
            return ktx2Bytes;
        }
    }
*/