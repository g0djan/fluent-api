using System.Linq;
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
        public void ExcludePropertiesOfType_ExcludeAllPropertiesOfThisType()
        {
            var printingConfig = new PrintingConfig<TestClass>();
            var strPresentation = printingConfig.ExcludePropertiesOfType<int>()
                .PrintToString(testClass);
            typeof(TestClass)
                .GetProperties()
                .Where(prop => prop.PropertyType == typeof(int))
                .Any(prop => strPresentation.Contains(prop.Name))
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
                .Should().Be("Id");
        }
    }
}