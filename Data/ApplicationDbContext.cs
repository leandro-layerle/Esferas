using Esferas.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Esferas.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
             : base(options)
        {
        }

        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Encuesta> Encuestas { get; set; }
        public DbSet<Pregunta> Preguntas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Respuesta> Respuestas { get; set; }
        public DbSet<LinkUnico> LinksUnicos { get; set; }
        public DbSet<Resultado> Resultados { get; set; }
        public DbSet<InformeGenerado> InformesGenerados { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración relacional con Fluent API

            modelBuilder.Entity<Categoria>()
                .HasOne(c => c.CategoriaPadre)
                .WithMany(c => c.Subcategorias)
                .HasForeignKey(c => c.CategoriaPadreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Encuesta>()
                .HasOne(e => e.Empresa)
                .WithMany(emp => emp.Encuestas)
                .HasForeignKey(e => e.EmpresaId);

            modelBuilder.Entity<Pregunta>()
                .HasOne(p => p.Encuesta)
                .WithMany(e => e.Preguntas)
                .HasForeignKey(p => p.EncuestaId);

            modelBuilder.Entity<Pregunta>()
                .HasOne(p => p.CategoriaPrimaria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaPrimariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pregunta>()
                .HasOne(p => p.CategoriaSecundaria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaSecundariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pregunta>()
                .HasOne(p => p.CategoriaTerciaria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaTerciariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Respuesta>()
                .HasOne(r => r.Pregunta)
                .WithMany(p => p.Respuestas)
                .HasForeignKey(r => r.PreguntaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Respuesta>()
                .HasOne(r => r.LinkUnico)
                .WithMany()
                .HasForeignKey(r => r.LinkUnicoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Respuesta>()
                .HasOne(r => r.Encuesta)
                .WithMany()
                .HasForeignKey(r => r.EncuestaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LinkUnico>()
                .HasOne(l => l.Encuesta)
                .WithMany(e => e.Links)
                .HasForeignKey(l => l.EncuestaId);

            modelBuilder.Entity<LinkUnico>(b =>
            {
                b.HasIndex(x => x.Token).IsUnique();
                b.Property(x => x.Token)
                 .HasDefaultValueSql("NEWID()"); // default al insertar
            });

            modelBuilder.Entity<LinkUnico>()
                .Property(l => l.EsLinkEmpresa)
                .HasDefaultValue(false);

            modelBuilder.Entity<Resultado>()
                .HasOne(r => r.Encuesta)
                .WithMany()
                .HasForeignKey(r => r.EncuestaId);

            modelBuilder.Entity<Resultado>()
                .HasOne(r => r.Categoria)
                .WithMany()
                .HasForeignKey(r => r.CategoriaId);

            modelBuilder.Entity<InformeGenerado>().ToTable("InformesGenerados");

            modelBuilder.Entity<InformeGenerado>().HasKey(i => i.Id);

            modelBuilder.Entity<InformeGenerado>().Property(i => i.Token)
                .IsRequired();

            modelBuilder.Entity<InformeGenerado>().HasIndex(i => new { i.EncuestaId, i.Token })
                .IsUnique();

            modelBuilder.Entity<InformeGenerado>().Property(i => i.ContenidoHtml)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<InformeGenerado>().Property(i => i.FechaGeneracion)
                .IsRequired();

            modelBuilder.Entity<InformeGenerado>().HasOne(i => i.Encuesta)
                .WithMany() // si más adelante querés agregar una colección en Encuesta, podés cambiarlo a .WithMany(e => e.Informes)
                .HasForeignKey(i => i.EncuestaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InformeGenerado>().Property(i => i.FechaExpiracion)
                .IsRequired(false);
        }
    }
}
