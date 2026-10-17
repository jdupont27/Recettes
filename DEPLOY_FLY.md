# Déploiement sur Fly.io

Migration depuis AWS App Runner — la base de données MySQL reste sur **Aiven** (aucune migration de données), et les photos restent sur **AWS S3** (`recettesbucket`, aucune migration non plus). Seul le conteneur applicatif change d'hébergeur.

## Prérequis (déjà faits une fois)

- Compte Fly.io créé
- CLI `flyctl` installé : `winget install --id=Fly-io.flyctl -e`
- Connecté : `flyctl auth login` — ou avec un jeton : `export FLY_API_TOKEN=...`

Si `flyctl` n'est pas reconnu directement après l'installation, ferme/rouvre ton terminal, ou utilise le chemin complet :
```
C:\Users\<toi>\AppData\Local\Microsoft\WinGet\Packages\Fly-io.flyctl_Microsoft.Winget.Source_8wekyb3d8bbwe\flyctl.exe
```

## Ce qui existe déjà pour `recetteapp`

- **App** Fly créée (`fly apps create recetteapp`)
- **Volume persistant** de 1 Go nommé `recetteapp_dataprotection`, monté sur `/root/.aspnet/DataProtection-Keys` (région `yyz` — Toronto) — essentiel pour que les utilisateurs restent connectés après un redéploiement
- **Secrets** déjà configurés : connexion Aiven, clé Gemini, identifiants AWS S3, code bêta (voir liste complète ci-dessous)

## Redéployer après une modification du code

```bash
cd C:\Repos\Recettes
flyctl deploy --app recetteapp
```
Reconstruit l'image Docker (`RecipeApp.Web/Dockerfile`) et remplace la version en ligne, sans toucher au volume des clés ni à la base Aiven. Compte 2-3 minutes (compilation .NET).

## Secrets configurés

| Variable | Contenu |
|---|---|
| `ConnectionStrings__DefaultConnection` | Connexion MySQL Aiven (host, port, `defaultdb`, `avnadmin`, SSL requis) |
| `Gemini__ApiKey`, `Gemini__Model`, `Gemini__VideoModel` | Accès API Gemini |
| `AWS__BucketName`, `AWS__Region` | Bucket S3 `recettesbucket`, région `us-east-2` |
| `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` | Identifiants IAM pour S3 (pas de rôle IAM type EC2 sur Fly — obligatoire ici) |
| `Beta__CodeInvitation` | Code d'invitation bêta |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

Pour changer une valeur existante **ou en ajouter une nouvelle** — même commande dans les deux cas :
```bash
flyctl secrets set --app recetteapp "NomDeLaVariable=valeur"
```
Si le nom existe déjà, Fly la remplace ; sinon, il l'ajoute. Ça redémarre automatiquement l'app pour que le changement prenne effet (quelques secondes d'interruption).

Convention de nommage : `__` (double underscore) correspond à une section imbriquée dans la config .NET — ex. `Gemini__ApiKey` = `Gemini:ApiKey` dans `appsettings.json`. Pour qu'une nouvelle variable serve à quelque chose, le code doit déjà la lire quelque part via `configuration["Section:Cle"]` — ajouter le secret seul ne suffit pas si le code ne la consulte pas.

Penser aussi à ajouter la nouvelle variable à `.env.example` (documentation pour le dev local) et, si elle est sensible, à l'`.env` local.

## Commandes utiles

```bash
# Voir les logs en direct
flyctl logs --app recetteapp

# Voir les secrets configurés (noms seulement, jamais les valeurs)
flyctl secrets list --app recetteapp

# Se connecter en SSH dans le conteneur (dépannage)
flyctl ssh console --app recetteapp

# Statut de la machine
flyctl status --app recetteapp
```

## Pourquoi un volume seulement pour DataProtection-Keys

Le dossier `wwwroot/uploads` du `docker-compose.yml` local n'est **pas** utilisé en production : `RecipeApp.Infrastructure/DependencyInjection.cs` bascule automatiquement sur `ServiceFichiersS3` dès que `AWS:BucketName` est configuré (c'est le cas). Seules les clés de chiffrement ASP.NET (cookies d'authentification, jetons anti-falsification) doivent survivre aux redémarrages — d'où le volume dédié à `/root/.aspnet/DataProtection-Keys` seulement.

## Bascule finale depuis AWS App Runner

Une fois le déploiement Fly.io validé (connexion, création de recette, upload de photo, extraction Gemini) :
1. Rediriger le trafic réel vers l'URL Fly.io (`recetteapp.fly.dev`, ou un domaine personnalisé si tu en ajoutes un plus tard)
2. Arrêter le service AWS App Runner pour cesser la facturation
3. Les identifiants AWS (S3) et la base Aiven restent utilisés tels quels — rien à changer de ce côté
