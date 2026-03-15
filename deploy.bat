@echo off
echo --- 1. Construction de l'image ---
docker build -t 571804723503.dkr.ecr.us-east-2.amazonaws.com/portfolio/recettes-app:latest -f RecipeApp.Web/Dockerfile .

echo --- 2. Connexion à ECR ---
aws ecr get-login-password --region us-east-2 | docker login --username AWS --password-stdin 571804723503.dkr.ecr.us-east-2.amazonaws.com

echo --- 3. Envoi vers AWS ---
docker push 571804723503.dkr.ecr.us-east-2.amazonaws.com/portfolio/recettes-app:latest

echo --- TERMINE ! AWS App Runner va maintenant mettre à jour le site. ---
pause