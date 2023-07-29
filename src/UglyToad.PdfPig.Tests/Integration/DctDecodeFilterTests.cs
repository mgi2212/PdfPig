namespace UglyToad.PdfPig.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UglyToad.PdfPig.Tokens;
    using Xunit;

    public class DctDecodeFilterTests
    {
        [Fact]
        public void LettersHaveCorrectColors()
        {
            using (var document = PdfDocument.Open(IntegrationHelpers.GetDocumentPath("Pig Production Handbook.pdf"), ParsingOptions.LenientParsingOff))
            {
                for (int p = 1; p <= document.NumberOfPages; p++)
                {
                    var page = document.GetPage(p);
                    int i = 0;
                    foreach (var image in page.GetImages())
                    {
                        if (image.ImageDictionary.TryGet<NameToken>(NameToken.Filter, out var filter) && filter.Data.Equals(NameToken.DctDecode))
                        {
                            image.TryGetPng(out var png);
                            File.WriteAllBytes($"Pig Production Handbook_{p}_{i}.png", png);
                        }
             
                        i++;
                    }
                }
            }
        }
    }
}
