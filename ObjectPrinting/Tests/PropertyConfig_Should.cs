using System;
using System.Linq;
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
        public void SetAlternativeSerialize_ChangeAllOfThisTypePropertySerializaion()
        {
            var printingConfig = new PrintingConfig<TestClass>();
            var propConfig = new PropertyConfig<TestClass, int>(printingConfig, null);
            Func<int, string> f = str => "topTest";
            propConfig
                .SetSerializeForType(f)
                .PrintToString(testClass)
                .Replace("topTest", "#")
                .Count(c => c == '#')
                .Should().Be(2);
        }

        [Test]
        public void SetSerializeForProperty_ChangeThisPropertySerializaion()
        {
            var printingConfig = new PrintingConfig<TestClass>();
            var propInfo = typeof(TestClass).GetProperty("String");
            var propConfig = new PropertyConfig<TestClass, string>(printingConfig, propInfo.Name);
            Func<string, string> f = str => "topTest";
            propConfig.SetSerializeForProperty(f)
                .PrintToString()
                .Contains("String")
                .Should().BeFalse();
        }

        [Test]
        public void TestCutProperties()
        {
            var printingConfig = new PrintingConfig<TestClass>();
            var propInfo = typeof(TestClass).GetProperty("String");
            var propConfig = new PropertyConfig<TestClass, string>(printingConfig, propInfo.Name);
            propConfig.CutProperties(4)
                .PrintToString(testClass)
                .Contains("there")
                .Should().BeFalse();
        }

    }
}