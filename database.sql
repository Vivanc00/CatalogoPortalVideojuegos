-- =====================================================================
-- Práctica 3 - Desarrollo de Interfaces
-- Catálogo de videojuegos: acceso a BBDD MySQL
-- Script de creación de la base de datos, la tabla Usuarios y datos de prueba
-- Ejecutar en MySQL Workbench conectado como root
-- =====================================================================

DROP DATABASE IF EXISTS catalogo_videojuegos;
CREATE DATABASE catalogo_videojuegos
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_spanish_ci;

USE catalogo_videojuegos;

-- Tabla de usuarios: el nombre de usuario no admite duplicados (UNIQUE)
CREATE TABLE Usuarios (
    id             INT          NOT NULL AUTO_INCREMENT,
    nombre_usuario VARCHAR(50)  NOT NULL,
    password       VARCHAR(100) COLLATE utf8mb4_0900_bin NOT NULL,  -- comparación exacta: distingue mayúsculas y espacios finales
    PRIMARY KEY (id),
    CONSTRAINT uq_usuarios_nombre UNIQUE (nombre_usuario)
);

-- Usuarios de prueba (los mismos que tenía la Práctica 2 en memoria)
INSERT INTO Usuarios (nombre_usuario, password) VALUES
    ('admin', '1234'),
    ('dam',   '2026');

-- Usuario de MySQL que utiliza la aplicación (no se usa root desde C#).
-- Solo tiene los permisos que necesita: consultar e insertar en Usuarios.
DROP USER IF EXISTS 'catalogo_app'@'localhost';
CREATE USER 'catalogo_app'@'localhost' IDENTIFIED BY 'CatalogoApp2026!';
GRANT SELECT, INSERT ON catalogo_videojuegos.Usuarios TO 'catalogo_app'@'localhost';
FLUSH PRIVILEGES;

-- Comprobación
SELECT * FROM Usuarios;
