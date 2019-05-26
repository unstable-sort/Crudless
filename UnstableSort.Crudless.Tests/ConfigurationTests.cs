using NUnit.Framework;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Tests.RequestTests;

namespace UnstableSort.Crudless.Tests
{
    [TestFixture]
    public class ConfigurationTests : BaseUnitTest
    {
        private CrudlessConfigManager _profileManager;

        [SetUp]
        public void SetUp()
        {
            _profileManager = Container.GetInstance<CrudlessConfigManager>();
        }

        [Test]
        public void Test_RequestWithoutProfile_FindsDefaultConfig()
        {
            var defaultConfig = new RequestConfig<CreateUserWithResponseRequest>();
            var createUserConfig = _profileManager.GetRequestConfigFor<CreateUserWithResponseRequest>();

            Assert.IsNotNull(defaultConfig);
            Assert.IsNotNull(createUserConfig);
            Assert.AreEqual(defaultConfig.GetType(), createUserConfig.GetType());
        }

        [Test]
        public void Test_FindAliases_HaveSameResult()
        {
            var requestConfig1 = _profileManager.GetRequestConfigFor(typeof(CreateUserWithoutResponseRequest));
            var requestConfig2 = _profileManager.GetRequestConfigFor<CreateUserWithoutResponseRequest>();

            Assert.IsNotNull(requestConfig1);
            Assert.AreEqual(requestConfig1.GetType(), requestConfig2.GetType());
        }
    }
}