using Shop.Application.Models;

namespace Shop.Application.Interfaces;

public interface IBarcodeService
{
    string GenerateBarcode(int userId, string phone, string email, string firstName, string lastName);
    BarcodeData? DecodeBarcode(string barcode);
} 