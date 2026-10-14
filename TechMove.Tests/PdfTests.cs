using System.IO;
using Xunit;

namespace TechMove.Tests
{
    public class PdfTests
    {
        [Fact]
        public void RejectNonPdfFile()
        {
            string fileName = "test.exe";

            bool valid =
                Path.GetExtension(fileName)
                .ToLower() == ".pdf";

            Assert.False(valid);
        }
    }
}