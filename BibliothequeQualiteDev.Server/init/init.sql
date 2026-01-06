USE bibliotheque;

SET FOREIGN_KEY_CHECKS=0;
START TRANSACTION;

-- ----------------------------------------------------
-- Table STATE
-- ----------------------------------------------------
CREATE TABLE IF NOT EXISTS STATE (
    state_id   TINYINT UNSIGNED NOT NULL AUTO_INCREMENT,
    state_name VARCHAR(30) NOT NULL,
    PRIMARY KEY (state_id)
) ENGINE=InnoDB;

-- ----------------------------------------------------
-- Table BOOK
-- ----------------------------------------------------
CREATE TABLE IF NOT EXISTS BOOK (
    book_id        INT UNSIGNED NOT NULL AUTO_INCREMENT,
    book_name      VARCHAR(150) NOT NULL,
    book_author    VARCHAR(100) NOT NULL,
    book_date      DATE NULL,
    book_editor    VARCHAR(100) NULL,
    book_image_ext VARCHAR(100) NULL,
    PRIMARY KEY (book_id)
) ENGINE=InnoDB;

-- ----------------------------------------------------
-- Table RIGHTS
-- ----------------------------------------------------
CREATE TABLE IF NOT EXISTS RIGHTS (
    right_id   SMALLINT UNSIGNED NOT NULL AUTO_INCREMENT,
    right_name VARCHAR(50) NOT NULL UNIQUE,
    PRIMARY KEY (right_id)
) ENGINE=InnoDB;

-- ----------------------------------------------------
-- Table ROLE
-- ----------------------------------------------------
CREATE TABLE IF NOT EXISTS ROLE (
    role_id   TINYINT UNSIGNED NOT NULL AUTO_INCREMENT,
    role_name VARCHAR(50) NOT NULL UNIQUE,
    PRIMARY KEY (role_id)
) ENGINE=InnoDB;

-- ----------------------------------------------------
-- Table ROLE_RIGHTS
-- ----------------------------------------------------
CREATE TABLE IF NOT EXISTS ROLE_RIGHTS (
    role_id  TINYINT UNSIGNED NOT NULL,
    right_id SMALLINT UNSIGNED NOT NULL,
    PRIMARY KEY (role_id, right_id),
    FOREIGN KEY (role_id)  REFERENCES ROLE(role_id)   ON DELETE CASCADE,
    FOREIGN KEY (right_id) REFERENCES RIGHTS(right_id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ----------------------------------------------------
-- Table USER
-- ----------------------------------------------------
CREATE TABLE IF NOT EXISTS USER (
    user_id    INT UNSIGNED NOT NULL AUTO_INCREMENT,
    user_pswd  VARCHAR(255) NOT NULL,
    user_name  VARCHAR(100) NOT NULL,
    user_mail  VARCHAR(150) NOT NULL UNIQUE,
    role_id    TINYINT UNSIGNED NOT NULL,
    PRIMARY KEY (user_id),
    FOREIGN KEY (role_id) REFERENCES ROLE(role_id) ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ----------------------------------------------------
-- Table LIBRARY_STOCK
-- ----------------------------------------------------
CREATE TABLE IF NOT EXISTS LIBRARY_STOCK (
    stock_id   INT UNSIGNED NOT NULL AUTO_INCREMENT,
    book_id    INT UNSIGNED NOT NULL,
    state_id   TINYINT UNSIGNED NOT NULL,
    PRIMARY KEY (stock_id),
    FOREIGN KEY (book_id)  REFERENCES BOOK(book_id)   ON DELETE CASCADE,
    FOREIGN KEY (state_id) REFERENCES STATE(state_id) ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ----------------------------------------------------
-- Table BORROWED
-- ----------------------------------------------------
CREATE TABLE IF NOT EXISTS BORROWED (
    id_borrow   INT UNSIGNED NOT NULL AUTO_INCREMENT,
    user_id     INT UNSIGNED NOT NULL,
    stock_id    INT UNSIGNED NOT NULL,
    date_start  DATE NOT NULL,
    date_end    DATE NOT NULL,
    is_returned TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (id_borrow),
    FOREIGN KEY (user_id)  REFERENCES USER(user_id)          ON DELETE CASCADE,
    FOREIGN KEY (stock_id) REFERENCES LIBRARY_STOCK(stock_id) ON DELETE CASCADE,
    INDEX idx_date_end (date_end),
    INDEX idx_user (user_id)
) ENGINE=InnoDB;

-- ----------------------------------------------------
-- Table DELAY
-- ----------------------------------------------------
CREATE TABLE IF NOT EXISTS DELAY (
    delay_id   INT UNSIGNED NOT NULL AUTO_INCREMENT,
    borrow_id  INT UNSIGNED NOT NULL,
    delay_time INT NOT NULL,
    PRIMARY KEY (delay_id),
    FOREIGN KEY (borrow_id) REFERENCES BORROWED(id_borrow) ON DELETE CASCADE
) ENGINE=InnoDB;

-- =====================================================
-- INSERTION DES DONNÉES
-- =====================================================

INSERT INTO STATE (state_id, state_name) VALUES
(1,'Neuf'),(2,'Très bon état'),(3,'Bon état'),(4,'État moyen'),(5,'Mauvais état'),(6,'Perdu');

INSERT INTO BOOK VALUES
(1,'1984','George Orwell','1949-06-08','Secker & Warburg','jpg'),
(2,'Le Petit Prince','Antoine de Saint-Exupéry','1943-04-06','Gallimard','jpg'),
(3,'Harry Potter à l''école des sorciers','J.K. Rowling','1997-06-26','Gallimard','jpg'),
(4,'Le Seigneur des Anneaux','J.R.R. Tolkien','1954-07-29','Allen & Unwin','jpg'),
(5,'Germinal','Émile Zola','1885-03-01','Charpentier','germinal.jpg'),
(6,'L''Étranger','Albert Camus','1942-05-19','Gallimard','etranger.jpg'),
(7,'Dune','Frank Herbert','1965-08-01','Chilton Books','dune.jpg'),
(8,'Le Nom du vent','Patrick Rothfuss','2007-03-27','DAW Books','jpg'),
(9,'Sapiens','Yuval Noah Harari','2011-01-01','Harvill Secker','jpg'),
(10,'Atomic Habits','James Clear','2018-10-16','Avery','atomichabits.jpg'),
(11,'Le Comte de Monte-Cristo','Alexandre Dumas','1844-08-28','Bauzin','jpg'),
(12,'Orgueil et Préjugés','Jane Austen','1813-01-28','T. Egerton','jpg'),
(13,'Fondation','Isaac Asimov','1951-05-01','Gnome Press','jpg'),
(14,'L''Alchimiste','Paulo Coelho','1988-01-01','Rocco','jpg'),
(15,'Le Parfum','Patrick Süskind','1985-03-01','Diogenes','jpg');

-- (le reste de tes INSERT est inchangé)

COMMIT;
SET FOREIGN_KEY_CHECKS=1;
