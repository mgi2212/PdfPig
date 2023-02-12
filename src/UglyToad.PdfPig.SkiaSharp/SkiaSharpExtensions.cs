namespace UglyToad.PdfPig.SkiaSharp
{
    using global::SkiaSharp;
    using System.Collections.Generic;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Graphics;
    using UglyToad.PdfPig.Graphics.Colors;
    using static UglyToad.PdfPig.Core.PdfSubpath;

    internal static class SkiaSharpExtensions
    {
        public static SKPath PdfPathToGraphicsPath(this PdfPath path, int height, double scale)
        {
            var gp = PdfSubpathsToGraphicsPath(path, height, scale);
            gp.FillType = path.FillingRule == FillingRule.NonZeroWinding ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
            return gp;
        }

        public static SKPath PdfSubpathsToGraphicsPath(this IReadOnlyList<PdfSubpath> pdfSubpaths, int height, double scale)
        {
            var gp = new SKPath();

            foreach (var subpath in pdfSubpaths)
            {
                foreach (var c in subpath.Commands)
                {
                    if (c is Move move)
                    {
                        gp.MoveTo(move.Location.ToSKPoint(height, scale));
                    }
                    else if (c is Line line)
                    {
                        gp.LineTo(line.To.ToSKPoint(height, scale));
                    }
                    else if (c is BezierCurve curve)
                    {
                        gp.CubicTo(curve.FirstControlPoint.ToSKPoint(height, scale),
                            curve.SecondControlPoint.ToSKPoint(height, scale),
                            curve.EndPoint.ToSKPoint(height, scale));
                    }
                    else if (c is Close)
                    {
                        gp.Close();
                    }
                }
            }
            return gp;
        }

        public static SKPoint ToSKPoint(this PdfPoint pdfPoint, int height, double scale)
        {
            return new SKPoint((float)(pdfPoint.X * scale), (float)(height - pdfPoint.Y * scale));
        }

        /// <summary>
        /// Default to Black.
        /// </summary>
        /// <param name="pdfColor"></param>
        /// <returns></returns>
        public static SKColor ToSystemColor(this IColor pdfColor)
        {
            if (pdfColor != null)
            {
                var colorRgb = pdfColor.ToRGBValues();
                if (pdfColor is AlphaColor alphaColor)
                {
                    return new SKColor((byte)(colorRgb.r * 255), (byte)(colorRgb.g * 255), (byte)(colorRgb.b * 255), (byte)(alphaColor.A * 255));
                }
                return new SKColor((byte)(colorRgb.r * 255), (byte)(colorRgb.g * 255), (byte)(colorRgb.b * 255));
            }
            return SKColors.Black;
        }
    }
}
