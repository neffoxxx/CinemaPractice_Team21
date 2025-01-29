-- Спочатку видаляємо всі зовнішні ключі
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'
ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id))
    + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
    ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
FROM sys.foreign_keys;
EXEC sp_executesql @sql;

-- Тепер видаляємо всі таблиці
SET @sql = N'';
SELECT @sql += N'DROP TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(object_id))
    + '.' + QUOTENAME(name) + ';'
FROM sys.tables
WHERE type = 'U' -- User-defined tables only
  AND name != '__EFMigrationsHistory'; -- Зберігаємо таблицю міграцій

EXEC sp_executesql @sql;

-- Очищаємо таблицю міграцій
DELETE FROM [__EFMigrationsHistory];