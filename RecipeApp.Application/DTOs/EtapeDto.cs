namespace RecipeApp.Application.DTOs;

public class EtapeDto
{
    public Guid Id { get; set; }
    public int NumeroEtape { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>Position de l'étape dans la vidéo source, en secondes (extraction vidéo). Null si non identifiable.</summary>
    public int? TimestampSecondes { get; set; }
}