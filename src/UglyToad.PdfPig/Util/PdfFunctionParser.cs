namespace UglyToad.PdfPig.Util
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UglyToad.PdfPig.Filters;
    using UglyToad.PdfPig.Functions;
    using UglyToad.PdfPig.Parser.Parts;
    using UglyToad.PdfPig.Tokenization.Scanner;
    using UglyToad.PdfPig.Tokens;

    internal static class PdfFunctionParser
    {
        public static PdfFunction Create(IToken function, IPdfTokenScanner scanner, ILookupFilterProvider filterProvider)
        {
            StreamToken functionStream = null;
            DictionaryToken functionDictionary;
            if (function is NameToken identity && identity == NameToken.Identity)
            {
                return new PdfFunctionTypeIdentity(null);
            }
            else if (DirectObjectFinder.TryGet(function, scanner, out StreamToken fs))
            {
                functionDictionary = fs.StreamDictionary;
                functionStream = new StreamToken(fs.StreamDictionary, fs.Decode(filterProvider, scanner));
            }
            else if (DirectObjectFinder.TryGet(function, scanner, out DictionaryToken fd))
            {
                functionDictionary = fd;
            }
            else
            {
                throw new InvalidOperationException("description to do");
            }

            Dictionary<NameToken, IToken> values = new Dictionary<NameToken, IToken>();
            foreach (var pair in functionDictionary.Data)
            {
                var name = NameToken.Create(pair.Key);
                switch (name)
                {
                    // TODO - improve that, this is bad code
                    case "Bounds":
                    case "Encode":
                    case "C0":
                    case "C1":
                    case "Range":
                        values[name] = DirectObjectFinder.Get<ArrayToken>(pair.Value, scanner);
                        break;

                    default:
                        values[name] = pair.Value;
                        break;
                }
            }

            functionDictionary = new DictionaryToken(values);

            int functionType = (functionDictionary.Data[NameToken.FunctionType] as NumericToken).Int;

            switch (functionType)
            {
                case 0:
                    if (functionStream == null)
                    {
                        throw new NotImplementedException("PdfFunctionType0 not stream");
                    }
                    return new PdfFunctionType0(functionStream);

                case 2:
                    return new PdfFunctionType2(functionDictionary);

                case 3:
                    var functions = new List<PdfFunction>();
                    if (functionDictionary.TryGet<ArrayToken>(NameToken.Functions, scanner, out var functionsToken))
                    {
                        foreach (IToken token in functionsToken.Data)
                        {
                            functions.Add(Create(token, scanner, filterProvider));
                        }
                    }
                    return new PdfFunctionType3(functionDictionary, functions);

                case 4:
                    if (functionStream == null)
                    {
                        throw new NotImplementedException("PdfFunctionType0 not stream");
                    }
                    return new PdfFunctionType4(functionStream);

                default:
                    throw new IOException("Error: Unknown function type " + functionType);
            }
        }
    }
}
