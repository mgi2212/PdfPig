namespace UglyToad.PdfPig.Tests.Integration
{
    using System.Linq;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
    using Xunit;

    // Other documents that use the fix in CharacterMapBuilder (UnicodeCategory.PrivateUse), unfortunately this string value is not used
    // - MOZILLA-3136-0
    // - Type0_CJK_Font
    // - 68-1990-01_A
    // - Pig Production Handbook (page 31)

    public class CMapTests
    {
        [Fact]
        public void Issue687()
        {
            using (var document = PdfDocument.Open(IntegrationHelpers.GetDocumentPath("340082.pdf")))
            {
                var page = document.GetPage(324);

                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters.Where(l => !string.IsNullOrEmpty(l.Value.Trim())).ToArray());
                var blocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);

                var referencesActual = blocks[2].TextLines[0].Words
                    .SelectMany(x => x.Letters)
                    .Where(l => !string.IsNullOrEmpty(l.Value.Trim()))
                    .Select(l => l.Value)
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
