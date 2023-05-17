namespace IccProfileNet
{
    /// <summary>
    /// TODO
    /// </summary>
    public readonly struct IccTagTableItem
    {
        #region ICC Profile tags constants
        internal const int TagCountOffset = 0;
        internal const int TagCountLength = 4;
        internal const int TagSignatureOffset = 4;
        internal const int TagSignatureLength = 4;
        internal const int TagOffsetOffset = 8;
        internal const int TagOffsetLength = 4;
        internal const int TagSizeOffset = 12;
        internal const int TagSizeLength = 4;
        #endregion

        /// <summary>
        /// Tag Signature.
        /// </summary>
        internal string Signature { get; }

        /// <summary>
        /// Offset to beginning of tag data element.
        /// </summary>
        internal uint Offset { get; }

        /// <summary>
        /// Size of tag data element.
        /// </summary>
        internal uint Size { get; }

        /// <summary>
        /// TODO
        /// </summary>
        internal IccTagTableItem(string signature, uint offset, uint size)
        {
            Signature = signature;
            Offset = offset;
            Size = size;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Signature}: offset={Offset}, size={Size}";
        }
    }
}
