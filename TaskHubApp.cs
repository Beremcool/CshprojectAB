using System.Text.Json;
using TaskHub.Delegates;
using TaskHub.Models;
using TaskHub.Repositories;
using TaskHub.Services;
using TaskHub.Utilities;

namespace TaskHub;

public class TaskHubApp
{
    private readonly TaskManager _taskManager;
    private readonly IRepository<TaskItem> _repository;
    private readonly DeadlineMonitor _deadlineMonitor;

    public TaskHubApp(
        TaskManager taskManager,
        IRepository<TaskItem> repository,
        DeadlineMonitor deadlineMonitor)
    {
        _taskManager = taskManager;
        _repository = repository;
        _deadlineMonitor = deadlineMonitor;
    }

    public async Task RunAsync()
    {
        _deadlineMonitor.Start();

        bool isRunning = true;

        while (isRunning)
        {
            try
            {
                PrintMainMenu();
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateTask();
                        break;
                    case "2":
                        ShowTasksMenu();
                        break;
                    case "3":
                        EditTask();
                        break;
                    case "4":
                        DeleteTask();
                        break;
                    case "5":
                        SearchTasksMenu();
                        break;
                    case "6":
                        await ShowStatisticsAsync();
                        break;
                    case "7":
                        await SaveTasksAsync();
                        break;
                    case "8":
                        await LoadTasksAsync();
                        break;
                    case "9":
                        MarkTaskAsDone();
                        break;
                    case "0":
                        isRunning = false;
                        break;
                    default:
                        ConsoleHelper.WriteError("Такого пункта меню нет.");
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Непредвиденная ошибка: {ex.Message}");
            }
        }

        Console.WriteLine("TaskHub завершает работу.");
    }

