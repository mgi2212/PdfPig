namespace UglyToad.PdfPig.SkiaSharp
{
    using global::SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Geometry;
    using UglyToad.PdfPig.Graphics;
    using UglyToad.PdfPig.Graphics.Colors;
    using UglyToad.PdfPig.Graphics.Operations;
    using UglyToad.PdfPig.PdfFonts;
    using UglyToad.PdfPig.Rendering;
    using UglyToad.PdfPig.Tokens;
    using static UglyToad.PdfPig.Core.PdfSubpath;

    public class SkiaSharpProcessor2 : BaseRenderStreamProcessor
    {
        private int _height;
        private int _width;
        private double _mult;
        private SKCanvas _canvas;
        //private SKRect _baseClipRect;
        private Page _page;

        public SkiaSharpProcessor2(Page page) : base(page)
        {
            _page = page;
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

        public MemoryStream GetImage(double scale)
        {
            _mult = scale;
            _height = ToInt(_page.Height);
            _width = ToInt(_page.Width);
            return Process(-1, _page.Operations);
        }

        public override MemoryStream Process(int pageNumberCurrent, IReadOnlyList<IGraphicsStateOperation> operations)
        {
            var ms = new MemoryStream();

            CloneAllStates();

            using (var bitmap = new SKBitmap(_width, _height))
            using (_canvas = new SKCanvas(bitmap))
            {
                using (var paint = new SKPaint() { Style = SKPaintStyle.Fill, Color = SKColors.White })
                {
                    _canvas.DrawRect(0, 0, _width, _height, paint);
                }

                ProcessOperations(operations);

                DrawAnnotations();

                using (SKData d = bitmap.Encode(SKEncodedImageFormat.Png, 100))
                {
                    d.SaveTo(ms);
                }
            }
            ms.Position = 0;
            return ms;
        }

        private void DrawAnnotations()
        {
            foreach (var annotation in _page.ExperimentalAccess.GetAnnotations())
            {
                if (annotation.AnnotationDictionary.TryGet(NameToken.Ap, out var appearance))
                {
                    if (appearance is DictionaryToken dic)
                    {
                        if (dic.Data.TryGetValue(NameToken.N, out var data))
                        {
                            if (data is IndirectReferenceToken irt)
                            {
                                base.TryGet(irt);
                            }
                        }
                    }
                    //if (annotation.AnnotationDictionary.TryGet(NameToken.Matrix, out var matrix))
                }

                PdfRectangle rect = annotation.Rectangle;
                float upperLeftX = (float)rect.BottomLeft.X;
                float upperLeftY = (float)rect.TopLeft.Y;

                var destRect = new SKRect(upperLeftX, (float)rect.TopLeft.Y,
                                          upperLeftX + (float)(rect.Width * _mult),
                                          upperLeftY + (float)(rect.Height * _mult));

                SKPaint fillBrush = new SKPaint()
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Black
                };

                _canvas.DrawRect(destRect, fillBrush);
                fillBrush.Dispose();
            }
        }

        public override void ShowGlyph(IFont font, IColor color, double fontSize, double pointSize, int code, string unicode, long currentOffset,
            TransformationMatrix renderingMatrix, TransformationMatrix textMatrix, TransformationMatrix transformationMatrix, CharacterBoundingBox characterBoundingBox)
        {
            if (font.TryGetNormalisedPath(code, out var path))
            {
                ShowVectorFontGlyph(path, color, renderingMatrix, textMatrix, transformationMatrix);
            }
            else
            {
                ShowNonVectorFontGlyph(font, color, pointSize, unicode, renderingMatrix, textMatrix, transformationMatrix, characterBoundingBox);
            }
        }

        private void ShowVectorFontGlyph(IReadOnlyList<PdfSubpath> path, IColor color,
            TransformationMatrix renderingMatrix, TransformationMatrix textMatrix, TransformationMatrix transformationMatrix)
        {
            // Vector based font
            using (var gp = new SKPath() { FillType = SKPathFillType.EvenOdd })
            {
                foreach (var subpath in path)
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

        private void ShowNonVectorFontGlyph(IFont font, IColor color, double pointSize, string unicode,
            TransformationMatrix renderingMatrix, TransformationMatrix textMatrix, TransformationMatrix transformationMatrix,
            CharacterBoundingBox characterBoundingBox)
        {
            // Not vector based font
            var transformedGlyphBounds = PerformantRectangleTransformer
                .Transform(renderingMatrix, textMatrix, transformationMatrix, characterBoundingBox.GlyphBounds);

            var transformedPdfBounds = PerformantRectangleTransformer
                .Transform(renderingMatrix, textMatrix, transformationMatrix, new PdfRectangle(0, 0, characterBoundingBox.Width, 0));

            var startBaseLine = transformedPdfBounds.BottomLeft.ToPointF(_height, _mult);
            if (transformedGlyphBounds.Rotation != 0)
            {
                _canvas.RotateDegrees((float)-transformedGlyphBounds.Rotation, startBaseLine.X, startBaseLine.Y);
            }

#if DEBUG
            var glyphRectangleNormalise = transformedGlyphBounds.Normalise();

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
            if (font.Details.IsBold && font.Details.IsItalic)
            {
                style = SKFontStyle.BoldItalic;
            }
            else if (font.Details.IsBold)
            {
                style = SKFontStyle.Bold;
            }
            else if (font.Details.IsItalic)
            {
                style = SKFontStyle.Italic;
            }

            var drawFont = SKTypeface.FromFamilyName(CleanFontName(font.Name), style);
            var fontPaint = new SKPaint(drawFont.ToFont((float)(pointSize * _mult)))
            {
                Color = color.ToSystemColor()
            };

            //System.Diagnostics.Debug.WriteLine($"DrawLetter: '{font.Name}'\t{fontSize} -> Font name used: '{drawFont.FamilyName}'");

            _canvas.DrawText(unicode, startBaseLine, fontPaint);
            _canvas.ResetMatrix();

            fontPaint.Dispose();
            drawFont.Dispose();
            style.Dispose();
        }

        public override void BeginMarkedContent(NameToken name, NameToken propertyDictionaryName, DictionaryToken properties)
        {
            // Do nothing - to check
        }

        public override void EndMarkedContent()
        {
            // Do nothing - to check
        }

        private SKPath? CurrentPath { get; set; }

        public override void BeginSubpath()
        {
            if (CurrentPath == null)
            {
                CurrentPath = new SKPath();
            }
        }

        public override PdfPoint? CloseSubpath()
        {
            // do nothing - to check
            return null;
        }

        public override void MoveTo(double x, double y)
        {
            BeginSubpath();
            var point = CurrentTransformationMatrix.Transform(new PdfPoint(x, y));
            float xs = (float)(point.X * _mult);
            float ys = (float)(_height - point.Y * _mult);

            CurrentPath.MoveTo(xs, ys);
        }

        public override void LineTo(double x, double y)
        {
            var point = CurrentTransformationMatrix.Transform(new PdfPoint(x, y));
            float xs = (float)(point.X * _mult);
            float ys = (float)(_height - point.Y * _mult);

            CurrentPath.LineTo(xs, ys);
        }

        public override void BezierCurveTo(double x2, double y2, double x3, double y3)
        {
            var controlPoint2 = CurrentTransformationMatrix.Transform(new PdfPoint(x2, y2));
            var end = CurrentTransformationMatrix.Transform(new PdfPoint(x3, y3));
            float x2s = (float)(controlPoint2.X * _mult);
            float y2s = (float)(_height - controlPoint2.Y * _mult);
            float x3s = (float)(end.X * _mult);
            float y3s = (float)(_height - end.Y * _mult);

            CurrentPath.QuadTo(x2s, y2s, x3s, y3s);
        }

        public override void BezierCurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            var controlPoint1 = CurrentTransformationMatrix.Transform(new PdfPoint(x1, y1));
            var controlPoint2 = CurrentTransformationMatrix.Transform(new PdfPoint(x2, y2));
            var end = CurrentTransformationMatrix.Transform(new PdfPoint(x3, y3));
            float x1s = (float)(controlPoint1.X * _mult);
            float y1s = (float)(_height - controlPoint1.Y * _mult);
            float x2s = (float)(controlPoint2.X * _mult);
            float y2s = (float)(_height - controlPoint2.Y * _mult);
            float x3s = (float)(end.X * _mult);
            float y3s = (float)(_height - end.Y * _mult);

            CurrentPath.CubicTo(x1s, y1s, x2s, y2s, x3s, y3s);
        }

        public override void ClosePath()
        {
            CurrentPath.Close();
        }

        public override void EndPath()
        {
            if (CurrentPath == null)
            {
                return;
            }

            // TODO
            CurrentPath.Dispose();
            CurrentPath = null;
        }

        public override void Rectangle(double x, double y, double width, double height)
        {
            BeginSubpath();
            var lowerLeft = CurrentTransformationMatrix.Transform(new PdfPoint(x, y));
            var upperRight = CurrentTransformationMatrix.Transform(new PdfPoint(x + width, y + height));
            float left = (float)(lowerLeft.X * _mult);
            float top = (float)(_height - upperRight.Y * _mult);
            float right = (float)(upperRight.X * _mult);
            float bottom = (float)(_height - lowerLeft.Y * _mult);
            SKRect rect = new SKRect(left, top, right, bottom);
            CurrentPath.AddRect(rect);
        }

        private float GetScaledLineWidth()
        {
            var currentState = GetCurrentState();
            // https://stackoverflow.com/questions/25690496/how-does-pdf-line-width-interact-with-the-ctm-in-both-horizontal-and-vertical-di
             // TODO - a hack but works, to put in ContentStreamProcessor
            return (float)(float)(currentState.LineWidth * (decimal)currentState.CurrentTransformationMatrix.A);
        }

        public override void StrokePath(bool close)
        {
            if (close)
            {
                CurrentPath.Close();
            }

            var currentState = GetCurrentState();

            PaintStrokePath(currentState);

            CurrentPath.Dispose();
            CurrentPath = null;
        }

        private void PaintStrokePath(CurrentGraphicsState currentGraphicsState)
        {
            using (SKPaint paint = new SKPaint())
            {
                float lineWidth = Math.Max((float)0.5, GetScaledLineWidth()) * (float)_mult; // A guess

                paint.Color = currentGraphicsState.CurrentStrokingColor.ToSystemColor();
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = lineWidth;

                switch (currentGraphicsState.JoinStyle) // to put in helper
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

                switch (currentGraphicsState.CapStyle) // to put in helper
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

                if (currentGraphicsState.LineDashPattern.Phase != 0 || currentGraphicsState.LineDashPattern.Array?.Count > 0) // to put in helper
                {
                    //* https://docs.microsoft.com/en-us/dotnet/api/system.drawing.pen.dashpattern?view=dotnet-plat-ext-3.1
                    //* The elements in the dashArray array set the length of each dash and space in the dash pattern. 
                    //* The first element sets the length of a dash, the second element sets the length of a space, the
                    //* third element sets the length of a dash, and so on. Consequently, each element should be a 
                    //* non-zero positive number.

                    if (currentGraphicsState.LineDashPattern.Array.Count == 1)
                    {
                        List<float> pattern = new List<float>();
                        var v = currentGraphicsState.LineDashPattern.Array[0];
                        pattern.Add((float)((double)v / _mult));
                        pattern.Add((float)((double)v / _mult));
                        paint.PathEffect = SKPathEffect.CreateDash(pattern.ToArray(), (float)v); // TODO
                    }
                    else if (currentGraphicsState.LineDashPattern.Array.Count > 0)
                    {
                        List<float> pattern = new List<float>();
                        for (int i = 0; i < currentGraphicsState.LineDashPattern.Array.Count; i++)
                        {
                            var v = currentGraphicsState.LineDashPattern.Array[i];
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

                _canvas.DrawPath(CurrentPath, paint);
            }
        }

        public override void FillPath(FillingRule fillingRule, bool close)
        {
            if (close)
            {
                CurrentPath.Close();
            }

            var currentState = GetCurrentState();

            PaintFillPath(currentState, fillingRule);

            CurrentPath.Dispose();
            CurrentPath = null;
        }

        private void PaintFillPath(CurrentGraphicsState currentGraphicsState, FillingRule fillingRule)
        {
            CurrentPath.FillType = GetSKPathFillType(fillingRule);

            using (SKPaint paint = new SKPaint())
            {
                paint.Color = currentGraphicsState.CurrentNonStrokingColor.ToSystemColor();
                paint.Style = SKPaintStyle.Fill;
                _canvas.DrawPath(CurrentPath, paint);
            }
        }

        public override void FillStrokePath(FillingRule fillingRule, bool close)
        {
            if (close)
            {
                CurrentPath.Close();
            }

            var currentState = GetCurrentState();

            PaintFillPath(currentState, fillingRule);
            PaintStrokePath(currentState);

            CurrentPath.Dispose();
            CurrentPath = null;
        }

        public override void PopState()
        {
            base.PopState();
            _canvas.Restore();
        }

        public override void PushState()
        {
            base.PushState();
            _canvas.Save();
        }

        public override void ModifyClippingIntersect(FillingRule clippingRule)
        {
            CurrentPath.FillType = GetSKPathFillType(clippingRule);
            _canvas.ClipPath(CurrentPath, SKClipOperation.Intersect);
        }

        private static SKPathFillType GetSKPathFillType(FillingRule fillingRule)
        {
            return fillingRule == FillingRule.NonZeroWinding ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
        }

        public override void ShowXObjectImage(XObjectContentRecord xObjectContentRecord)
        {
            var image = GetImageFromXObject(xObjectContentRecord);
            DrawImage(image);
        }

        public override void ShowInlineImage(InlineImage inlineImage)
        {
            DrawImage(inlineImage);
        }

        private void DrawImage(IPdfImage image)
        {
            var upperLeft = image.Bounds.TopLeft.ToPointF(_height, _mult);
            var destRect = new SKRect(upperLeft.X, upperLeft.Y,
                             upperLeft.X + (float)(image.Bounds.Width * _mult),
                             upperLeft.Y + (float)(image.Bounds.Height * _mult));

            byte[]? bytes = null;

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
    }
}
