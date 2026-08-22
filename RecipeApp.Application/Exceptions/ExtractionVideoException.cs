namespace RecipeApp.Application.Exceptions;

/// <summary>Erreur survenue pendant l'extraction d'une recette depuis une vidéo (upload, traitement Gemini, quota).</summary>
public class ExtractionVideoException : Exception
{
    public int CodeHttp { get; }

    public ExtractionVideoException(string message, int codeHttp = 502) : base(message)
    {
        CodeHttp = codeHttp;
    }
}
