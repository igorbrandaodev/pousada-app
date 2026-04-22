using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PousadaApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    fantasia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    logo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    url_slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categorias_cardapio",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empresa = table.Column<int>(type: "int", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    icone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_cardapio", x => x.id);
                    table.ForeignKey(
                        name: "FK_categorias_cardapio_empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hospedes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empresa = table.Column<int>(type: "int", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    observacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hospedes", x => x.id);
                    table.ForeignKey(
                        name: "FK_hospedes_empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quartos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empresa = table.Column<int>(type: "int", nullable: false),
                    numero = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    capacidade = table.Column<int>(type: "int", nullable: false),
                    preco_diaria = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    andar = table.Column<int>(type: "int", nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quartos", x => x.id);
                    table.ForeignKey(
                        name: "FK_quartos_empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empresa = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    senha_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ativo = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuarios_empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "produtos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empresa = table.Column<int>(type: "int", nullable: false),
                    id_categoria = table.Column<int>(type: "int", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    preco = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    foto_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    disponivel = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produtos", x => x.id);
                    table.ForeignKey(
                        name: "FK_produtos_categorias_cardapio_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categorias_cardapio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_produtos_empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empresa = table.Column<int>(type: "int", nullable: false),
                    id_hospede = table.Column<int>(type: "int", nullable: false),
                    id_quarto = table.Column<int>(type: "int", nullable: false),
                    data_checkin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_checkout = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    valor_total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    observacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.id);
                    table.ForeignKey(
                        name: "FK_reservas_empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservas_hospedes_id_hospede",
                        column: x => x.id_hospede,
                        principalTable: "hospedes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservas_quartos_id_quarto",
                        column: x => x.id_quarto,
                        principalTable: "quartos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "comandas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_empresa = table.Column<int>(type: "int", nullable: false),
                    id_reserva = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    data_abertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_fechamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comandas", x => x.id);
                    table.ForeignKey(
                        name: "FK_comandas_empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comandas_reservas_id_reserva",
                        column: x => x.id_reserva,
                        principalTable: "reservas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "itens_comanda",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_comanda = table.Column<int>(type: "int", nullable: false),
                    id_produto = table.Column<int>(type: "int", nullable: false),
                    quantidade = table.Column<int>(type: "int", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_comanda", x => x.id);
                    table.ForeignKey(
                        name: "FK_itens_comanda_comandas_id_comanda",
                        column: x => x.id_comanda,
                        principalTable: "comandas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_itens_comanda_produtos_id_produto",
                        column: x => x.id_produto,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "empresas",
                columns: new[] { "id", "created_at", "fantasia", "logo_url", "nome", "updated_at", "url_slug" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bella Vista", null, "Pousada Bella Vista", null, "bella-vista" });

            migrationBuilder.InsertData(
                table: "categorias_cardapio",
                columns: new[] { "id", "icone", "id_empresa", "nome" },
                values: new object[,]
                {
                    { 1, "pi pi-glass-water", 1, "Bebidas" },
                    { 2, "pi pi-sun", 1, "Pratos Principais" },
                    { 3, "pi pi-bolt", 1, "Petiscos" },
                    { 4, "pi pi-heart", 1, "Sobremesas" },
                    { 5, "pi pi-star", 1, "Drinks" }
                });

            migrationBuilder.InsertData(
                table: "hospedes",
                columns: new[] { "id", "cidade", "created_at", "documento", "email", "id_empresa", "nome", "observacoes", "telefone", "updated_at" },
                values: new object[,]
                {
                    { 1, "Sao Paulo - SP", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "123.456.789-00", "carlos.silva@email.com", 1, "Carlos Alberto Silva", null, "(11) 98765-4321", null },
                    { 2, "Rio de Janeiro - RJ", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "987.654.321-00", "maria.oliveira@email.com", 1, "Maria Fernanda Oliveira", null, "(21) 97654-3210", null },
                    { 3, "Belo Horizonte - MG", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "456.789.123-00", "joao.santos@email.com", 1, "Joao Pedro Santos", null, "(31) 96543-2109", null },
                    { 4, "Curitiba - PR", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "321.654.987-00", "ana.pereira@email.com", 1, "Ana Carolina Pereira", null, "(41) 95432-1098", null },
                    { 5, "Porto Alegre - RS", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "654.321.987-00", "rafael.costa@email.com", 1, "Rafael Mendes Costa", null, "(51) 94321-0987", null },
                    { 6, "Salvador - BA", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "789.123.456-00", "juliana.rocha@email.com", 1, "Juliana Martins Rocha", null, "(71) 93210-9876", null },
                    { 7, "Recife - PE", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "234.567.890-00", "fernando.lima@email.com", 1, "Fernando Augusto Lima", null, "(81) 92109-8765", null },
                    { 8, "Florianopolis - SC", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "567.890.234-00", "patricia.almeida@email.com", 1, "Patricia Souza Almeida", null, "(48) 91098-7654", null }
                });

            migrationBuilder.InsertData(
                table: "quartos",
                columns: new[] { "id", "andar", "ativo", "capacidade", "id_empresa", "numero", "preco_diaria", "status", "tipo" },
                values: new object[,]
                {
                    { 1, 1, true, 2, 1, "101", 250.00m, "Disponivel", "Standard" },
                    { 2, 1, true, 2, 1, "102", 250.00m, "Disponivel", "Standard" },
                    { 3, 1, true, 3, 1, "103", 400.00m, "Disponivel", "Luxo" },
                    { 4, 1, true, 4, 1, "104", 600.00m, "Disponivel", "Suite" },
                    { 5, 2, true, 2, 1, "201", 280.00m, "Disponivel", "Standard" },
                    { 6, 2, true, 3, 1, "202", 450.00m, "Disponivel", "Luxo" },
                    { 7, 2, true, 3, 1, "203", 450.00m, "Disponivel", "Luxo" },
                    { 8, 2, true, 4, 1, "204", 650.00m, "Disponivel", "Suite" }
                });

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "id", "ativo", "created_at", "email", "id_empresa", "nome", "role", "senha_hash" },
                values: new object[] { 1, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@pousada.com", 1, "Administrador", "Admin", "$2a$11$uPCh9a1iJzHlzjuoZgtGIeVjCloKvE4fhPXEysmJpwqIiQI0M/51K" });

            migrationBuilder.InsertData(
                table: "produtos",
                columns: new[] { "id", "descricao", "disponivel", "foto_url", "id_categoria", "id_empresa", "nome", "preco" },
                values: new object[,]
                {
                    { 1, null, true, null, 1, 1, "Agua Mineral 500ml", 6.00m },
                    { 2, null, true, null, 1, 1, "Refrigerante Lata", 8.00m },
                    { 3, null, true, null, 1, 1, "Suco Natural de Laranja", 12.00m },
                    { 4, null, true, null, 1, 1, "Cerveja Long Neck", 14.00m },
                    { 5, "Filé de peixe grelhado com legumes e arroz", true, null, 2, 1, "Filé de Peixe Grelhado", 58.00m },
                    { 6, "Picanha grelhada com arroz, feijao e farofa", true, null, 2, 1, "Picanha na Chapa", 72.00m },
                    { 7, "Moqueca com camarao, leite de coco e dende", true, null, 2, 1, "Moqueca de Camarao", 68.00m },
                    { 8, "Filé de frango empanado com molho e queijo", true, null, 2, 1, "Frango Parmegiana", 48.00m },
                    { 9, "Batata frita crocante com cheddar e bacon", true, null, 3, 1, "Porcao de Batata Frita", 32.00m },
                    { 10, "6 unidades de bolinho de bacalhau artesanal", true, null, 3, 1, "Bolinho de Bacalhau", 38.00m },
                    { 11, "Torradas com tomate, manjericao e azeite", true, null, 3, 1, "Bruschetta Italiana", 28.00m },
                    { 12, "Selecao de queijos e embutidos artesanais", true, null, 3, 1, "Tabua de Frios", 55.00m },
                    { 13, "Pudim caseiro de leite condensado", true, null, 4, 1, "Pudim de Leite", 18.00m },
                    { 14, "Bolo de chocolate com sorvete de baunilha", true, null, 4, 1, "Petit Gateau", 28.00m },
                    { 15, "Acai com granola, banana e leite condensado", true, null, 4, 1, "Acai na Tigela", 24.00m },
                    { 16, "Mousse cremoso de maracuja", true, null, 4, 1, "Mousse de Maracuja", 16.00m },
                    { 17, "Cachaca artesanal, limao e acucar", true, null, 5, 1, "Caipirinha de Limao", 22.00m },
                    { 18, "Rum, leite de coco e suco de abacaxi", true, null, 5, 1, "Pina Colada", 28.00m },
                    { 19, "Rum, hortela, limao, acucar e agua com gas", true, null, 5, 1, "Mojito", 26.00m },
                    { 20, "Gin, agua tonica, limao e especiarias", true, null, 5, 1, "Gin Tonica", 30.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_cardapio_id_empresa",
                table: "categorias_cardapio",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_comandas_id_empresa",
                table: "comandas",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_comandas_id_reserva",
                table: "comandas",
                column: "id_reserva");

            migrationBuilder.CreateIndex(
                name: "IX_empresas_url_slug",
                table: "empresas",
                column: "url_slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hospedes_id_empresa",
                table: "hospedes",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_itens_comanda_id_comanda",
                table: "itens_comanda",
                column: "id_comanda");

            migrationBuilder.CreateIndex(
                name: "IX_itens_comanda_id_produto",
                table: "itens_comanda",
                column: "id_produto");

            migrationBuilder.CreateIndex(
                name: "IX_produtos_id_categoria",
                table: "produtos",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_produtos_id_empresa",
                table: "produtos",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_quartos_id_empresa",
                table: "quartos",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_id_empresa",
                table: "reservas",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_id_hospede",
                table: "reservas",
                column: "id_hospede");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_id_quarto",
                table: "reservas",
                column: "id_quarto");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_empresa",
                table: "usuarios",
                column: "id_empresa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itens_comanda");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "comandas");

            migrationBuilder.DropTable(
                name: "produtos");

            migrationBuilder.DropTable(
                name: "reservas");

            migrationBuilder.DropTable(
                name: "categorias_cardapio");

            migrationBuilder.DropTable(
                name: "hospedes");

            migrationBuilder.DropTable(
                name: "quartos");

            migrationBuilder.DropTable(
                name: "empresas");
        }
    }
}
