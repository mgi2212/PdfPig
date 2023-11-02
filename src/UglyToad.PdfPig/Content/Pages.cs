namespace UglyToad.PdfPig.Content
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Outline.Destinations;
    using Parser;
    using Tokenization.Scanner;
    using Tokens;
    using Util;

    internal class Pages
    {
        private readonly Dictionary<Type, object> pageFactoryCache;
        private readonly IPageFactory<Page> defaultPageFactory;
        private readonly IPdfTokenScanner pdfScanner;
        private readonly Dictionary<int, PageTreeNode> pagesByNumber;

        public int Count => pagesByNumber.Count;

        /// <summary>
        /// The page tree for this document containing all pages, page numbers and their dictionaries.
        /// </summary>
        public PageTreeNode PageTree { get; }

        internal Pages(IPageFactory<Page> pageFactory,
            IPdfTokenScanner pdfScanner,
            PageTreeNode pageTree,
            Dictionary<int, PageTreeNode> pagesByNumber)
        {
            pageFactoryCache = new Dictionary<Type, object>();

            defaultPageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
            this.pdfScanner = pdfScanner ?? throw new ArgumentNullException(nameof(pdfScanner));
            this.pagesByNumber = pagesByNumber;
            PageTree = pageTree;

            AddPageFactory(defaultPageFactory);
        }

        internal Page GetPage(int pageNumber, NamedDestinations namedDestinations, ParsingOptions parsingOptions) =>
            GetPage(defaultPageFactory, pageNumber, namedDestinations, parsingOptions);

        internal TPage GetPage<TPage>(int pageNumber, NamedDestinations namedDestinations, ParsingOptions parsingOptions)
        {
            if (pageFactoryCache.TryGetValue(typeof(TPage), out var f) && f is IPageFactory<TPage> pageFactory)
            {
                return GetPage(pageFactory, pageNumber, namedDestinations, parsingOptions);
            }

            throw new InvalidOperationException($"Could not find page factory of type '{typeof(IPageFactory<TPage>)}' for page type {typeof(TPage)}.");
        }

        private TPage GetPage<TPage>(IPageFactory<TPage> pageFactory,
            int pageNumber,
            NamedDestinations namedDestinations,
            ParsingOptions parsingOptions)
        {
            if (pageNumber <= 0 || pageNumber > Count)
            {
                parsingOptions.Logger.Error($"Page {pageNumber} requested but is out of range.");

                throw new ArgumentOutOfRangeException(nameof(pageNumber),
                    $"Page number {pageNumber} invalid, must be between 1 and {Count}.");
            }

            var pageNode = GetPageNode(pageNumber);
            var pageStack = new Stack<PageTreeNode>();

            var currentNode = pageNode;
            while (currentNode != null)
            {
                pageStack.Push(currentNode);
                currentNode = currentNode.Parent;
            }

            var pageTreeMembers = new PageTreeMembers();

            while (pageStack.Count > 0)
            {
                currentNode = pageStack.Pop();

                if (currentNode.NodeDictionary.TryGet(NameToken.Resources, pdfScanner, out DictionaryToken resourcesDictionary))
                {
                    pageTreeMembers.ParentResources.Enqueue(resourcesDictionary);
                }

                if (currentNode.NodeDictionary.TryGet(NameToken.MediaBox, pdfScanner, out ArrayToken mediaBox))
                {
                    pageTreeMembers.MediaBox = new MediaBox(mediaBox.ToRectangle(pdfScanner));
                }

                if (currentNode.NodeDictionary.TryGet(NameToken.Rotate, pdfScanner, out NumericToken rotateToken))
                {
                    pageTreeMembers.Rotation = rotateToken.Int;
                }
            }

            var page = pageFactory.Create(
                pageNumber,
                pageNode.NodeDictionary,
                pageTreeMembers,
                namedDestinations);

            return page;
        }

        internal void AddPageFactory<TPage>(IPageFactory<TPage> pageFactory)
        {
            Type type = typeof(TPage);
            if (pageFactoryCache.ContainsKey(type))
            {
                throw new InvalidOperationException($"Could not add page factory for page type '{type}' as it was already added.");
            }

            pageFactoryCache.Add(type, pageFactory);
        }

        internal void AddPageFactory<TPage, TPageFactory>() where TPageFactory : IPageFactory<TPage>
        {
            var factory = (PageFactory)this.defaultPageFactory;

            var pageFactory = (IPageFactory<TPage>)Activator.CreateInstance(typeof(TPageFactory),
                factory.PdfScanner,
                factory.ResourceStore,
                factory.FilterProvider,
                factory.PageContentParser,
                factory.ParsingOptions);

            if (pageFactory == null)
            {
                throw new InvalidOperationException($"Something wrong happened while creating page factory of type '{typeof(TPageFactory)}' for page type '{typeof(TPage)}'.");
            }

            AddPageFactory(pageFactory);
        }

        internal PageTreeNode GetPageNode(int pageNumber)
        {
            if (!pagesByNumber.TryGetValue(pageNumber, out var node))
            {
                throw new InvalidOperationException($"Could not find page node by number for: {pageNumber}.");
            }

            return node;
        }

        internal PageTreeNode GetPageByReference(IndirectReference reference)
        {
            foreach (var page in pagesByNumber)
            {
                if (page.Value.Reference.Equals(reference))
                {
                    return page.Value;
                }
            }

            return null;
        }
    }
}
