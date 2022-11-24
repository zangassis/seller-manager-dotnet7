using Microsoft.AspNetCore.Http.HttpResults;
using SellerManager.Models;

namespace SellerManager.Helpers;
public static class MapSellers
{
    public static RouteGroupBuilder MapSellersApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllSellers);
        return group;
    }

    public static Ok<List<Seller>> GetAllSellers(SellerDb db)
    {
        var sellers = db.Sellers;
        return TypedResults.Ok(sellers.ToList());
    }
}