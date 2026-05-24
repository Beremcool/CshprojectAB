using TaskHub.Models;
using TaskHub.Utilities;

namespace TaskHub.Services;

public sealed class DeadlineMonitor : IDisposable
{
    private readonly TaskManager _taskManager;
    private readonly TimeSpan _interval;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly HashSet<int> _alreadyNotifiedTaskIds = new();
    private readonly object _syncRoot = new();
    private Task? _backgroundTask;
    private bool _disposed;

    public DeadlineMonitor(TaskManager taskManager, TimeSpan interval)
    {
        _taskManager = taskManager;
        _interval = interval;
    }

    public void Start()
    {
        ThrowIfDisposed();

        if (_backgroundTask is not null)
        {
            return;
        }

        _backgroundTask = Task.Run(CheckDeadlinesLoopAsync);
    }

    private async Task CheckDeadlinesLoopAsync()
    {
        CancellationToken token = _cancellationTokenSource.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_interval, token);

                List<TaskItem> overdueTasks = _taskManager.GetOverdueTasks();

                foreach (TaskItem task in overdueTasks)
                {
                    bool shouldNotify;

                    lock (_syncRoot)
                    {
                        shouldNotify = _alreadyNotifiedTaskIds.Add(task.Id);
                    }

                    if (shouldNotify)
                    {
                        ConsoleHelper.WriteNotification(
                            $"\n[УВЕДОМЛЕНИЕ] Задача просрочена: #{task.Id} — {task.Title} " +
                            $"(дедлайн: {task.Deadline:yyyy-MM-dd HH:mm})");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Нормальное завершение фоновой задачи.
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteNotification($"\n[ОШИБКА ФОНОВОЙ ПРОВЕРКИ] {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _cancellationTokenSource.Cancel();

        try
        {
            _backgroundTask?.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
            // Если фоновая задача завершалась из-за отмены, это не ошибка для пользователя.
        }

        _cancellationTokenSource.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DeadlineMonitor));
        }
    }
}
