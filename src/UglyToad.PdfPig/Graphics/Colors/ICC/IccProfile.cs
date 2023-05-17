using System;
using System.Collections.Generic;
using System.Linq;
using IccProfileNet.Parsers;
using IccProfileNet.Tags;
using UglyToad.PdfPig.Graphics.Colors;

namespace IccProfileNet
{
    /// <summary>
    /// ICC profile.
    /// </summary>
    internal class IccProfile
    {
        /// <summary>
        /// ICC profile header.
        /// </summary>
        public IccProfileHeader Header { get; }

        private readonly Lazy<IccTagTableItem[]> _tagTable;
        /// <summary>
        /// The tag table acts as a table of contents for the tags and an index into the tag data element in the profiles.
        /// </summary>
        public IccTagTableItem[] TagTable => _tagTable.Value;

        private readonly Lazy<IReadOnlyDictionary<string, IccTagTypeBase>> _tags;
        public IReadOnlyDictionary<string, IccTagTypeBase> Tags => _tags.Value;

        /// <summary>
        /// TODO
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// ICC profile v4.
        /// </summary>
        public IccProfile(byte[] data)
        {
            Data = data;
            Header = new IccProfileHeader(data);
            _tagTable = new Lazy<IccTagTableItem[]>(() => ParseTagTable(data.Skip(128).ToArray()));
            _tags = new Lazy<IReadOnlyDictionary<string, IccTagTypeBase>>(() => GetTags());
        }

        internal byte[] GetOrComputeProfileId()
        {
            if (Header.IsProfileIdComputed())
            {
                return Header.ProfileId;
            }

            return ComputeProfileId();
        }

        internal byte[] ComputeProfileId()
        {
            return IccHelper.ComputeProfileId(Data);
        }

        /// <summary>
        /// Validates the profile against the profile id.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the profile id contained in the header matches the re-computed profile id.
        /// <c>false</c> if they don't match.
        /// <c>null</c> if the profile id is not set in the header.
        /// </returns>
        public bool? ValidateProfile()
        {
            if (Header.IsProfileIdComputed())
            {
                return null;
            }

            return Header.ProfileId.SequenceEqual(ComputeProfileId());
        }

        private static IccTagTableItem[] ParseTagTable(byte[] bytes)
        {
            // Tag count (n)
            // 0 to 3
            uint tagCount = IccHelper.ReadUInt32(bytes
                .Skip(IccTagTableItem.TagCountOffset)
                .Take(IccTagTableItem.TagCountLength).ToArray());

            IccTagTableItem[] tagTableItems = new IccTagTableItem[tagCount];

            for (var i = 0; i < tagCount; ++i)
            {
                int currentOffset = i * (IccTagTableItem.TagSignatureLength +
                                         IccTagTableItem.TagOffsetLength +
                                         IccTagTableItem.TagSizeLength);

                // Tag Signature
                // 4 to 7
                string signature = IccHelper.GetString(bytes,
                    currentOffset + IccTagTableItem.TagSignatureOffset, IccTagTableItem.TagSignatureLength);

                // Offset to beginning of tag data element
                // 8 to 11
                uint offset = IccHelper.ReadUInt32(bytes
                    .Skip(currentOffset + IccTagTableItem.TagOffsetOffset)
                    .Take(IccTagTableItem.TagOffsetLength).ToArray());

                // Size of tag data element
                // 12 to 15
                uint size = IccHelper.ReadUInt32(bytes
                    .Skip(currentOffset + IccTagTableItem.TagSizeOffset)
                    .Take(IccTagTableItem.TagSizeLength).ToArray());

                tagTableItems[i] = new IccTagTableItem(signature, offset, size);
            }

            return tagTableItems;
        }

        private IReadOnlyDictionary<string, IccTagTypeBase> GetTags()
        {
            switch (Header.VersionMajor)
            {
                case 4:
                    {
                        var tags = new Dictionary<string, IccTagTypeBase>();
                        for (int t = 0; t < TagTable.Length; t++)
                        {
                            IccTagTableItem tag = TagTable[t];
                            tags.Add(tag.Signature, IccProfileV4TagParser.Parse(Data, tag));
                        }
                        return tags;
                    }

                case 2:
                    {
                        var tags = new Dictionary<string, IccTagTypeBase>();
                        for (int t = 0; t < TagTable.Length; t++)
                        {
                            IccTagTableItem tag = TagTable[t];
                            tags.Add(tag.Signature, IccProfileV2TagParser.Parse(Data, tag));
                        }
                        return tags;
                    }

                default:
                    throw new NotImplementedException($"ICC Profile v{Header.VersionMajor}.{Header.VersionMinor} is not supported.");
            }
        }



