using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Int32;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly Type[] FinalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };



        private ImmutableHashSet<Type> ExcludedPropertiesTypes { get; }
        private ImmutableDictionary<Type, CultureInfo> CultureInfoForNumbers { get; }
        private ImmutableDictionary<string, Func<object, string>> Serializers { get; }
        private ImmutableHashSet<string> ExcludedProperties { get; }
        private int StringMaxLength { get; }

        public PrintingConfig()
        {
            ExcludedPropertiesTypes = ImmutableHashSet.Create<Type>();
            CultureInfoForNumbers = ImmutableDictionary.Create<Type, CultureInfo>();
            Serializers = ImmutableDictionary.Create<string, Func<object, string>>();
            ExcludedProperties = ImmutableHashSet.Create<string>();
            StringMaxLength = MaxValue;
        }

        public PrintingConfig(ImmutableHashSet<Type> excludedPropertiesTypes,
            ImmutableDictionary<Type, CultureInfo> cultureInfoForNumbers,
            ImmutableDictionary<string, Func<object, string>> serializers,
            ImmutableHashSet<string> excludedProperties,
            int stringMaxLength)
        {
            ExcludedPropertiesTypes = excludedPropertiesTypes;
            CultureInfoForNumbers = cultureInfoForNumbers;
            Serializers = serializers;
            ExcludedProperties = excludedProperties;
            StringMaxLength = stringMaxLength;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (FinalTypes.Contains(type))
                return type == typeof(string) ? CutStringPresentation(obj.ToString()) : obj.ToString();

            var identation = "\n" + new string('\t', nestingLevel + 1);
            return type.Name + 
                 type.GetProperties()
                .Where(propertyInfo => !ExcludedProperties.Contains(propertyInfo.Name) && 
                                       !ExcludedPropertiesTypes.Contains(type))
                .Select(propertyInfo => SerializeProperty(obj, nestingLevel, propertyInfo, identation))
                .Concat(new[] {""})
                .Aggregate((sentence, next) => sentence + next);
        }

        private string SerializeProperty(object obj, int nestingLevel, PropertyInfo propertyInfo, string identation) => 
            Serializers.ContainsKey(propertyInfo.Name)
            ? identation + Serializers[propertyInfo.Name](propertyInfo.GetValue(obj))
            : StandardPropertySerialization(obj, nestingLevel, identation, propertyInfo);

        private string StandardPropertySerialization(object obj, int nestingLevel, string identation, PropertyInfo propertyInfo) => 
            string.Join("", identation, propertyInfo.Name, " = ",
            PrintToString(propertyInfo.GetValue(obj),
                nestingLevel + 1));

        private string CutStringPresentation(string str) => 
            str.Substring(0, Math.Min(str.Length, StringMaxLength));

        public PrintingConfig<TOwner> ExcludePropertiesOfType<TPropType>()
        {
            return new PrintingConfig<TOwner>(ExcludedPropertiesTypes.Add(typeof(TPropType)),
                CultureInfoForNumbers,
                Serializers,
                ExcludedProperties,
                StringMaxLength);
        }

        public PropertyConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyConfig<TOwner, TPropType>(this, null);
        }

        public PropertyConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> selector)
        {
            return new PropertyConfig<TOwner, TPropType>(this, FetchPropertyInfoFromExpression(selector).Name);
        }

        public PrintingConfig<TOwner> ExcludeProperty<TPropType>(Expression<Func<TOwner, TPropType>> selector)
        {
            return new PrintingConfig<TOwner>(ExcludedPropertiesTypes,
                CultureInfoForNumbers,
                Serializers,
                ExcludedProperties.Add(FetchPropertyInfoFromExpression(selector).Name),
                StringMaxLength);
        }

        //public PrintingConfig<TOwner> UpdateCultureInfo<In>(CultureInfo cultureInfo)

        public PrintingConfig<TOwner> UpdateCultureInfo(Type type, CultureInfo cultureInfo)
        {
            return new PrintingConfig<TOwner>(ExcludedPropertiesTypes,
                CultureInfoForNumbers.SetItem(type, cultureInfo),
                Serializers,
                ExcludedProperties,
                StringMaxLength);
        }

        public PrintingConfig<TOwner> UpdateSerializers(string propertyName, Func<object, string> serializer)
        {
            return new PrintingConfig<TOwner>(ExcludedPropertiesTypes,
                CultureInfoForNumbers,
                Serializers.SetItem(propertyName, serializer),
                ExcludedProperties, 
                StringMaxLength);
        }

        public PrintingConfig<TOwner> UpdateExcludedProperties(string propertyName)
        {
            return new PrintingConfig<TOwner>(ExcludedPropertiesTypes,
                CultureInfoForNumbers,
                Serializers,
                ExcludedProperties.Add(propertyName),
                StringMaxLength);
        }

        public PrintingConfig<TOwner> UpdateStringMaxLength(int maxLength)
        {
            return new PrintingConfig<TOwner>(ExcludedPropertiesTypes,
                CultureInfoForNumbers,
                Serializers,
                ExcludedProperties,
                maxLength);
        }


        private PropertyInfo FetchPropertyInfoFromExpression<TPropType>(Expression<Func<TOwner, TPropType>> selector) =>
            ((MemberExpression) selector.Body).Member as PropertyInfo;
    }
}