using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    delegate string ToStringConverter(object obj);

    public class PropertyConfig<TOwner, TPropType> : IPropertyConfig<TOwner>
    {
        private PrintingConfig<TOwner> PrintingConfig { get; }
        public PropertyInfo PropertyInfo { get; }

        public PropertyConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            PrintingConfig = printingConfig;
            PropertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> SetAlternativeSerialize(Func<TPropType, string> serializeFunc)
        {
            foreach (var propertyInfo in typeof(TOwner).GetProperties()
                .Where(prop => prop.PropertyType == typeof(TPropType)))
                UpdatePrintingConfigSerializers(serializeFunc, propertyInfo);
            return PrintingConfig;
        }

        public PrintingConfig<TOwner> SetSerializeForProperty(Func<TPropType, string> serializeFunc)
        {
            UpdatePrintingConfigSerializers(serializeFunc, PropertyInfo);
            return PrintingConfig;
        }

        private void UpdatePrintingConfigSerializers(Func<TPropType, string> serializeFunc, PropertyInfo propertyInfo) => 
            (PrintingConfig as IPrintingConfig)
            .Serializers[propertyInfo] = ChangeFuncSignature(serializeFunc);

        private static Func<object, string> ChangeFuncSignature(Func<TPropType, string> f) => 
            obj => f((TPropType)obj);

        PrintingConfig<TOwner> IPropertyConfig<TOwner>.ParentPrintingConfig => PrintingConfig;
    }

    interface IPropertyConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentPrintingConfig { get; }
    }

    public static class PropertyConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, int> propertyConfig, CultureInfo cultureInfo)
        {
            UpdatePrintingConfigCultureInfo(propertyConfig, cultureInfo);
            return (propertyConfig as IPropertyConfig<TOwner>).ParentPrintingConfig;
        }

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, double> propertyConfig, CultureInfo cultureInfo)
        {
            UpdatePrintingConfigCultureInfo(propertyConfig, cultureInfo);
            return (propertyConfig as IPropertyConfig<TOwner>).ParentPrintingConfig;
        }

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, long> propertyConfig, CultureInfo cultureInfo)
        {
            UpdatePrintingConfigCultureInfo(propertyConfig, cultureInfo);
            return (propertyConfig as IPropertyConfig<TOwner>).ParentPrintingConfig;
        }

        private static void UpdatePrintingConfigCultureInfo<TOwner, TPropType>(
            PropertyConfig<TOwner, TPropType> propertyConfig, 
            CultureInfo cultureInfo)
        {
            ((propertyConfig as IPropertyConfig<TOwner>)
                    .ParentPrintingConfig as IPrintingConfig)
                .CultureInfoForNumbers[typeof(TPropType)] = cultureInfo;
        }

        public static PrintingConfig<TOwner> CutProperties<TOwner>(
            this PropertyConfig<TOwner, string> propertyConfig, int maxLen)
        {
            (propertyConfig as IPropertyConfig<TOwner>).ParentPrintingConfig.StringMaxLength = maxLen;
            return (propertyConfig as IPropertyConfig<TOwner>).ParentPrintingConfig;
        }
    }

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
                    .Serializers.ContainsKey(propertyInfo).Should().BeTrue();
        }

        [Test]
        public void SetSerializeForProperty_AddFuncToPrintingConfig()
        {
            var printingConfig = new PrintingConfig<Person>();
            var propInfo = typeof(Person).GetProperty("Age");
            var propConfig = new PropertyConfig<Person, int>(printingConfig, propInfo);
            Func<int, string> f = str => "";
            propConfig.SetSerializeForProperty(f);
            (printingConfig as IPrintingConfig)
                .Serializers.ContainsKey(propInfo).Should().BeTrue();
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