namespace UglyToad.PdfPig.Content
{
    using Outline.Destinations;
    using Tokens;

    /// <summary>
    /// Page factory interface.
    /// </summary>
    public interface IPageFactory<TPage>
    {
        /// <summary>
        /// Create the page.
        /// </summary>
        TPage Create(int number,
            DictionaryToken dictionary,
            PageTreeMembers pageTreeMembers,
            NamedDestinations namedDestinations);
    }
}