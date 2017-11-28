using System;
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

        private ImmutableHashSet<Type> ExcludePropertiesTypes { get; set; }
        private ImmutableDictionary<Type, CultureInfo> CultureInfoForNumbers { get; set; }
        private ImmutableDictionary<string, Func<object, string>> Serializers { get; set; }
        private ImmutableHashSet<string> ExcludedProperties { get; set; }
        private int StringMaxLength { get; set; }

        public PrintingConfig()
        {
            ExcludePropertiesTypes = ImmutableHashSet.Create<Type>();
            CultureInfoForNumbers = ImmutableDictionary.Create<Type, CultureInfo>();
            Serializers = ImmutableDictionary.Create<string, Func<object, string>>();
            ExcludedProperties = ImmutableHashSet.Create<string>();
            StringMaxLength = MaxValue;
        }

        public string PrintToString(TOwner obj) => 
            PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (FinalTypes.Contains(type))
                return PrintFinalType(obj, type);

            var identation = GetIdentation(nestingLevel);
            return type.Name + 
                 type.GetProperties()
                .Where(propertyInfo => !ExcludedProperties.Contains(propertyInfo.Name) && 
                                       !ExcludePropertiesTypes.Contains(propertyInfo.PropertyType))
                .Select(propertyInfo => SerializeProperty(obj, nestingLevel, propertyInfo, identation))
                .Concat(new[] {""})
                .Aggregate((sentence, next) => sentence + next);
        }

        private static string GetIdentation(int nestingLevel) => 
            "\n" + new string('\t', nestingLevel + 1);

        private string PrintFinalType(object obj, Type type)
        {
            if (CultureInfoForNumbers.ContainsKey(type))
                return string.Format(CultureInfoForNumbers[type], "{0}", obj);
            return type == typeof(string) ? CutStringPresentation(obj.ToString()) : obj.ToString();
        }

        private string SerializeProperty(
            object obj, 
            int nestingLevel, 
            PropertyInfo propertyInfo, 
            string identation) => 
            Serializers.ContainsKey(propertyInfo.Name)
            ? identation + Serializers[propertyInfo.Name](propertyInfo.GetValue(obj))
            : StandardPropertySerialization(obj, nestingLevel, identation, propertyInfo);

        private string StandardPropertySerialization(
            object obj, 
            int nestingLevel, 
            string identation, 
            PropertyInfo propertyInfo) =>
            $"{identation}{propertyInfo.Name} = {PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1)}";

        private string CutStringPresentation(string str) => 
            str.Substring(0, Math.Min(str.Length, StringMaxLength));

        public PrintingConfig<TOwner> ExcludePropertiesOfType<TPropType>() => 
            UpdateExcludedPropertiesTypes(ExcludePropertiesTypes.Add(typeof(TPropType)));

        public PropertyConfig<TOwner, TPropType> Printing<TPropType>() => 
            new PropertyConfig<TOwner, TPropType>(this, null);

        public PropertyConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> selector) => 
            new PropertyConfig<TOwner, TPropType>(this, FetchPropertyNameFromExpression(selector));

        public PrintingConfig<TOwner> ExcludeProperty<TPropType>(Expression<Func<TOwner, TPropType>> selector) => 
            UpdateExcludedProperties(
            ExcludedProperties.Add(FetchPropertyNameFromExpression(selector)));

        private string FetchPropertyNameFromExpression<TPropType>(
            Expression<Func<TOwner, TPropType>> selector) => ((MemberExpression) selector.Body).Member.Name;


        public PrintingConfig<TOwner> CloneCurrentConfig()
        {
            return (PrintingConfig<TOwner>)MemberwiseClone();
        }

        public PrintingConfig<TOwner> UpdateExcludedPropertiesTypes(
            ImmutableHashSet<Type> excludedPropertiesTypes)
        {
            var config = CloneCurrentConfig();
            config.ExcludePropertiesTypes = excludedPropertiesTypes;
            return config;
        }

        public PrintingConfig<TOwner> UpdateCultureInfo(
            Type type, CultureInfo cultureInfo)
        {
            if (type != typeof(int) && type != typeof(double) && type != typeof(long))
                throw new ArgumentException("Culture info only for numbers think about it");
            var config = CloneCurrentConfig();
            config.CultureInfoForNumbers = CultureInfoForNumbers.SetItem(type, cultureInfo);
            return config;
        }

        public PrintingConfig<TOwner> UpdateSerializers(
            string propertyName, Func<object, string> serializer)
        {
            var config = CloneCurrentConfig();
            config.Serializers = Serializers.SetItem(propertyName, serializer);
            return config;
        }

        public PrintingConfig<TOwner> UpdateExcludedProperties(
            ImmutableHashSet<string> excludedProperties)
        {
            var config = CloneCurrentConfig();
            config.ExcludedProperties = excludedProperties;
            return config;
        }

        public PrintingConfig<TOwner> UpdateStringMaxLength(
            int maxLength)
        {
            var config = CloneCurrentConfig();
            config.StringMaxLength = maxLength;
            return config;
        }
    }
}