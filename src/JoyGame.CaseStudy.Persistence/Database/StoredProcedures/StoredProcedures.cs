namespace JoyGame.CaseStudy.Persistence.Database.StoredProcedures;

public static class StoredProcedures
{
    public static class GetProductsWithCategories
    {
        public const string Drop = @"
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetProductsWithCategories')
                DROP PROCEDURE GetProductsWithCategories";

        public const string Create = @"
            CREATE PROCEDURE GetProductsWithCategories
            AS
            BEGIN
                SELECT 
                    p.Id AS ProductId,
                    p.Name AS ProductName,
                    p.Description AS ProductDescription,
                    p.Price,
                    p.StockQuantity,
                    p.BusinessStatus,
                    c.Id AS CategoryId,
                    c.Name AS CategoryName,
                    c.Description AS CategoryDescription
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.Status = 1 -- Active status
                ORDER BY c.Name, p.Name
            END";
    }

    public static class GetRecursiveCategories
    {
        public const string Drop = @"
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetRecursiveCategories')
                DROP PROCEDURE GetRecursiveCategories";

        public const string Create = @"
            CREATE PROCEDURE GetRecursiveCategories
            AS
            BEGIN
                WITH RecursiveCTE AS (
                    -- Anchor member
                    SELECT 
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Slug,
                        c.ParentId,
                        CAST(c.Name AS NVARCHAR(500)) as Hierarchy,
                        0 as Level
                    FROM Categories c
                    WHERE c.ParentId IS NULL
                    AND c.Status = 1 -- Active status

                    UNION ALL

                    -- Recursive member
                    SELECT 
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Slug,
                        c.ParentId,
                        CAST(rc.Hierarchy + ' > ' + c.Name AS NVARCHAR(500)),
                        rc.Level + 1
                    FROM Categories c
                    INNER JOIN RecursiveCTE rc ON c.ParentId = rc.Id
                    WHERE c.Status = 1 -- Active status
                )
                SELECT 
                    Id,
                    Name,
                    Description,
                    Slug,
                    ParentId,
                    Hierarchy,
                    Level
                FROM RecursiveCTE
                ORDER BY Hierarchy
            END";
    }
}