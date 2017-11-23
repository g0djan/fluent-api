using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PropertyConfig_Should
    {
        [Test]
        public void SetAlternativeSerialize_AddFuncToPrintingConfig()
        {
            var printingConfig = new PrintingConfig<Person>();
            var propConfig = new PropertyConfig<Person, string>(printingConfig, null);
            Func<string, string> f = str => "";
            propConfig.SetAlternativeSerialize(f);
            foreach (var propertyInfo in typeof(string).GetProperties())
                (printingConfig as IPrintingConfig)
                    .Serializers.ContainsKey(propertyInfo.Name).Should().BeTrue();
        }

        [Test]
        public void SetSerializeForProperty_AddFuncToPrintingConfig()
        {
            var printingConfig = new PrintingConfig<Person>();
            var propInfo = typeof(Person).GetProperty("Age");
            var propConfig = new PropertyConfig<Person, int>(printingConfig, propInfo.Name);
            Func<int, string> f = str => "";
            propConfig.SetSerializeForProperty(f);
            (printingConfig as IPrintingConfig)
                .Serializers.ContainsKey(propInfo.Name).Should().BeTrue();
        }

        [Test]
        public void SetCultureInfo_UpdatingCultureInfoInPrintingConfig()
        {
            var printingConfig = new PrintingConfig<Person>();
            var propConfig = new PropertyConfig<Person, long>(printingConfig, null);
            propConfig.SetCultureInfo(CultureInfo.CurrentCulture);
            (printingConfig as IPrintingConfig)
                .CultureInfoForNumbers[typeof(long)].Should().Be(CultureInfo.CurrentCulture);
        }
    }
}