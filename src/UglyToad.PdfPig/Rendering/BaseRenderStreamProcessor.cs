namespace UglyToad.PdfPig.Rendering
{
    using System;
    using System.IO;
    using System.Linq;
    using UglyToad.PdfPig.Annotations;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Filters;
    using UglyToad.PdfPig.Fonts.Standard14Fonts;
    using UglyToad.PdfPig.Geometry;
    using UglyToad.PdfPig.Graphics;
    using UglyToad.PdfPig.Graphics.Colors;
    using UglyToad.PdfPig.Graphics.Operations;
    using UglyToad.PdfPig.Graphics.Operations.PathConstruction;
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
        /// Default FieldsHighlightColor from Adobe Acrobat Reader.
        /// TODO - make an option of that
        /// </summary>
        public static readonly IColor DefaultFieldsHighlightColor = new RGBColor((decimal)(204 / 255.0), (decimal)(215 / 255.0), 1);

        /// <summary>
        /// Default Required FieldsHighlightColor from Adobe Acrobat Reader.
        /// TODO - make an option of that
        /// </summary>
        public static readonly IColor DefaultRequiredFieldsHighlightColor = new RGBColor(1, 0, 0);

        private readonly DictionaryToken dictionary;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="page"></param>
        public BaseRenderStreamProcessor(Page page) : this(page.CropBox.Bounds, page.Content.resourceStore, page.Content.userSpaceUnit,
            page.Rotation, page.pdfScanner, page.Content.pageContentParser, page.Content.filterProvider,
            new PdfVector(page.MediaBox.Bounds.Width, page.MediaBox.Bounds.Height), page.Content.internalParsingOptions)
        {
            dictionary = page.Dictionary;
            pageNumber = page.Number;
        }

        internal BaseRenderStreamProcessor(PdfRectangle cropBox, IResourceStore resourceStore, UserSpaceUnit userSpaceUnit, PageRotationDegrees rotation,
                                  IPdfTokenScanner pdfScanner, IPageContentParser pageContentParser, ILookupFilterProvider filterProvider, PdfVector pageSize,
                                  InternalParsingOptions parsingOptions)
          : base(cropBox, resourceStore, userSpaceUnit, rotation, pdfScanner, pageContentParser, filterProvider, pageSize, parsingOptions)
        { }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public abstract MemoryStream GetImage(double scale);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns></returns>
        protected static DictionaryToken GetAppearance(Annotation annotation)
        {
            if (annotation.AnnotationDictionary.TryGet<DictionaryToken>(NameToken.Ap, out var appearance))
            {
                return appearance;
            }
            return null;
        }

        /// <summary>
        /// todo (cannot be loaded by SkiaSharp)
        /// </summary>
        /// <param name="afmName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Obsolete("SkiaSharp can't load AFM files")]
        protected byte[] GetAfmStream(string afmName)
        {
            // UglyToad.PdfPig.Fonts.Standard14Fonts .Standard14
            var assembly = typeof(Standard14).Assembly;

            var name = $"UglyToad.PdfPig.Fonts.Resources.AdobeFontMetrics.{afmName}.afm";

            //IInputBytes bytes;
            var memory = new MemoryStream();
            using (var resource = assembly.GetManifestResourceStream(name))
            {
                if (resource == null)
                {
                    throw new InvalidOperationException($"Could not find AFM resource with name: {name}.");
                }

                resource.CopyTo(memory);
                return memory.ToArray();
                //bytes = new ByteArrayInputBytes(memory.ToArray());
            }
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns></returns>
        protected StreamToken GetNormalAppearanceAsStream(Annotation annotation)
        {
            // https://github.com/apache/pdfbox/blob/trunk/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDUnderlineAppearanceHandler.java
            // https://github.com/apache/pdfbox/blob/trunk/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDHighlightAppearanceHandler.java
            var dict = GetAppearance(annotation);

            if (dict == null)
            {
                return GenerateNormalAppearanceAsStream(annotation);
            }

            // get Normal Appearance
            if (!dict.Data.TryGetValue(NameToken.N, out var data))
            {
                return null;
            }

            if (data is IndirectReferenceToken irt)
            {
                data = Get(irt);
                if (data is null)
                {
                    return null;
                }
            }

            if (data is StreamToken streamToken)
            {
                return streamToken;
            }
            else if (data is DictionaryToken dictionaryToken)
            {
                if (annotation.AnnotationDictionary.TryGet(NameToken.As, out var appearanceState))
                {
                    return dictionaryToken.Get<StreamToken>(appearanceState as NameToken, pdfScanner);
                }

                return null;
            }
            else if (data is ObjectToken objectToken)
            {
                if (objectToken.Data is StreamToken streamToken2)
                {
                    return streamToken2;
                }
                else if (objectToken.Data is DictionaryToken dictionaryToken2)
                {
                    // getAppearanceState
                    if (annotation.AnnotationDictionary.TryGet(NameToken.As, out var appearanceState))
                    {
                        return dictionaryToken2.Get<StreamToken>(appearanceState as NameToken, pdfScanner);
                    }
                    return null;
                }
            }

            throw new ArgumentException();
        }

        private StreamToken GenerateNormalAppearanceAsStream(Annotation annotation)
        {
            // https://github.com/apache/pdfbox/blob/c4b212ecf42a1c0a55529873b132ea338a8ba901/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDAbstractAppearanceHandler.java#L479

            switch (annotation.Type)
            {
                case AnnotationType.StrikeOut:
                    return GenerateStrikeOutNormalAppearanceAsStream(annotation);

                case AnnotationType.Highlight:
                    return GenerateHighlightNormalAppearanceAsStream(annotation);

                case AnnotationType.Underline:
                    return GenerateUnderlineNormalAppearanceAsStream(annotation);

                case AnnotationType.Link:
                    return GenerateLinkNormalAppearanceAsStream(annotation);
            }

            return null;
        }

        private StreamToken GenerateHighlightNormalAppearanceAsStream(Annotation annotation)
        {
            // TODO - draws on top of text, should be below
            // https://github.com/apache/pdfbox/blob/trunk/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDHighlightAppearanceHandler.java
            PdfRectangle rect = annotation.Rectangle;

            if (!annotation.AnnotationDictionary.TryGet<ArrayToken>(NameToken.Quadpoints, out var quadpoints))
            {
                return null;
            }

            var pathsArray = quadpoints.Data.OfType<NumericToken>().Select(x => (float)x.Double).ToArray();

            var ab = annotation.Border;

            if (!annotation.AnnotationDictionary.TryGet<ArrayToken>(NameToken.C, out var colorToken) || colorToken.Data.Count == 0)
            {
                return null;
            }
            var color = colorToken.Data.OfType<NumericToken>().Select(x => (decimal)x.Double).ToArray();

            decimal width = ab.BorderWidth;

            // Adjust rectangle even if not empty, see PLPDF.com-MarkupAnnotations.pdf
            //TODO in a class structure this should be overridable
            // this is similar to polyline but different data type
            //TODO padding should consider the curves too; needs to know in advance where the curve is
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            for (int i = 0; i < pathsArray.Length / 2; ++i)
            {
                float x = pathsArray[i * 2];
                float y = pathsArray[i * 2 + 1];
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }

            // get the delta used for curves and use it for padding
            float maxDelta = 0;
            for (int i = 0; i < pathsArray.Length / 8; ++i)
            {
                // one of the two is 0, depending whether the rectangle is 
                // horizontal or vertical
                // if it is diagonal then... uh...
                float delta = Math.Max((pathsArray[i + 0] - pathsArray[i + 4]) / 4,
                                       (pathsArray[i + 1] - pathsArray[i + 5]) / 4);
                maxDelta = Math.Max(delta, maxDelta);
            }

            var setLowerLeftX = Math.Min(minX - (float)width / 2.0, rect.BottomLeft.X); //.getLowerLeftX()));
            var setLowerLeftY = Math.Min(minY - (float)width / 2.0, rect.BottomLeft.Y); // .getLowerLeftY()));
            var setUpperRightX = Math.Max(maxX + (float)width / 2.0, rect.TopRight.X); //.getUpperRightX()));
            var setUpperRightY = Math.Max(maxY + (float)width / 2.0, rect.TopRight.Y); //rect.getUpperRightY()));
            PdfRectangle pdfRectangle = new PdfRectangle(setLowerLeftX, setLowerLeftY, setUpperRightX, setUpperRightY); //annotation.setRectangle(rect);

            try
            {
                using (var ms = new MemoryStream())
                {
                    /*
                    PDExtendedGraphicsState r0 = new PDExtendedGraphicsState();
                    PDExtendedGraphicsState r1 = new PDExtendedGraphicsState();
                    r0.setAlphaSourceFlag(false);
                    r0.setStrokingAlphaConstant(annotation.getConstantOpacity());
                    r0.setNonStrokingAlphaConstant(annotation.getConstantOpacity());
                    r1.setAlphaSourceFlag(false);
                    r1.setBlendMode(BlendMode.MULTIPLY);
                    cs.setGraphicsStateParameters(r0);
                    cs.setGraphicsStateParameters(r1);
                    PDFormXObject frm1 = new PDFormXObject(createCOSStream());
                    PDFormXObject frm2 = new PDFormXObject(createCOSStream());
                    frm1.setResources(new PDResources());
                    try (PDFormContentStream mwfofrmCS = new PDFormContentStream(frm1))
                    {
                        mwfofrmCS.drawForm(frm2);
                    }
                    frm1.setBBox(annotation.getRectangle());
                    COSDictionary groupDict = new COSDictionary();
                    groupDict.setItem(COSName.S, COSName.TRANSPARENCY);
                    //TODO PDFormXObject.setGroup() is missing
                    frm1.getCOSObject().setItem(COSName.GROUP, groupDict);
                    cs.drawForm(frm1);
                    frm2.setBBox(annotation.getRectangle());
                    */

                    new SetNonStrokeColor(color).Write(ms);

                    int of = 0;
                    while (of + 7 < pathsArray.Length)
                    {
                        // quadpoints spec sequence is incorrect, correct one is (4,5 0,1 2,3 6,7)
                        // https://stackoverflow.com/questions/9855814/pdf-spec-vs-acrobat-creation-quadpoints

                        // for "curvy" highlighting, two Bézier control points are used that seem to have a
                        // distance of about 1/4 of the height.
                        // note that curves won't appear if outside of the rectangle
                        float delta = 0;
                        if (pathsArray[of + 0] == pathsArray[of + 4] &&
                            pathsArray[of + 1] == pathsArray[of + 3] &&
                            pathsArray[of + 2] == pathsArray[of + 6] &&
                            pathsArray[of + 5] == pathsArray[of + 7])
                        {
                            // horizontal highlight
                            delta = (pathsArray[of + 1] - pathsArray[of + 5]) / 4;
                        }
                        else if (pathsArray[of + 1] == pathsArray[of + 5] &&
                                 pathsArray[of + 0] == pathsArray[of + 2] &&
                                 pathsArray[of + 3] == pathsArray[of + 7] &&
                                 pathsArray[of + 4] == pathsArray[of + 6])
                        {
                            // vertical highlight
                            delta = (pathsArray[of + 0] - pathsArray[of + 4]) / 4;
                        }

                        new BeginNewSubpath((decimal)pathsArray[of + 4], (decimal)pathsArray[of + 5]).Write(ms);

                        if (pathsArray[of + 0] == pathsArray[of + 4])
                        {
                            // horizontal highlight
                            new AppendDualControlPointBezierCurve(
                                (decimal)(pathsArray[of + 4] - delta), (decimal)(pathsArray[of + 5] + delta),
                                (decimal)(pathsArray[of + 0] - delta), (decimal)(pathsArray[of + 1] - delta),
                                (decimal)pathsArray[of + 0], (decimal)pathsArray[of + 1])
                                .Write(ms);
                        }
                        else if (pathsArray[of + 5] == pathsArray[of + 1])
                        {
                            // vertical highlight
                            new AppendDualControlPointBezierCurve(
                               (decimal)(pathsArray[of + 4] + delta), (decimal)(pathsArray[of + 5] + delta),
                               (decimal)(pathsArray[of + 0] - delta), (decimal)(pathsArray[of + 1] + delta),
                               (decimal)pathsArray[of + 0], (decimal)pathsArray[of + 1])
                               .Write(ms);
                        }
                        else
                        {
                            new AppendStraightLineSegment((decimal)pathsArray[of + 0], (decimal)pathsArray[of + 1])
                                .Write(ms);
                        }
                        new AppendStraightLineSegment((decimal)pathsArray[of + 2], (decimal)pathsArray[of + 3]).
                             Write(ms);

                        if (pathsArray[of + 2] == pathsArray[of + 6])
                        {
                            // horizontal highlight
                            new AppendDualControlPointBezierCurve(
                                (decimal)(pathsArray[of + 2] + delta), (decimal)(pathsArray[of + 3] - delta),
                                (decimal)(pathsArray[of + 6] + delta), (decimal)(pathsArray[of + 7] + delta),
                                (decimal)pathsArray[of + 6], (decimal)pathsArray[of + 7])
                                .Write(ms);
                        }
                        else if (pathsArray[of + 3] == pathsArray[of + 7])
                        {
                            // vertical highlight
                            new AppendDualControlPointBezierCurve(
                                (decimal)(pathsArray[of + 2] - delta), (decimal)(pathsArray[of + 3] - delta),
                                (decimal)(pathsArray[of + 6] + delta), (decimal)(pathsArray[of + 7] - delta),
                                (decimal)pathsArray[of + 6], (decimal)pathsArray[of + 7])
                                .Write(ms);
                        }
                        else
                        {
                            new AppendStraightLineSegment((decimal)pathsArray[of + 6], (decimal)pathsArray[of + 7])
                                .Write(ms);
                        }

                        PdfPig.Graphics.Operations.PathPainting.FillPathEvenOddRule.Value.Write(ms);
                        of += 8;
                    }

                    // https://github.com/apache/pdfbox/blob/c4b212ecf42a1c0a55529873b132ea338a8ba901/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDAbstractAppearanceHandler.java#L511
                    var dict = dictionary.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    /*
                        private void setTransformationMatrix(PDAppearanceStream appearanceStream)
                        {
                            PDRectangle bbox = getRectangle();
                            appearanceStream.setBBox(bbox);
                            AffineTransform transform = AffineTransform.getTranslateInstance(-bbox.getLowerLeftX(),
                                    -bbox.getLowerLeftY());
                            appearanceStream.setMatrix(transform);
                        }
                     */
                    if (annotation.AnnotationDictionary.TryGet(NameToken.Rect, out var rectToken))
                    {
                        dict.Add(NameToken.Bbox.Data, rectToken); // should use new rect
                    }

                    return new StreamToken(new DictionaryToken(dict), ms.ToArray());
                }
            }
            catch (Exception)
            {
                Console.WriteLine("");
                // log
            }
            return null;
        }

        PdfRectangle getPaddedRectangle(PdfRectangle rectangle, float padding)
        {
            return new PdfRectangle(
                rectangle.BottomLeft.X + padding,
                rectangle.BottomLeft.Y + padding,
                rectangle.BottomLeft.X + (rectangle.Width - 2 * padding),
                rectangle.BottomLeft.Y + (rectangle.Height - 2 * padding));
        }

        private StreamToken GenerateLinkNormalAppearanceAsStream(Annotation annotation)
        {
            // https://github.com/apache/pdfbox/blob/trunk/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDLinkAppearanceHandler.java

            PdfRectangle rect = annotation.Rectangle;

            var ab = annotation.Border;
            try
            {
                using (var ms = new MemoryStream())
                {
                    if (!annotation.AnnotationDictionary.TryGet<ArrayToken>(NameToken.C, out var colorToken) || colorToken.Data.Count == 0)
                    {
                        return null;
                    }
                    var color = colorToken.Data.OfType<NumericToken>().Select(x => (decimal)x.Double).ToArray();

                    new SetStrokeColor(color).Write(ms);

                    decimal lineWidth = ab.BorderWidth;

                    new UglyToad.PdfPig.Graphics.Operations.General.SetLineWidth(lineWidth).Write(ms);

                    // Acrobat applies a padding to each side of the bbox so the line is completely within
                    // the bbox.
                    PdfRectangle borderEdge = getPaddedRectangle(rect, (float)(lineWidth / 2.0m));

                    float[] pathsArray = null;
                    if (annotation.AnnotationDictionary.TryGet<ArrayToken>(NameToken.Quadpoints, out var quadpoints))
                    {
                        pathsArray = quadpoints.Data?.OfType<NumericToken>().Select(x => (float)x.Double)?.ToArray();
                    }

                    if (pathsArray != null)
                    {
                        // QuadPoints shall be ignored if any coordinate in the array lies outside
                        // the region specified by Rect.
                        for (int i = 0; i < pathsArray.Length / 2; ++i)
                        {
                            if (!rect.Contains(new PdfPoint(pathsArray[i * 2], pathsArray[i * 2 + 1])))
                            {
                                //LOG.warn("At least one /QuadPoints entry (" +
                                //        pathsArray[i * 2] + ";" + pathsArray[i * 2 + 1] +
                                //        ") is outside of rectangle, " + rect +
                                //        ", /QuadPoints are ignored and /Rect is used instead");
                                pathsArray = null;
                                break;
                            }
                        }
                    }

                    if (pathsArray == null)
                    {
                        // Convert rectangle coordinates as if it was a /QuadPoints entry
                        pathsArray = new float[8];
                        pathsArray[0] = (float)borderEdge.BottomLeft.X; //.getLowerLeftX();
                        pathsArray[1] = (float)borderEdge.BottomLeft.Y; //.getLowerLeftY();
                        pathsArray[2] = (float)borderEdge.TopRight.X; //.getUpperRightX();
                        pathsArray[3] = (float)borderEdge.BottomLeft.Y; //.getLowerLeftY();
                        pathsArray[4] = (float)borderEdge.TopRight.X; //.getUpperRightX();
                        pathsArray[5] = (float)borderEdge.TopRight.Y; //.getUpperRightY();
                        pathsArray[6] = (float)borderEdge.BottomLeft.X; //.getLowerLeftX();
                        pathsArray[7] = (float)borderEdge.TopRight.Y; //.getUpperRightY();
                    }

                    bool underlined = false;
                    if (pathsArray.Length >= 8)
                    {
                        // Get border style
                        if (annotation.AnnotationDictionary.TryGet<DictionaryToken>(NameToken.Bs, out var borderStyleToken))
                        {
                            if (borderStyleToken.TryGet<NameToken>(NameToken.S, out var styleToken))
                            {
                                underlined = styleToken.Data.Equals("U");
                                // Optional) The border style:
                                // S   (Solid) A solid rectangle surrounding the annotation.
                                // D   (Dashed) A dashed rectangle surrounding the annotation. The dash pattern may be specified by the D entry.
                                // B   (Beveled) A simulated embossed rectangle that appears to be raised above the surface of the page.
                                // I   (Inset) A simulated engraved rectangle that appears to be recessed below the surface of the page.
                                // U   (Underline) A single line along the bottom of the annotation rectangle.
                                // A conforming reader shall tolerate other border styles that it does not recognize and shall use the default value.
                            }
                        }
                    }

                    int of = 0;
                    while (of + 7 < pathsArray.Length)
                    {
                        new BeginNewSubpath((decimal)pathsArray[of], (decimal)pathsArray[of + 1]).Write(ms);
                        new AppendStraightLineSegment((decimal)pathsArray[of + 2], (decimal)pathsArray[of + 3]).Write(ms);
                        if (!underlined)
                        {
                            new AppendStraightLineSegment((decimal)pathsArray[of + 4], (decimal)pathsArray[of + 5]).Write(ms);
                            new AppendStraightLineSegment((decimal)pathsArray[of + 6], (decimal)pathsArray[of + 7]).Write(ms);
                            UglyToad.PdfPig.Graphics.Operations.PathConstruction.CloseSubpath.Value.Write(ms);
                        }
                        of += 8;
                    }

                    // TO CHECK
                    PdfPig.Graphics.Operations.PathPainting.StrokePath.Value.Write(ms);
                    //contentStream.drawShape(lineWidth, hasStroke, false);

                    // https://github.com/apache/pdfbox/blob/c4b212ecf42a1c0a55529873b132ea338a8ba901/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDAbstractAppearanceHandler.java#L511
                    var dict = dictionary.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    /*
                        private void setTransformationMatrix(PDAppearanceStream appearanceStream)
                        {
                            PDRectangle bbox = getRectangle();
                            appearanceStream.setBBox(bbox);
                            AffineTransform transform = AffineTransform.getTranslateInstance(-bbox.getLowerLeftX(),
                                    -bbox.getLowerLeftY());
                            appearanceStream.setMatrix(transform);
                        }
                     */
                    if (annotation.AnnotationDictionary.TryGet(NameToken.Rect, out var rectToken))
                    {
                        dict.Add(NameToken.Bbox.Data, rectToken); // should use new rect
                    }

                    return new StreamToken(new DictionaryToken(dict), ms.ToArray());
                }
            }
            catch (Exception)
            {
                Console.WriteLine("");
                // log
            }

            return null;
        }

        private StreamToken GenerateStrikeOutNormalAppearanceAsStream(Annotation annotation)
        {
            // https://github.com/apache/pdfbox/blob/trunk/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDStrikeoutAppearanceHandler.java

            PdfRectangle rect = annotation.Rectangle;

            if (!annotation.AnnotationDictionary.TryGet<ArrayToken>(NameToken.Quadpoints, out var quadpoints))
            {
                return null;
            }

            var pathsArray = quadpoints.Data.OfType<NumericToken>().Select(x => (float)x.Double).ToArray();

            var ab = annotation.Border;

            if (!annotation.AnnotationDictionary.TryGet<ArrayToken>(NameToken.C, out var colorToken) || colorToken.Data.Count == 0)
            {
                return null;
            }
            var color = colorToken.Data.OfType<NumericToken>().Select(x => (decimal)x.Double).ToArray();

            decimal width = ab.BorderWidth;
            if (width == 0)
            {
                width = 1.5m;
            }

            // Adjust rectangle even if not empty, see PLPDF.com-MarkupAnnotations.pdf
            //TODO in a class structure this should be overridable
            // this is similar to polyline but different data type
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            for (int i = 0; i < pathsArray.Length / 2; ++i)
            {
                float x = pathsArray[i * 2];
                float y = pathsArray[i * 2 + 1];
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
            var setLowerLeftX = Math.Min(minX - (float)width / 2.0, rect.BottomLeft.X); //.getLowerLeftX()));
            var setLowerLeftY = Math.Min(minY - (float)width / 2.0, rect.BottomLeft.Y); // .getLowerLeftY()));
            var setUpperRightX = Math.Max(maxX + (float)width / 2.0, rect.TopRight.X); //.getUpperRightX()));
            var setUpperRightY = Math.Max(maxY + (float)width / 2.0, rect.TopRight.Y); //rect.getUpperRightY()));
            PdfRectangle pdfRectangle = new PdfRectangle(setLowerLeftX, setLowerLeftY, setUpperRightX, setUpperRightY); //annotation.setRectangle(rect);

            try
            {
                using (var ms = new MemoryStream())
                {
                    //setOpacity(cs, annotation.getConstantOpacity()); // TODO

                    new SetStrokeColor(color).Write(ms);

                    //if (ab.dashArray != null)
                    //{
                    //    cs.setLineDashPattern(ab.dashArray, 0);
                    //}                   

                    new UglyToad.PdfPig.Graphics.Operations.General.SetLineWidth(width).Write(ms);

                    // spec is incorrect
                    // https://stackoverflow.com/questions/9855814/pdf-spec-vs-acrobat-creation-quadpoints
                    for (int i = 0; i < pathsArray.Length / 8; ++i)
                    {
                        // get mid point between bounds, subtract the line width to approximate what Adobe is doing
                        // See e.g. CTAN-example-Annotations.pdf and PLPDF.com-MarkupAnnotations.pdf
                        // and https://bugs.ghostscript.com/show_bug.cgi?id=693664
                        // do the math for diagonal annotations with this weird old trick:
                        // https://stackoverflow.com/questions/7740507/extend-a-line-segment-a-specific-distance
                        float len0 = (float)(Math.Sqrt(Math.Pow(pathsArray[i * 8] - pathsArray[i * 8 + 4], 2) +
                                             Math.Pow(pathsArray[i * 8 + 1] - pathsArray[i * 8 + 5], 2)));
                        float x0 = pathsArray[i * 8 + 4];
                        float y0 = pathsArray[i * 8 + 5];
                        if (len0 != 0)
                        {
                            // only if both coordinates are not identical to avoid divide by zero
                            x0 += (pathsArray[i * 8] - pathsArray[i * 8 + 4]) / len0 * (len0 / 2 - (float)ab.BorderWidth);
                            y0 += (pathsArray[i * 8 + 1] - pathsArray[i * 8 + 5]) / len0 * (len0 / 2 - (float)ab.BorderWidth);
                        }
                        float len1 = (float)(Math.Sqrt(Math.Pow(pathsArray[i * 8 + 2] - pathsArray[i * 8 + 6], 2) +
                                             Math.Pow(pathsArray[i * 8 + 3] - pathsArray[i * 8 + 7], 2)));
                        float x1 = pathsArray[i * 8 + 6];
                        float y1 = pathsArray[i * 8 + 7];
                        if (len1 != 0)
                        {
                            // only if both coordinates are not identical to avoid divide by zero
                            x1 += (pathsArray[i * 8 + 2] - pathsArray[i * 8 + 6]) / len1 * (len1 / 2 - (float)ab.BorderWidth);
                            y1 += (pathsArray[i * 8 + 3] - pathsArray[i * 8 + 7]) / len1 * (len1 / 2 - (float)ab.BorderWidth);
                        }
                        new BeginNewSubpath((decimal)x0, (decimal)y0).Write(ms);
                        new AppendStraightLineSegment((decimal)x1, (decimal)y1).Write(ms);
                    }
                    PdfPig.Graphics.Operations.PathPainting.StrokePath.Value.Write(ms);

                    // https://github.com/apache/pdfbox/blob/c4b212ecf42a1c0a55529873b132ea338a8ba901/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDAbstractAppearanceHandler.java#L511
                    var dict = dictionary.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    /*
                        private void setTransformationMatrix(PDAppearanceStream appearanceStream)
                        {
                            PDRectangle bbox = getRectangle();
                            appearanceStream.setBBox(bbox);
                            AffineTransform transform = AffineTransform.getTranslateInstance(-bbox.getLowerLeftX(),
                                    -bbox.getLowerLeftY());
                            appearanceStream.setMatrix(transform);
                        }
                     */
                    if (annotation.AnnotationDictionary.TryGet(NameToken.Rect, out var rectToken))
                    {
                        dict.Add(NameToken.Bbox.Data, rectToken); // should use new rect
                    }

                    return new StreamToken(new DictionaryToken(dict), ms.ToArray());
                }
            }
            catch (Exception)
            {
                Console.WriteLine("");
                // log
            }

            return null;
        }

        private StreamToken GenerateUnderlineNormalAppearanceAsStream(Annotation annotation)
        {
            // https://github.com/apache/pdfbox/blob/trunk/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDStrikeoutAppearanceHandler.java

            PdfRectangle rect = annotation.Rectangle;

            if (!annotation.AnnotationDictionary.TryGet<ArrayToken>(NameToken.Quadpoints, out var quadpoints))
            {
                return null;
            }

            var pathsArray = quadpoints.Data.OfType<NumericToken>().Select(x => (float)x.Double).ToArray();

            var ab = annotation.Border;

            if (!annotation.AnnotationDictionary.TryGet<ArrayToken>(NameToken.C, out var colorToken) || colorToken.Data.Count == 0)
            {
                return null;
            }
            var color = colorToken.Data.OfType<NumericToken>().Select(x => (decimal)x.Double).ToArray();

            decimal width = ab.BorderWidth;
            if (width == 0)
            {
                // value found in adobe reader
                width = 1.5m;
            }

            // Adjust rectangle even if not empty, see PLPDF.com-MarkupAnnotations.pdf
            //TODO in a class structure this should be overridable
            // this is similar to polyline but different data type
            // all coordinates (unlike painting) are used because I'm lazy
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            for (int i = 0; i < pathsArray.Length / 2; ++i)
            {
                float x = pathsArray[i * 2];
                float y = pathsArray[i * 2 + 1];
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
            var setLowerLeftX = Math.Min(minX - (float)width / 2.0, rect.BottomLeft.X); //.getLowerLeftX()));
            var setLowerLeftY = Math.Min(minY - (float)width / 2.0, rect.BottomLeft.Y); // .getLowerLeftY()));
            var setUpperRightX = Math.Max(maxX + (float)width / 2.0, rect.TopRight.X); //.getUpperRightX()));
            var setUpperRightY = Math.Max(maxY + (float)width / 2.0, rect.TopRight.Y); //rect.getUpperRightY()));
            PdfRectangle pdfRectangle = new PdfRectangle(setLowerLeftX, setLowerLeftY, setUpperRightX, setUpperRightY); //annotation.setRectangle(rect);

            try
            {
                using (var ms = new MemoryStream())
                {
                    //setOpacity(cs, annotation.getConstantOpacity()); // TODO

                    new SetStrokeColor(color).Write(ms);

                    //if (ab.dashArray != null)
                    //{
                    //    cs.setLineDashPattern(ab.dashArray, 0);
                    //}                   

                    new UglyToad.PdfPig.Graphics.Operations.General.SetLineWidth(width).Write(ms);

                    // spec is incorrect
                    // https://stackoverflow.com/questions/9855814/pdf-spec-vs-acrobat-creation-quadpoints
                    for (int i = 0; i < pathsArray.Length / 8; ++i)
                    {
                        // Adobe doesn't use the lower coordinate for the line, it uses lower + delta / 7.
                        // do the math for diagonal annotations with this weird old trick:
                        // https://stackoverflow.com/questions/7740507/extend-a-line-segment-a-specific-distance
                        float len0 = (float)(Math.Sqrt(Math.Pow(pathsArray[i * 8] - pathsArray[i * 8 + 4], 2) +
                                              Math.Pow(pathsArray[i * 8 + 1] - pathsArray[i * 8 + 5], 2)));
                        float x0 = pathsArray[i * 8 + 4];
                        float y0 = pathsArray[i * 8 + 5];
                        if (len0 != 0)
                        {
                            // only if both coordinates are not identical to avoid divide by zero
                            x0 += (pathsArray[i * 8] - pathsArray[i * 8 + 4]) / len0 * len0 / 7;
                            y0 += (pathsArray[i * 8 + 1] - pathsArray[i * 8 + 5]) / len0 * (len0 / 7);
                        }
                        float len1 = (float)(Math.Sqrt(Math.Pow(pathsArray[i * 8 + 2] - pathsArray[i * 8 + 6], 2) +
                                              Math.Pow(pathsArray[i * 8 + 3] - pathsArray[i * 8 + 7], 2)));
                        float x1 = pathsArray[i * 8 + 6];
                        float y1 = pathsArray[i * 8 + 7];
                        if (len1 != 0)
                        {
                            // only if both coordinates are not identical to avoid divide by zero
                            x1 += (pathsArray[i * 8 + 2] - pathsArray[i * 8 + 6]) / len1 * len1 / 7;
                            y1 += (pathsArray[i * 8 + 3] - pathsArray[i * 8 + 7]) / len1 * len1 / 7;
                        }

                        new BeginNewSubpath((decimal)x0, (decimal)y0).Write(ms);
                        new AppendStraightLineSegment((decimal)x1, (decimal)y1).Write(ms);
                    }
                    PdfPig.Graphics.Operations.PathPainting.StrokePath.Value.Write(ms);

                    // https://github.com/apache/pdfbox/blob/c4b212ecf42a1c0a55529873b132ea338a8ba901/pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/handlers/PDAbstractAppearanceHandler.java#L511
                    var dict = dictionary.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    /*
                        private void setTransformationMatrix(PDAppearanceStream appearanceStream)
                        {
                            PDRectangle bbox = getRectangle();
                            appearanceStream.setBBox(bbox);
                            AffineTransform transform = AffineTransform.getTranslateInstance(-bbox.getLowerLeftX(),
                                    -bbox.getLowerLeftY());
                            appearanceStream.setMatrix(transform);
                        }
                     */
                    if (annotation.AnnotationDictionary.TryGet(NameToken.Rect, out var rectToken))
                    {
                        dict.Add(NameToken.Bbox.Data, rectToken); // should use new rect
                    }

                    return new StreamToken(new DictionaryToken(dict), ms.ToArray());
                }
            }
            catch (Exception)
            {
                Console.WriteLine("");
                // log
            }

            return null;
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns></returns>
        protected IToken GetNormalAppearance(Annotation annotation)
        {
            var dict = GetAppearance(annotation);

            if (dict == null)
            {
                return null;
            }

            // get Normal Appearance
            if (!dict.Data.TryGetValue(NameToken.N, out var data))
            {
                return null;
            }

            if (data is IndirectReferenceToken irt)
            {
                data = Get(irt);
                if (data is null)
                {
                    return null;
                }
            }

            if (data is StreamToken streamToken)
            {
                return streamToken;
            }
            else if (data is DictionaryToken dictionaryToken)
            {
                return dictionaryToken;
            }
            else if (data is ObjectToken objectToken)
            {
                if (objectToken.Data is StreamToken streamToken2)
                {
                    return streamToken2;
                }
                else if (objectToken.Data is DictionaryToken dictionaryToken2)
                {
                    return dictionaryToken2;
                }
            }

            throw new ArgumentException();
        }

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
        protected IToken Get(IndirectReferenceToken nameToken)
        {
            return base.pdfScanner.Get(nameToken.Data);
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
