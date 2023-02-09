namespace UglyToad.PdfPig.Rendering
{
    using System;
    using System.IO;
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
        protected IToken TryGet(IndirectReferenceToken nameToken)
        {
            var test = base.pdfScanner.Get(nameToken.Data);
            throw new NotImplementedException();
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
