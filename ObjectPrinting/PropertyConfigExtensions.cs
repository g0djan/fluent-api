using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, int> propertyConfig, CultureInfo cultureInfo) => 
            SetCulture(propertyConfig, cultureInfo);

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, double> propertyConfig, CultureInfo cultureInfo) =>
            SetCulture(propertyConfig, cultureInfo);

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, long> propertyConfig, CultureInfo cultureInfo) =>
            SetCulture(propertyConfig, cultureInfo);

        private static PrintingConfig<TOwner> SetCulture<TOwner, TPropType>(
            PropertyConfig<TOwner, TPropType> propertyConfig, 
            CultureInfo cultureInfo) => 
            propertyConfig.GetCopyParentConfig().UpdateCultureInfo(typeof(TPropType), cultureInfo);

        public static PrintingConfig<TOwner> CutProperties<TOwner>(
            this PropertyConfig<TOwner, string> propertyConfig, int maxLen) => 
            propertyConfig.GetCopyParentConfig().UpdateStringMaxLength(maxLen);
    }
}