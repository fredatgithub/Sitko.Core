﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Sitko.Core.Apps.Blazor.Data;

namespace Sitko.Core.Apps.Blazor.Migrations
{
    [DbContext(typeof(BarContext))]
    [Migration("20210624054559_AddStorageItemsToBar")]
    partial class AddStorageItemsToBar
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Sitko.Core.Apps.Blazor.Data.Entities.BarModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Bar")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("StorageItem")
                        .HasColumnType("jsonb")
                        .HasColumnName("StorageItem");

                    b.Property<string>("StorageItems")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("StorageItems");

                    b.HasKey("Id");

                    b.ToTable("Bars");
                });

            modelBuilder.Entity("Sitko.Core.Apps.Blazor.Data.Entities.FooModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("BarModelId")
                        .HasColumnType("uuid");

                    b.Property<string>("Foo")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("BarModelId");

                    b.ToTable("Foos");
                });

            modelBuilder.Entity("Sitko.Core.Apps.Blazor.Data.Entities.FooModel", b =>
                {
                    b.HasOne("Sitko.Core.Apps.Blazor.Data.Entities.BarModel", null)
                        .WithMany("Foos")
                        .HasForeignKey("BarModelId");
                });

            modelBuilder.Entity("Sitko.Core.Apps.Blazor.Data.Entities.BarModel", b =>
                {
                    b.Navigation("Foos");
                });
#pragma warning restore 612, 618
        }
    }
}
