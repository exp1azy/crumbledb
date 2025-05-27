namespace CrumbleDB;

/// <summary>
/// Core interface to the Crumble database system, which provides access to collections
/// of entities stored as JSON files on disk.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CrumbleDbCore"/> class. 
/// Use <see cref="CrumbleDb.Open(string)"/> to create an instance of <see cref="CrumbleDbCore"/> safely.
/// </remarks>
/// <param name="path">The root directory where all collection JSON files are stored.</param>
public sealed class CrumbleDbCore(string path)
{
    private readonly string _path = path;

    /// <summary>
    /// Returns the names of all existing collections (JSON files) in the database directory.
    /// </summary>
    /// <returns>An array of collection names without the .json extension.</returns>
    public string[] GetCollectionNames()
    {
        if (!Directory.Exists(_path))
            return [];

        return Directory.GetFiles(_path, "*.json")
                        .Select(x => Path.GetFileNameWithoutExtension(x))
                        .ToArray();
    }

    /// <summary>
    /// Gets the full file path for the collection associated with the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the entity, which must inherit from <see cref="CrumbleEntity"/>.</typeparam>
    /// <returns>The full file path for the collection.</returns>
    public string GetPathOf<T>() where T : CrumbleEntity
    {
        return GetFullPath<T>();
    }

    /// <summary>
    /// Asynchronously retrieves or creates the collection file for the specified type <typeparamref name="T"/> 
    /// and loads it into memory.
    /// </summary>
    /// <typeparam name="T">The entity type, which must inherit from <see cref="CrumbleEntity"/>.</typeparam>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The loaded <see cref="CrumbleCollection{T}"/>.</returns>
    public async Task<CrumbleCollection<T>> GetCollectionAsync<T>(CancellationToken cancellationToken = default) where T : CrumbleEntity
    {
        var fullPath = GetFullPath<T>();

        if (!File.Exists(fullPath))
        {
            await using var _ = File.Create(fullPath);
        }

        return await CrumbleCollection<T>.CreateAsync(fullPath, cancellationToken);
    }

    /// <summary>
    /// Deletes the collection file associated with the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The entity type whose collection should be deleted.</typeparam>
    /// <returns><c>true</c> if the file existed and was deleted; otherwise, <c>false</c>.</returns>
    public bool DropCollection<T>() where T : CrumbleEntity
    {
        var fullPath = GetFullPath<T>();

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Asynchronously clears the contents of the collection file for the specified type <typeparamref name="T"/>,
    /// but does not delete the file.
    /// </summary>
    /// <typeparam name="T">The entity type whose collection should be cleared.</typeparam>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if the file existed and was cleared; otherwise, <c>false</c>.</returns>
    public async Task<bool> PurgeCollectionAsync<T>(CancellationToken cancellationToken = default) where T : CrumbleEntity
    {
        var fullPath = GetFullPath<T>();

        if (File.Exists(fullPath))
        {
            await File.WriteAllTextAsync(fullPath, string.Empty, cancellationToken);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Asynchronously clears all JSON collection files in the database directory.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task PurgeCollectionsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var file in Directory.GetFiles(_path, "*.json"))
            await File.WriteAllTextAsync(file, string.Empty, cancellationToken);
    }

    /// <summary>
    /// Creates a timestamped copy of the collection file for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The entity type whose collection file should be copied.</typeparam>
    /// <returns><c>true</c> if the file existed and was copied; otherwise, <c>false</c>.</returns>
    public bool Copy<T>() where T : CrumbleEntity
    {
        var fullPath = GetFullPath<T>();

        if (File.Exists(fullPath))
        {
            var fileName = Path.GetFileNameWithoutExtension(fullPath);
            var copyName = $"{fileName}_{DateTime.UtcNow.Ticks}.json";
            var copyPath = Path.Combine(_path, copyName);

            File.Copy(fullPath, copyPath);

            return true;
        }

        return false;
    }

    private string GetFullPath<T>() where T : CrumbleEntity
    {
        var typeName = typeof(T).Name.ToLowerInvariant();
        return Path.Combine(_path, $"{typeName}.json");
    }
}
