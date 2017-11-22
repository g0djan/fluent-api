using System;

namespace ObjectPrinting.Tests
{
    public static class ObjectExtensions
    {
        public static string PrintToString<TOwner>(this TOwner obj)
        {
            return new PrintingConfig<TOwner>().PrintToString(obj);
        }

        public static string PrintToString<TOwner>(this TOwner obj, 
            Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> configure)
        {
            return configure(new PrintingConfig<TOwner>()).PrintToString(obj);
        }
    }
}