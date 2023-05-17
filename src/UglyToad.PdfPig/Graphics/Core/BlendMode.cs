namespace UglyToad.PdfPig.Graphics.Core
{
    /// <summary>
    /// Blend modes.
    /// </summary>
    public enum BlendMode : byte
    {
        // Standard separable blend modes

        /// <summary>
        /// TODO
        /// </summary>
        Normal = 0,

        /// <summary>
        /// TODO
        /// </summary>
        Compatible = 1,

        /// <summary>
        /// TODO
        /// </summary>
        Multiply = 2,

        /// <summary>
        /// TODO
        /// </summary>
        Screen = 3,

        /// <summary>
        /// TODO
        /// </summary>
        Overlay = 4,

        /// <summary>
        /// TODO
        /// </summary>
        Darken = 5,

        /// <summary>
        /// TODO
        /// </summary>
        Lighten = 6,

        /// <summary>
        /// TODO
        /// </summary>
        ColorDodge = 7,

        /// <summary>
        /// TODO
        /// </summary>
        ColorBurn = 8,

        /// <summary>
        /// TODO
        /// </summary>
        HardLight = 9,

        /// <summary>
        /// TODO
        /// </summary>
        SoftLight = 10,

        /// <summary>
        /// TODO
        /// </summary>
        Difference = 11,

        /// <summary>
        /// TODO
        /// </summary>
        Exclusion = 12,

        // Standard nonseparable blend modes

        /// <summary>
        /// TODO
        /// </summary>
        Hue = 13,

        /// <summary>
        /// TODO
        /// </summary>
        Saturation = 14,

        /// <summary>
        /// TODO
        /// </summary>
        Color = 15,

        /// <summary>
        /// TODO
        /// </summary>
        Luminosity = 16
    }
}
