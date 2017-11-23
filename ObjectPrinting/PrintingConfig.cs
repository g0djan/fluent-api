using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private static readonly Type[] FinalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private HashSet<Type> ExcludedPropertiesTypes { get; }
        private Dictionary<Type, CultureInfo> CultureInfoForNumbers { get; }
        private Dictionary<string, Func<object, string>> Serializers { get; }
        private HashSet<PropertyInfo> ExcludedProperties { get; }
        public int StringMaxLength { get; set; }

        public PrintingConfig()
        {
            ExcludedPropertiesTypes = new HashSet<Type>();
            CultureInfoForNumbers = new Dictionary<Type, CultureInfo>();
            ExcludedProperties = new HashSet<PropertyInfo>();
            StringMaxLength = int.MaxValue;
            Serializers = new Dictionary<string, Func<object, string>>();
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
                return CutStrPresentation(obj.ToString());

            var identation = "\n" + new string('\t', nestingLevel + 1);
            return type.Name + 
                (from prop in type.GetProperties()
                 where !ExcludedProperties.Contains(prop) && !ExcludedPropertiesTypes.Contains(type)
                 select SerializeProperty(obj, nestingLevel, prop, identation))
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

        private string CutStrPresentation(string str) => 
            str.Substring(0, Math.Min(str.Length, StringMaxLength));

        public PrintingConfig<TOwner> ExcludePropertiesOfType<TPropType>()
        {
            ExcludedPropertiesTypes.Add(typeof(TPropType));
            return this;
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
            ExcludedProperties.Add(FetchPropertyInfoFromExpression(selector));
            return this;
        }

        private PropertyInfo FetchPropertyInfoFromExpression<TPropType>(Expression<Func<TOwner, TPropType>> selector) =>
            ((MemberExpression) selector.Body).Member as PropertyInfo;

        HashSet<Type> IPrintingConfig.ExcludedPropertiesOfType => ExcludedPropertiesTypes;
        Dictionary<Type, CultureInfo> IPrintingConfig.CultureInfoForNumbers => CultureInfoForNumbers;
        Dictionary<string, Func<object, string>> IPrintingConfig.Serializers => Serializers;
        HashSet<PropertyInfo> IPrintingConfig.ExcludedProperties => ExcludedProperties;
    }

    interface IPrintingConfig
    {
        HashSet<Type> ExcludedPropertiesOfType { get; }
        Dictionary<Type, CultureInfo> CultureInfoForNumbers { get; }
        Dictionary<string, Func<object, string>> Serializers { get; }
        HashSet<PropertyInfo> ExcludedProperties { get; }
        int StringMaxLength { get; }
    }
}