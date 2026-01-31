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
    using System.Reflection.Metadata;

    using Windows.System;

    public class Program
    {
        private static void Main(string[] args)
        {
            ConsoleMenu.Add("1", "MicroBase Create/Insert/Save", () => MenuPoint1());
            ConsoleMenu.Add("2", "MicroBase Load/Get", () => MenuPoint2());
            ConsoleMenu.Add("X", "Beenden", () => ApplicationExit());

            do
            {
                _ = ConsoleMenu.SelectKey(2, 2);
            }
            while (true);
        }

        private static void ApplicationExit()
        {
            Environment.Exit(0);
        }

        private static void MenuPoint1()
        {
            Console.Clear();

            var db = new MicroBase();

            Table<User> users = null;
            if (db.TableExists("Users") == false)
            {
                users = db.CreateTable<User>("Users");
            }

            if (users.IsRow(x => x.Name == "Gerhard") == false)
            {
                users.Insert(new User { Name = "Gerhard", CreatedAt = DateTime.Now });
            }

            if (users.IsRow(x => x.Name == "PTA") == false)
            {
                users.Insert(new User { Name = "PTA", CreatedAt = DateTime.Now });
            }

            db.UpdateTable("Users", users);
            DatabaseFile.Save(db, "mydb.bin", "SuperSecret123");

            ConsoleMenu.Wait();
        }

        private static void MenuPoint2()
        {
            Console.Clear();

            var db = DatabaseFile.Load("mydb.bin", "SuperSecret123");

            Table<User>  users = db.GetTable<User>("Users");

            foreach (var u in users.GetAll())
            {
                Console.WriteLine(u.Name);
            }

            ConsoleMenu.Wait();
        }
    }

    public class User : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class Product : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }
}
