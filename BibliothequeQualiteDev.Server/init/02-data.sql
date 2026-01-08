USE bibliotheque;
SET FOREIGN_KEY_CHECKS=0;

-- ROLES
INSERT INTO ROLES VALUES
(1,'Administrateur'),
(2,'Bibliothecaire'),
(3,'Etudiant'),
(4,'Professeur');

-- RIGHTS
INSERT INTO RIGHTS VALUES
(1,'gerer_utilisateurs'),
(2,'gerer_livres'),
(3,'gerer_stock');

-- ROLE_RIGHTS
-- Admin
INSERT INTO ROLE_RIGHTS (role_id, right_id) VALUES
(1,1),
(1,2),
(1,3);

-- Bibliothecaire
INSERT INTO ROLE_RIGHTS (role_id, right_id) VALUES
(2,2),
(2,3);




-- USERS
INSERT INTO USERS VALUES
(1, '$2a$12$XltnyTWkv8E7p6LuVHV4tO8tY6zaHoaKLKokRpBBYdbGjxxPNpXRC', 'Alice', 'alice@biblio.fr', 1),
(2, '$2a$12$XltnyTWkv8E7p6LuVHV4tO8tY6zaHoaKLKokRpBBYdbGjxxPNpXRC', 'Admin', 'admin@test.fr', 1),
(3, '$2a$12$XltnyTWkv8E7p6LuVHV4tO8tY6zaHoaKLKokRpBBYdbGjxxPNpXRC', 'Biblio', 'biblio@test.fr', 2);

INSERT INTO BOOK (book_name, book_author, book_date, book_editor, book_image_ext) VALUES
('Le Petit Prince', 'Antoine de Saint-Exupéry', '1943-04-06', 'Gallimard', 'jpg'),
('1984', 'George Orwell', '1949-06-08', 'Secker & Warburg', 'png'),
('L''Étranger', 'Albert Camus', '1942-05-19', 'Gallimard', 'jpg'),
('Les Misérables', 'Victor Hugo', '1862-01-01', 'A. Lacroix', 'jpg'),
('Clean Code', 'Robert C. Martin', '2008-08-01', 'Prentice Hall', 'png');

INSERT INTO LIBRARY_STOCK (book_id, total_stock, borrowed_count) VALUES
(1, 5, 1),
(2, 3, 0),
(3, 4, 2),
(4, 2, 1),
(5, 6, 3);


INSERT INTO BORROWED (user_id, book_id, date_start, date_end, is_returned) VALUES
(4, 1, '2025-01-01', '2025-01-15', 1),
(4, 3, '2025-01-10', '2025-01-25', 0),
(5, 5, '2025-01-05', '2025-01-20', 0),
(6, 4, '2024-12-15', '2025-01-05', 1);

INSERT INTO DELAY (borrow_id, delay_time) VALUES
(2, 5),
(3, 2);


SET FOREIGN_KEY_CHECKS=1;

