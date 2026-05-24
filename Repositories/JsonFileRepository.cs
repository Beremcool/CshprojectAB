using System.Text.Json;

namespace TaskHub.Repositories;

public sealed class JsonFileRepository<T> : IRepository<T>
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public JsonFileRepository(string filePath)
    {
        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public async Task SaveAsync(List<T> items)
    {
        ThrowIfDisposed();

        string fullPath = Path.GetFullPath(_filePath);
        string? directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);

        await JsonSerializer.SerializeAsync(stream, items, _jsonOptions);
    }

    public async Task<List<T>> LoadAsync()
    {
        ThrowIfDisposed();

        string fullPath = Path.GetFullPath(_filePath);

        if (!File.Exists(fullPath))
        {
            return new List<T>();
        }

        await using var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        List<T>? items = await JsonSerializer.DeserializeAsync<List<T>>(stream, _jsonOptions);
        return items ?? new List<T>();
    }

    public void Dispose()
    {
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(JsonFileRepository<T>));
        }
    }
}
