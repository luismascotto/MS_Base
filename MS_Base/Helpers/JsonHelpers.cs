using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MS_Base.Helpers;

public static class JsonHelpers
{
    public static async Task<T> CreateFromJsonStream<T>(this Stream stream)
    {
        using StreamReader streamReader = new(stream);
        T data = (T)JsonSerializer.Deserialize(System.Text.Encoding.UTF8.GetBytes(await streamReader.ReadToEndAsync()), typeof(T));
        return data;
    }

    public static async Task<T> CreateFromJsonString<T>(this String json)
    {
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(json));
        T data = await CreateFromJsonStream<T>(stream);
        return data;
    }

    public static async Task<T> CreateFromJsonFile<T>(this String fileName)
    {
        await using FileStream fileStream = new(fileName, FileMode.Open);
        T data = await CreateFromJsonStream<T>(fileStream);
        return data;
    }
}