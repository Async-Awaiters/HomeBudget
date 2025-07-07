using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.EF.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using HomeBudget.Directories.Services.Implementations;
using HomeBudget.Directories.Services.DTO;
using Microsoft.Extensions.Configuration;



namespace HomeBudget.Directories.Tests;

public class CategoryServiceTest
{
    private readonly Mock<ICategoriesRepository> _repositoryMock;
    private readonly Mock<ILogger<CategoryService>> _loggerMock;
    private readonly CategoryService _service;

    public CategoryServiceTest()
    {
        _repositoryMock = new Mock<ICategoriesRepository>();
        _loggerMock = new Mock<ILogger<CategoryService>>() ;

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetValue<int>("Services:Timeouts:CategoryService", 30000))
                  .Returns(100); // например, 100 мс

        _service = new CategoryService(_repositoryMock.Object, _loggerMock.Object, configMock.Object);
    }

    [Fact]
    public async Task GetAllCategoriesAsync()
    {
        var result = await _service.GetAllCategoriesAsync();

        _repositoryMock.Verify(r => r.GetAll(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCategoryByIdAsync()
    {
        var categoryId = Guid.NewGuid();

        var result = await _service.GetCategoryByIdAsync(categoryId);

        _repositoryMock.Verify(r => r.GetById(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUpdateDeleteCategoryAsync()
    {

        var dto = new CreateCategoryDto
        {
            Name = "New Category",
            ParentId = null,
            UserId = Guid.NewGuid()
        };
        var result = await _service.CreateCategoryAsync(dto);
        _repositoryMock.Verify(r => r.GetById(result.Id, It.IsAny<CancellationToken>()), Times.Once);

        result.Name = "New New Category";
        var resultTwo = await _service.UpdateCategoryAsync(result);
        _repositoryMock.Verify(r => r.GetById(result.Id, It.Equals<CancellationToken>(result, resultTwo)), Times.Once);


        await _service.DeleteCategoryAsync(result.Id);

    }


    private async IAsyncEnumerable<T> GetAsyncEnumerable<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
            yield return item;

        await Task.CompletedTask;
    }
}