FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RecipeApp.Web/RecipeApp.Web.csproj", "RecipeApp.Web/"]
COPY ["RecipeApp.Application/RecipeApp.Application.csproj", "RecipeApp.Application/"]
COPY ["RecipeApp.Domain/RecipeApp.Domain.csproj", "RecipeApp.Domain/"]
COPY ["RecipeApp.Infrastructure/RecipeApp.Infrastructure.csproj", "RecipeApp.Infrastructure/"]

RUN dotnet restore "RecipeApp.Web/RecipeApp.Web.csproj"

COPY . .
WORKDIR "/src/RecipeApp.Web"
RUN dotnet build "RecipeApp.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RecipeApp.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Créer le dossier uploads
RUN mkdir -p wwwroot/uploads/recettes

ENTRYPOINT ["dotnet", "RecipeApp.Web.dll"]
