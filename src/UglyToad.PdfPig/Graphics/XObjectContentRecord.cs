namespace UglyToad.PdfPig.Graphics
{
    using System;
    using Colors;
    using Core;
    using PdfPig.Core;
    using Tokens;
    using Util.JetBrains.Annotations;
    using XObjects;

    /// <summary>
    /// TODO
    /// </summary>
    public class XObjectContentRecord
    {
        /// <summary>
        /// TODO
        /// </summary>
        public XObjectType Type { get; }

        /// <summary>
        /// TODO
        /// </summary>
        [NotNull]
        public StreamToken Stream { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public TransformationMatrix AppliedTransformation { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public RenderingIntent DefaultRenderingIntent { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public ColorSpace DefaultColorSpace { get; }

        /// <summary>
        /// TODO
        /// </summary>
        internal XObjectContentRecord(XObjectType type, StreamToken stream, TransformationMatrix appliedTransformation,
            RenderingIntent defaultRenderingIntent,
            ColorSpace defaultColorSpace)
        {
            Type = type;
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            AppliedTransformation = appliedTransformation;
            DefaultRenderingIntent = defaultRenderingIntent;
            DefaultColorSpace = defaultColorSpace;
        }
    }
}
