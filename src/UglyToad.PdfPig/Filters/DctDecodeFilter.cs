namespace UglyToad.PdfPig.Filters
{
    using BigGustave.Jpgs;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Tokens;
    using static System.Net.Mime.MediaTypeNames;

    internal class DctDecodeFilter : IFilter
    {
        /// <inheritdoc />
        public bool IsSupported { get; } = true;

        /// <inheritdoc />
        public byte[] Decode(IReadOnlyList<byte> input, DictionaryToken streamDictionary, int filterIndex)
        {
            //throw new NotSupportedException("The DST (Discrete Cosine Transform) Filter indicates data is encoded in JPEG format. " +
            //                                "This filter is not currently supported but the raw data can be supplied to JPEG supporting libraries.");

            var decodeParms = DecodeParameterResolver.GetFilterParameters(streamDictionary, filterIndex);

            using (var ms = new MemoryStream(input.ToArray()))
            {
                var jpg = JpgOpener.Open(ms, true);

                byte[] output = new byte[3 * jpg.Width * jpg.Height];

                int i = 0;
                for (int col = 0; col < jpg.Height; col++)
                {
                    for (int row = 0; row < jpg.Width; row++)
                    {
                        var pixel = jpg.GetPixel(row, col);
                        output[i++] = pixel.R;
                        output[i++] = pixel.G;
                        output[i++] = pixel.B;
                    }
                }


                return output; //jpg.rawData;
            }
        }
    }
}
