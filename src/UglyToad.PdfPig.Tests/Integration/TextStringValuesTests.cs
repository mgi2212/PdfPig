namespace UglyToad.PdfPig.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
    using Xunit;

    public class TextStringValuesTests
    {
        [Fact]
        public void Issue687()
        {
            using (var document = PdfDocument.Open(IntegrationHelpers.GetDocumentPath("340082.pdf")))
            {
                var page = document.GetPage(324);

                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters.Where(l => !string.IsNullOrEmpty(l.Value.Trim())).ToArray());
                var blocks =  DocstrumBoundingBoxes.Instance.GetBlocks(words);

                var referencesActual = blocks[2].TextLines[0].Words
                    .SelectMany(x => x.Letters)
                    .Where(l => !string.IsNullOrEmpty(l.Value.Trim()))
                    .Select(l=>l.Value)
                    .ToArray();

                string[] expected = new[] { "r", "e", "f", "e", "r", "e", "n", "c", "e", "s" };

                Assert.Equal(expected.Length, referencesActual.Length);

                for (int i = 0; i < expected.Length; i++)
                {
                    Assert.Equal(expected[i], referencesActual[i]);
                }
            }
        }
    }
}
