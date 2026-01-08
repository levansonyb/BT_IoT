using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixLoginError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Users_user_id",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorData_Devices_device_id",
                table: "SensorData");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "SensorData",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "SensorData",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SensorData",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "received_at",
                table: "SensorData",
                newName: "ReceivedAt");

            migrationBuilder.RenameColumn(
                name: "device_id",
                table: "SensorData",
                newName: "DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_SensorData_device_id",
                table: "SensorData",
                newName: "IX_SensorData_DeviceId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Devices",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Devices",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Devices",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "is_online",
                table: "Devices",
                newName: "IsOnline");

            migrationBuilder.RenameColumn(
                name: "device_key",
                table: "Devices",
                newName: "DeviceKey");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Devices",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_user_id",
                table: "Devices",
                newName: "IX_Devices_UserId");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Users_UserId",
                table: "Devices",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorData_Devices_DeviceId",
                table: "SensorData",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Users_UserId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorData_Devices_DeviceId",
                table: "SensorData");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "SensorData",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "SensorData",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SensorData",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ReceivedAt",
                table: "SensorData",
                newName: "received_at");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "SensorData",
                newName: "device_id");

            migrationBuilder.RenameIndex(
                name: "IX_SensorData_DeviceId",
                table: "SensorData",
                newName: "IX_SensorData_device_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Devices",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Devices",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Devices",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "IsOnline",
                table: "Devices",
                newName: "is_online");

            migrationBuilder.RenameColumn(
                name: "DeviceKey",
                table: "Devices",
                newName: "device_key");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Devices",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                newName: "IX_Devices_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Users_user_id",
                table: "Devices",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorData_Devices_device_id",
                table: "SensorData",
                column: "device_id",
                principalTable: "Devices",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
