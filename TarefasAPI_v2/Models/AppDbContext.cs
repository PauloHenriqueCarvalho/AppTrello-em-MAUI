using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TarefasAPI_v2.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Board> Boards { get; set; }

    public virtual DbSet<Coluna> Colunas { get; set; }

    public virtual DbSet<Tarefa> Tarefas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=TarefasDB_v2;User id=sa;Password=1234;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Board__3214EC07688C47B2");

            entity.ToTable("Board");

            entity.Property(e => e.DataCadastro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Nome).HasMaxLength(150);

            entity.HasOne(d => d.UsuarioCriador).WithMany(p => p.Boards)
                .HasForeignKey(d => d.UsuarioCriadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Board_Usuario");

            entity.HasMany(d => d.Usuarios).WithMany(p => p.BoardsNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "BoardUsuario",
                    r => r.HasOne<Usuario>().WithMany()
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BoardUsuario_Usuario"),
                    l => l.HasOne<Board>().WithMany()
                        .HasForeignKey("BoardId")
                        .HasConstraintName("FK_BoardUsuario_Board"),
                    j =>
                    {
                        j.HasKey("BoardId", "UsuarioId");
                        j.ToTable("BoardUsuario");
                    });
        });

        modelBuilder.Entity<Coluna>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Coluna__3214EC07A96FA72A");

            entity.ToTable("Coluna");

            entity.Property(e => e.Cor)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasDefaultValue("#333333");
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Nome).HasMaxLength(150);

            entity.HasOne(d => d.Board).WithMany(p => p.Colunas)
                .HasForeignKey(d => d.BoardId)
                .HasConstraintName("FK_Coluna_Board");
        });

        modelBuilder.Entity<Tarefa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tarefa__3214EC0761C7B327");

            entity.ToTable("Tarefa");

            entity.HasIndex(e => e.BoardId, "IX_Tarefa_BoardId");

            entity.HasIndex(e => e.ColunaId, "IX_Tarefa_ColunaId");

            entity.Property(e => e.Arquivada)
                .HasDefaultValue(false)
                .HasColumnName("arquivada");
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Titulo).HasMaxLength(200);

            entity.HasOne(d => d.Board).WithMany(p => p.Tarefas)
                .HasForeignKey(d => d.BoardId)
                .HasConstraintName("FK_Tarefa_Board");

            entity.HasOne(d => d.Coluna).WithMany(p => p.Tarefas)
                .HasForeignKey(d => d.ColunaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tarefa_Coluna");

            entity.HasOne(d => d.UsuarioCriador).WithMany(p => p.Tarefas)
                .HasForeignKey(d => d.UsuarioCriadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tarefa_UsuarioCriador");

            entity.HasMany(d => d.Usuarios).WithMany(p => p.TarefasNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "TarefaUsuario",
                    r => r.HasOne<Usuario>().WithMany()
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TarefaUsuario_Usuario"),
                    l => l.HasOne<Tarefa>().WithMany()
                        .HasForeignKey("TarefaId")
                        .HasConstraintName("FK_TarefaUsuario_Tarefa"),
                    j =>
                    {
                        j.HasKey("TarefaId", "UsuarioId");
                        j.ToTable("TarefaUsuario");
                        j.HasIndex(new[] { "UsuarioId" }, "IX_TarefaUsuario_UsuarioId");
                    });
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC0794A8B52A");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Login, "UQ__Usuario__5E55825BA2E691C7").IsUnique();

            entity.Property(e => e.DataCadastro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Login).HasMaxLength(150);
            entity.Property(e => e.Nome).HasMaxLength(150);
            entity.Property(e => e.SenhaHash).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
