using System.Security.Cryptography;
using System.Text;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Domain.Enums;
using JoyGame.CaseStudy.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JoyGame.CaseStudy.Persistence.Database;

public class DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync()
    {
        try
        {
            await context.Database.MigrateAsync();

            await SeedRolesAndPermissionsAsync();
            await SeedAdminUserAsync();
            await SeedCategoriesAsync();
            await SeedProductsAsync();

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedRolesAndPermissionsAsync()
    {
        if (!context.Permissions.Any())
        {
            var permissions = new List<Permission>
            {
                new() { Name = "product_view", Description = "Can view products" },
                new() { Name = "product_manage", Description = "Can manage products" },
                new() { Name = "category_view", Description = "Can view categories" },
                new() { Name = "category_manage", Description = "Can manage categories" },
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }

        if (!context.Roles.Any())
        {
            var adminRole = new Role { Name = "Administrator", Description = "System Administrator" };
            var managerRole = new Role { Name = "Manager", Description = "Store Manager" };
            var userRole = new Role { Name = "User", Description = "Regular User" };

            await context.Roles.AddRangeAsync(new[] { adminRole, managerRole, userRole });
            await context.SaveChangesAsync();

            var allPermissions = await context.Permissions.ToListAsync();

            foreach (var permission in allPermissions)
            {
                await context.RolePermissions.AddAsync(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            foreach (var permission in allPermissions)
            {
                await context.RolePermissions.AddAsync(new RolePermission
                {
                    RoleId = managerRole.Id,
                    PermissionId = permission.Id
                });
            }

            var viewPermissions = allPermissions.Where(p => p.Name.EndsWith("_view"));
            foreach (var permission in viewPermissions)
            {
                await context.RolePermissions.AddAsync(new RolePermission
                {
                    RoleId = userRole.Id,
                    PermissionId = permission.Id
                });
            }

            await context.SaveChangesAsync();
        }
    }

    private async Task SeedAdminUserAsync()
    {
        if (!context.Users.Any())
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");

            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@joygame.com",
                PasswordHash = HashPassword("Admin123!"),
                FirstName = "System",
                LastName = "Administrator",
                Status = EntityStatus.Active,
                BusinessStatus = UserStatus.Active,
                EmailConfirmed = true
            };
            var normalUser = new User
            {
                Username = "user",
                Email = "user@joygame.com",
                PasswordHash = HashPassword("User123!"),
                FirstName = "Regular",
                LastName = "User",
                Status = EntityStatus.Active,
                BusinessStatus = UserStatus.Active,
                EmailConfirmed = true
            };
            var users = new List<User> { adminUser, normalUser };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                },
                new UserRole
                {
                    UserId = normalUser.Id,
                    RoleId = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User").ContinueWith(r => r.Result.Id)
                }
            };

            await context.UserRoles.AddRangeAsync(userRoles);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (!context.Categories.Any())
        {
            var parentCategories = new List<Category>
            {
                new Category
                {
                    Name = "Games",
                    Description = "Video Games",
                    Slug = "games",
                    Status = EntityStatus.Active,
                    ParentId = null
                },
                new Category
                {
                    Name = "Consoles",
                    Description = "Gaming Consoles",
                    Slug = "consoles",
                    Status = EntityStatus.Active,
                    ParentId = null
                },
                new Category
                {
                    Name = "Accessories",
                    Description = "Gaming Accessories",
                    Slug = "accessories",
                    Status = EntityStatus.Active,
                    ParentId = null
                },
                new Category
                {
                    Name = "Merchandise",
                    Description = "Gaming Merchandise",
                    Slug = "merchandise",
                    Status = EntityStatus.Active,
                    ParentId = null
                }
            };

            await context.Categories.AddRangeAsync(parentCategories);
            await context.SaveChangesAsync();

            var gamesCategory = await context.Categories.FirstAsync(c => c.Name == "Games");
            var consolesCategory = await context.Categories.FirstAsync(c => c.Name == "Consoles");
            var accessoriesCategory = await context.Categories.FirstAsync(c => c.Name == "Accessories");
            var merchandiseCategory = await context.Categories.FirstAsync(c => c.Name == "Merchandise");

            var childCategories = new List<Category>
            {
                new Category
                {
                    Name = "Action Games",
                    Description = "Action and Adventure Games",
                    Slug = "action-games",
                    Status = EntityStatus.Active,
                    ParentId = gamesCategory.Id
                },
                new Category
                {
                    Name = "RPG Games",
                    Description = "Role Playing Games",
                    Slug = "rpg-games",
                    Status = EntityStatus.Active,
                    ParentId = gamesCategory.Id
                },
                new Category
                {
                    Name = "Sports Games",
                    Description = "Sports and Racing Games",
                    Slug = "sports-games",
                    Status = EntityStatus.Active,
                    ParentId = gamesCategory.Id
                },
                new Category
                {
                    Name = "Puzzle Games",
                    Description = "Puzzle and Strategy Games",
                    Slug = "puzzle-games",
                    Status = EntityStatus.Active,
                    ParentId = gamesCategory.Id
                },
                new Category
                {
                    Name = "Shooter Games",
                    Description = "First and Third Person Shooter Games",
                    Slug = "shooter-games",
                    Status = EntityStatus.Active,
                    ParentId = gamesCategory.Id
                },
                new Category
                {
                    Name = "PlayStation",
                    Description = "PlayStation Consoles",
                    Slug = "playstation",
                    Status = EntityStatus.Active,
                    ParentId = consolesCategory.Id
                },
                new Category
                {
                    Name = "Xbox",
                    Description = "Xbox Consoles",
                    Slug = "xbox",
                    Status = EntityStatus.Active,
                    ParentId = consolesCategory.Id
                },
                new Category
                {
                    Name = "Nintendo",
                    Description = "Nintendo Consoles",
                    Slug = "nintendo",
                    Status = EntityStatus.Active,
                    ParentId = consolesCategory.Id
                },
                new Category
                {
                    Name = "Controllers",
                    Description = "Gaming Controllers",
                    Slug = "controllers",
                    Status = EntityStatus.Active,
                    ParentId = accessoriesCategory.Id
                },
                new Category
                {
                    Name = "Headsets",
                    Description = "Gaming Headsets",
                    Slug = "headsets",
                    Status = EntityStatus.Active,
                    ParentId = accessoriesCategory.Id
                },
                new Category
                {
                    Name = "Keyboards",
                    Description = "Gaming Keyboards",
                    Slug = "keyboards",
                    Status = EntityStatus.Active,
                    ParentId = accessoriesCategory.Id
                },
                new Category
                {
                    Name = "T-Shirts",
                    Description = "Gaming T-Shirts",
                    Slug = "t-shirts",
                    Status = EntityStatus.Active,
                    ParentId = merchandiseCategory.Id
                },
                new Category
                {
                    Name = "Posters",
                    Description = "Gaming Posters",
                    Slug = "posters",
                    Status = EntityStatus.Active,
                    ParentId = merchandiseCategory.Id
                },
                new Category
                {
                    Name = "Figurines",
                    Description = "Gaming Figurines",
                    Slug = "figurines",
                    Status = EntityStatus.Active,
                    ParentId = merchandiseCategory.Id
                }
            };

            await context.Categories.AddRangeAsync(childCategories);
            await context.SaveChangesAsync();

            logger.LogInformation("Categories seeded successfully");
        }
    }

    private async Task SeedProductsAsync()
    {
        if (!context.Products.Any())
        {
            var gamesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Games");
            var consolesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Consoles");
            var accessoriesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Accessories");
            var merchandiseCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Merchandise");
            var actionGamesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Action Games");
            var rpgGamesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "RPG Games");
            var sportsGamesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sports Games");
            var puzzleGamesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Puzzle Games");
            var shooterGamesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Shooter Games");
            var playstationCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "PlayStation");
            var xboxCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Xbox");
            var nintendoCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Nintendo");
            var controllersCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Controllers");
            var headsetsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Headsets");
            var keyboardsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Keyboards");
            var tshirtsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "T-Shirts");
            var postersCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Posters");
            var figurinesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Figurines");


            var products = new List<Product>
            {
                new()
                {
                    Name = "The Last of Us Part II",
                    Description = "Intense action-adventure survival game",
                    Slug = "last-of-us-2",
                    Price = 59.99m,
                    CategoryId = actionGamesCategory.Id,
                    StockQuantity = 150,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "MLB The Show 24",
                    Description = "Premier baseball simulation game",
                    Slug = "mlb-the-show-24",
                    Price = 59.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 100,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "F1 24",
                    Description = "Official Formula 1 racing simulation",
                    Slug = "f1-24",
                    Price = 69.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 85,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Madden NFL 24",
                    Description = "Latest American football simulation",
                    Slug = "madden-24",
                    Price = 59.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 120,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "WWE 2K24",
                    Description = "Professional wrestling game with extensive roster",
                    Slug = "wwe-2k24",
                    Price = 59.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 90,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Call of Duty: Warzone Mobile",
                    Description = "Mobile version of the popular battle royale game",
                    Slug = "cod-warzone-mobile",
                    Price = 0.00m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 999,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Destiny 2: The Final Shape",
                    Description = "Latest expansion for the online shooter",
                    Slug = "destiny-2-final-shape",
                    Price = 49.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 200,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Counter-Strike 2",
                    Description = "Next generation of tactical team-based shooter",
                    Slug = "counter-strike-2",
                    Price = 0.00m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 999,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Overwatch 2 Premium Battle Pass",
                    Description = "Season pass for hero shooter with exclusive content",
                    Slug = "overwatch-2-premium-pass",
                    Price = 9.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 999,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PlayStation 5 God of War Ragnarök Bundle",
                    Description = "PS5 console with God of War Ragnarök game",
                    Slug = "ps5-gow-bundle",
                    Price = 559.99m,
                    CategoryId = playstationCategory.Id,
                    StockQuantity = 45,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PlayStation Portal",
                    Description = "Remote player for PlayStation 5",
                    Slug = "playstation-portal",
                    Price = 199.99m,
                    CategoryId = playstationCategory.Id,
                    StockQuantity = 75,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Xbox Series X Starfield Bundle",
                    Description = "Xbox Series X with Starfield Digital Premium Edition",
                    Slug = "xbox-series-x-starfield",
                    Price = 599.99m,
                    CategoryId = xboxCategory.Id,
                    StockQuantity = 60,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Xbox Series S Starter Bundle",
                    Description = "Xbox Series S with 3 months Game Pass Ultimate",
                    Slug = "xbox-series-s-starter",
                    Price = 299.99m,
                    CategoryId = xboxCategory.Id,
                    StockQuantity = 100,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Nintendo Switch OLED Zelda Edition",
                    Description = "Limited edition OLED Switch with Tears of the Kingdom design",
                    Slug = "switch-oled-zelda",
                    Price = 359.99m,
                    CategoryId = nintendoCategory.Id,
                    StockQuantity = 40,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Nintendo Switch Lite Pokemon Edition",
                    Description = "Special Pokemon-themed Switch Lite console",
                    Slug = "switch-lite-pokemon",
                    Price = 199.99m,
                    CategoryId = nintendoCategory.Id,
                    StockQuantity = 85,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Red Dead Redemption 2",
                    Description = "Epic Western-themed action-adventure game",
                    Slug = "red-dead-redemption-2",
                    Price = 49.99m,
                    CategoryId = actionGamesCategory.Id,
                    StockQuantity = 120,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Spider-Man 2",
                    Description = "Superhero action-adventure game featuring Peter Parker and Miles Morales",
                    Slug = "spider-man-2",
                    Price = 69.99m,
                    CategoryId = actionGamesCategory.Id,
                    StockQuantity = 200,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Elden Ring",
                    Description = "Open-world action RPG from FromSoftware",
                    Slug = "elden-ring",
                    Price = 59.99m,
                    CategoryId = actionGamesCategory.Id,
                    StockQuantity = 175,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Horizon Forbidden West",
                    Description = "Action RPG set in a post-apocalyptic world",
                    Slug = "horizon-forbidden-west",
                    Price = 69.99m,
                    CategoryId = actionGamesCategory.Id,
                    StockQuantity = 160,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Resident Evil 4 Remake",
                    Description = "Reimagined survival horror classic",
                    Slug = "resident-evil-4-remake",
                    Price = 59.99m,
                    CategoryId = actionGamesCategory.Id,
                    StockQuantity = 140,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Devil May Cry 5 Special Edition",
                    Description = "High-octane action game with multiple playable characters",
                    Slug = "devil-may-cry-5-special",
                    Price = 39.99m,
                    CategoryId = actionGamesCategory.Id,
                    StockQuantity = 90,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Sekiro: Shadows Die Twice",
                    Description = "Intense action game set in feudal Japan",
                    Slug = "sekiro",
                    Price = 49.99m,
                    CategoryId = actionGamesCategory.Id,
                    StockQuantity = 85,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Ghost of Tsushima Director's Cut",
                    Description = "Open-world samurai adventure with expanded content",
                    Slug = "ghost-of-tsushima-directors-cut",
                    Price = 59.99m,
                    CategoryId = actionGamesCategory.Id,
                    StockQuantity = 110,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Hogwarts Legacy",
                    Description = "Open-world action RPG set in the Wizarding World",
                    Slug = "hogwarts-legacy",
                    Price = 69.99m,
                    CategoryId = actionGamesCategory.Id,
                    StockQuantity = 200,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Diablo IV",
                    Description = "Dark fantasy action RPG",
                    Slug = "diablo-4",
                    Price = 69.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 180,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Final Fantasy VII Rebirth",
                    Description = "Next chapter in the Final Fantasy VII remake series",
                    Slug = "ff7-rebirth",
                    Price = 69.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 250,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Persona 6",
                    Description = "Latest entry in the popular JRPG series",
                    Slug = "persona-6",
                    Price = 59.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 150,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Dragon's Dogma 2",
                    Description = "Action RPG with dynamic combat and vast open world",
                    Slug = "dragons-dogma-2",
                    Price = 69.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 120,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Star Wars: Knights of the Old Republic Remake",
                    Description = "Rebuilt classic RPG set in the Star Wars universe",
                    Slug = "kotor-remake",
                    Price = 69.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 160,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "EA Sports FC 24 Ultimate Edition",
                    Description = "Premium version of the football simulation with exclusive content",
                    Slug = "ea-sports-fc-24-ultimate",
                    Price = 99.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 150,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "NBA 2K24 Championship Edition",
                    Description = "Definitive basketball simulation with all content",
                    Slug = "nba-2k24-championship",
                    Price = 99.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 130,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PGA Tour 2K24",
                    Description = "Professional golf simulation with licensed courses",
                    Slug = "pga-tour-2k24",
                    Price = 59.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 75,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "UFC 5",
                    Description = "Ultimate fighting championship game with realistic graphics",
                    Slug = "ufc-5",
                    Price = 69.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 90,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Gran Turismo 7",
                    Description = "Premier racing simulation for PlayStation",
                    Slug = "gran-turismo-7",
                    Price = 69.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 110,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Tennis World Tour 3",
                    Description = "Professional tennis simulation with career mode",
                    Slug = "tennis-world-tour-3",
                    Price = 49.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 60,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Cricket 24",
                    Description = "Official cricket game with international teams",
                    Slug = "cricket-24",
                    Price = 49.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 70,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Tony Hawk's Pro Skater 1+2",
                    Description = "Remastered skateboarding classics",
                    Slug = "tony-hawk-pro-skater-1-2",
                    Price = 39.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 85,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "eFootball 2024",
                    Description = "Free-to-play football game with premium content",
                    Slug = "efootball-2024",
                    Price = 0.00m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 999,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Riders Republic",
                    Description = "Extreme sports open world game",
                    Slug = "riders-republic",
                    Price = 49.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 95,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "NHL 24",
                    Description = "Official NHL ice hockey simulation",
                    Slug = "nhl-24",
                    Price = 59.99m,
                    CategoryId = sportsGamesCategory.Id,
                    StockQuantity = 80,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Portal: Companion Collection",
                    Description = "Bundle of Portal and Portal 2 puzzle games",
                    Slug = "portal-companion-collection",
                    Price = 19.99m,
                    CategoryId = puzzleGamesCategory.Id,
                    StockQuantity = 150,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Braid: Anniversary Edition",
                    Description = "Remastered version of the classic time-manipulation puzzle game",
                    Slug = "braid-anniversary",
                    Price = 29.99m,
                    CategoryId = puzzleGamesCategory.Id,
                    StockQuantity = 100,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "The Witness",
                    Description = "Open-world puzzle game with unique mechanics",
                    Slug = "the-witness",
                    Price = 39.99m,
                    CategoryId = puzzleGamesCategory.Id,
                    StockQuantity = 75,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Talos Principle 2",
                    Description = "Philosophical first-person puzzle game",
                    Slug = "talos-principle-2",
                    Price = 49.99m,
                    CategoryId = puzzleGamesCategory.Id,
                    StockQuantity = 120,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Cocoon",
                    Description = "Unique puzzle adventure from the creators of LIMBO",
                    Slug = "cocoon",
                    Price = 24.99m,
                    CategoryId = puzzleGamesCategory.Id,
                    StockQuantity = 90,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "The Last Campfire",
                    Description = "Cozy puzzle adventure about finding hope",
                    Slug = "last-campfire",
                    Price = 14.99m,
                    CategoryId = puzzleGamesCategory.Id,
                    StockQuantity = 85,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Outer Wilds",
                    Description = "Space exploration puzzle adventure",
                    Slug = "outer-wilds",
                    Price = 24.99m,
                    CategoryId = puzzleGamesCategory.Id,
                    StockQuantity = 110,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Superliminal",
                    Description = "Mind-bending perspective puzzle game",
                    Slug = "superliminal",
                    Price = 19.99m,
                    CategoryId = puzzleGamesCategory.Id,
                    StockQuantity = 95,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "We Were Here Forever",
                    Description = "Cooperative puzzle adventure",
                    Slug = "we-were-here-forever",
                    Price = 29.99m,
                    CategoryId = puzzleGamesCategory.Id,
                    StockQuantity = 70,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Pikmin 4",
                    Description = "Strategic puzzle adventure with tiny creatures",
                    Slug = "pikmin-4",
                    Price = 59.99m,
                    CategoryId = puzzleGamesCategory.Id,
                    StockQuantity = 130,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Tales of Arise",
                    Description = "Action RPG with stunning visuals",
                    Slug = "tales-of-arise",
                    Price = 59.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 85,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Octopath Traveler II",
                    Description = "Classic-style JRPG with multiple protagonists",
                    Slug = "octopath-traveler-2",
                    Price = 59.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 95,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Sea of Stars",
                    Description = "Retro-inspired turn-based RPG",
                    Slug = "sea-of-stars",
                    Price = 34.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 110,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Like a Dragon: Infinite Wealth",
                    Description = "Latest entry in the Yakuza RPG series",
                    Slug = "like-a-dragon-infinite-wealth",
                    Price = 69.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 120,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Lies of P",
                    Description = "Soulslike RPG inspired by Pinocchio",
                    Slug = "lies-of-p",
                    Price = 59.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 75,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Dragon Quest XII",
                    Description = "Newest entry in the classic JRPG series",
                    Slug = "dragon-quest-12",
                    Price = 59.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 150,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Path of Exile 2",
                    Description = "Free-to-play action RPG sequel",
                    Slug = "path-of-exile-2",
                    Price = 0.00m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 999,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Eiyuden Chronicle: Hundred Heroes",
                    Description = "Spiritual successor to Suikoden series",
                    Slug = "eiyuden-chronicle",
                    Price = 49.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 80,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Lost Ark Gold Pack",
                    Description = "Premium currency pack for MMO RPG",
                    Slug = "lost-ark-gold",
                    Price = 49.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 999,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Black Desert Online Ultimate Edition",
                    Description = "Complete edition of the popular MMORPG",
                    Slug = "black-desert-ultimate",
                    Price = 99.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 200,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Granblue Fantasy: Relink",
                    Description = "Action RPG based on popular mobile game",
                    Slug = "granblue-fantasy-relink",
                    Price = 59.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 90,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Bloodborne Remastered",
                    Description = "Updated version of the gothic action RPG",
                    Slug = "bloodborne-remastered",
                    Price = 49.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 180,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Fable IV",
                    Description = "New entry in the fantasy RPG series",
                    Slug = "fable-4",
                    Price = 69.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 160,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Dragon Age: Dreadwolf",
                    Description = "Next chapter in the Dragon Age series",
                    Slug = "dragon-age-dreadwolf",
                    Price = 69.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 200,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Monster Hunter World: Ultimate Edition",
                    Description = "Complete package with all expansions",
                    Slug = "monster-hunter-world-ultimate",
                    Price = 59.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 95,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Starfield Premium Edition",
                    Description = "Premium version with exclusive content",
                    Slug = "starfield-premium",
                    Price = 99.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 150,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Final Fantasy XIV Complete Edition",
                    Description = "MMO RPG with all expansions included",
                    Slug = "ffxiv-complete",
                    Price = 59.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 300,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "The Elder Scrolls Online Collection",
                    Description = "MMO RPG with all chapters and expansions",
                    Slug = "eso-collection",
                    Price = 59.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 250,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Cyberpunk 2077: Ultimate Edition",
                    Description = "Complete edition with Phantom Liberty expansion",
                    Slug = "cyberpunk-2077-ultimate",
                    Price = 69.99m,
                    CategoryId = rpgGamesCategory.Id,
                    StockQuantity = 180,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Battlefield 2024",
                    Description = "Next-generation military shooter",
                    Slug = "battlefield-2024",
                    Price = 69.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 200,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Apex Legends Champion Edition",
                    Description = "Premium bundle for battle royale game",
                    Slug = "apex-legends-champion",
                    Price = 39.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 150,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Rainbow Six: Siege Year 9 Pass",
                    Description = "Annual content pass for tactical shooter",
                    Slug = "r6-siege-year-9",
                    Price = 29.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 999,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "DOOM Eternal Deluxe Edition",
                    Description = "Complete package with all DLC content",
                    Slug = "doom-eternal-deluxe",
                    Price = 69.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 120,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Valorant Premium Battle Pass",
                    Description = "Season pass with exclusive rewards",
                    Slug = "valorant-battle-pass",
                    Price = 9.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 999,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Metro Saga Bundle",
                    Description = "Complete collection of Metro series",
                    Slug = "metro-saga-bundle",
                    Price = 59.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 85,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Titanfall 3",
                    Description = "Fast-paced shooter with mechs",
                    Slug = "titanfall-3",
                    Price = 69.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 160,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Borderlands 4",
                    Description = "New chapter in the looter-shooter series",
                    Slug = "borderlands-4",
                    Price = 69.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 175,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Halo Infinite Season Pass",
                    Description = "Latest season content for Halo Infinite",
                    Slug = "halo-infinite-season-pass",
                    Price = 9.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 999,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Back 4 Blood Ultimate Edition",
                    Description = "Cooperative zombie shooter complete edition",
                    Slug = "back-4-blood-ultimate",
                    Price = 49.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 90,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Payday 3 Deluxe Edition",
                    Description = "Premium version of the heist shooter",
                    Slug = "payday-3-deluxe",
                    Price = 79.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 110,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "The Division 3",
                    Description = "Online tactical shooter RPG",
                    Slug = "division-3",
                    Price = 69.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 140,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Insurgency: Sandstorm Gold Edition",
                    Description = "Tactical military shooter complete edition",
                    Slug = "insurgency-sandstorm-gold",
                    Price = 49.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 70,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Left 4 Dead 3",
                    Description = "Cooperative zombie shooter sequel",
                    Slug = "left-4-dead-3",
                    Price = 59.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 190,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Hunt: Showdown Ultimate Hunter Edition",
                    Description = "Premium package for the PvPvE shooter",
                    Slug = "hunt-showdown-ultimate",
                    Price = 59.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 85,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Ready or Not",
                    Description = "Tactical SWAT simulation shooter",
                    Slug = "ready-or-not",
                    Price = 39.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 95,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Escape from Tarkov Edge of Darkness",
                    Description = "Premium edition of hardcore shooter",
                    Slug = "tarkov-edge-of-darkness",
                    Price = 139.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 60,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Hell Let Loose Ultimate Edition",
                    Description = "WW2 tactical shooter complete edition",
                    Slug = "hell-let-loose-ultimate",
                    Price = 49.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 75,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "World War 3 Veteran Package",
                    Description = "Military shooter premium bundle",
                    Slug = "ww3-veteran-package",
                    Price = 59.99m,
                    CategoryId = shooterGamesCategory.Id,
                    StockQuantity = 80,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Razer BlackWidow V4 Pro",
                    Description = "Premium mechanical gaming keyboard with optical switches",
                    Slug = "razer-blackwidow-v4-pro",
                    Price = 229.99m,
                    CategoryId = keyboardsCategory.Id,
                    StockQuantity = 50,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Corsair K100 RGB",
                    Description = "High-end mechanical gaming keyboard with media controls",
                    Slug = "corsair-k100-rgb",
                    Price = 199.99m,
                    CategoryId = keyboardsCategory.Id,
                    StockQuantity = 45,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "SteelSeries Apex Pro TKL",
                    Description = "Tenkeyless gaming keyboard with adjustable switches",
                    Slug = "steelseries-apex-pro-tkl",
                    Price = 189.99m,
                    CategoryId = keyboardsCategory.Id,
                    StockQuantity = 60,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Logitech G915 TKL Wireless",
                    Description = "Wireless mechanical gaming keyboard with low profile",
                    Slug = "logitech-g915-tkl",
                    Price = 229.99m,
                    CategoryId = keyboardsCategory.Id,
                    StockQuantity = 40,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "HyperX Alloy Origins Core",
                    Description = "Compact mechanical gaming keyboard",
                    Slug = "hyperx-alloy-origins-core",
                    Price = 89.99m,
                    CategoryId = keyboardsCategory.Id,
                    StockQuantity = 75,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "ROCCAT Vulcan II Max",
                    Description = "Premium optical gaming keyboard with wrist rest",
                    Slug = "roccat-vulcan-2-max",
                    Price = 199.99m,
                    CategoryId = keyboardsCategory.Id,
                    StockQuantity = 35,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Razer Huntsman Mini",
                    Description = "60% compact optical gaming keyboard",
                    Slug = "razer-huntsman-mini",
                    Price = 119.99m,
                    CategoryId = keyboardsCategory.Id,
                    StockQuantity = 65,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Corsair K70 RGB PRO",
                    Description = "Tournament-ready mechanical gaming keyboard",
                    Slug = "corsair-k70-rgb-pro",
                    Price = 169.99m,
                    CategoryId = keyboardsCategory.Id,
                    StockQuantity = 55,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "SteelSeries Arctis Nova Pro",
                    Description = "High-end wireless gaming headset with active noise cancellation",
                    Slug = "steelseries-arctis-nova-pro",
                    Price = 349.99m,
                    CategoryId = headsetsCategory.Id,
                    StockQuantity = 30,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Razer BlackShark V2 Pro (2024)",
                    Description = "Premium wireless gaming headset with THX audio",
                    Slug = "razer-blackshark-v2-pro-2024",
                    Price = 199.99m,
                    CategoryId = headsetsCategory.Id,
                    StockQuantity = 45,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "HyperX Cloud III Wireless",
                    Description = "Premium wireless gaming headset with DTS",
                    Slug = "hyperx-cloud-3-wireless",
                    Price = 169.99m,
                    CategoryId = headsetsCategory.Id,
                    StockQuantity = 55,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Logitech G Pro X 2 Lightspeed",
                    Description = "Professional wireless gaming headset",
                    Slug = "logitech-g-pro-x-2-lightspeed",
                    Price = 249.99m,
                    CategoryId = headsetsCategory.Id,
                    StockQuantity = 40,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Corsair HS80 RGB Wireless",
                    Description = "Premium wireless gaming headset with Dolby Atmos",
                    Slug = "corsair-hs80-rgb-wireless",
                    Price = 149.99m,
                    CategoryId = headsetsCategory.Id,
                    StockQuantity = 60,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "ASTRO Gaming A50 Gen 5",
                    Description = "Wireless gaming headset with charging base station",
                    Slug = "astro-a50-gen-5",
                    Price = 299.99m,
                    CategoryId = headsetsCategory.Id,
                    StockQuantity = 35,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "DualSense Edge White",
                    Description = "Premium PS5 controller with customizable buttons",
                    Slug = "dualsense-edge-white",
                    Price = 199.99m,
                    CategoryId = controllersCategory.Id,
                    StockQuantity = 50,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "DualSense Edge Black",
                    Description = "Premium PS5 controller with customizable buttons in black",
                    Slug = "dualsense-edge-black",
                    Price = 199.99m,
                    CategoryId = controllersCategory.Id,
                    StockQuantity = 45,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Xbox Elite Controller Series 3",
                    Description = "Next-gen premium Xbox controller",
                    Slug = "xbox-elite-controller-series-3",
                    Price = 179.99m,
                    CategoryId = controllersCategory.Id,
                    StockQuantity = 40,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "SCUF Reflex Pro",
                    Description = "Professional PS5 custom controller",
                    Slug = "scuf-reflex-pro",
                    Price = 219.99m,
                    CategoryId = controllersCategory.Id,
                    StockQuantity = 30,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Razer Wolverine V3 Pro",
                    Description = "Premium Xbox/PC wireless controller",
                    Slug = "razer-wolverine-v3-pro",
                    Price = 249.99m,
                    CategoryId = controllersCategory.Id,
                    StockQuantity = 35,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PowerA Enhanced Wired Controller",
                    Description = "Budget-friendly wired Xbox controller",
                    Slug = "powera-enhanced-wired",
                    Price = 29.99m,
                    CategoryId = controllersCategory.Id,
                    StockQuantity = 100,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Nintendo Switch Pro Controller",
                    Description = "Official premium Switch controller",
                    Slug = "switch-pro-controller",
                    Price = 69.99m,
                    CategoryId = controllersCategory.Id,
                    StockQuantity = 80,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "8BitDo Ultimate Controller",
                    Description = "Wireless controller with charging dock",
                    Slug = "8bitdo-ultimate-controller",
                    Price = 69.99m,
                    CategoryId = controllersCategory.Id,
                    StockQuantity = 60,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Thrustmaster eSwap X Pro",
                    Description = "Modular professional controller",
                    Slug = "thrustmaster-eswap-x-pro",
                    Price = 159.99m,
                    CategoryId = controllersCategory.Id,
                    StockQuantity = 40,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PlayStation Classic Controller",
                    Description = "Retro-style USB controller",
                    Slug = "ps-classic-controller",
                    Price = 19.99m,
                    CategoryId = controllersCategory.Id,
                    StockQuantity = 120,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "God of War Ragnarök Collector's T-Shirt",
                    Description = "Limited edition graphic t-shirt",
                    Slug = "gow-ragnarok-shirt",
                    Price = 29.99m,
                    CategoryId = tshirtsCategory.Id,
                    StockQuantity = 150,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "The Legend of Zelda: TOTK T-Shirt",
                    Description = "Official game artwork t-shirt",
                    Slug = "zelda-totk-shirt",
                    Price = 24.99m,
                    CategoryId = tshirtsCategory.Id,
                    StockQuantity = 200,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PlayStation Icons T-Shirt",
                    Description = "Classic PlayStation symbols design",
                    Slug = "playstation-icons-shirt",
                    Price = 19.99m,
                    CategoryId = tshirtsCategory.Id,
                    StockQuantity = 180,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Xbox 20th Anniversary T-Shirt",
                    Description = "Commemorative Xbox anniversary design",
                    Slug = "xbox-20th-anniversary-shirt",
                    Price = 24.99m,
                    CategoryId = tshirtsCategory.Id,
                    StockQuantity = 160,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Final Fantasy XVI Collector's T-Shirt",
                    Description = "Premium gaming t-shirt with art print",
                    Slug = "ff16-collector-shirt",
                    Price = 34.99m,
                    CategoryId = tshirtsCategory.Id,
                    StockQuantity = 100,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Elden Ring Map Poster",
                    Description = "Detailed map of the Lands Between",
                    Slug = "elden-ring-map-poster",
                    Price = 29.99m,
                    CategoryId = postersCategory.Id,
                    StockQuantity = 120,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Mass Effect Trilogy Art Print",
                    Description = "High-quality art print set",
                    Slug = "mass-effect-trilogy-print",
                    Price = 49.99m,
                    CategoryId = postersCategory.Id,
                    StockQuantity = 80,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Horizon Forbidden West Landscape Poster",
                    Description = "Premium landscape art poster",
                    Slug = "horizon-fw-landscape-poster",
                    Price = 24.99m,
                    CategoryId = postersCategory.Id,
                    StockQuantity = 90,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Cyberpunk 2077 Night City Map",
                    Description = "Detailed city map poster",
                    Slug = "cyberpunk-night-city-map",
                    Price = 19.99m,
                    CategoryId = postersCategory.Id,
                    StockQuantity = 150,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Spider-Man 2 Action Scene Poster",
                    Description = "Dynamic artwork poster",
                    Slug = "spiderman-2-action-poster",
                    Price = 19.99m,
                    CategoryId = postersCategory.Id,
                    StockQuantity = 130,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Kratos Premium Statue",
                    Description = "High-detail collector's statue",
                    Slug = "kratos-premium-statue",
                    Price = 199.99m,
                    CategoryId = figurinesCategory.Id,
                    StockQuantity = 30,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Link TOTK Collector's Figure",
                    Description = "Limited edition figurine",
                    Slug = "link-totk-figure",
                    Price = 149.99m,
                    CategoryId = figurinesCategory.Id,
                    StockQuantity = 40,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "2B NieR: Automata Figure",
                    Description = "Detailed PVC statue",
                    Slug = "2b-nier-figure",
                    Price = 179.99m,
                    CategoryId = figurinesCategory.Id,
                    StockQuantity = 25,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Cloud Strife Remake Figure",
                    Description = "Premium Final Fantasy VII figure",
                    Slug = "cloud-strife-remake-figure",
                    Price = 189.99m,
                    CategoryId = figurinesCategory.Id,
                    StockQuantity = 35,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Aloy Forbidden West Figure",
                    Description = "Collector's edition statue",
                    Slug = "aloy-forbidden-west-figure",
                    Price = 169.99m,
                    CategoryId = figurinesCategory.Id,
                    StockQuantity = 45,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PlayStation 5 Spider-Man 2 Bundle",
                    Description = "PS5 console with Spider-Man 2 game",
                    Slug = "ps5-spiderman-2-bundle",
                    Price = 559.99m,
                    CategoryId = playstationCategory.Id,
                    StockQuantity = 50,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PlayStation 5 Slim Digital Edition",
                    Description = "Compact PS5 console without disc drive",
                    Slug = "ps5-slim-digital",
                    Price = 449.99m,
                    CategoryId = playstationCategory.Id,
                    StockQuantity = 75,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PlayStation 5 FFXVI Bundle",
                    Description = "PS5 with Final Fantasy XVI game",
                    Slug = "ps5-ff16-bundle",
                    Price = 559.99m,
                    CategoryId = playstationCategory.Id,
                    StockQuantity = 40,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Xbox Series X Forza Edition",
                    Description = "Limited edition Forza themed console",
                    Slug = "xbox-series-x-forza",
                    Price = 549.99m,
                    CategoryId = xboxCategory.Id,
                    StockQuantity = 35,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Nintendo Switch OLED Pokémon Edition",
                    Description = "Special edition OLED Switch with Pokémon design",
                    Slug = "switch-oled-pokemon",
                    Price = 359.99m,
                    CategoryId = nintendoCategory.Id,
                    StockQuantity = 45,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Nintendo Switch Lite Animal Crossing Edition",
                    Description = "Themed Switch Lite console",
                    Slug = "switch-lite-animal-crossing",
                    Price = 199.99m,
                    CategoryId = nintendoCategory.Id,
                    StockQuantity = 65,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PlayStation VR2 Horizon Bundle",
                    Description = "VR headset with Horizon Call of the Mountain",
                    Slug = "psvr2-horizon-bundle",
                    Price = 599.99m,
                    CategoryId = playstationCategory.Id,
                    StockQuantity = 30,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PlayStation 4 Pro 1TB Refurbished",
                    Description = "Certified refurbished PS4 Pro console",
                    Slug = "ps4-pro-refurbished",
                    Price = 299.99m,
                    CategoryId = playstationCategory.Id,
                    StockQuantity = 25,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Xbox One X Refurbished",
                    Description = "Certified refurbished Xbox One X console",
                    Slug = "xbox-one-x-refurbished",
                    Price = 279.99m,
                    CategoryId = xboxCategory.Id,
                    StockQuantity = 20,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Nintendo Switch Dock Set",
                    Description = "Additional dock for Nintendo Switch",
                    Slug = "switch-dock-set",
                    Price = 89.99m,
                    CategoryId = nintendoCategory.Id,
                    StockQuantity = 100,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PS5 DualSense Charging Station",
                    Description = "Official charging dock for two controllers",
                    Slug = "dualsense-charging-station",
                    Price = 29.99m,
                    CategoryId = accessoriesCategory.Id,
                    StockQuantity = 150,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Xbox Series X Vertical Stand",
                    Description = "Official vertical stand with cooling",
                    Slug = "xbox-series-x-stand",
                    Price = 19.99m,
                    CategoryId = accessoriesCategory.Id,
                    StockQuantity = 120,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PS5 HD Camera",
                    Description = "1080p camera for streaming and PS VR2",
                    Slug = "ps5-hd-camera",
                    Price = 59.99m,
                    CategoryId = accessoriesCategory.Id,
                    StockQuantity = 80,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Xbox Series X Seagate Expansion Card 1TB",
                    Description = "Official storage expansion card",
                    Slug = "xbox-expansion-card-1tb",
                    Price = 219.99m,
                    CategoryId = accessoriesCategory.Id,
                    StockQuantity = 45,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PS5 Media Remote",
                    Description = "Media control remote for PlayStation 5",
                    Slug = "ps5-media-remote",
                    Price = 29.99m,
                    CategoryId = accessoriesCategory.Id,
                    StockQuantity = 110,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Nintendo Switch Carrying Case",
                    Description = "Premium protective case with game storage",
                    Slug = "switch-carrying-case",
                    Price = 19.99m,
                    CategoryId = accessoriesCategory.Id,
                    StockQuantity = 200,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "PS5 Console Cover - Cosmic Red",
                    Description = "Official colored console cover",
                    Slug = "ps5-cover-cosmic-red",
                    Price = 54.99m,
                    CategoryId = accessoriesCategory.Id,
                    StockQuantity = 90,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                },
                new()
                {
                    Name = "Xbox Quick Charging Stand",
                    Description = "Fast charging stand for Xbox controllers",
                    Slug = "xbox-charging-stand",
                    Price = 39.99m,
                    CategoryId = accessoriesCategory.Id,
                    StockQuantity = 130,
                    Status = EntityStatus.Active,
                    BusinessStatus = ProductStatus.Available
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        return Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }
}