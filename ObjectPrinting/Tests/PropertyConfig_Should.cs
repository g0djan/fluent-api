using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PropertyConfig_Should
    {
        private TestClass testClass;

        [SetUp]
        public void SetUp()
        {
            testClass = new TestClass();
        }

        [Test]
        public void SetAlternativeSerialize_AddFuncToPrintingConfig()
        {
            var printingConfig = new PrintingConfig<TestClass>();
            var propConfig = new PropertyConfig<TestClass, string>(printingConfig, null);
            Func<string, string> f = str => "topTest";
            propConfig
                .SetAlternativeSerialize(f)
                .PrintToString(testClass)
                .Contains("topTest")
                .Should().BeTrue();
        }

        [Test]
        public void SetSerializeForProperty_AddFuncToPrintingConfig()
        {
            var printingConfig = new PrintingConfig<TestClass>();
            var propInfo = typeof(TestClass).GetProperty("String");
            var propConfig = new PropertyConfig<TestClass, int>(printingConfig, propInfo.Name);
            Func<int, string> f = str => "topTest";
            propConfig.SetSerializeForProperty(f)
                .PrintToString()
                .Contains("String")
                .Should().BeFalse();
        }

//        [Test]
//        public void SetCultureInfo_UpdatingCultureInfoInPrintingConfig()
//        {
//            var printingConfig = new PrintingConfig<Person>();
//            var propConfig = new PropertyConfig<Person, long>(printingConfig, null);
//            propConfig.SetCultureInfo(CultureInfo.CurrentCulture);
//            printingConfig
//                .CultureInfoForNumbers[typeof(long)].Should().Be(CultureInfo.CurrentCulture);
//        }
    }
}