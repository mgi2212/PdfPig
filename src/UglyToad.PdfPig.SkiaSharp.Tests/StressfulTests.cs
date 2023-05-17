namespace UglyToad.PdfPig.SkiaSharp.Tests
{
    using global::SkiaSharp;
    using System;
    using System.IO;
    using Xunit;

    public class StressfulTests
    {
        private const int mult = 5;

        [Fact]
        public void RunAll()
        {
            const string paths = "C:\\Users\\Bob\\Document Layout Analysis\\stressful corpus";

            foreach (var path in Directory.GetFiles(paths, "*.pdf", SearchOption.AllDirectories))
            {
                RunAllPages(path);
            }
        }

        private static void RunAllPages(string file)
        {
            try
            {
                const string directory = "StressfulTests";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var document = PdfDocument.Open(file, new ParsingOptions { UseLenientParsing = false }))
                {
                    for (var i = 0; i < document.NumberOfPages; i++)
                    {
                        var imageName = $"{Path.GetFileName(file)}_all_{i + 1}_skia-sharp.png";
                        var savePath = Path.Combine(directory, imageName);

                        try
                        {
                            var page = PdfDocument.Open(file, new ParsingOptions { UseLenientParsing = false }).GetPage(i + 1); // TODO - wrong, we reopen every time

                            using (var ms = new SkiaSharpProcessor(page).GetImage(mult))
                            using (Stream s = new FileStream(savePath, FileMode.Create))
                            {
                                var bitmap = SKBitmap.Decode(ms);
                                SKData d = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100);

                                d.SaveTo(s);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"PdfDocument.GetPage {ex}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PdfDocument.Open {ex}");
            }
        }
    }
}
