using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintingConfig_Should
    {
        [Test]
        public void ExcludePropertiesOfType_AddTypeOfExcludingPropertyToHashSet()
        {
            var printingConfig = new PrintingConfig<Person>();
            printingConfig.ExcludePropertiesOfType<int>();
            ((IPrintingConfig) printingConfig)
                .ExcludedPropertiesOfType.Contains(typeof(int))
                .Should().BeTrue();
        }

        [Test]
        public void ExcludeProperty_AddExcludingPropertyToHashSet()
        {
            var printingConfig = new PrintingConfig<Person>();
            printingConfig.ExcludeProperty(person => person.Id);
            ((IPrintingConfig) printingConfig)
                .ExcludedProperties.Contains(typeof(Person).GetProperty("Id"))
                .Should().BeTrue();
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