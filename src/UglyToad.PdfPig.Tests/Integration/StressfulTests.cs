namespace UglyToad.PdfPig.Tests.Integration
{
    using System;
    using System.IO;
    using Xunit;

    public class StressfulTests
    {
        [Fact(Skip = "for debug")]
        public void RunAll()
        {
            const string paths = "C:\\Users\\Bob\\Document Layout Analysis\\stressful corpus";

            foreach (var path in Directory.GetFiles(paths, "*.pdf", SearchOption.AllDirectories))
            {
                try
                {
                    using (var doc = PdfDocument.Open(path))
                    {
                        for (int p = 1; p <= doc.NumberOfPages; p++)
                        {
                            try
                            {
                                var page = doc.GetPage(p);
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
}
