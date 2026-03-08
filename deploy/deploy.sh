#!/bin/bash
# ============================================================
# deploy.sh — Mise à jour rapide de l'app (après le 1er déploiement)
#
# Utilisation depuis votre poste local :
#   bash deploy/deploy.sh --ip 54.89.53.91 --key recetteapp-key.pem
#
# Ou directement sur le serveur :
#   cd /opt/recetteapp && bash deploy/deploy.sh
# ============================================================
set -e

SERVER_IP=""
KEY_FILE=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --ip)  SERVER_IP="$2"; shift 2 ;;
        --key) KEY_FILE="$2";  shift 2 ;;
        *) echo "Argument inconnu : $1"; exit 1 ;;
    esac
done

run() {
    if [ -n "$SERVER_IP" ]; then
        ssh -i "$KEY_FILE" -o StrictHostKeyChecking=no "ec2-user@${SERVER_IP}" "$@"
    else
        bash -c "$*"
    fi
}

echo "=== Récupération des modifications ==="
run "cd /opt/recetteapp && git pull origin master"

echo "=== Redémarrage des conteneurs ==="
run "cd /opt/recetteapp && docker-compose down && docker-compose up -d --build"

echo "=== Nettoyage ==="
run "docker image prune -f"

echo ""
echo "=== Déploiement terminé ! ==="
run "cd /opt/recetteapp && docker-compose ps"
