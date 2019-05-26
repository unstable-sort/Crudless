using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UnstableSort.Crudless.Mediator.Tests
{
    [TestFixture]
    public class ErrorExtensionsTests : BaseUnitTest
    {
        [Test]
        public void AsResponse_WithSingleError_ReturnsResponseWithError()
        {
            // Arrange
            var error = new Error { ErrorMessage = "Error.", PropertyName = "Property" };

            // Act
            var result = error.AsResponse();

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Property", result.Errors[0].PropertyName);
            Assert.AreEqual("Error.", result.Errors[0].ErrorMessage);
        }

        [Test]
        public async Task AsResponseAsync_WithSingleError_ReturnsResponseWithError()
        {
            // Arrange
            var error = new Error {ErrorMessage = "Error.", PropertyName = "Property"};

            // Act
            var result = await error.AsResponseAsync();

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Property", result.Errors[0].PropertyName);
            Assert.AreEqual("Error.", result.Errors[0].ErrorMessage);
        }

        [Test]
        public void AsResponseWithResult_WithSingleError_ReturnsResponseWithError()
        {
            // Arrange
            var error = new Error { ErrorMessage = "Error.", PropertyName = "Property" };

            // Act
            var result = error.AsResponse<string>();

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsNull(result.Result);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Property", result.Errors[0].PropertyName);
            Assert.AreEqual("Error.", result.Errors[0].ErrorMessage);
        }

        [Test]
        public async Task AsResponseAsyncWithResult_WithSingleError_ReturnsResponseWithError()
        {
            // Arrange
            var error = new Error { ErrorMessage = "Error.", PropertyName = "Property" };

            // Act
            var result = await error.AsResponseAsync<string>();

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsNull(result.Result);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Property", result.Errors[0].PropertyName);
            Assert.AreEqual("Error.", result.Errors[0].ErrorMessage);
        }

        [Test]
        public void AsResponse_WithErrorList_ReturnsResponseWithErrors()
        {
            // Arrange
            var errors = new List<Error>
            {
                new Error { ErrorMessage = "Error 1.", PropertyName = "Property1" },
                new Error { ErrorMessage = "Error 2.", PropertyName = "Property2" }
            };

            // Act
            var result = errors.AsResponse();

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("Property1", result.Errors[0].PropertyName);
            Assert.AreEqual("Error 1.", result.Errors[0].ErrorMessage);
            Assert.AreEqual("Property2", result.Errors[1].PropertyName);
            Assert.AreEqual("Error 2.", result.Errors[1].ErrorMessage);
        }

        [Test]
        public async Task AsResponseAsync_WithErrorList_ReturnsResponseWithErrors()
        {
            // Arrange
            var errors = new List<Error>
            {
                new Error { ErrorMessage = "Error 1.", PropertyName = "Property1" },
                new Error { ErrorMessage = "Error 2.", PropertyName = "Property2" }
            };

            // Act
            var result = await errors.AsResponseAsync();

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("Property1", result.Errors[0].PropertyName);
            Assert.AreEqual("Error 1.", result.Errors[0].ErrorMessage);
            Assert.AreEqual("Property2", result.Errors[1].PropertyName);
            Assert.AreEqual("Error 2.", result.Errors[1].ErrorMessage);
        }

        [Test]
        public void AsResponseWithResult_WithErrorList_ReturnsResponseWithErrors()
        {
            // Arrange
            var errors = new List<Error>
            {
                new Error { ErrorMessage = "Error 1.", PropertyName = "Property1" },
                new Error { ErrorMessage = "Error 2.", PropertyName = "Property2" }
            };

            // Act
            var result = errors.AsResponse<string>();

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("Property1", result.Errors[0].PropertyName);
            Assert.AreEqual("Error 1.", result.Errors[0].ErrorMessage);
            Assert.AreEqual("Property2", result.Errors[1].PropertyName);
            Assert.AreEqual("Error 2.", result.Errors[1].ErrorMessage);
        }

        [Test]
        public async Task AsResponseAsyncWithResult_WithErrorList_ReturnsResponseWithErrors()
        {
            // Arrange
            var errors = new List<Error>
            {
                new Error { ErrorMessage = "Error 1.", PropertyName = "Property1" },
                new Error { ErrorMessage = "Error 2.", PropertyName = "Property2" }
            };

            // Act
            var result = await errors.AsResponseAsync<string>();

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("Property1", result.Errors[0].PropertyName);
            Assert.AreEqual("Error 1.", result.Errors[0].ErrorMessage);
            Assert.AreEqual("Property2", result.Errors[1].PropertyName);
            Assert.AreEqual("Error 2.", result.Errors[1].ErrorMessage);
        }
    }
}
