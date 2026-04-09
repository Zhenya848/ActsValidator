namespace UserService.Domain.Shared;

public record ProductData
{
    public string ProductId { get; set; }
    public int Price { get; set; }
}