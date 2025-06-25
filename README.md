# CrumbleDB - A Lightweight JSON-based Database for .NET
## Overview
CrumbleDB is a simple, persistent, in-memory database solution for .NET applications that stores data in JSON files. It provides a lightweight alternative to full-fledged database systems when you need basic persistence with minimal overhead.
#### Key features
- Type-safe collections with generic support
- Asynchronous operations
- Automatic JSON serialization/deserialization
- Transaction support with rollback capability
- File-based persistence
- Backups
## Quick Start
Add the CrumbleDB package to your project via NuGet (package name would be specified here in a real scenario).
#### 1. Initialize the Database
```csharp
using CrumbleDB.Core;

// Open or create a database in the specified folder
var db = CrumbleDb.Open("path/to/database/folder");
```
#### 2. Define Your Entity
```csharp
using CrumbleDB.Entities;

public class User : CrumbleEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}
```
#### 3. Work with Collections
```csharp
// Get or create a collection
var users = await db.GetCollectionAsync<User>();

// Add items
users.Add(new User { Name = "Alice", Email = "alice@example.com", Age = 30 });
await users.AddForcedAsync(new User { Name = "Bob", Email = "bob@example.com", Age = 25 });

// Query items
var youngUsers = users.Values.Where(u => u.Age < 30).ToList();

// Update items
users.UpdateById(userId, user => user.Age++);

// Remove items
users.RemoveById(userId);

// Execute transaction
await users.ExecuteTransactionAsync(() =>
{
	collection.AddRange(Generate(100));
	collection.RemoveAll(x => x.Age >= 50);
	collection.ForEach((user) => user.Name += "_Updated");
});
```
## Core Concepts
#### `CrumbleEntity`
All entities must inherit from `CrumbleEntity` which provides:
- Automatic GUID-based ID generation
- Value-based equality comparison
- Hash code implementation
#### Collections
Collections (`CrumbleCollection<T>`) are typed containers that:
- Store entities of type `T` where `T : CrumbleEntity`    
- Persist data to JSON files
- Provide in-memory operations with optional immediate persistence
#### Database Operations
The `CrumbleDbCore` class provides:
- Collection management
- Backup creation
- Collection purging
- File information retrieval
## API Reference
#### `CrumbleCollection<T>`
**Basic Operations**
- `Add(T item)` - Adds an item to the collection (in-memory only)
- `AddForcedAsync(T item)` - Adds an item and persists immediately
- `AddRange(IEnumerable<T> items)` - Adds multiple items to the collection
- `AddRangeForcedAsync(IEnumerable<T> items)` - Adds multiple items to the collection and immediately writes them to file
- `Remove(T item)` - Removes an item (in-memory only)
- `RemoveForcedAsync(T item)` - Removes an item and persists immediately
- `Clear()` - Clears the collection (in-memory only)
- `ClearForcedAsync()` - Clears and persists immediately
- `WriteAsync()` - Asynchronously writes the collection to disk using the specified file path

**Query Operations**
- `Values` - Gets all items as a read-only list
- `Count` - Gets the number of items
- `IsEmpty` - Checks if the collection is empty
- `ToDictionary()` - Converts to a dictionary keyed by entity IDs

**Advanced Operations**
- `ExecuteTransactionAsync(Action action)` - Executes an action with rollback on failure
- `ForEach(Action<T> action)` - Executes a specified action on each element in the collection
- `ForEachForcedAsync(Action<T> action)` - Executes a specified action on each element in the collection and immediately writes the collection to file
- `UpdateById(Guid id, Action<T> patch)` - Updates an entity by ID
- `UpdateByIdForcedAsync(Guid id, Action<T> patch)` - Updates an existing item in the collection by its ID and immediately writes it to file
- `UpdateAll(Predicate<T> predicate, Action<T> patch)` - Updates all items in the collection that match the specified predicate using the specified patch function
- `UpdateAllForcedAsync(Predicate<T> predicate, Action<T> patch)` - Updates all items in the collection that match the specified predicate using the specified patch function and immediately writes the collection to file
- `RemoveById(Guid id)` - Removes an entity by ID
- `RemoveByIdForcedAsync(Guid id)` - Removes an item by its ID and immediately writes the collection to file
- `Rewrite(IEnumerable<T> items)` - Replaces all items in the collection
- `RewriteForcedAsync(IEnumerable<T> items)` - Replaces the entire collection with the specified items and immediately writes them to file.
#### `CrumbleDbCore`
**Collection Management**
- `GetCollectionAsync<T>()` - Gets or creates a collection
- `GetCollectionInfo<T>()` - Gets file information about a collection
- `DropCollection<T>()` - Deletes a collection file
- `PurgeCollectionAsync<T>()` - Clears a collection file

**Maintenance**
- `CreateBackup<T>()` - Creates a timestamped backup
- `RestoreBackupAsync<T>(string backupPath)` - Restores the collection file for the specified type from a backup file
- `PurgeCollectionsAsync()` - Clears all collection files
- `GetCollectionNames()` - Lists all collection names

---

- Not designed for high-throughput scenarios
- No built-in indexing or advanced query capabilities
- Entire collections are loaded into memory
- No relationship management between collections