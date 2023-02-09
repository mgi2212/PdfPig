namespace UglyToad.PdfPig.SkiaSharp
{
    using global::SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Geometry;
    using UglyToad.PdfPig.Graphics;
    using UglyToad.PdfPig.Graphics.Colors;
    using UglyToad.PdfPig.PdfFonts;
    using static UglyToad.PdfPig.Core.PdfSubpath;

    public class SkiaSharpProcessor : BaseDrawingProcessor
    {
        private int _height;
        private int _width;
        private double _mult;
        private SKCanvas _canvas;

        private SKRect _baseClipRect;
        public SkiaSharpProcessor() : base(new SkiaSharpLogger())
        { }

        public override MemoryStream DrawPage(Page page, double scale)
        {
            var ms = new MemoryStream();
            base.Init(page);
            _mult = scale;

            _height = ToInt(page.Height);
            _width = ToInt(page.Width);

            _baseClipRect = new SKRect(0, 0, _width, _height);

            using (var bitmap = new SKBitmap(_width, _height))
            using (_canvas = new SKCanvas(bitmap))
            {
                UpdateClipPath();

                using (var paint = new SKPaint() { Style = SKPaintStyle.Fill, Color = SKColors.White })
                {
                    _canvas.DrawRect(0, 0, _width, _height, paint);
                }

                foreach (var stateOperation in page.Operations)
                {
                    stateOperation.Run(this);
                }

                using (SKData d = bitmap.Encode(SKEncodedImageFormat.Png, 100))
                {
                    d.SaveTo(ms);
                }
            }
            ms.Position = 0;
            return ms;
        }

        public override void DrawImage(IPdfImage image)
        {
            var upperLeft = image.Bounds.TopLeft.ToPointF(_height, _mult);
            var destRect = new SKRect(upperLeft.X, upperLeft.Y,
                             upperLeft.X + (float)(image.Bounds.Width * _mult),
                             upperLeft.Y + (float)(image.Bounds.Height * _mult));
            byte[] bytes = null;

            if (image.Bounds.Rotation != 0)
            {
                _canvas.RotateDegrees((float)-image.Bounds.Rotation, upperLeft.X, upperLeft.Y);
            }

            try
            {
                if (!image.TryGetPng(out bytes))
                {
                    if (image.TryGetBytes(out var bytesL))
                    {
                        bytes = bytesL.ToArray();
                    }
                }

                if (bytes?.Length > 0)
                {
                    try
                    {
                        using (var bitmap = SKBitmap.Decode(bytes))
                        {
                            _canvas.DrawBitmap(bitmap, destRect);
                        }
                        return;
                    }
                    catch (Exception)
                    {
                        // Try with raw bytes
                        using (var bitmap = SKBitmap.Decode(image.RawBytes.ToArray()))
                        {
                            _canvas.DrawBitmap(bitmap, destRect);
                        }
                    }
                }
                else
                {
                    using (var bitmap = SKBitmap.Decode(image.RawBytes.ToArray()))
                    {
                        _canvas.DrawBitmap(bitmap, destRect);
                    }
                }
            }
            catch (Exception)
            {
#if DEBUG
                var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = new SKColor(SKColors.GreenYellow.Red, SKColors.GreenYellow.Green, SKColors.GreenYellow.Blue, 40)
                };
                _canvas.DrawRect(destRect, paint);
                paint.Dispose();
#endif
            }
            finally
            {
                _canvas.ResetMatrix();
            }
        }

        public override void DrawLetter(IReadOnlyList<PdfSubpath> pdfSubpaths, IColor color,
            TransformationMatrix renderingMatrix, TransformationMatrix textMatrix, TransformationMatrix transformationMatrix)
        {
            if (pdfSubpaths == null)
            {
                throw new ArgumentException("DrawLetter(): empty path");
            }

            using (SKPath gp = new SKPath() { FillType = SKPathFillType.EvenOdd })
            {
                foreach (var subpath in pdfSubpaths)
                {
                    foreach (var c in subpath.Commands)
                    {
                        if (c is Move move)
                        {
                            var loc = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, move.Location);
                            gp.MoveTo((float)(loc.x * _mult), (float)(_height - loc.y * _mult));
                        }
                        else if (c is Line line)
                        {
                            var to = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, line.To);
                            gp.LineTo((float)(to.x * _mult), (float)(_height - to.y * _mult));
                        }
                        else if (c is BezierCurve curve)
                        {
                            var first = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, curve.FirstControlPoint);
                            var second = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, curve.SecondControlPoint);
                            var end = TransformPoint(renderingMatrix, textMatrix, transformationMatrix, curve.EndPoint);

                            gp.CubicTo((float)(first.x * _mult), (float)(_height - first.y * _mult),
                                       (float)(second.x * _mult), (float)(_height - second.y * _mult),
                                       (float)(end.x * _mult), (float)(_height - end.y * _mult));
                        }
                        else if (c is Close)
                        {
                            gp.Close();
                        }
                    }
                }

                SKPaint fillBrush = new SKPaint()
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Black
                };

                if (color != null)
                {
                    fillBrush.Color = color.ToSystemColor();
                }

                _canvas.DrawPath(gp, fillBrush);
                fillBrush.Dispose();
            }
        }

        public override void DrawLetter(string value, PdfRectangle glyphRectangle, PdfPoint startBaseLine, PdfPoint endBaseLine,
            double width, double fontSize, FontDetails font, IColor color, double pointSize)
        {
            var baseLine = startBaseLine.ToPointF(_height, _mult);
            if (glyphRectangle.Rotation != 0)
            {
                _canvas.RotateDegrees((float)-glyphRectangle.Rotation, baseLine.X, baseLine.Y);
            }

#if DEBUG
            var glyphRectangleNormalise = glyphRectangle.Normalise();

            var upperLeftNorm = glyphRectangleNormalise.TopLeft.ToPointF(_height, _mult);
            var bottomLeftNorm = glyphRectangleNormalise.BottomLeft.ToPointF(_height, _mult);
            SKRect rect = new SKRect(upperLeftNorm.X, upperLeftNorm.Y,
                upperLeftNorm.X + (float)(glyphRectangleNormalise.Width * _mult),
                upperLeftNorm.Y + (float)(glyphRectangleNormalise.Height * _mult));
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(SKColors.Red.Red, SKColors.Red.Green, SKColors.Red.Blue, 40)
            };

            _canvas.DrawRect(rect, paint);

            paint.Style = SKPaintStyle.Stroke;

            _canvas.DrawRect(rect, paint);
            paint.Dispose();
