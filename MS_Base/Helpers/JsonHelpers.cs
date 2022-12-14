using System;
using Newtonsoft.Json;
using System.IO;

namespace MS_Base.Helpers
{
    public static class JsonHelpers
    {
        public static T CreateFromJsonStream<T>(this Stream stream) {
            JsonSerializer serializer = new();
            T data;
            using (StreamReader streamReader = new(stream)) {
                data = (T)serializer.Deserialize(streamReader, typeof(T));
            }
            return data;
        }

        public static T CreateFromJsonString<T>(this String json) {
            T data;
            using (MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(json))) {
                data = CreateFromJsonStream<T>(stream);
            }
            return data;
        }

        public static T CreateFromJsonFile<T>(this String fileName) {
            T data;
            using (FileStream fileStream = new(fileName, FileMode.Open)) {
                data = CreateFromJsonStream<T>(fileStream);
            }
            return data;
        }
    }
}
