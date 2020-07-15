CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

CREATE TABLE "DataProtectionKeys" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DataProtectionKeys" PRIMARY KEY AUTOINCREMENT,
    "Created" TEXT NOT NULL,
    "Name" TEXT NULL,
    "Value" TEXT NOT NULL
);

CREATE TABLE "SigningKeys" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SigningKeys" PRIMARY KEY AUTOINCREMENT,
    "Created" TEXT NOT NULL,
    "Name" TEXT NULL,
    "Value" TEXT NOT NULL
);

CREATE UNIQUE INDEX "IX_DataProtectionKeys_Name" ON "DataProtectionKeys" ("Name");

CREATE UNIQUE INDEX "IX_SigningKeys_Name" ON "SigningKeys" ("Name");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20200417101952_KeyManagement', '3.1.3');

