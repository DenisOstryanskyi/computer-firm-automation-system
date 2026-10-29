/* ============================================================
   CFAS — створення бази даних ComputerFirm (SQL Server LocalDB)
   Скрипт ВИКОНУЄТЬСЯ ПРОГРАМОЮ при першому запуску, якщо бази
   ще немає (клас DbInitializer). Існуючу базу НЕ чіпає.
   Розділювач пакетів — рядок GO (обробляється програмою).
   ============================================================ */

CREATE DATABASE ComputerFirm COLLATE Ukrainian_CI_AS;
GO
USE ComputerFirm;
GO

/* ==================== 1. ТАБЛИЦІ ==================== */

CREATE TABLE roles (
    role_id   INT IDENTITY(1,1) PRIMARY KEY,
    role_name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE users (
    user_id       INT IDENTITY(1,1) PRIMARY KEY,
    login         NVARCHAR(50)  NOT NULL UNIQUE,
    password_hash CHAR(64)      NOT NULL,   -- SHA-256(пароль), HEX верхнім регістром
    full_name     NVARCHAR(100) NOT NULL,
    role_id       INT NOT NULL REFERENCES roles(role_id),
    is_active     BIT NOT NULL DEFAULT 1,
    created_at    DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE clients (
    client_id  INT IDENTITY(1,1) PRIMARY KEY,
    full_name  NVARCHAR(100) NOT NULL,
    phone      NVARCHAR(20)  NULL,
    email      NVARCHAR(100) NULL,
    address    NVARCHAR(200) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE categories (
    category_id   INT IDENTITY(1,1) PRIMARY KEY,
    category_name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE products (
    product_id    INT IDENTITY(1,1) PRIMARY KEY,
    product_name  NVARCHAR(100) NOT NULL,
    category_id   INT NOT NULL REFERENCES categories(category_id),
    price         DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    stock_quantity INT NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0)
);

CREATE TABLE orders (
    order_id   INT IDENTITY(1,1) PRIMARY KEY,
    client_id  INT NOT NULL REFERENCES clients(client_id),
    user_id    INT NOT NULL REFERENCES users(user_id),
    status     NVARCHAR(20) NOT NULL DEFAULT N'Нове'
        CHECK (status IN (N'Нове', N'Виконується', N'Виконане', N'Скасоване')),
    order_date DATETIME NOT NULL DEFAULT GETDATE(),
    comment    NVARCHAR(200) NULL
);

CREATE TABLE order_items (
    item_id    INT IDENTITY(1,1) PRIMARY KEY,
    order_id   INT NOT NULL REFERENCES orders(order_id),
    product_id INT NOT NULL REFERENCES products(product_id),
    quantity   INT NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(10,2) NOT NULL CHECK (unit_price >= 0)
);

CREATE TABLE event_log (
    event_id   INT IDENTITY(1,1) PRIMARY KEY,
    user_id    INT NULL REFERENCES users(user_id),
    event_type NVARCHAR(20)  NOT NULL,   -- LOGIN, LOGIN_FAIL, INSERT, UPDATE, DELETE, ORDER, SYSTEM
    event_text NVARCHAR(300) NOT NULL,
    event_time DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE settings (
    setting_key   NVARCHAR(50)  PRIMARY KEY,
    setting_value NVARCHAR(200) NOT NULL,
    description   NVARCHAR(200) NULL
);
GO

CREATE INDEX ix_orders_date    ON orders(order_date);
CREATE INDEX ix_event_log_time ON event_log(event_time);
GO

/* ============ 2. ФУНКЦІЯ: сума замовлення ============ */

CREATE FUNCTION dbo.fn_order_total(@order_id INT)
RETURNS DECIMAL(12,2)
AS
BEGIN
    DECLARE @total DECIMAL(12,2);
    SELECT @total = SUM(quantity * unit_price)
    FROM order_items WHERE order_id = @order_id;
    RETURN ISNULL(@total, 0);
END
GO

/* ======== 3. ТРИГЕР: списання товару зі складу ======== */
-- (якщо товару не хватає - CHECK у products відкочує операцію)

CREATE TRIGGER trg_order_items_stock
ON order_items AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p
    SET p.stock_quantity = p.stock_quantity - s.qty
    FROM products p
    JOIN (SELECT product_id, SUM(quantity) AS qty
          FROM inserted GROUP BY product_id) s ON s.product_id = p.product_id;
END
GO

/* =========== 4. ПРОЦЕДУРА: запис у журнал =========== */

CREATE PROCEDURE sp_log_event
    @user_id    INT,
    @event_type NVARCHAR(20),
    @event_text NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO event_log (user_id, event_type, event_text)
    VALUES (@user_id, @event_type, @event_text);
END
GO

/* ====== 5. ПРОЦЕДУРА: звіт продажів за період ====== */

CREATE PROCEDURE sp_sales_report
    @date_from DATE,
    @date_to   DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.order_id, o.order_date, c.full_name AS client_name,
           dbo.fn_order_total(o.order_id) AS order_total
    FROM orders o
    JOIN clients c ON c.client_id = o.client_id
    WHERE o.status = N'Виконане'
      AND o.order_date >= @date_from
      AND o.order_date <  DATEADD(DAY, 1, @date_to)
    ORDER BY o.order_date;
END
GO

/* ================== ПЕРВИННІ ДАНІ ================== */

INSERT INTO roles (role_name)
VALUES (N'Адміністратор'), (N'Менеджер'), (N'Працівник');

INSERT INTO settings (setting_key, setting_value, description) VALUES
 (N'firm_name',          N'Комп''ютерна фірма «Тех-Сервіс»', N'Назва фірми'),
 (N'prepayment_percent', N'30',                              N'Частка передоплати, %');

-- Початковий користувач: логін admin, пароль admin123 (SHA-256, HEX верхнім регістром).
-- Пароль слід змінити після першого входу.
INSERT INTO users (login, password_hash, full_name, role_id) VALUES
 ('admin', '240BE518FABD2724DDB6F04EEB1DA5967448D7E831C08C8FA822809F74C720A9',
  N'Адміністратор системи', 1);

INSERT INTO categories (category_name)
VALUES (N'Готові ПК'), (N'Комплектуючі'), (N'Периферія');
GO
