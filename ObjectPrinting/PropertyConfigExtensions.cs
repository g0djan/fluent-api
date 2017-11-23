using System.Globalization;

namespace ObjectPrinting
{
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
}