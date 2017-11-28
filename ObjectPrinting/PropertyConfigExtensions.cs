using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, int> propertyConfig, CultureInfo cultureInfo) =>
            propertyConfig.SetCulture(cultureInfo);

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, double> propertyConfig, CultureInfo cultureInfo) =>
            propertyConfig.SetCulture(cultureInfo);

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, long> propertyConfig, CultureInfo cultureInfo) =>
            propertyConfig.SetCulture(cultureInfo);

        public static PrintingConfig<TOwner> CutProperties<TOwner>(
            this PropertyConfig<TOwner, string> propertyConfig, int maxLen) => 
            propertyConfig.SetCutProperty(maxLen);
    }
}