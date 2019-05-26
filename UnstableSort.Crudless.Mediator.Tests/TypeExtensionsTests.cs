using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UnstableSort.Crudless.Mediator.Tests
{
    [TestFixture]
    public class TypeExtensionsTests : BaseUnitTest
    {
        [Test]
        public void TypeHasAttribute_TypeFindsAttribute()
        {
            // Arrange
            var type = typeof(BaseDoNotValidateRequest);

            // Act
            var result = type.HasAttribute(typeof(FakeDoNotValidateAttribute), false);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TypeDoesNotHaveAttribute_TypeDoesNotFindAttribute()
        {
            // Arrange
            var type = typeof(FakeDoValidateRequest);

            // Act
            var result = type.HasAttribute(typeof(FakeDoNotValidateAttribute), false);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void BaseInterfaceHasAttribute_TypeFindsAttribute()
        {
            // Arrange
            var type = typeof(FakeInheritsDoNotValidateRequest);

            // Act
            var result = type.HasAttribute(typeof(FakeDoNotValidateAttribute), true);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void BaseTypeHasAttribute_TypeFindsAttribute()
        {
            // Arrange
            var type = typeof(FakeInheritsDoNotValidateRequest);

            // Act
            var result = type.HasAttribute(typeof(FakeDoNotValidateAttribute), true);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TypeHasDerivedAttribute_TypeFindsAttribute()
        {
            // Arrange
            var type = typeof(BaseDoNotValidateRequest);

            // Act
            var result = type.HasAttribute(typeof(DoNotValidateAttribute));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void RequestHasAttribute_HandlerTypeFindsAttribute()
        {
            // Arrange
            var handlerType = typeof(FakeDoNotValidateHandler);

            // Act
            var result = handlerType.RequestHasAttribute(typeof(DoNotValidateAttribute));

            // Assert
            Assert.IsTrue(result);
        }
        
        [Test]
        public void RequestDoesNotHaveAttribute_HandlerTypeDoesNotFindAttribute()
        {
            // Arrange
            var handlerType = typeof(FakeDoValidateHandler);

            // Act
            var result = handlerType.RequestHasAttribute(typeof(DoNotValidateAttribute));

            // Assert
            Assert.IsFalse(result);
        }
    }

    public class FakeDoNotValidateAttribute : DoNotValidateAttribute
    {
    }

    [FakeDoNotValidate]
    public interface IFakeDoNotValidateRequest : IRequest
    {

    }

    [FakeDoNotValidate]
    public class BaseDoNotValidateRequest : IRequest
    {

    }

    public class FakeImplementsDoNotValidateRequest : IFakeDoNotValidateRequest
    {
    }

    public class FakeInheritsDoNotValidateRequest : BaseDoNotValidateRequest
    {
    }

    public class FakeDoValidateRequest : IRequest
    {
    }

    public class FakeDoNotValidateHandler : IRequestHandler<FakeInheritsDoNotValidateRequest>
    {
        public Task<Response> HandleAsync(FakeInheritsDoNotValidateRequest request, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }

    public class FakeDoValidateHandler : IRequestHandler<FakeDoValidateRequest>
    {
        public Task<Response> HandleAsync(FakeDoValidateRequest request, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
