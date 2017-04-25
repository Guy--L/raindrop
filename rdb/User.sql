CREATE TABLE [dbo].[User]
(
	[UserId] INT NOT NULL PRIMARY KEY IDENTITY,
	[Name] varchar(32),
	[Password] varchar(64)
)
