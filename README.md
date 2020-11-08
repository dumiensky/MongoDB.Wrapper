## MongoDB.Wrapper

A handy CRUD wrapper with soft deletion over MongoDB.Driver.**MongoClient**.

This wrapper is designed with _less-code-is-better_ in mind. It uses code-first aproach, just install the nuget and start working on the database!

## Installation
Install [NuGet package **MongoDB.Wrapper**](https://www.nuget.org/packages/MongoDB.Wrapper/)

#### dotnet cli: 
`dotnet add package MongoDB.Wrapper`

#### .csproj edit:
```
<ItemGroup>
	<PackageReference Include="MongoDB.Wrapper" Version="*" />
</ItemGroup>
```

## Usage

#### Dependency Injection

`MongoDB.Wrapper.MongoDb` requires a `MongoDB.Wrapper.Settings.MongoDbSettings` settings instance to work

>_Examples here use **ASP .Net Core DI container** and **appsettings.json** to store the settings. Wrapper is registered as a singleton, because [that's what the docs recommend](http://mongodb.github.io/mongo-csharp-driver/2.0/reference/driver/connecting/#re-use)_

```javascript
// appsettings.json
{
	"MongoDbSettings": {
		"ConnectionString": "<YOUR CONNECTION STRING>",
		"DatabaseName": "<YOUR DATABASE NAME>"
	}
}
```

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
	// get the settings from appsettings.json
	var mongoDbSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
	
	// register the settings and the wrapper
	services.AddSingleton(mongoDbSettings);
	services.AddSingleton<IMongoDb, MongoDb>();
	
	// OR
	
	// register only the instance of the wrapper
	services.AddSingleton<IMongoDb>(new MongoDb(mongoDbSettings));
}
```

#### Normal class instantiation

```csharp
var mongoDbSettings = new MongoDbSettings
{
	ConnectionString = "<YOUR CONNECTION STRING>",
	DatabaseName = "<YOUR DATABASE NAME>"
};

var mongoDb = new MongoDb(mongoDbSettings);
```

## Examples

The wrapper will work on any entity that derive from `MongoDB.Wrapper.Entity` or implement `MongoDB.Wrapper.Abstractions.IEntity`
```csharp
public class Person : Entity
{
	public string Name { get; set; }
	public int Age { get; set; }
}

var person = new Person
{
	Name = "Steve",
	Age = 21
};
```

Every method exposed by the wrapper interface (except for `Query<>`) is asynchronous, so the _-Async_ suffix is omitted.

#### Add an entity to the database
```csharp
// assigns a new Id to entity and adds it to collection of type TEntity
await mongoDb.Add(person);

// id can be accessed straight from the object
Guid assignedId = person.Id;
```

#### Access the entities
```csharp
// get list of all persons in the database
var persons = await mongoDb.Get<Person>();
 
// get single entity by its' Id
var person = await mongoDb.Get<Person>(personId);

// get all entities with age of 10
var persons = await mongoDb.Get<Person>(p => p.Age == 10);

// get all entites with name Frank, older than 18, including the deleted ones
var persons = await mongoDb.Get<Person>(p => p.Name == "Frank" && p.Age > 18, includeDeleted: true);
```

#### Update the entities
```csharp
person.Name = "Frank";

// replace the entity by its' Id
await mongoDb.Replace(person);
```

#### List of all methods exposed by the wrapper

Returned type | Method | Description
------------- | ------ | -----------
`IMongoQueryable<TEntity>` | **Query<>(includeDeleted)**<br/>`Query<TEntity>(bool includeDeleted = false)` | Returns an open query to operate on
`Task` | **Add(entity)**<br/>`Add<TEntity>(TEntity)` | Assigns a new Id to entity and adds it to collection of type TEntity
`Task<bool>` | **Any(includeDeleted)**<br/>`Any<TEntity>(bool includeDeleted = false)` | Returns whether the collection of type TEntity contains entites
`Task<bool>` | **Any(predicate, includeDeleted)**<br/>`Any<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false)` | Returns whether the collection of type TEntity contains entites matching the predicate
`Task<int>` | **Count(includeDeleted)**<br/>`Count<TEntity>(bool includeDeleted = false)` | Returns the count of entites in the collection of type TEntity
`Task<int>` | **Count(predicate, includeDeleted)**<br/>`Count<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false)` | Returns the count of entites in the collection of type TEntity that match given predicate
`Task<TEntity>` | **Get(id)**<br/>`Get<TEntity>(Guid id)` | Returns entity of type TEntity, which Id equals passed Guid. Default if not found
`Task<List<TEntity>>` | **Get(includeDeleted)**<br/>`Get<TEntity>(bool includeDeleted = false)` | Returns list of entities from collection of type TEntity
`Task<List<TEntity>>` | **Get(predicate, includeDeleted)**`Get<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false)` | Returns list of entities from collection of type TEntity, that match the predicate
`Task<TEntity>` | **FirstOrDefault(predicate, includeDeleted)**<br/>`FirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false)` | Returns the first entity from collection of type TEntity, that matches the predicate
`Task<TEntity>` | **SingleOrDefault(predicate, includeDeleted)**<br/>`SingleOrDefault<TEntity>(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false)` | Returns the entity from collection of type TEntity, that matches the predicate. If more than one entity matches the predicate, an exception is thrown
`Task` | **Replace(entity)**<br/>`Replace<TEntity>(TEntity entity)` | Replaces the object of type TEntity with passed object, compared by IEntity.Id. If IEntity.Id equals default, entity is added. When entity with given id is not found, an exception is thrown.

## Roadmap

1. `Update()` method that will update only given properties of an entity

**Feel free to create an Issue or a Pull Request**
