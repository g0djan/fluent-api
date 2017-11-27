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
                return PrintFinalType(obj, type);

            var identation = "\n" + new string('\t', nestingLevel + 1);
            return type.Name + 
                 type.GetProperties()
                .Where(propertyInfo => !ExcludedProperties.Contains(propertyInfo.Name) && 
                                       !ExcludedPropertiesTypes.Contains(type))
                .Select(propertyInfo => SerializeProperty(obj, nestingLevel, propertyInfo, identation))
                .Concat(new[] {""})
                .Aggregate((sentence, next) => sentence + next);
        }

        private string PrintFinalType(object obj, Type type)
        {
            if (CultureInfoForNumbers.ContainsKey(type))
                return string.Format(CultureInfoForNumbers[type], "{0}", obj);
            return type == typeof(string) ? CutStringPresentation(obj.ToString()) : obj.ToString();
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

        public PrintingConfig<TOwner> ExcludePropertiesOfType<TPropType>() => 
            GetUpdated(ExcludedPropertiesTypes.Add(typeof(TPropType)));

        public PropertyConfig<TOwner, TPropType> Printing<TPropType>() => 
            new PropertyConfig<TOwner, TPropType>(this, null);

        public PropertyConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> selector) => 
            new PropertyConfig<TOwner, TPropType>(this, FetchPropertyNameFromExpression(selector));

        public PrintingConfig<TOwner> ExcludeProperty<TPropType>(Expression<Func<TOwner, TPropType>> selector) => 
            GetUpdated(excludedProperties:
            ExcludedProperties.Add(FetchPropertyNameFromExpression(selector)));

        private string FetchPropertyNameFromExpression<TPropType>(
            Expression<Func<TOwner, TPropType>> selector) => ((MemberExpression) selector.Body).Member.Name;

        public PrintingConfig<TOwner> GetUpdated(
            ImmutableHashSet<Type> excludedPropertiesTypes = null,
            Tuple<Type, CultureInfo> cultureInfo = null,
            Tuple<string, Func<object, string>> serializer = null,
            ImmutableHashSet<string> excludedProperties = null,
            int? stringMaxLength = null)
        {
            return new PrintingConfig<TOwner>(
                excludedPropertiesTypes ?? ExcludedPropertiesTypes,
                cultureInfo != null ? CultureInfoForNumbers.SetItem(cultureInfo.Item1, cultureInfo.Item2) : CultureInfoForNumbers,
                serializer != null ? Serializers.SetItem(serializer.Item1, serializer.Item2) : Serializers,
                excludedProperties ?? ExcludedProperties,
                stringMaxLength ?? StringMaxLength);
        }
    }
}