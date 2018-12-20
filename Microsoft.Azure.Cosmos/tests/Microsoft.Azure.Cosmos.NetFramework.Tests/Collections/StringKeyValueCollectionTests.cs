﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Azure.Cosmos.Internal;
    using VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class StringKeyValueCollectionTests
    {
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestSimpleOperations()
        {
            INameValueCollection collection = new StringKeyValueCollection();
            int count = 10;
            for (int i = 0; i < count; ++i)
            {
                collection.Add(i.ToString(), i.ToString());
            }
            foreach (string key in collection)
            {
                Assert.AreEqual(key, collection[key]);
            }
        }

        private enum NameValueCollectionType
        {
            StringKeyValueCollection,
            NameValueCollectionWrapper,
            DictionaryNameValueCollection
        }

        private static Array NameValueCollectionTypes = Enum.GetValues(typeof(NameValueCollectionType));

        private static INameValueCollection CreateNameValueCollection(NameValueCollectionType type = NameValueCollectionType.StringKeyValueCollection)
        {
            return CreateNameValueCollection(0, 0, type);
        }

        private static INameValueCollection CreateNameValueCollection(StringComparer comparer, NameValueCollectionType type = NameValueCollectionType.StringKeyValueCollection)
        {
            INameValueCollection nameValueCollection = null;

            switch (type)
            {
                case NameValueCollectionType.StringKeyValueCollection:
                    nameValueCollection = new StringKeyValueCollection(comparer);
                    break;
                case NameValueCollectionType.NameValueCollectionWrapper:
                    nameValueCollection = new NameValueCollectionWrapper(comparer);
                    break;
                case NameValueCollectionType.DictionaryNameValueCollection:
                    nameValueCollection = new DictionaryNameValueCollection(comparer);
                    break;
                default:
                    throw new ArgumentException("Not supported type of NameValueCollection");
            }

            return nameValueCollection;
        }

        private static INameValueCollection CreateNameValueCollection(INameValueCollection collection, NameValueCollectionType type = NameValueCollectionType.StringKeyValueCollection)
        {
            INameValueCollection nameValueCollection = null;

            switch (type)
            {
                case NameValueCollectionType.StringKeyValueCollection:
                    nameValueCollection = new StringKeyValueCollection(collection);
                    break;
                case NameValueCollectionType.NameValueCollectionWrapper:
                    nameValueCollection = new NameValueCollectionWrapper(collection);
                    break;
                case NameValueCollectionType.DictionaryNameValueCollection:
                    nameValueCollection = new DictionaryNameValueCollection(collection);
                    break;
                default:
                    throw new ArgumentException("Not supported type of NameValueCollection");
            }

            return nameValueCollection;
        }

        private static INameValueCollection CreateNameValueCollection(int count, int start = 0, NameValueCollectionType type = NameValueCollectionType.StringKeyValueCollection, bool populateData = true)
        {
            INameValueCollection nameValueCollection = null;

            switch (type)
            {
                case NameValueCollectionType.StringKeyValueCollection:
                    nameValueCollection = new StringKeyValueCollection(count);
                    break;
                case NameValueCollectionType.NameValueCollectionWrapper:
                    nameValueCollection = new NameValueCollectionWrapper(count);
                    break;
                case NameValueCollectionType.DictionaryNameValueCollection:
                    nameValueCollection = new DictionaryNameValueCollection(count);
                    break;
                default:
                    throw new ArgumentException("Not supported type of NameValueCollection");
            }

            if (populateData)
            {
                for (int i = start; i < start + count; i++)
                {
                    nameValueCollection.Add("Name_" + i, "Value_" + i);
                }
            }

            return nameValueCollection;
        }

        #region Add tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAdd()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestAdd(0, 0, type);
                TestAdd(0, 5, type);
                TestAdd(5, 0, type);
                TestAdd(5, 5, type);
            }
        }

        private void TestAdd(int count1, int count2, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection1 = CreateNameValueCollection(count1, 0, type);
            INameValueCollection nameValueCollection2 = CreateNameValueCollection(count2, count1, type);

            nameValueCollection2.Add(nameValueCollection1);
            Assert.AreEqual(count1 + count2, nameValueCollection2.Count());
            Assert.AreEqual(count1 + count2, nameValueCollection2.AllKeys().Length);
            Assert.AreEqual(count1 + count2, nameValueCollection2.Keys().Count());

            var keys1 = nameValueCollection1.Keys().ToArray();
            var keys2 = nameValueCollection2.Keys().ToArray();

            for (int i = 0; i < count1; i++)
            {
                string name = keys1[i];
                string value = nameValueCollection1.Get(name);
                Assert.IsTrue(nameValueCollection2.AllKeys().Contains(name));
                Assert.IsTrue(nameValueCollection2.Keys().Contains(name));

                Assert.AreEqual(value, nameValueCollection2.Get(name));
                Assert.AreEqual(value, nameValueCollection2[name]);

                Assert.AreEqual(value, nameValueCollection2.Get(keys2[i + count2]));
                Assert.AreEqual(value, nameValueCollection2[keys2[i + count2]]);

                Assert.AreEqual(name, keys2[i + count2]);

                Assert.IsTrue(new string[] { value }.SequenceEqual(nameValueCollection2.GetValues(name)));
                Assert.IsTrue(new string[] { value }.SequenceEqual(nameValueCollection2.GetValues(keys2[i + count2])));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAddExistingKeys()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestAddExistingKeys(type);
            }
        }

        private void TestAddExistingKeys(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection1 = CreateNameValueCollection(type);
            INameValueCollection nameValueCollection2 = CreateNameValueCollection(type);

            string name = "name";
            string value1 = "value1";
            string value2 = "value2";
            nameValueCollection1.Add(name, value1);
            nameValueCollection2.Add(name, value2);

            nameValueCollection2.Add(nameValueCollection1);
            Assert.AreEqual(1, nameValueCollection2.Count());
            Assert.AreEqual(value2 + "," + value1, nameValueCollection2[name]);
            Assert.IsTrue(new string[] { value2, value1 }.SequenceEqual(nameValueCollection2.GetValues(name)));
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAddMultipleValues()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestAddMultipleValues(type);
            }
        }

        private void TestAddMultipleValues(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection1 = CreateNameValueCollection(type);
            INameValueCollection nameValueCollection2 = CreateNameValueCollection(type);

            string name = "name";
            string value1 = "value1";
            string value2 = "value2";
            nameValueCollection1.Add(name, value1);
            nameValueCollection1.Add(name, value2);

            nameValueCollection2.Add(nameValueCollection1);
            Assert.AreEqual(1, nameValueCollection2.Count());
            Assert.AreEqual(value1 + "," + value2, nameValueCollection2[name]);
            Assert.IsTrue(new string[] { value1, value2 }.SequenceEqual(nameValueCollection2.GetValues(name)));
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAddNameValueCollectionWithNullKeys()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestAddNameValueCollectionWithNullKeys(type);
            }
        }

        private void TestAddNameValueCollectionWithNullKeys(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection1 = CreateNameValueCollection(0, 0, type, false);
            INameValueCollection nameValueCollection2 = CreateNameValueCollection(0, 0, type, false);
            INameValueCollection nameValueCollection3 = CreateNameValueCollection(0, 0, type, false);

            string nullKeyValue1 = "value";
            string nullKeyValue2 = "value";
            nameValueCollection1.Add(null, nullKeyValue1);
            nameValueCollection2.Add(null, nullKeyValue2);

            nameValueCollection3.Add(nameValueCollection1);

            Assert.AreEqual(1, nameValueCollection2.Count());
            Assert.IsTrue(nameValueCollection3.AllKeys().Contains(null));
            Assert.AreEqual(nullKeyValue1, nameValueCollection3[null]);

            nameValueCollection3.Add(nameValueCollection2);
            Assert.IsTrue(nameValueCollection3.AllKeys().Contains(null));
            Assert.AreEqual(nullKeyValue1 + "," + nullKeyValue2, nameValueCollection3[null]);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAddNameValueCollectionWithNullValues()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestAddNameValueCollectionWithNullValues(type);
            }
        }

        private void TestAddNameValueCollectionWithNullValues(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection1 = CreateNameValueCollection(type);
            INameValueCollection nameValueCollection2 = CreateNameValueCollection(type);

            string nullValueName = "name";
            nameValueCollection1.Add(nullValueName, null);
            nameValueCollection2.Add(nameValueCollection1);

            Assert.AreEqual(1, nameValueCollection2.Count());
            Assert.IsTrue(nameValueCollection2.AllKeys().Contains(nullValueName));
            Assert.IsNull(nameValueCollection2[nullValueName]);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestAddNullNameValueCollectionThrowsArgumentNullException()
        {
            CreateNameValueCollection().Add(null);
        }

        #endregion Add tests

        #region Add (string, string) tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAddStringString()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestAddStringString(type);
            }
        }

        private void TestAddStringString(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(type);
            for (int i = 0; i < 10; i++)
            {
                string name = "Name_" + i;
                string value = "Value_" + i;
                nameValueCollection.Add(name, value);
                Assert.AreEqual(i + 1, nameValueCollection.Count());
                Assert.AreEqual(i + 1, nameValueCollection.AllKeys().Length);
                Assert.AreEqual(i + 1, nameValueCollection.Keys().Count());

                // We should be able to access values by the name
                Assert.AreEqual(value, nameValueCollection[name]);
                Assert.AreEqual(value, nameValueCollection.Get(name));

                Assert.IsTrue(nameValueCollection.AllKeys().Contains(name));
                Assert.IsTrue(nameValueCollection.Keys().Cast<string>().Contains(name));

                Assert.IsTrue(new string[] { value }.SequenceEqual(nameValueCollection.GetValues(name)));

                // Get(string), GetValues(string) and this[string] should be case insensitive
                Assert.AreEqual(value, nameValueCollection[name.ToUpperInvariant()]);
                Assert.AreEqual(value, nameValueCollection[name.ToLowerInvariant()]);

                Assert.AreEqual(value, nameValueCollection.Get(name.ToUpperInvariant()));
                Assert.AreEqual(value, nameValueCollection.Get(name.ToLowerInvariant()));

                Assert.IsTrue(new string[] { value }.SequenceEqual(nameValueCollection.GetValues(name.ToUpperInvariant())));
                Assert.IsTrue(new string[] { value }.SequenceEqual(nameValueCollection.GetValues(name.ToLowerInvariant())));

                Assert.IsFalse(nameValueCollection.AllKeys().Contains(name.ToUpperInvariant()));
                Assert.IsFalse(nameValueCollection.AllKeys().Contains(name.ToLowerInvariant()));

                Assert.IsFalse(nameValueCollection.Keys().Cast<string>().Contains(name.ToUpperInvariant()));
                Assert.IsFalse(nameValueCollection.Keys().Cast<string>().Contains(name.ToLowerInvariant()));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAddStringStringNullName()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestAddStringStringNullName(type);
            }
        }

        private void TestAddStringStringNullName(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(type);
            string value = "value";
            nameValueCollection.Add(null, value);
            Assert.AreEqual(1, nameValueCollection.Count());
            Assert.AreEqual(1, nameValueCollection.AllKeys().Length);
            Assert.AreEqual(1, nameValueCollection.Keys().Count());

            Assert.IsTrue(nameValueCollection.AllKeys().Contains(null));
            Assert.IsTrue(nameValueCollection.Keys().Cast<string>().Contains(null));

            Assert.AreEqual(value, nameValueCollection[null]);
            Assert.AreEqual(value, nameValueCollection.Get(null));

            Assert.IsTrue(new string[] { value }.SequenceEqual(nameValueCollection.GetValues(null)));
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAddStringStringNullValue()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestAddStringStringNullValue(type);
            }
        }

        private void TestAddStringStringNullValue(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(type);
            string name = "name";
            nameValueCollection.Add(name, null);
            Assert.AreEqual(1, nameValueCollection.Count());
            Assert.AreEqual(1, nameValueCollection.AllKeys().Length);
            Assert.AreEqual(1, nameValueCollection.Keys().Count());

            Assert.IsTrue(nameValueCollection.AllKeys().Contains(name));
            Assert.IsTrue(nameValueCollection.Keys().Cast<string>().Contains(name));

            Assert.IsNull(nameValueCollection[name]);
            Assert.IsNull(nameValueCollection.Get(name));

            Assert.IsNull(nameValueCollection.GetValues(name));
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAddStringStringAddingValueToExistingNameAppendsValueToOriginalValue()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestAddStringStringAddingValueToExistingNameAppendsValueToOriginalValue(type);
            }
        }

        private void TestAddStringStringAddingValueToExistingNameAppendsValueToOriginalValue(NameValueCollectionType type)
        {
            var nameValueCollection = CreateNameValueCollection(type);
            string name = "name";
            nameValueCollection.Add(name, "value1");
            nameValueCollection.Add(name, "value2");
            nameValueCollection.Add(name, null);

            Assert.AreEqual(1, nameValueCollection.Count());
            Assert.AreEqual(1, nameValueCollection.AllKeys().Length);
            Assert.AreEqual(1, nameValueCollection.Keys().Count());

            Assert.IsTrue(nameValueCollection.AllKeys().Contains(name));
            Assert.IsTrue(nameValueCollection.Keys().Cast<string>().Contains(name));

            string[] expected = new string[] { "value1", "value2" };
            string expectedString = string.Join(",", expected);

            Assert.AreEqual(expectedString, nameValueCollection[name]);
            Assert.AreEqual(expectedString, nameValueCollection.Get(name));

            Assert.IsTrue(expected.SequenceEqual(nameValueCollection.GetValues(name)));    
        }

        #endregion

        #region Clear tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestClear()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestClear(0, type);
                TestClear(5, type);
            }
        }
        
        private void TestClear(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(10, 0, type);
            nameValueCollection.Clear();
            Assert.AreEqual(0, nameValueCollection.Count());
            Assert.AreEqual(0, nameValueCollection.AllKeys().Length);
            Assert.AreEqual(0, nameValueCollection.Keys().Count());

            nameValueCollection.Clear();
            Assert.AreEqual(0, nameValueCollection.Count());
            Assert.AreEqual(0, nameValueCollection.AllKeys().Length);
            Assert.AreEqual(0, nameValueCollection.Keys().Count());
        }

        #endregion

        #region Ctor tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestCtorEmpty()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestCtorEmpty(type);
            }
        }

        private void TestCtorEmpty(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(type);
            Assert.AreEqual(0, nameValueCollection.Count());
            Assert.AreEqual(0, nameValueCollection.Keys().Count());
            Assert.AreEqual(0, nameValueCollection.AllKeys().Length);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestCtorInt()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestCtorInt(0, type);
                TestCtorInt(5, type);
            }
        }

        private void TestCtorInt(int capacity, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(capacity, 0, type, false);
            Assert.AreEqual(0, nameValueCollection.Count());
            Assert.AreEqual(0, nameValueCollection.Keys().Count());
            Assert.AreEqual(0, nameValueCollection.AllKeys().Length);

            int newCount = capacity + 10;
            for (int i = 0; i < newCount; i++)
            {
                nameValueCollection.Add("Name_" + i, "Value_" + i);
            }
            Assert.AreEqual(newCount, nameValueCollection.Count());
        }

        private delegate void AssertThrowDelegate();

        private static void AssertThrow(Type exceptionType, AssertThrowDelegate code)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestCtorNegativeCapacityThrowsArgumentOutOfRangeException()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestCtorNegativeCapacityThrowsArgumentOutOfRangeException(type);
            }
        }

        private void TestCtorNegativeCapacityThrowsArgumentOutOfRangeException(NameValueCollectionType type)
        {
            AssertThrow(typeof(ArgumentOutOfRangeException), () => CreateNameValueCollection(-1, 0, type));
            AssertThrow(typeof(OutOfMemoryException), () => CreateNameValueCollection(int.MaxValue));
        }

        private static IEnumerable<INameValueCollection> CtorNameValueCollectionTestData(NameValueCollectionType type)
        {
            yield return CreateNameValueCollection(10, 0, type, false);
            yield return CreateNameValueCollection(type);
            yield return CreateNameValueCollection(10, 0, type, true);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestCtorNameValueCollection()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                foreach (INameValueCollection collection in CtorNameValueCollectionTestData(type))
                {
                    TestCtorNameValueCollection(collection);
                }
            }
        }

        private void TestCtorNameValueCollection(INameValueCollection nameValueCollection1)
        {
            INameValueCollection nameValueCollection2 = CreateNameValueCollection(nameValueCollection1);

            Assert.AreEqual(nameValueCollection1.Count(), nameValueCollection2.Count());
            Assert.IsTrue(nameValueCollection1.Keys().SequenceEqual(nameValueCollection2.Keys()));
            Assert.IsTrue(nameValueCollection1.AllKeys().SequenceEqual(nameValueCollection2.AllKeys()));

            // Modify nameValueCollection1 does not affect nameValueCollection2
            string previous = nameValueCollection1["Name_1"];
            nameValueCollection1["Name_1"] = "newvalue";
            Assert.AreEqual(previous, nameValueCollection2["Name_1"]);

            nameValueCollection1.Remove("Name_1");
            Assert.AreEqual(previous, nameValueCollection2["Name_1"]);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestCtorNullNameValueCollectionThrowsArgumentNullException()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestCtorNullNameValueCollectionThrowsArgumentNullException(type);
            }
        }

        private void TestCtorNullNameValueCollectionThrowsArgumentNullException(NameValueCollectionType type)
        {
            AssertThrow(typeof(ArgumentNullException), () => CreateNameValueCollection((INameValueCollection)null, type));
        }

        private static IEnumerable<StringComparer> CtorStringComparerTestData()
        {
            yield return new IdiotComparer();
            yield return null;
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestCtorIEqualityComparer()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                foreach (StringComparer comparer in CtorStringComparerTestData())
                {
                    TestCtorStringComparer(comparer, type);
                }
            }
        }

        private void TestCtorStringComparer(StringComparer comparer, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(comparer, type);
            VerifyCtorStringComparer(nameValueCollection, comparer, 10);
        }

        private void VerifyCtorStringComparer(INameValueCollection nameValueCollection, StringComparer equalityComparer, int newCount)
        {
            Assert.AreEqual(0, nameValueCollection.Count());
            Assert.AreEqual(0, nameValueCollection.Keys().Count());
            Assert.AreEqual(0, nameValueCollection.AllKeys().Length);

            string[] values = new string[newCount];
            for (int i = 0; i < newCount; i++)
            {
                string value = "Value_" + i;
                nameValueCollection.Add("Name_" + i, value);
                values[i] = value;
            }
            if (equalityComparer?.GetType() == typeof(IdiotComparer))
            {
                Assert.AreEqual(1, nameValueCollection.Count());
                string expectedValues = string.Join(",", values);
                Assert.AreEqual(expectedValues, nameValueCollection["Name_1"]);
                Assert.AreEqual(expectedValues, nameValueCollection["any-name"]);

                nameValueCollection.Remove("any-name");
                Assert.AreEqual(0, nameValueCollection.Count());
            }
            else
            {
                Assert.AreEqual(newCount, nameValueCollection.Count());
                nameValueCollection.Remove("any-name");
                Assert.AreEqual(newCount, nameValueCollection.Count());
            }
        }

        private class IdiotComparer : StringComparer
        {
            public override int Compare(string x, string y)
            {
                return 0;
            }

            public override bool Equals(string x, string y)
            {
                return true;
            }

            public override int GetHashCode(string obj)
            {
                return 0;
            }
        }

        #endregion

        #region Get string tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestGetNoSuchNameReturnsNull()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestGetNoSuchNameReturnsNull(0, type);
                TestGetNoSuchNameReturnsNull(5, type);
            }
        }

        private void TestGetNoSuchNameReturnsNull(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count, 0, type);
            Assert.IsNull(nameValueCollection.Get("no-such-name"));
            Assert.IsNull(nameValueCollection.Get(null));
        }

        #endregion

        #region Get values string tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestGetValuesNoSuchNameReturnsNull()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestGetValuesNoSuchNameReturnsNull(0, type);
                TestGetValuesNoSuchNameReturnsNull(5, type);
            }
        }

        private void TestGetValuesNoSuchNameReturnsNull(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count);
            Assert.IsNull(nameValueCollection.GetValues("no-such-name"));
            Assert.IsNull(nameValueCollection.GetValues(null));
        }

        #endregion

        #region Remove tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestRemove()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestRemove(0, type);
                TestRemove(5, type);
            }
        }

        private void TestRemove(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count);
            nameValueCollection.Remove("no-such-name");
            nameValueCollection.Remove(null);
            Assert.AreEqual(count, nameValueCollection.Count());

            for (int i = 0; i < count; i++)
            {
                string name = "Name_" + i;
                // Remove should be case insensitive
                if (i == 0)
                {
                    nameValueCollection.Remove(name.ToUpperInvariant());
                }
                else if (i == 1)
                {
                    nameValueCollection.Remove(name.ToLowerInvariant());
                }
                else
                {
                    nameValueCollection.Remove(name);
                }
                Assert.AreEqual(count - i - 1, nameValueCollection.Count());
                Assert.AreEqual(count - i - 1, nameValueCollection.AllKeys().Length);
                Assert.AreEqual(count - i - 1, nameValueCollection.Keys().Count());

                Assert.IsNull(nameValueCollection[name]);
                Assert.IsNull(nameValueCollection.Get(name));

                Assert.IsFalse(nameValueCollection.AllKeys().Contains(name));
                Assert.IsFalse(nameValueCollection.Keys().Cast<string>().Contains(name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestRemoveMultipleValuesSameName()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestRemoveMultipleValuesSameName(type);
            }
        }

        private void TestRemoveMultipleValuesSameName(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(0, 0, type, false);
            string name = "name";
            nameValueCollection.Add(name, "value1");
            nameValueCollection.Add(name, "value2");
            nameValueCollection.Add(name, "value3");

            nameValueCollection.Remove(name);
            Assert.IsNull(nameValueCollection[name]);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestRemoveNullName()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestRemoveNullName(type);
            }
        }

        private void TestRemoveNullName(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(0, 0, type, false);
            nameValueCollection.Add(null, "value");
            nameValueCollection.Remove(null);
            Assert.AreEqual(0, nameValueCollection.Count());
            Assert.IsNull(nameValueCollection[null]);
        }

        #endregion

        #region Set item tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestItemSet()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestItemSet(type);
            }
        }

        private void TestItemSet(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(0, 0, type, false);
            for (int i = 0; i < 10; i++)
            {
                string newName = "Name_" + i;
                string newValue = "Value_" + i;
                nameValueCollection[newName] = newValue;

                Assert.AreEqual(i + 1, nameValueCollection.Count());
                Assert.AreEqual(newValue, nameValueCollection[newName]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestItemSetOvewriteExistingValue()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestItemSetOvewriteExistingValue(type);
            }
        }

        private void TestItemSetOvewriteExistingValue(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(type);
            string name = "name";
            string value = "value";
            nameValueCollection.Add(name, "old-value");

            nameValueCollection[name] = value;
            Assert.AreEqual(value, nameValueCollection[name]);
            Assert.IsTrue(new string[] { value }.SequenceEqual(nameValueCollection.GetValues(name)));
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestItemSetNullName()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestItemSetNullName(0, type);
                TestItemSetNullName(5, type);
            }
        }

        private void TestItemSetNullName(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count, 0, type);

            string nullNameValue = "value";
            nameValueCollection[null] = nullNameValue;
            Assert.AreEqual(count + 1, nameValueCollection.Count());
            Assert.AreEqual(nullNameValue, nameValueCollection[null]);

            string newNullNameValue = "newvalue";
            nameValueCollection[null] = newNullNameValue;
            Assert.AreEqual(count + 1, nameValueCollection.Count());
            Assert.AreEqual(newNullNameValue, nameValueCollection[null]);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestItemSetNullValue()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestItemSetNullValue(0, type);
                TestItemSetNullValue(5, type);
            }
        }

        private void TestItemSetNullValue(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count, 0, type);

            string nullValueName = "name";
            nameValueCollection[nullValueName] = null;
            Assert.AreEqual(count + 1, nameValueCollection.Count());
            Assert.IsNull(nameValueCollection[nullValueName]);

            nameValueCollection[nullValueName] = "abc";
            Assert.AreEqual(count + 1, nameValueCollection.Count());
            Assert.AreEqual("abc", nameValueCollection[nullValueName]);

            nameValueCollection[nullValueName] = null;
            Assert.AreEqual(count + 1, nameValueCollection.Count());
            Assert.IsNull(nameValueCollection[nullValueName]);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestItemSetIsCaseSensitive()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestItemSetIsCaseSensitive(type);
            }
        }

        private void TestItemSetIsCaseSensitive(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(0, 0, type, false);
            nameValueCollection["name"] = "value1";
            nameValueCollection["Name"] = "value2";
            nameValueCollection["NAME"] = "value3";
            Assert.AreEqual(1, nameValueCollection.Count());
            Assert.AreEqual("value3", nameValueCollection["name"]);
        }

        #endregion

        #region Set string string tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestSet()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestSet(type);
            }
        }

        private void TestSet(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(0, 0, type, false);
            int newCount = 10;
            for (int i = 0; i < newCount; i++)
            {
                string newName = "Name_" + i;
                string newValue = "Value_" + i;
                nameValueCollection.Set(newName, newValue);

                Assert.AreEqual(i + 1, nameValueCollection.Count());
                Assert.AreEqual(newValue, nameValueCollection.Get(newName));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestSetOvewriteExistingValue()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestSetOvewriteExistingValue(type);
            }
        }

        private void TestSetOvewriteExistingValue(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(0, 0, type, false);
            string name = "name";
            string value = "value";
            nameValueCollection.Add(name, "old-value");

            nameValueCollection.Set(name, value);
            Assert.AreEqual(value, nameValueCollection.Get(name));
            Assert.IsTrue(new string[] { value }.SequenceEqual(nameValueCollection.GetValues(name)));
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestSetNullName()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestSetNullName(0, type);
                TestSetNullName(5, type);
            }
        }

        private void TestSetNullName(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count, 0, type);

            string nullNameValue = "value";
            nameValueCollection.Set(null, nullNameValue);
            Assert.AreEqual(count + 1, nameValueCollection.Count());
            Assert.AreEqual(nullNameValue, nameValueCollection.Get(null));

            string newNullNameValue = "newvalue";
            nameValueCollection.Set(null, newNullNameValue);
            Assert.AreEqual(count + 1, nameValueCollection.Count());
            Assert.AreEqual(newNullNameValue, nameValueCollection.Get(null));
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestSetNullValue()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestSetNullValue(0, type);
                TestSetNullValue(5, type);
            }
        }

        private void TestSetNullValue(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count);

            string nullValueName = "name";
            nameValueCollection.Set(nullValueName, null);
            Assert.AreEqual(count + 1, nameValueCollection.Count());
            Assert.IsNull(nameValueCollection.Get(nullValueName));

            nameValueCollection.Set(nullValueName, "abc");
            Assert.AreEqual(count + 1, nameValueCollection.Count());
            Assert.AreEqual("abc", nameValueCollection.Get(nullValueName));

            nameValueCollection.Set(nullValueName, null);
            Assert.AreEqual(count + 1, nameValueCollection.Count());
            Assert.IsNull(nameValueCollection.Get(nullValueName));
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestSetIsCaseSensitive()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestSetIsCaseSensitive(type);
            }
        }

        private void TestSetIsCaseSensitive(NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(0, 0, type, false);
            nameValueCollection.Set("name", "value1");
            nameValueCollection.Set("Name", "value2");
            nameValueCollection.Set("NAME", "value3");
            Assert.AreEqual(1, nameValueCollection.Count());
            Assert.AreEqual("value3", nameValueCollection.Get("name"));
        }

        #endregion

        #region Get enumerator tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestGetEnumerator()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestGetEnumerator(0, type);
                TestGetEnumerator(10, type);
            }
        }

        private void TestGetEnumerator(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count, 0, type);
            Assert.AreNotEqual(nameValueCollection.GetEnumerator(), nameValueCollection.GetEnumerator());

            IEnumerator enumerator = nameValueCollection.GetEnumerator();
            string[] keys = nameValueCollection.Keys().ToArray();

            for (int i = 0; i < 2; i++)
            {
                int counter = 0;
                while (enumerator.MoveNext())
                {
                    Assert.AreEqual(keys[counter], enumerator.Current);
                    counter++;
                }
                Assert.AreEqual(count, nameValueCollection.Count());
                enumerator = nameValueCollection.GetEnumerator();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestGetEnumeratorInvalid()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestGetEnumeratorInvalid(0, type);
                TestGetEnumeratorInvalid(10, type);
            }
        }

        private void TestGetEnumeratorInvalid(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count, 0, type);
            IEnumerator enumerator = nameValueCollection.GetEnumerator();

            // Has not started enumerating
            AssertThrow(typeof(InvalidOperationException), () => { var tmp = enumerator.Current; });

            // Has finished enumerating
            while (enumerator.MoveNext()) ;
            AssertThrow(typeof(InvalidOperationException), () => { var tmp = enumerator.Current; });

            // Modify collection
            enumerator.MoveNext();
            nameValueCollection.Add("new-name", "new-value");
            AssertThrow(typeof(InvalidOperationException), () => enumerator.MoveNext());
            AssertThrow(typeof(InvalidOperationException), () => enumerator.Reset());
            if (count > 0)
            {
                Assert.IsNotNull(enumerator.Current);
            }

            // Clear collection
            enumerator = nameValueCollection.GetEnumerator();
            enumerator.MoveNext();
            nameValueCollection.Clear();
            AssertThrow(typeof(InvalidOperationException), () => { var tmp = enumerator.Current; });
            AssertThrow(typeof(InvalidOperationException), () => enumerator.MoveNext());
            AssertThrow(typeof(InvalidOperationException), () => enumerator.Reset());
        }

        #endregion

        #region Keys tests

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestKeysPreservesInstance()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestKeysPreservesInstance(0, type);
                TestKeysPreservesInstance(10, type);
            }
        }

        private void TestKeysPreservesInstance(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count);
            Assert.IsTrue(nameValueCollection.Keys().SequenceEqual(nameValueCollection.Keys()));
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestKeysGetEnumerator()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestKeysGetEnumerator(0, type);
                TestKeysGetEnumerator(10, type);
            }
        }

        private void TestKeysGetEnumerator(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count, 0,  type);

            var keys = nameValueCollection.Keys();
            Assert.AreNotEqual(keys.GetEnumerator(), keys.GetEnumerator());
            var keysArray = keys.ToArray();

            IEnumerator enumerator = keys.GetEnumerator();
            for (int i = 0; i < 2; i++)
            {
                int counter = 0;
                while (enumerator.MoveNext())
                {
                    Assert.AreEqual(keysArray[counter], enumerator.Current);
                    counter++;
                }
                Assert.AreEqual(count, keys.Count());
                enumerator = keys.GetEnumerator();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestKeysGetEnumeratorInvalid()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestKeysGetEnumeratorInvalid(0, type);
                TestKeysGetEnumeratorInvalid(10, type);
            }
        }

        private void TestKeysGetEnumeratorInvalid(int count, NameValueCollectionType type)
        {
            INameValueCollection nameValueCollection = CreateNameValueCollection(count, 0, type);
            var keys = nameValueCollection.Keys();
            IEnumerator enumerator = keys.GetEnumerator();

            // Has not started enumerating
            AssertThrow(typeof(InvalidOperationException), () => { var tmp = enumerator.Current; });

            // Has finished enumerating
            while (enumerator.MoveNext()) ;
            AssertThrow(typeof(InvalidOperationException), () => { var tmp = enumerator.Current; });

            // Has reset enumerating
            enumerator = keys.GetEnumerator();
            AssertThrow(typeof(InvalidOperationException), () => { var tmp = enumerator.Current; });

            // Modify collection
            enumerator.MoveNext();
            nameValueCollection.Add("new-name", "new-value");
            AssertThrow(typeof(InvalidOperationException), () => { var tmp = enumerator.MoveNext(); });
            AssertThrow(typeof(InvalidOperationException), () => enumerator.Reset());
            if (count > 0)
            {
                Assert.IsNotNull(enumerator.Current);
            }

            // Clear collection
            enumerator = keys.GetEnumerator();
            enumerator.MoveNext();
            nameValueCollection.Clear();
            AssertThrow(typeof(InvalidOperationException), () => { var tmp = enumerator.Current; });
            AssertThrow(typeof(InvalidOperationException), () => enumerator.MoveNext());
            AssertThrow(typeof(InvalidOperationException), () => enumerator.Reset());
        }

        #endregion

        #region Test Clone()

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestClone()
        {
            foreach (NameValueCollectionType type in NameValueCollectionTypes)
            {
                TestClone(type);
            }
        }

        private void TestCloneCheckType(INameValueCollection c1, INameValueCollection c2)
        {
            var type1 = c1.GetType();
            var type2 = c2.GetType();
            Assert.AreEqual(type1, type2);
            if (type1 == typeof(StringKeyValueCollection))
            {
                TestCloneCheckType(((StringKeyValueCollection)c1).internalCollection, ((StringKeyValueCollection)c2).internalCollection);
            }
        }

        private void TestClone(NameValueCollectionType type)
        {
            INameValueCollection c = CreateNameValueCollection(10, 0, type);
            INameValueCollection cloneC = c.Clone();
            TestCloneCheckType(c, cloneC);
            switch (type)
            {
                case NameValueCollectionType.StringKeyValueCollection:

                    break;
            }
            foreach (string key in c)
            {
                var cValues = c.GetValues(key);
                var cloneValues = cloneC.GetValues(key);
                Assert.IsTrue(cValues.SequenceEqual(cloneValues));
            }
        }

        #endregion

        #region Test update through ResponseHeaders property

        /// <summary>
        /// With the replacement of NameValueCollection, the response headers in exception types and response types are stored with the new 
        /// INameValueCollection type. A new Headers property is introduced to return this internal key-value mapping. The existing 
        /// ResponseHeaders was updated to return a conversion of this internal collection, therefore, we need to make sure headers updated 
        /// through this existing API are persisted within the ResponseHeaders APIs.
        /// </summary>
        [TestMethod]
        public void TestExceptionHeaders()
        {
            DocumentClientException e = new DocumentClientException(String.Empty, System.Net.HttpStatusCode.BadRequest, SubStatusCodes.Unknown);
            // Updating exception headers through response headers should persist 
            string headerKey = "x-header-1";
            string headerValue = "-1";

            e.ResponseHeaders[headerKey] = headerValue;
            Assert.AreEqual(e.ResponseHeaders[headerKey], headerValue);
        }

        /// <summary>
        /// <see cref="TestExceptionHeaders"/>
        /// </summary>
        [TestMethod]
        public void TestResponseHeaders()
        {
            DocumentServiceResponse response = new DocumentServiceResponse(null, new StringKeyValueCollection(), System.Net.HttpStatusCode.Accepted);
            // Updating reponse headers through response headers should persist
            string headerKey = "x-header-1";
            string headerValue = "-1";

            response.ResponseHeaders[headerKey] = headerValue;
            Assert.AreEqual(response.ResponseHeaders[headerKey], headerValue);
        }

        #endregion
    }
}
