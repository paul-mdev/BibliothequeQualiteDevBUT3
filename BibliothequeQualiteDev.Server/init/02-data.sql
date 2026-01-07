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

SET FOREIGN_KEY_CHECKS=1;
