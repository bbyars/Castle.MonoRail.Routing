using System.Collections.Generic;
using System.Collections.Specialized;
using Castle.MonoRail.Routing;
using NUnit.Framework;

namespace Castle.MonoRail.Routing.Test
{
    [TestFixture]
    public class HashTest
    {
        [Test]
        public void EmptyHash()
        {
            var hash = new Hash();
            Assert.AreEqual(0, hash.Keys.Count);
        }

        [Test]
        public void HashWithNullInitializer()
        {
            var hash = new Hash(null);
            Assert.AreEqual(0, hash.Keys.Count);
        }

        [Test]
        public void HashWithAnonymousObjectInitializer()
        {
            var hash = new Hash(new { First = "1", Second = 2 });
            Assert.AreEqual(hash, new Dictionary<string, object> { { "First", "1" }, { "Second", 2 } });
        }

        [Test]
        public void ToNameValueCollection()
        {
            var hash = new Hash(new { First = "1", Second = 2 });
            var collection = hash.ToNameValueCollection();
            Assert.AreEqual(2, collection.Count);
            Assert.AreEqual("1", collection["First"]);
            Assert.AreEqual("2", collection["Second"]);
        }

        [Test]
        public void ToNameValueCollectionWithNullValue()
        {
            var hash = new Hash();
            hash["Key"] = null;
            var collection = hash.ToNameValueCollection();
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("", collection["Key"]);
        }

        [Test]
        public void FromNameValueCollection()
        {
            var collection = new NameValueCollection { { "First", "1" }, { "Second", "2" } };
            var expected = new Hash(new { First = "1", Second = "2" });
            Assert.AreEqual(expected, new Hash(collection));
        }
    }
}