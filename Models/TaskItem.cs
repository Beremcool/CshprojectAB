namespace TaskHub.Models;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Priority Priority { get; set; } = Priority.Medium;

    public DateTime Deadline { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.New;

    public bool IsDone => Status == TaskItemStatus.Done;

    public bool IsOverdue => !IsDone && Deadline < DateTime.Now;

    public TaskItem Copy()
    {
        return new TaskItem
        {
            Id = Id,
            Title = Title,
            Description = Description,
            Priority = Priority,
            Deadline = Deadline,
            Status = Status
        };
    }

    public override string ToString()
    {
        string overdueText = IsOverdue ? " | ПРОСРОЧЕНА" : string.Empty;

        return
            $"ID: {Id}\n" +
            $"Название: {Title}\n" +
            $"Описание: {Description}\n" +
            $"Приоритет: {Priority}\n" +
            $"Дедлайн: {Deadline:yyyy-MM-dd HH:mm}\n" +
            $"Статус: {Status}{overdueText}\n";
    }
}
