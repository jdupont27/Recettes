#!/bin/bash
# ============================================================
# setup-ec2.sh — Déploiement initial de l'app sur EC2
#
# Appelé automatiquement par aws-setup.sh OU manuellement :
#   bash deploy/setup-ec2.sh \
#     --ip 54.89.53.91 \
#     --key recetteapp-key.pem \
#     --repo https://github.com/user/recettes.git \
#     --db-password MON_MOT_DE_PASSE \
#     --db-root MON_ROOT_PASSWORD \
#     --gemini-key MA_CLE_GEMINI \
#     --beta-code 12345 \
#     --bucket recetteapp-images-xxx \
#     --access-key AKIAXXXXXXX \
#     --secret-key xxxxxxxxxxx
# ============================================================
set -e

# ── Lecture des arguments ──────────────────────────────────
SERVER_IP=""
KEY_FILE=""
REPO_URL=""
DB_PASSWORD=""
DB_ROOT_PASSWORD=""
GEMINI_API_KEY=""
BETA_CODE_INVITATION=""
AWS_BUCKET_NAME=""
AWS_REGION="us-east-1"
AWS_ACCESS_KEY_ID=""
AWS_SECRET_ACCESS_KEY=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --ip)           SERVER_IP="$2";               shift 2 ;;
        --key)          KEY_FILE="$2";                shift 2 ;;
        --repo)         REPO_URL="$2";                shift 2 ;;
        --db-password)  DB_PASSWORD="$2";             shift 2 ;;
        --db-root)      DB_ROOT_PASSWORD="$2";        shift 2 ;;
        --gemini-key)   GEMINI_API_KEY="$2";          shift 2 ;;
        --beta-code)    BETA_CODE_INVITATION="$2";    shift 2 ;;
        --bucket)       AWS_BUCKET_NAME="$2";         shift 2 ;;
        --region)       AWS_REGION="$2";              shift 2 ;;
        --access-key)   AWS_ACCESS_KEY_ID="$2";       shift 2 ;;
        --secret-key)   AWS_SECRET_ACCESS_KEY="$2";   shift 2 ;;
        *) echo "Argument inconnu : $1"; exit 1 ;;
    esac
done

SSH_OPTS="-i ${KEY_FILE} -o StrictHostKeyChecking=no -o ConnectTimeout=10 -o ServerAliveInterval=60 -o ServerAliveCountMax=10"
SSH_CMD="ssh $SSH_OPTS ec2-user@${SERVER_IP}"

# ── Attendre que le SSH soit disponible ───────────────────
echo "Attente de la disponibilité du serveur SSH..."
SSH_READY=0
for i in $(seq 1 40); do
    if $SSH_CMD "echo ok" > /dev/null 2>&1; then
        echo "    Serveur accessible !"
        SSH_READY=1
        break
    fi
    echo "    Tentative $i/40..."
    sleep 10
done
if [ "$SSH_READY" -eq 0 ]; then
    echo "ERREUR : Le serveur SSH n'est pas accessible après 400 secondes."
    exit 1
fi

# ── Attendre que Docker soit installé (user-data) ─────────
echo "Attente de Docker (installation automatique en cours)..."
DOCKER_READY=0
for i in $(seq 1 30); do
    if $SSH_CMD "docker info > /dev/null 2>&1" 2>/dev/null; then
        echo "    Docker est prêt !"
        DOCKER_READY=1
        break
    fi
    echo "    Tentative $i/30..."
    sleep 15
done
if [ "$DOCKER_READY" -eq 0 ]; then
    echo "ERREUR : Docker n'est pas prêt après 450 secondes."
    exit 1
fi

# ── Cloner le dépôt ───────────────────────────────────────
echo "Clonage du dépôt..."
$SSH_CMD "sudo mkdir -p /opt/recetteapp && sudo chown ec2-user:ec2-user /opt/recetteapp"
if $SSH_CMD "[ -d /opt/recetteapp/.git ]"; then
    $SSH_CMD "cd /opt/recetteapp && git pull"
else
    $SSH_CMD "git clone '$REPO_URL' /opt/recetteapp"
fi

# ── Créer le fichier .env ─────────────────────────────────
echo "Création du fichier .env..."
$SSH_CMD "cat > /opt/recetteapp/.env" <<EOF
DB_PASSWORD=${DB_PASSWORD}
DB_ROOT_PASSWORD=${DB_ROOT_PASSWORD}
GEMINI_API_KEY=${GEMINI_API_KEY}
BETA_CODE_INVITATION=${BETA_CODE_INVITATION}
AWS_BUCKET_NAME=${AWS_BUCKET_NAME}
AWS_REGION=${AWS_REGION}
AWS_ACCESS_KEY_ID=${AWS_ACCESS_KEY_ID}
AWS_SECRET_ACCESS_KEY=${AWS_SECRET_ACCESS_KEY}
EOF
$SSH_CMD "chmod 600 /opt/recetteapp/.env"

# ── Lancer l'application ──────────────────────────────────
echo "Démarrage de l'application..."
$SSH_CMD "cd /opt/recetteapp && docker-compose up -d --build"

echo ""
echo "====================================================="
echo " Déploiement terminé !"
echo "====================================================="
echo " http://${SERVER_IP}"
echo ""
echo " Voir les logs : ssh $SSH_OPTS ec2-user@${SERVER_IP}"
echo "                 cd /opt/recetteapp && docker-compose logs -f app"
echo "====================================================="
