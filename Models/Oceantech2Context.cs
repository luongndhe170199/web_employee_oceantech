using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OceanTechLevel1.Models;

public partial class Oceantech2Context : DbContext
{
    public Oceantech2Context()
    {
    }

    public Oceantech2Context(DbContextOptions<Oceantech2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Commune> Communes { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Ethnicity> Ethnicities { get; set; }

    public virtual DbSet<Occupation> Occupations { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Commune>(entity =>
        {
            entity.HasKey(e => e.CommuneId).HasName("PK__Commune__31E0EB30A20D332B");

            entity.ToTable("Commune");

            entity.Property(e => e.CommuneId).HasColumnName("commune_id");
            entity.Property(e => e.CommuneName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("commune_name");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");

            entity.HasOne(d => d.District).WithMany(p => p.Communes)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("FK__Commune__distric__4E88ABD4");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.DistrictId).HasName("PK__District__2521322B6E27834D");

            entity.ToTable("District");

            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("district_name");
            entity.Property(e => e.ProvinceId).HasColumnName("province_id");

            entity.HasOne(d => d.Province).WithMany(p => p.Districts)
                .HasForeignKey(d => d.ProvinceId)
                .HasConstraintName("FK__District__provin__4BAC3F29");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3213E83FA703DE79");

            entity.ToTable("Employee");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.BirthDate)
                .HasColumnType("date")
                .HasColumnName("birth_date");
            entity.Property(e => e.CitizenId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("citizen_id");
            entity.Property(e => e.CommuneId).HasColumnName("commune_id");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.EthnicityId).HasColumnName("ethnicity_id");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("full_name");
            entity.Property(e => e.MoreInfo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("more_info");
            entity.Property(e => e.OccupationId).HasColumnName("occupation_id");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.PositionId).HasColumnName("position_id");
            entity.Property(e => e.ProvinceId).HasColumnName("province_id");

            entity.HasOne(d => d.Commune).WithMany(p => p.Employees)
                .HasForeignKey(d => d.CommuneId)
                .HasConstraintName("FK__Employee__commun__5BE2A6F2");

            entity.HasOne(d => d.District).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("FK__Employee__distri__5AEE82B9");

            entity.HasOne(d => d.Ethnicity).WithMany(p => p.Employees)
                .HasForeignKey(d => d.EthnicityId)
                .HasConstraintName("FK__Employee__ethnic__571DF1D5");

            entity.HasOne(d => d.Occupation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.OccupationId)
                .HasConstraintName("FK__Employee__occupa__5812160E");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PositionId)
                .HasConstraintName("FK__Employee__positi__59063A47");

            entity.HasOne(d => d.Province).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ProvinceId)
                .HasConstraintName("FK__Employee__provin__59FA5E80");
        });

        modelBuilder.Entity<Ethnicity>(entity =>
        {
            entity.HasKey(e => e.EthnicityId).HasName("PK__Ethnicit__68127C8C20E6D764");

            entity.ToTable("Ethnicity");

            entity.Property(e => e.EthnicityId).HasColumnName("ethnicity_id");
            entity.Property(e => e.EthnicityName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ethnicity_name");
        });

        modelBuilder.Entity<Occupation>(entity =>
        {
            entity.HasKey(e => e.OccupationId).HasName("PK__Occupati__5DA13705058E9EF6");

            entity.ToTable("Occupation");

            entity.Property(e => e.OccupationId).HasColumnName("occupation_id");
            entity.Property(e => e.OccupationName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("occupation_name");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PK__Position__99A0E7A4BD19999E");

            entity.ToTable("Position");

            entity.Property(e => e.PositionId).HasColumnName("position_id");
            entity.Property(e => e.PositionName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("position_name");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.ProvinceId).HasName("PK__Province__08DCB60F601A1E0A");

            entity.ToTable("Province");

            entity.Property(e => e.ProvinceId).HasColumnName("province_id");
            entity.Property(e => e.ProvinceName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("province_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
