namespace UglyToad.PdfPig.Graphics
{
    using Colors;
    using Content;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tokens;

    internal class ColorSpaceContext : IColorSpaceContext
    {
        public IColorSpaceContext DeepClone()
        {
            return new ColorSpaceContext(currentStateFunc, resourceStore)
            {
                CurrentStrokingColorSpaceDetails = CurrentStrokingColorSpaceDetails,
                CurrentNonStrokingColorSpaceDetails = CurrentNonStrokingColorSpaceDetails
            };
        }

        private readonly Func<CurrentGraphicsState> currentStateFunc;
        private readonly IResourceStore resourceStore;

        /// <summary>
        /// The <see cref="ColorSpaceDetails"/> used for stroking operations.
        /// </summary>
        public ColorSpaceDetails CurrentStrokingColorSpaceDetails { get; private set; } = DeviceGrayColorSpaceDetails.Instance;

        /// <summary>
        /// The <see cref="ColorSpaceDetails"/> used for non-stroking operations.
        /// </summary>
        public ColorSpaceDetails CurrentNonStrokingColorSpaceDetails { get; set; } = DeviceGrayColorSpaceDetails.Instance;

        public ColorSpaceContext(Func<CurrentGraphicsState> currentStateFunc, IResourceStore resourceStore)
        {
            this.currentStateFunc = currentStateFunc ?? throw new ArgumentNullException(nameof(currentStateFunc));
            this.resourceStore = resourceStore ?? throw new ArgumentNullException(nameof(resourceStore));
        }

        public void SetStrokingColorspace(NameToken colorspace)
        {
            CurrentStrokingColorSpaceDetails = resourceStore.GetColorSpaceDetails(colorspace, null);
            currentStateFunc().CurrentStrokingColor = CurrentStrokingColorSpaceDetails.GetInitializeColor();
        }

        public void SetNonStrokingColorspace(NameToken colorspace)
        {
            CurrentNonStrokingColorSpaceDetails = resourceStore.GetColorSpaceDetails(colorspace, null);
            currentStateFunc().CurrentNonStrokingColor = CurrentNonStrokingColorSpaceDetails.GetInitializeColor();
        }

        public void SetStrokingColor(IReadOnlyList<decimal> operands, NameToken patternName)
        {
            if (patternName != null && CurrentStrokingColorSpaceDetails is PatternColorSpaceDetails patternColorSpaceDetails)
            {
                currentStateFunc().CurrentStrokingColor = patternColorSpaceDetails.GetPattern(patternName);
            }
            else
            {
                currentStateFunc().CurrentStrokingColor = CurrentStrokingColorSpaceDetails.GetColor(operands.Select(v => (double)v).ToArray());
            }
        }

        public void SetStrokingColorGray(decimal gray)
        {
            CurrentStrokingColorSpaceDetails = DeviceGrayColorSpaceDetails.Instance;

            if (gray == 0)
            {
                currentStateFunc().CurrentStrokingColor = GrayColor.Black;
            }
            else if (gray == 1)
            {
                currentStateFunc().CurrentStrokingColor = GrayColor.White;
            }
            else
            {
                currentStateFunc().CurrentStrokingColor = new GrayColor(gray);
            }
        }

        public void SetStrokingColorRgb(decimal r, decimal g, decimal b)
        {
            CurrentStrokingColorSpaceDetails = DeviceRgbColorSpaceDetails.Instance;

            if (r == 0 && g == 0 && b == 0)
            {
                currentStateFunc().CurrentStrokingColor = RGBColor.Black;
            }
            else if (r == 1 && g == 1 && b == 1)
            {
                currentStateFunc().CurrentStrokingColor = RGBColor.White;
            }
            else
            {
                currentStateFunc().CurrentStrokingColor = new RGBColor(r, g, b);
            }
        }

        public void SetStrokingColorCmyk(decimal c, decimal m, decimal y, decimal k)
        {
            CurrentStrokingColorSpaceDetails = DeviceCmykColorSpaceDetails.Instance;

            if (c == 0 && m == 0 && y == 0 && k == 1)
            {
                currentStateFunc().CurrentStrokingColor = CMYKColor.Black;
            }
            else if (c == 0 && m == 0 && y == 0 && k == 0)
            {
                currentStateFunc().CurrentStrokingColor = CMYKColor.White;
            }

            currentStateFunc().CurrentStrokingColor = new CMYKColor(c, m, y, k);
        }

        public void SetNonStrokingColor(IReadOnlyList<decimal> operands, NameToken patternName)
        {
            if (patternName != null && CurrentNonStrokingColorSpaceDetails is PatternColorSpaceDetails patternColorSpaceDetails)
            {
                currentStateFunc().CurrentNonStrokingColor = patternColorSpaceDetails.GetPattern(patternName);
            }
            else
            {
                currentStateFunc().CurrentNonStrokingColor = CurrentNonStrokingColorSpaceDetails.GetColor(operands.Select(v => (double)v).ToArray());
            }
        }

        public void SetNonStrokingColorGray(decimal gray)
        {
            CurrentNonStrokingColorSpaceDetails = DeviceGrayColorSpaceDetails.Instance;

            if (gray == 0)
            {
                currentStateFunc().CurrentNonStrokingColor = GrayColor.Black;
            }
            else if (gray == 1)
            {
                currentStateFunc().CurrentNonStrokingColor = GrayColor.White;
            }
            else
            {
                currentStateFunc().CurrentNonStrokingColor = new GrayColor(gray);
            }
        }

        public void SetNonStrokingColorRgb(decimal r, decimal g, decimal b)
        {
            CurrentNonStrokingColorSpaceDetails = DeviceRgbColorSpaceDetails.Instance;

            if (r == 0 && g == 0 && b == 0)
            {
                currentStateFunc().CurrentNonStrokingColor = RGBColor.Black;
            }
            else if (r == 1 && g == 1 && b == 1)
            {
                currentStateFunc().CurrentNonStrokingColor = RGBColor.White;
            }
            else
            {
                currentStateFunc().CurrentNonStrokingColor = new RGBColor(r, g, b);
            }
        }

        public void SetNonStrokingColorCmyk(decimal c, decimal m, decimal y, decimal k)
        {
            CurrentNonStrokingColorSpaceDetails = DeviceCmykColorSpaceDetails.Instance;
            currentStateFunc().CurrentNonStrokingColor = new CMYKColor(c, m, y, k);
        }
    }
}
