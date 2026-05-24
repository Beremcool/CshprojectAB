using System.Globalization;

namespace TaskHub.Utilities;

public static class ConsoleHelper
{
    private static readonly object ConsoleSyncRoot = new();

    public static string ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            WriteError("Значение не должно быть пустым.");
        }
    }

    public static string? ReadOptionalString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }

    public static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int value) && value > 0)
                {
                    return value;
                }

                WriteError("Введите положительное целое число.");
            }
        }
    public static DateTime ReadDateTime(string prompt)
    {
        string[] formats =
        {
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd",
            "dd.MM.yyyy HH:mm",
            "dd.MM.yyyy"
        };

        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (DateTime.TryParseExact(
                    input,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime exactDate))
            {
                return exactDate;
            }

            if (DateTime.TryParse(input, out DateTime flexibleDate))
            {
                return flexibleDate;
            }

            WriteError("Введите дату в формате yyyy-MM-dd HH:mm, yyyy-MM-dd, dd.MM.yyyy HH:mm или dd.MM.yyyy.");
        }
    }

    public static DateTime? ReadOptionalDateTime(string prompt)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        string[] formats =
        {
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd",
            "dd.MM.yyyy HH:mm",
            "dd.MM.yyyy"
        };

        if (DateTime.TryParseExact(
                input,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime exactDate))
        {
            return exactDate;
        }

        if (DateTime.TryParse(input, out DateTime flexibleDate))
        {
            return flexibleDate;
        }

        WriteError("Дата не распознана. Значение не будет изменено.");
        return null;
    }

    public static TEnum ReadEnum<TEnum>(string prompt) where TEnum : struct, Enum
    {
        while (true)
        {
            PrintEnumValues<TEnum>();
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (TryParseEnum(input, out TEnum value))
            {
                return value;
            }

            WriteError("Выберите значение из списка.");
        }
    }

    public static TEnum? ReadOptionalEnum<TEnum>(string prompt) where TEnum : struct, Enum
    {
        PrintEnumValues<TEnum>();
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        if (TryParseEnum(input, out TEnum value))
        {
            return value;
        }

        WriteError("Значение не распознано. Поле не будет изменено.");
        return null;
    }

    public static void WriteSuccess(string message)
    {
        lock (ConsoleSyncRoot)
        {
            Console.WriteLine(message);
        }
    }

    public static void WriteError(string message)
    {
        lock (ConsoleSyncRoot)
        {
            Console.WriteLine($"Ошибка: {message}");
        }
    }

    public static void WriteNotification(string message)
    {
        lock (ConsoleSyncRoot)
        {
            Console.WriteLine(message);
            Console.Write("> ");
        }
    }

    public static void Pause()
    {
        Console.WriteLine("\nНажмите Enter, чтобы продолжить...");
        Console.ReadLine();
    }

    private static void PrintEnumValues<TEnum>() where TEnum : struct, Enum
    {
        foreach (TEnum value in Enum.GetValues<TEnum>())
        {
            Console.WriteLine($"{Convert.ToInt32(value)}. {value}");
        }
    }

    private static bool TryParseEnum<TEnum>(string? input, out TEnum value) where TEnum : struct, Enum
    {
        if (int.TryParse(input, out int number) && Enum.IsDefined(typeof(TEnum), number))
        {
            value = (TEnum)Enum.ToObject(typeof(TEnum), number);
            return true;
        }

        if (Enum.TryParse(input, ignoreCase: true, out value) && Enum.IsDefined(typeof(TEnum), value))
        {
            return true;
        }

        value = default;
        return false;
    }
}
