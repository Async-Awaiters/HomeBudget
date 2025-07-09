using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.Services.Implementations;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HomeBudget.Directories.Tests
{
    public class CurrencyServiceTests
    {
        private readonly Mock<ICurrencyRepository> _mockRepo;
        private readonly IConfiguration _config;
        private readonly ILogger<CurrencyService> _logger;
        private readonly CurrencyService _service;

        public CurrencyServiceTests()
        {
            _mockRepo = new Mock<ICurrencyRepository>();
            _logger = new NullLogger<CurrencyService>();

            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Services:Timeouts:CurrencyService", "30000"}
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new CurrencyService(_mockRepo.Object, _logger, _config);
        }

        [Fact]
        public async Task GetAllCurrenciesAsync_ShouldCallRepositoryGetAll()
        {
            // Arrange
            var currencies = new List<Currency>
            {
                new() { Id = Guid.NewGuid(), Name = "Dollar", Code = "USD", Country = "USA" },
                new() { Id = Guid.NewGuid(), Name = "Euro", Code = "EUR", Country = "Germany" }
            };

            _mockRepo.Setup(r => r.GetAll(It.IsAny<CancellationToken>()))
            .Returns(currencies.AsQueryable());

            // Act
            var result = await _service.GetAllCurrenciesAsync();

            // Assert
            _mockRepo.Verify(r => r.GetAll(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetCurrencyByIdAsync_ShouldReturnCurrency_WhenExists()
        {
            // Arrange
            var currencyId = Guid.NewGuid();
            var currency = new Currency
            {
                Id = Guid.NewGuid(),
                Name = "US Dollar",
                Code = "USD",
                Country = "USA"
            };

            _mockRepo.Setup(r => r.GetById(currencyId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(currency);

            // Act
            var result = await _service.GetCurrencyByIdAsync(currencyId);

            // Assert
            _mockRepo.Verify(r => r.GetById(currencyId, It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(currencyId, result.Id);
        }

        [Fact]
        public async Task GetCurrencyByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var currencyId = Guid.NewGuid();

            _mockRepo.Setup(r => r.GetById(currencyId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Currency)null);

            // Act
            var result = await _service.GetCurrencyByIdAsync(currencyId);

            // Assert
            Assert.Null(result);
        }
    }
}
