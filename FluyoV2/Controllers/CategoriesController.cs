using FluyoV2.Controllers.Base;
using FluyoV2.Features.Categories.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluyoV2.Controllers;

[Route("api/categories")]
[Authorize]
public class CategoriesController : BaseController
{
    private readonly CategoriesService _service;

    public CategoriesController(CategoriesService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _service.GetAll();

        return Success(
            result,
            "Categorías consultadas correctamente");
    }

    [HttpGet("income")]
    public IActionResult GetIncome()
    {
        var result = _service.GetIncome();

        return Success(
            result,
            "Categorías de ingreso consultadas correctamente");
    }

    [HttpGet("expense")]
    public IActionResult GetExpense()
    {
        var result = _service.GetExpense();

        return Success(
            result,
            "Categorías de gasto consultadas correctamente");
    }
}