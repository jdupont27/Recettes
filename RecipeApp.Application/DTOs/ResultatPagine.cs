namespace RecipeApp.Application.DTOs;

public class ResultatPagine<T>
{
    public IEnumerable<T> Elements { get; set; } = Enumerable.Empty<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int TaillePage { get; set; }
    public int NombrePages => TaillePage > 0 ? (int)Math.Ceiling((double)Total / TaillePage) : 0;
}