        internal bool TryProcess(double[] input, out double[] output)
        {
            try
            {
                double[] whitePoint = new double[] // Wrong
                {
                    Header.nCIEXYZ.X,
                    Header.nCIEXYZ.Y,
                    Header.nCIEXYZ.Z,
                };

                //if (Tags.TryGetValue(IccTags.MediaWhitePointTag, out var wtptTag) && wtptTag is IccXyzType wt)
                //{
                //    whitePoint = new double[] { wt.Xyz.X, wt.Xyz.Y, wt.Xyz.Z };
                //}
                var colorSpaceTransformer = new CIEBasedColorSpaceTransformer((whitePoint[0], whitePoint[1], whitePoint[2]), RGBWorkingSpace.sRGB);

                switch (Header.ProfileClass)
                {
                    case IccProfileClass.Input:
                        {
                            // 8.3 Input profiles
                            // 8.3.2 N-component LUT-based Input profiles
                            if (Tags.TryGetValue(IccTags.AToB0Tag, out var lutAB0TagI) &&
                                lutAB0TagI is IccLutABType lutAb0I)
                            {
                                //output = lutAB.Process(input, Header);
                                //return true;
                            }

                            // 8.3.3 Three-component matrix-based Input profiles
                            if (Tags.TryGetValue(IccTags.RedMatrixColumnTag, out var rmcTagI) &&
                                Tags.TryGetValue(IccTags.RedTRCTag, out var rTrcTagI))
                            {
                                // Check other taggs
                            }

                            // 8.3.4 Monochrome Input profiles
                            if (Tags.TryGetValue(IccTags.GrayTRCTag, out var tag))
                            {

                            }
                            break;
                        }

                    case IccProfileClass.Display:
                        {
                            // 8.4 Display profiles
                            // 8.4.2 N-Component LUT-based Display profilesS
                            if (Tags.TryGetValue(IccTags.AToB0Tag, out var lutAB0TagD) && lutAB0TagD is IccLutABType lutAb0D &&
                                Tags.TryGetValue(IccTags.BToA0Tag, out var lutBA0TagD) && lutBA0TagD is IccLutABType lutBa0D)
                            {
                                output = lutBa0D.Process(input, Header);
                                return true;
                            }

                            // 8.4.3 Three-component matrix-based Display profiles
                            // See p197 of Wiley book
                            if (Tags.TryGetValue(IccTags.RedMatrixColumnTag, out var rmcTag) && rmcTag is IccXyzType rmc &&
                                Tags.TryGetValue(IccTags.GreenMatrixColumnTag, out var gmcTag) && gmcTag is IccXyzType gmc &&
                                Tags.TryGetValue(IccTags.BlueMatrixColumnTag, out var bmcTag) && bmcTag is IccXyzType bmc &&

                                Tags.TryGetValue(IccTags.RedTRCTag, out var rTrcTag) && rTrcTag is IccBaseCurveType rTrc &&
                                Tags.TryGetValue(IccTags.GreenTRCTag, out var gTrcTag) && gTrcTag is IccBaseCurveType gTrc &&
                                Tags.TryGetValue(IccTags.BlueTRCTag, out var bTrcTag) && bTrcTag is IccBaseCurveType bTrc)
                            {
                                // Optional
                                if (Tags.TryGetValue(IccTags.ChromaticAdaptationTag, out var caTag) && caTag is IccS15Fixed16ArrayType ca)
                                {

                                }

                                double channel1 = input[0];
                                double channel2 = input[1];
                                double channel3 = input[2];

                                double lR = rTrc.Process(channel1);
                                double lG = gTrc.Process(channel2);
                                double lB = bTrc.Process(channel3);

                                double cX = rmc.Xyz.X * lR + gmc.Xyz.X * lG + bmc.Xyz.X * lB;
                                double cY = rmc.Xyz.Y * lR + gmc.Xyz.Y * lG + bmc.Xyz.Y * lB;
                                double cZ = rmc.Xyz.Z * lR + gmc.Xyz.Z * lG + bmc.Xyz.Z * lB;

                                var (R, G, B) = colorSpaceTransformer.XYZToRGB((cX, cY, cZ));

                                output = new double[] { R, G, B };
                                return true;
                            }

                            // 8.4.4 Monochrome Display profiles
                            if (Tags.TryGetValue(IccTags.GrayTRCTag, out var tag) && tag is IccBaseCurveType lut)
                            {
                                var x = lut.Process(input[0]);

                                if (Header.Pcs == IccProfileConnectionSpace.PCSXYZ)
                                {
                                    var (R, G, B) = colorSpaceTransformer.XYZToRGB((x, x, x));

                                    output = new double[] { R, G, B };
                                    return true;
                                }
                                else if (Header.Pcs == IccProfileConnectionSpace.PCSLAB)
                                {
                                    var lab = new LabColorSpaceDetails(whitePoint.Select(v => (decimal)v).ToArray(), null, new[] { -128.0m, 127.0m, -128.0m, 127.0m });

                                    var rgb = lab.GetColor(x, x, x).ToRGBValues(); // Should also do not rgb

                                    output = new double[] { (double)rgb.r, (double)rgb.g, (double)rgb.b };
                                    return true;
                                }
                            }
                            break;
                        }

                    case IccProfileClass.Output:
                        {
                            // 8.5.2 N-component LUT-based Output profiles
                            if (Tags.TryGetValue(IccTags.AToB0Tag, out var lutAB0Tag) && lutAB0Tag is IIccClutType lutAb0 &&
                                Tags.TryGetValue(IccTags.AToB1Tag, out var lutAB1Tag) && lutAB1Tag is IIccClutType lutAb1 &&
                                Tags.TryGetValue(IccTags.AToB2Tag, out var lutAB2Tag) && lutAB2Tag is IIccClutType lutAb2 &&


                                Tags.TryGetValue(IccTags.BToA0Tag, out var lutBA0Tag) && lutBA0Tag is IIccClutType lutBa0 &&
                                Tags.TryGetValue(IccTags.BToA1Tag, out var lutBA1Tag) && lutBA1Tag is IIccClutType lutBa1 &&
                                Tags.TryGetValue(IccTags.BToA2Tag, out var lutBA2Tag) && lutBA2Tag is IIccClutType lutBa2 &&

                                Tags.TryGetValue(IccTags.GamutTag, out var gamutTag) && gamutTag is IIccClutType gamut)
                            {
                                // Tags.TryGetValue(IccTags.ColorantTableTag, out var colorantTableTag)
                                var pcs = lutAb0.Process(input, Header);

                                //double[] outOfGammut = gamut.Process(xyz, Header);
                                /*
                                 * This tag provides a table in which PCS values are the input and a single output value for each input value is the
                                 * output. If the output value is 0, the PCS colour is in-gamut. If the output is non-zero, the PCS colour is out-ofgamut.
                                 */

                                if (Header.Pcs == IccProfileConnectionSpace.PCSXYZ)
                                {
                                    var (R, G, B) = colorSpaceTransformer.XYZToRGB((pcs[0], pcs[1], pcs[2]));

                                    output = new double[] { R, G, B };
                                    return true;
                                }
                                else if (Header.Pcs == IccProfileConnectionSpace.PCSLAB)
                                {
                                    var lab = new LabColorSpaceDetails(whitePoint.Select(v => (decimal)v).ToArray(), null, new[] { -128.0m, 127.0m, -128.0m, 127.0m });

                                    double mL = pcs[0];
                                    double ma = pcs[1];
                                    double mb = pcs[2];

                                    //double y = ((mL * 100.0) + 16.0) / 116.0;
                                    //double x = (ma * 255.0 - 128.0) / 500.0 + y;
                                    //double z = -(mb * 255.0 - 128.0) / 200.0 + y;
                                    //var (R, G, B) = colorSpaceTransformer.XYZToRGB((x, y, z));

                                    double LStar = pcs[0] * 100.0;
                                    double aStar = pcs[1] * 255.0 - 128.0;
                                    double bStar = pcs[2] * 255.0 - 128.0;
                                    var (r, g, b) = lab.GetColor(LStar, aStar, bStar).ToRGBValues(); // Should also do not rgb?

                                    output = new double[] { (double)r,  (double)g,  (double)b };
                                    return true;
                                }
                            }

                            // 8.5.3 Monochrome Output profiles
                            if (Tags.TryGetValue(IccTags.GrayTRCTag, out var tag))
                            {

                            }
                            break;
                        }

                    case IccProfileClass.DeviceLink:
                        {
                            // TODO
                            break;
                        }

                    case IccProfileClass.ColorSpace:
                        {
                            // 8.7 ColorSpace profile
                            if (Tags.TryGetValue(IccTags.AToB0Tag, out var lutAB0Tag) && lutAB0Tag is IccLutABType lutAb0 &&
                                Tags.TryGetValue(IccTags.BToA0Tag, out var lutBA0Tag) && lutBA0Tag is IccLutABType lutBa0)
                            {

                            }
                            break;
                        }

                    case IccProfileClass.Abstract:
                        {
                            // TODOS
                            break;
                        }

                    case IccProfileClass.NamedColor:
                        {
                            // TODO
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception)
            {
                // Ignore
                output = null;
                return false;
            }

            output = null;
            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ICC Profile v{Header}";
        }
    }
}