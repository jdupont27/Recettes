#!/bin/bash
# ============================================================
# aws-teardown.sh — Suppression de toute l'infrastructure AWS
#
# Utilisation :
#   bash deploy/aws-teardown.sh
# ============================================================
set -e

# ── Configuration — remplissez avec les valeurs du aws-setup.sh ──
INSTANCE_ID=""        # ex: i-0123456789abcdef0
BUCKET_NAME=""        # ex: recetteapp-images-1234567890
REGION="us-east-1"
IAM_USER="jdupont27"
SG_NAME="recetteapp-sg"
KEY_NAME="recetteapp-key"
POLICY_NAME="recetteapp-s3-policy"
# ──────────────────────────────────────────────────────────────

if [ -z "$INSTANCE_ID" ] || [ -z "$BUCKET_NAME" ]; then
    echo "ERREUR : Remplissez INSTANCE_ID et BUCKET_NAME en haut du script."
    exit 1
fi

echo ""
echo "====================================================="
echo " RecetteApp — Suppression de l'infrastructure AWS"
echo "====================================================="
echo ""

# ── 1. Elastic IP ─────────────────────────────────────────
echo "[1/6] Libération de l'Elastic IP..."
ALLOCATION_ID=$(aws ec2 describe-addresses \
    --filters "Name=instance-id,Values=$INSTANCE_ID" \
    --region "$REGION" \
    --query 'Addresses[0].AllocationId' --output text 2>/dev/null || echo "None")

if [ "$ALLOCATION_ID" != "None" ] && [ -n "$ALLOCATION_ID" ]; then
    ASSOCIATION_ID=$(aws ec2 describe-addresses \
        --allocation-ids "$ALLOCATION_ID" \
        --region "$REGION" \
        --query 'Addresses[0].AssociationId' --output text)
    aws ec2 disassociate-address --association-id "$ASSOCIATION_ID" --region "$REGION" 2>/dev/null || true
    aws ec2 release-address --allocation-id "$ALLOCATION_ID" --region "$REGION"
    echo "    Elastic IP libérée."
else
    echo "    Aucune Elastic IP trouvée."
fi

# ── 2. Instance EC2 ───────────────────────────────────────
echo "[2/6] Suppression de l'instance EC2 : $INSTANCE_ID"
aws ec2 terminate-instances --instance-ids "$INSTANCE_ID" --region "$REGION" > /dev/null
echo "    Attente de la fin de l'instance..."
aws ec2 wait instance-terminated --instance-ids "$INSTANCE_ID" --region "$REGION"
echo "    Instance supprimée."

# ── 3. Bucket S3 ──────────────────────────────────────────
echo "[3/6] Suppression du bucket S3 : $BUCKET_NAME"
aws s3 rm "s3://${BUCKET_NAME}" --recursive 2>/dev/null || true
aws s3api delete-bucket --bucket "$BUCKET_NAME" --region "$REGION"
echo "    Bucket supprimé."

# ── 4. Security Group ─────────────────────────────────────
echo "[4/6] Suppression du Security Group : $SG_NAME"
SG_ID=$(aws ec2 describe-security-groups \
    --filters "Name=group-name,Values=${SG_NAME}" \
    --region "$REGION" \
    --query 'SecurityGroups[0].GroupId' --output text 2>/dev/null || echo "None")

if [ "$SG_ID" != "None" ] && [ -n "$SG_ID" ]; then
    aws ec2 delete-security-group --group-id "$SG_ID" --region "$REGION"
    echo "    Security Group supprimé."
else
    echo "    Security Group introuvable."
fi

# ── 5. Paire de clés EC2 ──────────────────────────────────
echo "[5/6] Suppression de la paire de clés : $KEY_NAME"
aws ec2 delete-key-pair --key-name "$KEY_NAME" --region "$REGION" 2>/dev/null || true
echo "    Paire de clés supprimée."

# ── 6. Politique IAM ──────────────────────────────────────
echo "[6/6] Suppression de la politique IAM : $POLICY_NAME"
POLICY_ARN=$(aws iam list-policies --scope Local \
    --query "Policies[?PolicyName=='${POLICY_NAME}'].Arn" --output text 2>/dev/null || echo "")

if [ -n "$POLICY_ARN" ]; then
    aws iam detach-user-policy --user-name "$IAM_USER" --policy-arn "$POLICY_ARN" 2>/dev/null || true
    aws iam delete-policy --policy-arn "$POLICY_ARN"
    echo "    Politique IAM supprimée."
else
    echo "    Politique IAM introuvable."
fi

echo ""
echo "====================================================="
echo " Infrastructure supprimée avec succès !"
echo "====================================================="
echo ""
echo " N'oubliez pas de supprimer manuellement :"
echo "   - Le fichier ${KEY_NAME}.pem sur votre ordinateur"
echo "   - Les clés d'accès IAM de $IAM_USER si elles ne servent plus"
echo "====================================================="
