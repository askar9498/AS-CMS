using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingUserColumnsAndUserGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b423115b-653f-4bc1-b3b6-f08c4e0aa8d8"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("cba62a14-e639-4d3f-a150-8463e4c9ac77"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("e2421323-8a04-448d-8b25-a3c32f1ed40f"));

            migrationBuilder.AddColumn<string>(
                name: "ActivityField",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyDescription",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyNationalId",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyPhone",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationLevel",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Expertise",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullAddress",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleScholarLink",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Interests",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalCode",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficialDocumentsUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrcidLink",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentativeEmail",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentativeName",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentativeNationalId",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentativePhone",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResearchGateLink",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumeUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SavedInterests",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPublicProfile",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SkillLevel",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UserGroupId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PermissionEnum = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLoginLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogoutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLoginLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLoginLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupPermissions",
                columns: table => new
                {
                    PermissionsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGroupsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupPermissions", x => new { x.PermissionsId, x.UserGroupsId });
                    table.ForeignKey(
                        name: "FK_UserGroupPermissions_Permissions_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupPermissions_UserGroups_UserGroupsId",
                        column: x => x.UserGroupsId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "IsActive", "Name", "PermissionEnum", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("19b4f26d-18bd-4ba3-828a-a1b142ee93e1"), "GET_USERS", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4106), "Can view all users", true, "Get Users", 9, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4106) },
                    { new Guid("1eadeb59-84f4-40e6-a87a-a2afa5bfb780"), "GET_USER_PERMISSIONS", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4116), "Can view user permissions", true, "Get User Permissions", 4104, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4116) },
                    { new Guid("23f289f4-212f-4c93-97ca-ce23d9979f8c"), "REGISTER_USER", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4101), "Can register new users", true, "Register User", 1, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4102) },
                    { new Guid("3b58eb5f-f35f-4581-8257-af708a343a51"), "GET_ROLES", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4103), "Can view roles", true, "Get Roles", 2, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4103) },
                    { new Guid("44704977-117e-45cd-bf2a-c811ec4f01ee"), "RESET_PASSWORD", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4113), "Can reset user passwords", true, "Reset Password", 4, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4113) },
                    { new Guid("5d8d2010-20ae-41b3-8410-1d4f894f1606"), "DELETE_USER", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4108), "Can delete users", true, "Delete User", 6, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4108) },
                    { new Guid("659c1d82-a170-4666-bb37-b23e4f93cb7e"), "SET_USER_PERMISSIONS", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4117), "Can set user permissions", true, "Set User Permissions", 46, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4117) },
                    { new Guid("72a00946-cf57-4ce2-94d3-8b527aea527e"), "GET_USER_LOGIN_LOGS", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4118), "Can view user login logs", true, "Get User Login Logs", 48, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4118) },
                    { new Guid("7b053445-0821-433b-8a9c-95aa5fd6c951"), "GET_PERMISSIONS", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4104), "Can view permissions", true, "Get Permissions", 10, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4105) },
                    { new Guid("98c814af-a257-4094-bf29-f6577315c4ad"), "SET_ROLE", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4114), "Can assign roles to users", true, "Set Role", 5, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4114) },
                    { new Guid("9b1f2500-9ee5-47e3-999b-6bf9f1125e7b"), "SET_ACCURACY", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4119), "Can set user accuracy", true, "Set Accuracy", 12, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4119) },
                    { new Guid("af2bdfbb-9cf0-4c26-9ecc-dbccae0c1269"), "SEARCH_USER", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4111), "Can search users", true, "Search User", 13, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4112) },
                    { new Guid("ead52393-02b9-4d0c-a9b8-b10ac620d487"), "GET_USER", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4099), "Can view user details", true, "Get User", 0, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4099) },
                    { new Guid("f1546d50-12e0-4e25-b845-3e245edefdd9"), "ADD_ROLE", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4115), "Can create new roles", true, "Add Role", 47, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4115) },
                    { new Guid("f9e93456-2cc6-450c-9dcc-67fa09c181ac"), "UPDATE_USER", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4107), "Can update user information", true, "Update User", 7, new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(4107) }
                });

            migrationBuilder.InsertData(
                table: "UserGroups",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("2f30ec39-4a9d-4faf-b7d2-dc02d638d94a"), new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(3939), "Default group for corporate users", true, "Corporate", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(3939) },
                    { new Guid("4086a4cd-ff38-402c-8a03-4dd9f2bbe10f"), new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(3931), "Administrator with full access", true, "Admin", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(3935) },
                    { new Guid("b2986afb-2b86-4e53-b6e5-069e2c61b837"), new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(3938), "Default group for individual users", true, "Individual", new DateTime(2025, 8, 2, 11, 28, 28, 666, DateTimeKind.Utc).AddTicks(3938) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserGroupId",
                table: "Users",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionEnum",
                table: "Permissions",
                column: "PermissionEnum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupPermissions_UserGroupsId",
                table: "UserGroupPermissions",
                column: "UserGroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_Name",
                table: "UserGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLoginLogs_UserId",
                table: "UserLoginLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserGroups_UserGroupId",
                table: "Users",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserGroups_UserGroupId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserGroupPermissions");

            migrationBuilder.DropTable(
                name: "UserLoginLogs");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserGroupId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActivityField",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyDescription",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyNationalId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyPhone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EducationLevel",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Expertise",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FullAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GoogleScholarLink",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Interests",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NationalCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OfficialDocumentsUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrcidLink",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RepresentativeEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RepresentativeName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RepresentativeNationalId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RepresentativePhone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResearchGateLink",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResumeUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SavedInterests",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShowPublicProfile",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SkillLevel",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserGroupId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Users");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("b423115b-653f-4bc1-b3b6-f08c4e0aa8d8"), new DateTime(2025, 8, 2, 7, 58, 40, 934, DateTimeKind.Utc).AddTicks(2879), "Content moderator", true, "Moderator", new DateTime(2025, 8, 2, 7, 58, 40, 934, DateTimeKind.Utc).AddTicks(2880) },
                    { new Guid("cba62a14-e639-4d3f-a150-8463e4c9ac77"), new DateTime(2025, 8, 2, 7, 58, 40, 934, DateTimeKind.Utc).AddTicks(2877), "Standard user", true, "User", new DateTime(2025, 8, 2, 7, 58, 40, 934, DateTimeKind.Utc).AddTicks(2878) },
                    { new Guid("e2421323-8a04-448d-8b25-a3c32f1ed40f"), new DateTime(2025, 8, 2, 7, 58, 40, 934, DateTimeKind.Utc).AddTicks(2870), "Administrator with full access", true, "Admin", new DateTime(2025, 8, 2, 7, 58, 40, 934, DateTimeKind.Utc).AddTicks(2871) }
                });
        }
    }
}
