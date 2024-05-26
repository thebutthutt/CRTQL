DECLARE @ErrMsg = VARCHAR(500)

CREATE
    OR

ALTER PROCEDURE [GetTitleForHREmployeesWithMaxVacation] (
    @Gender NCHAR(1),
    @NumberOfYearsHired INT
    )
AS
BEGIN TRY
    BEGIN TRAN

    DECLARE @SomeTable TABLE (
        [col1] INT,
        [col2] VARCHAR(50),
        [col3] UNIQUEIDENTIFIER,
        [col4] BIT
        )

    SELECT
        [JobTitle],
        [Col3] AS [FakeName],
        [col4]
    FROM
        [AdventureWorks2017].[HumanResources].[Employee]
        INNER JOIN
        @SomeTable AS [SomeTable]
        ON [Employee].[Col1] = [SomeTable].[col1]
            AND [Employee].[Col2] = [SomeTable].[col2]
            AND [Employee].[Col3] = [SomeTable].[col3]
        JOIN
        [Scherm].[Trblk]
        ON [Scherm].[Trblk].[ccc] = [SomeTable].[col4]
    WHERE
        [Gender] = @Gender
        AND DATEDIFF(YEAR, [HireDate], GETDATE()) < @NumberOfYearsHired;

    COMMIT TRAN
END TRY

BEGIN CATCH
    IF (@@TRANCOUNT > 0)
    BEGIN
        ROLLBACK TRAN

        SET @ErrMsg = CONCAT (
                (
                    SELECT
                        ERROR_PROCEDURE()
                    ),
                '>',
                (
                    SELECT
                        ERROR_LINE()
                    ),
                ':',
                (
                    SELECT
                        ERROR_MESSAGE()
                    )
                )

        RAISERROR (@ErrMsg)
    END
END CATCH
