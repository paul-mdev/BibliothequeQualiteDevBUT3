using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;
using BibliothequeQualiteDev.Server.Controllers;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Session;


namespace BibliothequeQualiteDev.Tests.Controllers
{
    public class AuthControllerTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private void SetupSession(AuthController controller, int? userId = null)
        {
            var httpContext = new DefaultHttpContext();
            var sessionItems = new Dictionary<string, byte[]>();

            var session = new TestSession(sessionItems);
            httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });

            // Utiliser SetInt32 APRÈS avoir configuré le HttpContext
            if (userId.HasValue)
            {
                httpContext.Session.SetInt32("user_id", userId.Value);
                httpContext.Session.SetString("user_mail", "user@example.com");
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsOk()
        {
            var context = CreateInMemoryContext();
            var controller = new AuthController(context);
            SetupSession(controller);
            var dto = new AuthController.RegisterDTO
            {
                user_name = "Test User",
                user_mail = "test@example.com",
                user_pswd = "password123"
            };
            var result = await controller.Register(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext();
            context.USERS.Add(new UsersModel
            {
                user_mail = "existing@example.com",
                user_pswd = "hash",
                user_name = "Existing",
                role_id = 1
            });
            await context.SaveChangesAsync();
            var controller = new AuthController(context);
            SetupSession(controller);
            var dto = new AuthController.RegisterDTO
            {
                user_name = "New User",
                user_mail = "existing@example.com",
                user_pswd = "password123"
            };
            var result = await controller.Register(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            var context = CreateInMemoryContext();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
            context.ROLES.Add(new RolesModel { role_id = 1, role_name = "Admin" });
            context.USERS.Add(new UsersModel
            {
                user_id = 1,
                user_mail = "user@example.com",
                user_pswd = hashedPassword,
                user_name = "Test User",
                role_id = 1
            });
            await context.SaveChangesAsync();
            var controller = new AuthController(context);
            SetupSession(controller);
            var dto = new AuthController.LoginDTO
            {
                user_mail = "user@example.com",
                user_pswd = "password123"
            };
            var result = await controller.Login(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var context = CreateInMemoryContext();
            var controller = new AuthController(context);
            SetupSession(controller);
            var dto = new AuthController.LoginDTO
            {
                user_mail = "nonexistent@example.com",
                user_pswd = "wrongpassword"
            };
            var result = await controller.Login(dto);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Me_WithValidSession_ReturnsUserInfo()
        {
            var context = CreateInMemoryContext();
            context.ROLES.Add(new RolesModel { role_id = 1, role_name = "Admin" });
            context.USERS.Add(new UsersModel
            {
                user_id = 1,
                user_mail = "user@example.com",
                user_pswd = "hash",
                user_name = "Test",
                role_id = 1
            });
            await context.SaveChangesAsync();
            var controller = new AuthController(context);
            SetupSession(controller, 1);
            var result = await controller.Me();
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Me_WithoutSession_ReturnsUnauthorized()
        {
            var context = CreateInMemoryContext();
            var controller = new AuthController(context);
            SetupSession(controller);
            var result = await controller.Me();
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Logout_ClearsSession_ReturnsOk()
        {
            var context = CreateInMemoryContext();
            var controller = new AuthController(context);
            SetupSession(controller, 1);
            var result = controller.Logout();
            Assert.IsType<OkResult>(result);
        }

        // Classes helper pour la session
        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store;

            public TestSession(Dictionary<string, byte[]> store)
            {
                _store = store;
            }

            public bool IsAvailable => true;
            public string Id => Guid.NewGuid().ToString();
            public IEnumerable<string> Keys => _store.Keys;

            public void Clear() => _store.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public void Remove(string key) => _store.Remove(key);

            public void Set(string key, byte[] value) => _store[key] = value;

            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
        }
        private class SessionFeature : ISessionFeature
        {
            public ISession Session { get; set; }
        }
    }

    public class UsersControllerTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            var context = CreateInMemoryContext();
            context.ROLES.Add(new RolesModel { role_id = 1, role_name = "Admin" });
            context.USERS.Add(new UsersModel
            {
                user_id = 1,
                user_name = "User1",
                user_mail = "user1@test.com",
                user_pswd = "hash",
                role_id = 1
            });
            await context.SaveChangesAsync();

            var controller = new UsersController(context);

            var result = await controller.GetUsers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetUser_WithValidId_ReturnsUser()
        {
            var context = CreateInMemoryContext();
            context.ROLES.Add(new RolesModel { role_id = 1, role_name = "Admin" });
            context.USERS.Add(new UsersModel
            {
                user_id = 1,
                user_name = "User1",
                user_mail = "user1@test.com",
                user_pswd = "hash",
                role_id = 1
            });
            await context.SaveChangesAsync();

            var controller = new UsersController(context);

            var result = await controller.GetUser(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ReturnsNotFound()
        {
            var context = CreateInMemoryContext();
            var controller = new UsersController(context);

            var result = await controller.GetUser(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateUser_WithValidData_ReturnsOk()
        {
            var context = CreateInMemoryContext();
            var controller = new UsersController(context);

            var dto = new UsersController.UsersCreateDTO
            {
                user_name = "New User",
                user_mail = "new@test.com",
                user_pswd = "password123",
                role_id = 1
            };

            var result = await controller.CreateUser(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CreateUser_WithExistingEmail_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext();
            context.USERS.Add(new UsersModel
            {
                user_mail = "existing@test.com",
                user_pswd = "hash",
                user_name = "Existing",
                role_id = 1
            });
            await context.SaveChangesAsync();

            var controller = new UsersController(context);

            var dto = new UsersController.UsersCreateDTO
            {
                user_name = "New",
                user_mail = "existing@test.com",
                user_pswd = "password",
                role_id = 1
            };

            var result = await controller.CreateUser(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUser_WithValidData_ReturnsOk()
        {
            var context = CreateInMemoryContext();
            context.USERS.Add(new UsersModel
            {
                user_id = 1,
                user_mail = "user@test.com",
                user_pswd = "hash",
                user_name = "Original",
                role_id = 1
            });
            await context.SaveChangesAsync();

            var controller = new UsersController(context);

            var dto = new UsersController.UsersCreateDTO
            {
                user_name = "Updated",
                user_mail = "user@test.com",
                user_pswd = "",
                role_id = 1
            };

            var result = await controller.UpdateUser(1, dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUser_WithInvalidId_ReturnsNotFound()
        {
            var context = CreateInMemoryContext();
            var controller = new UsersController(context);

            var dto = new UsersController.UsersCreateDTO
            {
                user_name = "Updated",
                user_mail = "user@test.com",
                user_pswd = "",
                role_id = 1
            };

            var result = await controller.UpdateUser(999, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ReturnsNoContent()
        {
            var context = CreateInMemoryContext();
            context.USERS.Add(new UsersModel
            {
                user_id = 1,
                user_mail = "user@test.com",
                user_pswd = "hash",
                user_name = "ToDelete",
                role_id = 1
            });
            await context.SaveChangesAsync();

            var controller = new UsersController(context);

            var result = await controller.DeleteUser(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_WithInvalidId_ReturnsNotFound()
        {
            var context = CreateInMemoryContext();
            var controller = new UsersController(context);

            var result = await controller.DeleteUser(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }

    public class RolesControllerTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetRoles_ReturnsAllRoles()
        {
            var context = CreateInMemoryContext();
            context.ROLES.AddRange(
                new RolesModel { role_id = 1, role_name = "Admin" },
                new RolesModel { role_id = 2, role_name = "User" }
            );
            await context.SaveChangesAsync();

            var controller = new RolesController(context);

            var result = await controller.GetRoles();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetRole_WithValidId_ReturnsRole()
        {
            var context = CreateInMemoryContext();
            context.ROLES.Add(new RolesModel { role_id = 1, role_name = "Admin" });
            await context.SaveChangesAsync();

            var controller = new RolesController(context);

            var result = await controller.GetRole(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRole_WithInvalidId_ReturnsNotFound()
        {
            var context = CreateInMemoryContext();
            var controller = new RolesController(context);

            var result = await controller.GetRole(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }

    public class BookControllerTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task Get_ReturnsAllBooks()
        {
            var context = CreateInMemoryContext();
            context.BOOK.AddRange(
                new BookModel { book_id = 1, book_name = "Book1", book_author = "Author1", book_editor = "Editor1", book_date = System.DateTime.Now },
                new BookModel { book_id = 2, book_name = "Book2", book_author = "Author2", book_editor = "Editor2", book_date = System.DateTime.Now }
            );
            await context.SaveChangesAsync();

            var controller = new BookController(context);

            var result = await controller.Get();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetBook_WithValidId_ReturnsBook()
        {
            var context = CreateInMemoryContext();
            context.BOOK.Add(new BookModel
            {
                book_id = 1,
                book_name = "Test Book",
                book_author = "Author",
                book_editor = "Editor",
                book_date = System.DateTime.Now
            });
            context.SaveChanges();

            var controller = new BookController(context);

            var result = controller.GetBook(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetBook_WithInvalidId_ReturnsNotFound()
        {
            var context = CreateInMemoryContext();
            var controller = new BookController(context);

            var result = controller.GetBook(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAvailableCount_WithExistingStock_ReturnsCount()
        {
            var context = CreateInMemoryContext();
            context.LIBRARY_STOCK.Add(new LibraryStockModel
            {
                book_id = 1,
                total_stock = 10,
                borrowed_count = 3
            });
            await context.SaveChangesAsync();

            var controller = new BookController(context);

            var result = await controller.GetAvailableCount(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(7, okResult.Value);
        }

        [Fact]
        public async Task GetAvailableCount_WithNoStock_ReturnsZero()
        {
            var context = CreateInMemoryContext();
            var controller = new BookController(context);

            var result = await controller.GetAvailableCount(999);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0, okResult.Value);
        }

        [Fact]
        public void DeleteBook_WithValidId_ReturnsNoContent()
        {
            var context = CreateInMemoryContext();
            context.BOOK.Add(new BookModel
            {
                book_id = 1,
                book_name = "ToDelete",
                book_author = "Author",
                book_editor = "Editor",
                book_date = System.DateTime.Now
            });
            context.SaveChanges();

            var controller = new BookController(context);

            var result = controller.DeleteBook(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void DeleteBook_WithInvalidId_ReturnsNotFound()
        {
            var context = CreateInMemoryContext();
            var controller = new BookController(context);

            var result = controller.DeleteBook(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }

    public class StatisticsControllerTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetStatistics_ReturnsValidData()
        {
            var context = CreateInMemoryContext();

            context.BOOK.Add(new BookModel
            {
                book_id = 1,
                book_name = "Book",
                book_author = "Author",
                book_editor = "Editor",
                book_date = System.DateTime.Now
            });

            context.USERS.Add(new UsersModel
            {
                user_id = 1,
                user_name = "User",
                user_mail = "user@test.com",
                user_pswd = "hash",
                role_id = 1
            });

            await context.SaveChangesAsync();

            var controller = new StatisticsController(context);

            var result = await controller.GetStatistics();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var stats = Assert.IsType<StatisticsDto>(okResult.Value);
            Assert.Equal(1, stats.TotalBooks);
            Assert.Equal(1, stats.TotalUsers);
        }
    }

    public class UsersBorrowedControllerTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private void SetupSession(UsersBorrowedController controller, int? userId = null)
        {
            var httpContext = new DefaultHttpContext();
            var sessionItems = new Dictionary<string, byte[]>();

            var session = new TestSession(sessionItems);
            httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });

            if (userId.HasValue)
            {
                httpContext.Session.SetInt32("user_id", userId.Value);
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task GetMyBorrowed_WithoutSession_ReturnsUnauthorized()
        {
            var context = CreateInMemoryContext();
            var controller = new UsersBorrowedController(context);
            SetupSession(controller);

            var result = await controller.GetMyBorrowed();

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetAllBorrowed_WithoutSession_ReturnsUnauthorized()
        {
            var context = CreateInMemoryContext();
            var controller = new UsersBorrowedController(context);
            SetupSession(controller);

            var result = await controller.GetAllBorrowed();

            Assert.IsType<UnauthorizedResult>(result);
        }

        // Ajoutez ces classes helper
        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store;

            public TestSession(Dictionary<string, byte[]> store)
            {
                _store = store;
            }

            public bool IsAvailable => true;
            public string Id => Guid.NewGuid().ToString();
            public IEnumerable<string> Keys => _store.Keys;

            public void Clear() => _store.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public void Remove(string key) => _store.Remove(key);

            public void Set(string key, byte[] value) => _store[key] = value;

            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
        }

        private class SessionFeature : ISessionFeature
        {
            public ISession Session { get; set; }
        }
    }
}