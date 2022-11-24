namespace SellerManager.Models;
public class Seller
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public Seller(Guid id, string? name)
    {
        Id = id;
        Name = name;
    }
}
