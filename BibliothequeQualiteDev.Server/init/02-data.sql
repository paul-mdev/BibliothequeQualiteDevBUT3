USE bibliotheque;
SET FOREIGN_KEY_CHECKS=0;

-- STATES
INSERT INTO STATES VALUES
(1,'Neuf'),(2,'Très bon état'),(3,'Bon état'),
(4,'État moyen'),(5,'Mauvais état'),(6,'Perdu');

-- ROLES
INSERT INTO ROLES VALUES
(1,'Administrateur'),(2,'Bibliothécaire'),
(3,'Étudiant'),(4,'Professeur');

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

-- USERS
INSERT INTO USERS VALUES
(1,'$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi','Alice','alice@biblio.fr',1),
(2,'1','Admin','admin@test.fr',1)
;

SET FOREIGN_KEY_CHECKS=1;
