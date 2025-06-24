namespace CrumbleDB.Entities;

/// <summary>
/// Represents information about a collection file.
/// </summary>
public class CollectionInfo
{
    /// <summary>
    /// Gets or sets the creation time of the collection file.
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the directory name of the collection file.
    /// </summary>
    public string? DirectoryName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the collection file exists.
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// Gets or sets the extension of the collection file.
    /// </summary>
    public string Extension { get; set; }

    /// <summary>
    /// Gets or sets the full file path of the collection file.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the collection file is read-only.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the last access time of the collection file.
    /// </summary>
    public DateTime LastAccessTime { get; set; }

    /// <summary>
    /// Gets or sets the last write time of the collection file.
    /// </summary>
    public DateTime LastWriteTime { get; set; }

    /// <summary>
    /// Gets or sets the length of the collection file.
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// Gets or sets the link target of the collection file.
    /// </summary>
    public string? LinkTarget { get; set; }

    /// <summary>
    /// Gets or sets the name of the collection file.
    /// </summary>
    public string Name { get; set; }
}