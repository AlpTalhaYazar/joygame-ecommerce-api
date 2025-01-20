namespace JoyGame.CaseStudy.Persistence.Database.StoredProcedures;

public static class StoredProcedures
{
    public const string GetProductsWithCategories = @"
        CREATE OR ALTER PROCEDURE [dbo].[GetProductsWithCategories]
            @CategoryId INT = NULL
        AS
        BEGIN
            WITH RecursiveCategories AS (
                -- Anchor member: Get the initial category
                SELECT Id, ParentId, Name
                FROM Categories
                WHERE (@CategoryId IS NULL OR Id = @CategoryId)
                AND Status != 3 -- Not Deleted

                UNION ALL

                -- Recursive member: Get all child categories
                SELECT c.Id, c.ParentId, c.Name
                FROM Categories c
                INNER JOIN RecursiveCategories rc ON c.ParentId = rc.Id
                WHERE c.Status != 3 -- Not Deleted
            )
            SELECT 
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.BusinessStatus,
                c.Id AS CategoryId,
                c.Name AS CategoryName
            FROM Products p
            INNER JOIN Categories c ON p.CategoryId = c.Id
            WHERE c.Id IN (SELECT Id FROM RecursiveCategories)
            AND p.Status != 3 -- Not Deleted
            ORDER BY c.Name, p.Name;
        END";

    public const string GetCategoryHierarchy = @"
        CREATE OR ALTER PROCEDURE [dbo].[GetCategoryHierarchy]
        AS
        BEGIN
            WITH RecursiveCategories AS (
                -- Anchor member: Start with root categories (no parent)
                SELECT 
                    Id,
                    ParentId,
                    Name,
                    0 AS Level,
                    CAST(Name AS NVARCHAR(MAX)) AS Path
                FROM Categories
                WHERE ParentId IS NULL
                AND Status != 3 -- Not Deleted

                UNION ALL

                -- Recursive member: Get all child categories
                SELECT 
                    c.Id,
                    c.ParentId,
                    c.Name,
                    rc.Level + 1,
                    CAST(rc.Path + ' > ' + c.Name AS NVARCHAR(MAX))
                FROM Categories c
                INNER JOIN RecursiveCategories rc ON c.ParentId = rc.Id
                WHERE c.Status != 3 -- Not Deleted
            )
            SELECT 
                Id,
                ParentId,
                Name,
                Level,
                Path
            FROM RecursiveCategories
            ORDER BY Path;
        END";
}