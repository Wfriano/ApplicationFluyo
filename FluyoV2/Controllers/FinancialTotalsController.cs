using FluyoV2.Controllers.Base;
using FluyoV2.Features.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluyoV2.Controllers;

[Route("api/financial-totals")]
[Authorize]
public class FinancialTotalsController : BaseController
{
    private readonly DashboardService _service;

    public FinancialTotalsController(
        DashboardService service)
    {
        _service = service;
    }

    [HttpGet("assets/pending-installments-total")]
    public async Task<IActionResult> GetAssetsPendingInstallmentsTotal()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var total = await _service.GetAssetsPendingInstallmentsTotalAsync(userId);

        return Success(total, "Total pendiente por cuotas consultado correctamente");
    }

    [HttpGet("assets/pending-installments")]
    public async Task<IActionResult> GetAssetsPendingInstallments()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetAssetsPendingInstallmentsAsync(userId);

        return Success(result, "Detalle de cuotas pendientes consultado correctamente");
    }

    [HttpGet("commitments/total")]
    public async Task<IActionResult> GetCommitmentsTotal()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Failure("Usuario no autorizado");

        var result = await _service.GetCommitmentsTotalAsync(userId);

        return Success(result, "Total de compromisos consultado correctamente");
    }
}
