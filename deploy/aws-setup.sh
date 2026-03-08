#!/bin/bash
# ============================================================
# aws-setup.sh — Création de toute l'infrastructure AWS
#
# Prérequis :
#   1. AWS CLI installé : https://aws.amazon.com/cli/
#   2. Configuré avec un compte admin : aws configure
#
# Utilisation :
#   chmod +x aws-setup.sh
#   ./aws-setup.sh
# ============================================================
set -e

# ── Configuration ──────────────────────────────────────────
APP_NAME="recetteapp"
IAM_USER="jdupont27"                            # Utilisateur IAM existant
REGION="us-east-1"
BUCKET_NAME="${APP_NAME}-images-$(date +%s)"   # Nom unique garanti
EC2_KEY_NAME="${APP_NAME}-key"
INSTANCE_TYPE="t2.micro"                        # Free Tier
AMI_ID=""   # Récupéré automatiquement ci-dessous (Amazon Linux 2023 la plus récente)
# ───────────────────────────────────────────────────────────

echo ""
echo "====================================================="
echo " RecetteApp — Création de l'infrastructure AWS"
echo "====================================================="
echo ""

# Récupérer l'AMI Amazon Linux 2023 la plus récente automatiquement
AMI_ID=$(aws ec2 describe-images \
    --region "$REGION" \
    --owners amazon \
    --filters "Name=name,Values=al2023-ami-*-x86_64" "Name=state,Values=available" \
    --query 'sort_by(Images, &CreationDate)[-1].ImageId' \
    --output text)
echo "AMI détectée : $AMI_ID"

# ── 1. Bucket S3 ──────────────────────────────────────────
echo "[1/5] Création du bucket S3 : $BUCKET_NAME"

aws s3api create-bucket \
    --bucket "$BUCKET_NAME" \
    --region "$REGION" \
    --create-bucket-configuration LocationConstraint="$REGION" 2>/dev/null \
    || aws s3api create-bucket --bucket "$BUCKET_NAME" --region "$REGION"

# Désactiver le blocage des accès publics (images publiques)
aws s3api put-public-access-block \
    --bucket "$BUCKET_NAME" \
    --public-access-block-configuration \
        "BlockPublicAcls=false,IgnorePublicAcls=false,BlockPublicPolicy=false,RestrictPublicBuckets=false"

# Politique de lecture publique
aws s3api put-bucket-policy --bucket "$BUCKET_NAME" --policy "{
  \"Version\": \"2012-10-17\",
  \"Statement\": [{
    \"Sid\": \"PublicReadGetObject\",
    \"Effect\": \"Allow\",
    \"Principal\": \"*\",
    \"Action\": \"s3:GetObject\",
    \"Resource\": \"arn:aws:s3:::${BUCKET_NAME}/*\"
  }]
}"

echo "    Bucket créé : https://${BUCKET_NAME}.s3.amazonaws.com"

# ── 2. Politique IAM S3 ───────────────────────────────────
echo "[2/5] Attachement de la politique S3 à l'utilisateur : ${IAM_USER}"

# Politique S3 limitée au bucket
POLICY_ARN=$(aws iam create-policy \
    --policy-name "${APP_NAME}-s3-policy" \
    --policy-document "{
      \"Version\": \"2012-10-17\",
      \"Statement\": [{
        \"Effect\": \"Allow\",
        \"Action\": [\"s3:PutObject\", \"s3:DeleteObject\", \"s3:GetObject\"],
        \"Resource\": \"arn:aws:s3:::${BUCKET_NAME}/*\"
      }]
    }" \
    --query 'Policy.Arn' --output text 2>/dev/null \
    || aws iam list-policies --scope Local --query "Policies[?PolicyName=='${APP_NAME}-s3-policy'].Arn" --output text)

aws iam attach-user-policy \
    --user-name "${IAM_USER}" \
    --policy-arn "$POLICY_ARN"

# Générer les clés d'accès
echo "[3/5] Génération des clés d'accès IAM"
KEYS=$(aws iam create-access-key --user-name "${IAM_USER}")
ACCESS_KEY=$(echo "$KEYS" | grep 'AccessKeyId' | awk -F'"' '{print $4}')
SECRET_KEY=$(echo "$KEYS" | grep 'SecretAccessKey' | awk -F'"' '{print $4}')

# ── 3. Paire de clés EC2 ──────────────────────────────────
echo "[4/5] Création de la paire de clés EC2"
aws ec2 create-key-pair \
    --key-name "$EC2_KEY_NAME" \
    --region "$REGION" \
    --query 'KeyMaterial' \
    --output text > "${EC2_KEY_NAME}.pem"
chmod 400 "${EC2_KEY_NAME}.pem"
echo "    Clé privée sauvegardée : ${EC2_KEY_NAME}.pem  ← GARDEZ CE FICHIER EN SÉCURITÉ"

