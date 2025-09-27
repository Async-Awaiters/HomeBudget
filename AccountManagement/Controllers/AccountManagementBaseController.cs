using System.Security.Claims;
using AccountManagement.EF.Exceptions;
using AccountManagement.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Controllers;

public abstract class AccountManagementBaseController : ControllerBase
{
    protected readonly ILogger<AccountManagementBaseController> _logger;

    public AccountManagementBaseController(ILogger<AccountManagementBaseController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Выполняет операцию с логированием и обработкой исключений
    /// </summary>
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
    /// Возвращает ID пользователя из токена
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception> <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
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
