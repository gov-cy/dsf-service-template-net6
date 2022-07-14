using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dsf_service_template_net6.Helpers;


namespace dsf_service_template_net6.Extensions
{
   
    public static class SessionExtensions
    {
        public static object Encryption { get; private set; }

        public static void SetObjectAsJson(this ISession session, string key, object value, string encKey = null)
        {   
            if (!string.IsNullOrEmpty(encKey))
            {
                session.SetString(key, dsf_service_template_net6.Helpers.Encryption.Encrypt(JsonConvert.SerializeObject(value), encKey, true));
            }
            else
            {
                session.SetString(key, JsonConvert.SerializeObject(value));
            }
            
        }

        public static T GetObjectFromJson<T>(this ISession session, string key, string encKey = null)
        {
            string value = session.GetString(key);

            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(encKey))
            {
                value = dsf_service_template_net6.Helpers.Encryption.Decrypt(value.ToString(), encKey, true);
            }

            //var value = Encryption.Decrypt(session.GetString(key), encKey, true);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
