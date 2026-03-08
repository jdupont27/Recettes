#!/bin/bash
# ============================================================
# setup-ec2.sh — Initialisation d'une instance EC2 Amazon Linux 2023
# À exécuter UNE SEULE FOIS après la création de l'instance.
#
# Utilisation :
#   chmod +x setup-ec2.sh
#   ./setup-ec2.sh
# ============================================================
set -e

REPO_URL="https://github.com/VOTRE_NOM/VOTRE_REPO.git"  # À modifier
APP_DIR="/opt/recetteapp"

echo "=== Installation de Docker ==="
sudo dnf update -y
sudo dnf install -y docker git
sudo systemctl enable docker
sudo systemctl start docker
sudo usermod -aG docker ec2-user

echo "=== Installation de Docker Compose ==="
COMPOSE_VERSION=$(curl -s https://api.github.com/repos/docker/compose/releases/latest | grep '"tag_name"' | cut -d'"' -f4)
sudo curl -SL "https://github.com/docker/compose/releases/download/${COMPOSE_VERSION}/docker-compose-linux-x86_64" \
    -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

echo "=== Clonage du dépôt ==="
sudo mkdir -p "$APP_DIR"
sudo chown ec2-user:ec2-user "$APP_DIR"
git clone "$REPO_URL" "$APP_DIR"

echo "=== Configuration des variables d'environnement ==="
cat > "$APP_DIR/.env" <<'EOF'
GEMINI_API_KEY=REMPLACER_PAR_VOTRE_CLE
AWS_BUCKET_NAME=REMPLACER_PAR_NOM_DU_BUCKET
AWS_REGION=us-east-1
AWS_ACCESS_KEY_ID=REMPLACER
AWS_SECRET_ACCESS_KEY=REMPLACER
EOF

echo ""
echo ">>> IMPORTANT : Éditez le fichier $APP_DIR/.env avec vos vraies clés avant de continuer !"
echo "    nano $APP_DIR/.env"
echo ""
echo "=== Une fois .env configuré, démarrez l'app avec : ==="
echo "    cd $APP_DIR && docker-compose up -d --build"
