# CrumbleDB
**CrumbleDB** is a lightweight, file-based persistence library designed for .NET applications that require simple, local storage without the complexity of a full database engine. It provides a generic, strongly-typed collection system backed by JSON files, enabling developers to store and retrieve data entities with minimal overhead. **CrumbleDB** supports asynchronous read and write operations and basic collection management such as adding, updating, removing, and purging entities. It is particularly suited for applications that need fast prototyping, offline data caching, or embedded storage with full control over serialization and file access.
## Description
In the **CrumbleDB** library, the core entities are designed to work together in a clean, modular architecture centered around file-based persistence. Here's how they are related:
- `CrumbleEntity` is the abstract base class for all stored entities. It defines a unique Id (as a `Guid`) and implements `IEquatable<CrumbleEntity>`, allowing entities to be compared and identified reliably.
- `CrumbleCollection<T>` is a generic, strongly-typed container for entities that inherit from `CrumbleEntity`. It encapsulates a list of entities, provides methods for adding, updating, removing, and iterating over data, and handles asynchronous serialization/deserialization to and from a JSON file. The collection manages both in-memory state and persistence logic, linking the entities to a file path on disk.
- `CrumbleDbCore` acts as the manager for collections stored in a specific directory. It is responsible for creating, retrieving, purging, copying, and deleting collection files. It builds the path to each collection’s JSON file based on the entity type name, ensuring type-safe and organized storage.
- `CrumbleDb` is a static entry point for the library. It provides a method to open or create a Crumble database folder and returns a configured `CrumbleDbCore` instance for managing collections within that directory.

Together, these components form a layered structure:
**`CrumbleDb` opens a storage context → `CrumbleDbCore` manages collections → `CrumbleCollection<T>` handles a specific data set → each item in the collection is a `CrumbleEntity`**. This design ensures separation of concerns between storage orchestration, file access, and entity management.
## Functionality
`CrumbleDb` offers the following methods:
- `Open()` - Opens a Crumble database located in the specified folder.

`CrumbleDbCore` offers the following methods:
- `GetCollectionNames()` - Returns the names of all existing collections (JSON files) in the database directory.
- `GetPathOf<T>()` - Gets the full file path for the collection associated with the specified type.
- `GetCollectionAsync<T>()` - Asynchronously retrieves or creates the collection file for the specified type and loads it into memory.
- `DropCollection<T>()` - Deletes the collection file associated with the specified type.
- `PurgeCollectionAsync<T>()` - Asynchronously clears the contents of the collection file for the specified type, but does not delete the file.
- `PurgeCollectionsAsync()` - Asynchronously clears all JSON collection files in the database directory.
- `Copy<T>()` - Creates a timestamped copy of the collection file for the specified type.

`CrumbleCollection` offers the following methods:
- `CreateAsync()` - Asynchronously loads a collection from the specified file path.
- `Add()` - Adds a new item to the collection.
- `AddRange()` - Adds multiple items to the collection.
- `Rewrite()` - Replaces the entire collection with the specified items.
- `Filter()` - Removes all items from the collection that do not match the specified predicate.
- `UpdateById()` - Updates an existing item in the collection by its Id (`Guid`).
- `Remove()` - Removes an item from the collection.
- `RemoveById()` - Removes an item by its.
- `RemoveAll()` - Removes all items that match the specified predicate.
- `Clear()` - Removes all items from the collection (in-memory only).
- `ForEach()` - Executes a specified action on each element in the collection.
- `ClearForcedAsync()` - Clears the collection and immediately writes the empty state to the file.
- `WriteAsync()` - Asynchronously writes the collection to disk using the specified file path.
- `ToDictionary()` - Converts the collection to a dictionary where the keys are the Id (`Guid`).
## Usage
Adding data to a file:
```csharp
var core = CrumbleDb.Open("data");
var entites = await core.GetCollectionAsync<TestEntity>();

entites.Add(new TestEntity { Name = "test" });
await entites.WriteAsync();
```

Get the full path to the file where the specified entities are stored:
```csharp
var core = CrumbleDb.Open(_testDir);
var path = core.GetPathOf<TestEntity>();
```
