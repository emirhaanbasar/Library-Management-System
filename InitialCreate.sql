IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE TABLE [Books] (
        [BookId] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Author] nvarchar(max) NOT NULL,
        [Genre] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        CONSTRAINT [PK_Books] PRIMARY KEY ([BookId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE TABLE [Seats] (
        [SeatId] int NOT NULL IDENTITY,
        [SeatNumber] int NOT NULL,
        CONSTRAINT [PK_Seats] PRIMARY KEY ([SeatId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [TC] nvarchar(450) NOT NULL,
        [Username] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Email] nvarchar(450) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [FaceId] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE TABLE [BookRentals] (
        [RentalId] int NOT NULL IDENTITY,
        [BookId] int NOT NULL,
        [UserId] int NOT NULL,
        [RentalDate] datetime2 NOT NULL,
        [ReturnDate] datetime2 NULL,
        CONSTRAINT [PK_BookRentals] PRIMARY KEY ([RentalId]),
        CONSTRAINT [FK_BookRentals_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE CASCADE,
        CONSTRAINT [FK_BookRentals_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE TABLE [SeatReservations] (
        [ReservationId] int NOT NULL IDENTITY,
        [SeatId] int NOT NULL,
        [UserId] int NOT NULL,
        [ReservationDate] datetime2 NOT NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        CONSTRAINT [PK_SeatReservations] PRIMARY KEY ([ReservationId]),
        CONSTRAINT [FK_SeatReservations_Seats_SeatId] FOREIGN KEY ([SeatId]) REFERENCES [Seats] ([SeatId]) ON DELETE CASCADE,
        CONSTRAINT [FK_SeatReservations_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BookRentals_BookId] ON [BookRentals] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BookRentals_UserId] ON [BookRentals] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SeatReservations_SeatId] ON [SeatReservations] ([SeatId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SeatReservations_UserId] ON [SeatReservations] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Seats_SeatNumber] ON [Seats] ([SeatNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_TC] ON [Users] ([TC]) WHERE [TC] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250718114746_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250718114746_InitialCreate', N'9.0.7');
END;

COMMIT;
GO

