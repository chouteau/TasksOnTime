
if not exists (select * from sysobjects where name = 'ScheduledTask' and xtype = 'U')
Begin
	Create table ScheduledTask (
		Id uniqueidentifier not null,
		Name nvarchar(200) not null,
		Period int not null,
		Interval int not null,
		StartDay int not null,
		StartHour int not null,
		StartMinute int not null,
		AssemblyQualifiedName nvarchar(200) not null,
		StartedCount int not null,
		Enabled bit not null,
		AllowMultipleInstance bit not null,
		AllowLocalMultipleInstances bit not null,
		NextRunningDate datetime2 not null,
		SerializedParameters varchar(5000) not null,
		Description nvarchar(1024) not null,
		ProcessMode int not null,
		CreationDate datetime2 not null,
		LastUpdate datetime2 not null,
		CONSTRAINT [PK_ScheduledTask] PRIMARY KEY ([Id])
	)
End
Go

if not exists (select * from sysobjects where name = 'HostRegistration' and xtype = 'U')
Begin
	Create table HostRegistration (
		Id uniqueidentifier not null,
		UniqueKey varchar(400) not null,
		MachineName varchar(200) not null,
		HostName varchar(200) not null,
		State int not null,
		CreationDate datetime2 not null,
		CONSTRAINT [PK_HostRegistration] PRIMARY KEY ([Id])
	)
End
Go

if not exists (select * from sysobjects where name = 'ProgressInfo' and xtype = 'U')
Begin
	Create table ProgressInfo (
		Id uniqueidentifier not null,
		CreationDate datetime2 not null,
		TaskId uniqueidentifier not null,
		Type int not null,
		GroupName nvarchar(100) null,
		Subject nvarchar(500) null,
		Body nvarchar(1024) null,
		EntityName nvarchar(500) null,
		EntityId nvarchar(100) null,
		SerializedEntity varchar(5000) null,
		TotalCount int null,
		ProgressIndex int null,
		CONSTRAINT [PK_ProgressInfo] PRIMARY KEY ([Id])
	)
End
Go

if not exists (select * from sysobjects where name = 'RunningTask' and xtype = 'U')
Begin
	Create table RunningTask (
		Id uniqueidentifier not null,
		TaskName nvarchar(200) not null,
		HostKey nvarchar(200) null,
		CreationDate DateTime2 not null,
		EnqueuedDate DateTime2 null,
		RunningDate DateTime2 null,
		CancelingDate DateTime2 null,
		CanceledDate DateTime2 null,
		TerminatedDate DateTime2 null,
		FailedDate DateTime2 null,
		ErrorStack varchar(5000),
		IsForced bit null,
		CONSTRAINT [PK_RunningTask] PRIMARY KEY ([Id])
	)
End
Go
