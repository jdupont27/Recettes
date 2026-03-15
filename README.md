# 🍳 Extracteur de Recettes Intelligent

Application web moderne conçue pour numériser et organiser vos recettes. 
Grâce à l'intelligence artificielle, l'application analyse une photo 
(manuscrite ou imprimée) et extrait automatiquement les ingrédients 
et les étapes de préparation.

---

## 🚀 Fonctionnalités Clés

* **IA & Vision :** Extraction automatique des ingrédients via **Google Gemini API**.
* **Gestion de Contenu :** CRUD complet pour organiser vos recettes personnelles.
* **Stockage Cloud :** Hébergement des images sur **Amazon S3**.
* **Déploiement Moderne :** Containerisation avec **Docker** et déploiement sur **AWS App Runner**.
* **Base de données :** Persistance des données sur **MySQL** (via Aiven).

---

## 🛠️ Stack Technique

| Technologie | Utilisation |
| :--- | :--- |
| **ASP.NET Core 10** | Framework principal (Blazor Server) |
| **AWS (S3 / App Runner)** | Infrastructure Cloud et Hébergement |
| **Google Gemini API** | Intelligence Artificielle (analyse d'images) |
| **Entity Framework Core** | Mapping de données (Code First) |
| **MySQL** | Base de données relationnelle |
| **Docker** | Containerisation et portabilité |


## 🗺️ Roadmap

Ce projet est un prototype fonctionnel. Les améliorations prévues incluent :

- [ ] **Tests unitaires** — Ajout de tests automatisés
      handlers CQRS et la logique de permissions
- [ ] **Health Checks** — Endpoints de monitoring pour la DB, S3 et l'API Gemini
- [ ] **CI/CD** — Pipeline GitHub Actions pour build, test et déploiement 
      automatique vers AWS App Runner
