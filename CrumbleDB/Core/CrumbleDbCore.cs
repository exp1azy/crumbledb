using CrumbleDB.Entities;

namespace CrumbleDB.Core;

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

        return Directory
            .GetFiles(_path, "*.json")
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
    /// Retrieves information about the collection file associated with the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The entity type whose collection information should be retrieved.</typeparam>
    /// <returns>A <see cref="CollectionInfo"/> object containing information about the collection file.</returns>
    public CollectionInfo GetCollectionInfo<T>() where T : CrumbleEntity
    {
        var fullPath = GetFullPath<T>();
        var fullInfo = new FileInfo(fullPath);

        var result = new CollectionInfo
        {
            CreationTime = fullInfo.CreationTime,
            DirectoryName = fullInfo.DirectoryName,
            Exists = fullInfo.Exists,
            Extension = fullInfo.Extension,
            FullName = fullInfo.FullName,
            IsReadOnly = fullInfo.IsReadOnly,
            LastAccessTime = fullInfo.LastAccessTime,
            LastWriteTime = fullInfo.LastWriteTime,
            LinkTarget = fullInfo.LinkTarget,
            Name = fullInfo.Name
        };

        try
        {
            result.Length = fullInfo.Length;
        }
        catch
        {
            result.Length = -1;
        }

        return result;
    }

    /// <summary>
    /// Creates a timestamped copy (backup) of the collection file for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The entity type whose collection file should be backed up.</typeparam>
    public void CreateBackup<T>() where T : CrumbleEntity
    {
        var typeName = typeof(T).Name.ToLowerInvariant();
        var fullPath = Path.Combine(_path, $"{typeName}_{DateTime.UtcNow.Ticks}.json");

        File.Copy(GetFullPath<T>(), fullPath);
    }

    /// <summary>
    /// Restores the collection file for the specified type <typeparamref name="T"/> from a backup file.
    /// </summary>
    /// <typeparam name="T">The entity type whose collection file should be restored.</typeparam>
    /// <param name="backupPath">The path to the backup file.</param>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task<CrumbleCollection<T>> RestoreBackupAsync<T>(string backupPath) where T : CrumbleEntity
    {
        if (!File.Exists(backupPath))
            throw new FileNotFoundException($"The specified backup file ({backupPath}) does not exist.");

        string fullPath = GetFullPath<T>();
        File.Copy(backupPath, fullPath, overwrite: true);

        return await CrumbleCollection<T>.CreateAsync(fullPath);
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

    private string GetFullPath<T>() where T : CrumbleEntity
    {
        var typeName = typeof(T).Name.ToLowerInvariant();
        return Path.Combine(_path, $"{typeName}.json");
    }
}
