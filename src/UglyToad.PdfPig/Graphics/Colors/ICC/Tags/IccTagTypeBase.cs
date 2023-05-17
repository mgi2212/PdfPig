namespace IccProfileNet.Tags
{
    /// <summary>
    /// Interface for ICC tage type.
    /// </summary>
    public abstract class IccTagTypeBase
    {
        /// <summary>
        /// TODO
        /// </summary>
        public const int TypeSignatureOffset = 0;

        /// <summary>
        /// TODO
        /// </summary>
        public const int TypeSignatureLength = 4;

        /// <summary>
        /// TODO
        /// </summary>
        public const int ReservedOffset = 4;

        /// <summary>
        /// TODO
        /// </summary>
        public const int ReservedLength = 4;

        /*
        /// <summary>
        /// Tag Signature.
        /// </summary>
        public string Signature { get; }
        */

        /// <summary>
        /// Tag raw data.
        /// </summary>
        public byte[] RawData { get; protected set; }
    }
}
