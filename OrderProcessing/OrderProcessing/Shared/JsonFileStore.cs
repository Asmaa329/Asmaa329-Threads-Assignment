using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderProcessing.OrderProcessing.Shared
{
    public class JsonFileStore
    {
            private static readonly SemaphoreSlim _lock = new(1, 1);

            private static readonly JsonSerializerOptions _options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            private readonly string _dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");

            public JsonFileStore()
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            public async Task<T> ReadAsync<T>(string fileName, CancellationToken ct) where T : new()
            {
                string filePath = Path.Combine(_dataDirectory, fileName);

                await _lock.WaitAsync( ct);
                try
                {
                    if (!File.Exists(filePath))
                    {
                        return new T();
                    }

                    Log.Info($"Reading from {fileName}...");

                    using var stream = File.OpenRead(filePath);
                    return await JsonSerializer.DeserializeAsync<T>(stream, _options, ct) ?? new T();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error reading file {fileName}: {ex.Message}");
                    throw new Exceptions.DataFileCorruptedException(fileName, ex.Message);
                }
                finally
                {
                    _lock.Release();
                }
            }

            public async Task WriteAsync<T>(string fileName, T dataObject, CancellationToken ct)
            {
                string filePath = Path.Combine(_dataDirectory, fileName);

                await _lock.WaitAsync(ct);
                try
                {
                    Log.Info($"Writing to {fileName}...");

                        using var stream = File.Create(filePath);
                        await JsonSerializer.SerializeAsync(stream, dataObject, _options, ct);
                    
                         Log.Success($"Successfully wrote to {fileName}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Error writing to file {fileName}: {ex.Message}");
                    throw new InvalidOperationException($"Error writing to file {fileName}", ex);
                }
                finally
                {
                    _lock.Release();
                }
            }
        


    }
}