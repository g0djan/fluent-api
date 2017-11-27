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
            return typeof(TOwner).GetProperties()
                .Where(prop => prop.PropertyType == typeof(TPropType))
                .Aggregate(PrintingConfig, (current, propertyInfo) =>
                    UpdatePrintingConfigSerializers(current, serializeFunc, propertyInfo.Name));
        }

        public PrintingConfig<TOwner> SetSerializeForProperty(Func<TPropType, string> serializeFunc) => 
            UpdatePrintingConfigSerializers(PrintingConfig, serializeFunc, PropertyName);

        private PrintingConfig<TOwner> UpdatePrintingConfigSerializers(PrintingConfig<TOwner> config,
            Func<TPropType, string> serializeFunc, 
            string propertyName)
        {
            return config
                .GetUpdated(serializer : Tuple.Create(propertyName, ChangeFuncSignature(serializeFunc)));
        }

        private static Func<object, string> ChangeFuncSignature(Func<TPropType, string> f) => 
            obj => f((TPropType)obj);

        PrintingConfig<TOwner> IPropertyConfig<TOwner>.ParentPrintingConfig => PrintingConfig;
    }

    interface IPropertyConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentPrintingConfig { get; }
    }
}