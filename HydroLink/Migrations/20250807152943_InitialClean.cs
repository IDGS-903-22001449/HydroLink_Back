using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HydroLink.Migrations
{
    /// <inheritdoc />
    public partial class InitialClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Componente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Especificaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsPersonalizable = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Componente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MateriaPrima",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MateriaPrima", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Persona",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoPersona = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Empresa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    Activo = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rol = table.Column<int>(type: "int", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Producto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductoHydroLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Especificaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TipoInstalacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TiempoInstalacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Garantia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImagenBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoHydroLink", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductoModular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CapacidadPorModulo = table.Column<int>(type: "int", nullable: false),
                    PrecioBaseModulo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioModuloAdicional = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TipoPlanta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Dimensiones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Especificaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoModular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComponenteMateriaPrima",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponenteId = table.Column<int>(type: "int", nullable: false),
                    MateriaPrimaId = table.Column<int>(type: "int", nullable: false),
                    CantidadNecesaria = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FactorConversion = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PorcentajeMerma = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    ComponenteId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponenteMateriaPrima", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponenteMateriaPrima_Componente_ComponenteId",
                        column: x => x.ComponenteId,
                        principalTable: "Componente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComponenteMateriaPrima_Componente_ComponenteId1",
                        column: x => x.ComponenteId1,
                        principalTable: "Componente",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComponenteMateriaPrima_MateriaPrima_MateriaPrimaId",
                        column: x => x.MateriaPrimaId,
                        principalTable: "MateriaPrima",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CostoPromedioMateriaPrima",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MateriaPrimaId = table.Column<int>(type: "int", nullable: false),
                    CostoPromedioActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExistenciaActual = table.Column<int>(type: "int", nullable: false),
                    ValorInventarioTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualizadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostoPromedioMateriaPrima", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostoPromedioMateriaPrima_MateriaPrima_MateriaPrimaId",
                        column: x => x.MateriaPrimaId,
                        principalTable: "MateriaPrima",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Compra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compra_Persona_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comentario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Calificacion = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comentario_Persona_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comentario_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListaMaterial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    MateriaPrimaId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListaMaterial_MateriaPrima_MateriaPrimaId",
                        column: x => x.MateriaPrimaId,
                        principalTable: "MateriaPrima",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListaMaterial_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComponenteRequerido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoHydroLinkId = table.Column<int>(type: "int", nullable: false),
                    ComponenteId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Especificaciones = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponenteRequerido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponenteRequerido_Componente_ComponenteId",
                        column: x => x.ComponenteId,
                        principalTable: "Componente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponenteRequerido_ProductoHydroLink_ProductoHydroLinkId",
                        column: x => x.ProductoHydroLinkId,
                        principalTable: "ProductoHydroLink",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Venta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CotizacionId = table.Column<int>(type: "int", nullable: true),
                    ClienteId1 = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Venta_Persona_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Venta_Persona_ClienteId1",
                        column: x => x.ClienteId1,
                        principalTable: "Persona",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Venta_Persona_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Persona",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Venta_ProductoHydroLink_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "ProductoHydroLink",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComponenteModulo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoModularId = table.Column<int>(type: "int", nullable: false),
                    ComponenteId = table.Column<int>(type: "int", nullable: false),
                    CantidadBase = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadPorModuloAdicional = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TipoComponente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NotasInstalacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EsObligatorio = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductoModularId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponenteModulo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponenteModulo_Componente_ComponenteId",
                        column: x => x.ComponenteId,
                        principalTable: "Componente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponenteModulo_ProductoModular_ProductoModularId",
                        column: x => x.ProductoModularId,
                        principalTable: "ProductoModular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComponenteModulo_ProductoModular_ProductoModularId1",
                        column: x => x.ProductoModularId1,
                        principalTable: "ProductoModular",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CotizacionModular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoModularId = table.Column<int>(type: "int", nullable: false),
                    NombreCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TelefonoCliente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NombreProyecto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CantidadModulos = table.Column<int>(type: "int", nullable: false),
                    SubtotalComponentes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubtotalManoObra = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubtotalMateriales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PorcentajeGanancia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoGanancia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EspecificacionesEspeciales = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotizacionModular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionModular_ProductoModular_ProductoModularId",
                        column: x => x.ProductoModularId,
                        principalTable: "ProductoModular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompraDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompraId = table.Column<int>(type: "int", nullable: false),
                    MateriaPrimaId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompraDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompraDetalle_Compra_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompraDetalle_MateriaPrima_MateriaPrimaId",
                        column: x => x.MateriaPrimaId,
                        principalTable: "MateriaPrima",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoteInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroLote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MateriaPrimaId = table.Column<int>(type: "int", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CantidadInicial = table.Column<int>(type: "int", nullable: false),
                    CantidadDisponible = table.Column<int>(type: "int", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: true),
                    CompraId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoteInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoteInventario_Compra_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LoteInventario_MateriaPrima_MateriaPrimaId",
                        column: x => x.MateriaPrimaId,
                        principalTable: "MateriaPrima",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoteInventario_Persona_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Cotizacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreProyecto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombreCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TelefonoCliente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    SubtotalComponentes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubtotalManoObra = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubtotalMateriales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PorcentajeGanancia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoGanancia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VentaId = table.Column<int>(type: "int", nullable: true),
                    ClienteId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cotizacion_Persona_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizacion_Persona_ClienteId1",
                        column: x => x.ClienteId1,
                        principalTable: "Persona",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cotizacion_Persona_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Persona",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cotizacion_ProductoHydroLink_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "ProductoHydroLink",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizacion_Venta_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MovimientoComponente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponenteId = table.Column<int>(type: "int", nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumeroLote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VentaId = table.Column<int>(type: "int", nullable: true),
                    CompraId = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoComponente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientoComponente_Componente_ComponenteId",
                        column: x => x.ComponenteId,
                        principalTable: "Componente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientoComponente_Compra_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientoComponente_Venta_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VentaDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaDetalle_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VentaDetalle_Venta_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Venta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CotizacionModularDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CotizacionModularId = table.Column<int>(type: "int", nullable: false),
                    ComponenteId = table.Column<int>(type: "int", nullable: false),
                    NombreComponente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CantidadBase = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadAdicional = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TipoComponente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Especificaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotasInstalacion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotizacionModularDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionModularDetalle_Componente_ComponenteId",
                        column: x => x.ComponenteId,
                        principalTable: "Componente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CotizacionModularDetalle_CotizacionModular_CotizacionModularId",
                        column: x => x.CotizacionModularId,
                        principalTable: "CotizacionModular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientoInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MateriaPrimaId = table.Column<int>(type: "int", nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumeroLote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompraId = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoteInventarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientoInventario_Compra_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientoInventario_LoteInventario_LoteInventarioId",
                        column: x => x.LoteInventarioId,
                        principalTable: "LoteInventario",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovimientoInventario_MateriaPrima_MateriaPrimaId",
                        column: x => x.MateriaPrimaId,
                        principalTable: "MateriaPrima",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CotizacionDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CotizacionId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: true),
                    ComponenteId = table.Column<int>(type: "int", nullable: false),
                    NombreItem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrecioUnitarioEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Especificaciones = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotizacionDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionDetalle_Componente_ComponenteId",
                        column: x => x.ComponenteId,
                        principalTable: "Componente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CotizacionDetalle_Cotizacion_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CotizacionDetalle_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_ProductoId",
                table: "Comentario",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_UsuarioId",
                table: "Comentario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponenteMateriaPrima_ComponenteId_MateriaPrimaId",
                table: "ComponenteMateriaPrima",
                columns: new[] { "ComponenteId", "MateriaPrimaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponenteMateriaPrima_ComponenteId1",
                table: "ComponenteMateriaPrima",
                column: "ComponenteId1");

            migrationBuilder.CreateIndex(
                name: "IX_ComponenteMateriaPrima_MateriaPrimaId",
                table: "ComponenteMateriaPrima",
                column: "MateriaPrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponenteModulo_ComponenteId",
                table: "ComponenteModulo",
                column: "ComponenteId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponenteModulo_ProductoModularId",
                table: "ComponenteModulo",
                column: "ProductoModularId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponenteModulo_ProductoModularId1",
                table: "ComponenteModulo",
                column: "ProductoModularId1");

            migrationBuilder.CreateIndex(
                name: "IX_ComponenteRequerido_ComponenteId",
                table: "ComponenteRequerido",
                column: "ComponenteId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponenteRequerido_ProductoHydroLinkId",
                table: "ComponenteRequerido",
                column: "ProductoHydroLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_Compra_ProveedorId",
                table: "Compra",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_CompraDetalle_CompraId",
                table: "CompraDetalle",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_CompraDetalle_MateriaPrimaId",
                table: "CompraDetalle",
                column: "MateriaPrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_CostoPromedioMateriaPrima_MateriaPrimaId",
                table: "CostoPromedioMateriaPrima",
                column: "MateriaPrimaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_ClienteId",
                table: "Cotizacion",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_ClienteId1",
                table: "Cotizacion",
                column: "ClienteId1");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_ProductoId",
                table: "Cotizacion",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_UsuarioId",
                table: "Cotizacion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_VentaId",
                table: "Cotizacion",
                column: "VentaId",
                unique: true,
                filter: "[VentaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionDetalle_ComponenteId",
                table: "CotizacionDetalle",
                column: "ComponenteId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionDetalle_CotizacionId",
                table: "CotizacionDetalle",
                column: "CotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionDetalle_ProductoId",
                table: "CotizacionDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionModular_ProductoModularId",
                table: "CotizacionModular",
                column: "ProductoModularId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionModularDetalle_ComponenteId",
                table: "CotizacionModularDetalle",
                column: "ComponenteId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionModularDetalle_CotizacionModularId",
                table: "CotizacionModularDetalle",
                column: "CotizacionModularId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaMaterial_MateriaPrimaId",
                table: "ListaMaterial",
                column: "MateriaPrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaMaterial_ProductoId",
                table: "ListaMaterial",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_LoteInventario_CompraId",
                table: "LoteInventario",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_LoteInventario_MateriaPrimaId",
                table: "LoteInventario",
                column: "MateriaPrimaId");

            migrationBuilder.CreateIndex(
                name: "IX_LoteInventario_NumeroLote",
                table: "LoteInventario",
                column: "NumeroLote",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoteInventario_ProveedorId",
                table: "LoteInventario",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoComponente_ComponenteId",
                table: "MovimientoComponente",
                column: "ComponenteId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoComponente_CompraId",
                table: "MovimientoComponente",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoComponente_VentaId",
                table: "MovimientoComponente",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoInventario_CompraId",
                table: "MovimientoInventario",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoInventario_LoteInventarioId",
                table: "MovimientoInventario",
                column: "LoteInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoInventario_MateriaPrimaId_FechaMovimiento",
                table: "MovimientoInventario",
                columns: new[] { "MateriaPrimaId", "FechaMovimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_Venta_ClienteId",
                table: "Venta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Venta_ClienteId1",
                table: "Venta",
                column: "ClienteId1");

            migrationBuilder.CreateIndex(
                name: "IX_Venta_ProductoId",
                table: "Venta",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Venta_UsuarioId",
                table: "Venta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalle_ProductoId",
                table: "VentaDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalle_VentaId",
                table: "VentaDetalle",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Comentario");

            migrationBuilder.DropTable(
                name: "ComponenteMateriaPrima");

            migrationBuilder.DropTable(
                name: "ComponenteModulo");

            migrationBuilder.DropTable(
                name: "ComponenteRequerido");

            migrationBuilder.DropTable(
                name: "CompraDetalle");

            migrationBuilder.DropTable(
                name: "CostoPromedioMateriaPrima");

            migrationBuilder.DropTable(
                name: "CotizacionDetalle");

            migrationBuilder.DropTable(
                name: "CotizacionModularDetalle");

            migrationBuilder.DropTable(
                name: "ListaMaterial");

            migrationBuilder.DropTable(
                name: "MovimientoComponente");

            migrationBuilder.DropTable(
                name: "MovimientoInventario");

            migrationBuilder.DropTable(
                name: "VentaDetalle");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Cotizacion");

            migrationBuilder.DropTable(
                name: "CotizacionModular");

            migrationBuilder.DropTable(
                name: "Componente");

            migrationBuilder.DropTable(
                name: "LoteInventario");

            migrationBuilder.DropTable(
                name: "Producto");

            migrationBuilder.DropTable(
                name: "Venta");

            migrationBuilder.DropTable(
                name: "ProductoModular");

            migrationBuilder.DropTable(
                name: "Compra");

            migrationBuilder.DropTable(
                name: "MateriaPrima");

            migrationBuilder.DropTable(
                name: "ProductoHydroLink");

            migrationBuilder.DropTable(
                name: "Persona");
        }
    }
}
