using TaskHub.Models;

namespace TaskHub.Services;

public static class StatisticsService
{
    public static Task<StatisticsReport> BuildAsync(List<TaskItem> tasks)
    {
        return Task.Run(() =>
        {
            var byPriority = Enum
                .GetValues<Priority>()
                .ToDictionary(priority => priority, priority => tasks.Count(t => t.Priority == priority));

            return new StatisticsReport
            {
                TotalTasks = tasks.Count,
                DoneTasks = tasks.Count(t => t.Status == TaskItemStatus.Done),
                OverdueTasks = tasks.Count(t => t.IsOverdue),
                TasksByPriority = byPriority
            };
        });
    }
}
