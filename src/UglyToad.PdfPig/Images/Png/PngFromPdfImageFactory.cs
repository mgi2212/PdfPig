namespace UglyToad.PdfPig.Images.Png
{
    using Content;
    using Graphics.Colors;
    using UglyToad.PdfPig.Core;

    internal static class PngFromPdfImageFactory
    {
        public static bool TryGenerate(IPdfImage image, out byte[] bytes)
        {
            bytes = null;

            var hasValidDetails = image.ColorSpaceDetails != null &&
                                  !(image.ColorSpaceDetails is UnsupportedColorSpaceDetails);

            if (!hasValidDetails)
            {
                return false;
            }

            var actualColorSpace = image.ColorSpaceDetails;

            var isColorSpaceSupported = actualColorSpace.Type != ColorSpace.Pattern;

            if (!isColorSpaceSupported || !image.TryGetBytes(out var bytesPure))
            {
                return false;
            }

            try
            {
                bytesPure = ColorSpaceDetailsByteConverter.Convert(image.ColorSpaceDetails, bytesPure,
                    image.BitsPerComponent, image.WidthInSamples, image.HeightInSamples);

                // TODO - why should that ever be false??
                // the below works but makes test fail,
                // we need to find a way to check for alpha channel
                bool hasAlphaChannel = image.SMask != null; // TODO - a guess

                var requiredSize = (image.WidthInSamples * image.HeightInSamples * actualColorSpace.NumberOfColorComponents);

                var actualSize = bytesPure.Count;
                var isCorrectlySized = bytesPure.Count == requiredSize ||
                    // Spec, p. 37: "...error if the stream contains too much data, with the exception that
                    // there may be an extra end-of-line marker..."
                    (actualSize == requiredSize + 1 && bytesPure[actualSize - 1] == ReadHelper.AsciiLineFeed) ||
                    (actualSize == requiredSize + 1 && bytesPure[actualSize - 1] == ReadHelper.AsciiCarriageReturn) ||
                    // The combination of a CARRIAGE RETURN followed immediately by a LINE FEED is treated as one EOL marker.
                    (actualSize == requiredSize + 2 &&
                        bytesPure[actualSize - 2] == ReadHelper.AsciiCarriageReturn &&
                        bytesPure[actualSize - 1] == ReadHelper.AsciiLineFeed);

                if (!isCorrectlySized)
                {
                    return false;
                }

                // The below should actually go into ColorSpaceDetailsByteConverter.Convert(..) ??
                bytes = actualColorSpace.GetPngImage(bytesPure, image.HeightInSamples, image.WidthInSamples, hasAlphaChannel);

                return true;
            }
            catch
            {
                // TODO - logs
                // ignored.
            }

            return false;
        }
    }
}
