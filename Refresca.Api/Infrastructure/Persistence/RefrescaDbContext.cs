using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Refresca.Api.Domain.Entities;

namespace Refresca.Api.Infrastructure.Persistence;

public sealed class RefrescaDbContext(DbContextOptions<RefrescaDbContext> options)
    : DbContext(options)
{
    public DbSet<Leitura> Leituras => Set<Leitura>();
    public DbSet<PontoRefresca> PontosRefresca => Set<PontoRefresca>();
    public DbSet<PausaHidratacao> PausasHidratacao => Set<PausaHidratacao>();
    public DbSet<ClimaDiario> ClimasDiarios => Set<ClimaDiario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // O SQLite não tem tipo próprio para DateTimeOffset e o EF Core não traduz ORDER BY nem
        // comparações com ele. Guardar como número (ticks) permite ordenar no banco.
        var dataComFuso = new DateTimeOffsetToBinaryConverter();

        modelBuilder.Entity<Leitura>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TotemId).HasMaxLength(50).IsRequired();
            e.Property(x => x.Nivel).HasMaxLength(10).IsRequired();
            e.Property(x => x.MedidoEm).HasConversion(dataComFuso);
            e.HasIndex(x => new { x.TotemId, x.MedidoEm });
        });

        modelBuilder.Entity<PontoRefresca>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Endereco).HasMaxLength(200).IsRequired();
            e.Property(x => x.Horario).HasMaxLength(60);
            e.Property(x => x.TotemId).HasMaxLength(50).IsRequired();
            e.HasMany(x => x.Leituras).WithOne().HasForeignKey(x => x.TotemId)
                .HasPrincipalKey(x => x.TotemId);
        });

        modelBuilder.Entity<PausaHidratacao>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RegistradaEm).HasConversion(dataComFuso);
            e.HasIndex(x => x.TrabalhadorId);
            e.HasOne(x => x.PontoRefresca).WithMany()
                .HasForeignKey(x => x.PontoRefrescaId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ClimaDiario>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Dia).IsUnique();
        });
    }
}
