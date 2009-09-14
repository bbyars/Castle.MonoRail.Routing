using System.Collections;
using NUnit.Framework;

namespace Castle.MonoRail.Routing.Test
{
    [TestFixture]
    public class DictionaryExtensionsTest
    {
        [Test]
        public void EmptyDictionaryPrettyPrint()
        {
            Assert.AreEqual("{}", new Hashtable().PrettyPrint());
        }

        [Test]
        public void SingleElementPrettyPrint()
        {
            Assert.AreEqual("{", 0);
        }
        // Can we do a visitor extension hierarchy?
    }

    public interface IVisitor
    {
        void Visit(IDictionary dictionary);
        void Visit(int number);
        void Visit(string text);
        void Visit(object obj);
    }

    public static class PrettyPrintExtensions
    {
        public static void Accept(this object obj, IVisitor visitor)
        {
            visitor.Visit(obj);
        }
    }
}
