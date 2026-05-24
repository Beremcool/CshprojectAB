using TaskHub;
using TaskHub.Models;
using TaskHub.Repositories;
using TaskHub.Services;

var manager = new TaskManager();

using IRepository<TaskItem> repository = new JsonFileRepository<TaskItem>("tasks.json");
using var monitor = new DeadlineMonitor(manager, TimeSpan.FromSeconds(10));

var app = new TaskHubApp(manager, repository, monitor);
await app.RunAsync();
