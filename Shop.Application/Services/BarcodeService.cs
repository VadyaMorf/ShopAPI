using System.Text;
using System.Text.Json;
using Shop.Application.Interfaces;
using Shop.Application.Models;

namespace Shop.Application.Services;

public class BarcodeService : IBarcodeService
{
    public string GenerateBarcode(int userId, string phone, string email, string firstName, string lastName)
    {
        var barcodeData = new BarcodeData
        {
            UserId = userId,
            Phone = phone,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var json = JsonSerializer.Serialize(barcodeData);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    public BarcodeData? DecodeBarcode(string barcode)
    {
        try
        {
            var bytes = Convert.FromBase64String(barcode);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<BarcodeData>(json);
        }
        catch
        {
            return null;
        }
    }
} 