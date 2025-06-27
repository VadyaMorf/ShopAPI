using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Application.Models;
using Shop.Core.Abstractions;

namespace Shop.Endpoints;

public static class BarcodeEndpoints
{
    public static void MapBarcodeEndpoints(this WebApplication app)
    {
        app.MapGet("/api/barcode/generate", [Authorize] async (
            IBarcodeService barcodeService,
            IUserRepositories userRepository,
            HttpContext context) =>
        {
            var userId = context.User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Results.Unauthorized();
            }

            var user = await userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return Results.NotFound("Пользователь не найден");
            }

            var barcode = barcodeService.GenerateBarcode(
                user.Id.GetHashCode(),
                user.PhoneNumber ?? "",
                user.Email ?? "",
                user.FirstName ?? "",
                user.LastName ?? ""
            );

            return Results.Ok(new { barcode });
        })
        .WithName("GenerateBarcode");

        app.MapPost("/api/barcode/decode", async (
            IBarcodeService barcodeService,
            [FromBody] DecodeBarcodeRequest request) =>
        {
            var barcodeData = barcodeService.DecodeBarcode(request.Barcode);
            if (barcodeData == null)
            {
                return Results.BadRequest("Неверный формат штрих-кода");
            }

            return Results.Ok(barcodeData);
        })
        .WithName("DecodeBarcode");
    }
}

public class DecodeBarcodeRequest
{
    public string Barcode { get; set; } = string.Empty;
} 