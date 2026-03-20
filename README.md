# 🍾 SpiritShop — ASP.NET Core 8 Web API

Повноцінний навчальний проект магазину алкоголю, що охоплює всі ключові теми ASP.NET Core Web API.

---

## 🚀 Швидкий старт

```bash
# 1. Клонувати / відкрити у Visual Studio
# 2. Налаштувати рядок підключення в appsettings.json
# 3. Виконати міграції:
cd SpiritShop.API
dotnet ef migrations add InitialCreate --project ../SpiritShop.Infrastructure
dotnet ef database update

# 4. Запустити
dotnet run --project SpiritShop.API

# 5. Swagger UI: https://localhost:5001
```

**Тестовий адмін:** `admin@spiritshop.com` / `Admin123!`

---

## 📁 Структура проекту

```
SpiritShop/
├── SpiritShop.API/                 ← Точка входу, контролери, middleware
│   ├── Controllers/
│   │   ├── ProductsController.cs  ← REST API (GET/POST/PUT/DELETE)
│   │   └── AuthController.cs     ← JWT аутентифікація
│   ├── Middleware/
│   │   ├── RequestLoggingMiddleware.cs   ← Кастомний middleware
│   │   └── GlobalExceptionMiddleware.cs ← Глобальна обробка помилок
│   ├── Program.cs                 ← Конфігурація DI та middleware pipeline
│   └── appsettings.json
│
├── SpiritShop.Application/         ← Бізнес-логіка (CQRS, MediatR, DTO)
│   ├── Commands/                  ← Команди (Create, Update, Delete, Upload)
│   ├── Queries/                   ← Запити (GetAll, GetById)
│   ├── DTOs/                      ← Data Transfer Objects
│   ├── Behaviors/                 ← MediatR Pipeline (валідація)
│   └── Interfaces/                ← Абстракції (IAppDbContext, IJwtTokenService)
│
├── SpiritShop.Domain/              ← Сутності, бізнес-правила
│   └── Entities/                  ← Product, Category, Order, DomainException
│
├── SpiritShop.Infrastructure/      ← EF Core, JWT, File Storage
│   ├── Data/AppDbContext.cs       ← DbContext (Code First + Seed)
│   └── Services/                  ← JwtTokenService, LocalFileStorageService
│
└── SpiritShop.Tests/               ← Unit та Integration тести
    ├── Unit/                      ← Тести з Moq та InMemory DB
    └── Integration/               ← WebApplicationFactory тести
```

---

## 📚 Покрита теорія

### ASP.NET Core основи
| Тема | Файл |
|------|------|
| ASP.NET Core vs класичний ASP.NET | `Program.cs` (коментарі) |
| Переваги ASP.NET Core | `Program.cs` (коментарі) |
| Що таке Middleware | `Middleware/RequestLoggingMiddleware.cs` |
| Структура проекту | Вся папка `SpiritShop/` |
| Program.cs і його роль | `SpiritShop.API/Program.cs` |

### REST API та HTTP
| Тема | Файл |
|------|------|
| Що таке REST API | `Controllers/ProductsController.cs` |
| GET, POST, PUT, DELETE | `Controllers/ProductsController.cs` |
| Контролер | `Controllers/ProductsController.cs`, `AuthController.cs` |
| `[Route]`, `[HttpGet]`, `[HttpPost]` | `Controllers/ProductsController.cs` |

### DTO та валідація
| Тема | Файл |
|------|------|
| DTO та навіщо | `Application/DTOs/Dtos.cs` |
| Валідація ModelState | `DTOs/Dtos.cs` + `Controllers/ProductsController.cs` |

### Swagger
| Тема | Файл |
|------|------|
| Swagger/OpenAPI | `Program.cs` (секція AddSwaggerGen) |

### EF Core
| Тема | Файл |
|------|------|
| DbContext | `Infrastructure/Data/AppDbContext.cs` |
| Code First vs Database First | `AppDbContext.cs` (коментарі) |
| Міграції | `AppDbContext.cs` + команди нижче |
| Add-Migration / Update-Database | Секція "Міграції" нижче |

### CQRS та MediatR
| Тема | Файл |
|------|------|
| CQRS патерн | `Application/Queries/ProductQueries.cs` |
| MediatR | `Commands/`, `Queries/`, `Program.cs` |

