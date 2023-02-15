namespace UglyToad.PdfPig.SkiaSharp.Tests
{
    using global::SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ExportToImage2
    {
        private const int mult = 5;

        private const string ByzantineGenerals = "byz";
        private const string NonLatinAcrobatDistiller = "Single Page Non Latin - from acrobat distiller";
        private const string SingleGoogleDrivePage = "Single Page Simple - from google drive";
        private const string SinglePageFormattedType0Content = "Type0 Font";
        private const string SinglePageType1Content = "ICML03-081";
        private const string SingleInkscapePage = "Single Page Simple - from inkscape";
        private const string MotorInsuranceClaim = "Motor Insurance claim form";
        private const string PigProduction = "Pig Production Handbook";
        private const string SinglePage90ClockwiseRotation = "SinglePage90ClockwiseRotation - from PdfPig";
        private const string SinglePage180ClockwiseRotation = "SinglePage180ClockwiseRotation - from PdfPig";
        private const string SinglePage270ClockwiseRotation = "SinglePage270ClockwiseRotation - from PdfPig";
        private const string TransparentImage = "Random 2 Columns Lists Images";

        private const string cat_genetics = "cat-genetics.pdf";
        private const string data = "data.pdf";
        private const string FarmerMac = "FarmerMac.pdf";
        private const string FICTIF_TABLE_INDEX = "FICTIF_TABLE_INDEX.pdf";
        private const string pop_bugzilla37292 = "pop-bugzilla37292.pdf";
        private const string steam_in_page_dict = "steam_in_page_dict.pdf";

        private const string d_68_1990_01_A = "68-1990-01_A.pdf";
        private const string AcroFormsBasicFields = "AcroFormsBasicFields.pdf";
        private const string bold_italic = "bold-italic.pdf";

        private const string path_ext_oddeven = "path_ext_oddeven.pdf";
        private const string odwriteex = "odwriteex.pdf";
        private const string ssm2163 = "ssm2163.pdf";
        private const string Layer_pdf_322_High_Holborn_building_Brochure = "Layer pdf - 322_High_Holborn_building_Brochure.pdf";
        private const string SPARC_v9_Architecture_Manual = "SPARC - v9 Architecture Manual.pdf";

        private const string Baring_Amy_Tay_Finance_Manager_1 = "Baring_Amy.Tay_Finance.Manager-1.pdf";

        private const string complex_rotated = "complex rotated.pdf";

        private const string cat_genetics_bobld = "cat-genetics_bobld.pdf";

        private static string GetFilename(string name)
        {
            var documentFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Documents"));

            if (!name.EndsWith(".pdf"))
            {
                name += ".pdf";
            }

            return Path.Combine(documentFolder, name);
        }

        [Fact]
        public void complex_rotatedTest()
        {
            Run(complex_rotated, 1);
        }

        [Fact]
        public void Baring_Amy_Tay_Finance_Manager_1Test()
        {
            Run(Baring_Amy_Tay_Finance_Manager_1, 1);
        }

        [Fact]
        public void Baring_Amy_Tay_Finance_Manager_1Test2()
        {
            Run(Baring_Amy_Tay_Finance_Manager_1, 2);
        }

        [Fact]
        public void SPARC_v9_Architecture_ManualTest()
        {
            Run(SPARC_v9_Architecture_Manual, 1);
        }

        [Fact]
        public void Layer_pdf_322_High_Holborn_building_BrochureTest()
        {
            Run(Layer_pdf_322_High_Holborn_building_Brochure, 1);
        }

        [Fact]
        public void ssm2163Test()
        {
            Run(ssm2163, 1);
        }

        [Fact]
        public void odwriteexTest()
        {
            Run(odwriteex, 1);
        }

        [Fact]
        public void path_ext_oddevenTest()
        {
            Run(path_ext_oddeven, 1);
        }

        [Fact]
        public void d_68_1990_01_ATest()
        {
            Run(d_68_1990_01_A, 1);
        }

        [Fact]
        public void d_68_1990_01_ATest2()
        {
            Run(d_68_1990_01_A, 2);
        }

        [Fact]
        public void d_68_1990_01_ATest3()
        {
            Run(d_68_1990_01_A, 7);
        }

        [Fact]
        public void AcroFormsBasicFieldsTest()
        {
            Run(AcroFormsBasicFields, 1);
        }

        [Fact]
        public void bold_italicTest()
        {
            Run(bold_italic, 1);
        }


        [Fact]
        public void ByzantineGeneralsTest()
        {
            Run(ByzantineGenerals, 1);
        }

        [Fact]
        public void NonLatinAcrobatDistillerTest()
        {
            Run(NonLatinAcrobatDistiller, 1);
        }

        [Fact]
        public void SingleGoogleDrivePageTest()
        {
            Run(SingleGoogleDrivePage, 1);
        }

        [Fact]
        public void SinglePageFormattedType0ContentTest()
        {
            Run(SinglePageFormattedType0Content, 1);
        }

        [Fact]
        public void SinglePageType1ContentTest()
        {
            Run(SinglePageType1Content, 1);
        }

        [Fact]
        public void SinglePageType1ContentTest2()
        {
            Run(SinglePageType1Content, 4);
        }

        [Fact]
        public void SingleInkscapePageTest()
        {
            Run(SingleInkscapePage, 1);
        }

        [Fact]
        public void MotorInsuranceClaimTest()
        {
            Run(MotorInsuranceClaim, 1);
        }

        [Fact]
        public void PigProductionTest()
        {
            Run(PigProduction, 1);
        }

        [Fact]
        public void SinglePage90ClockwiseRotationTest()
        {
            Run(SinglePage90ClockwiseRotation, 1);
        }

        [Fact]
        public void SinglePage180ClockwiseRotationTest()
        {
            Run(SinglePage180ClockwiseRotation, 1);
        }

        [Fact]
        public void SinglePage270ClockwiseRotationTest()
        {
            Run(SinglePage270ClockwiseRotation, 1);
        }

        [Fact(Skip = "no doc")]
        public void TransparentImageTest()
        {
            Run(TransparentImage, 1);
        }

        [Fact]
        public void cat_geneticsTest_bobld()
        {
            Run(cat_genetics_bobld, 1);
        }


        [Fact]
        public void cat_geneticsTest()
        {
            Run(cat_genetics, 1);
        }

        [Fact]
        public void dataTest()
        {
            Run(data, 1);
        }

        [Fact]
        public void FarmerMacTest()
        {
            Run(FarmerMac, 1);
        }

        [Fact]
        public void FICTIF_TABLE_INDEXTest()
        {
            Run(FICTIF_TABLE_INDEX, 1);
        }

        [Fact]
        public void pop_bugzilla37292Test()
        {
            Run(pop_bugzilla37292, 1);
        }

        [Fact]
        public void steam_in_page_dictTest()
        {
            Run(steam_in_page_dict, 1);
        }

        public static void Run(string file, int pageNo)
        {
            const string directory = "Images2";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var imageName = $"{file}_{pageNo}_system-drawing.jpg";
            var savePath = Path.Combine(directory, imageName);

            var pdfFileName = GetFilename(file);
            using (var doc = PdfDocument.Open(pdfFileName))
            {
                var page = doc.GetPage(pageNo);

                SkiaSharpProcessor2 skiaSharpProcessor2 = new SkiaSharpProcessor2(page);
                using (var ms = skiaSharpProcessor2.GetImage(mult))
                using (Stream s = new FileStream(savePath, FileMode.Create))
                {
                    var bitmap = SKBitmap.Decode(ms);
                    SKData d = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100);

                    d.SaveTo(s);
                }
            }
        }
    }
}
