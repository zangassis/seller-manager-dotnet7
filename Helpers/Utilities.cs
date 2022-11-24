using SellerManager.Models;

namespace SellerManager.Helpers;
public static class Utilities
{
    public static string IsValid(Seller seller)
    {
        string errorMessage = string.Empty;

        if (string.IsNullOrEmpty(seller.Name))
            errorMessage = "Seller name is required";

        return errorMessage;
    }
}