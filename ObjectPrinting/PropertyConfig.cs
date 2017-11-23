using System;
using System.Linq;

namespace ObjectPrinting
{
    public class PropertyConfig<TOwner, TPropType> : IPropertyConfig<TOwner>
    {
        private PrintingConfig<TOwner> PrintingConfig { get; }
        public string PropertyName { get; }

        public PropertyConfig(PrintingConfig<TOwner> printingConfig, string propertyName)
        {
            PrintingConfig = printingConfig;
            PropertyName = propertyName;
        }

        public PrintingConfig<TOwner> SetAlternativeSerialize(Func<TPropType, string> serializeFunc)
        {
            foreach (var propertyInfo in typeof(TOwner).GetProperties()
                .Where(prop => prop.PropertyType == typeof(TPropType)))
                UpdatePrintingConfigSerializers(serializeFunc, propertyInfo.Name);
            return PrintingConfig;
        }

        public PrintingConfig<TOwner> SetSerializeForProperty(Func<TPropType, string> serializeFunc)
        {
            UpdatePrintingConfigSerializers(serializeFunc, PropertyName);
            return PrintingConfig;
        }

        private void UpdatePrintingConfigSerializers(Func<TPropType, string> serializeFunc, string propertyName) => 
            (PrintingConfig as IPrintingConfig)
            .Serializers[propertyName] = ChangeFuncSignature(serializeFunc);

        private static Func<object, string> ChangeFuncSignature(Func<TPropType, string> f) => 
            obj => f((TPropType)obj);

        PrintingConfig<TOwner> IPropertyConfig<TOwner>.ParentPrintingConfig => PrintingConfig;
    }

    interface IPropertyConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentPrintingConfig { get; }
    }
}