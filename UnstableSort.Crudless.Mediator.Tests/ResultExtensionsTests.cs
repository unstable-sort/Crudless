using System.Threading.Tasks;
using NUnit.Framework;

namespace UnstableSort.Crudless.Mediator.Tests
{
    [TestFixture]
    public class ResultExtensionsTests : BaseUnitTest
    {
        [Test]
        public void AsResponse_ReturnsObjectAsDataInResponse()
        {
            // Arrange
            var request = new FakeRequest { Foo = "Bar" };

            // Act
            var result = request.AsResponse();

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.AreEqual("Bar", result.Result.Foo);
        }

        [Test]
        public async Task AsResponseAsync_ReturnsObjectAsDataInResponse()
        {
            // Arrange
            var request = new FakeRequest { Foo = "Bar" };

            // Act
            var result = await request.AsResponseAsync();

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.AreEqual("Bar", result.Result.Foo);
        }
    }

    public class FakeRequest : IRequest
    {
        public string Foo { get; set; }
    }
}
