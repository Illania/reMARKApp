//
// Project: Mark5.Mobile.Common
// File: SerializationUtils.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mark5.Mobile.Common.Utilities
{

    public static class SerializationUtils
    {

        static readonly JsonSerializer jsonSerializer;

        static SerializationUtils()
        {
            var jsSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                Converters =
                {
                    new VersionConverter()
                }
            };
            jsonSerializer = JsonSerializer.Create(jsSettings);
        }

        #region string

        public static string Serialize<T>(T obj)
        {
            using (var w = new StringWriter())
            {
                jsonSerializer.Serialize(w, obj, typeof(T));
                return w.ToString();
            }
        }

        public static Task<string> SerializeAsync<T>(T obj)
        {
            return Task.Run(() =>
            {
                return Serialize(obj);
            });
        }

        public static T Deserialize<T>(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return default(T);
            }

            using (var sr = new StringReader(str))
            using (var jr = new JsonTextReader(sr))
            {
                return jsonSerializer.Deserialize<T>(jr);
            }
        }

        public static Task<T> DeserializeAsync<T>(string str)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return default(T);
                }

                return Deserialize<T>(str);
            });
        }

        public static object Deserialize(string str, Type type)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            using (var sr = new StringReader(str))
            using (var jr = new JsonTextReader(sr))
            {
                return jsonSerializer.Deserialize(jr, type);
            }
        }

        public static Task<object> DeserializeAsync(string str, Type type)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return null;
                }

                return Deserialize(str, type);
            });
        }

        #endregion

        #region byte[]

        public static byte[] SerializeToByteArray<T>(T obj) where T : class
        {
            if (obj == null)
            {
                return null;
            }

            return GetBytes(Serialize(obj));
        }

        public static T DeserializeFromByteArray<T>(byte[] bytes) where T : class
        {
            if (bytes == null || bytes.Length < 1)
            {
                return null;
            }

            return Deserialize<T>(GetString(bytes));
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        #endregion

    }
}

