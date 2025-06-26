# Настройка Railway для ShopAPI

## Проблема
Ошибка аутентификации PostgreSQL: `password authentication failed for user "vlad"`

## Решение

### 1. Добавить PostgreSQL в Railway
1. В Railway Dashboard создайте новый сервис PostgreSQL
2. Запишите данные подключения

### 2. Настроить переменные окружения
В настройках вашего приложения в Railway добавьте следующие переменные:

```
DB_HOST=your_postgres_host.railway.app
DB_PORT=5432
DB_NAME=railway
DB_USER=postgres
DB_PASSWORD=your_postgres_password
```

### 3. Где найти данные подключения
1. Откройте ваш PostgreSQL сервис в Railway
2. Перейдите в раздел "Connect"
3. Скопируйте данные из "Postgres Connection URL" или отдельных полей

### 4. Пример переменных окружения
```
DB_HOST=containers-us-west-123.railway.app
DB_PORT=5432
DB_NAME=railway
DB_USER=postgres
DB_PASSWORD=abc123def456
```

### 5. Проверка
После настройки переменных:
1. Перезапустите деплой
2. Проверьте логи - должно появиться сообщение "Используем переменные окружения для подключения к БД"
3. Приложение должно успешно подключиться к базе данных

### 6. Альтернативное решение
Если у вас уже есть PostgreSQL сервис, убедитесь что:
- Переменные окружения настроены правильно
- Пароль скопирован без лишних символов
- Хост и порт соответствуют вашему сервису 