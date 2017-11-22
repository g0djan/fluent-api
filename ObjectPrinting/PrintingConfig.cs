using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private HashSet<Type> ExcludedPropertiesTypes { get; }
        private Dictionary<Type, CultureInfo> CultureInfoForNumbers { get; }
        private Dictionary<PropertyInfo, Func<object, string>> Serializers { get; }
        private HashSet<PropertyInfo> ExcludedProperties { get; }
        public int StringMaxLength { get; set; }

        public PrintingConfig()
        {
            ExcludedPropertiesTypes = new HashSet<Type>();
            CultureInfoForNumbers = new Dictionary<Type, CultureInfo>();
            ExcludedProperties = new HashSet<PropertyInfo>();
            StringMaxLength = int.MaxValue;
            Serializers = new Dictionary<PropertyInfo, Func<object, string>>();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            var type = obj.GetType();
            var str = obj.ToString();
            if (finalTypes.Contains(type))
                return str.Substring(0, Math.Min(str.Length, StringMaxLength)) + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (ExcludedProperties.Contains(propertyInfo) ||
                    ExcludedPropertiesTypes.Contains(type))
                    continue;
                if (Serializers.ContainsKey(propertyInfo))
                {
                    sb.Append(Serializers[propertyInfo](propertyInfo.GetValue(obj)));
                    continue;
                }
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

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
            return new PropertyConfig<TOwner, TPropType>(this, FetchPropertyInfoFromExpression(selector));
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
        Dictionary<PropertyInfo, Func<object, string>> IPrintingConfig.Serializers => Serializers;
        HashSet<PropertyInfo> IPrintingConfig.ExcludedProperties => ExcludedProperties;
    }

    interface IPrintingConfig
    {
        HashSet<Type> ExcludedPropertiesOfType { get; }
        Dictionary<Type, CultureInfo> CultureInfoForNumbers { get; }
        Dictionary<PropertyInfo, Func<object, string>> Serializers { get; }
        HashSet<PropertyInfo> ExcludedProperties { get; }
        int StringMaxLength { get; }
    }

    [TestFixture]
    public class PrintingConfig_Should
    {
        [Test]
        public void DoSomething_WhenSomething()
        {
            var person = new Person {Id = Guid.NewGuid(), Name = "godjan", Height = 180.180180, Age = 20};
            var personPrintingConfig = new PrintingConfig<Person>();
            Console.WriteLine(personPrintingConfig.PrintToString(person));
            Console.WriteLine("you must do it");
        }

        [Test]
        public void ExcludePropertiesOfType_AddTypeOfExcludingPropertyToHashSet()
        {
            var printingConfig = new PrintingConfig<Person>();
            printingConfig.ExcludePropertiesOfType<int>();
            ((IPrintingConfig) printingConfig)
                .ExcludedPropertiesOfType.Contains(typeof(int))
                .Should().BeTrue();
        }

        [Test]
        public void ExcludeProperty_AddExcludingPropertyToHashSet()
        {
            var printingConfig = new PrintingConfig<Person>();
            printingConfig.ExcludeProperty(person => person.Id);
            ((IPrintingConfig) printingConfig)
                .ExcludedProperties.Contains(typeof(Person).GetProperty("Id"))
                .Should().BeTrue();
        }

        [Test]
        public void Printing_ReturnsPropertyConfigOfTPropType()
        {
            var printingConfig = new PrintingConfig<Person>();
            printingConfig.Printing<double>().GetType().Should().Be<PropertyConfig<Person, double>>();
        }

        [Test]
        public void Printing_ReturnPropertyConfigWithPropertyInfoFromExpression()
        {
            var printingConfig = new PrintingConfig<Person>();
            printingConfig.Printing(person => person.Id)
                .PropertyInfo.Name
                .Should().Be(typeof(Person).GetProperty("Id")?.Name);
        }
    }
}