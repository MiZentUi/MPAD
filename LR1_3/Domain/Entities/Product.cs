namespace LR1_3.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Service? Service { get; set; }
    public string? Image { get; set; }
    public string? Mime { get; set; }
}