# Zetflix

Онлайн-кинотеатр с каталогом фильмов и сериалов, личным кабинетом, отзывами и отдельной административной панелью. Публичный сайт работает на ASP.NET Core MVC, а управление контентом — через React-приложение и API.

## Возможности

- **Каталог:** фильмы, сериалы, сезоны, эпизоды и жанры; поиск, фильтры и пагинация.
- **Просмотр:** страницы контента, видеоплеер, описания и постеры.
- **Аккаунт:** регистрация, вход, профиль, роли `User`, `Moderator`, `Admin`.
- **Отзывы:** публикация и модерация, события активности через SignalR.
- **Админка:** сводка и аналитика, пользователи, фильмы, сериалы, эпизоды, жанры, отзывы, подписки, медиафайлы и настройки сайта.
- **Сервисы:** форма контактов с SMTP, аудит действий в файле и опционально в MongoDB.
- **Интерфейс:** русская и английская локализация, адаптивные страницы, обработка ошибок 403/404/500.

## Стек

| Часть | Технологии |
| --- | --- |
| Backend и публичный сайт | C#, .NET 10, ASP.NET Core MVC, Razor |
| Данные | PostgreSQL, Entity Framework Core 10, Npgsql |
| Административная панель | React 18, Vite 4, React Router, Bootstrap |
| Обновления в реальном времени | ASP.NET Core SignalR |
| Дополнительные интеграции | MongoDB для аудита, SMTP для почты |

## Структура проекта

```text
Zetflix/
├── Controllers/       # MVC-страницы и административное API
├── Data/              # DbContext, фабрика миграций и наполнение каталога
├── Migrations/        # Миграции Entity Framework Core
├── Models/            # Модели данных и настроек
├── ViewModels/        # Данные для страниц и форм
├── Views/             # Публичный сайт на Razor
├── Services/          # Авторизация админки, почта, аудит, локализация
├── Middleware/        # Проверка доступа, роли и HTTP-заголовки
├── Hubs/              # SignalR
└── wwwroot/           # Стили, скрипты, изображения и медиа

dashboard/            # React-админка, подключённая к backend API
frontend/             # Исходники React-шаблона Streamit
zetflix.sln           # Решение .NET
```

Основной публичный сайт находится в `Zetflix/Views`, его отдаёт backend. Для обычного запуска проекта папку `frontend` собирать не требуется.

## Быстрый старт

### 1. Требования

- .NET SDK **10**.
- Node.js и npm; зависимости интерфейсов зафиксированы в `package-lock.json`.
- Запущенный PostgreSQL. В примере ниже используется PostgreSQL 16 в Docker.
- Инструмент `dotnet-ef` версии **10.0.7** для применения миграций.

```bash
git clone git@github.com:GusmanovIlya/Zetflix.git
cd Zetflix

dotnet tool install --global dotnet-ef --version 10.0.7
```

Если `dotnet-ef` уже установлен, проверьте совместимость его основной версии с EF Core 10.

### 2. Подготовка PostgreSQL

Проект по умолчанию использует локальную базу `zetflix_db` на порту `5432` с пользователем `postgres` и демонстрационным паролем `1212`.

Если локальной базы ещё нет, её можно запустить отдельно:

```bash
docker run --name zetflix-postgres \
  -e POSTGRES_DB=zetflix_db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=1212 \
  -p 127.0.0.1:5432:5432 \
  -v zetflix-postgres-data:/var/lib/postgresql/data \
  -d postgres:16
```

Для повторного запуска существующего контейнера: `docker start zetflix-postgres`.

Если порт `5432` уже занят PostgreSQL, используйте существующий сервер или другой порт. Настройки подключения должны совпадать в двух местах:

- `Zetflix/appsettings.json` — подключение запущенного приложения; его можно переопределить через `ConnectionStrings__DefaultConnection` или .NET user-secrets.
- `Zetflix/Data/ApplicationDbContextFactory.cs` — подключение при выполнении `dotnet ef`. Сейчас фабрика содержит отдельную фиксированную строку, поэтому изменение только user-secrets не меняет подключение миграций.

Демонстрационный пароль предназначен только для локального запуска.

### 3. Миграции и запуск сайта

```bash
dotnet restore Zetflix/Zetflix.csproj
dotnet ef database update --project Zetflix/Zetflix.csproj
dotnet run --project Zetflix/Zetflix.csproj --launch-profile http
```

Миграции создают структуру БД и начальные записи. После запуска `CatalogSeeder` дополняет каталог демонстрационными фильмами, сериалами, эпизодами, жанрами и отзывами. Наполнение запускается в фоне: первые несколько секунд каталог может быть ещё не готов.

Публичный сайт: **http://localhost:5145**.