# ── 4. Security Group ─────────────────────────────────────
echo "[5/5] Création du Security Group"
SG_ID=$(aws ec2 create-security-group \
    --group-name "${APP_NAME}-sg" \
    --description "RecetteApp - HTTP + SSH" \
    --region "$REGION" \
    --query 'GroupId' --output text)

aws ec2 authorize-security-group-ingress --group-id "$SG_ID" --region "$REGION" \
    --ip-permissions \
        "IpProtocol=tcp,FromPort=22,ToPort=22,IpRanges=[{CidrIp=0.0.0.0/0}]" \
        "IpProtocol=tcp,FromPort=80,ToPort=80,IpRanges=[{CidrIp=0.0.0.0/0}]"

# ── 5. Instance EC2 ───────────────────────────────────────
echo "[6/6] Lancement de l'instance EC2 (t2.micro — Free Tier)"

# Script d'initialisation automatique au démarrage de l'instance
USER_DATA=$(cat <<USERDATA
#!/bin/bash
dnf update -y
dnf install -y docker git
systemctl enable docker
systemctl start docker
usermod -aG docker ec2-user
COMPOSE_VERSION=\$(curl -s https://api.github.com/repos/docker/compose/releases/latest | grep '"tag_name"' | cut -d'"' -f4)
curl -SL "https://github.com/docker/compose/releases/download/\${COMPOSE_VERSION}/docker-compose-linux-x86_64" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
USERDATA
)

INSTANCE_ID=$(aws ec2 run-instances \
    --image-id "$AMI_ID" \
    --instance-type "$INSTANCE_TYPE" \
    --key-name "$EC2_KEY_NAME" \
    --security-group-ids "$SG_ID" \
    --region "$REGION" \
    --user-data "$USER_DATA" \
    --tag-specifications "ResourceType=instance,Tags=[{Key=Name,Value=${APP_NAME}}]" \
    --query 'Instances[0].InstanceId' \
    --output text)

echo "    Instance lancée : $INSTANCE_ID"
echo "    Attente du démarrage..."
aws ec2 wait instance-running --instance-ids "$INSTANCE_ID" --region "$REGION"

# ── Elastic IP (IP fixe gratuite) ─────────────────────────
echo "    Allocation d'une Elastic IP..."
ALLOCATION_ID=$(aws ec2 allocate-address \
    --domain vpc \
    --region "$REGION" \
    --query 'AllocationId' --output text)

aws ec2 associate-address \
    --instance-id "$INSTANCE_ID" \
    --allocation-id "$ALLOCATION_ID" \
    --region "$REGION" > /dev/null

PUBLIC_IP=$(aws ec2 describe-addresses \
    --allocation-ids "$ALLOCATION_ID" \
    --region "$REGION" \
    --query 'Addresses[0].PublicIp' \
    --output text)

# ── Résumé ────────────────────────────────────────────────
echo ""
echo "====================================================="
echo " Infrastructure créée avec succès !"
echo "====================================================="
echo ""
echo " IP fixe (Elastic)  : $PUBLIC_IP"
echo " DNS public         : ec2-$(echo $PUBLIC_IP | tr '.' '-').compute-1.amazonaws.com"
echo " Bucket S3          : $BUCKET_NAME"
echo " Clé EC2            : ${EC2_KEY_NAME}.pem"
echo ""
echo " Prochaine étape — connectez-vous et déployez :"
echo ""
echo "   ssh -i ${EC2_KEY_NAME}.pem ec2-user@${PUBLIC_IP}"
echo ""
echo "   # Sur le serveur :"
echo "   git clone VOTRE_REPO /opt/recetteapp"
echo "   cd /opt/recetteapp"
echo "   cat > .env <<EOF"
echo "   DB_PASSWORD=choisir-un-mot-de-passe-fort"
echo "   DB_ROOT_PASSWORD=choisir-un-mot-de-passe-root-fort"
echo "   GEMINI_API_KEY=votre-cle-gemini"
echo "   BETA_CODE_INVITATION=votre-code-invitation"
echo "   AWS_BUCKET_NAME=${BUCKET_NAME}"
echo "   AWS_REGION=${REGION}"
echo "   AWS_ACCESS_KEY_ID=${ACCESS_KEY}"
echo "   AWS_SECRET_ACCESS_KEY=${SECRET_KEY}"
echo "   EOF"
echo "   docker-compose up -d --build"
echo ""
echo " Application accessible sur : http://${PUBLIC_IP}"
echo " (ou) http://ec2-$(echo $PUBLIC_IP | tr '.' '-').compute-1.amazonaws.com"
echo ""
echo "====================================================="
echo " IMPORTANT : Sauvegardez ces informations !"
echo "====================================================="
echo " AWS_ACCESS_KEY_ID     = $ACCESS_KEY"
echo " AWS_SECRET_ACCESS_KEY = $SECRET_KEY"
echo " AWS_BUCKET_NAME       = $BUCKET_NAME"
echo "====================================================="
