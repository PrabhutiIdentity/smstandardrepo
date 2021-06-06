
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMEnterprise.Authentication
{
   public class MyContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            foreach (var property in properties)
            {
                property.PropertyName = char.ToUpper(property.PropertyName[0]) + string.Join("", property.PropertyName.Skip(1));
            }

            return properties;
        }
    }
}
