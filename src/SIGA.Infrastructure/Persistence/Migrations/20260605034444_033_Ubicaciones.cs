using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _033_Ubicaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ciudades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DepartamentoId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ciudades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ciudades_departamentos_DepartamentoId",
                        column: x => x.DepartamentoId,
                        principalTable: "departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ciudades_DepartamentoId_Nombre",
                table: "ciudades",
                columns: new[] { "DepartamentoId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departamentos_Nombre",
                table: "departamentos",
                column: "Nombre",
                unique: true);

            // ── Seed: Departamentos de Paraguay ───────────────────────────────────
            migrationBuilder.Sql("""
                INSERT INTO departamentos ("Nombre", "IsActive") VALUES
                ('Asunción',         true),
                ('Concepción',       true),
                ('San Pedro',        true),
                ('Cordillera',       true),
                ('Guairá',           true),
                ('Caaguazú',         true),
                ('Caazapá',          true),
                ('Itapúa',           true),
                ('Misiones',         true),
                ('Paraguarí',        true),
                ('Alto Paraná',      true),
                ('Central',          true),
                ('Ñeembucú',         true),
                ('Amambay',          true),
                ('Canindeyú',        true),
                ('Presidente Hayes', true),
                ('Boquerón',         true),
                ('Alto Paraguay',    true);
                """);

            // ── Seed: Ciudades por departamento ───────────────────────────────────
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                  d_id INT;
                BEGIN

                -- Asunción
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Asunción';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Asunción', d_id, true);

                -- Concepción
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Concepción';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Concepción',d_id,true),('Belén',d_id,true),('Horqueta',d_id,true),
                  ('Loreto',d_id,true),('San Carlos',d_id,true),('San Lázaro',d_id,true),
                  ('Yby Yaú',d_id,true),('Arroyito',d_id,true),('Paso Barreto',d_id,true),
                  ('San Alfredo',d_id,true),('Sgto. José Félix López',d_id,true);

                -- San Pedro
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'San Pedro';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('San Pedro del Ycuamandiyú',d_id,true),('Villa del Rosario',d_id,true),
                  ('General Elizardo Aquino',d_id,true),('Choré',d_id,true),
                  ('Nueva Germania',d_id,true),('San Estanislao',d_id,true),
                  ('Tacuatí',d_id,true),('Unión',d_id,true),('25 de Diciembre',d_id,true),
                  ('Lima',d_id,true),('Itacurubí del Rosario',d_id,true),('San Pablo',d_id,true),
                  ('Liberación',d_id,true),('Río Verde',d_id,true),('Santa Rosa del Aguaray',d_id,true),
                  ('Yvyrarobaná',d_id,true),('Capiibary',d_id,true),('General Isidoro Resquín',d_id,true),
                  ('San Vicente Pancholo',d_id,true);

                -- Cordillera
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Cordillera';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Caacupé',d_id,true),('Altos',d_id,true),('Arroyos y Esteros',d_id,true),
                  ('Caraguatay',d_id,true),('Emboscada',d_id,true),('Itauguá',d_id,true),
                  ('Juan de Mena',d_id,true),('Loma Grande',d_id,true),('Mbocayaty del Yhaguy',d_id,true),
                  ('Nueva Colombia',d_id,true),('Piribebuy',d_id,true),('Primero de Marzo',d_id,true),
                  ('San Bernardino',d_id,true),('San José Obrero',d_id,true),('Tobatí',d_id,true),
                  ('Valenzuela',d_id,true),('Eusebio Ayala',d_id,true),('Isla Pucú',d_id,true);

                -- Guairá
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Guairá';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Villarrica',d_id,true),('Borja',d_id,true),('Colonia Independencia',d_id,true),
                  ('Félix Pérez Cardozo',d_id,true),('Gral. Eugenio A. Garay',d_id,true),
                  ('Mbocayaty',d_id,true),('Natalicio Talavera',d_id,true),('Ñumí',d_id,true),
                  ('San Salvador',d_id,true),('Yataity del Guairá',d_id,true),
                  ('Paso Yobái',d_id,true),('Tembiapora',d_id,true),('Iturbe',d_id,true);

                -- Caaguazú
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Caaguazú';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Coronel Oviedo',d_id,true),('Caaguazú',d_id,true),
                  ('Doctor Juan Manuel Frutos',d_id,true),('José Domingo Ocampos',d_id,true),
                  ('Nueva Londres',d_id,true),('San Joaquín',d_id,true),
                  ('San José de los Arroyos',d_id,true),('Yhú',d_id,true),
                  ('R.I. 3 Corrales',d_id,true),('Mbutuy',d_id,true),
                  ('Raúl Arsenio Oviedo',d_id,true),('Campo 9',d_id,true),
                  ('Repatriación',d_id,true),('La Pastora',d_id,true),('3 de Febrero',d_id,true),
                  ('Simón Bolívar',d_id,true),('Mcal. Francisco Solano López',d_id,true);

                -- Caazapá
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Caazapá';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Caazapá',d_id,true),('Abai',d_id,true),('Doctor Moisés S. Bertoni',d_id,true),
                  ('Gral. Higinio Morínigo',d_id,true),('Buena Vista',d_id,true),
                  ('Maciel',d_id,true),('San Juan Nepomuceno',d_id,true),('Tavaí',d_id,true),
                  ('Yuty',d_id,true),('3 de Mayo',d_id,true);

                -- Itapúa
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Itapúa';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Encarnación',d_id,true),('Alto Verá',d_id,true),('Bella Vista',d_id,true),
                  ('Cambyretá',d_id,true),('Capitán Meza',d_id,true),('Capitán Miranda',d_id,true),
                  ('Carlos Antonio López',d_id,true),('Carmen del Paraná',d_id,true),
                  ('Coronel Bogado',d_id,true),('Edelira',d_id,true),('Fram',d_id,true),
                  ('General Artigas',d_id,true),('Hohenau',d_id,true),('Itapúa Poty',d_id,true),
                  ('Jesús',d_id,true),('José Leandro Oviedo',d_id,true),('La Paz',d_id,true),
                  ('Mayor Otaño',d_id,true),('Natalio',d_id,true),('Nueva Alborada',d_id,true),
                  ('Obligado',d_id,true),('Pirapo',d_id,true),('San Cosme y Damián',d_id,true),
                  ('San Juan del Paraná',d_id,true),('San Pedro del Paraná',d_id,true),
                  ('San Rafael del Paraná',d_id,true),('Tomás Romero Pereira',d_id,true),
                  ('Trinidad',d_id,true);

                -- Misiones
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Misiones';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('San Juan Bautista',d_id,true),('Ayolas',d_id,true),('San Ignacio',d_id,true),
                  ('San Miguel',d_id,true),('San Patricio',d_id,true),('Santa María',d_id,true),
                  ('Santa Rosa',d_id,true),('Santiago',d_id,true),('Villa Florida',d_id,true),
                  ('Yabebyry',d_id,true);

                -- Paraguarí
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Paraguarí';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Paraguarí',d_id,true),('Acahay',d_id,true),('Caapucú',d_id,true),
                  ('Carapeguá',d_id,true),('Escobar',d_id,true),
                  ('General Bernardino Caballero',d_id,true),('La Colmena',d_id,true),
                  ('Mbuyapey',d_id,true),('Pirayú',d_id,true),('Quiindy',d_id,true),
                  ('Quyquyhó',d_id,true),('San Roque González de Santa Cruz',d_id,true),
                  ('Sapucaí',d_id,true),('Tebicuary',d_id,true),('Yaguarón',d_id,true),
                  ('Ybycuí',d_id,true);

                -- Alto Paraná
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Alto Paraná';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Ciudad del Este',d_id,true),('Presidente Franco',d_id,true),
                  ('Minga Guazú',d_id,true),('Hernandarias',d_id,true),('Minga Porã',d_id,true),
                  ('Domingo Martínez de Irala',d_id,true),('Dr. Juan León Mallorquín',d_id,true),
                  ('Iruña',d_id,true),('Itakyry',d_id,true),('Juan E. O''Leary',d_id,true),
                  ('Los Cedrales',d_id,true),('Naranjal',d_id,true),('Ñacunday',d_id,true),
                  ('Santa Rita',d_id,true),('San Alberto',d_id,true),('San Cristóbal',d_id,true),
                  ('Santa Fe del Paraná',d_id,true),('Santa Rosa del Monday',d_id,true),
                  ('Tavapy',d_id,true),('Yguazú',d_id,true);

                -- Central
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Central';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Areguá',d_id,true),('Capiatá',d_id,true),('Fernando de la Mora',d_id,true),
                  ('Guarambaré',d_id,true),('Itá',d_id,true),('Itauguá',d_id,true),
                  ('J. Augusto Saldívar',d_id,true),('Lambaré',d_id,true),('Limpio',d_id,true),
                  ('Luque',d_id,true),('Mariano Roque Alonso',d_id,true),('Ñemby',d_id,true),
                  ('Nueva Italia',d_id,true),('San Antonio',d_id,true),('San Lorenzo',d_id,true),
                  ('Villeta',d_id,true),('Ypané',d_id,true);

                -- Ñeembucú
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Ñeembucú';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Pilar',d_id,true),('Alberdi',d_id,true),('Cerrito',d_id,true),
                  ('General Díaz',d_id,true),('Gral. José Eduvigis Díaz',d_id,true),
                  ('Guazú Cuá',d_id,true),('Humaitá',d_id,true),('Isla Umbú',d_id,true),
                  ('Laureles',d_id,true),('Mayor José J. Martínez',d_id,true),('Olimpo',d_id,true),
                  ('Paso de Patria',d_id,true),('San Juan Bautista del Ñeembucú',d_id,true),
                  ('Tacuaras',d_id,true),('Villa Oliva',d_id,true),('Villalbín',d_id,true);

                -- Amambay
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Amambay';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Pedro Juan Caballero',d_id,true),('Bella Vista Norte',d_id,true),
                  ('Capitán Bado',d_id,true),('Zanja Pytá',d_id,true),('Karapai',d_id,true);

                -- Canindeyú
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Canindeyú';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Salto del Guairá',d_id,true),('Corpus Christi',d_id,true),
                  ('Curuguaty',d_id,true),('Katueté',d_id,true),('La Paloma',d_id,true),
                  ('Nueva Esperanza',d_id,true),('Ypehu',d_id,true),('Yby Pytã',d_id,true),
                  ('Francisco Caballero Álvarez',d_id,true),('Itanará',d_id,true),
                  ('Liberación',d_id,true);

                -- Presidente Hayes
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Presidente Hayes';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Villa Hayes',d_id,true),('Benjamín Aceval',d_id,true),
                  ('General José María Bruguez',d_id,true),('José Falcón',d_id,true),
                  ('Nanawa',d_id,true),('Pozo Colorado',d_id,true),
                  ('Tte. 1.° Víctor Villar',d_id,true),('Puerto Pinasco',d_id,true);

                -- Boquerón
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Boquerón';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Filadelfia',d_id,true),('Loma Plata',d_id,true),
                  ('Mariscal Estigarribia',d_id,true);

                -- Alto Paraguay
                SELECT "Id" INTO d_id FROM departamentos WHERE "Nombre" = 'Alto Paraguay';
                INSERT INTO ciudades ("Nombre","DepartamentoId","IsActive") VALUES
                  ('Fuerte Olimpo',d_id,true),('Bahía Negra',d_id,true),
                  ('Carmelo Peralta',d_id,true),('La Victoria',d_id,true);

                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ciudades");

            migrationBuilder.DropTable(
                name: "departamentos");
        }
    }
}
