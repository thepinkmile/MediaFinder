﻿// <auto-generated />
using MediaFinder_v2.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MediaFinder_v2.DataAccessLayer.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230930215925_FixedNameTypoInFileDetails")]
    partial class FixedNameTypoInFileDetails
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("MediaFinder_v2.DataAccessLayer.Models.AppSettingValue", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("AppSettings");
                });

            modelBuilder.Entity("MediaFinder_v2.DataAccessLayer.Models.FileDetails", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("FileSize")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FileType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MD5_Hash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ParentPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SHA256_Hash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SHA512_Hash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("ShouldExport")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("FileDetails");
                });

            modelBuilder.Entity("MediaFinder_v2.DataAccessLayer.Models.SearchDirectory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SettingsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SettingsId");

                    b.ToTable("SearchDirectory");
                });

            modelBuilder.Entity("MediaFinder_v2.DataAccessLayer.Models.SearchSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("ExtractArchives")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("PerformDeepAnalysis")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Recursive")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("SearchSettings");
                });

            modelBuilder.Entity("MediaFinder_v2.DataAccessLayer.Models.SearchDirectory", b =>
                {
                    b.HasOne("MediaFinder_v2.DataAccessLayer.Models.SearchSettings", "Settings")
                        .WithMany("Directories")
                        .HasForeignKey("SettingsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Settings");
                });

            modelBuilder.Entity("MediaFinder_v2.DataAccessLayer.Models.SearchSettings", b =>
                {
                    b.Navigation("Directories");
                });
#pragma warning restore 612, 618
        }
    }
}
