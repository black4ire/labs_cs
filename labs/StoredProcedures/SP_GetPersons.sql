-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_GetPersons] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select
	[Person].[Person].[FirstName],
	[Person].[Person].[LastName],
	[Person].[Address].[City],
	[Person].[BusinessEntity].[rowguid],
	[Person].[PersonPhone].[PhoneNumber],
	[HumanResources].[EmployeePayHistory].[ModifiedDate],
	[Sales].[Customer].[AccountNumber]
	from [Person].[Person]
	join [Person].[Address] on [Person].[Address].[AddressID] = [Person].[Person].[BusinessEntityID]
	join [Person].[BusinessEntity] on [Person].[BusinessEntity].[BusinessEntityID] = [Person].[Person].[BusinessEntityID]
	join [Person].[PersonPhone] on [Person].[PersonPhone].[BusinessEntityID] = [Person].[Person].[BusinessEntityID]
	join [HumanResources].[EmployeePayHistory] on [HumanResources].[EmployeePayHistory].[BusinessEntityID] = [Person].[Person].[BusinessEntityID]
	join [Sales].[Customer] on [Sales].[Customer].[CustomerID] = [Person].[Person].[BusinessEntityID]
END;
GO
