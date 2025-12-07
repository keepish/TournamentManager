using Microsoft.EntityFrameworkCore;

namespace TournamentManager.Core.Models;

public partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<ParticipantTournamentCategory> ParticipantTournamentCategories { get; set; }

    public virtual DbSet<Tournament> Tournaments { get; set; }

    public virtual DbSet<TournamentCategory> TournamentCategories { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("category");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MaxAge).HasColumnName("maxAge");
            entity.Property(e => e.MaxWeight)
                .HasPrecision(4, 1)
                .HasColumnName("maxWeight");
            entity.Property(e => e.MinAge).HasColumnName("minAge");
            entity.Property(e => e.MinWeight)
                .HasPrecision(4, 1)
                .HasColumnName("minWeight");
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("match");

            entity.HasIndex(e => e.FirstParticipantId, "firstParticipantId_idx");

            entity.HasIndex(e => e.SecondParticipantId, "secondParticipantId_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FirstParticipantId).HasColumnName("firstParticipantId");
            entity.Property(e => e.FirstParticipantScore).HasColumnName("firstParticipantScore");
            entity.Property(e => e.SecondParticipantId).HasColumnName("secondParticipantId");
            entity.Property(e => e.SecondParticipantScore).HasColumnName("secondParticipantScore");

            entity.HasOne(d => d.FirstParticipant).WithMany(p => p.MatchFirstParticipants)
                .HasForeignKey(d => d.FirstParticipantId)
                .HasConstraintName("firstParticipantId");

            entity.HasOne(d => d.SecondParticipant).WithMany(p => p.MatchSecondParticipants)
                .HasForeignKey(d => d.SecondParticipantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("secondParticipantId");
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("participant");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Birthday)
                .HasColumnType("datetime")
                .HasColumnName("birthday");
            entity.Property(e => e.Gender)
                .HasColumnType("bit(1)")
                .HasColumnName("gender");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(200)
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.Surname)
                .HasMaxLength(150)
                .HasColumnName("surname");
            entity.Property(e => e.Weight)
                .HasPrecision(4, 1)
                .HasColumnName("weight");
        });

        modelBuilder.Entity<ParticipantTournamentCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("participant_tournament_category");

            entity.HasIndex(e => e.ParticipantId, "participantId_idx");

            entity.HasIndex(e => e.TournamentCategoryId, "tournamentCategoryId_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ParticipantId).HasColumnName("participantId");
            entity.Property(e => e.TournamentCategoryId).HasColumnName("tournamentCategoryId");

            entity.HasOne(d => d.Participant).WithMany(p => p.ParticipantTournamentCategories)
                .HasForeignKey(d => d.ParticipantId)
                .HasConstraintName("participantId");

            entity.HasOne(d => d.TournamentCategory).WithMany(p => p.ParticipantTournamentCategories)
                .HasForeignKey(d => d.TournamentCategoryId)
                .HasConstraintName("tournamentCategoryId");
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tournament");

            entity.HasIndex(e => e.OrganizerId, "id_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(150)
                .HasColumnName("address");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("endDate");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.OrganizerId).HasColumnName("organizerId");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("startDate");

            entity.HasOne(d => d.Organizer).WithMany(p => p.Tournaments)
                .HasForeignKey(d => d.OrganizerId)
                .HasConstraintName("organizerId");
        });

        modelBuilder.Entity<TournamentCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tournament_category");

            entity.HasIndex(e => e.CategoryId, "id_idx");

            entity.HasIndex(e => e.TournamentId, "id_idx1");

            entity.HasIndex(e => e.JudgeId, "judgeId_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("categoryId");
            entity.Property(e => e.JudgeId).HasColumnName("judgeId");
            entity.Property(e => e.SitesNumber).HasColumnName("sitesNumber");
            entity.Property(e => e.TournamentId).HasColumnName("tournamentId");

            entity.HasOne(d => d.Category).WithMany(p => p.TournamentCategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("categoryId");

            entity.HasOne(d => d.Judge).WithMany(p => p.TournamentCategories)
                .HasForeignKey(d => d.JudgeId)
                .HasConstraintName("judgeId");

            entity.HasOne(d => d.Tournament).WithMany(p => p.TournamentCategories)
                .HasForeignKey(d => d.TournamentId)
                .HasConstraintName("tournamentId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login)
                .HasMaxLength(150)
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(1000)
                .HasColumnName("passwordHash");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(200)
                .HasColumnName("patronymic");
            entity.Property(e => e.Surname)
                .HasMaxLength(150)
                .HasColumnName("surname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var dbUser = Environment.GetEnvironmentVariable("DB_USER");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (!string.IsNullOrWhiteSpace(dbName) && !string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbPassword))
            {
                var cs = $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword}";
                optionsBuilder.UseMySQL(cs);
            }
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
