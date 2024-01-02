﻿// <auto-generated />
using System;
using MediaFinder.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MediaFinder.DataAccessLayer.Migrations
{
    [DbContext(typeof(MediaFinderDbContext))]
    [Migration("20240102224437_AddingDiscoveryRunsConfigIdAndWorkingDirectoryFields")]
    partial class AddingDiscoveryRunsConfigIdAndWorkingDirectoryFields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("MediaFinder.DataAccessLayer.Models.DiscoveryExecution", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("ConfigurationId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("StartDateTime")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WorkingDirectory")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ConfigurationId");

                    b.ToTable("Runs");
                });

            modelBuilder.Entity("MediaFinder.DataAccessLayer.Models.FileDetails", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("Created")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Extracted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("FileSize")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FileType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MD5_Hash")
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<string>("ParentPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RelativePath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SHA256_Hash")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("SHA512_Hash")
                        .HasMaxLength(512)
                        .HasColumnType("TEXT");

                    b.Property<bool>("ShouldExport")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("FileDetails");
                });

            modelBuilder.Entity("MediaFinder.DataAccessLayer.Models.FileProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("FileDetailsId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FileDetailsId");

                    b.ToTable("FileProperties");
                });

            modelBuilder.Entity("MediaFinder.DataAccessLayer.Models.SearchDirectory", b =>
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

                    b.ToTable("SearchDirectories");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Path = "C:\\Users\\User\\Pictures",
                            SettingsId = 1
                        },
                        new
                        {
                            Id = 2,
                            Path = "C:\\TEMP\\Source",
                            SettingsId = 2
                        });
                });

            modelBuilder.Entity("MediaFinder.DataAccessLayer.Models.SearchSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("ExtractArchives")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ExtractionDepth")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("MinImageHeight")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("MinImageWidth")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("MinVideoHeight")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("MinVideoWidth")
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

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ExtractArchives = false,
                            Name = "Default",
                            PerformDeepAnalysis = false,
                            Recursive = true
                        },
                        new
                        {
                            Id = 2,
                            ExtractArchives = true,
                            ExtractionDepth = 5,
                            MinImageHeight = 200L,
                            MinImageWidth = 200L,
                            MinVideoHeight = 300L,
                            MinVideoWidth = 600L,
                            Name = "Testing",
                            PerformDeepAnalysis = true,
                            Recursive = true
                        });
                });

            modelBuilder.Entity("MediaFinder.DataAccessLayer.Models.DiscoveryExecution", b =>
                {
                    b.HasOne("MediaFinder.DataAccessLayer.Models.SearchSettings", "Configuration")
                        .WithMany()
                        .HasForeignKey("ConfigurationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Configuration");
                });

            modelBuilder.Entity("MediaFinder.DataAccessLayer.Models.FileProperty", b =>
                {
                    b.HasOne("MediaFinder.DataAccessLayer.Models.FileDetails", "FileDetails")
                        .WithMany("FileProperties")
                        .HasForeignKey("FileDetailsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FileDetails");
                });

            modelBuilder.Entity("MediaFinder.DataAccessLayer.Models.SearchDirectory", b =>
                {
                    b.HasOne("MediaFinder.DataAccessLayer.Models.SearchSettings", "Settings")
                        .WithMany("Directories")
                        .HasForeignKey("SettingsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Settings");
                });

            modelBuilder.Entity("MediaFinder.DataAccessLayer.Models.FileDetails", b =>
                {
                    b.Navigation("FileProperties");
                });

            modelBuilder.Entity("MediaFinder.DataAccessLayer.Models.SearchSettings", b =>
                {
                    b.Navigation("Directories");
                });
#pragma warning restore 612, 618
        }
    }
}
