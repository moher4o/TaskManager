/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [FullName]
      ,[JobTitleId]
      ,[SectorId]
      ,[DepartmentId]
      ,[DirectorateId]
      ,[TelephoneNumber]
      ,[MobileNumber]
      ,[DaeuAccaunt]
      ,[Id]
      ,[Email]
      ,[RoleId]
      ,[isDeleted]
  FROM [TaskManager].[dbo].[Employees]

  insert into dbo.Employees(FullName,JobTitleId,DepartmentId,DirectorateId,DaeuAccaunt,RoleId)
   Values(N'Ангел Иванов Вуков',N'17',N'3',N'1',N'ryzen7\samso',N'1')