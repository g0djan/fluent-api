using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintingConfig_Should
    {
        private TestClass testClass;

        [SetUp]
        public void SetUp()
        {
            testClass = new TestClass();
        }


        [Test]
        public void ExcludePropertiesOfType_AddTypeOfExcludingPropertyToHashSet()
        {
            var printingConfig = new PrintingConfig<TestClass>();
            printingConfig.ExcludePropertiesOfType<int>()
                .PrintToString(testClass)
                .Contains("Integer")
                .Should().BeFalse();
        }

        [Test]
        public void ExcludeProperty_AddExcludingPropertyToHashSet()
        {
            var printingConfig = new PrintingConfig<TestClass>();
            printingConfig.ExcludeProperty(test => test.String)
                .PrintToString(testClass)
                .Contains("String")
                .Should().BeFalse();
        }

        [Test]
        public void Printing_ReturnsPropertyConfigOfTPropType()
        {
            var printingConfig = new PrintingConfig<Person>();
            printingConfig.Printing<double>().GetType().Should().Be<PropertyConfig<Person, double>>();
        }

        [Test]
        public void Printing_ReturnPropertyConfigWithPropertyInfoFromExpression()
        {
            var printingConfig = new PrintingConfig<Person>();
            printingConfig.Printing(person => person.Id)
                .PropertyName
                .Should().Be(typeof(Person).GetProperty("Id")?.Name);
        }
    }
}