#endif

            var style = SKFontStyle.Normal;
            if (font.IsBold && font.IsItalic)
            {
                style = SKFontStyle.BoldItalic;
            }
            else if (font.IsBold)
            {
                style = SKFontStyle.Bold;
            }
            else if (font.IsItalic)
            {
                style = SKFontStyle.Italic;
            }

            var drawFont = SKTypeface.FromFamilyName(CleanFontName(font.Name), style);
            var fontPaint = new SKPaint(drawFont.ToFont((float)(pointSize * _mult)))
            {
                Color = color.ToSystemColor()
            };

            //System.Diagnostics.Debug.WriteLine($"DrawLetter: '{font.Name}'\t{fontSize} -> Font name used: '{drawFont.FamilyName}'");

            _canvas.DrawText(value, baseLine, fontPaint);
            _canvas.ResetMatrix();

            fontPaint.Dispose();
            drawFont.Dispose();
            style.Dispose();
        }

        public override void DrawPath(PdfPath path)
        {
            using (var gp = path.PdfPathToGraphicsPath(_height, _mult))
            using (SKPaint paint = new SKPaint())
            {
                if (path.IsStroked)
                {
                    float lineWidth = Math.Max((float)0.5, (float)path.LineWidth) * (float)_mult; // A guess

                    paint.Color = path.StrokeColor.ToSystemColor();
                    paint.Style = SKPaintStyle.Stroke;
                    paint.StrokeWidth = lineWidth;

                    switch (path.LineJoinStyle)
                    {
                        case PdfPig.Graphics.Core.LineJoinStyle.Bevel:
                            paint.StrokeJoin = SKStrokeJoin.Bevel;
                            break;

                        case PdfPig.Graphics.Core.LineJoinStyle.Miter:
                            paint.StrokeJoin = SKStrokeJoin.Miter;
                            break;

                        case PdfPig.Graphics.Core.LineJoinStyle.Round:
                            paint.StrokeJoin = SKStrokeJoin.Round;
                            break;
                    }

                    switch (path.LineCapStyle)
                    {
                        case PdfPig.Graphics.Core.LineCapStyle.Butt:
                            paint.StrokeCap = SKStrokeCap.Butt;
                            break;

                        case PdfPig.Graphics.Core.LineCapStyle.ProjectingSquare:
                            paint.StrokeCap = SKStrokeCap.Square;
                            break;

                        case PdfPig.Graphics.Core.LineCapStyle.Round:
                            paint.StrokeCap = SKStrokeCap.Round;
                            break;
                    }

                    if (path.LineDashPattern.HasValue)
                    {
                        //* https://docs.microsoft.com/en-us/dotnet/api/system.drawing.pen.dashpattern?view=dotnet-plat-ext-3.1
                        //* The elements in the dashArray array set the length of each dash and space in the dash pattern. 
                        //* The first element sets the length of a dash, the second element sets the length of a space, the
                        //* third element sets the length of a dash, and so on. Consequently, each element should be a 
                        //* non-zero positive number.

                        if (path.LineDashPattern.Value.Array.Count == 1)
                        {
                            List<float> pattern = new List<float>();
                            var v = path.LineDashPattern.Value.Array[0];
                            pattern.Add((float)((double)v / _mult));
                            pattern.Add((float)((double)v / _mult));
                            paint.PathEffect = SKPathEffect.CreateDash(pattern.ToArray(), (float)v); // TODO
                        }
                        else if (path.LineDashPattern.Value.Array.Count > 0)
                        {
                            List<float> pattern = new List<float>();
                            for (int i = 0; i < path.LineDashPattern.Value.Array.Count; i++)
                            {
                                var v = path.LineDashPattern.Value.Array[i];
                                if (v == 0)
                                {
                                    pattern.Add((float)(1.0 / 72.0 * _mult));
                                }
                                else
                                {
                                    pattern.Add((float)((double)v / _mult));
                                }
                            }
                            //pen.DashPattern = pattern.ToArray(); // TODO
                            paint.PathEffect = SKPathEffect.CreateDash(pattern.ToArray(), pattern[0]); // TODO
                        }
                        //pen.DashOffset = path.LineDashPattern.Value.Phase; // mult?? //  // TODO
                    }

                    _canvas.DrawPath(gp, paint);
                }

                if (path.IsFilled)
                {
                    paint.Color = path.FillColor.ToSystemColor();
                    paint.Style = SKPaintStyle.Fill;
                    _canvas.DrawPath(gp, paint);
                }
            }
        }

        public override void UpdateClipPath()
        {
            ResetClippingPathToOriginal();

            using (var clipping = GetCurrentState().CurrentClippingPath.PdfPathToGraphicsPath(_height, _mult))
            {
                _canvas.ClipPath(clipping);
            }
        }

        private void ResetClippingPathToOriginal()
        {
            _canvas.Restore();
            _canvas.ClipRect(_baseClipRect);
            _canvas.Save();
        }

        private int ToInt(double value)
        {
            return (int)Math.Ceiling(value * _mult);
        }

        private static string CleanFontName(string font)
        {
            if (font.Length > 7 && font[6].Equals('+'))
            {
                string subset = font.Substring(0, 6);
                if (subset.Equals(subset.ToUpper()))
                {
                    return font.Split('+')[1];
                }
            }

            return font;
        }
    }
}
