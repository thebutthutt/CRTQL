CREATE PROCEDURE [GetTitleForHREmployeesWithMaxVacation] (
    @Gender NCHAR(1),
    @NumberOfYearsHired INT,
    @EmployeeCount INT OUTPUT,
    @MaxVacation INT OUTPUT
    )
AS
BEGIN TRY
    BEGIN TRANSACTION

    SELECT [JobTitle]
    FROM [AdventureWorks2017].[HumanResources].[Employee]
    WHERE [Gender] = @Gender
        AND DATEDIFF(YEAR, [HireDate], GETDATE()) < @NumberOfYearsHired;

    SELECT @EmployeeCount = @@ROWCOUNT;

    SELECT @MaxVacation = MAX([VacationHours])
    FROM [AdventureWorks2017].[HumanResources].[Employee]
    WHERE [Gender] = @Gender
        AND DATEDIFF(YEAR, [HireDate], GETDATE()) <= @NumberOfYearsHired;

    COMMIT TRANSACTION
END TRY

BEGIN CATCH
    IF (@@TRANCOUNT > 0)
    BEGIN
        ROLLBACK TRANSACTION

        PRINT 'Error detected, all changes reversed'
    END

    SELECT ERROR_NUMBER() AS [ErrorNumber],
        ERROR_SEVERITY() AS [ErrorSeverity],
        ERROR_STATE() AS [ErrorState],
        ERROR_PROCEDURE() AS [ErrorProcedure],
        ERROR_LINE() AS [ErrorLine],
        ERROR_MESSAGE() AS [ErrorMessage]
END CATCH
