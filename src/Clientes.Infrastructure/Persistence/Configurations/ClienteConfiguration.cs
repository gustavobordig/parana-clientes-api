using Clientes.Domain.Entities;
using Clientes.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clientes.Infrastructure.Persistence.Configurations;

internal sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Nome)
            .HasMaxLength(Cliente.NomeTamanhoMaximo)
            .IsRequired();

        builder.Property(c => c.Cpf)
            .HasConversion(cpf => cpf.Valor, valor => Cpf.Criar(valor))
            .HasColumnType("char(11)")
            .IsRequired();

        builder.HasIndex(c => c.Cpf).IsUnique();

        builder.Property(c => c.Email)
            .HasConversion(email => email.Valor, valor => Email.Criar(valor))
            .HasMaxLength(Email.TamanhoMaximo)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(c => c.DataNascimento).HasColumnType("date");
        builder.Property(c => c.RendaMensal).HasPrecision(18, 2);
        builder.Property(c => c.CriadoEm);
        builder.HasIndex(c => c.CriadoEm);
    }
}
