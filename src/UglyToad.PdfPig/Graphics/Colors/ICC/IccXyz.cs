namespace IccProfileNet
{
    internal struct IccXyz
    {
        /// <summary>
        /// X.
        /// </summary>
        internal double X { get; }

        /// <summary>
        /// Y.
        /// </summary>
        internal double Y { get; }

        /// <summary>
        /// Z.
        /// </summary>
        internal double Z { get; }

        internal IccXyz(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }
    }
}
