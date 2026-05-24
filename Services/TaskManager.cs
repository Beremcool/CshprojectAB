using TaskHub.Delegates;
using TaskHub.Models;

namespace TaskHub.Services;

public class TaskManager
{
    private readonly List<TaskItem> _tasks = new();
    private readonly object _syncRoot = new();
    private int _nextId = 1;

    public TaskItem CreateTask(
        string title,
        string description,
        Priority priority,
        DateTime deadline,
        TaskItemStatus status)
    {
        lock (_syncRoot)
        {
            var task = new TaskItem
            {
                Id = _nextId++,
                Title = title,
                Description = description,
                Priority = priority,
                Deadline = deadline,
                Status = status
            };

            _tasks.Add(task);
            return task.Copy();
        }
    }

    public bool EditTask(
        int id,
        string? title,
        string? description,
        Priority? priority,
        TaskItemStatus? status,
        DateTime? deadline = null)
    {
        lock (_syncRoot)
        {
            TaskItem? task = _tasks.FirstOrDefault(t => t.Id == id);

            if (task is null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                task.Title = title;
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                task.Description = description;
            }

            if (priority.HasValue)
            {
                task.Priority = priority.Value;
            }

            if (status.HasValue)
            {
                task.Status = status.Value;
            }

            if (deadline.HasValue)
            {
                task.Deadline = deadline.Value;
            }

            return true;
        }
    }

    public bool MarkAsDone(int id)
    {
        lock (_syncRoot)
        {
            TaskItem? task = _tasks.FirstOrDefault(t => t.Id == id);

            if (task is null)
            {
                return false;
            }

            task.Status = TaskItemStatus.Done;
            return true;
        }
    }

    public bool DeleteTask(int id)
    {
        lock (_syncRoot)
        {
            TaskItem? task = _tasks.FirstOrDefault(t => t.Id == id);

            if (task is null)
            {
                return false;
            }

            _tasks.Remove(task);
            return true;
        }
    }

    public TaskItem? FindById(int id)
    {
        lock (_syncRoot)
        {
            return _tasks.FirstOrDefault(t => t.Id == id)?.Copy();
        }
    }

    public List<TaskItem> GetAllTasks()
    {
        lock (_syncRoot)
        {
            return _tasks.Select(t => t.Copy()).ToList();
        }
    }

    public List<TaskItem> GetByFilter(TaskFilter filter)
    {
        lock (_syncRoot)
        {
            return _tasks
                .Where(t => filter(t))
                .Select(t => t.Copy())
                .ToList();
        }
    }

    public List<TaskItem> GetOverdueTasks()
    {
        return GetByFilter(t => t.IsOverdue);
    }

    public List<TaskItem> ExportTasks()
    {
        return GetAllTasks();
    }

    public void ReplaceTasks(List<TaskItem> tasks)
    {
        lock (_syncRoot)
        {
            _tasks.Clear();

            foreach (TaskItem task in tasks)
            {
                if (string.IsNullOrWhiteSpace(task.Title))
                {
                    task.Title = "Без названия";
                }

                task.Description ??= string.Empty;
                _tasks.Add(task.Copy());
            }

            _nextId = _tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1;
        }
    }
}
