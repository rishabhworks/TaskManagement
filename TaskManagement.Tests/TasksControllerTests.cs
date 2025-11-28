using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagement.API.Controllers;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Models;
using TaskStatus = TaskManagement.Core.Models.TaskStatus;

namespace TaskManagement.Tests;

public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _mockRepository;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mockRepository = new Mock<ITaskRepository>();
        _controller = new TasksController(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllTasks_ReturnsOk_WithTasks()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", Status = TaskStatus.Pending },
            new() { Id = 2, Title = "Task 2", Status = TaskStatus.InProgress }
        };

        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);

        var result = await _controller.GetAllTasks();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(ok.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task GetTask_ValidId_ReturnsOk()
    {
        var taskId = 1;
        var task = new TaskItem { Id = taskId, Title = "Test Task", Status = TaskStatus.Pending };

        _mockRepository.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _controller.GetTask(taskId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<TaskItem>(ok.Value);
        Assert.Equal(taskId, returned.Id);
    }

    [Fact]
    public async Task GetTask_InvalidId_ReturnsNotFound()
    {
        var taskId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync((TaskItem?)null);

        var result = await _controller.GetTask(taskId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateTask_Valid_ReturnsCreated()
    {
        var dto = new TaskItemCreateDto
        {
            Title = "New Task",
            Description = "Test Description",
            Status = TaskStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var created = new TaskItem
        {
            Id = 1,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
                       .ReturnsAsync(created);

        var result = await _controller.CreateTask(dto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returned = Assert.IsType<TaskItem>(createdAt.Value);
        Assert.Equal(1, returned.Id);
    }

    [Fact]
    public async Task DeleteTask_Valid_ReturnsNoContent()
    {
        var id = 1;
        _mockRepository.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

        var result = await _controller.DeleteTask(id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteTask_Invalid_ReturnsNotFound()
    {
        var id = 999;
        _mockRepository.Setup(r => r.DeleteAsync(id)).ReturnsAsync(false);

        var result = await _controller.DeleteTask(id);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateTask_Valid_ReturnsOk()
    {
        var id = 1;
        var dto = new TaskItemUpdateDto
        {
            Title = "Updated Task",
            Description = "Updated Description",
            Status = TaskStatus.Completed,
            DueDate = DateTime.UtcNow.AddDays(3)
        };

        var updated = new TaskItem
        {
            Id = id,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            DueDate = dto.DueDate,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>()))
                       .ReturnsAsync(updated);

        var result = await _controller.UpdateTask(id, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<TaskItem>(ok.Value);
        Assert.Equal("Updated Task", returned.Title);
        Assert.Equal(TaskStatus.Completed, returned.Status);
    }
}
