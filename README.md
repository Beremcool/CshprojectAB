# TaskHub

Финальный проект по C#: консольное приложение для управления задачами.

## Что умеет приложение

- Создание задачи: название, описание, приоритет, дедлайн, статус.
- Просмотр задач:
  - все задачи;
  - выполненные;
  - невыполненные;
  - задачи с высоким приоритетом.
- Редактирование задачи.
- Удаление задачи.
- Поиск задач:
  - по названию;
  - по статусу;
  - по приоритету.
- Статистика:
  - общее количество задач;
  - количество выполненных;
  - количество просроченных;
  - количество задач по приоритетам.
- Сохранение задач в файл `tasks.json`.
- Загрузка задач из файла `tasks.json`.
- Фоновая проверка дедлайнов раз в 10 секунд.

## Что использовано по требованиям

| Требование | Где используется |
|---|---|
| ООП и классы | `TaskItem`, `TaskManager`, `TaskHubApp`, `DeadlineMonitor`, `JsonFileRepository<T>` |
| Коллекции | `List<TaskItem>`, `Dictionary<Priority, int>` |
| Generic | `IRepository<T>`, `JsonFileRepository<T>`, `ReadEnum<TEnum>()` |
| Делегаты | `TaskFilter` |
| Обработка исключений | `try-catch` в `TaskHubApp` при сохранении/загрузке и в главном цикле |
| IDisposable | `IRepository<T> : IDisposable`, `JsonFileRepository<T>`, `DeadlineMonitor` |
| static классы/методы | `ConsoleHelper`, `StatisticsService` |
| async/await | `SaveAsync`, `LoadAsync`, `RunAsync`, `ShowStatisticsAsync` |
| Многопоточность | `DeadlineMonitor`, `Task.Run`, `CancellationTokenSource` |
| Синхронизация | `lock` в `TaskManager` и `DeadlineMonitor` |

## Как запустить

Вариант через терминал:

```bash
cd TaskHub
dotnet run
```

Нужен .NET SDK 8.0 или новее.

## Формат дедлайна

Можно вводить дату в одном из форматов:

```text
yyyy-MM-dd HH:mm
yyyy-MM-dd
dd.MM.yyyy HH:mm
dd.MM.yyyy
```

Примеры:

```text
2026-05-30 18:00
2026-05-30
30.05.2026 18:00
30.05.2026
```

## Файл сохранения

После выбора пункта меню `Сохранить задачи в файл` приложение создаст рядом с проектом файл:

```text
tasks.json
```
