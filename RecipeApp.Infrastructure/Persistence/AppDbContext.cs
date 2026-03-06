using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Domain.Entities;
using RecipeApp.Infrastructure.Identity;

namespace RecipeApp.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Recette> Recettes => Set<Recette>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<EtapeRecette> EtapesRecette => Set<EtapeRecette>();
    public DbSet<PartageRecette> PartagesRecette => Set<PartageRecette>();
    public DbSet<Categorie> Categories => Set<Categorie>();
    public DbSet<RecetteCategorie> RecetteCategories => Set<RecetteCategorie>();
    public DbSet<Etiquette> Etiquettes => Set<Etiquette>();
    public DbSet<RecetteEtiquette> RecetteEtiquettes => Set<RecetteEtiquette>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Renommer les tables Identity en français
        modelBuilder.Entity<ApplicationUser>().ToTable("Utilisateurs");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UtilisateursRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UtilisateursClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UtilisateursConnexions");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RolesClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UtilisateursTokens");

        // Configuration de Recette
        modelBuilder.Entity<Recette>(entite =>
        {
            entite.HasKey(r => r.Id);
            entite.Property(r => r.Titre).IsRequired().HasMaxLength(200);
            entite.Property(r => r.Description).HasMaxLength(2000);
            entite.Property(r => r.TypeCuisine).HasMaxLength(100);
            entite.Property(r => r.Visibilite).HasConversion<int>();
            entite.Property(r => r.Difficulte).HasConversion<int>();
        });

        // Configuration de Ingredient
        modelBuilder.Entity<Ingredient>(entite =>
        {
            entite.HasKey(i => i.Id);
            entite.Property(i => i.Nom).IsRequired().HasMaxLength(200);
            entite.Property(i => i.Quantite).HasPrecision(10, 3);
            entite.Property(i => i.Unite).HasConversion<int>();
            entite.HasOne(i => i.Recette)
                  .WithMany(r => r.Ingredients)
                  .HasForeignKey(i => i.RecetteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de EtapeRecette
        modelBuilder.Entity<EtapeRecette>(entite =>
        {
            entite.HasKey(e => e.Id);
            entite.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entite.HasOne(e => e.Recette)
                  .WithMany(r => r.Etapes)
                  .HasForeignKey(e => e.RecetteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de PartageRecette (clé composite)
        modelBuilder.Entity<PartageRecette>(entite =>
        {
            entite.HasKey(p => new { p.RecetteId, p.UtilisateurId });
            entite.HasOne(p => p.Recette)
                  .WithMany(r => r.Partages)
                  .HasForeignKey(p => p.RecetteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de RecetteCategorie (clé composite)
        modelBuilder.Entity<RecetteCategorie>(entite =>
        {
            entite.HasKey(rc => new { rc.RecetteId, rc.CategorieId });
            entite.HasOne(rc => rc.Recette)
                  .WithMany(r => r.RecetteCategories)
                  .HasForeignKey(rc => rc.RecetteId)
                  .OnDelete(DeleteBehavior.Cascade);
            entite.HasOne(rc => rc.Categorie)
                  .WithMany(c => c.RecetteCategories)
                  .HasForeignKey(rc => rc.CategorieId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration de RecetteEtiquette (clé composite)
        modelBuilder.Entity<RecetteEtiquette>(entite =>
        {
            entite.HasKey(re => new { re.RecetteId, re.EtiquetteId });
            entite.HasOne(re => re.Recette)
                  .WithMany(r => r.RecetteEtiquettes)
                  .HasForeignKey(re => re.RecetteId)
                  .OnDelete(DeleteBehavior.Cascade);
            entite.HasOne(re => re.Etiquette)
                  .WithMany(e => e.RecetteEtiquettes)
                  .HasForeignKey(re => re.EtiquetteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration de Categorie
        modelBuilder.Entity<Categorie>(entite =>
        {
            entite.HasKey(c => c.Id);
            entite.Property(c => c.Nom).IsRequired().HasMaxLength(100);
            entite.HasIndex(c => c.Nom).IsUnique();
        });

        // Configuration de Etiquette
        modelBuilder.Entity<Etiquette>(entite =>
        {
            entite.HasKey(e => e.Id);
            entite.Property(e => e.Nom).IsRequired().HasMaxLength(100);
            entite.HasIndex(e => e.Nom).IsUnique();
        });
    }
}
