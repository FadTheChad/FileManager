# FileManager

It uh, parses ini files but for a different purpose? Why? idk.

You know how `.ini` files look like:

```ini
[General]
appName = FileManager
version = 1.0.0
enableLogging = true

[Paths]
inputDirectory = D:/Projects/Input
outputDirectory = D:/Projects/Output
logFile = D:/Projects/FileManager/logs/app.log

[UserSettings]
theme = dark
autosaveInterval = 10   ; in minutes
language = en-US

[Network]
useProxy = false
proxyHost = 127.0.0.1
proxyPort = 8080
```

Well I basically changed the `=` to `:`, and instead of storing config settings, I'm using it like a local data file — kinda like a text-based database or a weird CSV replacement.

That’s it.

That’s basically it.

Why am I doing this? It was kinda fun.

---

## 📦 What This Is

A generic C# file-based storage utility that uses reflection to read and write object data in a human-readable `.ini`-style format.

---

## 🚀 Example Format

```ini
[user]
name: Alice
age: 25
roles: [User, Editor]
```

---

## 🛠 Usage

### 1. Define a model

```csharp
public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
    public List<string> Roles { get; set; }
}
```

### 2. Initialize FileManager

```csharp
using FileParser;

var manager = new FileManager<User>("users.txt");
```

---

## ➕ Create

```csharp
var user = new User
{
    Name = "Alice",
    Age = 25,
    Roles = new List<string> { "User", "Editor" }
};

manager.Create(user);
manager.Save();
```

---

## 🔁 Update

```csharp
bool updated = manager.Update("Name", "Alice", u =>
{
    u.Age = 26;
    u.Roles.Add("Reviewer");
});

if (updated)
    manager.Save();
```

---

## ❌ Delete

```csharp
bool deleted = manager.Delete("Name", "Alice");

if (deleted)
    manager.Save();
```

---

## 📋 Read All

```csharp
var users = manager.GetAll();

foreach (var user in users)
{
    Console.WriteLine($"{user.Name}, {user.Age}, Roles: {string.Join(", ", user.Roles)}");
}
```

---

## ⚠ Notes

- Supports strings, numbers, enums, and lists
- Properties are matched by name (case-insensitive)
- `[Section]` headers start a new object
- Only public properties are used
- Lists are stored like `[item1, item2, item3]`

---
