namespace UglyToad.PdfPig.Graphics.Colors
{
    using Tokens;

    /// <summary>
    /// A color space definition from a resource dictionary.
    /// </summary>
    public struct ResourceColorSpace
    {
        /// <summary>
        /// TODO
        /// </summary>
        public NameToken Name { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public IToken Data { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public ResourceColorSpace(NameToken name, IToken data)
        {
            Name = name;
            Data = data;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ResourceColorSpace(NameToken name) : this(name, null) { }
    }
}
