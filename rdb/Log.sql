CREATE TABLE [dbo].[Log]
(
	[LogId] INT NOT NULL PRIMARY KEY IDENTITY,
	[DeviceId] int not null,
	[Stamp] DateTime not null,
	[UserId] int not null, 
    [Status] INT NOT NULL
)
