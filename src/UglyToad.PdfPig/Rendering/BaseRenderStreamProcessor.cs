namespace UglyToad.PdfPig.Rendering
{
    using System;
    using System.IO;
    using UglyToad.PdfPig.Annotations;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Filters;
    using UglyToad.PdfPig.Geometry;
    using UglyToad.PdfPig.Graphics;
    using UglyToad.PdfPig.Parser;
    using UglyToad.PdfPig.Tokenization.Scanner;
    using UglyToad.PdfPig.Tokens;
    using UglyToad.PdfPig.XObjects;

    /// <summary>
    /// TODO
    /// </summary>
    public abstract class BaseRenderStreamProcessor : BaseStreamProcessor<MemoryStream>
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="page"></param>
        public BaseRenderStreamProcessor(Page page) : this(page.CropBox.Bounds, page.Content.resourceStore, page.Content.userSpaceUnit,
            page.Rotation, page.pdfScanner, page.Content.pageContentParser, page.Content.filterProvider,
            new PdfVector(page.MediaBox.Bounds.Width, page.MediaBox.Bounds.Height), page.Content.internalParsingOptions)
        { }

        internal BaseRenderStreamProcessor(PdfRectangle cropBox, IResourceStore resourceStore, UserSpaceUnit userSpaceUnit, PageRotationDegrees rotation,
                                  IPdfTokenScanner pdfScanner, IPageContentParser pageContentParser, ILookupFilterProvider filterProvider, PdfVector pageSize,
                                  InternalParsingOptions parsingOptions)
          : base(cropBox, resourceStore, userSpaceUnit, rotation, pdfScanner, pageContentParser, filterProvider, pageSize, parsingOptions)
        { }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public abstract MemoryStream GetImage(double scale);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns></returns>
        protected static DictionaryToken GetAppearance(Annotation annotation)
        {
            if (annotation.AnnotationDictionary.TryGet<DictionaryToken>(NameToken.Ap, out var appearance))
            {
                return appearance;
            }
            return null;
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns></returns>
        protected StreamToken GetNormalAppearanceAsStream(Annotation annotation)
        {
            var dict = GetAppearance(annotation);

            if (dict == null)
            {
                return null;
            }

            // get Normal Appearance
            if (!dict.Data.TryGetValue(NameToken.N, out var data))
            {
                return null;
            }

            if (data is IndirectReferenceToken irt)
            {
                data = Get(irt);
                if (data is null)
                {
                    return null;
                }
            }

            if (data is StreamToken streamToken)
            {
                return streamToken;
            }
            else if (data is DictionaryToken dictionaryToken)
            {
                return null; //return dictionaryToken;
            }
            else if (data is ObjectToken objectToken)
            {
                if (objectToken.Data is StreamToken streamToken2)
                {
                    return streamToken2;
                }
                else if (objectToken.Data is DictionaryToken dictionaryToken2)
                {
                    return null;  //return dictionaryToken2;
                }
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns></returns>
        protected DictionaryToken GetNormalAppearance(Annotation annotation)
        {
            var dict = GetAppearance(annotation);

            if (dict == null)
            {
                return null;
            }

            // get Normal Appearance
            if (!dict.Data.TryGetValue(NameToken.N, out var data))
            {
                return null;
            }

            if (data is IndirectReferenceToken irt)
            {
                data = Get(irt);
                if (data is null)
                {
                    return null;
                }
            }

            if (data is StreamToken streamToken)
            {
                return streamToken.StreamDictionary;
            }
            else if (data is DictionaryToken dictionaryToken)
            {
                return dictionaryToken;
            }
            else if (data is ObjectToken objectToken)
            {
                if (objectToken.Data is StreamToken streamToken2)
                {
                    return streamToken2.StreamDictionary;
                }
                else if (objectToken.Data is DictionaryToken dictionaryToken2)
                {
                    return dictionaryToken2;
                }
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="xObjectContentRecord"></param>
        /// <returns></returns>
        protected IPdfImage GetImageFromXObject(XObjectContentRecord xObjectContentRecord)
        {
            return XObjectFactory.ReadImage(xObjectContentRecord, pdfScanner, filterProvider, resourceStore);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="nameToken"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        protected IToken Get(IndirectReferenceToken nameToken)
        {
            return base.pdfScanner.Get(nameToken.Data);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <param name="tl"></param>
        /// <returns></returns>
        protected static (double x, double y) TransformPoint(TransformationMatrix first, TransformationMatrix second, TransformationMatrix third, PdfPoint tl)
        {
            var topLeftX = tl.X;
            var topLeftY = tl.Y;

            // First
            var x = first.A * topLeftX + first.C * topLeftY + first.E;
            var y = first.B * topLeftX + first.D * topLeftY + first.F;
            topLeftX = x;
            topLeftY = y;

            // Second
            x = second.A * topLeftX + second.C * topLeftY + second.E;
            y = second.B * topLeftX + second.D * topLeftY + second.F;
            topLeftX = x;
            topLeftY = y;

            // Third
            x = third.A * topLeftX + third.C * topLeftY + third.E;
            y = third.B * topLeftX + third.D * topLeftY + third.F;
            topLeftX = x;
            topLeftY = y;

            return (topLeftX, topLeftY);
        }
    }
}
