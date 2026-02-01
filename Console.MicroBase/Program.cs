//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Lifeprojects.de">
//     Class: Program
//     Copyright © Lifeprojects.de 2026
// </copyright>
// <Template>
// 	Version 3.0.2026.1, 08.1.2026
// </Template>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>31.01.2026 19:41:52</date>
//
// <summary>
// Konsolen Applikation mit Menü
// </summary>
//-----------------------------------------------------------------------

namespace Console.MicroBase
{
    /* Imports from NET Framework */
    using System;
    using System.Diagnostics;
    using System.Reflection.Metadata;

    using Windows.System;

    public class Program
    {
        private static void Main(string[] args)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DemoDataPath = Path.Combine(new DirectoryInfo(currentDirectory).Parent.Parent.Parent.FullName, "DemoDatei");
            if (Directory.Exists(DemoDataPath) == false)
            {
                Directory.CreateDirectory(DemoDataPath);
            }

            ConsoleMenu.Add("0", "MicroBase Create/Save", () => MenuPoint0());
            ConsoleMenu.Add("1", "MicroBase Create/Insert/Save", () => MenuPoint1());
            ConsoleMenu.Add("2", "MicroBase Load/Get", () => MenuPoint2());
            ConsoleMenu.Add("3", "MicroBase Einzel Update", () => MenuPoint3());
            ConsoleMenu.Add("4", "MicroBase Save/Load Datenbank Async", () => MenuPoint4());
            ConsoleMenu.Add("X", "Beenden", () => ApplicationExit());

            do
            {
                _ = ConsoleMenu.SelectKey(2, 2);
            }
            while (true);
        }
        internal static string DemoDataPath { get; private set; }

        private static void ApplicationExit()
        {
            Environment.Exit(0);
        }

        private static void MenuPoint0()
        {
            Console.Clear();

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
            ConsoleMenu.Print($"Datenbank '{databaseName}' wurde erstellt.", ConsoleColor.Green);
            ConsoleMenu.Wait();
        }

        private static void MenuPoint1()
        {
            Console.Clear();

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
            ConsoleMenu.Print($"Insert '{users.Count}' wurde erstellt.", ConsoleColor.Green);

            /*
            if (users.IsRow(x => x.Name == "Gerhard") == false)
            {
                users.Insert(new User { Name = "Gerhard", CreatedAt = DateTime.Now });
            }
            */


            db.UpdateTable("Users", users);
            DatabaseFile.Save(db, DemoDataPath, "SuperSecret123");
            ConsoleMenu.Print($"Datenbank '{databaseName}' wurde erstellt.", ConsoleColor.Green);
            ConsoleMenu.Wait();
        }

        private static void MenuPoint2()
        {
            Console.Clear();

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

            ConsoleMenu.Wait();
        }

        private static void MenuPoint3()
        {
            Console.Clear();

            string databaseName = Path.Combine(DemoDataPath, "mydbUpdate.bin");
            if (File.Exists(databaseName) == true)
            {
                File.Delete(databaseName);
            }

            var db = DatabaseFile.Load(databaseName, "SuperSecret123");
            Table<User> users = db.GetTable<User>("Users");

            User userGet = users.Get(x => x.Name == "Donald Duck");
            userGet.Department = "Werbung";
            users.Update(userGet);

            db.UpdateTable("Users", users);
            DatabaseFile.Save(db, DemoDataPath, "SuperSecret123");

            ConsoleMenu.Wait();
        }

        private static void MenuPoint4()
        {
            Console.Clear();

            string databaseName = Path.Combine(DemoDataPath, "mydbAsync.bin");
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
            DatabaseFile.SaveAsync(db, databaseName, "SuperSecret123").GetAwaiter();
            ConsoleMenu.Print($"Datenbank '{databaseName}' wurde erstellt.", ConsoleColor.Green);

            var dbAsync = DatabaseFile.Load(databaseName, "SuperSecret123");

            foreach (var tableName in dbAsync.GetTableNames())
            {
                ConsoleMenu.Print($"Tabelle: {tableName}", ConsoleColor.Cyan);
            }


            ConsoleMenu.Wait();
        }
    }

    [DebuggerDisplay("Name = {this.Name}; UserName = {this.UserName}; Department = {this.Department}; IsActive = {this.IsActive}")]
    public class User : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Department { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Product : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProductNumber { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
