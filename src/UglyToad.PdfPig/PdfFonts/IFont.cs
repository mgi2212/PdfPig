namespace UglyToad.PdfPig.PdfFonts
{
    using Core;
    using System.Collections.Generic;
    using Tokens;

    /// <summary>
    /// TODO
    /// </summary>
    public interface IFont
    {
        /// <summary>
        /// TODO
        /// </summary>
        NameToken Name { get; }

        /// <summary>
        /// TODO
        /// </summary>
        bool IsVertical { get; }

        /// <summary>
        /// TODO
        /// </summary>
        FontDetails Details { get; }

        /// <summary>
        /// TODO
        /// </summary>
        int ReadCharacterCode(IInputBytes bytes, out int codeLength);

        /// <summary>
        /// TODO
        /// </summary>
        bool TryGetUnicode(int characterCode, out string value);

        /// <summary>
        /// TODO
        /// </summary>
        CharacterBoundingBox GetBoundingBox(int characterCode);

        /// <summary>
        /// TODO
        /// </summary>
        TransformationMatrix GetFontMatrix();

        /// <summary>
        /// Returns the glyph path for the given character code.
        /// </summary>
        /// <param name="characterCode">Character code in a PDF. Not to be confused with unicode.</param>
        /// <param name="path">The glyph path for the given character code.</param>
        bool TryGetPath(int characterCode, out IReadOnlyList<PdfSubpath> path);

        /// <summary>
        /// Returns the normalised glyph path for the given character code in a PDF.
        /// </summary>
        /// <param name="characterCode">Character code in a PDF. Not to be confused with unicode.</param>
        /// <param name="path">The normalized glyph path for the given character code.</param>
        bool TryGetNormalisedPath(int characterCode, out IReadOnlyList<PdfSubpath> path);
    }
}
