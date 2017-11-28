using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private TestClass testClass;

        [SetUp]
        public void SetUp()
        {
            testClass = new TestClass();
        }


        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 176.1};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .ExcludePropertiesOfType<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<string>().SetSerializeForType(obj => "obj")
                //3. Для числовых типов указать культуру
                .Printing<double>().SetCultureInfo(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(pers => pers.Age).SetSerializeForProperty(property => "property")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>().CutProperties(10)
                //6. Исключить из сериализации конкретного свойства
                .ExcludeProperty(pers => pers.Id);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию		
            var defaultSerialization = person.PrintToString();
            //8. ...с конфигурированием
            var s3 = person.PrintToString(conf => conf.ExcludePropertiesOfType<double>());


            Console.WriteLine(s1);
            Console.WriteLine(defaultSerialization);
            Console.WriteLine(s3);
        }

        [Test]
        public void ExcludindPropertiesOfIntType()
        {
            var printer = ObjectPrinter.For<TestClass>()
                .ExcludePropertiesOfType<int>();
            Console.WriteLine(printer.PrintToString(testClass));
        }

        [Test]
        public void ExcludingThePropertyFromPresentation()
        {
            var printer = ObjectPrinter.For<TestClass>()
                .ExcludeProperty(testClazz => testClass.Second);
            Console.WriteLine(printer.PrintToString(testClass));
        }

        [Test]
        public void SetAlternativeSerialize()
        {
            var printer = ObjectPrinter.For<TestClass>().
                Printing<int>().
                SetSerializeForType(prop => "kek");
            Console.WriteLine(printer.PrintToString(testClass));
        }

        [Test]
        public void SetSerializeForProperty()
        {
            var printer = ObjectPrinter.For<TestClass>()
                .Printing(prop => prop.Second)
                .SetSerializeForProperty(prop => "rap" + (prop * 4).ToString());
            Console.WriteLine(printer.PrintToString(testClass));
        }

        [Test]
        public void CutStringProperties()
        {
            var printer = ObjectPrinter.For<TestClass>()
                .Printing<string>()
                .CutProperties(4);
            Console.WriteLine(printer.PrintToString(testClass));
        }

        [Test]
        public void ChangeCultureInfo()
        {
            var printer = ObjectPrinter.For<TestClass>()
                .Printing<double>()
                .SetCultureInfo(CultureInfo.InvariantCulture);
            Console.WriteLine(printer.PrintToString(testClass));
        }
    }
}