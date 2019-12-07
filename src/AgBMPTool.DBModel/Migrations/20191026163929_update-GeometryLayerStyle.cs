using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace AgBMPTool.DBModel.Migrations
{
    public partial class updateGeometryLayerStyle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "AnimalType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BMPCombinationType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BMPCombinationType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BMPEffectivenessLocationType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BMPEffectivenessLocationType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Farm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farm", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeometryLayerStyle",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    layername = table.Column<string>(nullable: true),
                    type = table.Column<string>(nullable: true),
                    style = table.Column<string>(nullable: true),
                    pointsize = table.Column<string>(nullable: true),
                    color = table.Column<string>(nullable: true),
                    simplelinewidth = table.Column<string>(nullable: true),
                    outlinecolor = table.Column<string>(nullable: true),
                    outlinewidth = table.Column<string>(nullable: true),
                    outlinestyle = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeometryLayerStyle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Investor",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegalSubDivision",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Meridian = table.Column<short>(nullable: false),
                    Range = table.Column<short>(nullable: false),
                    Township = table.Column<short>(nullable: false),
                    Section = table.Column<short>(nullable: false),
                    Quarter = table.Column<string>(nullable: true),
                    LSD = table.Column<short>(nullable: false),
                    FullDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalSubDivision", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModelComponentType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    IsStructure = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelComponentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Municipality",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipality", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationConstraintValueType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationConstraintValueType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parcel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Meridian = table.Column<short>(nullable: false),
                    Range = table.Column<short>(nullable: false),
                    Township = table.Column<short>(nullable: false),
                    Section = table.Column<short>(nullable: false),
                    Quarter = table.Column<string>(nullable: true),
                    FullDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSpatialUnitType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSpatialUnitType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioModelResultVariableType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioModelResultVariableType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioResultSummarizationType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioResultSummarizationType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    IsBaseLine = table.Column<bool>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    UnitSymbol = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Watershed",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Area = table.Column<decimal>(nullable: false),
                    OutletReachId = table.Column<int>(nullable: false),
                    Modified = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Watershed", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Province",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    Code = table.Column<string>(maxLength: 2, nullable: true),
                    CountryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Province", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Province_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BMPType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    ModelComponentTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BMPType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BMPType_ModelComponentType_ModelComponentTypeId",
                        column: x => x.ModelComponentTypeId,
                        principalTable: "ModelComponentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioModelResultType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    UnitTypeId = table.Column<int>(nullable: false),
                    ModelComponentTypeId = table.Column<int>(nullable: false),
                    ScenarioModelResultVariableTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioModelResultType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioModelResultType_ScenarioModelResultVariableType_Sce~",
                        column: x => x.ScenarioModelResultVariableTypeId,
                        principalTable: "ScenarioModelResultVariableType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScenarioModelResultType_UnitType_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalTable: "UnitType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelComponent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    WatershedId = table.Column<int>(nullable: false),
                    ModelComponentTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelComponent_ModelComponentType_ModelComponentTypeId",
                        column: x => x.ModelComponentTypeId,
                        principalTable: "ModelComponentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModelComponent_Watershed_WatershedId",
                        column: x => x.WatershedId,
                        principalTable: "Watershed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scenario",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    WatershedId = table.Column<int>(nullable: false),
                    ScenarioTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scenario_ScenarioType_ScenarioTypeId",
                        column: x => x.ScenarioTypeId,
                        principalTable: "ScenarioType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scenario_Watershed_WatershedId",
                        column: x => x.WatershedId,
                        principalTable: "Watershed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubWatershed",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Area = table.Column<decimal>(nullable: false),
                    Modified = table.Column<DateTimeOffset>(nullable: false),
                    WatershedId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubWatershed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubWatershed_Watershed_WatershedId",
                        column: x => x.WatershedId,
                        principalTable: "Watershed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserName = table.Column<string>(nullable: true),
                    NormalizedUserName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    NormalizedEmail = table.Column<string>(nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    Address1 = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    Municipality = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    ProvinceId = table.Column<int>(nullable: false),
                    DateOfBirth = table.Column<DateTime>(nullable: true),
                    TaxRollNumber = table.Column<string>(nullable: true),
                    DriverLicense = table.Column<string>(nullable: true),
                    LastFourDigitOfSIN = table.Column<string>(maxLength: 4, nullable: true),
                    Organization = table.Column<string>(nullable: true),
                    LastModified = table.Column<DateTime>(nullable: false),
                    UserTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Province_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Province",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_UserType_UserTypeId",
                        column: x => x.UserTypeId,
                        principalTable: "UserType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BMPCombinationBMPTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    BMPCombinationTypeId = table.Column<int>(nullable: false),
                    BMPTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BMPCombinationBMPTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BMPCombinationBMPTypes_BMPCombinationType_BMPCombinationTyp~",
                        column: x => x.BMPCombinationTypeId,
                        principalTable: "BMPCombinationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BMPCombinationBMPTypes_BMPType_BMPTypeId",
                        column: x => x.BMPTypeId,
                        principalTable: "BMPType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BMPEffectivenessType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false),
                    ScenarioModelResultTypeId = table.Column<int>(nullable: true),
                    UnitTypeId = table.Column<int>(nullable: false),
                    ScenarioModelResultVariableTypeId = table.Column<int>(nullable: true),
                    DefaultWeight = table.Column<int>(nullable: false),
                    DefaultConstraintTypeId = table.Column<int>(nullable: true),
                    DefaultConstraint = table.Column<decimal>(nullable: true),
                    BMPEffectivenessLocationTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BMPEffectivenessType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BMPEffectivenessType_BMPEffectivenessLocationType_BMPEffect~",
                        column: x => x.BMPEffectivenessLocationTypeId,
                        principalTable: "BMPEffectivenessLocationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BMPEffectivenessType_OptimizationConstraintValueType_Defaul~",
                        column: x => x.DefaultConstraintTypeId,
                        principalTable: "OptimizationConstraintValueType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BMPEffectivenessType_ScenarioModelResultType_ScenarioModelR~",
                        column: x => x.ScenarioModelResultTypeId,
                        principalTable: "ScenarioModelResultType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BMPEffectivenessType_ScenarioModelResultVariableType_Scenar~",
                        column: x => x.ScenarioModelResultVariableTypeId,
                        principalTable: "ScenarioModelResultVariableType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BMPEffectivenessType_UnitType_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalTable: "UnitType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelComponentBMPTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    BMPTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelComponentBMPTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelComponentBMPTypes_BMPType_BMPTypeId",
                        column: x => x.BMPTypeId,
                        principalTable: "BMPType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModelComponentBMPTypes_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatershedExistingBMPType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    ScenarioTypeId = table.Column<int>(nullable: false),
                    BMPTypeId = table.Column<int>(nullable: false),
                    InvestorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatershedExistingBMPType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatershedExistingBMPType_BMPType_BMPTypeId",
                        column: x => x.BMPTypeId,
                        principalTable: "BMPType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatershedExistingBMPType_Investor_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "Investor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatershedExistingBMPType_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatershedExistingBMPType_ScenarioType_ScenarioTypeId",
                        column: x => x.ScenarioTypeId,
                        principalTable: "ScenarioType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioModelResult",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ScenarioId = table.Column<int>(nullable: false),
                    ModelComponentId = table.Column<int>(nullable: false),
                    ScenarioModelResultTypeId = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Value = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioModelResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioModelResult_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScenarioModelResult_Scenario_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScenarioModelResult_ScenarioModelResultType_ScenarioModelRe~",
                        column: x => x.ScenarioModelResultTypeId,
                        principalTable: "ScenarioModelResultType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitScenario",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    ScenarioId = table.Column<int>(nullable: false),
                    BMPCombinationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitScenario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitScenario_BMPCombinationType_BMPCombinationId",
                        column: x => x.BMPCombinationId,
                        principalTable: "BMPCombinationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnitScenario_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnitScenario_Scenario_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subbasin",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    SubWatershedId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subbasin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subbasin_SubWatershed_SubWatershedId",
                        column: x => x.SubWatershedId,
                        principalTable: "SubWatershed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    StartYear = table.Column<int>(nullable: false),
                    EndYear = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ScenarioTypeId = table.Column<int>(nullable: false),
                    ProjectSpatialUnitTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_ProjectSpatialUnitType_ProjectSpatialUnitTypeId",
                        column: x => x.ProjectSpatialUnitTypeId,
                        principalTable: "ProjectSpatialUnitType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Project_ScenarioType_ScenarioTypeId",
                        column: x => x.ScenarioTypeId,
                        principalTable: "ScenarioType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Project_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMunicipalities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<int>(nullable: false),
                    MunicipalityId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMunicipalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMunicipalities_Municipality_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "Municipality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMunicipalities_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserParcels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<int>(nullable: false),
                    ParcelId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserParcels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserParcels_Parcel_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserParcels_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWatersheds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<int>(nullable: false),
                    WatershedId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWatersheds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWatersheds_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWatersheds_Watershed_WatershedId",
                        column: x => x.WatershedId,
                        principalTable: "Watershed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitScenarioEffectiveness",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UnitScenarioId = table.Column<int>(nullable: false),
                    BMPEffectivenessTypeId = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Value = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitScenarioEffectiveness", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitScenarioEffectiveness_BMPEffectivenessType_BMPEffective~",
                        column: x => x.BMPEffectivenessTypeId,
                        principalTable: "BMPEffectivenessType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnitScenarioEffectiveness_UnitScenario_UnitScenarioId",
                        column: x => x.UnitScenarioId,
                        principalTable: "UnitScenario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reach",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubbasinId = table.Column<int>(nullable: false),
                    Geometry = table.Column<MultiLineString>(type: "geometry (multilinestring)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reach", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reach_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reach_Subbasin_SubbasinId",
                        column: x => x.SubbasinId,
                        principalTable: "Subbasin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubArea",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubbasinId = table.Column<int>(nullable: false),
                    LegalSubDivisionId = table.Column<int>(nullable: false),
                    ParcelId = table.Column<int>(nullable: false),
                    Area = table.Column<decimal>(nullable: false),
                    Elevation = table.Column<decimal>(nullable: false),
                    Slope = table.Column<decimal>(nullable: false),
                    LandUse = table.Column<string>(nullable: true),
                    SoilTexture = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubArea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubArea_LegalSubDivision_LegalSubDivisionId",
                        column: x => x.LegalSubDivisionId,
                        principalTable: "LegalSubDivision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubArea_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubArea_Parcel_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubArea_Subbasin_SubbasinId",
                        column: x => x.SubbasinId,
                        principalTable: "Subbasin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Optimization",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ProjectId = table.Column<int>(nullable: false),
                    OptimizationTypeId = table.Column<int>(nullable: false),
                    BudgetTarget = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Optimization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Optimization_OptimizationType_OptimizationTypeId",
                        column: x => x.OptimizationTypeId,
                        principalTable: "OptimizationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Optimization_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMunicipalities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ProjectId = table.Column<int>(nullable: false),
                    MunicipalityId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMunicipalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMunicipalities_Municipality_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "Municipality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectMunicipalities_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectWatersheds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ProjectId = table.Column<int>(nullable: false),
                    WatershedId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectWatersheds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectWatersheds_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectWatersheds_Watershed_WatershedId",
                        column: x => x.WatershedId,
                        principalTable: "Watershed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solution",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ProjectId = table.Column<int>(nullable: false),
                    FromOptimization = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solution_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatchBasin",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Area = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(6,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatchBasin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatchBasin_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatchBasin_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatchBasin_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClosedDrain",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPoint>(type: "geometry (multipoint)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosedDrain", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClosedDrain_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClosedDrain_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClosedDrain_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dugout",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Area = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(6,0)", nullable: false),
                    AnimalTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dugout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dugout_AnimalType_AnimalTypeId",
                        column: x => x.AnimalTypeId,
                        principalTable: "AnimalType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dugout_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dugout_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dugout_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feedlot",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    AnimalTypeId = table.Column<int>(nullable: false),
                    AnimalNumber = table.Column<int>(nullable: false),
                    AnimalAdultRatio = table.Column<decimal>(type: "numeric(3,3)", nullable: false),
                    Area = table.Column<decimal>(type: "numeric(5,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedlot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedlot_AnimalType_AnimalTypeId",
                        column: x => x.AnimalTypeId,
                        principalTable: "AnimalType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedlot_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedlot_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedlot_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowDiversion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPoint>(type: "geometry (multipoint)", nullable: true),
                    Length = table.Column<decimal>(type: "numeric(6,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowDiversion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowDiversion_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowDiversion_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowDiversion_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrassedWaterway",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Width = table.Column<decimal>(type: "numeric(5,0)", nullable: false),
                    Length = table.Column<decimal>(type: "numeric(5,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrassedWaterway", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrassedWaterway_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrassedWaterway_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrassedWaterway_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IsolatedWetland",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Area = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(6,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsolatedWetland", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IsolatedWetland_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IsolatedWetland_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IsolatedWetland_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lake",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Area = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(6,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lake", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lake_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lake_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lake_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManureStorage",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPoint>(type: "geometry (multipoint)", nullable: true),
                    Area = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(6,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManureStorage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManureStorage_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManureStorage_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManureStorage_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointSource",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPoint>(type: "geometry (multipoint)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointSource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointSource_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PointSource_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PointSource_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservoir",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Area = table.Column<decimal>(type: "numeric(12,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(12,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservoir", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservoir_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservoir_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservoir_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiparianBuffer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Width = table.Column<decimal>(type: "numeric(5,0)", nullable: false),
                    Length = table.Column<decimal>(type: "numeric(5,0)", nullable: false),
                    Area = table.Column<decimal>(type: "numeric(12,4)", nullable: false),
                    AreaRatio = table.Column<decimal>(type: "numeric(12,0)", nullable: false),
                    DrainageArea = table.Column<Polygon>(type: "geometry (polygon)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiparianBuffer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiparianBuffer_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiparianBuffer_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiparianBuffer_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiparianWetland",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Area = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(6,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiparianWetland", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiparianWetland_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiparianWetland_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiparianWetland_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RockChute",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPoint>(type: "geometry (multipoint)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RockChute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RockChute_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RockChute_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RockChute_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SmallDam",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Area = table.Column<decimal>(type: "numeric(12,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(12,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmallDam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmallDam_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SmallDam_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SmallDam_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VegetativeFilterStrip",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Width = table.Column<decimal>(type: "numeric(5,0)", nullable: false),
                    Length = table.Column<decimal>(type: "numeric(5,0)", nullable: false),
                    Area = table.Column<decimal>(type: "numeric(12,4)", nullable: false),
                    AreaRatio = table.Column<decimal>(type: "numeric(12,0)", nullable: false),
                    DrainageArea = table.Column<Polygon>(type: "geometry (polygon)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VegetativeFilterStrip", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VegetativeFilterStrip_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VegetativeFilterStrip_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VegetativeFilterStrip_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wascob",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ModelComponentId = table.Column<int>(nullable: false),
                    SubAreaId = table.Column<int>(nullable: false),
                    ReachId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    Geometry = table.Column<MultiPolygon>(type: "geometry (multipolygon)", nullable: true),
                    Area = table.Column<decimal>(type: "numeric(12,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(12,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wascob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wascob_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wascob_Reach_ReachId",
                        column: x => x.ReachId,
                        principalTable: "Reach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wascob_SubArea_SubAreaId",
                        column: x => x.SubAreaId,
                        principalTable: "SubArea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationConstraints",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    OptimizationId = table.Column<int>(nullable: false),
                    BMPEffectivenessTypeId = table.Column<int>(nullable: false),
                    OptimizationConstraintValueTypeId = table.Column<int>(nullable: false),
                    Constraint = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationConstraints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptimizationConstraints_BMPEffectivenessType_BMPEffectivene~",
                        column: x => x.BMPEffectivenessTypeId,
                        principalTable: "BMPEffectivenessType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptimizationConstraints_OptimizationConstraintValueType_Opt~",
                        column: x => x.OptimizationConstraintValueTypeId,
                        principalTable: "OptimizationConstraintValueType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptimizationConstraints_Optimization_OptimizationId",
                        column: x => x.OptimizationId,
                        principalTable: "Optimization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationLegalSubDivisions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    OptimizationId = table.Column<int>(nullable: false),
                    BMPTypeId = table.Column<int>(nullable: false),
                    LegalSubDivisionId = table.Column<int>(nullable: false),
                    IsSelected = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationLegalSubDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptimizationLegalSubDivisions_BMPType_BMPTypeId",
                        column: x => x.BMPTypeId,
                        principalTable: "BMPType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptimizationLegalSubDivisions_LegalSubDivision_LegalSubDivi~",
                        column: x => x.LegalSubDivisionId,
                        principalTable: "LegalSubDivision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptimizationLegalSubDivisions_Optimization_OptimizationId",
                        column: x => x.OptimizationId,
                        principalTable: "Optimization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationParcels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    OptimizationId = table.Column<int>(nullable: false),
                    BMPTypeId = table.Column<int>(nullable: false),
                    ParcelId = table.Column<int>(nullable: false),
                    IsSelected = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationParcels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptimizationParcels_BMPType_BMPTypeId",
                        column: x => x.BMPTypeId,
                        principalTable: "BMPType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptimizationParcels_Optimization_OptimizationId",
                        column: x => x.OptimizationId,
                        principalTable: "Optimization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptimizationParcels_Parcel_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationWeights",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    OptimizationId = table.Column<int>(nullable: false),
                    BMPEffectivenessTypeId = table.Column<int>(nullable: false),
                    Weight = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationWeights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptimizationWeights_BMPEffectivenessType_BMPEffectivenessTy~",
                        column: x => x.BMPEffectivenessTypeId,
                        principalTable: "BMPEffectivenessType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptimizationWeights_Optimization_OptimizationId",
                        column: x => x.OptimizationId,
                        principalTable: "Optimization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolutionLegalSubDivisions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    SolutionId = table.Column<int>(nullable: false),
                    BMPTypeId = table.Column<int>(nullable: false),
                    LegalSubDivisionId = table.Column<int>(nullable: false),
                    IsSelected = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolutionLegalSubDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolutionLegalSubDivisions_BMPType_BMPTypeId",
                        column: x => x.BMPTypeId,
                        principalTable: "BMPType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolutionLegalSubDivisions_LegalSubDivision_LegalSubDivision~",
                        column: x => x.LegalSubDivisionId,
                        principalTable: "LegalSubDivision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolutionLegalSubDivisions_Solution_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "Solution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolutionModelComponents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    SolutionId = table.Column<int>(nullable: false),
                    BMPTypeId = table.Column<int>(nullable: false),
                    ModelComponentId = table.Column<int>(nullable: false),
                    IsSelected = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolutionModelComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolutionModelComponents_BMPType_BMPTypeId",
                        column: x => x.BMPTypeId,
                        principalTable: "BMPType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolutionModelComponents_ModelComponent_ModelComponentId",
                        column: x => x.ModelComponentId,
                        principalTable: "ModelComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolutionModelComponents_Solution_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "Solution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolutionParcels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    SolutionId = table.Column<int>(nullable: false),
                    BMPTypeId = table.Column<int>(nullable: false),
                    ParcelId = table.Column<int>(nullable: false),
                    IsSelected = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolutionParcels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolutionParcels_BMPType_BMPTypeId",
                        column: x => x.BMPTypeId,
                        principalTable: "BMPType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolutionParcels_Parcel_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolutionParcels_Solution_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "Solution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AnimalType",
                columns: new[] { "Id", "Description", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 6, "Goat manure", "Goat", 6 },
                    { 7, "Horse manure", "Horse", 7 },
                    { 5, "Sheep manure", "Sheep", 5 },
                    { 4, "Swine manure", "Swine", 4 },
                    { 3, "Cow-Calf manure", "Cow-Calf", 3 },
                    { 2, "Beef manure", "Beef", 2 },
                    { 1, "Dairy manure", "Dairy", 1 },
                    { 8, "Turkey manure", "Turkey", 8 },
                    { 9, "Duck manure", "Duck", 9 }
                });

            migrationBuilder.InsertData(
                table: "BMPCombinationType",
                columns: new[] { "Id", "Description", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 14, "Livestock feedlot ", "FDLT", 14 },
                    { 15, "Manure storage ", "MSCD", 15 },
                    { 16, "Rock chute", "RKCH", 16 },
                    { 17, "Point source ", "PTSR", 17 },
                    { 18, "Manure incorporation with 48h", "MI48H", 18 },
                    { 11, "Closed drain", "CLDR", 11 },
                    { 19, "Manure application setback", "MASB", 19 },
                    { 13, "Manure catch basin/impondment", "MCBI", 13 },
                    { 12, "Dugout", "DGOT", 12 },
                    { 21, "Manure application in spring rather than fall", "SAFA", 21 },
                    { 10, "Water and sediment control basin", "WASCOB", 10 },
                    { 9, "Small dam", "SMDM", 9 },
                    { 1, "Isolated wetland ", "ISWET", 1 },
                    { 2, "Riparian wetland ", "RIWET", 2 },
                    { 3, "Lake ", "LAKE", 3 },
                    { 4, "Vegetative filter strip", "VFST", 4 },
                    { 5, "Riparian buffer", "RIBUF", 5 },
                    { 6, "Grassed waterway", "GWW", 6 },
                    { 20, "No manure application on snow", "NAOS", 20 },
                    { 22, "Manure application based on soil nitrogen limit", "ASNL", 22 },
                    { 27, "Rotational grazing", "ROGZ", 27 },
                    { 24, "Livestock wintering site", "WSMG", 24 },
                    { 40, "Fertilizer management", "FERMG", 40 },
                    { 39, "Irrigation management", "IRRMG", 39 },
                    { 38, "Sustainable use of natural area", "SUNA", 38 },
                    { 37, "Plant species in tame pasture", "PSTPS", 37 },
                    { 36, "Minimum tillage on high slope", "MTHS", 36 },
                    { 35, "Residule management", "RDMG", 35 },
                    { 23, "Manure application based on soil phosphorous limit", "ASPL", 23 },
                    { 33, "Tile drain management", "TLDMG", 33 },
                    { 34, "Terrace", "TERR", 34 },
                    { 31, "Crop rotation", "CRRO", 31 },
                    { 30, "Conservation tillage", "CSTL", 30 },
                    { 29, "Cover crop", "CVCR", 29 },
                    { 28, "Windbreak", "WDBR", 28 },
                    { 7, "Flow diversion", "FLDV", 7 },
                    { 26, "Livestock stream access management", "SAMG", 26 },
                    { 25, "Livestock off-site watering", "OFSW", 25 },
                    { 32, "Forage conversion", "FRCV", 32 },
                    { 8, "Reservoir ", "RESV", 8 }
                });

            migrationBuilder.InsertData(
                table: "BMPEffectivenessLocationType",
                columns: new[] { "Id", "Description", "IsDefault", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Onsite", false, "Onsite", 1 },
                    { 2, "Offsite", true, "Offsite", 2 }
                });

            migrationBuilder.InsertData(
                table: "Country",
                columns: new[] { "Id", "Description", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Canada", "Canada", 1 },
                    { 2, "USA", "USA", 2 }
                });

            migrationBuilder.InsertData(
                table: "GeometryLayerStyle",
                columns: new[] { "Id", "color", "layername", "outlinecolor", "outlinestyle", "outlinewidth", "pointsize", "simplelinewidth", "style", "type" },
                values: new object[,]
                {
                    { 1, "rgb(158, 0, 0, 0.6)", "Parcel", "white", null, "1", null, "", "vertical", "simple-fill" },
                    { 3, "blue", "Reach", "", null, "", null, "4", "", "simple-line" },
                    { 4, "purple", "Farm", "white", null, "1", null, "", "horizontal", "simple-fill" },
                    { 5, "blue", "Municipality", "white", null, "1", null, "", "diagonal-cross", "simple-fill" },
                    { 6, "yellow", "SubWaterShed", "white", null, "1", null, "", "cross", "simple-fill" },
                    { 7, "purple", "WaterShed", "white", null, "1", null, "", "backward-diagonal", "simple-fill" },
                    { 2, "purple", "LSD", "white", null, "1", null, "", "horizontal", "simple-fill" }
                });

            migrationBuilder.InsertData(
                table: "ModelComponentType",
                columns: new[] { "Id", "Description", "IsStructure", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Basic unit of ESAT dataset (intersect between parcel, LSD, and subbasin)", false, "SubArea", 1 },
                    { 2, "Reach", false, "Reach", 2 },
                    { 3, "Isolated wetland", true, "IsolatedWetland", 3 },
                    { 4, "Riparian wetland", true, "RiparianWetland", 4 },
                    { 6, "Vegetative filter strip", true, "VegetativeFilterStrip", 6 },
                    { 7, "Riparian buffer", true, "RiparianBuffer", 7 },
                    { 8, "Grassed waterway", true, "GrassedWaterway", 8 },
                    { 9, "Flow diversion", true, "FlowDiversion", 9 },
                    { 10, "Reservoir", true, "Reservoir", 10 },
                    { 12, "Water and sediment control basin", true, "Wascob", 12 },
                    { 13, "Small water pond for animal drinking", true, "Dugout", 13 },
                    { 14, "Used to control surface runoff from a feeding operation or manure storage facility", true, "CatchBasin", 14 },
                    { 15, "Animal feeding operation with an intensive animal farming ", true, "Feedlot", 15 },
                    { 16, "On-farm manure storage", true, "ManureStorage", 16 },
                    { 17, "A structure that directs flow to a stream ", true, "RockChute", 17 },
                    { 18, "Point source", true, "PointSource", 18 },
                    { 19, "An underground pipe that directs head surface water to a mainstream", true, "ClosedDrain", 19 },
                    { 11, "Small dam", true, "SmallDam", 11 },
                    { 5, "Lake", true, "Lake", 5 }
                });

            migrationBuilder.InsertData(
                table: "OptimizationConstraintValueType",
                columns: new[] { "Id", "Description", "IsDefault", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Percentage", true, "Percentage", 1 },
                    { 2, "Absolute Value", false, "Absolute Value", 2 }
                });

            migrationBuilder.InsertData(
                table: "OptimizationType",
                columns: new[] { "Id", "Description", "IsDefault", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Eco-service", false, "Eco-service", 1 },
                    { 2, "Budget", true, "Budget", 2 }
                });

            migrationBuilder.InsertData(
                table: "ProjectSpatialUnitType",
                columns: new[] { "Id", "Description", "IsDefault", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 2, "Parcel", true, "Parcel", 2 },
                    { 1, "LSD", false, "LSD", 1 }
                });

            migrationBuilder.InsertData(
                table: "ScenarioModelResultVariableType",
                columns: new[] { "Id", "Description", "IsDefault", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 15, "Biodiversity", false, "Biodiversity", 15 },
                    { 14, "Soil carbon", false, "Soil carbon", 14 },
                    { 13, "TP", true, "TP", 13 },
                    { 12, "PP", false, "PP", 12 },
                    { 16, "Outflow", false, "Outflow", 16 },
                    { 11, "DP", false, "DP", 11 },
                    { 10, "TN", false, "TN", 10 },
                    { 9, "PN", false, "PN", 9 },
                    { 8, "DN", false, "DN", 8 },
                    { 3, "Soil moisture", false, "Soil moisture", 3 },
                    { 6, "Runoff", false, "Runoff", 6 },
                    { 5, "Groundwater recharge", false, "Groundwater recharge", 5 },
                    { 4, "ET", false, "ET", 4 },
                    { 2, "Temperature", false, "Temperature", 2 },
                    { 1, "Precipitation", false, "Precipitation", 1 },
                    { 7, "TSS", false, "TSS", 7 }
                });

            migrationBuilder.InsertData(
                table: "ScenarioResultSummarizationType",
                columns: new[] { "Id", "Description", "IsDefault", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 6, "Watershed", false, "Watershed", 6 },
                    { 5, "Subwatershed", false, "Subwatershed", 5 },
                    { 4, "Municipality", false, "Municipality", 4 },
                    { 3, "Farm", false, "Farm", 3 },
                    { 1, "LSD", false, "LSD", 1 },
                    { 2, "Parcel", true, "Parcel", 2 }
                });

            migrationBuilder.InsertData(
                table: "ScenarioType",
                columns: new[] { "Id", "Description", "IsBaseLine", "IsDefault", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Conventional", true, false, "Conventional", 1 },
                    { 2, "Existing", true, true, "Existing", 2 }
                });

            migrationBuilder.InsertData(
                table: "UnitType",
                columns: new[] { "Id", "Description", "Name", "SortOrder", "UnitSymbol" },
                values: new object[,]
                {
                    { 10, "N/P Yield", "N/P Yield", 10, "kg" },
                    { 15, "Unitless", "Unitless", 15, "-" },
                    { 14, "Soil carbon", "Soil carbon", 14, "ton" },
                    { 13, "Volume", "Volume", 13, "m3" },
                    { 12, "Cost", "Cost", 12, "$" },
                    { 11, "Flow", "Flow", 11, "m3/s" },
                    { 9, "TSS Yield", "TSS Yield", 9, "ton" },
                    { 3, "Precipitation", "Precipitation", 3, "mm" },
                    { 7, "Groundwater recharge", "Groundwater recharge", 7, "mm" },
                    { 6, "ET", "ET", 6, "mm" },
                    { 5, "Soil moisture", "Soil moisture", 5, "mm" },
                    { 4, "Temperature", "Temperature", 4, "oC" },
                    { 2, "Percentage", "Percentage", 2, "%" },
                    { 1, "Elevation", "Elevation", 1, "m" },
                    { 8, "Runoff", "Runoff", 8, "mm" }
                });

            migrationBuilder.InsertData(
                table: "UserType",
                columns: new[] { "Id", "Description", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 3, "Municipality Manager", "Municipality Manager", 3 },
                    { 1, "Admin", "Admin", 1 },
                    { 2, "Watershed Manager", "Watershed Manager", 2 },
                    { 4, "Farmer", "Farmer", 4 }
                });

            migrationBuilder.InsertData(
                table: "BMPEffectivenessType",
                columns: new[] { "Id", "BMPEffectivenessLocationTypeId", "DefaultConstraint", "DefaultConstraintTypeId", "DefaultWeight", "Description", "Name", "ScenarioModelResultTypeId", "ScenarioModelResultVariableTypeId", "SortOrder", "UnitTypeId" },
                values: new object[] { 22, 2, null, null, 0, "BMP yearly cost", "BMP cost", null, null, 22, 12 });

            migrationBuilder.InsertData(
                table: "BMPType",
                columns: new[] { "Id", "Description", "ModelComponentTypeId", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 29, "Cover crop", 1, "CVCR", 29 },
                    { 32, "Forage conversion", 1, "FRCV", 32 },
                    { 31, "Crop rotation", 1, "CRRO", 31 },
                    { 30, "Conservation tillage", 1, "CSTL", 30 },
                    { 28, "Windbreak", 1, "WDBR", 28 },
                    { 27, "Rotational grazing", 1, "ROGZ", 27 },
                    { 26, "Livestock stream access management", 1, "SAMG", 26 },
                    { 25, "Livestock off-site watering", 1, "OFSW", 25 },
                    { 24, "Livestock wintering site", 1, "WSMG", 24 },
                    { 23, "Manure application based on soil phosphorous limit", 1, "ASPL", 23 },
                    { 22, "Manure application based on soil nitrogen limit", 1, "ASNL", 22 },
                    { 21, "Manure application in spring rather than fall", 1, "SAFA", 21 },
                    { 20, "No manure application on snow", 1, "NAOS", 20 },
                    { 19, "Manure application setback", 1, "MASB", 19 },
                    { 18, "Manure incorporation with 48h", 1, "MI48H", 18 },
                    { 6, "Grassed waterway", 1, "GWW", 6 },
                    { 5, "Riparian buffer", 1, "RIBUF", 5 },
                    { 4, "Vegetative filter strip", 1, "VFST", 4 },
                    { 34, "Terrace", 1, "TERR", 34 },
                    { 35, "Residule management", 1, "RDMG", 35 },
                    { 33, "Tile drain management", 1, "TLDMG", 33 },
                    { 37, "Plant species in tame pasture", 1, "PSTPS", 37 },
                    { 11, "Closed drain", 19, "CLDR", 11 },
                    { 17, "Point source ", 18, "PTSR", 17 },
                    { 16, "Rock chute", 17, "RKCH", 16 },
                    { 15, "Manure storage ", 16, "MSCD", 15 },
                    { 14, "Livestock feedlot ", 15, "FDLT", 14 },
                    { 13, "Manure catch basin/impondment", 14, "MCBI", 13 },
                    { 12, "Dugout", 13, "DGOT", 12 },
                    { 36, "Minimum tillage on high slope", 1, "MTHS", 36 },
                    { 9, "Small dam", 11, "SMDM", 9 },
                    { 10, "Water and sediment control basin", 12, "WASCOB", 10 },
                    { 7, "Flow diversion", 9, "FLDV", 7 },
                    { 3, "Lake ", 5, "LAKE", 3 },
                    { 2, "Riparian wetland ", 4, "RIWET", 2 },
                    { 1, "Isolated wetland ", 3, "ISWET", 1 },
                    { 40, "Fertilizer management", 1, "FERMG", 40 },
                    { 39, "Irrigation management", 1, "IRRMG", 39 },
                    { 38, "Sustainable use of natural area", 1, "SUNA", 38 },
                    { 8, "Reservoir ", 10, "RESV", 8 }
                });

            migrationBuilder.InsertData(
                table: "Province",
                columns: new[] { "Id", "Code", "CountryId", "Description", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 58, null, 2, "Vermont", "Vermont", 58 },
                    { 62, null, 2, "Wisconsin", "Wisconsin", 62 },
                    { 59, null, 2, "Virginia", "Virginia", 59 },
                    { 60, null, 2, "Washington", "Washington", 60 },
                    { 61, null, 2, "West Virginia", "West Virginia", 61 },
                    { 63, null, 2, "Wyoming", "Wyoming", 63 },
                    { 1, null, 1, "Alberta", "Alberta", 1 },
                    { 65, null, 2, "Puerto Rico", "Puerto Rico", 65 },
                    { 66, null, 2, "U.S. Virgin Islands", "U.S. Virgin Islands", 66 },
                    { 68, null, 2, "Guam", "Guam", 68 },
                    { 69, null, 2, "Northern Mariana Islands", "Northern Mariana Islands", 69 },
                    { 57, null, 2, "Utah", "Utah", 57 },
                    { 64, null, 2, "Washington DC", "Washington DC", 64 },
                    { 56, null, 2, "Texas", "Texas", 56 },
                    { 67, null, 2, "American Samoa", "American Samoa", 67 },
                    { 54, null, 2, "South Dakota", "South Dakota", 54 },
                    { 25, null, 2, "Idaho", "Idaho", 25 },
                    { 24, null, 2, "Hawaii", "Hawaii", 24 },
                    { 23, null, 2, "Georgia", "Georgia", 23 },
                    { 22, null, 2, "Florida", "Florida", 22 },
                    { 21, null, 2, "Delaware", "Delaware", 21 },
                    { 20, null, 2, "Connecticut", "Connecticut", 20 },
                    { 19, null, 2, "Colorado", "Colorado", 19 },
                    { 18, null, 2, "California", "California", 18 },
                    { 55, null, 2, "Tennessee", "Tennessee", 55 },
                    { 16, null, 2, "Arizona", "Arizona", 16 },
                    { 15, null, 2, "Alaska", "Alaska", 15 },
                    { 26, null, 2, "Illinois", "Illinois", 26 },
                    { 14, null, 2, "Alabama", "Alabama", 14 },
                    { 12, null, 1, "Nunavut", "Nunavut", 12 },
                    { 11, null, 1, "Northwest Territories", "Northwest Territories", 11 },
                    { 10, null, 1, "Saskatchewan", "Saskatchewan", 10 },
                    { 9, null, 1, "Quebec", "Quebec", 9 },
                    { 8, null, 1, "Prince Edward Island", "Prince Edward Island", 8 },
                    { 7, null, 1, "Ontario", "Ontario", 7 },
                    { 6, null, 1, "Nova Scotia", "Nova Scotia", 6 },
                    { 5, null, 1, "Newfoundland and Labrador", "Newfoundland and Labrador", 5 },
                    { 4, null, 1, "New Brunswick", "New Brunswick", 4 },
                    { 3, null, 1, "Manitoba", "Manitoba", 3 },
                    { 2, null, 1, "British Columbia", "British Columbia", 2 },
                    { 13, null, 1, "Yukon Territory", "Yukon Territory", 13 },
                    { 27, null, 2, "Indiana", "Indiana", 27 },
                    { 17, null, 2, "Arkansas", "Arkansas", 17 },
                    { 29, null, 2, "Kansas", "Kansas", 29 },
                    { 53, null, 2, "South Carolina", "South Carolina", 53 },
                    { 28, null, 2, "Iowa", "Iowa", 28 },
                    { 51, null, 2, "Pennsylvania", "Pennsylvania", 51 },
                    { 50, null, 2, "Oregon", "Oregon", 50 },
                    { 49, null, 2, "Oklahoma", "Oklahoma", 49 },
                    { 48, null, 2, "Ohio", "Ohio", 48 },
                    { 47, null, 2, "North Dakota", "North Dakota", 47 },
                    { 46, null, 2, "North Carolina", "North Carolina", 46 },
                    { 45, null, 2, "New York", "New York", 45 },
                    { 44, null, 2, "New Mexico", "New Mexico", 44 },
                    { 43, null, 2, "New Jersey", "New Jersey", 43 },
                    { 42, null, 2, "New Hampshire", "New Hampshire", 42 },
                    { 52, null, 2, "Rhode Island", "Rhode Island", 52 },
                    { 40, null, 2, "Nebraska", "Nebraska", 40 },
                    { 41, null, 2, "Nevada", "Nevada", 41 },
                    { 32, null, 2, "Maine", "Maine", 32 },
                    { 33, null, 2, "Maryland", "Maryland", 33 },
                    { 34, null, 2, "Massachusetts", "Massachusetts", 34 },
                    { 35, null, 2, "Michigan", "Michigan", 35 },
                    { 31, null, 2, "Louisiana", "Louisiana", 31 },
                    { 37, null, 2, "Mississippi", "Mississippi", 37 },
                    { 38, null, 2, "Missouri", "Missouri", 38 },
                    { 30, null, 2, "Kentucky", "Kentucky", 30 },
                    { 39, null, 2, "Montana", "Montana", 39 },
                    { 36, null, 2, "Minnesota", "Minnesota", 36 }
                });

            migrationBuilder.InsertData(
                table: "ScenarioModelResultType",
                columns: new[] { "Id", "Description", "ModelComponentTypeId", "Name", "ScenarioModelResultVariableTypeId", "SortOrder", "UnitTypeId" },
                values: new object[,]
                {
                    { 12, "Subarea yearly PP yield", 1, "PP Yield", 12, 12, 10 },
                    { 16, "Annual average flow rate at reach outlet", 2, "Runoff reach outflow", 16, 16, 11 },
                    { 23, "Yearly TP loading at reach outlet", 2, "TP reach loading", 13, 23, 10 },
                    { 22, "Yearly PP loading at reach outlet", 2, "PP reach loading", 12, 22, 10 },
                    { 21, "Yearly DP loading at reach outlet", 2, "DP reach loading", 11, 21, 10 },
                    { 20, "Yearly TN loading at reach outlet", 2, "TN reach loading", 10, 20, 10 },
                    { 19, "Yearly PN loading at reach outlet", 2, "PN reach loading", 9, 19, 10 },
                    { 18, "Yearly DN loading at reach outlet", 2, "DN reach loading", 8, 18, 10 },
                    { 13, "Subarea yearly TP yield", 1, "TP Yield", 13, 13, 10 },
                    { 11, "Subarea yearly DP yield", 1, "DP Yield", 11, 11, 10 },
                    { 7, "Subarea yearly TSS yield", 1, "TSS Yield", 7, 7, 9 },
                    { 9, "Subarea yearly PN yield", 1, "PN Yield", 9, 9, 10 },
                    { 8, "Subarea yearly DN yield", 1, "DN Yield", 8, 8, 10 },
                    { 17, "Yearly TSS loading at reach outlet", 2, "TSS reach loading", 7, 17, 9 },
                    { 6, "Subarea yearly runoff", 1, "Runoff", 6, 6, 8 },
                    { 5, "Subarea yearly GW recharge", 1, "Groundwater recharge", 5, 5, 7 },
                    { 4, "Subarea yearly ET", 1, "ET", 4, 4, 6 },
                    { 3, "Subarea annual average soil water content", 1, "Soil moisture", 3, 3, 5 },
                    { 2, "Suarea annual average temperature", 1, "Temperature", 2, 2, 4 },
                    { 1, "Subarea yearly precipitation", 1, "Precipitation", 1, 1, 3 },
                    { 14, "Subarea yearly average soil carbon sequestration", 1, "Soil carbon", 14, 14, 14 },
                    { 10, "Subarea yearly TN yield", 1, "TN Yield", 10, 10, 10 },
                    { 15, "Subarea yearly biodiversity index", 1, "Biodiversity", 15, 15, 15 }
                });

            migrationBuilder.InsertData(
                table: "BMPCombinationBMPTypes",
                columns: new[] { "Id", "BMPCombinationTypeId", "BMPTypeId" },
                values: new object[,]
                {
                    { 4, 4, 4 },
                    { 37, 37, 37 },
                    { 38, 38, 38 },
                    { 39, 39, 39 },
                    { 40, 40, 40 },
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 7, 7, 7 },
                    { 9, 9, 9 },
                    { 10, 10, 10 },
                    { 12, 12, 12 },
                    { 13, 13, 13 },
                    { 14, 14, 14 },
                    { 15, 15, 15 },
                    { 16, 16, 16 },
                    { 17, 17, 17 },
                    { 11, 11, 11 },
                    { 36, 36, 36 },
                    { 35, 35, 35 },
                    { 8, 8, 8 },
                    { 33, 33, 33 },
                    { 34, 34, 34 },
                    { 5, 5, 5 },
                    { 6, 6, 6 },
                    { 18, 18, 18 },
                    { 19, 19, 19 },
                    { 20, 20, 20 },
                    { 22, 22, 22 },
                    { 23, 23, 23 },
                    { 24, 24, 24 },
                    { 21, 21, 21 },
                    { 25, 25, 25 },
                    { 31, 31, 31 },
                    { 30, 30, 30 },
                    { 29, 29, 29 },
                    { 32, 32, 32 },
                    { 27, 27, 27 },
                    { 26, 26, 26 },
                    { 28, 28, 28 }
                });

            migrationBuilder.InsertData(
                table: "BMPEffectivenessType",
                columns: new[] { "Id", "BMPEffectivenessLocationTypeId", "DefaultConstraint", "DefaultConstraintTypeId", "DefaultWeight", "Description", "Name", "ScenarioModelResultTypeId", "ScenarioModelResultVariableTypeId", "SortOrder", "UnitTypeId" },
                values: new object[,]
                {
                    { 11, 1, null, null, 0, "BMP onsite effectiveness on yearly TP", "TP onsite", 13, 13, 11, 2 },
                    { 14, 2, null, null, 0, "BMP offsite effectiveness on yearly outlet flow rate", "Runoff offsite", 16, 16, 14, 2 },
                    { 21, 2, 20m, 1, 100, "BMP offsite effectiveness on yearly outlet TP", "TP offsite", 23, 13, 21, 2 },
                    { 20, 2, null, null, 0, "BMP offsite effectiveness on yearly outlet PP", "PP offsite", 22, 12, 20, 2 },
                    { 19, 2, null, null, 0, "BMP offsite effectiveness on yearly outlet DP", "DP offsite", 21, 11, 19, 2 },
                    { 18, 2, null, null, 0, "BMP offsite effectiveness on yearly outlet TN", "TN offsite", 20, 10, 18, 2 },
                    { 17, 2, null, null, 0, "BMP offsite effectiveness on yearly outlet PN", "PN offsite", 19, 9, 17, 2 },
                    { 16, 2, null, null, 0, "BMP offsite effectiveness on yearly outlet DN", "DN offsite", 18, 8, 16, 2 },
                    { 10, 1, null, null, 0, "BMP onsite effectiveness on yearly PP", "PP onsite", 12, 12, 10, 2 },
                    { 1, 1, null, null, 0, "BMP onsite effectiveness on annual average soil water", "Soil moisture onsite", 3, 3, 1, 2 },
                    { 8, 1, null, null, 0, "BMP onsite effectiveness on yearly TN", "TN onsite", 10, 10, 8, 2 },
                    { 7, 1, null, null, 0, "BMP onsite effectiveness on yearly PN", "PN onsite", 9, 9, 7, 2 },
                    { 6, 1, null, null, 0, "BMP onsite effectiveness on yearly DN", "DN onsite", 8, 8, 6, 2 },
                    { 15, 2, null, null, 0, "BMP offsite effectiveness on yearly outlet TSS", "TSS offsite", 17, 7, 15, 2 },
                    { 5, 1, null, null, 0, "BMP onsite effectiveness on yearly TSS", "TSS onsite", 7, 7, 5, 2 },
                    { 4, 1, null, null, 0, "BMP onsite effectiveness on yearly runoff", "Runoff onsite", 6, 6, 4, 2 },
                    { 3, 1, null, null, 0, "BMP onsite effectiveness on yearly GW recharge", "Groundwater recharge onsite", 5, 5, 3, 2 },
                    { 2, 1, null, null, 0, "BMP onsite effectiveness on yearly ET", "ET onsite", 4, 4, 2, 2 },
                    { 12, 1, null, null, 0, "BMP onsite effectiveness on yearly soil carbon", "Soil carbon onsite", 14, 14, 12, 2 },
                    { 9, 1, null, null, 0, "BMP onsite effectiveness on yearly DP", "DP onsite", 11, 11, 9, 2 },
                    { 13, 1, null, null, 0, "BMP onsite effectiveness on yearly biodiversity index", "Biodiversity onsite", 15, 15, 13, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BMPCombinationBMPTypes_BMPCombinationTypeId",
                table: "BMPCombinationBMPTypes",
                column: "BMPCombinationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BMPCombinationBMPTypes_BMPTypeId",
                table: "BMPCombinationBMPTypes",
                column: "BMPTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BMPEffectivenessType_BMPEffectivenessLocationTypeId",
                table: "BMPEffectivenessType",
                column: "BMPEffectivenessLocationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BMPEffectivenessType_DefaultConstraintTypeId",
                table: "BMPEffectivenessType",
                column: "DefaultConstraintTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BMPEffectivenessType_ScenarioModelResultTypeId",
                table: "BMPEffectivenessType",
                column: "ScenarioModelResultTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BMPEffectivenessType_ScenarioModelResultVariableTypeId",
                table: "BMPEffectivenessType",
                column: "ScenarioModelResultVariableTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BMPEffectivenessType_UnitTypeId",
                table: "BMPEffectivenessType",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BMPType_ModelComponentTypeId",
                table: "BMPType",
                column: "ModelComponentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CatchBasin_ModelComponentId",
                table: "CatchBasin",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatchBasin_ReachId",
                table: "CatchBasin",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_CatchBasin_SubAreaId",
                table: "CatchBasin",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedDrain_ModelComponentId",
                table: "ClosedDrain",
                column: "ModelComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedDrain_ReachId",
                table: "ClosedDrain",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosedDrain_SubAreaId",
                table: "ClosedDrain",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Dugout_AnimalTypeId",
                table: "Dugout",
                column: "AnimalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Dugout_ModelComponentId",
                table: "Dugout",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dugout_ReachId",
                table: "Dugout",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_Dugout_SubAreaId",
                table: "Dugout",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedlot_AnimalTypeId",
                table: "Feedlot",
                column: "AnimalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedlot_ModelComponentId",
                table: "Feedlot",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedlot_ReachId",
                table: "Feedlot",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedlot_SubAreaId",
                table: "Feedlot",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowDiversion_ModelComponentId",
                table: "FlowDiversion",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowDiversion_ReachId",
                table: "FlowDiversion",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowDiversion_SubAreaId",
                table: "FlowDiversion",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_GrassedWaterway_ModelComponentId",
                table: "GrassedWaterway",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrassedWaterway_ReachId",
                table: "GrassedWaterway",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_GrassedWaterway_SubAreaId",
                table: "GrassedWaterway",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_IsolatedWetland_ModelComponentId",
                table: "IsolatedWetland",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IsolatedWetland_ReachId",
                table: "IsolatedWetland",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_IsolatedWetland_SubAreaId",
                table: "IsolatedWetland",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Lake_ModelComponentId",
                table: "Lake",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lake_ReachId",
                table: "Lake",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_Lake_SubAreaId",
                table: "Lake",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ManureStorage_ModelComponentId",
                table: "ManureStorage",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManureStorage_ReachId",
                table: "ManureStorage",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_ManureStorage_SubAreaId",
                table: "ManureStorage",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelComponent_ModelComponentTypeId",
                table: "ModelComponent",
                column: "ModelComponentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelComponent_WatershedId",
                table: "ModelComponent",
                column: "WatershedId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelComponentBMPTypes_BMPTypeId",
                table: "ModelComponentBMPTypes",
                column: "BMPTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelComponentBMPTypes_ModelComponentId",
                table: "ModelComponentBMPTypes",
                column: "ModelComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Optimization_OptimizationTypeId",
                table: "Optimization",
                column: "OptimizationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Optimization_ProjectId",
                table: "Optimization",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationConstraints_BMPEffectivenessTypeId",
                table: "OptimizationConstraints",
                column: "BMPEffectivenessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationConstraints_OptimizationConstraintValueTypeId",
                table: "OptimizationConstraints",
                column: "OptimizationConstraintValueTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationConstraints_OptimizationId",
                table: "OptimizationConstraints",
                column: "OptimizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationLegalSubDivisions_BMPTypeId",
                table: "OptimizationLegalSubDivisions",
                column: "BMPTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationLegalSubDivisions_LegalSubDivisionId",
                table: "OptimizationLegalSubDivisions",
                column: "LegalSubDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationLegalSubDivisions_OptimizationId",
                table: "OptimizationLegalSubDivisions",
                column: "OptimizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationParcels_BMPTypeId",
                table: "OptimizationParcels",
                column: "BMPTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationParcels_OptimizationId",
                table: "OptimizationParcels",
                column: "OptimizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationParcels_ParcelId",
                table: "OptimizationParcels",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationWeights_BMPEffectivenessTypeId",
                table: "OptimizationWeights",
                column: "BMPEffectivenessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationWeights_OptimizationId",
                table: "OptimizationWeights",
                column: "OptimizationId");

            migrationBuilder.CreateIndex(
                name: "IX_PointSource_ModelComponentId",
                table: "PointSource",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointSource_ReachId",
                table: "PointSource",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_PointSource_SubAreaId",
                table: "PointSource",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ProjectSpatialUnitTypeId",
                table: "Project",
                column: "ProjectSpatialUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ScenarioTypeId",
                table: "Project",
                column: "ScenarioTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_UserId",
                table: "Project",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMunicipalities_MunicipalityId",
                table: "ProjectMunicipalities",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMunicipalities_ProjectId",
                table: "ProjectMunicipalities",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectWatersheds_ProjectId",
                table: "ProjectWatersheds",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectWatersheds_WatershedId",
                table: "ProjectWatersheds",
                column: "WatershedId");

            migrationBuilder.CreateIndex(
                name: "IX_Province_CountryId",
                table: "Province",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Reach_ModelComponentId",
                table: "Reach",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reach_SubbasinId",
                table: "Reach",
                column: "SubbasinId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservoir_ModelComponentId",
                table: "Reservoir",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservoir_ReachId",
                table: "Reservoir",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservoir_SubAreaId",
                table: "Reservoir",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_RiparianBuffer_ModelComponentId",
                table: "RiparianBuffer",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiparianBuffer_ReachId",
                table: "RiparianBuffer",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_RiparianBuffer_SubAreaId",
                table: "RiparianBuffer",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_RiparianWetland_ModelComponentId",
                table: "RiparianWetland",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiparianWetland_ReachId",
                table: "RiparianWetland",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_RiparianWetland_SubAreaId",
                table: "RiparianWetland",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_RockChute_ModelComponentId",
                table: "RockChute",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RockChute_ReachId",
                table: "RockChute",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_RockChute_SubAreaId",
                table: "RockChute",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenario_ScenarioTypeId",
                table: "Scenario",
                column: "ScenarioTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenario_WatershedId",
                table: "Scenario",
                column: "WatershedId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioModelResult_ModelComponentId",
                table: "ScenarioModelResult",
                column: "ModelComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioModelResult_ScenarioId",
                table: "ScenarioModelResult",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioModelResult_ScenarioModelResultTypeId",
                table: "ScenarioModelResult",
                column: "ScenarioModelResultTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioModelResultType_ScenarioModelResultVariableTypeId",
                table: "ScenarioModelResultType",
                column: "ScenarioModelResultVariableTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioModelResultType_UnitTypeId",
                table: "ScenarioModelResultType",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SmallDam_ModelComponentId",
                table: "SmallDam",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmallDam_ReachId",
                table: "SmallDam",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_SmallDam_SubAreaId",
                table: "SmallDam",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Solution_ProjectId",
                table: "Solution",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolutionLegalSubDivisions_BMPTypeId",
                table: "SolutionLegalSubDivisions",
                column: "BMPTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionLegalSubDivisions_LegalSubDivisionId",
                table: "SolutionLegalSubDivisions",
                column: "LegalSubDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionLegalSubDivisions_SolutionId",
                table: "SolutionLegalSubDivisions",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionModelComponents_BMPTypeId",
                table: "SolutionModelComponents",
                column: "BMPTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionModelComponents_ModelComponentId",
                table: "SolutionModelComponents",
                column: "ModelComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionModelComponents_SolutionId",
                table: "SolutionModelComponents",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionParcels_BMPTypeId",
                table: "SolutionParcels",
                column: "BMPTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionParcels_ParcelId",
                table: "SolutionParcels",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionParcels_SolutionId",
                table: "SolutionParcels",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubArea_LegalSubDivisionId",
                table: "SubArea",
                column: "LegalSubDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubArea_ModelComponentId",
                table: "SubArea",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubArea_ParcelId",
                table: "SubArea",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_SubArea_SubbasinId",
                table: "SubArea",
                column: "SubbasinId");

            migrationBuilder.CreateIndex(
                name: "IX_Subbasin_SubWatershedId",
                table: "Subbasin",
                column: "SubWatershedId");

            migrationBuilder.CreateIndex(
                name: "IX_SubWatershed_WatershedId",
                table: "SubWatershed",
                column: "WatershedId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitScenario_BMPCombinationId",
                table: "UnitScenario",
                column: "BMPCombinationId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitScenario_ModelComponentId",
                table: "UnitScenario",
                column: "ModelComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitScenario_ScenarioId",
                table: "UnitScenario",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitScenarioEffectiveness_BMPEffectivenessTypeId",
                table: "UnitScenarioEffectiveness",
                column: "BMPEffectivenessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitScenarioEffectiveness_UnitScenarioId",
                table: "UnitScenarioEffectiveness",
                column: "UnitScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_User_ProvinceId",
                table: "User",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_User_UserTypeId",
                table: "User",
                column: "UserTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMunicipalities_MunicipalityId",
                table: "UserMunicipalities",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMunicipalities_UserId",
                table: "UserMunicipalities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserParcels_ParcelId",
                table: "UserParcels",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserParcels_UserId",
                table: "UserParcels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWatersheds_UserId",
                table: "UserWatersheds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWatersheds_WatershedId",
                table: "UserWatersheds",
                column: "WatershedId");

            migrationBuilder.CreateIndex(
                name: "IX_VegetativeFilterStrip_ModelComponentId",
                table: "VegetativeFilterStrip",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VegetativeFilterStrip_ReachId",
                table: "VegetativeFilterStrip",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_VegetativeFilterStrip_SubAreaId",
                table: "VegetativeFilterStrip",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Wascob_ModelComponentId",
                table: "Wascob",
                column: "ModelComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wascob_ReachId",
                table: "Wascob",
                column: "ReachId");

            migrationBuilder.CreateIndex(
                name: "IX_Wascob_SubAreaId",
                table: "Wascob",
                column: "SubAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_WatershedExistingBMPType_BMPTypeId",
                table: "WatershedExistingBMPType",
                column: "BMPTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WatershedExistingBMPType_InvestorId",
                table: "WatershedExistingBMPType",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_WatershedExistingBMPType_ModelComponentId",
                table: "WatershedExistingBMPType",
                column: "ModelComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_WatershedExistingBMPType_ScenarioTypeId",
                table: "WatershedExistingBMPType",
                column: "ScenarioTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BMPCombinationBMPTypes");

            migrationBuilder.DropTable(
                name: "CatchBasin");

            migrationBuilder.DropTable(
                name: "ClosedDrain");

            migrationBuilder.DropTable(
                name: "Dugout");

            migrationBuilder.DropTable(
                name: "Farm");

            migrationBuilder.DropTable(
                name: "Feedlot");

            migrationBuilder.DropTable(
                name: "FlowDiversion");

            migrationBuilder.DropTable(
                name: "GeometryLayerStyle");

            migrationBuilder.DropTable(
                name: "GrassedWaterway");

            migrationBuilder.DropTable(
                name: "IsolatedWetland");

            migrationBuilder.DropTable(
                name: "Lake");

            migrationBuilder.DropTable(
                name: "ManureStorage");

            migrationBuilder.DropTable(
                name: "ModelComponentBMPTypes");

            migrationBuilder.DropTable(
                name: "OptimizationConstraints");

            migrationBuilder.DropTable(
                name: "OptimizationLegalSubDivisions");

            migrationBuilder.DropTable(
                name: "OptimizationParcels");

            migrationBuilder.DropTable(
                name: "OptimizationWeights");

            migrationBuilder.DropTable(
                name: "PointSource");

            migrationBuilder.DropTable(
                name: "ProjectMunicipalities");

            migrationBuilder.DropTable(
                name: "ProjectWatersheds");

            migrationBuilder.DropTable(
                name: "Reservoir");

            migrationBuilder.DropTable(
                name: "RiparianBuffer");

            migrationBuilder.DropTable(
                name: "RiparianWetland");

            migrationBuilder.DropTable(
                name: "RockChute");

            migrationBuilder.DropTable(
                name: "ScenarioModelResult");

            migrationBuilder.DropTable(
                name: "ScenarioResultSummarizationType");

            migrationBuilder.DropTable(
                name: "SmallDam");

            migrationBuilder.DropTable(
                name: "SolutionLegalSubDivisions");

            migrationBuilder.DropTable(
                name: "SolutionModelComponents");

            migrationBuilder.DropTable(
                name: "SolutionParcels");

            migrationBuilder.DropTable(
                name: "UnitScenarioEffectiveness");

            migrationBuilder.DropTable(
                name: "UserMunicipalities");

            migrationBuilder.DropTable(
                name: "UserParcels");

            migrationBuilder.DropTable(
                name: "UserWatersheds");

            migrationBuilder.DropTable(
                name: "VegetativeFilterStrip");

            migrationBuilder.DropTable(
                name: "Wascob");

            migrationBuilder.DropTable(
                name: "WatershedExistingBMPType");

            migrationBuilder.DropTable(
                name: "AnimalType");

            migrationBuilder.DropTable(
                name: "Optimization");

            migrationBuilder.DropTable(
                name: "Solution");

            migrationBuilder.DropTable(
                name: "BMPEffectivenessType");

            migrationBuilder.DropTable(
                name: "UnitScenario");

            migrationBuilder.DropTable(
                name: "Municipality");

            migrationBuilder.DropTable(
                name: "Reach");

            migrationBuilder.DropTable(
                name: "SubArea");

            migrationBuilder.DropTable(
                name: "BMPType");

            migrationBuilder.DropTable(
                name: "Investor");

            migrationBuilder.DropTable(
                name: "OptimizationType");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "BMPEffectivenessLocationType");

            migrationBuilder.DropTable(
                name: "OptimizationConstraintValueType");

            migrationBuilder.DropTable(
                name: "ScenarioModelResultType");

            migrationBuilder.DropTable(
                name: "BMPCombinationType");

            migrationBuilder.DropTable(
                name: "Scenario");

            migrationBuilder.DropTable(
                name: "LegalSubDivision");

            migrationBuilder.DropTable(
                name: "ModelComponent");

            migrationBuilder.DropTable(
                name: "Parcel");

            migrationBuilder.DropTable(
                name: "Subbasin");

            migrationBuilder.DropTable(
                name: "ProjectSpatialUnitType");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ScenarioModelResultVariableType");

            migrationBuilder.DropTable(
                name: "UnitType");

            migrationBuilder.DropTable(
                name: "ScenarioType");

            migrationBuilder.DropTable(
                name: "ModelComponentType");

            migrationBuilder.DropTable(
                name: "SubWatershed");

            migrationBuilder.DropTable(
                name: "Province");

            migrationBuilder.DropTable(
                name: "UserType");

            migrationBuilder.DropTable(
                name: "Watershed");

            migrationBuilder.DropTable(
                name: "Country");
        }
    }
}
