# Shop API

API для интернет-магазина медицинских товаров с поддержкой импорта данных из XML файлов.

## Развертывание на Railway

### 1. Подготовка

1. Зарегистрируйтесь на [Railway](https://railway.app/)
2. Установите Railway CLI: `npm install -g @railway/cli`
3. Войдите в аккаунт: `railway login`

### 2. Создание проекта на Railway

1. Перейдите в [Railway Dashboard](https://railway.app/dashboard)
2. Нажмите "New Project"
3. Выберите "Deploy from GitHub repo"
4. Подключите ваш GitHub репозиторий

### 3. Настройка базы данных

1. В проекте Railway нажмите "New Service"
2. Выберите "Database" → "PostgreSQL"
3. Railway автоматически создаст базу данных

### 4. Настройка переменных окружения

В настройках вашего API сервиса добавьте следующие переменные:

```
DB_HOST=your-postgres-host
DB_NAME=your-database-name
DB_USER=your-username
DB_PASSWORD=your-password
DB_PORT=5432
JWT_SECRET_KEY=your-super-secret-jwt-key-here
```

### 5. Развертывание

1. Railway автоматически обнаружит Dockerfile и развернет приложение
2. Дождитесь завершения сборки и деплоя
3. Получите URL вашего API из настроек сервиса

### 6. Проверка работы

- Health check: `https://your-app.railway.app/health`
- Swagger UI: `https://your-app.railway.app/swagger` (в development)

## Локальная разработка

```bash
# Восстановление зависимостей
dotnet restore

# Запуск приложения
cd Shop
dotnet run
```

## Структура проекта

- `Shop/` - Основной API проект
- `Shop.Core/` - Модели и интерфейсы
- `Shop.Application/` - Бизнес-логика и сервисы
- `Shop.DataAccess/` - Доступ к данным и Entity Framework
- `Shop.Infrastructure/` - JWT и хеширование паролей 