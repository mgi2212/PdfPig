namespace UglyToad.PdfPig.PdfFonts
{
    using Core;

    /// <summary>
    /// TODO
    /// </summary>
    public class CharacterBoundingBox
    {
        /// <summary>
        /// TODO
        /// </summary>
        public PdfRectangle GlyphBounds { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public CharacterBoundingBox(PdfRectangle bounds, double width)
        {
            GlyphBounds = bounds;
            Width = width;
        }
    }
}
