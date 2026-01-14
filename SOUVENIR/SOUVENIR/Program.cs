/// Підключення стандартного простору імен C#
/// Містить базові типи даних, Console, Exception тощо
using System;

/// Простір імен для роботи з файлами (CSV-файли)
using System.IO;

/// Містить LINQ-методи: Skip, Any, Where, Select, Min, Max, Average
using System.Linq;

/// Простір імен для криптографії (хешування паролів)
using System.Security.Cryptography;

/// Для роботи з кодуванням тексту (UTF-8)
using System.Text;

/// Для роботи з колекціями (List тощо)
using System.Collections.Generic;

/// Основний клас програми
class Program
{
    /// Константа з ім’ям файлу користувачів
    const string USERS_FILE = "users.csv";

    /// Константа з ім’ям файлу з турами / товарами
    const string TRAVEL_FILE = "shop.csv";

    /// Головний метод програми — точка входу
    static void Main()
    {
        /// Ініціалізація файлів (створення, якщо їх нема)
        InitFiles();

        /// Безкінечний цикл головного меню
        while (true)
        {
            Console.WriteLine("\n1 - Реєстрацiя");
            Console.WriteLine("2 - Вхiд");
            Console.WriteLine("0 - Вихiд");
            Console.Write("Ваш вибiр: ");

            /// Обробка вибору користувача
            switch (Console.ReadLine())
            {
                case "1":
                    Register(); /// Реєстрація нового користувача
                    break;

                case "2":
                    /// Якщо вхід успішний — відкриваємо меню турів
                    if (Login())
                        TravelMenu();
                    break;

                case "0":
                    return; /// Завершення програми

                default:
                    Console.WriteLine("Невiрний вибiр");
                    break;
            }
        }
    }

    /// Метод створює CSV-файли, якщо вони відсутні
    static void InitFiles()
    {
        /// Якщо файл користувачів не існує — створюємо з шапкою
        if (!File.Exists(USERS_FILE))
            File.WriteAllText(USERS_FILE, "Id,Email,PasswordHash\n");

        /// Якщо файл турів не існує — створюємо з шапкою
        if (!File.Exists(TRAVEL_FILE))
            File.WriteAllText(TRAVEL_FILE, "Id,Name,Country,Price,Days\n");
    }

    /// Метод реєстрації користувача
    static void Register()
    {
        Console.Write("Email: ");
        string email = Console.ReadLine();

        Console.Write("Password: ");
        string password = Console.ReadLine();

        /// Зчитуємо всі рядки з users.csv
        var lines = File.ReadAllLines(USERS_FILE);

        /// Перевіряємо, чи email уже існує (пропускаємо шапку)
        if (lines.Skip(1).Any(l => l.Split(',')[1] == email))
        {
            Console.WriteLine("Email вже iснує");
            return;
        }

        /// Генеруємо унікальний ID
        int id = GenerateId(USERS_FILE);

        /// Хешуємо пароль
        string hash = Hash(password);

        /// Додаємо нового користувача у файл
        File.AppendAllText(USERS_FILE, $"{id},{email},{hash}\n");

        Console.WriteLine("Реєстрацiя успiшна");
    }

    /// Метод входу (авторизації)
    static bool Login()
    {
        Console.Write("Email: ");
        string email = Console.ReadLine();

        Console.Write("Password: ");
        string password = Console.ReadLine();

        /// Хешуємо введений пароль
        string hash = Hash(password);

        /// Перебираємо всі рядки файлу користувачів (без шапки)
        foreach (var line in File.ReadAllLines(USERS_FILE).Skip(1))
        {
            /// Розбиваємо рядок на поля
            var p = line.Split(',');

            /// Якщо рядок пошкоджений — пропускаємо
            if (p.Length != 3) continue;

            /// Перевірка email та хешу пароля
            if (p[1] == email && p[2] == hash)
            {
                Console.WriteLine("Вхiд виконано");
                return true;
            }
        }

        Console.WriteLine("Невiрнi данi");
        return false;
    }

    /// Меню роботи з турами / сувенірами
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

    /// Додавання нового туру / сувеніру
    static void AddSouvenir()
    {
        Console.Write("Назва: ");
        string name = Console.ReadLine();

        Console.Write("Країна: ");
        string cat = Console.ReadLine();

        Console.Write("Цiна: ");
        if (!double.TryParse(Console.ReadLine(), out double price))
            return; /// Захист від неправильного вводу

        Console.Write("Кiлькiсть днiв: ");
        if (!int.TryParse(Console.ReadLine(), out int qty))
            return;

        /// Генеруємо ID
        int id = GenerateId(TRAVEL_FILE);

        /// Додаємо запис у CSV
        File.AppendAllText(TRAVEL_FILE, $"{id},{name},{cat},{price},{qty}\n");

        Console.WriteLine("Тур додано");
    }

    /// Вивід усіх записів
    static void ShowAll()
    {
        var lines = File.ReadAllLines(TRAVEL_FILE);

        /// Якщо є тільки шапка — файл порожній
        if (lines.Length <= 1)
        {
            Console.WriteLine("Список порожнiй");
            return;
        }

        Console.WriteLine("\nID | Назва | Країна | Цiна | Кiлькiсть днiв");

        /// Перебір усіх рядків без шапки
        foreach (var l in lines.Skip(1))
        {
            var p = l.Split(',');
            if (p.Length != 5) continue;

            /// Форматований вивід
            Console.WriteLine($"{p[0],-3} {p[1],-15} {p[2],-12} {p[3],-6} {p[4],-5}");
        }
    }

    /// Пошук по назві
    static void Search()
    {
        Console.Write("Пошук по назвi: ");
        string q = Console.ReadLine().ToLower();

        foreach (var l in File.ReadAllLines(TRAVEL_FILE).Skip(1))
        {
            var p = l.Split(',');
            if (p.Length != 5) continue;

            /// Перевірка входження тексту
            if (p[1].ToLower().Contains(q))
                Console.WriteLine($"{p[0]} {p[1]} {p[2]} {p[3]} {p[4]}");
        }
    }

    /// Видалення запису за ID
    static void Delete()
    {
        Console.Write("ID для видалення: ");
        string id = Console.ReadLine();

        /// Фільтруємо всі рядки, крім того, що починається з потрібного ID
        var lines = File.ReadAllLines(TRAVEL_FILE)
            .Where(l => !l.StartsWith(id + ","))
            .ToArray();

        /// Повний перезапис файлу
        File.WriteAllLines(TRAVEL_FILE, lines);

        Console.WriteLine("Видалено");
    }

    /// Статистика по кількості днів
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

    /// Генерація унікального ID
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

    /// Хешування пароля за допомогою SHA-256
    static string Hash(string input)
    {
        using var sha = SHA256.Create();

        /// Перетворюємо рядок у байти → хеш → Base64
        return Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(input))
        );
    }
}
}
