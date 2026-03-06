// Helpers JavaScript pour le drag & drop des ingrédients et étapes
// Les événements Blazor @ondragstart/@ondrop gèrent la logique côté C#.
// Ce fichier est réservé pour des extensions futures si nécessaire.

window.recetteApp = {
    // Fait défiler vers le haut de la page
    scrollHaut: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
};
