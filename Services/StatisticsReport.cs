using TaskHub.Models;

namespace TaskHub.Services;

public class StatisticsReport
{
    public int TotalTasks { get; init; }

    public int DoneTasks { get; init; }

    public int OverdueTasks { get; init; }

    public Dictionary<Priority, int> TasksByPriority { get; init; } = new();
}
