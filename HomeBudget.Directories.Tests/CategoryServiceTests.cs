using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.Services.Interfaces;
using HomeBudget.Directories.Services.Implementations;
using HomeBudget.Directories.EF.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;


namespace HomeBudget.Directories.Tests;
public class CategoryServiceTests
{
    private readonly Mock<ICategoriesRepository> _mockRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<CategoryService> _logger;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _mockRepo = new Mock<ICategoriesRepository>();
        _logger = new NullLogger<CategoryService>();

        var inMemorySettings = new Dictionary<string, string?>
            {
                {"Services:Timeouts:CategoryService", "30000"}
            };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _service = new CategoryService(_mockRepo.Object, _logger, _config);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldCallRepositoryGetAll()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Food" },
            new() { Id = Guid.NewGuid(), Name = "Travel" }
        };

        _mockRepo.Setup(r => r.GetAll(It.IsAny<CancellationToken>()))
                 .Returns(categories.AsQueryable());

        // Act
        var result = await _service.GetAllCategoriesAsync();

        // Assert
        _mockRepo.Verify(r => r.GetAll(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Food" };
        _mockRepo.Setup(r => r.GetById(categoryId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(category);

        // Act
        var result = await _service.GetCategoryByIdAsync(categoryId);

        // Assert
        _mockRepo.Verify(r => r.GetById(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.Id);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetById(categoryId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Category)null);

        // Act
        var result = await _service.GetCategoryByIdAsync(categoryId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCallRepositoryCreate()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Food" };
        _mockRepo.Setup(r => r.Create(category, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateCategoryAsync(category);

        // Assert
        _mockRepo.Verify(r => r.Create(category, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Same(category, result);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldCallRepositoryUpdate()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Updated Food" };
        _mockRepo.Setup(r => r.Update(category, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateCategoryAsync(category);

        // Assert
        _mockRepo.Verify(r => r.Update(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldCallRepositoryDelete()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepo.Setup(r => r.Delete(categoryId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteCategoryAsync(categoryId);

        // Assert
        _mockRepo.Verify(r => r.Delete(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldThrowException_WhenRepositoryFails()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Food" };
        _mockRepo.Setup(r => r.Create(category, It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new DbUpdateException("Simulated error"));

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () => await _service.CreateCategoryAsync(category));
    }
}

