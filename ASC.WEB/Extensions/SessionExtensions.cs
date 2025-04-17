﻿using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ASC.Web.Extensions
{
    public static class SessionExtensions
    {
        public static void SetSession<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T GetSession<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