    private static void PrintMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("========== TaskHub ==========");
        Console.WriteLine("1. Создать задачу");
        Console.WriteLine("2. Просмотреть задачи");
        Console.WriteLine("3. Редактировать задачу");
        Console.WriteLine("4. Удалить задачу");
        Console.WriteLine("5. Поиск задач");
        Console.WriteLine("6. Статистика");
        Console.WriteLine("7. Сохранить задачи в файл");
        Console.WriteLine("8. Загрузить задачи из файла");
        Console.WriteLine("9. Отметить задачу как выполненную");
        Console.WriteLine("0. Выход");
        Console.Write("> ");
    }

    private void CreateTask()
    {
        Console.WriteLine("\n--- Создание задачи ---");

        string title = ConsoleHelper.ReadNonEmptyString("Название: ");
        string description = ConsoleHelper.ReadOptionalString("Описание: ") ?? string.Empty;
        Priority priority = ConsoleHelper.ReadEnum<Priority>("Приоритет: ");
        DateTime deadline = ConsoleHelper.ReadDateTime("Дедлайн: ");
        
        if (deadline < DateTime.Now)
        {
            ConsoleHelper.WriteWarning("Внимание: дедлайн в прошлом!");
        }
        
        TaskItemStatus status = ConsoleHelper.ReadEnum<TaskItemStatus>("Статус: ");

        TaskItem createdTask = _taskManager.CreateTask(title, description, priority, deadline, status);
        ConsoleHelper.WriteSuccess($"Задача создана. ID: {createdTask.Id}");
    }

    private void ShowTasksMenu()
    {
        Console.WriteLine("\n--- Просмотр задач ---");
        Console.WriteLine("1. Все задачи");
        Console.WriteLine("2. Выполненные");
        Console.WriteLine("3. Невыполненные");
        Console.WriteLine("4. Задачи с высоким приоритетом");
        Console.Write("> ");

        string? choice = Console.ReadLine();

        TaskFilter filter = choice switch
        {
            "1" => task => true,
            "2" => task => task.Status == TaskItemStatus.Done,
            "3" => task => task.Status != TaskItemStatus.Done,
            "4" => task => task.Priority == Priority.High,
            _ => task => false
        };

        if (choice is not ("1" or "2" or "3" or "4"))
        {
            ConsoleHelper.WriteError("Такого пункта меню нет.");
            return;
        }

        List<TaskItem> tasks = _taskManager.GetByFilter(filter);
        PrintTasks(tasks);
    }

    private void EditTask()
    {
        Console.WriteLine("\n--- Редактирование задачи ---");
        int id = ConsoleHelper.ReadInt("Введите ID задачи: ");

        TaskItem? currentTask = _taskManager.FindById(id);

        if (currentTask is null)
        {
            ConsoleHelper.WriteError("Задача не найдена.");
            return;
        }

        Console.WriteLine("Текущие данные:");
        Console.WriteLine(currentTask);
        Console.WriteLine("Пустая строка означает: оставить текущее значение.");

        string? title = ConsoleHelper.ReadOptionalString("Новое название: ");
        string? description = ConsoleHelper.ReadOptionalString("Новое описание: ");
        Priority? priority = ConsoleHelper.ReadOptionalEnum<Priority>("Новый приоритет: ");
        TaskItemStatus? status = ConsoleHelper.ReadOptionalEnum<TaskItemStatus>("Новый статус: ");
        DateTime? deadline = ConsoleHelper.ReadOptionalDateTime("Новый дедлайн: ");

        bool edited = _taskManager.EditTask(
            id,
            title,
            description,
            priority,
            status,
            deadline);

        ConsoleHelper.WriteSuccess(edited ? "Задача обновлена." : "Задача не найдена.");
    }

    private void DeleteTask()
    {
        Console.WriteLine("\n--- Удаление задачи ---");
        int id = ConsoleHelper.ReadInt("Введите ID задачи: ");

        bool deleted = _taskManager.DeleteTask(id);
        ConsoleHelper.WriteSuccess(deleted ? "Задача удалена." : "Задача не найдена.");
    }

    private void SearchTasksMenu()
    {
        Console.WriteLine("\n--- Поиск задач ---");
        Console.WriteLine("1. По названию");
        Console.WriteLine("2. По статусу");
        Console.WriteLine("3. По приоритету");
        Console.Write("> ");

        string? choice = Console.ReadLine();
        List<TaskItem> result;

        switch (choice)
        {
            case "1":
            {
                string query = ConsoleHelper.ReadNonEmptyString("Введите часть названия: ");
                result = _taskManager.GetByFilter(task =>
                    task.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
                break;
            }
            case "2":
            {
                TaskItemStatus status = ConsoleHelper.ReadEnum<TaskItemStatus>("Выберите статус: ");
                result = _taskManager.GetByFilter(task => task.Status == status);
                break;
            }
            case "3":
            {
                Priority priority = ConsoleHelper.ReadEnum<Priority>("Выберите приоритет: ");
                result = _taskManager.GetByFilter(task => task.Priority == priority);
                break;
            }
            default:
                ConsoleHelper.WriteError("Такого пункта меню нет.");
                return;
        }

        PrintTasks(result);
    }

    private async Task ShowStatisticsAsync()
    {
        Console.WriteLine("\n--- Статистика ---");

        List<TaskItem> tasks = _taskManager.GetAllTasks();
        StatisticsReport report = await StatisticsService.BuildAsync(tasks);

        Console.WriteLine($"Всего задач: {report.TotalTasks}");
        Console.WriteLine($"Выполнено: {report.DoneTasks}");
        Console.WriteLine($"Просрочено: {report.OverdueTasks}");
        Console.WriteLine("По приоритетам:");

        foreach (KeyValuePair<Priority, int> item in report.TasksByPriority)
        {
            Console.WriteLine($"- {item.Key}: {item.Value}");
        }
    }

    private async Task SaveTasksAsync()
    {
        try
        {
            List<TaskItem> tasks = _taskManager.ExportTasks();
            await _repository.SaveAsync(tasks);
            ConsoleHelper.WriteSuccess("Задачи сохранены в файл tasks.json.");
        }
        catch (IOException ex)
        {
            ConsoleHelper.WriteError($"Ошибка работы с файлом: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            ConsoleHelper.WriteError($"Нет доступа к файлу: {ex.Message}");
        }
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            List<TaskItem> tasks = await _repository.LoadAsync();
            _taskManager.ReplaceTasks(tasks);
            ConsoleHelper.WriteSuccess($"Задачи загружены. Количество: {tasks.Count}");
        }
        catch (JsonException ex)
        {
            ConsoleHelper.WriteError($"Файл поврежден или имеет неверный JSON-формат: {ex.Message}");
        }
        catch (IOException ex)
        {
            ConsoleHelper.WriteError($"Ошибка работы с файлом: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            ConsoleHelper.WriteError($"Нет доступа к файлу: {ex.Message}");
        }
    }

    private void MarkTaskAsDone()
    {
        Console.WriteLine("\n--- Выполнение задачи ---");
        int id = ConsoleHelper.ReadInt("Введите ID задачи: ");

        bool updated = _taskManager.MarkAsDone(id);
        ConsoleHelper.WriteSuccess(updated ? "Задача отмечена как выполненная." : "Задача не найдена.");
    }

    private static void PrintTasks(List<TaskItem> tasks)
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("Задачи не найдены.");
            return;
        }

        foreach (TaskItem task in tasks.OrderBy(t => t.Deadline))
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine(task);
        }
    }
}
