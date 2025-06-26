using CrumbleDB.Entities;

namespace CrumbleDB;

/// <summary>
/// Represents a persistent, in-memory collection of <typeparamref name="T"/> entities
/// backed by a JSON file on disk.
/// </summary>
/// <typeparam name="T">The entity type. Must inherit from <see cref="CrumbleEntity"/>.</typeparam>
public class CrumbleCollection<T>(string path, List<T> data) where T : CrumbleEntity
{
    private readonly List<T> _data = data;
    private readonly string _path = path;

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Indicates whether the collection is empty.
    /// </summary>
    public bool IsEmpty => _data.Count == 0;

    /// <summary>
    /// Gets the collection items as a read-only list.
    /// </summary>
    public IReadOnlyList<T> Values => _data.AsReadOnly();

    /// <summary>
    /// Gets the total size of the collection in bytes.
    /// </summary>
    public long TotalSizeBytes => new FileInfo(_path).Length;

    /// <summary>
    /// Adds a new item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        _data.Add(item);
    }

    /// <summary>
    /// Adds a new item to the collection and immediately writes it to file.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task AddForcedAsync(T item, CancellationToken cancellationToken = default)
    {
        Add(item);
        await WriteAsync(cancellationToken);
    }

    /// <summary>
    /// Adds multiple items to the collection.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<T> items)
    {
        _data.AddRange(items);
    }

    /// <summary>
    /// Adds multiple items to the collection and immediately writes them to file.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task AddRangeForcedAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
    {
        AddRange(items);
        await WriteAsync(cancellationToken);
    }

    /// <summary>
    /// Removes all items from the collection (in-memory only).
    /// </summary>
    public void Clear()
    {
        _data.Clear();
    }

    /// <summary>
    /// Clears the collection and immediately writes the empty state to the file.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClearForcedAsync(CancellationToken cancellationToken = default)
    {
        _data.Clear();
        await WriteAsync(cancellationToken);
    }

    /// <summary>
    /// Executes a specified action on the collection and writes the result to file.
    /// </summary>
    /// <remarks>
    /// If an error occurs, the changes are rolled back.
    /// </remarks>
    /// <param name="action">The action to apply to the collection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the transaction was completed successfully; otherwise <c>false</c>.</returns>
    public async Task<bool> ExecuteTransactionAsync(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        var backup = new T[_data.Count];
        _data.CopyTo(backup);

        try
        {
            action();
            await WriteAsync(cancellationToken);
            return true;
        }
        catch
        {
            _data.Clear();
            _data.AddRange(backup);
            return false;
        }
    }

    /// <summary>
    /// Executes a specified action on each element in the collection.
    /// </summary>
    /// <param name="action">The action to apply to each element.</param>
    public void ForEach(Action<T> action)
    {
        _data.ForEach(action);
    }

    /// <summary>
    /// Executes a specified action on each element in the collection and immediately writes the collection to file.
    /// </summary>
    /// <param name="action">The action to apply to each element.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ForEachForcedAsync(Action<T> action, CancellationToken cancellationToken = default)
    {
        ForEach(action);
        await WriteAsync(cancellationToken);
    }

    /// <summary>
    /// Removes an item from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><c>true</c> if the item was successfully removed; otherwise <c>false</c>.</returns>
    public bool Remove(T item)
    {
        return _data.Remove(item);
    }

    /// <summary>
    /// Removes all items that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate used to determine which items to remove.</param>
    public void RemoveAll(Predicate<T> predicate)
    {
        _data.RemoveAll(predicate);
    }

    /// <summary>
    /// Removes all items that match the specified predicate and immediately writes the collection to file.
    /// </summary>
    /// <param name="predicate">The predicate used to determine which items to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RemoveAllForcedAsync(Predicate<T> predicate, CancellationToken cancellationToken = default)
    {
        RemoveAll(predicate);
        await WriteAsync(cancellationToken);
    }

    /// <summary>
    /// Removes an item by its <see cref="CrumbleEntity.Id"/>.
    /// </summary>
    /// <param name="id">The ID of the item to remove.</param>
    /// <returns><c>true</c> if the item was found and removed; otherwise <c>false</c>.</returns>
    public bool RemoveById(Guid id)
    {
        var item = _data.FirstOrDefault(e => e.Id == id);
        return item != null && _data.Remove(item);
    }

    /// <summary>
    /// Removes an item by its <see cref="CrumbleEntity.Id"/> and immediately writes the collection to file.
    /// </summary>
    /// <param name="id">The ID of the item to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the item was found and removed; otherwise <c>false</c>.</returns>
    public async Task<bool> RemoveByIdForcedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = RemoveById(id);

        if (result)
            await WriteAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Removes an item from the collection and immediately writes it to file.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the item was successfully removed; otherwise <c>false</c>.</returns>
    public async Task<bool> RemoveForcedAsync(T item, CancellationToken cancellationToken = default)
    {
        var result = Remove(item);

        if (result)
            await WriteAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Replaces the entire collection with the specified items.
    /// </summary>
    /// <param name="items">The items to replace the existing collection.</param>
    public void Rewrite(IEnumerable<T> items)
    {
        _data.Clear();
        _data.AddRange(items);
    }

    /// <summary>
    /// Replaces the entire collection with the specified items and immediately writes them to file.
    /// </summary>
    /// <param name="items">The items to replace the existing collection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RewriteForcedAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
    {
        Rewrite(items);
        await WriteAsync(cancellationToken);
    }

    /// <summary>
    /// Converts the collection to a dictionary where the keys are the <see cref="CrumbleEntity.Id"/> values.
    /// </summary>
    /// <returns>A dictionary where the keys are the <see cref="CrumbleEntity.Id"/> values.</returns>
    public Dictionary<Guid, T> ToDictionary()
    {
        return _data.ToDictionary(x => x.Id, x => x);
    }

    /// <summary>
    /// Updates an existing item in the collection by its <see cref="CrumbleEntity.Id"/>.
    /// </summary>
    /// <param name="id">The identifier of the item to update.</param>
    /// <param name="patch">The patch function to apply to the item.</param>
    /// <returns><c>true</c> if the item was found and updated; otherwise <c>false</c>.</returns>
    public bool UpdateById(Guid id, Action<T> patch)
    {
        var item = _data.FirstOrDefault(e => e.Id == id);
        if (item == null) return false;

        patch(item);
        return true;
    }

    /// <summary>
    /// Updates an existing item in the collection by its <see cref="CrumbleEntity.Id"/> and immediately writes it to file.
    /// </summary>
    /// <param name="id">The identifier of the item to update.</param>
    /// <param name="patch">The patch function to apply to the item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the item was found and updated; otherwise <c>false</c>.</returns>
    public async Task<bool> UpdateByIdForcedAsync(Guid id, Action<T> patch, CancellationToken cancellationToken = default)
    {
        var result = UpdateById(id, patch);

        if (result)
            await WriteAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Updates all items in the collection that match the specified predicate using the specified patch function.
    /// </summary>
    /// <param name="predicate">The predicate to match items to update.</param>
    /// <param name="patch">The patch function to apply to the matched items.</param>
    /// <returns>Number of items that were updated.</returns>
    public int UpdateAll(Predicate<T> predicate, Action<T> patch)
    {
        var matches = _data.Where(x => predicate(x)).ToList();
        matches.ForEach(patch);
        return matches.Count;
    }

    /// <summary>
    /// Updates all items in the collection that match the specified predicate using the specified patch function and immediately writes the collection to file.
    /// </summary>
    /// <param name="predicate">The predicate to match items to update.</param>
    /// <param name="patch">The patch function to apply to the matched items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of items that were updated.</returns>
    public async Task<int> UpdateAllForcedAsync(Predicate<T> predicate, Action<T> patch, CancellationToken cancellationToken = default)
    {
        var matches = _data.Where(x => predicate(x)).ToList();
        matches.ForEach(patch);
        await WriteAsync(cancellationToken);

        return matches.Count;
    }

    /// <summary>
    /// Asynchronously writes the collection to disk using the specified file path.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task WriteAsync(CancellationToken cancellationToken = default)
    {
        await using var ms = new MemoryStream();
        await SpanJson.JsonSerializer.Generic.Utf8.SerializeAsync(_data, ms, cancellationToken);
        int bufferSize = GetBufferSize(ms.Length);

        await using var fs = new FileStream(
            _path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize,
            useAsync: true
        );

        ms.Position = 0;
        await ms.CopyToAsync(fs, bufferSize, cancellationToken);
    }

    internal static async Task<CrumbleCollection<T>> CreateAsync(string path, CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(path);
        int bufferSize = GetBufferSize(fileInfo.Length);

        await using var fs = new FileStream(
            path,
            FileMode.OpenOrCreate,
            FileAccess.Read,
            FileShare.Read,
            bufferSize,
            useAsync: true
        );

        var data = fs.Length == 0
            ? []
            : await SpanJson.JsonSerializer.Generic.Utf8.DeserializeAsync<List<T>>(fs, cancellationToken);

        return new CrumbleCollection<T>(path, data!);
    }

    private static int GetBufferSize(long dataSize)
    {
        return dataSize switch
        {
            0 => 16 * 1024,
            <= 64 * 1024 => 4 * 1024,
            <= 1 * 1024 * 1024 => 8 * 1024,
            <= 16 * 1024 * 1024 => 16 * 1024,
            <= 128 * 1024 * 1024 => 32 * 1024,
            <= 1024 * 1024 * 1024 => 64 * 1024,
            _ => 128 * 1024
        };
    }
}