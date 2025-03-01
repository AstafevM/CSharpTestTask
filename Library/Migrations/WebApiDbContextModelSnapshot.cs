﻿// <auto-generated />
using System;
using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Library.Migrations
{
    [DbContext(typeof(WebApiDbContext))]
    partial class WebApiDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Library.Entities.CsvDataEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("ExecutionTime")
                        .HasColumnType("integer");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Value")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.ToTable("Values", (string)null);
                });

            modelBuilder.Entity("Library.Entities.IntegralResultEntity", b =>
                {
                    b.Property<string>("FileName")
                        .HasColumnType("text");

                    b.Property<double>("AverageExecutionTime")
                        .HasColumnType("double precision");

                    b.Property<double>("AverageValue")
                        .HasColumnType("double precision");

                    b.Property<double>("MaxValue")
                        .HasColumnType("double precision");

                    b.Property<double>("MedianValue")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("MinDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double>("MinValue")
                        .HasColumnType("double precision");

                    b.Property<int>("TimeDelta")
                        .HasColumnType("integer");

                    b.HasKey("FileName");

                    b.ToTable("Results", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
