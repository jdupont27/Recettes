#!/bin/bash
# ============================================================
# deploy.sh — Mise à jour et redéploiement de l'application
# À exécuter depuis /opt/recetteapp sur l'instance EC2 pour
# appliquer les nouvelles modifications du dépôt git.
#
# Utilisation :
#   cd /opt/recetteapp
#   ./deploy/deploy.sh
# ============================================================
set -e

APP_DIR="/opt/recetteapp"
cd "$APP_DIR"

echo "=== Récupération des dernières modifications ==="
git pull origin master

echo "=== Reconstruction et redémarrage des conteneurs ==="
docker-compose down
docker-compose up -d --build

echo "=== Nettoyage des images inutilisées ==="
docker image prune -f

echo ""
echo "=== Déploiement terminé ! ==="
docker-compose ps
