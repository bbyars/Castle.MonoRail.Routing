using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Castle.MonoRail.Routing
{
    /// <summary>
    /// A friendly Hashtable object
    /// </summary>
    public class Hash : Dictionary<string, object>
    {
        public Hash() : this(null)
        {
        }

        /// <summary>
        /// The object whose property/value combinations will be added to this Hashtable.
        /// Alternatively, you can pass a NameValueCollection, which is handy when you
        /// want to convert a NameValueCollection into a Dictionary.
        /// </summary>
        /// <example>
        /// var hash = new Hash(new {FirstName = "Bob", LastName = "Smith"});
        /// Assert.AreEqual("Bob", hash["FirstName"]);
        /// Assert.AreEqual("Smith", hash["LastName"]);
        /// </example>
        public Hash(object values) : this(values, StringComparer.InvariantCulture)
        {
        }

        public Hash(object values, IEqualityComparer<string> comparer) : base(comparer)
        {
            var collection = values as NameValueCollection;
            if (collection != null)
            {
                foreach (var key in collection.AllKeys)
                {
                    Add(key, collection[key]);
                }
            }
            else
            {
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(values))
                {
                    Add(property.Name, property.GetValue(values));
                }
            }
        }

        /// <summary>
        /// Converts the hash to a NameValueCollection (consider adding to an IDictionaryExtensions class)
        /// </summary>
        public virtual NameValueCollection ToNameValueCollection()
        {
            var result = new NameValueCollection();
            foreach (var pair in this)
            {
                var value = pair.Value == null ? "" : pair.Value.ToString();
                result.Add(pair.Key, value);
            }

            return result;
        }
    }
}