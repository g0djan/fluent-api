using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, int> propertyConfig, CultureInfo cultureInfo) => 
            (propertyConfig as IPropertyConfig<TOwner>).ParentPrintingConfig.UpdateCultureInfo(typeof(int), cultureInfo);

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, double> propertyConfig, CultureInfo cultureInfo) => 
            (propertyConfig as IPropertyConfig<TOwner>).ParentPrintingConfig.UpdateCultureInfo(typeof(double), cultureInfo);

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(
            this PropertyConfig<TOwner, long> propertyConfig, CultureInfo cultureInfo) => 
            (propertyConfig as IPropertyConfig<TOwner>).ParentPrintingConfig.UpdateCultureInfo(typeof(long), cultureInfo);

        public static PrintingConfig<TOwner> CutProperties<TOwner>(
            this PropertyConfig<TOwner, string> propertyConfig, int maxLen) => 
            (propertyConfig as IPropertyConfig<TOwner>).ParentPrintingConfig.UpdateStringMaxLength(maxLen);
    }
}