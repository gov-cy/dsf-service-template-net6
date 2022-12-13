using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;

namespace Dsf.Service.Template.Extensions
{
   
    public static class SessionExtensions
    {
      

        public static void SetObjectAsJson(this ISession session, string key, object value)
        {   
           session.SetString(key, JsonConvert.SerializeObject(value));
        }
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            string value = session.GetString(key)!;
            //var value = Encryption.Decrypt(session.GetString(key), encKey, true);
            return value != null ? JsonConvert.DeserializeObject<T>(value) : default;
        }
    }
}
