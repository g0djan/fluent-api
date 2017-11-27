using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
	[TestFixture]
	public class ObjectPrinterAcceptanceTests
	{
		[Test]
		public void Demo()
		{
			var person = new Person { Name = "Alex", Age = 19 };

		    var printer = ObjectPrinter.For<Person>()
		        //1. Исключить из сериализации свойства определенного типа
		        .ExcludePropertiesOfType<Guid>()
		        //2. Указать альтернативный способ сериализации для определенного типа
		        .Printing<string>().SetAlternativeSerialize(obj => "obj")
		        //3. Для числовых типов указать культуру
		        .Printing<int>().SetCultureInfo(CultureInfo.CurrentCulture)
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
	}
}