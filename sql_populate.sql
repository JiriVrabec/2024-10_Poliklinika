INSERT INTO Diagnoses (diagnose_name) VALUES
('Vampirismus'), ('Zánět spojivek'), ('Lykantropie'),
('Rakovina střev'), ('Poranění kolene šípem'), ('Bodná rána morgulskou ocelí'),
('Maniodeprese'), ('Prodělal COVID'), ('Kuřák'),
('Diabetik'), ('Alkoholik'), ('Porucha jater');

INSERT INTO Patients (first_name, last_name, birth_date) VALUES
('Frodo', 'Pytlík', '1937-6-9'),
('Jan', 'Novák', '1987-5-3'),
('Abraham', 'Van Helsing', '1897-4-2'),
('Serana', 'Harkon', '1012-6-26'),
('Jan', 'Novák', '1985-6-8'),
('Aela', 'the Huntress', '2011-11-11'),
('Jana', 'Dvořáková', '1958-8-7'),
('Chuck', 'Norris', '1940-3-10'),
('Miloš', 'Zeman', '1944-9-28'),
('Jeníček', 'Hansel', '1971-1-7');

INSERT INTO Patients_Diagnoses (patient_id, diagnose_id) VALUES
(1, 6), (1, 7),
(2, 2), (2, 9),
(3, 3),
(4, 1), (4, 5),
(5, 8),
(6, 3), (6, 5),
(7, 11), (7, 8),
(9, 1), (9, 2), (9, 8), (9, 9), (9, 10), (9, 11), (9, 12),
(10, 10);