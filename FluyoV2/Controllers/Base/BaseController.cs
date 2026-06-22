using FluyoV2.Common;
using Microsoft.AspNetCore.Mvc;

namespace FluyoV2.Controllers.Base;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult Success<T>(
        T data,
        string message = "Operación exitosa")
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }

    protected IActionResult Failure(
        string message)
    {
        return BadRequest(
            ApiResponse<object>.Fail(message));
    }

    protected IActionResult NotFoundResponse(
        string message)
    {
        return NotFound(
            ApiResponse<object>.Fail(message));
    }
}