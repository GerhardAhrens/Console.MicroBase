# MicroBase

![NET](https://img.shields.io/badge/NET-10.0-green.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
![VS2022](https://img.shields.io/badge/Visual%20Studio-2026-white.svg)
![Version](https://img.shields.io/badge/Version-1.0.2026.0-yellow.svg)]

MicroBase ist eine properitäre .NET 10.0 Datenbank für geringe bis mittlere Datenmengen, die speziell für den Einsatz in Desktop-Anwendungen entwickelt wurde. Sie bietet eine einfache und effiziente Möglichkeit, Daten lokal zu speichern und abzurufen, ohne auf externe Datenbankserver angewiesen zu sein.

Die MicroBase kann als Embedded-Datenbank in Desktop-Anwendungen integriert werden und ermöglicht es Entwicklern, Datenstrukturen flexibel zu definieren und zu verwalten. Sie unterstützt grundlegende Datenbankoperationen wie Einfügen, Aktualisieren, Löschen und Abfragen von Daten.\
Die Embedded-Datenbank bietet folgende Features:
- Implementiert als Singlefile Datenbank
- bietet grundlegende CRUD Operationen
- Die Embedded-Datenbank ist immer verschlüsselt
- Intern werden die Daten in einer JSON-Struktur gespeichert.
- Zum lesen und schrieben der Datenbank werden zusätzlich Async Methoden angeboten.

## Beispiele
### Erstellung einer MicroBase Datenbank
```csharp
string databaseName = Path.Combine(DemoDataPath, "mydbTableOnly.bin");
if (File.Exists(databaseName) == true)
{
    File.Delete(databaseName);
}

var db = new MicroBase();

Table<Product> product = null;
if (db.TableExists("Product") == false)
{
    product = db.CreateTable<Product>("Product");
}

Table<User> users = null;
if (db.TableExists("Users") == false)
{
    users = db.CreateTable<User>("Users");
}

db.UpdateTable("Users", users);
DatabaseFile.Save(db, databaseName, "SuperSecret123");
```

### Einfügen von Datensätzen (Insert)

```csharp
string databaseName = Path.Combine(DemoDataPath, "mydb.bin");
if (File.Exists(databaseName) == true)
{
    File.Delete(databaseName);
}

var db = new MicroBase();

Table<User> users = null;
if (db.TableExists("Users") == false)
{
    users = db.CreateTable<User>("Users");
}

users.Insert(new User { Name = "Gerhard", UserName = "XX0001", Department="IT", IsActive=true });
users.Insert(new User { Name = "Charlie", UserName = "XX0002", Department = "Test", IsActive = true });
users.Insert(new User { Name = "Donald Duck", UserName = "XX0003", Department = "Test", IsActive = true });
users.Insert(new User { Name = "Dagober Duck", UserName = "XX0004", Department = "Verkauf", IsActive = true });
users.Insert(new User { Name = "Daisy Duck", UserName = "XX0005", Department = "Verkauf", IsActive = true });
db.UpdateTable("Users", users);
DatabaseFile.Save(db, DemoDataPath, "SuperSecret123");
```

### Lesen von Datensätzen (Get)

```csharp
string databaseName = Path.Combine(DemoDataPath, "mydbGet.bin");
if (File.Exists(databaseName) == true)
{
    File.Delete(databaseName);
}

var db = DatabaseFile.Load(databaseName, "SuperSecret123");
Table<User>  users = db.GetTable<User>("Users");

User userGet = users.Get(x => x.Name == "Donald Duck");

foreach (var u in users.GetAll())
{
    Console.WriteLine($"{u.Name}; {u.UserName}; {u.Department}; {u.IsActive}");
}

ConsoleMenu.PrintLine();

foreach (var u in users.GetAll(x => x.Department == "Test"))
{
    Console.WriteLine($"{u.Name}; {u.UserName}; {u.Department}; {u.IsActive}");
}
```
\
Dieses Projekt ist eher als Lern-Projekt zuum Thema Datenbanken in .NET gedacht und weniger für den produktiven Einsatz geeignet. Für professionelle Anwendungen sollten etablierte Datenbanklösungen in Betracht gezogen werden.
Trotzdem hoffe ich, dass MicroBase als nützliches Werkzeug für Entwickler dient, die eine einfache und leichtgewichtige Datenbanklösung für ihre Desktop-Anwendungen (z.B. für Konfiguratione oder zum temporären Zwischenspeichern von Daten) suchen.
