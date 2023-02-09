namespace UglyToad.PdfPig.Rendering
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UglyToad.PdfPig.Graphics;

    /// <summary>
    /// TODO
    /// </summary>
    public class PdfRenderer<T> where T : BaseStreamProcessor<MemoryStream>, new()
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="pdfDocument"></param>
        /// <returns></returns>
        public T Create(PdfDocument pdfDocument)
        {
            var proc = new T();

            return proc;
        }
    }
}
