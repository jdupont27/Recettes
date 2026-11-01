# 🍳 Extracteur de Recettes Intelligent

Application web moderne conçue pour numériser et organiser vos recettes. 
Grâce à l'intelligence artificielle, l'application analyse une photo 
(manuscrite ou imprimée) ou une vidéo Youtube et extrait automatiquement les ingrédients 
et les étapes de préparation.

https://recetteapp.fly.dev/

L'inscription nécessite un code d'invitation (mode bêta) — voir `.env.example`.

---

## 🚀 Fonctionnalités Clés

* **IA & Vision :** Extraction automatique des ingrédients via **Google Gemini API** (photo ou vidéo YouTube).
* **Gestion de Contenu :** CRUD complet pour organiser vos recettes personnelles.
* **Stockage Cloud :** Hébergement des images sur **Amazon S3**.
* **Comptes & sécurité :** Authentification ASP.NET Identity, réinitialisation de mot de passe par courriel.
* **Déploiement Moderne :** Containerisation avec **Docker** et hébergement sur **Fly.io**.
* **Base de données :** Persistance des données sur **MySQL** (via Aiven).

---

## 🚢 Déploiement

Hébergement principal sur **Fly.io** via `deploy-fly.bat` (machine en veille
automatique — `auto_stop_machines` — pour réduire les coûts). Voir
`DEPLOY_FLY.md` pour la configuration initiale et les secrets.

Le script `deploy.bat` (AWS App Runner) est conservé dans le repo : le
service AWS est fermé pour l'instant, mais le script permet d'y redéployer
rapidement au besoin (même base de données Aiven et même bucket S3).

---

## 🛠️ Stack Technique

| Technologie | Utilisation |
| :--- | :--- |
| **ASP.NET Core 10** | Framework principal (Blazor Server) |
| **Fly.io** | Hébergement de l'application (conteneur) |
| **Amazon S3** | Stockage des images |
| **Google Gemini API** | Intelligence Artificielle (analyse d'images et de vidéos) |
| **Resend** | Envoi de courriels (réinitialisation de mot de passe) |
| **Entity Framework Core** | Mapping de données (Code First) |
| **MySQL** | Base de données relationnelle (Aiven) |
| **Docker** | Containerisation et portabilité |


## 🗺️ Roadmap

Ce projet est un prototype fonctionnel. Les améliorations prévues incluent :

- [ ] **Tests unitaires** — Ajout de tests automatisés
      handlers CQRS et la logique de permissions
- [ ] **Health Checks** — Endpoints de monitoring pour la DB, S3 et l'API Gemini
- [ ] **CI/CD** — Pipeline GitHub Actions pour build, test et déploiement 
      automatique vers Fly.io
- [ ] **Domaine de courriel vérifié** — pour l'envoi direct des courriels de
      réinitialisation de mot de passe (actuellement en mode relais manuel)