### Аутентифікація та авторизація
| Тема | Файл |
|------|------|
| Аутентифікація | `Controllers/AuthController.cs` |
| Авторизація | `Controllers/ProductsController.cs` (`[Authorize(Roles)]`) |
| JWT | `Infrastructure/Services/JwtTokenService.cs` |
| Claims | `JwtTokenService.cs` + `AuthController.cs` (GetCurrentUser) |

### Middleware
| Тема | Файл |
|------|------|
| Кастомний middleware | `Middleware/RequestLoggingMiddleware.cs` |
| Порядок middleware | `Program.cs` (секція ③—⑩) |

### Файли
| Тема | Файл |
|------|------|
| IFormFile | `Commands/ProductCommands.cs` + `ProductCommandHandlers.cs` |
| Збереження файлів | `Infrastructure/Services/LocalFileStorageService.cs` |
| Валідація файлів | `Commands/ProductCommandHandlers.cs` |
| Обмеження розміру | `Commands/ProductCommandHandlers.cs` + `Program.cs` |

### JSON
| Тема | Файл |
|------|------|
| Робота з JSON | `Middleware/GlobalExceptionMiddleware.cs` (JsonSerializer) |

### Тестування
| Тема | Файл |
|------|------|
| Unit тестування | `Tests/Unit/ProductHandlerTests.cs` |
| Unit vs Integration | `Tests/Unit/` vs `Tests/Integration/` |
| Moq | `Tests/Unit/ProductHandlerTests.cs` (DeleteProductCommandHandlerTests) |

---

## 🔧 Міграції EF Core

```bash
# Додати нову міграцію (Code First)
dotnet ef migrations add <НазваМіграції> \
  --project SpiritShop.Infrastructure \
  --startup-project SpiritShop.API

# Застосувати міграцію до БД
dotnet ef database update \
  --project SpiritShop.Infrastructure \
  --startup-project SpiritShop.API

# Переглянути список міграцій
dotnet ef migrations list \
  --project SpiritShop.Infrastructure \
  --startup-project SpiritShop.API

# Відкотити останню міграцію
dotnet ef migrations remove \
  --project SpiritShop.Infrastructure \
  --startup-project SpiritShop.API
```

---

## 🌐 API Endpoints

### Products (публічні GET, Admin для решти)
```
GET    /api/products?page=1&pageSize=10&category=Whiskey&maxPrice=1000
GET    /api/products/{id}
POST   /api/products                    [Admin]
PUT    /api/products/{id}               [Admin]
DELETE /api/products/{id}               [Admin]
POST   /api/products/{id}/image         [Admin, multipart/form-data]
```

### Auth
```
POST   /api/auth/register
POST   /api/auth/login
GET    /api/auth/me                     [Authorize]
```

---

## 🧪 Запуск тестів

```bash
# Всі тести
dotnet test

# Тільки unit тести
dotnet test --filter "FullyQualifiedName~Unit"

# Тільки integration тести
dotnet test --filter "FullyQualifiedName~Integration"
```

---

## 💡 Ключові концепції

### CQRS Flow
```
HTTP Request
    ↓
Controller
    ↓ _mediator.Send(query/command)
MediatR Pipeline
    ↓ ValidationBehavior (FluentValidation)
    ↓ Handler
    ↓ DbContext / Service
    ↑ Result (DTO)
Controller
    ↓
HTTP Response
```

### JWT Flow
```
POST /api/auth/login  { email, password }
    ↓ Перевірка пароля (UserManager)
    ↓ Генерація JWT (Claims: userId, email, roles)
    ← { token: "eyJ..." }

GET /api/products  Authorization: Bearer eyJ...
    ↓ JwtBearer Middleware валідує підпис
    ↓ Встановлює HttpContext.User
    ↓ [Authorize] перевіряє IsAuthenticated
    ← 200 OK або 401
```

### Порядок Middleware
```
① GlobalExceptionMiddleware   ← найзовніший (ловить всі помилки)
② RequestLoggingMiddleware
③ Swagger (Development)
④ HttpsRedirection
⑤ StaticFiles
⑥ CORS
⑦ Routing
⑧ Authentication            ← хто ти? (JWT)
⑨ Authorization             ← що можеш? (ролі)
⑩ MapControllers
```
