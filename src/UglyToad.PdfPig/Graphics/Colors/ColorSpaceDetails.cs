namespace UglyToad.PdfPig.Graphics.Colors
{
    using IccProfileNet;
    using IccProfileNet.Tags;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Tokens;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Functions;
    using UglyToad.PdfPig.Images;
    using UglyToad.PdfPig.Images.Png;
    using UglyToad.PdfPig.Util;
    using UglyToad.PdfPig.Util.JetBrains.Annotations;

    // TODO - SHould not use PngBuilded??

    /// <summary>
    /// Contains more document-specific information about the <see cref="ColorSpace"/>.
    /// </summary>
    public abstract class ColorSpaceDetails
    {
        /// <summary>
        /// The type of the ColorSpace.
        /// </summary>
        public ColorSpace Type { get; }

        /// <summary>
        /// The underlying type of ColorSpace, usually equal to <see cref="Type"/>
        /// unless <see cref="ColorSpace.Indexed"/>.
        /// </summary>
        public ColorSpace BaseType { get; protected set; }

        /// <summary>
        /// Create a new <see cref="ColorSpaceDetails"/>.
        /// </summary>
        protected ColorSpaceDetails(ColorSpace type)
        {
            Type = type;
            BaseType = type;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="values"></param>
        public abstract IColor GetColor(params double[] values);

        /// <summary>
        /// TODO
        /// </summary>
        public abstract byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel);

        /// <summary>
        /// Get the color that initialize the current stroking or nonstroking colour.
        /// </summary>
        public abstract IColor GetInitializeColor();

        /// <summary>
        /// The number of components for the color space.
        /// </summary>
        public abstract int NumberOfColorComponents { get; }

        /// <summary>
        /// Convert to byte.
        /// </summary>
        protected static byte ConvertToByte(decimal componentValue)
        {
            var rounded = Math.Round(componentValue * 255, MidpointRounding.AwayFromZero);
            return (byte)rounded;
        }
    }

    /// <summary>
    /// A grayscale value is represented by a single number in the range 0.0 to 1.0,
    /// where 0.0 corresponds to black, 1.0 to white, and intermediate values to different gray levels.
    /// </summary>
    public sealed class DeviceGrayColorSpaceDetails : ColorSpaceDetails
    {
        /// <summary>
        /// The single instance of the <see cref="DeviceGrayColorSpaceDetails"/>.
        /// </summary>
        public static readonly DeviceGrayColorSpaceDetails Instance = new DeviceGrayColorSpaceDetails();

        private DeviceGrayColorSpaceDetails() : base(ColorSpace.DeviceGray)
        {
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            if (values == null || values.Length != 1)
            {
                throw new ArgumentException(nameof(values));
            }

            double gray = values[0];
            if (gray == 0)
            {
                return GrayColor.Black;
            }
            else if (gray == 1)
            {
                return GrayColor.White;
            }
            else
            {
                return new GrayColor((decimal)gray);
            }
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            return GrayColor.Black;
        }

        /// <inheritdoc/>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            var builder = PngBuilder.Create(widthInSamples, heightInSamples, hasAlphaChannel);
            int i = 0; // To remove, use col and row to get index
            for (var col = 0; col < heightInSamples; col++)
            {
                for (var row = 0; row < widthInSamples; row++)
                {
                    var pixel = bytesPure[i++];
                    builder.SetPixel(pixel, pixel, pixel, row, col);
                }
            }
            return builder.Save();
        }

        /// <inheritdoc/>
        public override int NumberOfColorComponents => 1;
    }

    /// <summary>
    /// Color values are defined by three components representing the intensities of the additive primary colorants red, green and blue.
    /// Each component is specified by a number in the range 0.0 to 1.0, where 0.0 denotes the complete absence of a primary component and 1.0 denotes maximum intensity.
    /// </summary>
    public sealed class DeviceRgbColorSpaceDetails : ColorSpaceDetails
    {
        /// <summary>
        /// The single instance of the <see cref="DeviceRgbColorSpaceDetails"/>.
        /// </summary>
        public static readonly DeviceRgbColorSpaceDetails Instance = new DeviceRgbColorSpaceDetails();

        private DeviceRgbColorSpaceDetails() : base(ColorSpace.DeviceRGB)
        {
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            if (values == null || values.Length != 3)
            {
                throw new ArgumentException(nameof(values));
            }

            double r = values[0];
            double g = values[1];
            double b = values[2];
            if (r == 0 && g == 0 && b == 0)
            {
                return RGBColor.Black;
            }
            else if (r == 1 && g == 1 && b == 1)
            {
                return RGBColor.White;
            }

            return new RGBColor((decimal)r, (decimal)g, (decimal)b);
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            return RGBColor.Black;
        }

        /// <inheritdoc/>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            var builder = PngBuilder.Create(widthInSamples, heightInSamples, hasAlphaChannel);
            int i = 0; // To remove, use col and row to get index
            for (var col = 0; col < heightInSamples; col++)
            {
                for (var row = 0; row < widthInSamples; row++)
                {
                    builder.SetPixel(bytesPure[i++], bytesPure[i++], bytesPure[i++], row, col);
                }
            }
            return builder.Save();
        }

        /// <inheritdoc/>
        public override int NumberOfColorComponents => 3;
    }

    /// <summary>
    /// Color values are defined by four components cyan, magenta, yellow and black
    /// </summary>
    public sealed class DeviceCmykColorSpaceDetails : ColorSpaceDetails
    {
        /// <summary>
        /// The single instance of the <see cref="DeviceCmykColorSpaceDetails"/>.
        /// </summary>
        public static readonly DeviceCmykColorSpaceDetails Instance = new DeviceCmykColorSpaceDetails();

        private DeviceCmykColorSpaceDetails() : base(ColorSpace.DeviceCMYK)
        {
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            if (values == null || values.Length != 4)
            {
                throw new ArgumentException(nameof(values));
            }

            double c = values[0];
            double m = values[1];
            double y = values[2];
            double k = values[3];
            if (c == 0 && m == 0 && y == 0 && k == 1)
            {
                return CMYKColor.Black;
            }
            else if (c == 0 && m == 0 && y == 0 && k == 0)
            {
                return CMYKColor.White;
            }

            return new CMYKColor((decimal)c, (decimal)m, (decimal)y, (decimal)k);
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            return CMYKColor.Black;
        }

        /// <inheritdoc/>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            var builder = PngBuilder.Create(widthInSamples, heightInSamples, hasAlphaChannel);
            int i = 0; // To remove, use col and row to get index
            for (var col = 0; col < heightInSamples; col++)
            {
                for (var row = 0; row < widthInSamples; row++)
                {
                    // TODO: put conversion in CMYKColor as static
                    // Where CMYK in 0..1
                    // R = 255 × (1-C) × (1-K)
                    // G = 255 × (1-M) × (1-K)
                    // B = 255 × (1-Y) × (1-K)

                    var c = bytesPure[i++] / 255d;
                    var m = bytesPure[i++] / 255d;
                    var y = bytesPure[i++] / 255d;
                    var k = bytesPure[i++] / 255d;
                    var r = ConvertToByte((decimal)((1 - c) * (1 - k)));
                    var g = ConvertToByte((decimal)((1 - m) * (1 - k)));
                    var b = ConvertToByte((decimal)((1 - y) * (1 - k)));

                    builder.SetPixel(r, g, b, row, col);
                }
            }
            return builder.Save();
        }

        /// <inheritdoc/>
        public override int NumberOfColorComponents => 4;
    }

    /// <summary>
    /// An Indexed color space allows a PDF content stream to use small integers as indices into a color map or color table of arbitrary colors in some other space.
    /// A PDF consumer treats each sample value as an index into the color table and uses the color value it finds there.
    /// </summary>
    public sealed class IndexedColorSpaceDetails : ColorSpaceDetails
    {
        /// <summary>
        /// Creates a indexed color space useful for exracting stencil masks as black-and-white images,
        /// i.e. with a color palette of two colors (black and white). If the decode parameter array is
        /// [0, 1] it indicates that black is at index 0 in the color palette, whereas [1, 0] indicates
        /// that the black color is at index 1.
        /// </summary>
        internal static ColorSpaceDetails Stencil(ColorSpaceDetails colorSpaceDetails, decimal[] decode)
        {
            var blackIsOne = decode.Length >= 2 && decode[0] == 1 && decode[1] == 0;
            return new IndexedColorSpaceDetails(colorSpaceDetails, 1, blackIsOne ? new byte[] { 255, 0 } : new byte[] { 0, 255 });
        }

        /// <summary>
        /// The base color space in which the values in the color table are to be interpreted.
        /// It can be any device or CIE-based color space or(in PDF 1.3) a Separation or DeviceN space,
        /// but not a Pattern space or another Indexed space.
        /// </summary>
        public ColorSpaceDetails BaseColorSpaceDetails { get; }

        /// <summary>
        /// An integer that specifies the maximum valid index value. Can be no greater than 255.
        /// </summary>
        public byte HiVal { get; }

        /// <summary>
        /// Provides the mapping between index values and the corresponding colors in the base color space.
        /// </summary>
        public IReadOnlyList<byte> ColorTable { get; }

        /// <summary>
        /// Create a new <see cref="IndexedColorSpaceDetails"/>.
        /// </summary>
        public IndexedColorSpaceDetails(ColorSpaceDetails baseColorSpaceDetails, byte hiVal, IReadOnlyList<byte> colorTable)
            : base(ColorSpace.Indexed)
        {
            BaseColorSpaceDetails = baseColorSpaceDetails ?? throw new ArgumentNullException(nameof(baseColorSpaceDetails));
            HiVal = hiVal;
            ColorTable = colorTable;
            BaseType = baseColorSpaceDetails.BaseType;
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            if (values.Length > 1)
            {
                // TODO - not the correct way
                return BaseColorSpaceDetails.GetColor(values);
            }

            var csBytes = ColorSpaceDetailsByteConverter.UnwrapIndexedColorSpaceBytes(this, values.Select(d => (byte)d).ToArray());
            return BaseColorSpaceDetails.GetColor(csBytes.Select(b => b / 255.0).ToArray());
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            // Setting the current stroking or nonstroking colour space to an Indexed colour space shall
            // initialize the corresponding current colour to 0.
            return GetColor(0);
        }

        /// <inheritdoc/>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            var builder = PngBuilder.Create(widthInSamples, heightInSamples, hasAlphaChannel);
            int i = 0; // To remove, use col and row to get index
            for (var col = 0; col < heightInSamples; col++)
            {
                for (var row = 0; row < widthInSamples; row++)
                {
                    var (r, g, b) = GetColor(bytesPure[i++]).ToRGBValues();
                    builder.SetPixel(ConvertToByte(r), ConvertToByte(g), ConvertToByte(b), row, col);
                }
            }
            return builder.Save();
        }

        /// <inheritdoc/>
        public override int NumberOfColorComponents => 1;
    }

    /// <summary>
    /// TODO
    /// </summary>
    public sealed class DeviceNColorSpaceDetails : ColorSpaceDetails
    {
        /// <summary>
        /// Specifies the name of the colorant that this Separation color space is intended to represent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The special colorant name All refers collectively to all colorants available on an output device,
        /// including those for the standard process colorants.
        /// </para>
        /// <para>
        /// The special colorant name None never produces any visible output.
        /// Painting operations in a Separation space with this colorant name have no effect on the current page.
        /// </para>
        /// </remarks>
        public IReadOnlyList<NameToken> Names { get; }

        /// <summary>
        /// If the colorant name associated with a Separation color space does not correspond to a colorant available on the device,
        /// the application arranges for subsequent painting operations to be performed in an alternate color space.
        /// The intended colors can be approximated by colors in a device or CIE-based color space
        /// which are then rendered with the usual primary or process colorants.
        /// </summary>
        public ColorSpaceDetails AlternateColorSpaceDetails { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public DeviceNColorSpaceAttributes? Attributes { get; }

        /// <summary>
        /// During subsequent painting operations, an application calls this function to transform a tint value into
        /// color component values in the alternate color space.
        /// The function is called with the tint value and must return the corresponding color component values.
        /// That is, the number of components and the interpretation of their values depend on the <see cref="AlternateColorSpaceDetails"/>.
        /// </summary>
        private readonly PdfFunction func;

        private readonly Dictionary<string, IColor> lookupTable = new Dictionary<string, IColor>()
        {
            // TODO - not always RGBColor type?
            { "Red",    new RGBColor(1, 0, 0) },
            { "Green",  new RGBColor(0, 1, 0) },
            { "Blue",   new RGBColor(0, 0, 1) },
            { "Black",  RGBColor.Black },
            { "White",  RGBColor.White },

            // Special names
            // The special colorant name All shall refer collectively to all colorants available on an output device,
            // including those for the standard process colorants. When a Separation space with this colorant name is
            // the current colour space, painting operators shall apply tint values to all available colorants at once.
            { "All", null }, // TODO

            // The special colorant name None shall not produce any visible output. Painting operations in a
            // Separationspace with this colorant name shall have no effect on the current page.
            { "None", null } // TODO
        };

        /// <summary>
        /// Create a new <see cref="SeparationColorSpaceDetails"/>.
        /// </summary>
        public DeviceNColorSpaceDetails(IReadOnlyList<NameToken> names, ColorSpaceDetails alternateColorSpaceDetails,
            PdfFunction tintFunction, DeviceNColorSpaceAttributes? attributes = null)
            : base(ColorSpace.DeviceN)
        {
            Names = names;
            NumberOfColorComponents = Names.Count; // TODO - To check
            AlternateColorSpaceDetails = alternateColorSpaceDetails;
            Attributes = attributes; // TODO - use attributes
            func = tintFunction;
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            // TODO - Named colors

            // TODO - caching
            var evaled = func.Eval(values);
            return AlternateColorSpaceDetails.GetColor(evaled);
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            // When this space is set to the current colour space (using the CS or cs operators), each component
            // shall be given an initial value of 1.0. The SCN and scn operators respectively shall set the current
            // stroking and nonstroking colour.
            return GetColor(Enumerable.Repeat(1.0, NumberOfColorComponents).ToArray());
        }

        /// <inheritdoc/>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            var builder = PngBuilder.Create(widthInSamples, heightInSamples, hasAlphaChannel);
            int i = 0; // To remove, use col and row to get index
            for (var col = 0; col < heightInSamples; col++)
            {
                for (var row = 0; row < widthInSamples; row++)
                {
                    double[] comps = new double[NumberOfColorComponents];
                    for (int n = 0; n < NumberOfColorComponents; n++)
                    {
                        comps[n] = bytesPure[i++] / 255.0; // Do we want to divide by 255?
                    }
                    var (r, g, b) = GetColor(comps).ToRGBValues();
                    builder.SetPixel(ConvertToByte(r), ConvertToByte(g), ConvertToByte(b), row, col);
                }
            }
            return builder.Save();
        }

        /// <summary>
        /// <inheritdoc/>
        /// <para>The 'N' in DeviceN.</para>
        /// </summary>
        public override int NumberOfColorComponents { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public struct DeviceNColorSpaceAttributes
        {
            /// <summary>
            /// Subtype - NameToken - Optional - Default value: DeviceN.
            /// </summary>
            public NameToken Subtype { get; }

            /// <summary>
            /// Colorants - dictionary - Required if Subtype is NChannel and the colour space includes spot colorants; otherwise optional
            /// </summary>
            public DictionaryToken Colorants { get; }

            /// <summary>
            /// Process - dictionary - Required if Subtype is NChannel and the colour space includes components of a process colour space, otherwise optional
            /// </summary>
            public DictionaryToken Process { get; }

            /// <summary>
            /// MixingHints - dictionary - Optional
            /// </summary>
            public DictionaryToken MixingHints { get; }

            /// <summary>
            /// TODO
            /// </summary>
            public DeviceNColorSpaceAttributes()
            {
                Subtype = NameToken.DeviceN;
                Colorants = null;
                Process = null;
                MixingHints = null;
            }

            /// <summary>
            /// TODO
            /// </summary>
            public DeviceNColorSpaceAttributes(NameToken subtype, DictionaryToken colorants, DictionaryToken process, DictionaryToken mixingHints)
            {
                Subtype = subtype;
                Colorants = colorants;
                Process = process;
                MixingHints = mixingHints;
            }
        }
    }

    /// <summary>
    /// A Separation color space provides a means for specifying the use of additional colorants or
    /// for isolating the control of individual color components of a device color space for a subtractive device.
    /// When such a space is the current color space, the current color is a single-component value, called a tint,
    /// that controls the application of the given colorant or color components only.
    /// </summary>
    public sealed class SeparationColorSpaceDetails : ColorSpaceDetails
    {
        /// <summary>
        /// Specifies the name of the colorant that this Separation color space is intended to represent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The special colorant name All refers collectively to all colorants available on an output device,
        /// including those for the standard process colorants.
        /// </para>
        /// <para>
        /// The special colorant name None never produces any visible output.
        /// Painting operations in a Separation space with this colorant name have no effect on the current page.
        /// </para>
        /// </remarks>
        public NameToken Name { get; }

        private readonly IColor namedColor;

        /// <summary>
        /// If the colorant name associated with a Separation color space does not correspond to a colorant available on the device,
        /// the application arranges for subsequent painting operations to be performed in an alternate color space.
        /// The intended colors can be approximated by colors in a device or CIE-based color space
        /// which are then rendered with the usual primary or process colorants.
        /// </summary>
        public ColorSpaceDetails AlternateColorSpaceDetails { get; }

        /// <summary>
        /// During subsequent painting operations, an application calls this function to transform a tint value into
        /// color component values in the alternate color space.
        /// The function is called with the tint value and must return the corresponding color component values.
        /// That is, the number of components and the interpretation of their values depend on the <see cref="AlternateColorSpaceDetails"/>.
        /// </summary>
        public PdfFunction TintFunction { get; }

        private readonly Dictionary<string, IColor> lookupTable = new Dictionary<string, IColor>()
        {
            // TODO - not always RGBColor type
            { "Red",    new RGBColor(1, 0, 0) },
            { "Green",  new RGBColor(0, 1, 0) },
            { "Blue",   new RGBColor(0, 0, 1) },
            { "Black",  RGBColor.Black },
            { "White",  RGBColor.White },

            // Special names
            // The special colorant name All shall refer collectively to all colorants available on an output device,
            // including those for the standard process colorants. When a Separation space with this colorant name is
            // the current colour space, painting operators shall apply tint values to all available colorants at once.
            { "All", null }, // TODO

            // The special colorant name None shall not produce any visible output. Painting operations in a
            // Separationspace with this colorant name shall have no effect on the current page.
            { "None", null }
        };

        /// <summary>
        /// Create a new <see cref="SeparationColorSpaceDetails"/>.
        /// </summary>
        public SeparationColorSpaceDetails(NameToken name,
            ColorSpaceDetails alternateColorSpaceDetails,
            PdfFunction tintFunction)
            : base(ColorSpace.Separation)
        {
            Name = name;
            AlternateColorSpaceDetails = alternateColorSpaceDetails;

            if (lookupTable.TryGetValue(name, out var lookup))
            {
                namedColor = lookup;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Unknown color name '{Name.Data}'");
            }
            TintFunction = tintFunction;
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            if (namedColor != null)
            {
                return namedColor; // TODO - check if correct
            }

            // TODO - caching
            var evaled = TintFunction.Eval(values);
            return AlternateColorSpaceDetails.GetColor(evaled);
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            // The initial value for both the stroking and nonstroking colour in the graphics state shall be 1.0.
            return GetColor(1.0);
        }

        /// <inheritdoc/>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            var builder = PngBuilder.Create(widthInSamples, heightInSamples, hasAlphaChannel);
            int i = 0; // To remove, use col and row to get index
            for (var col = 0; col < heightInSamples; col++)
            {
                for (var row = 0; row < widthInSamples; row++)
                {
                    var (r, g, b) = GetColor(bytesPure[i++] / 255.0).ToRGBValues(); // Do we want to divide by 255?
                    builder.SetPixel(ConvertToByte(r), ConvertToByte(g), ConvertToByte(b), row, col);
                }
            }
            return builder.Save();
        }

        /// <inheritdoc/>
        public override int NumberOfColorComponents => 1; // TODO - To check
    }

    /// <summary>
    /// CIE (Commission Internationale de l'Éclairage) colorspace.
    /// Specifies color related to human visual perception with the aim of producing consistent color on different output devices.
    /// CalGray - A CIE A color space with a single transformation.
    /// A represents the gray component of a calibrated gray space. The component must be in the range 0.0 to 1.0.
    /// </summary>
    public sealed class CalGrayColorSpaceDetails : ColorSpaceDetails
    {
        private readonly CIEBasedColorSpaceTransformer colorSpaceTransformer;
        /// <summary>
        /// An array of three numbers [XW  YW  ZW] specifying the tristimulus value, in the CIE 1931 XYZ space of the
        /// diffuse white point. The numbers XW and ZW shall be positive, and YW shall be equal to 1.0.
        /// </summary>
        public IReadOnlyList<decimal> WhitePoint { get; }

        /// <summary>
        /// An array of three numbers [XB  YB  ZB] specifying the tristimulus value, in the CIE 1931 XYZ space of the
        /// diffuse black point. All three numbers must be non-negative. Default value: [0.0  0.0  0.0].
        /// </summary>
        public IReadOnlyList<decimal> BlackPoint { get; }

        /// <summary>
        /// A number defining the gamma for the gray (A) component. Gamma must be positive and is generally
        /// greater than or equal to 1. Default value: 1.
        /// </summary>
        public decimal Gamma { get; }

        /// <summary>
        /// Create a new <see cref="CalGrayColorSpaceDetails"/>.
        /// </summary>
        public CalGrayColorSpaceDetails([NotNull] IReadOnlyList<decimal> whitePoint, [CanBeNull] IReadOnlyList<decimal> blackPoint, decimal? gamma)
            : base(ColorSpace.CalGray)
        {
            WhitePoint = whitePoint ?? throw new ArgumentNullException(nameof(whitePoint));
            if (WhitePoint.Count != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(whitePoint), whitePoint, $"Must consist of exactly three numbers, but was passed {whitePoint.Count}.");
            }

            BlackPoint = blackPoint ?? new[] { 0m, 0, 0 };
            if (BlackPoint.Count != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(blackPoint), blackPoint, $"Must consist of exactly three numbers, but was passed {blackPoint.Count}.");
            }

            Gamma = gamma ?? 1m;

            colorSpaceTransformer =
                new CIEBasedColorSpaceTransformer(((double)WhitePoint[0], (double)WhitePoint[1], (double)WhitePoint[2]), RGBWorkingSpace.sRGB)
                {
                    DecoderABC = color => (
                    Math.Pow(color.A, (double)Gamma),
                    Math.Pow(color.B, (double)Gamma),
                    Math.Pow(color.C, (double)Gamma)),

                    MatrixABC = new Matrix3x3(
                    (double)WhitePoint[0], 0, 0,
                    0, (double)WhitePoint[1], 0,
                    0, 0, (double)WhitePoint[2])
                };
        }

        /// <summary>
        /// Transforms the supplied A color to grayscale RGB (sRGB) using the properties of this
        /// <see cref="CalGrayColorSpaceDetails"/> in the transformation process.
        /// A represents the gray component of a calibrated gray space. The component must be in the range 0.0 to 1.0.
        /// </summary>
        internal RGBColor TransformToRGB(double colorA)
        {
            var (R, G, B) = colorSpaceTransformer.TransformToRGB((colorA, colorA, colorA));
            return new RGBColor((decimal)R, (decimal)G, (decimal)B);
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            if (values == null || values.Length != 1)
            {
                throw new ArgumentException(nameof(values));
            }

            return TransformToRGB(values[0]);
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            // Setting the current stroking or nonstroking colour space to any CIE-based colour space shall
            // initialize all components of the corresponding current colour to 0.0 (unless the range of valid
            // values for a given component does not include 0.0, in which case the nearest valid value shall
            // be substituted.)
            return TransformToRGB(0);
        }

        /// <inheritdoc/>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            var builder = PngBuilder.Create(widthInSamples, heightInSamples, hasAlphaChannel);
            int i = 0; // To remove, use col and row to get index
            for (var col = 0; col < heightInSamples; col++)
            {
                for (var row = 0; row < widthInSamples; row++)
                {
                    var (r, g, b) = GetColor(bytesPure[i++] / 255.0).ToRGBValues();
                    builder.SetPixel(ConvertToByte(r), ConvertToByte(g), ConvertToByte(b), row, col);
                }
            }
            return builder.Save();
        }

        /// <inheritdoc/>
        public override int NumberOfColorComponents => 1;
    }

    /// <summary>
    /// CIE (Commission Internationale de l'Éclairage) colorspace.
    /// Specifies color related to human visual perception with the aim of producing consistent color on different output devices.
    /// CalRGB - A CIE ABC color space with a single transformation.
    /// A, B and C represent red, green and blue color values in the range 0.0 to 1.0.
    /// </summary>
    public sealed class CalRGBColorSpaceDetails : ColorSpaceDetails
    {
        private readonly CIEBasedColorSpaceTransformer colorSpaceTransformer;

        /// <summary>
        /// An array of three numbers [XW  YW  ZW] specifying the tristimulus value, in the CIE 1931 XYZ space of the
        /// diffuse white point. The numbers XW and ZW shall be positive, and YW shall be equal to 1.0.
        /// </summary>
        public IReadOnlyList<decimal> WhitePoint { get; }

        /// <summary>
        /// An array of three numbers [XB  YB  ZB] specifying the tristimulus value, in the CIE 1931 XYZ space of the
        /// diffuse black point. All three numbers must be non-negative. Default value: [0.0  0.0  0.0].
        /// </summary>
        public IReadOnlyList<decimal> BlackPoint { get; }

        /// <summary>
        /// An array of three numbers [GR  GG  GB] specifying the gamma for the red, green and blue (A, B, C) components
        /// of the color space. Default value: [1.0  1.0  1.0].
        /// </summary>
        public IReadOnlyList<decimal> Gamma { get; }

        /// <summary>
        /// An array of nine numbers [XA  YA  ZA  XB  YB  ZB  XC  YC  ZC] specifying the linear interpretation of the
        /// decoded A, B, C components of the color space with respect to the final XYZ representation. Default value:
        /// [1  0  0  0  1  0  0  0  1].
        /// </summary>
        public IReadOnlyList<decimal> Matrix { get; }

        /// <summary>
        /// Create a new <see cref="CalRGBColorSpaceDetails"/>.
        /// </summary>
        public CalRGBColorSpaceDetails([NotNull] IReadOnlyList<decimal> whitePoint, [CanBeNull] IReadOnlyList<decimal> blackPoint, [CanBeNull] IReadOnlyList<decimal> gamma, [CanBeNull] IReadOnlyList<decimal> matrix)
            : base(ColorSpace.CalRGB)
        {
            WhitePoint = whitePoint ?? throw new ArgumentNullException(nameof(whitePoint));
            if (WhitePoint.Count != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(whitePoint), whitePoint, $"Must consist of exactly three numbers, but was passed {whitePoint.Count}.");
            }

            BlackPoint = blackPoint ?? new[] { 0m, 0, 0 };
            if (BlackPoint.Count != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(blackPoint), blackPoint, $"Must consist of exactly three numbers, but was passed {blackPoint.Count}.");
            }

            Gamma = gamma ?? new[] { 1m, 1, 1 };
            if (Gamma.Count != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(gamma), gamma, $"Must consist of exactly three numbers, but was passed {gamma.Count}.");
            }

            Matrix = matrix ?? new[] { 1m, 0, 0, 0, 1, 0, 0, 0, 1 };
            if (Matrix.Count != 9)
            {
                throw new ArgumentOutOfRangeException(nameof(matrix), matrix, $"Must consist of exactly nine numbers, but was passed {matrix.Count}.");
            }

            colorSpaceTransformer =
                new CIEBasedColorSpaceTransformer(((double)WhitePoint[0], (double)WhitePoint[1], (double)WhitePoint[2]), RGBWorkingSpace.sRGB)
                {
                    DecoderABC = color => (
                    Math.Pow(color.A, (double)Gamma[0]),
                    Math.Pow(color.B, (double)Gamma[1]),
                    Math.Pow(color.C, (double)Gamma[2])),

                    MatrixABC = new Matrix3x3(
                    (double)Matrix[0], (double)Matrix[3], (double)Matrix[6],
                    (double)Matrix[1], (double)Matrix[4], (double)Matrix[7],
                    (double)Matrix[2], (double)Matrix[5], (double)Matrix[8])
                };
        }

        /// <summary>
        /// Transforms the supplied ABC color to RGB (sRGB) using the properties of this <see cref="CalRGBColorSpaceDetails"/>
        /// in the transformation process.
        /// A, B and C represent red, green and blue calibrated color values in the range 0.0 to 1.0. 
        /// </summary>
        internal RGBColor TransformToRGB((double A, double B, double C) colorAbc)
        {
            var (R, G, B) = colorSpaceTransformer.TransformToRGB((colorAbc.A, colorAbc.B, colorAbc.C));
            return new RGBColor((decimal)R, (decimal)G, (decimal)B);
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            return TransformToRGB((values[0], values[1], values[2]));
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            // Setting the current stroking or nonstroking colour space to any CIE-based colour space shall
            // initialize all components of the corresponding current colour to 0.0 (unless the range of valid
            // values for a given component does not include 0.0, in which case the nearest valid value shall
            // be substituted.)
            return TransformToRGB((0, 0, 0));
        }

        /// <inheritdoc/>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            var builder = PngBuilder.Create(widthInSamples, heightInSamples, hasAlphaChannel);
            int i = 0; // To remove, use col and row to get index
            for (var col = 0; col < heightInSamples; col++)
            {
                for (var row = 0; row < widthInSamples; row++)
                {
                    var (r, g, b) = GetColor(bytesPure[i++] / 255.0, bytesPure[i++] / 255.0, bytesPure[i++] / 255.0).ToRGBValues();
                    builder.SetPixel(ConvertToByte(r), ConvertToByte(g), ConvertToByte(b), row, col);
                }
            }
            return builder.Save();
        }

        /// <inheritdoc/>
        public override int NumberOfColorComponents => 3;
    }

    /// <summary>
    /// CIE (Commission Internationale de l'Éclairage) colorspace.
    /// Specifies color related to human visual perception with the aim of producing consistent color on different output devices.
    /// CalRGB - A CIE ABC color space with a single transformation.
    /// A, B and C represent red, green and blue color values in the range 0.0 to 1.0.
    /// </summary>
    public sealed class LabColorSpaceDetails : ColorSpaceDetails
    {
        private readonly CIEBasedColorSpaceTransformer colorSpaceTransformer;

        /// <summary>
        /// An array of three numbers [XW  YW  ZW] specifying the tristimulus value, in the CIE 1931 XYZ space of the
        /// diffuse white point. The numbers XW and ZW shall be positive, and YW shall be equal to 1.0.
        /// </summary>
        public IReadOnlyList<double> WhitePoint { get; }

        /// <summary>
        /// An array of three numbers [XB  YB  ZB] specifying the tristimulus value, in the CIE 1931 XYZ space of the
        /// diffuse black point. All three numbers must be non-negative. Default value: [0.0  0.0  0.0].
        /// </summary>
        public IReadOnlyList<double> BlackPoint { get; }

        /// <summary>
        /// An array of four numbers [a_min a_max b_min b_max] that shall specify the range of valid values for the a* and b* (B and C)
        /// components of the colour space — that is, a_min ≤ a* ≤ a_max and b_min ≤ b* ≤ b_max
        /// <para>Component values falling outside the specified range shall be adjusted to the nearest valid value without error indication.</para>
        /// Default value: [−100 100 −100 100].
        /// </summary>
        public IReadOnlyList<double> Matrix { get; }

        /// <summary>
        /// Create a new <see cref="LabColorSpaceDetails"/>.
        /// </summary>
        public LabColorSpaceDetails([NotNull] IReadOnlyList<decimal> whitePoint, [CanBeNull] IReadOnlyList<decimal> blackPoint, [CanBeNull] IReadOnlyList<decimal> matrix)
            : base(ColorSpace.Lab)
        {
            WhitePoint = whitePoint?.Select(v => (double)v).ToArray() ?? throw new ArgumentNullException(nameof(whitePoint));
            if (WhitePoint.Count != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(whitePoint), whitePoint, $"Must consist of exactly three numbers, but was passed {whitePoint.Count}.");
            }

            BlackPoint = blackPoint?.Select(v => (double)v).ToArray() ?? new[] { 0.0, 0.0, 0.0 };
            if (BlackPoint.Count != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(blackPoint), blackPoint, $"Must consist of exactly three numbers, but was passed {blackPoint.Count}.");
            }

            Matrix = matrix?.Select(v => (double)v).ToArray() ?? new[] { -100.0, 100.0, -100.0, 100.0 };
            if (Matrix.Count != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(matrix), matrix, $"Must consist of exactly four numbers, but was passed {matrix.Count}.");
            }

            colorSpaceTransformer = new CIEBasedColorSpaceTransformer((WhitePoint[0], WhitePoint[1], WhitePoint[2]), RGBWorkingSpace.sRGB);
        }

        /// <summary>
        /// Transforms the supplied ABC color to RGB (sRGB) using the properties of this <see cref="LabColorSpaceDetails"/>
        /// in the transformation process.
        /// A, B and C represent the L*, a*, and b* components of a CIE 1976 L*a*b* space. The range of the first (L*)
        /// component shall be 0 to 100; the ranges of the second and third (a* and b*) components shall be defined by
        /// the Range entry in the colour space dictionary
        /// </summary>
        internal RGBColor TransformToRGB((double A, double B, double C) colorAbc)
        {
            // Component Ranges: L*: [0 100]; a* and b*: [−128 127]
            double b = PdfFunction.ClipToRange((double)colorAbc.B, (double)Matrix[0], (double)Matrix[1]);
            double c = PdfFunction.ClipToRange((double)colorAbc.C, (double)Matrix[2], (double)Matrix[3]);

            double M = ((double)colorAbc.A + 16.0) / 116.0;
            double L = M + (b / 500.0);
            double N = M - (c / 200.0);

            double X = WhitePoint[0] * g(L);
            double Y = WhitePoint[1] * g(M);
            double Z = WhitePoint[2] * g(N);

            var (R, G, B) = colorSpaceTransformer.TransformToRGB((X, Y, Z));
            return new RGBColor((decimal)R, (decimal)G, (decimal)B);
        }

        private static double g(double x)
        {
            if (x > 6.0 / 29.0)
            {
                return x * x * x;
            }
            return 108.0 / 841.0 * (x - 4.0 / 29.0);
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            return TransformToRGB((values[0], values[1], values[2]));
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            // Setting the current stroking or nonstroking colour space to any CIE-based colour space shall
            // initialize all components of the corresponding current colour to 0.0 (unless the range of valid
            // values for a given component does not include 0.0, in which case the nearest valid value shall
            // be substituted.)
            return TransformToRGB((0, 0, 0));
        }

        /// <inheritdoc/>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            var builder = PngBuilder.Create(widthInSamples, heightInSamples, hasAlphaChannel);
            int i = 0; // To remove, use col and row to get index
            for (var col = 0; col < heightInSamples; col++)
            {
                for (var row = 0; row < widthInSamples; row++)
                {
                    var (r, g, b) = GetColor(bytesPure[i++] / 255.0, bytesPure[i++] / 255.0, bytesPure[i++] / 255.0).ToRGBValues();
                    builder.SetPixel(ConvertToByte(r), ConvertToByte(g), ConvertToByte(b), row, col);
                }
            }
            return builder.Save();
        }

        /// <inheritdoc/>
        public override int NumberOfColorComponents => 3; // TODO - To check
    }

    /// <summary>
    /// The ICCBased color space is one of the CIE-based color spaces supported in PDFs. These color spaces
    /// enable a page description to specify color values in a way that is related to human visual perception.
    /// The goal is for the same color specification to produce consistent results on different output devices,
    /// within the limitations of each device.
    ///
    /// Currently support for this color space is limited in PdfPig. Calculations will only be based on
    /// the color space of <see cref="AlternateColorSpaceDetails"/>.
    /// </summary>
    public class ICCBasedColorSpaceDetails : ColorSpaceDetails
    {
        /// <summary>
        /// The number of color components in the color space described by the ICC profile data.
        /// This numbers shall match the number of components actually in the ICC profile.
        /// Valid values are 1, 3 and 4.
        /// </summary>
        public override int NumberOfColorComponents { get; }

        /// <summary>
        /// <para>
        /// An alternate color space that can be used in case the one specified in the stream data is not
        /// supported. Non-conforming readers may use this color space. The alternate color space may be any
        /// valid color space (except a Pattern color space). If this property isn't explicitly set during
        /// construction, it will assume one of the color spaces, DeviceGray, DeviceRGB or DeviceCMYK depending
        /// on whether the value of <see cref="NumberOfColorComponents"/> is 1, 3 or respectively.
        /// </para>
        /// <para>
        /// Conversion of the source color values should not be performed when using the alternate color space.
        /// Color values within the range of the ICCBased color space might not be within the range of the
        /// alternate color space. In this case, the nearest values within the range of the alternate space
        /// must be substituted.
        /// </para>
        /// </summary>
        [NotNull]
        public ColorSpaceDetails AlternateColorSpaceDetails { get; }

        /// <summary>
        /// A list of 2 x <see cref="NumberOfColorComponents"/> numbers [min0 max0  min1 max1  ...] that
        /// specifies the minimum and maximum valid values of the corresponding color components. These
        /// values must match the information in the ICC profile. Default value: [0.0 1.0  0.0 1.0  ...].
        /// </summary>
        [NotNull]
        public IReadOnlyList<decimal> Range { get; }

        /// <summary>
        /// An optional metadata stream that contains metadata for the color space.
        /// </summary>
        [CanBeNull]
        public XmpMetadata Metadata { get; }

        
        /// <summary>
        /// ICC profile.
        /// </summary>
        [CanBeNull]
        internal IccProfile Profile { get; }        

        /// <summary>
        /// Create a new <see cref="ICCBasedColorSpaceDetails"/>.
        /// </summary>
        internal ICCBasedColorSpaceDetails(int numberOfColorComponents, [CanBeNull] ColorSpaceDetails alternateColorSpaceDetails,
            [CanBeNull] IReadOnlyList<decimal> range, [CanBeNull] XmpMetadata metadata, [CanBeNull] IReadOnlyList<byte> rawProfile)
            : base(ColorSpace.ICCBased)
        {
            if (numberOfColorComponents != 1 && numberOfColorComponents != 3 && numberOfColorComponents != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfColorComponents), "must be 1, 3 or 4");
            }

            NumberOfColorComponents = numberOfColorComponents;
            AlternateColorSpaceDetails = alternateColorSpaceDetails ??
                (NumberOfColorComponents == 1 ? (ColorSpaceDetails)DeviceGrayColorSpaceDetails.Instance :
                NumberOfColorComponents == 3 ? (ColorSpaceDetails)DeviceRgbColorSpaceDetails.Instance : (ColorSpaceDetails)DeviceCmykColorSpaceDetails.Instance);

            BaseType = AlternateColorSpaceDetails.BaseType;
            Range = range ??
                Enumerable.Range(0, numberOfColorComponents).Select(_ => new[] { 0.0m, 1.0m }).SelectMany(x => x).ToList();
            if (Range.Count != 2 * numberOfColorComponents)
            {
                throw new ArgumentOutOfRangeException(nameof(range), range,
                    $"Must consist of exactly {2 * numberOfColorComponents} (2 x NumberOfColorComponents), but was passed {range.Count}");
            }
            Metadata = metadata;

            if (rawProfile != null)
            {
                System.IO.Directory.CreateDirectory("ICC_Profiles_errors");

                string iccProfileName = Guid.NewGuid().ToString().ToLower();

                try
                {
                    Profile = new IccProfile(rawProfile.ToArray());

                    if (Profile.Tags.TryGetValue(IccTags.ProfileDescriptionTag, out var desc))
                    {
                        if (desc is IccMultiLocalizedUnicodeType localised)
                        {

                            iccProfileName = $"[{Profile.Header}] {localised.Records[0].Text.Trim()}";
                        }
                        else
                        {
                            iccProfileName = $"[{Profile.Header}] {desc.ToString().Trim()}";
                        }
                    }

                    iccProfileName = string.Join("-", iccProfileName.Split(Path.GetInvalidFileNameChars()));

                    File.WriteAllBytes($"ICC_Profiles_errors/{iccProfileName}.icc",
                        rawProfile.ToArray());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR creating ICC profile: {ex}");

                    Directory.CreateDirectory("ICC_Profiles_errors");
                    File.WriteAllBytes($"ICC_Profiles_errors/error_{iccProfileName}.icc",
                        rawProfile.ToArray());
                }
            }
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            if (values == null || values.Length != NumberOfColorComponents)
            {
                throw new ArgumentException(nameof(values));
            }

            if (Profile != null && Profile.TryProcess(values, out double[] test) && test.Length == 3)
            {
                return new RGBColor((decimal)test[0], (decimal)test[1], (decimal)test[2]);
            }

            return AlternateColorSpaceDetails.GetColor(values);
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            // Setting the current stroking or nonstroking colour space to any CIE-based colour space shall
            // initialize all components of the corresponding current colour to 0.0 (unless the range of valid
            // values for a given component does not include 0.0, in which case the nearest valid value shall
            // be substituted.)
            double[] init = Enumerable.Repeat(0.0, NumberOfColorComponents).ToArray();
            return GetColor(init);
        }

        /// <inheritdoc/>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            var builder = PngBuilder.Create(widthInSamples, heightInSamples, hasAlphaChannel);
            int i = 0; // To remove, use col and row to get index
            for (var col = 0; col < heightInSamples; col++)
            {
                for (var row = 0; row < widthInSamples; row++)
                {
                    double[] comps = new double[NumberOfColorComponents];
                    for (int k1 = 0; k1 < NumberOfColorComponents; k1++)
                    {
                        comps[k1] = bytesPure[i++] / 255.0; // Do we want to divide by 255?
                    }
                    var (r, g, b) = GetColor(comps).ToRGBValues();
                    builder.SetPixel(ConvertToByte(r), ConvertToByte(g), ConvertToByte(b), row, col);
                }
            }
            return builder.Save();
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public sealed class PatternColorSpaceDetails : ColorSpaceDetails
    {
        /// <summary>
        /// TODO
        /// </summary>
        public IReadOnlyDictionary<NameToken, PatternColor> Patterns { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public PatternColorSpaceDetails(IReadOnlyDictionary<NameToken, PatternColor> patterns) : base(ColorSpace.Pattern)
        {
            Patterns = patterns ?? throw new ArgumentNullException(nameof(patterns));
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="name"></param>
        public PatternColor GetPattern(NameToken name)
        {
            return Patterns[name];
        }

        /// <summary>
        /// <inheritdoc/>
        /// <para>
        /// Cannot be called for <see cref="PatternColorSpaceDetails"/>, will throw a <see cref="InvalidOperationException"/>.
        /// </para>
        /// </summary>
        public override IColor GetColor(params double[] values)
        {
            throw new InvalidOperationException("PatternColorSpaceDetails");
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            return null;
        }

        /// <summary>
        /// <inheritdoc/>
        /// <para>
        /// Cannot be called for <see cref="PatternColorSpaceDetails"/>, will throw a <see cref="InvalidOperationException"/>.
        /// </para>
        /// </summary>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            throw new InvalidOperationException("PatternColorSpaceDetails");
        }

        /// <summary>
        /// <inheritdoc/>
        /// <para>
        /// Cannot be called for <see cref="PatternColorSpaceDetails"/>, will throw a <see cref="InvalidOperationException"/>.
        /// </para>
        /// </summary>
        public override int NumberOfColorComponents => throw new InvalidOperationException("PatternColorSpaceDetails");
    }

    /// <summary>
    /// A ColorSpace which the PdfPig library does not currently support. Please raise a PR if you need support for this ColorSpace.
    /// </summary>
    public sealed class UnsupportedColorSpaceDetails : ColorSpaceDetails
    {
        /// <summary>
        /// The single instance of the <see cref="UnsupportedColorSpaceDetails"/>.
        /// </summary>
        public static readonly UnsupportedColorSpaceDetails Instance = new UnsupportedColorSpaceDetails();

        private readonly IColor debugColor = new RGBColor(255m / 255m, 20m / 255m, 147m / 255m);

        private UnsupportedColorSpaceDetails() : base(ColorSpace.DeviceGray)
        {
        }

        /// <inheritdoc/>
        public override IColor GetColor(params double[] values)
        {
            return debugColor;
        }

        /// <inheritdoc/>
        public override IColor GetInitializeColor()
        {
            return debugColor;
        }

        /// <summary>
        /// <inheritdoc/>
        /// <para>
        /// Cannot be called for <see cref="UnsupportedColorSpaceDetails"/>, will throw a <see cref="InvalidOperationException"/>.
        /// </para>
        /// </summary>
        public override int NumberOfColorComponents => throw new InvalidOperationException("UnsupportedColorSpaceDetails");

        /// <summary>
        /// <inheritdoc/>
        /// <para>
        /// Cannot be called for <see cref="UnsupportedColorSpaceDetails"/>, will throw a <see cref="InvalidOperationException"/>.
        /// </para>
        /// </summary>
        public override byte[] GetPngImage(IReadOnlyList<byte> bytesPure, int heightInSamples, int widthInSamples, bool hasAlphaChannel)
        {
            throw new InvalidOperationException("UnsupportedColorSpaceDetails");
        }
    }
}
