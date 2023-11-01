namespace UglyToad.PdfPig.Parser
{
    using System.Collections.Generic;
    using Annotations;
    using Content;
    using Core;
    using Filters;
    using Geometry;
    using Graphics;
    using Graphics.Operations;
    using Outline.Destinations;
    using Tokenization.Scanner;
    using Tokens;

    internal class PageFactory : BasePageFactory<Page>
    {
        public PageFactory(
            IPdfTokenScanner pdfScanner,
            IResourceStore resourceStore,
            ILookupFilterProvider filterProvider,
            IPageContentParser pageContentParser,
            ParsingOptions parsingOptions)
            : base(pdfScanner, resourceStore, filterProvider, pageContentParser, parsingOptions)
        {
        }

        private PageContent GetContent(
            int pageNumber,
            IReadOnlyList<byte> contentBytes,
            CropBox cropBox,
            UserSpaceUnit userSpaceUnit,
            PageRotationDegrees rotation,
            TransformationMatrix initialMatrix,
            ParsingOptions parsingOptions)
        {
            var operations = PageContentParser.Parse(pageNumber,
                new ByteArrayInputBytes(contentBytes),
                parsingOptions.Logger);

            var context = new ContentStreamProcessor(
                pageNumber,
                ResourceStore,
                PdfScanner,
                PageContentParser,
                FilterProvider,
                cropBox,
                userSpaceUnit,
                rotation,
                initialMatrix,
                parsingOptions);

            return context.Process(pageNumber, operations);
        }

        protected override Page ProcessPage(int pageNumber,
            DictionaryToken dictionary,
            NamedDestinations namedDestinations,
            MediaBox mediaBox,
            CropBox cropBox,
            UserSpaceUnit userSpaceUnit,
            PageRotationDegrees rotation,
            TransformationMatrix initialMatrix,
            IReadOnlyList<byte> contentBytes)
        {
            PageContent content;
            if (contentBytes == null)
            {
                content = new PageContent(EmptyArray<IGraphicsStateOperation>.Instance,
                    EmptyArray<Letter>.Instance,
                    EmptyArray<PdfPath>.Instance,
                    EmptyArray<Union<XObjectContentRecord, InlineImage>>.Instance,
                    EmptyArray<MarkedContentElement>.Instance,
                    PdfScanner,
                    FilterProvider,
                    ResourceStore);
            }
            else
            {
                content = GetContent(pageNumber,
                    contentBytes,
                    cropBox,
                    userSpaceUnit,
                    rotation,
                    initialMatrix,
                    ParsingOptions);
            }

            var annotationProvider = new AnnotationProvider(PdfScanner,
                dictionary,
                initialMatrix,
                namedDestinations,
                ParsingOptions.Logger);

            return new Page(pageNumber,
                dictionary,
                mediaBox,
                cropBox,
                rotation,
                content,
                annotationProvider,
                PdfScanner);
        }
    }
}
