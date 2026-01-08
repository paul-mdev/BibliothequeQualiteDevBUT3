USE bibliotheque;
SET FOREIGN_KEY_CHECKS=0;

-- ROLES
INSERT INTO ROLES VALUES
(1,'Administrateur'),(2,'Bibliothecaire'),
(3,'Etudiant'),(4,'Professeur');

-- RIGHTS
INSERT INTO RIGHTS VALUES
(1,'emprunter_livre'),(2,'prolonger_emprunt'),
(3,'reserver_livre'),(4,'gerer_utilisateurs'),
(5,'gerer_livres'),(6,'gerer_stock'),(7,'voir_retards');

-- ROLE_RIGHTS
INSERT INTO ROLE_RIGHTS VALUES
(1,1),(1,2),(1,3),(1,4),(1,5),(1,6),(1,7),
(2,1),(2,2),(2,3),(2,5),(2,6),(2,7),
(3,1),(3,3),
(4,1),(4,2),(4,3);

-- USERS (mot de passe : "password" hashé avec bcrypt pour tous)
INSERT INTO USERS (user_id, user_pswd, user_name, user_mail, role_id) VALUES
-- Administrateurs
(1, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Alice Martin', 'alice@biblio.fr', 1),
(2, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin Test', 'admin@test.fr', 1),

-- Bibliothécaires
(3, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Sophie Durand', 'sophie.durand@biblio.fr', 2),
(4, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Marc Lefebvre', 'marc.lefebvre@biblio.fr', 2),

-- Étudiants
(5, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Emma Dubois', 'emma.dubois@etudiant.fr', 3),
(6, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Lucas Bernard', 'lucas.bernard@etudiant.fr', 3),
(7, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Chloé Petit', 'chloe.petit@etudiant.fr', 3),
(8, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Thomas Roux', 'thomas.roux@etudiant.fr', 3),
(9, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Léa Moreau', 'lea.moreau@etudiant.fr', 3),
(10, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Hugo Simon', 'hugo.simon@etudiant.fr', 3),
(11, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Manon Laurent', 'manon.laurent@etudiant.fr', 3),
(12, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Nathan Girard', 'nathan.girard@etudiant.fr', 3),

-- Professeurs
(13, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Dr. Marie Dupont', 'marie.dupont@prof.fr', 4),
(14, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Prof. Jean Leroy', 'jean.leroy@prof.fr', 4),
(15, '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Dr. Claire Fontaine', 'claire.fontaine@prof.fr', 4);

-- BOOKS
INSERT INTO BOOK (book_id, book_name, book_author, book_date, book_editor, book_image_ext) VALUES
-- Informatique et Programmation
(1, 'Clean Code', 'Robert C. Martin', '2008-08-01', 'Prentice Hall', 'jpg'),
(2, 'Design Patterns', 'Gang of Four', '1994-10-21', 'Addison-Wesley', 'jpg'),
(3, 'The Pragmatic Programmer', 'Andrew Hunt', '1999-10-30', 'Addison-Wesley', 'jpg'),
(4, 'Introduction to Algorithms', 'Thomas H. Cormen', '2009-07-31', 'MIT Press', 'jpg'),
(5, 'JavaScript: The Good Parts', 'Douglas Crockford', '2008-05-01', "O'Reilly", 'jpg'),
(6, 'You Don''t Know JS', 'Kyle Simpson', '2014-06-01', "O'Reilly", 'jpg'),
(7, 'Python Crash Course', 'Eric Matthes', '2015-11-01', 'No Starch Press', 'jpg'),
(8, 'Effective Java', 'Joshua Bloch', '2017-12-27', 'Addison-Wesley', 'jpg'),

-- Science et Mathématiques
(9, 'Une brève histoire du temps', 'Stephen Hawking', '1988-04-01', 'Bantam Books', 'jpg'),
(10, 'Cosmos', 'Carl Sagan', '1980-10-12', 'Random House', 'jpg'),
(11, 'Le Gène égoïste', 'Richard Dawkins', '1976-01-01', 'Oxford University Press', 'jpg'),
(12, 'Sapiens', 'Yuval Noah Harari', '2011-01-01', 'Harper', 'jpg'),
(13, 'L''Origine des espèces', 'Charles Darwin', '1859-11-24', 'John Murray', 'jpg'),

-- Littérature française
(14, 'Les Misérables', 'Victor Hugo', '1862-04-03', 'Albert Lacroix', 'jpg'),
(15, 'Le Petit Prince', 'Antoine de Saint-Exupéry', '1943-04-06', 'Reynal & Hitchcock', 'jpg'),
(16, 'L''Étranger', 'Albert Camus', '1942-01-01', 'Gallimard', 'jpg'),
(17, 'Madame Bovary', 'Gustave Flaubert', '1856-12-15', 'Michel Lévy', 'jpg'),
(18, 'Le Rouge et le Noir', 'Stendhal', '1830-11-13', 'A. Levavasseur', 'jpg'),

-- Littérature anglaise
(19, '1984', 'George Orwell', '1949-06-08', 'Secker & Warburg', 'jpg'),
(20, 'Pride and Prejudice', 'Jane Austen', '1813-01-28', 'T. Egerton', 'jpg'),
(21, 'To Kill a Mockingbird', 'Harper Lee', '1960-07-11', 'J.B. Lippincott', 'jpg'),
(22, 'The Great Gatsby', 'F. Scott Fitzgerald', '1925-04-10', 'Scribner', 'jpg'),
(23, 'Harry Potter à l''école des sorciers', 'J.K. Rowling', '1997-06-26', 'Bloomsbury', 'jpg'),
(24, 'The Lord of the Rings', 'J.R.R. Tolkien', '1954-07-29', 'Allen & Unwin', 'jpg'),

-- Philosophie
(25, 'Ainsi parlait Zarathoustra', 'Friedrich Nietzsche', '1883-01-01', 'Ernst Schmeitzner', 'jpg'),
(26, 'Critique de la raison pure', 'Emmanuel Kant', '1781-01-01', 'Johann Friedrich Hartknoch', 'jpg'),
(27, 'Le Banquet', 'Platon', '0001-01-01', 'Antique', 'jpg'),
(28, 'Discours de la méthode', 'René Descartes', '1637-01-01', 'Jan Maire', 'jpg'),

-- Histoire
(29, 'Guns, Germs, and Steel', 'Jared Diamond', '1997-03-01', 'W. W. Norton', 'jpg'),
(30, 'The Silk Roads', 'Peter Frankopan', '2015-08-27', 'Bloomsbury', 'jpg'),
(31, 'Histoire de France', 'Jules Michelet', '1833-01-01', 'Hachette', 'jpg'),

-- Économie et Business
(32, 'Le Capital au XXIe siècle', 'Thomas Piketty', '2013-09-01', 'Seuil', 'jpg'),
(33, 'Freakonomics', 'Steven Levitt', '2005-04-12', 'William Morrow', 'jpg'),
(34, 'Thinking, Fast and Slow', 'Daniel Kahneman', '2011-10-25', 'Farrar, Straus and Giroux', 'jpg'),
(35, 'The Lean Startup', 'Eric Ries', '2011-09-13', 'Crown Business', 'jpg'),

-- Romans contemporains
(36, 'L''Alchimiste', 'Paulo Coelho', '1988-01-01', 'HarperCollins', 'jpg'),
(37, 'La Vérité sur l''affaire Harry Quebert', 'Joël Dicker', '2012-09-05', 'De Fallois', 'jpg'),
(38, 'Gone Girl', 'Gillian Flynn', '2012-06-05', 'Crown Publishing', 'jpg'),
(39, 'The Kite Runner', 'Khaled Hosseini', '2003-05-29', 'Riverhead Books', 'jpg'),
(40, 'Life of Pi', 'Yann Martel', '2001-09-11', 'Knopf Canada', 'jpg');

-- LIBRARY_STOCK
INSERT INTO LIBRARY_STOCK (book_id, total_stock, borrowed_count) VALUES
-- Livres populaires avec plusieurs exemplaires
(1, 5, 2), (2, 4, 1), (3, 4, 2), (4, 3, 1), (5, 3, 1),
(6, 3, 0), (7, 4, 2), (8, 3, 1), (9, 5, 3), (10, 4, 2),
(11, 3, 1), (12, 6, 4), (13, 2, 0), (14, 4, 2), (15, 5, 3),
(16, 3, 1), (17, 3, 1), (18, 2, 1), (19, 5, 3), (20, 3, 1),
(21, 4, 2), (22, 3, 1), (23, 6, 4), (24, 4, 2), (25, 2, 0),
(26, 2, 1), (27, 2, 0), (28, 2, 1), (29, 3, 1), (30, 3, 2),
(31, 2, 0), (32, 3, 1), (33, 3, 1), (34, 4, 2), (35, 3, 1),
(36, 4, 2), (37, 3, 1), (38, 3, 1), (39, 3, 1), (40, 3, 0);

-- BORROWED
INSERT INTO BORROWED (id_borrow, user_id, book_id, date_start, date_end, is_returned) VALUES
-- Emprunts en cours avec différentes échéances
-- CRITIQUES (≤ 5 jours)
(1, 5, 1, '2025-12-10', '2026-01-09', 0),   -- 1 jour restant
(2, 6, 2, '2025-12-08', '2026-01-12', 0),   -- 4 jours restants
(3, 7, 9, '2025-12-11', '2026-01-10', 0),   -- 2 jours restants

-- AVERTISSEMENT (6-30 jours)
(4, 8, 3, '2025-12-15', '2026-01-14', 0),   -- 6 jours restants
(5, 9, 7, '2025-12-20', '2026-01-19', 0),   -- 11 jours restants
(6, 10, 10, '2025-12-25', '2026-01-24', 0), -- 16 jours restants
(7, 11, 12, '2026-01-01', '2026-01-31', 0), -- 23 jours restants
(8, 12, 14, '2026-01-05', '2026-02-04', 0), -- 27 jours restants
(9, 5, 15, '2026-01-08', '2026-02-07', 0),  -- 30 jours restants (votre test!)

-- Emprunts normaux (> 30 jours)
(10, 13, 4, '2026-01-05', '2026-03-05', 0),  -- 56 jours
(11, 14, 8, '2026-01-07', '2026-03-07', 0),  -- 58 jours
(12, 6, 19, '2026-01-06', '2026-03-06', 0),  -- 57 jours
(13, 7, 21, '2026-01-05', '2026-02-20', 0),  -- 43 jours
(14, 8, 23, '2026-01-04', '2026-02-15', 0),  -- 38 jours

-- Emprunts rendus (historique)
(15, 5, 5, '2025-11-01', '2025-11-30', 1),
(16, 6, 11, '2025-11-05', '2025-12-05', 1),
(17, 7, 16, '2025-10-10', '2025-11-10', 1),
(18, 8, 17, '2025-10-15', '2025-11-15', 1),
(19, 9, 18, '2025-09-20', '2025-10-20', 1),
(20, 10, 22, '2025-09-25', '2025-10-25', 1),
(21, 11, 26, '2025-08-10', '2025-09-10', 1),
(22, 12, 28, '2025-08-15', '2025-09-15', 1),
(23, 13, 29, '2025-07-01', '2025-08-01', 1),
(24, 14, 32, '2025-07-10', '2025-08-10', 1),
(25, 15, 33, '2025-06-15', '2025-07-15', 1),

-- Emprunts avec retards (dates dépassées, non rendus)
(26, 6, 20, '2025-11-10', '2025-12-10', 0),  -- 29 jours de retard
(27, 7, 24, '2025-11-15', '2025-12-15', 0),  -- 24 jours de retard
(28, 8, 30, '2025-11-20', '2025-12-20', 0),  -- 19 jours de retard
(29, 9, 36, '2025-12-01', '2025-12-31', 0),  -- 8 jours de retard
(30, 10, 37, '2025-12-05', '2026-01-04', 0), -- 4 jours de retard

-- Plus d'emprunts pour les professeurs (droits étendus)
(31, 13, 34, '2025-12-01', '2026-02-28', 0), -- 51 jours
(32, 14, 35, '2025-12-15', '2026-03-15', 0), -- 66 jours
(33, 15, 38, '2026-01-01', '2026-03-01', 0), -- 52 jours
(34, 13, 39, '2025-11-01', '2026-01-30', 0), -- 22 jours

-- Emprunts multiples pour tester les stats
(35, 5, 9, '2025-10-01', '2025-10-30', 1),
(36, 5, 12, '2025-09-01', '2025-09-30', 1),
(37, 6, 12, '2025-08-01', '2025-08-30', 1),
(38, 7, 12, '2025-07-01', '2025-07-30', 1),
(39, 8, 19, '2025-10-15', '2025-11-15', 1),
(40, 9, 19, '2025-09-15', '2025-10-15', 1);

-- DELAY (retards enregistrés)
INSERT INTO DELAY (delay_id, borrow_id, delay_time) VALUES
(1, 26, 29),  -- 29 jours de retard
(2, 27, 24),  -- 24 jours de retard
(3, 28, 19),  -- 19 jours de retard
(4, 29, 8),   -- 8 jours de retard
(5, 30, 4);   -- 4 jours de retard

SET FOREIGN_KEY_CHECKS=1;