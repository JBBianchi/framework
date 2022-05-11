using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using Xunit;

namespace Neuroglia.UnitTests.Cases.Serialization
{

    class DynamicObjectWrapper
    {

        [Newtonsoft.Json.JsonConverter(typeof(DynamicValueConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.DynamicValueConverter))]
        public object SingleValue { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(DynamicValueConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.DynamicValueConverter))]
        public object Array { get; set; }

    }

    public class DynamicValueSerializationTests
    {

        DynamicObjectWrapper GetTestData()
        {
            return new DynamicObjectWrapper() 
            {
                SingleValue = new
                {
                    FirstName = "FakeFirstName",
                    LastName = "FakeLastName",
                    DateOfBirth = DateTime.Today,
                    Address = new
                    {
                        Street = "FakeStreet",
                        ZipCode = "FakeZipCode",
                        City = "FakeCity",
                        Country = "FakeCountry"
                    }.ToExpandoObject()
                }.ToExpandoObject(),
                Array = new object[]
                {
                    new
                    {
                        FirstName = "FakeFirstName",
                        LastName = "FakeLastName",
                        DateOfBirth = DateTime.Today,
                        Address = new
                        {
                            Street = "FakeStreet",
                            ZipCode = "FakeZipCode",
                            City = "FakeCity",
                            Country = "FakeCountry"
                        }.ToExpandoObject()
                    }.ToExpandoObject(),
                    new
                    {
                        FirstName = "FakeFirstName",
                        LastName = "FakeLastName",
                        DateOfBirth = DateTime.Today,
                        Address = new
                        {
                            Street = "FakeStreet",
                            ZipCode = "FakeZipCode",
                            City = "FakeCity",
                            Country = "FakeCountry"
                        }.ToExpandoObject()
                    }.ToExpandoObject()
                }
            };
        }

        [Fact]
        public void SerializeAndDeserialize_ToFrom_Newtonsof_ShouldWork()
        {
            //arrange
            var serialized = GetTestData();

            //act
            var json = JsonConvert.SerializeObject(serialized);
            var deserialized = JsonConvert.DeserializeObject<DynamicObjectWrapper>(json);

            //assert
            AssertDynamicObjectMatches(serialized, deserialized);
        }

        [Fact]
        public void SerializeAndDeserialize_ToFrom_SystemTextJson_ShouldWork()
        {
            //arrange
            var serialized = GetTestData();

            //act
            var json = System.Text.Json.JsonSerializer.Serialize(serialized);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<DynamicObjectWrapper>(json);

            //assert
            AssertDynamicObjectMatches(serialized, deserialized);
        }

        [Fact]
        public void SerializeAndDeserialize_ToFrom_Protobuf_ShouldWork()
        {

        }

        void AssertDynamicObjectMatches(DynamicObjectWrapper obj1, DynamicObjectWrapper obj2)
        {
            var dyn1 = (dynamic)obj1.SingleValue;
            var dyn2 = (dynamic)obj2.SingleValue;
            Assert.Equal(dyn1.FirstName, dyn2.FirstName);
            Assert.Equal(dyn1.LastName, dyn2.LastName);
            Assert.Equal(dyn1.DateOfBirth, dyn2.DateOfBirth);
            Assert.Equal(dyn1.Address, dyn2.Address);
            var array1 = ((IEnumerable)obj1.Array).OfType<object>().ToArray();
            var array2 = ((IEnumerable)obj2.Array).OfType<object>().ToArray();
            for (int i = 0, count = array1.Count(); i < count; i++)
            {
                var elem1 = (dynamic)array1[i];
                var elem2 = (dynamic)array2[i];
                Assert.Equal(elem1.FirstName, elem2.FirstName);
                Assert.Equal(elem1.LastName, elem2.LastName);
                Assert.Equal(elem1.DateOfBirth, elem2.DateOfBirth);
                Assert.Equal(elem1.Address, elem2.Address);
            }
        }

    }

}
