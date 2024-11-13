﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(DBContext))]
    partial class DBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.DocumentManagement.Correspondent.Correspondent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Document_count")
                        .HasColumnType("integer");

                    b.Property<List<string>>("GroupsChange")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("GroupsView")
                        .HasColumnType("text[]");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("Is_insensitive")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("Last_correspondence")
                        .HasColumnType("timestamp with time zone");

                    b.Property<List<string>>("Match")
                        .HasColumnType("text[]");

                    b.Property<int>("Matching_algorithm")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<List<string>>("UsersChange")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("UsersView")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.ToTable("Correspondents");
                });

            modelBuilder.Entity("Domain.DocumentManagement.CustomFields.CustomField", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Data_type")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("CustomFields");
                });

            modelBuilder.Entity("Domain.DocumentManagement.CustomFields.DocumentCustomField", b =>
                {
                    b.Property<Guid>("DocumentId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CustomFieldId")
                        .HasColumnType("uuid");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("DocumentId", "CustomFieldId");

                    b.HasIndex("CustomFieldId");

                    b.ToTable("DocumentCustomField");
                });

            modelBuilder.Entity("Domain.DocumentManagement.DocumentNote.DocumentNote", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("DocumentId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Note")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("User")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DocumentId");

                    b.ToTable("DocumentNotes");
                });

            modelBuilder.Entity("Domain.DocumentManagement.DocumentTypes.DocumentType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Document_count")
                        .HasColumnType("integer");

                    b.Property<List<Guid>>("ExtractedData")
                        .HasColumnType("uuid[]");

                    b.Property<List<string>>("GroupsChange")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("GroupsView")
                        .HasColumnType("text[]");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("Is_insensitive")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<List<string>>("Match")
                        .HasColumnType("text[]");

                    b.Property<int>("Matching_algorithm")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<List<string>>("UsersChange")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("UsersView")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.ToTable("DocumentTypes");
                });

            modelBuilder.Entity("Domain.DocumentManagement.StoragePath.StoragePath", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Document_count")
                        .HasColumnType("integer");

                    b.Property<List<string>>("GroupsChange")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("GroupsView")
                        .HasColumnType("text[]");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("Is_insensitive")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<List<string>>("Match")
                        .HasColumnType("text[]");

                    b.Property<int>("Matching_algorithm")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Owner")
                        .HasColumnType("text");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<List<string>>("UsersChange")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("UsersView")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.ToTable("StoragePaths");
                });

            modelBuilder.Entity("Domain.DocumentManagement.Views.View", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("TagId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("TagId");

                    b.ToTable("Views");
                });

            modelBuilder.Entity("Domain.DocumentManagement.tags.DocumentTags", b =>
                {
                    b.Property<Guid>("DocumentId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TagId")
                        .HasColumnType("uuid");

                    b.HasKey("DocumentId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("DocumentTags");
                });

            modelBuilder.Entity("Domain.DocumentManagement.tags.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Document_count")
                        .HasColumnType("integer");

                    b.Property<List<string>>("GroupsChange")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("GroupsView")
                        .HasColumnType("text[]");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("Is_inbox")
                        .HasColumnType("boolean");

                    b.Property<bool>("Is_insensitive")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<List<string>>("Match")
                        .HasColumnType("text[]");

                    b.Property<int>("Matching_algorithm")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<List<string>>("UsersChange")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("UsersView")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Domain.Documents.ArchiveSerialNumbers", b =>
                {
                    b.Property<Guid>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DocumentCount")
                        .HasColumnType("integer");

                    b.Property<string>("GroupName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Prefix")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateOnly>("Year")
                        .HasColumnType("date");

                    b.HasKey("GroupId");

                    b.ToTable("ArchiveSerialNumbers");
                });

            modelBuilder.Entity("Domain.Documents.Document", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Archive_Serial_Number")
                        .HasColumnType("text");

                    b.Property<string>("Base64Data")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Checksum")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("CorrespondentId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DocumentTypeId")
                        .HasColumnType("uuid");

                    b.Property<string>("FileData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("GroupId")
                        .HasColumnType("uuid");

                    b.Property<List<string>>("GroupsChange")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("GroupsView")
                        .HasColumnType("text[]");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<byte[]>("Iv")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<byte[]>("Key")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("Lang")
                        .HasColumnType("text");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Mailrule")
                        .HasColumnType("text");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Owner")
                        .HasColumnType("text");

                    b.Property<int>("Source")
                        .HasColumnType("integer");

                    b.Property<Guid?>("StoragePathId")
                        .HasColumnType("uuid");

                    b.Property<string>("ThumbnailUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<List<string>>("UsersChange")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("UsersView")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.HasIndex("CorrespondentId");

                    b.HasIndex("DocumentTypeId");

                    b.HasIndex("StoragePathId");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Domain.FileTasks.FileTasks", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Source")
                        .HasColumnType("integer");

                    b.Property<Guid>("Task_documentId")
                        .HasColumnType("uuid");

                    b.Property<int>("Task_problem")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Task_documentId");

                    b.ToTable("FileTasks");
                });

            modelBuilder.Entity("Domain.Logs.Logs", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Message")
                        .HasColumnType("text");

                    b.Property<int>("Source")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("Domain.Templates.Template", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AssignCorrespondent")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AssignDocumentType")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AssignStoragePath")
                        .HasColumnType("uuid");

                    b.Property<List<Guid>>("AssignTags")
                        .HasColumnType("uuid[]");

                    b.Property<string>("AssignTitle")
                        .HasColumnType("text");

                    b.Property<List<string>>("Assign_change_groups")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("Assign_change_users")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("Assign_view_groups")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("Assign_view_users")
                        .HasColumnType("text[]");

                    b.Property<int?>("Content_matching_algorithm")
                        .HasColumnType("integer");

                    b.Property<List<string>>("Content_matching_pattern")
                        .HasColumnType("text[]");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DocumentClassification")
                        .HasColumnType("text");

                    b.Property<string>("FilterFilename")
                        .HasColumnType("text");

                    b.Property<string>("FilterMailrule")
                        .HasColumnType("text");

                    b.Property<string>("FilterPath")
                        .HasColumnType("text");

                    b.Property<Guid?>("Has_Correspondent")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("Has_Document_Type")
                        .HasColumnType("uuid");

                    b.Property<List<Guid>>("Has_Tags")
                        .HasColumnType("uuid[]");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool?>("Is_Enabled")
                        .HasColumnType("boolean");

                    b.Property<bool?>("Is_Insensitive")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<string>("Owner")
                        .HasColumnType("text");

                    b.Property<int[]>("Sources")
                        .HasColumnType("integer[]");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("Domain.DocumentManagement.CustomFields.DocumentCustomField", b =>
                {
                    b.HasOne("Domain.DocumentManagement.CustomFields.CustomField", "CustomField")
                        .WithMany("DocumentsCustomFields")
                        .HasForeignKey("CustomFieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Documents.Document", "Document")
                        .WithMany("DocumentsCustomFields")
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CustomField");

                    b.Navigation("Document");
                });

            modelBuilder.Entity("Domain.DocumentManagement.DocumentNote.DocumentNote", b =>
                {
                    b.HasOne("Domain.Documents.Document", "Document")
                        .WithMany("Notes")
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Document");
                });

            modelBuilder.Entity("Domain.DocumentManagement.Views.View", b =>
                {
                    b.HasOne("Domain.DocumentManagement.tags.Tag", "Tag")
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("Domain.DocumentManagement.tags.DocumentTags", b =>
                {
                    b.HasOne("Domain.Documents.Document", "Document")
                        .WithMany("Tags")
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.DocumentManagement.tags.Tag", "Tag")
                        .WithMany("Documents")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Document");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("Domain.Documents.Document", b =>
                {
                    b.HasOne("Domain.DocumentManagement.Correspondent.Correspondent", "Correspondent")
                        .WithMany("Documents")
                        .HasForeignKey("CorrespondentId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Domain.DocumentManagement.DocumentTypes.DocumentType", "Document_Type")
                        .WithMany("Documents")
                        .HasForeignKey("DocumentTypeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Domain.DocumentManagement.StoragePath.StoragePath", "StoragePath")
                        .WithMany("Documents")
                        .HasForeignKey("StoragePathId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Correspondent");

                    b.Navigation("Document_Type");

                    b.Navigation("StoragePath");
                });

            modelBuilder.Entity("Domain.FileTasks.FileTasks", b =>
                {
                    b.HasOne("Domain.Documents.Document", "Task_document")
                        .WithMany()
                        .HasForeignKey("Task_documentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Task_document");
                });

            modelBuilder.Entity("Domain.DocumentManagement.Correspondent.Correspondent", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("Domain.DocumentManagement.CustomFields.CustomField", b =>
                {
                    b.Navigation("DocumentsCustomFields");
                });

            modelBuilder.Entity("Domain.DocumentManagement.DocumentTypes.DocumentType", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("Domain.DocumentManagement.StoragePath.StoragePath", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("Domain.DocumentManagement.tags.Tag", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("Domain.Documents.Document", b =>
                {
                    b.Navigation("DocumentsCustomFields");

                    b.Navigation("Notes");

                    b.Navigation("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}
