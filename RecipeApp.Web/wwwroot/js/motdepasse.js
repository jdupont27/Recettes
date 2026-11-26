// Bascule afficher / masquer pour les champs de mot de passe.
// Délégation d'événement sur le document : fonctionne aussi après la navigation
// améliorée de Blazor (enhanced navigation), qui remplace le DOM sans recharger la page.
(function () {
    document.addEventListener('click', function (e) {
        var bouton = e.target.closest('[data-basculer-mdp]');
        if (!bouton) return;

        var groupe = bouton.closest('.input-group');
        var champ = groupe ? groupe.querySelector('input') : null;
        if (!champ) return;

        var afficher = champ.type === 'password';
        champ.type = afficher ? 'text' : 'password';

        var libelle = afficher ? 'Masquer le mot de passe' : 'Afficher le mot de passe';
        bouton.setAttribute('aria-pressed', afficher ? 'true' : 'false');
        bouton.setAttribute('aria-label', libelle);
        bouton.setAttribute('title', libelle);

        bouton.querySelector('.icone-oeil').classList.toggle('d-none', afficher);
        bouton.querySelector('.icone-oeil-barre').classList.toggle('d-none', !afficher);

        champ.focus();
    });

    // Remet les champs en mode masqué avant l'envoi du formulaire, pour que le
    // navigateur propose correctement d'enregistrer le mot de passe.
    document.addEventListener('submit', function (e) {
        e.target.querySelectorAll('[data-basculer-mdp]').forEach(function (bouton) {
            var champ = bouton.closest('.input-group').querySelector('input');
            if (champ && champ.type === 'text') {
                champ.type = 'password';
                bouton.setAttribute('aria-pressed', 'false');
                bouton.setAttribute('aria-label', 'Afficher le mot de passe');
                bouton.setAttribute('title', 'Afficher le mot de passe');
                bouton.querySelector('.icone-oeil').classList.remove('d-none');
                bouton.querySelector('.icone-oeil-barre').classList.add('d-none');
            }
        });
    }, true);
})();
