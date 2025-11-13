using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        // Constants for repeated literals
        private const string SqlServerIdentity = "SqlServer:Identity";
        private const string Users = "Users";
        private const string Products = "Products";
        private const string Orders = "Orders";
        private const string Carts = "Carts";
        private const string CartItems = "CartItems";
        private const string OrderItems = "OrderItems";
        private const string Payments = "Payments";
        private const string Categories = "Categories";
        private const string NVarCharMax = "nvarchar(max)";
        private const string NVarChar100 = "nvarchar(100)";
        private const string NVarChar150 = "nvarchar(150)";
        private const string NVarChar20 = "nvarchar(20)";
        private const string NVarChar500 = "nvarchar(500)";
        private const string Decimal182 = "decimal(18,2)";
        private const string DateTime2 = "datetime2";
        private const string UserId = "UserId";
        private const string ProductId = "ProductId";
        private const string OrderId = "OrderId";
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: Categories,
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    CategoryName = table.Column<string>(type: NVarChar100, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: Users,
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    FullName = table.Column<string>(type: NVarChar100, maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: NVarChar150, maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: NVarCharMax, nullable: false),
                    Role = table.Column<string>(type: NVarChar20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: DateTime2, nullable: false),
                    LastLogin = table.Column<DateTime>(type: DateTime2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: Products,
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    Name = table.Column<string>(type: NVarChar100, maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: NVarChar500, maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: Decimal182, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: Categories,
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: Carts,
                columns: table => new
                {
                    CartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.CartId);
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: Users,
                        principalColumn: UserId,
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: Orders,
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: DateTime2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: Decimal182, nullable: false),
                    Status = table.Column<string>(type: NVarCharMax, nullable: false),
                    ShippingAddress = table.Column<string>(type: NVarChar500, maxLength: 500, nullable: false),
                    PaymentMethod = table.Column<string>(type: NVarCharMax, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: Users,
                        principalColumn: UserId,
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: CartItems,
                columns: table => new
                {
                    CartItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: Decimal182, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemId);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: Carts,
                        principalColumn: "CartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: Products,
                        principalColumn: ProductId,
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: OrderItems,
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: Decimal182, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: Orders,
                        principalColumn: OrderId,
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: Products,
                        principalColumn: ProductId,
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: Payments,
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<string>(type: NVarCharMax, nullable: false),
                    Amount = table.Column<decimal>(type: Decimal182, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: DateTime2, nullable: false),
                    Status = table.Column<string>(type: NVarCharMax, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: Orders,
                        principalColumn: OrderId,
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: CartItems,
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: CartItems,
                column: ProductId);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: Carts,
                column: UserId);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: OrderItems,
                column: OrderId);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: OrderItems,
                column: ProductId);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: Orders,
                column: UserId);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: Payments,
                column: OrderId,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: Products,
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: Users,
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: CartItems);

            migrationBuilder.DropTable(
                name: OrderItems);

            migrationBuilder.DropTable(
                name: Payments);

            migrationBuilder.DropTable(
                name: Carts);

            migrationBuilder.DropTable(
                name: Products);

            migrationBuilder.DropTable(
                name: Orders);

            migrationBuilder.DropTable(
                name: Categories);

            migrationBuilder.DropTable(
                name: Users);
        }
    }
}
