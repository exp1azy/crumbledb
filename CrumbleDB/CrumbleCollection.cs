using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrumbleDB
{
    /// <summary>
    /// Represents a persistent, in-memory collection of <typeparamref name="T"/> entities 
    /// backed by a JSON file on disk.
    /// </summary>
    /// <typeparam name="T">The entity type. Must inherit from <see cref="CrumbleEntity"/>.</typeparam>
    public sealed class CrumbleCollection<T>(string path, List<T> data) where T : CrumbleEntity
    {
        private readonly string _path = path;
        private readonly List<T> _data = data;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Gets the collection items as a read-only list.
        /// </summary>
        public IReadOnlyList<T> Values => _data;

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count => _data.Count;

        /// <summary>
        /// Indicates whether the collection is empty.
        /// </summary>
        public bool IsEmpty => _data.Count == 0;

        /// <summary>
        /// Asynchronously loads a <see cref="CrumbleCollection{T}"/> from the specified file path.
        /// If the file does not exist or is empty, an empty collection is returned.
        /// </summary>
        /// <remarks>
        /// Call <see cref="CrumbleDbCore.GetCollectionAsync{T}(CancellationToken)"/> to create a <see cref="CrumbleCollection{T}"/> instance safely.
        /// </remarks>
        /// <param name="path">The file path to load from.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the loaded collection.</returns>
        public static async Task<CrumbleCollection<T>> CreateAsync(string path, CancellationToken cancellationToken = default)
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

            var data = fs.Length == 0 ? [] : await JsonSerializer.DeserializeAsync<List<T>>(fs, cancellationToken: cancellationToken);

            return new CrumbleCollection<T>(path, data!);
        }

        /// <summary>
        /// Adds a new item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            _data.Add(item);
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
        /// Replaces the entire collection with the specified items.
        /// </summary>
        /// <param name="items">The items to replace the existing collection.</param>
        public void Rewrite(IEnumerable<T> items)
        {
            _data.Clear();
            _data.AddRange(items);
        }

        /// <summary>
        /// Removes all items from the collection that do not match the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to use for filtering.</param>
        public void Filter(Predicate<T> predicate)
        {
            _data.RemoveAll(x => !predicate(x));
        }

        /// <summary>
        /// Updates an existing item in the collection by its <see cref="CrumbleEntity.Id"/>.
        /// </summary>
        /// <param name="id">The identifier of the item to update.</param>
        /// <param name="newItem">The new item to replace the existing one.</param>
        /// <returns><c>true</c> if the item was found and updated; otherwise <c>false</c>.</returns>
        public bool UpdateById(Guid id, T newItem)
        {
            var index = _data.FindIndex(e => e.Id == id);
            if (index == -1) return false;

            _data[index] = newItem;
            return true;
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
        /// Removes all items that match the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate used to determine which items to remove.</param>
        public void RemoveAll(Predicate<T> predicate)
        {
            _data.RemoveAll(predicate);
        }

        /// <summary>
        /// Removes all items from the collection (in-memory only).
        /// </summary>
        public void Clear()
        {
            _data.Clear();
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
        /// Clears the collection and immediately writes the empty state to the file.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ClearForcedAsync(CancellationToken cancellationToken = default)
        {
            _data.Clear();
            await WriteAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes the collection to disk using the specified file path.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task WriteAsync(CancellationToken cancellationToken = default)
        {
            var fileInfo = new FileInfo(_path);
            int bufferSize = GetBufferSize(fileInfo.Exists ? fileInfo.Length : 0);

            await using var fs = new FileStream(
                _path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                useAsync: true
            );

            await JsonSerializer.SerializeAsync(fs, _data, _serializerOptions, cancellationToken);
        }

        /// <summary>
        /// Converts the collection to a dictionary where the keys are the <see cref="CrumbleEntity.Id"/> values.
        /// </summary>
        /// <returns>A dictionary where the keys are the <see cref="CrumbleEntity.Id"/> values.</returns>
        public Dictionary<Guid, T> ToDictionary()
        {
            return _data.ToDictionary(x => x.Id, x => x);
        }

        private static int GetBufferSize(long fileSize)
        {
            return fileSize switch
            {
                <= 64 * 1024 => 4 * 1024,      
                <= 1 * 1024 * 1024 => 8 * 1024,
                <= 16 * 1024 * 1024 => 16 * 1024,
                <= 128 * 1024 * 1024 => 32 * 1024,
                _ => 64 * 1024
            };
        }
    }
}
