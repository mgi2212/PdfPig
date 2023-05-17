namespace UglyToad.PdfPig.SkiaSharp.Tests
{
    using global::SkiaSharp;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using Xunit;

    public class ExportToImage
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

        private const string url_link = "url_link.pdf";

        private const string complex_rotated_highlight = "complex rotated_highlight.pdf";
        private const string issue_484 = "issue_484.pdf";

        private const string GHOSTSCRIPT_458780_1 = "GHOSTSCRIPT-458780-1.pdf";

        private const string ZeroHeightLetters = "ZeroHeightLetters.pdf";

        private const string TIKA_1228_0 = "TIKA-1228-0.pdf";
        private const string TIKA_1552_0 = "TIKA-1552-0.pdf";
        private const string TIKA_1575_2 = "TIKA-1575-2.pdf";
        private const string TIKA_1857_0 = "TIKA-1857-0.pdf";
        private const string TIKA_2121_0 = "TIKA-2121-0.pdf";

        private const string output_w3c_csswg_drafts_issues2023 = "output_w3c_csswg_drafts_issues2023.pdf";

        private const string MOZILLA_7952_0 = "MOZILLA-7952-0.pdf";
        private const string MOZILLA_11308_0 = "MOZILLA-11308-0.pdf";
        private const string MOZILLA_10145_0 = "MOZILLA-10145-0.pdf";

        private const string MOZILLA_10448_0 = "MOZILLA-10448-0.pdf";
        private const string MOZILLA_10427_0 = "MOZILLA-10427-0.pdf";

        private const string DeviceN_CS_test = "DeviceN_CS_test.pdf";

        private const string version4pdf = "version4pdf.pdf";

        private const string d_22060_A1_01_Plans_1 = "22060_A1_01_Plans-1.pdf"; // https://github.com/mozilla/pdf.js/issues/3136
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
        public void version4pdfTest()
        {
            RunAllPages(version4pdf);
        }

        [Fact]
        public void DeviceN_CS_testTest()
        {
            RunAllPages(DeviceN_CS_test);
        }

        [Fact(Skip = "too long")]
        public void MOZILLA_10448_0Test()
        {
            RunAllPages(MOZILLA_10448_0);
        }

        [Fact]
        public void d_22060_A1_01_Plans_1Test()
        {
            Run(d_22060_A1_01_Plans_1, 1);
        }

        [Fact]
        public void MOZILLA_10427_0Test()
        {
            // Image issue  
            Run(MOZILLA_10427_0, 2);
        }

        [Fact]
        public void MOZILLA_7952_0Test()
        {
            RunAllPages(MOZILLA_7952_0);
        }

        [Fact]
        public void MOZILLA_11308_0Test()
        {
            RunAllPages(MOZILLA_11308_0);
        }

        [Fact]
        public void MOZILLA_10145_0Test()
        {
            RunAllPages(MOZILLA_10145_0); // DeviceN with stream
        }

        [Fact]
        public void output_w3c_csswg_drafts_issues2023Test()
        {
            Run(output_w3c_csswg_drafts_issues2023, 1);
        }

        [Fact]
        public void TIKA_2121_0Test()
        {
            Run(TIKA_2121_0, 9);
        }

        [Fact(Skip = "Not all")]
        public void TIKA_2121_0TestAll()
        {
            RunAllPages(TIKA_2121_0);
        }

        [Fact]
        public void TIKA_1857_0TestAll()
        {
            RunAllPages(TIKA_1857_0);
        }

        [Fact]
        public void TIKA_1575_2TestAll()
        {
            RunAllPages(TIKA_1575_2);
        }

        [Fact]
        public void TIKA_1552_0Test()
        {
            Run(TIKA_1552_0, 1);
        }

        [Fact]
        public void TIKA_1552_0Test1()
        {
            Run(TIKA_1552_0, 3);
        }

        [Fact]
        public void TIKA_1228_0Test() // Issue - lightbulb inverted
        {
            Run(TIKA_1228_0, 66);
        }

        [Fact(Skip = "Not all")]
        public void TIKA_1228_0TestAll()
        {
            RunAllPages(TIKA_1228_0);
        }

        [Fact]
        public void ZeroHeightLettersTest()
        {
            Run(ZeroHeightLetters, 1);
        }

        [Fact]
        public void GHOSTSCRIPT_458780_1Test()
        {
            Run(GHOSTSCRIPT_458780_1, 1);
        }

        [Fact]
        public void issue_484Test()
        {
            Run(issue_484, 1);
        }

        [Fact]
        public void url_linkTest()
        {
            Run(url_link, 1);
        }

        [Fact]
        public void complex_rotated_highlightTest()
        {
            Run(complex_rotated_highlight, 1);
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

        [Fact] //(Skip = "Not all")]
        public void d_68_1990_01_ATestAll()
        {
            RunAllPages(d_68_1990_01_A);
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
        public void PigProductionTest2()
        {
            Run(PigProduction, 5);
        }

        [Fact]
        public void PigProductionTest3()
        {
            Run(PigProduction, 15);
        }

        [Fact]
        public void PigProductionTest4()
        {
            Run(PigProduction, 5);
        }

        [Fact] //(Skip = "Not all")]
        public void PigProductionTestAll()
        {
            RunAllPages(PigProduction);
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

        private const string directory = "Images2";

        private static void Run(string file, int pageNo)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var imageName = $"{file}_{pageNo}_skia-sharp.png";
            var savePath = Path.Combine(directory, imageName);

            var pdfFileName = GetFilename(file);
            using (var doc = PdfDocument.Open(pdfFileName, new ParsingOptions { UseLenientParsing = false }))
            {
                var page = doc.GetPage(pageNo);
                var images = page.GetImages().ToArray();
                using (var ms = new SkiaSharpProcessor(page).GetImage(mult))
                using (Stream s = new FileStream(savePath, FileMode.Create))
                {
                    var bitmap = SKBitmap.Decode(ms);
                    SKData d = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100);
                    d.SaveTo(s);
                }
            }
        }

        private static void RunAllPages(string file)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var pdfFileName = GetFilename(file);

            using (var document = PdfDocument.Open(pdfFileName, new ParsingOptions { UseLenientParsing = false }))
            {
                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    var imageName = $"{file}_all_{i + 1}_skia-sharp.png";
                    var savePath = Path.Combine(directory, imageName);

                    var page = PdfDocument.Open(pdfFileName, new ParsingOptions { UseLenientParsing = false }).GetPage(i + 1); // TODO - wrong, we reopen every time

                    using (var ms = new SkiaSharpProcessor(page).GetImage(mult))
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
}
