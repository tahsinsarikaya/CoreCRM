using CoreCRM.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CoreCRM.Tests.Serialization
{
    public class JsonValueConverterTests
    {
        private static JsonElement ToJsonElement(string json) =>
            JsonSerializer.Deserialize<JsonElement>(json);

        [Fact]
        public void ToClrValue_WithStringElement_ReturnsString()
        {
            var element = ToJsonElement("\"Ahmet\"");
            Assert.Equal("Ahmet", JsonValueConverter.ToClrValue(element));
        }

        [Fact]
        public void ToClrValue_WithIntegerElement_ReturnsLong()
        {
            var element = ToJsonElement("35");
            Assert.Equal(35L, JsonValueConverter.ToClrValue(element));
        }

        [Fact]
        public void ToClrValue_WithDecimalElement_ReturnsDouble()
        {
            var element = ToJsonElement("35.5");
            Assert.Equal(35.5, JsonValueConverter.ToClrValue(element));
        }

        [Fact]
        public void ToClrValue_WithBooleanElement_ReturnsBool()
        {
            Assert.Equal(true, JsonValueConverter.ToClrValue(ToJsonElement("true")));
            Assert.Equal(false, JsonValueConverter.ToClrValue(ToJsonElement("false")));
        }

        [Fact]
        public void ToClrValue_WithNullElement_ReturnsNull()
        {
            var element = ToJsonElement("null");
            Assert.Null(JsonValueConverter.ToClrValue(element));
        }

        [Fact]
        public void ToClrValue_WithNonJsonElementValue_ReturnsValueUnchanged()
        {
            // JsonElement olmayan bir değer verilirse (örn. zaten dönüştürülmüş), olduğu gibi dönmeli.
            Assert.Equal("already-a-string", JsonValueConverter.ToClrValue("already-a-string"));
        }
    }
}
