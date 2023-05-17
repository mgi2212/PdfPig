namespace UglyToad.PdfPig.Util
{
    using System;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Filters;
    using UglyToad.PdfPig.Functions;
    using UglyToad.PdfPig.Graphics.Colors;
    using UglyToad.PdfPig.Tokenization.Scanner;
    using UglyToad.PdfPig.Tokens;

    internal static class ShadingParser
    {
        public static Shading Create(IToken shading, IPdfTokenScanner scanner, IResourceStore resourceStore, ILookupFilterProvider filterProvider)
        {
            DictionaryToken shadingDictionary = null;
            StreamToken shadingStream = null;

            if (shading is StreamToken fs)
            {
                shadingDictionary = fs.StreamDictionary;
                shadingStream = new StreamToken(fs.StreamDictionary, fs.Decode(filterProvider, scanner));
            }
            else if (shading is DictionaryToken fd)
            {
                shadingDictionary = fd;
            }

            ShadingType shadingType;
            if (shadingDictionary.TryGet<NumericToken>(NameToken.ShadingType, scanner, out var shadingTypeToken))
            {
                shadingType = (ShadingType)shadingTypeToken.Int;
            }
            else
            {
                throw new ArgumentException("ShadingType is required.");
            }

            ColorSpaceDetails colorSpaceDetails = null;
            if (shadingDictionary.TryGet<NameToken>(NameToken.ColorSpace, scanner, out var colorSpaceToken))
            {
                colorSpaceDetails = resourceStore.GetColorSpaceDetails(colorSpaceToken, shadingDictionary);
            }
            else if (shadingDictionary.TryGet<ArrayToken>(NameToken.ColorSpace, scanner, out var colorSpaceSToken))
            {
                var first = colorSpaceSToken.Data[0];
                if (first is NameToken firstColorSpaceName)
                {
                    colorSpaceDetails = resourceStore.GetColorSpaceDetails(firstColorSpaceName, shadingDictionary);
                }
                else
                {
                    throw new ArgumentException("ColorSpace is required.");
                }
            }
            else
            {
                throw new ArgumentException("ColorSpace is required.");
            }

            /*
             * In addition, some shading dictionaries also include a Function entry whose value shall be a
             * function object (dictionary or stream) defining how colours vary across the area to be shaded.
             * In such cases, the shading dictionary usually defines the geometry of the shading, and the
             * function defines the colour transitions across that geometry. The function is required for
             * some types of shading and optional for others.
             */
            PdfFunction function = null;
            if (shadingDictionary.ContainsKey(NameToken.Function))
            {
                function = PdfFunctionParser.Create(shadingDictionary.Data[NameToken.Function], scanner, filterProvider);
            }
            else
            {
                // 8.7.4.5.2 Type 1 (Function-Based) Shadings - Required
                // 8.7.4.5.3 Type 2 (Axial) Shadings - Required
                // 8.7.4.5.4 Type 3 (Radial) Shadings - Required
                // 8.7.4.5.5 Type 4 Shadings (Free-Form Gouraud-Shaded Triangle Meshes) - Optional
                // 8.7.4.5.6 Type 5 Shadings (Lattice-Form Gouraud-Shaded Triangle Meshes) - Optional
                // 8.7.4.5.7 Type 6 Shadings (Coons Patch Meshes) - Optional
                // 8.7.4.5.8 Type 7 Shadings (Tensor-Product Patch Meshes) - N/A
                if (shadingType == ShadingType.FunctionBased || shadingType == ShadingType.Axial || shadingType == ShadingType.Radial)
                {
                    throw new ArgumentNullException($"{NameToken.Function} is required for shading type '{shadingType}'.");
                }
            }

            if (!shadingDictionary.TryGet<ArrayToken>(NameToken.Background, scanner, out var backgroundToken))
            {
                // Optional
            }

            if (shadingDictionary.TryGet<ArrayToken>(NameToken.Bbox, scanner, out var bboxToken))
            {
                // TODO - check if array (sais it's 'rectangle')
                // Optional
            }

            bool antiAlias = false; // Default value: false.
            if (shadingDictionary.TryGet<BooleanToken>(NameToken.AntiAlias, scanner, out var antiAliasToken))
            {
                // Optional
                antiAlias = antiAliasToken.Data;
            }

            decimal[] coords = null;
            if (shadingDictionary.TryGet<ArrayToken>(NameToken.Coords, scanner, out var coordsToken))
            {
                coords = coordsToken.Data.OfType<NumericToken>().Select(v => v.Data).ToArray();
            }

            decimal[] domain = null;
            if (shadingDictionary.TryGet<ArrayToken>(NameToken.Domain, scanner, out var domainToken))
            {
                domain = domainToken.Data.OfType<NumericToken>().Select(v => v.Data).ToArray();
            }
            else
            {
                // set default values
                domain = new decimal[] { 0, 1 };
            }

            bool[] extend = null;
            if (shadingDictionary.TryGet<ArrayToken>(NameToken.Extend, scanner, out var extendToken))
            {
                extend = extendToken.Data.OfType<BooleanToken>().Select(v => v.Data).ToArray();
            }
            else
            {
                // set default values
                extend = new bool[] { false, false };
            }

            return new Shading(shadingType, antiAlias, shadingDictionary,
                colorSpaceDetails, function, coords, domain, extend,
                bboxToken?.ToRectangle(scanner), backgroundToken);
        }
    }
}
