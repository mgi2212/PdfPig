namespace UglyToad.PdfPig.SkiaSharp.Tests
{
    using System.IO;
    using Xunit;

    public class IccTests
    {
        // https://color.org/srgbprofiles.xalter

        [Fact]
        public void IccV4()
        {
            /*
            const string path = "C:\\Users\\Bob\\Downloads\\sRGB_v4_ICC_preference_displayclass.icc";

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                var profile = IccProfile.IccProfile.Create(ms.ToArray());
                Assert.NotNull(profile);
                Assert.Equal(4, profile.Header.VersionMajor);
            }
            */
        }
    }
}
