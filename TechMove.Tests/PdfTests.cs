using System.IO;
using Xunit;

namespace TechMove.Tests
{
    public class PdfTests
    {
        [Fact]
        public void AcceptPdfFile()
        {
            // Arrange
            string fileName = "Agreement.pdf";

            // Act
            bool valid = Path.GetExtension(fileName)
                .ToLower() == ".pdf";

            // Assert
            Assert.True(valid);
        }

        [Fact]
        public void RejectNonPdfFile()
        {
            // Arrange
            string fileName = "Test.exe";

            // Act
            bool valid = Path.GetExtension(fileName)
                .ToLower() == ".pdf";

            // Assert
            Assert.False(valid);
        }
    }
}