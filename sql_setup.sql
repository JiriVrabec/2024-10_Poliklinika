CREATE DATABASE PoliklinikaMemos;

USE PoliklinikaMemos;

CREATE TABLE Patients (
	patient_id INTEGER UNSIGNED AUTO_INCREMENT,
    first_name VARCHAR(64) NOT NULL,
    last_name VARCHAR(64) NOT NULL,
    birth_date DATE NOT NULL,
    PRIMARY KEY (patient_id)
);

CREATE TABLE Diagnoses (
	diagnose_id INTEGER UNSIGNED AUTO_INCREMENT,
    diagnose_name VARCHAR(128) NOT NULL,
    PRIMARY KEY (diagnose_id)
);

CREATE TABLE Patients_Diagnoses (
    patient_id INT UNSIGNED NOT NULL,
    diagnose_id INT UNSIGNED NOT NULL,
    FOREIGN KEY (patient_id) REFERENCES Patients(patient_id),
    FOREIGN KEY (diagnose_id) REFERENCES Diagnoses(diagnose_id),
    UNIQUE (patient_id, diagnose_id)
);

CREATE INDEX pd_patient_id ON Patients_Diagnoses(patient_id);
-- CREATE INDEX pd_diagnose_id ON Patients_Diagnoses(diagnose_id);