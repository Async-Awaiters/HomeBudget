using System.Security.Claims;
using AccountManagement.EF.Exceptions;
using AccountManagement.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Controllers;

/// <summary>
/// Базовый контроллер для управления учетными записями
/// </summary>
public abstract class AccountManagementBaseController : ControllerBase
{
    /// <summary>
    /// Логгер для записи информации о выполнении операций
    /// </summary>
    protected readonly ILogger<AccountManagementBaseController> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AccountManagementBaseController"/>
    /// </summary>
    /// <param name="logger">Логгер для регистрации событий</param>
    public AccountManagementBaseController(ILogger<AccountManagementBaseController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Выполняет операцию с логированием и обработкой исключений
    /// </summary>
    /// <param name="operation">Делегат асинхронной операции</param>
    /// <returns>HTTP-ответ с результатом выполнения операции</returns>
    /// <exception cref="AccessDeniedException">Если доступ запрещен</exception>
    /// <exception cref="EntityAlreadyExistsException">Если сущность уже существует</exception>
    /// <exception cref="EntityNotFoundException">Если сущность не найдена</exception>
    /// <exception cref="InvalidTransactionException">Если транзакция не может быть выполнена</exception>
    /// <exception cref="ArgumentNullException">Если передан null в обязательный параметр</exception>
    /// <exception cref="Exception">При других ошибках выполнения</exception>
    protected async Task<IActionResult> ExecuteWithLogging(Func<Task<IActionResult>> operation)
    {
        try
        {
            return await operation();
        }
        catch (AccessDeniedException ex)
        {
            _logger.LogError(ex, ex.Message);
            return Forbid(ex.Message);
        }
        catch (EntityAlreadyExistsException ex)
        {
            _logger.LogError(ex, ex.Message);
            return Conflict(ex.Message);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogError(ex, ex.Message);
            return NotFound(ex.Message);
        }
        catch (InvalidTransactionException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка выполнения запроса {nameof(operation)}");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Извлекает идентификатор пользователя из контекста HTTP-запроса
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса</param>
    /// <returns>Уникальный идентификатор пользователя (GUID)</returns>
    /// <exception cref="AccessDeniedException">Если отсутствует или недействителен идентификатор пользователя</exception>
    protected static Guid GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new AccessDeniedException("Необходима авторизация");

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new AccessDeniedException("Неверный формат ID пользователя");

        return userId;
    }
}
