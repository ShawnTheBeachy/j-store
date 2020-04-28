# JStore
.NET Core temporary JSON storage for those times when you want persistent storage without a SQL database, etc.

## Usage

All you need to use JStore is a `Repository`.
```csharp
using JStore;

var intRepository = new Repository<int>();
await intRepository.InitializeAsync();
```

And that quickly, you're up and running!

## Reading items

Each repository holds an in-memory list of items. You can access it via the `Items` property.

```csharp
var integers = intRepository.Items;
```

## Adding and removing items

Because the `Items` property is just a `List<T>` you can add, remove, and insert items just like normal.

```csharp
intRepository.Items.Add(10);
```

## Saving

Your changes to your item list will not actually be saved to the backing JSON file until you tell the repository to save its item list.

```csharp
await intRepository.SaveAsync();
```

## Dependency injection

If you're using ASP.NET Core and want to inject your repositories, just call `services.AddJStore()` in your startup method. Now you can inject any repository you please!

## Notes

The performance of JStore improved. However, since it is not intended to be used as long-term or production storage, I am leaving it as-is for now. It serves its purpose, which is to provide persistent storage for a small data-set when you are in the prototyping stage and just want persistent storage for testing without all the overhead of using, say, a SQL database.
