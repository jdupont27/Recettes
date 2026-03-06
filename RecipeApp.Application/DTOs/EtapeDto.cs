namespace RecipeApp.Application.DTOs;

public class EtapeDto
{
    public Guid Id { get; set; }
    public int NumeroEtape { get; set; }
    public string Description { get; set; } = string.Empty;
}