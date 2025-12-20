using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

class Program
{
    const string USERS_FILE = "users.csv";
    const string TRAVEL_FILE = "shop.csv";

    static void Main()
    {
        InitFiles();

        while (true)
        {
            Console.WriteLine("\n1 - Реєстрацiя");
            Console.WriteLine("2 - Вхiд");
            Console.WriteLine("0 - Вихiд");
            Console.Write("Ваш вибiр: ");

            switch (Console.ReadLine())
            {
                case "1": Register(); break;
                case "2":
                    if (Login())
                        TravelMenu();
                    break;
                case "0": return;
                default: Console.WriteLine("Невiрний вибiр"); break;
            }
        }
    }

    static void InitFiles()
    {
        if (!File.Exists(USERS_FILE))
            File.WriteAllText(USERS_FILE, "Id,Email,PasswordHash\n");

        if (!File.Exists(TRAVEL_FILE))
            File.WriteAllText(TRAVEL_FILE, "Id,Name,Country,Price,Days\n");
    }

    static void Register()
    {
        Console.Write("Email: ");
        string email = Console.ReadLine();

        Console.Write("Password: ");
        string password = Console.ReadLine();

        var lines = File.ReadAllLines(USERS_FILE);

        if (lines.Skip(1).Any(l => l.Split(',')[1] == email))
        {
            Console.WriteLine("Email вже iснує");
            return;
        }

        int id = GenerateId(USERS_FILE);
        string hash = Hash(password);

        File.AppendAllText(USERS_FILE, $"{id},{email},{hash}\n");
        Console.WriteLine("Реєстрацiя успiшна");
    }

    static bool Login()
    {
        Console.Write("Email: ");
        string email = Console.ReadLine();

        Console.Write("Password: ");
        string password = Console.ReadLine();

        string hash = Hash(password);

        foreach (var line in File.ReadAllLines(USERS_FILE).Skip(1))
        {
            var p = line.Split(',');
            if (p.Length != 3) continue;

            if (p[1] == email && p[2] == hash)
            {
                Console.WriteLine("Вхiд виконано");
                return true;
            }
        }

        Console.WriteLine("Невiрнi данi");
        return false;
    }

    static void TravelMenu()
    {
        while (true)
        {
            Console.WriteLine("\n1 - Додати тур");
            Console.WriteLine("2 - Показати всi");
            Console.WriteLine("3 - Пошук");
            Console.WriteLine("4 - Видалити");
            Console.WriteLine("5 - Статистика");
            Console.WriteLine("0 - Вихiд");
            Console.Write("Вибiр: ");

            switch (Console.ReadLine())
            {
                case "1": AddSouvenir(); break;
                case "2": ShowAll(); break;
                case "3": Search(); break;
                case "4": Delete(); break;
                case "5": Stats(); break;
                case "0": return;
                default: Console.WriteLine("Невiрний вибiр"); break;
            }
        }
    }

    static void AddSouvenir()
    {
        Console.Write("Назва: ");
        string name = Console.ReadLine();

        Console.Write("Країна: ");
        string cat = Console.ReadLine();

        Console.Write("Цiна: ");
        if (!double.TryParse(Console.ReadLine(), out double price)) return;

        Console.Write("Кiлькiсть днiв: ");
        if (!int.TryParse(Console.ReadLine(), out int qty)) return;

        int id = GenerateId(TRAVEL_FILE);
        File.AppendAllText(TRAVEL_FILE, $"{id},{name},{cat},{price},{qty}\n");

        Console.WriteLine("Тур додано");
    }

    static void ShowAll()
    {
        var lines = File.ReadAllLines(TRAVEL_FILE);
        if (lines.Length <= 1)
        {
            Console.WriteLine("Список порожнiй");
            return;
        }

        Console.WriteLine("\nID | Назва | Країна | Цiна | Кiлькiсть днiв");
        foreach (var l in lines.Skip(1))
        {
            var p = l.Split(',');
            if (p.Length != 5) continue;

            Console.WriteLine($"{p[0],-3} {p[1],-15} {p[2],-12} {p[3],-6} {p[4],-5}");
        }
    }

    static void Search()
    {
        Console.Write("Пошук по назвi: ");
        string q = Console.ReadLine().ToLower();

        foreach (var l in File.ReadAllLines(TRAVEL_FILE).Skip(1))
        {
            var p = l.Split(',');
            if (p.Length != 5) continue;

            if (p[1].ToLower().Contains(q))
                Console.WriteLine($"{p[0]} {p[1]} {p[2]} {p[3]} {p[4]}");
        }
    }

    static void Delete()
    {
        Console.Write("ID для видалення: ");
        string id = Console.ReadLine();

        var lines = File.ReadAllLines(TRAVEL_FILE)
            .Where(l => !l.StartsWith(id + ","))
            .ToArray();

        File.WriteAllLines(TRAVEL_FILE, lines);
        Console.WriteLine("Видалено");
    }

    static void Stats()
    {
        var items = File.ReadAllLines(TRAVEL_FILE)
            .Skip(1)
            .Select(l => l.Split(','))
            .Where(p => p.Length == 5)
            .Select(p => int.Parse(p[4]))
            .ToList();

        if (!items.Any()) return;

        Console.WriteLine($"Кiлькiсть позицiй: {items.Count}");
        Console.WriteLine($"Мiн: {items.Min()}");
        Console.WriteLine($"Макс: {items.Max()}");
        Console.WriteLine($"Сума: {items.Sum()}");
        Console.WriteLine($"Середнє: {items.Average():F2}");
    }

    static int GenerateId(string file)
    {
        int max = 0;
        foreach (var l in File.ReadAllLines(file).Skip(1))
        {
            var p = l.Split(',');
            if (int.TryParse(p[0], out int id) && id > max)
                max = id;
        }
        return max + 1;
    }

    static string Hash(string input)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
    }
}