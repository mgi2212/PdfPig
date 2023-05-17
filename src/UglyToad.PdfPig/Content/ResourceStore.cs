namespace UglyToad.PdfPig.Content
{
    using Core;
    using Graphics.Colors;
    using Parser.Parts;
    using PdfFonts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tokenization.Scanner;
    using Tokens;
    using UglyToad.PdfPig.Filters;
    using Util;

    internal class ResourceStore : IResourceStore
    {
        private readonly FilterProviderWithLookup filterProvider = new FilterProviderWithLookup(DefaultFilterProvider.Instance);

        private readonly IPdfTokenScanner scanner;
        private readonly IFontFactory fontFactory;

        private readonly Dictionary<IndirectReference, IFont> loadedFonts = new Dictionary<IndirectReference, IFont>();
        private readonly Dictionary<NameToken, IFont> loadedDirectFonts = new Dictionary<NameToken, IFont>();
        private readonly StackDictionary<NameToken, IndirectReference> currentResourceState = new StackDictionary<NameToken, IndirectReference>();

        private readonly Dictionary<NameToken, ColorSpaceDetails> loadedNamedColorSpaceDetails = new Dictionary<NameToken, ColorSpaceDetails>();

        private readonly Dictionary<NameToken, DictionaryToken> extendedGraphicsStates = new Dictionary<NameToken, DictionaryToken>();

        private readonly Dictionary<NameToken, ResourceColorSpace> namedColorSpaces = new Dictionary<NameToken, ResourceColorSpace>();

        private readonly Dictionary<NameToken, DictionaryToken> markedContentProperties = new Dictionary<NameToken, DictionaryToken>();

        private readonly Dictionary<NameToken, Shading> shadingsProperties = new Dictionary<NameToken, Shading>();

        internal readonly Dictionary<NameToken, PatternColor> patternsProperties = new Dictionary<NameToken, PatternColor>();

        private (NameToken name, IFont font) lastLoadedFont;

        public ResourceStore(IPdfTokenScanner scanner, IFontFactory fontFactory)
        {
            this.scanner = scanner;
            this.fontFactory = fontFactory;
        }

        public ColorSpaceDetails GetColorSpaceDetails(NameToken name, DictionaryToken dictionary)
        {
            if (name.TryMapToColorSpace(out var colorspaceActual))
            {
                if (dictionary == null)
                {
                    dictionary = new DictionaryToken(new Dictionary<NameToken, IToken>());
                }

                // TODO - We need to find a way to store profile that have an actual dictionnary, e.g. ICC profiles - without parsing them again


                return ColorSpaceDetailsParser.GetColorSpaceDetails(colorspaceActual, dictionary, scanner, this, filterProvider);
            }
            else if (loadedNamedColorSpaceDetails.TryGetValue(name, out ColorSpaceDetails csdLoaded))
            {
                return csdLoaded;
            }
            else if (TryGetNamedColorSpace(name, out var namedColorSpace))
            {
                if (dictionary != null)
                {

                }

                if (namedColorSpace.Name.TryMapToColorSpace(out var mapped))
                {
                    if (namedColorSpace.Data is ArrayToken separationArray)
                    {
                        var pseudoDictionary = new DictionaryToken(
                            new Dictionary<NameToken, IToken>
                            {
                            { NameToken.ColorSpace, separationArray }
                            });
                        var csd = ColorSpaceDetailsParser.GetColorSpaceDetails(mapped, pseudoDictionary, scanner, this, filterProvider, false);

                        loadedNamedColorSpaceDetails.Add(name, csd);
                        return csd;
                    }
                    else if (namedColorSpace.Data is NameToken namedCs)
                    {

                    }
                }
            }
            throw new InvalidOperationException("GetColorSpaceDetails");
        }

        public void LoadResourceDictionary(DictionaryToken resourceDictionary, InternalParsingOptions parsingOptions)
        {
            lastLoadedFont = (null, null);

            currentResourceState.Push();

            if (resourceDictionary.TryGet(NameToken.Font, out var fontBase))
            {
                var fontDictionary = DirectObjectFinder.Get<DictionaryToken>(fontBase, scanner);

                LoadFontDictionary(fontDictionary, parsingOptions);
            }

            if (resourceDictionary.TryGet(NameToken.Xobject, out var xobjectBase))
            {
                var xobjectDictionary = DirectObjectFinder.Get<DictionaryToken>(xobjectBase, scanner);

                foreach (var pair in xobjectDictionary.Data)
                {
                    if (pair.Value is NullToken)
                    {
                        continue;
                    }

                    if (!(pair.Value is IndirectReferenceToken reference))
                    {
                        throw new InvalidOperationException($"Expected the XObject dictionary value for key /{pair.Key} to be an indirect reference, instead got: {pair.Value}.");
                    }

                    currentResourceState[NameToken.Create(pair.Key)] = reference.Data;
                }
            }

            if (resourceDictionary.TryGet(NameToken.ExtGState, scanner, out DictionaryToken extGStateDictionaryToken))
            {
                foreach (var pair in extGStateDictionaryToken.Data)
                {
                    var name = NameToken.Create(pair.Key);
                    var state = DirectObjectFinder.Get<DictionaryToken>(pair.Value, scanner);

                    extendedGraphicsStates[name] = state;
                }
            }

            if (resourceDictionary.TryGet(NameToken.Pattern, scanner, out DictionaryToken patternDictionary))
            {
                foreach (var namePatternPair in patternDictionary.Data)
                {
                    var name = NameToken.Create(namePatternPair.Key);
                    patternsProperties[name] = PatternParser.Create(namePatternPair.Value, scanner, this, filterProvider);

                    /*
                    if (DirectObjectFinder.TryGet(namePatternPair.Value, scanner, out DictionaryToken patternArray))
                    {
                        int patternType = (patternArray.Data[NameToken.PatternType] as NumericToken).Int;

                        ArrayToken patternMatrix = null;
                        if (!(patternArray.Data.ContainsKey(NameToken.Matrix) &&
                            DirectObjectFinder.TryGet(patternArray.Data[NameToken.Matrix], scanner, out patternMatrix)))
                        {
                            // optional - Default value: the identity matrix [1 0 0 1 0 0]
                            patternMatrix = new ArrayToken(new decimal[] { 1, 0, 0, 1, 0, 0 }.Select(v => new NumericToken(v)).ToArray());
                        }

                        DictionaryToken patternExtGState = null;
                        if (!(patternArray.Data.ContainsKey(NameToken.ExtGState) &&
                            DirectObjectFinder.TryGet(patternArray.Data[NameToken.ExtGState], scanner, out patternExtGState)))
                        {
                            // optional
                        }

                        switch (patternType)
                        {
                            case 1: // Tiling
                                throw new NotImplementedException("Tiling style pattern");

                            case 2: // Shading
                                Shading patternShading = null;
                                if (DirectObjectFinder.TryGet(patternArray.Data[NameToken.Shading], scanner, out DictionaryToken patternShadingDic))
                                {
                                    patternShading = ShadingParser.Create(patternShadingDic, scanner, this, filterProvider);
                                }
                                else if (DirectObjectFinder.TryGet(patternArray.Data[NameToken.Shading], scanner, out StreamToken patternShadingStr))
                                {
                                    patternShading = ShadingParser.Create(patternShadingStr, scanner, this, filterProvider);
                                }
                                else
                                {
                                    throw new ArgumentException();
                                }
                                patternsProperties[name] = new Pattern(patternType, patternMatrix, patternShading, patternExtGState);
                                break;
                        }
                    }
                    else
                    {
                        throw new PdfDocumentFormatException($"Invalid Pattern token encountered in page resource dictionary: {namePatternPair.Value}.");
                    }
                    */
                }
            }

            if (resourceDictionary.TryGet(NameToken.ColorSpace, scanner, out DictionaryToken colorSpaceDictionary))
            {
                foreach (var nameColorSpacePair in colorSpaceDictionary.Data)
                {
                    var name = NameToken.Create(nameColorSpacePair.Key);

                    if (DirectObjectFinder.TryGet(nameColorSpacePair.Value, scanner, out NameToken colorSpaceName))
                    {
                        namedColorSpaces[name] = new ResourceColorSpace(colorSpaceName);
                    }
                    else if (DirectObjectFinder.TryGet(nameColorSpacePair.Value, scanner, out ArrayToken colorSpaceArray))
                    {
                        if (colorSpaceArray.Length == 0)
                        {
                            throw new PdfDocumentFormatException($"Empty ColorSpace array encountered in page resource dictionary: {resourceDictionary}.");
                        }

                        var first = colorSpaceArray.Data[0];

                        if (!(first is NameToken arrayNamedColorSpace))
                        {
                            throw new PdfDocumentFormatException($"Invalid ColorSpace array encountered in page resource dictionary: {colorSpaceArray}.");
                        }

                        namedColorSpaces[name] = new ResourceColorSpace(arrayNamedColorSpace, colorSpaceArray);
                    }
                    else
                    {
                        throw new PdfDocumentFormatException($"Invalid ColorSpace token encountered in page resource dictionary: {nameColorSpacePair.Value}.");
                    }
                }
            }

            if (resourceDictionary.TryGet(NameToken.Properties, scanner, out DictionaryToken markedContentPropertiesList))
            {
                foreach (var pair in markedContentPropertiesList.Data)
                {
                    var key = NameToken.Create(pair.Key);

                    if (!DirectObjectFinder.TryGet(pair.Value, scanner, out DictionaryToken namedProperties))
                    {
                        continue;
                    }

                    markedContentProperties[key] = namedProperties;
                }
            }

            if (resourceDictionary.TryGet(NameToken.Shading, scanner, out DictionaryToken shadingList))
            {
                foreach (var pair in shadingList.Data)
                {
                    var key = NameToken.Create(pair.Key);
                    if (DirectObjectFinder.TryGet(pair.Value, scanner, out DictionaryToken namedPropertiesDictionary))
                    {
                        shadingsProperties[key] = ShadingParser.Create(namedPropertiesDictionary, scanner, this, filterProvider);
                    }
                    else if (DirectObjectFinder.TryGet(pair.Value, scanner, out StreamToken namedPropertiesStream))
                    {
                        /*
                         * Shading types 4 to 7 shall be defined by a stream containing descriptive data characterizing
                         * the shading’s gradient fill. In these cases, the shading dictionary is also a stream dictionary
                         * and may contain any of the standard entries common to all streams (see Table 5). In particular,
                         * shall include a Length entry.
                         */
                        shadingsProperties[key] = ShadingParser.Create(namedPropertiesStream, scanner, this, filterProvider);
                    }
                    else
                    {
                        throw new NotImplementedException("Shading");
                    }
                }
            }
        }

        public void UnloadResourceDictionary()
        {
            lastLoadedFont = (null, null);
            currentResourceState.Pop();
        }

        private void LoadFontDictionary(DictionaryToken fontDictionary, InternalParsingOptions parsingOptions)
        {
            lastLoadedFont = (null, null);

            foreach (var pair in fontDictionary.Data)
            {
                if (pair.Value is IndirectReferenceToken objectKey)
                {
                    var reference = objectKey.Data;

                    currentResourceState[NameToken.Create(pair.Key)] = reference;

                    if (loadedFonts.ContainsKey(reference))
                    {
                        continue;
                    }

                    var fontObject = DirectObjectFinder.Get<DictionaryToken>(objectKey, scanner);

                    if (fontObject == null)
                    {
                        //This is a valid use case
                        continue;
                    }

                    try
                    {
                        loadedFonts[reference] = fontFactory.Get(fontObject);
                    }
                    catch
                    {
                        if (!parsingOptions.SkipMissingFonts)
                        {
                            throw;
                        }
                    }
                }
                else if (pair.Value is DictionaryToken fd)
                {
                    loadedDirectFonts[NameToken.Create(pair.Key)] = fontFactory.Get(fd);
                }
                else
                {
                    continue;
                }
            }
        }

        public IFont GetFont(NameToken name)
        {
            if (lastLoadedFont.name == name)
            {
                return lastLoadedFont.font;
            }

            IFont font;
            if (currentResourceState.TryGetValue(name, out var reference))
            {
                loadedFonts.TryGetValue(reference, out font);
            }
            else if (!loadedDirectFonts.TryGetValue(name, out font))
            {
                return null;
            }

            lastLoadedFont = (name, font);

            return font;
        }

        public IFont GetFontDirectly(IndirectReferenceToken fontReferenceToken)
        {
            lastLoadedFont = (null, null);

            if (!DirectObjectFinder.TryGet(fontReferenceToken, scanner, out DictionaryToken fontDictionaryToken))
            {
                throw new PdfDocumentFormatException($"The requested font reference token {fontReferenceToken} wasn't a font.");
            }

            return fontFactory.Get(fontDictionaryToken);
        }

        public bool TryGetNamedColorSpace(NameToken name, out ResourceColorSpace namedToken)
        {
            namedToken = default;

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!namedColorSpaces.TryGetValue(name, out var colorSpaceName))
            {
                return false;
            }

            namedToken = colorSpaceName;

            return true;
        }

        public StreamToken GetXObject(NameToken name)
        {
            var reference = currentResourceState[name];
            return DirectObjectFinder.Get<StreamToken>(new IndirectReferenceToken(reference), scanner);
        }

        public DictionaryToken GetExtendedGraphicsStateDictionary(NameToken name)
        {
            return extendedGraphicsStates[name];
        }

        public DictionaryToken GetMarkedContentPropertiesDictionary(NameToken name)
        {
            return markedContentProperties.TryGetValue(name, out var result) ? result : null;
        }

        public IToken GetByIndirectRefference(IndirectReferenceToken indirectReferenceToken)
        {
            return scanner.Get(indirectReferenceToken.Data)?.Data;
        }

        public Shading GetShadingDictionary(NameToken name)
        {
            return shadingsProperties[name];
        }

        public IReadOnlyDictionary<NameToken, PatternColor> GetPatterns()
        {
            return patternsProperties;
        }
    }
}