### 4. Запуск административной панели

В другом терминале, из корня репозитория:

```bash
cd dashboard
npm ci
npm run dev
```

Административная панель: **http://localhost:3000**. По умолчанию она обращается к API на `http://localhost:5145`.

Если backend работает по другому адресу, задайте в локальном `dashboard/.env.development`:

```dotenv
VITE_API_BASE_URL=http://localhost:5145
```

Для стандартного размещения сборки админки в корне сайта задайте в `dashboard/.env.production`:

```dotenv
VITE_URL=/
VITE_API_BASE_URL=http://localhost:5145
```

В `dashboard/env.example` сохранён путь исходного шаблона Streamit; для собственного размещения его нужно заменить. Значения `VITE_*` включаются в клиентскую сборку, поэтому секреты в них не хранят.

Оба Vite-приложения по умолчанию запрашивают порт `3000`. Если нужен также отдельный шаблон из `frontend`, запускайте его на другом порту: `npm run dev -- --port 3002`.

## Локальные адреса

| Компонент | Адрес |
| --- | --- |
| Публичный сайт и backend | http://localhost:5145 |
| React-админка | http://localhost:3000 |
| Проверка ответа backend | http://localhost:5145/api/health |
| SignalR hub | http://localhost:5145/hubs/notifications |

`/api/health` подтверждает ответ приложения; этот endpoint не проверяет подключение к базе данных.

## Демонстрационные аккаунты

На новой базе создаются следующие аккаунты:

| Роль | Email | Пароль |
| --- | --- | --- |
| Admin | `admin@zetflix.com` | `admin123` |
| User | `ilya@zetflix.com` | `user123` |
| Moderator | `moderator@zetflix.com` | `moderator123` |

Это учебные учётные записи. Перед доступом из интернета их нужно заменить. После изменения роли выйдите из аккаунта и войдите заново.

## Почта и аудит

### SMTP

В репозитории отправка почты по умолчанию отключена. Для локальной разработки используйте .NET user-secrets: идентификатор хранилища уже задан в `.csproj`.

```bash
dotnet user-secrets set "Email:Enabled" "true" --project Zetflix/Zetflix.csproj
dotnet user-secrets set "Email:SmtpHost" "smtp.example.com" --project Zetflix/Zetflix.csproj
dotnet user-secrets set "Email:SmtpPort" "587" --project Zetflix/Zetflix.csproj
dotnet user-secrets set "Email:SmtpUser" "your-email@example.com" --project Zetflix/Zetflix.csproj
dotnet user-secrets set "Email:SmtpPassword" "YOUR_SMTP_PASSWORD" --project Zetflix/Zetflix.csproj
dotnet user-secrets set "Email:FromAddress" "your-email@example.com" --project Zetflix/Zetflix.csproj
dotnet user-secrets set "Email:FromName" "Zetflix" --project Zetflix/Zetflix.csproj
```

User-secrets загружаются в окружении `Development`, которое включает профиль `http`. Для серверного окружения используйте переменные окружения вида `Email__SmtpPassword` или хранилище секретов.

### MongoDB

MongoDB не обязательна для базового запуска. По умолчанию интеграция отключена; файловый аудит записывается в `Zetflix/App_Data/audit.log`.

Для дополнительного сохранения событий в локальную MongoDB:

```bash
dotnet user-secrets set "MongoAudit:Enabled" "true" --project Zetflix/Zetflix.csproj
dotnet user-secrets set "MongoAudit:ConnectionString" "mongodb://localhost:27017" --project Zetflix/Zetflix.csproj
```

## Проверка сборки

Из корня репозитория:

```bash
dotnet build Zetflix/Zetflix.csproj
npm --prefix dashboard ci
npm --prefix dashboard run build
```

Отдельная сборка исходного React-шаблона, если она нужна:

```bash
npm --prefix frontend ci
npm --prefix frontend run build
```

Для ручной проверки откройте каталог, выполните поиск, войдите тестовым пользователем и проверьте профиль и отзывы. Затем войдите в админку и проверьте загрузку списка фильмов и пользователей. Эти действия проверяют работу с БД, в отличие от одного запроса `/api/health`.

## Текущий статус и оформление

Проект рассчитан на локальную разработку и демонстрацию. Настройки HTTPS, производственные секреты, учётные записи и инфраструктуру развёртывания нужно подготовить отдельно. Готового Docker Compose для всего приложения в репозитории нет; Docker-команда выше запускает только PostgreSQL.

В интерфейсах используются материалы и структура шаблона **Streamit / IQONIC Design**. Условия использования сторонних шаблонов и медиа определяются их правообладателями; собственная лицензия всему проекту в этом репозитории не назначена.
