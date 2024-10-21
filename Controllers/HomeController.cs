using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Poliklinika.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;

namespace Poliklinika.Controllers;

public class HomeController : Controller
{
    private const string _dateFormat = "yyyy-MM-dd";
    private readonly string _connectionString;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultSQL")!;
    }
    #region Actions
    /// <summary>
    /// List all patients
    /// </summary>
    public IActionResult Overview()
    {
        try
        {
            return View(MySQLGetPatients());
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return RedirectToAction("Error");
        }
    }
    /// <summary>
    /// Get patient's data and list of diagnoses
    /// </summary>
    /// <param name="id">Patient's id</param>
    public IActionResult Editor(uint id)
    {
        try
        {
            Patient patient = id == 0 ? new() : MySQLGetPatient(id);
            var diagnoses = MySQLGetDiagnoses();
            var chceked = MySQLPatientDiagnoses(id);
            foreach (uint diagnose in chceked)
            {
                diagnoses[diagnose].Checked = true;
            }
            return View(new EditorModel() { Patient = patient, Diagnoses = diagnoses });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return RedirectToAction("Error");
        }
    }
    /// <summary>
    /// Add or edit patient, update their diagnoses, go to Overview
    /// </summary>
    /// <param name="patient">Patient data to be saved</param>
    public IActionResult SubmitEdit(EditorModel em)
    {
        try
        {
            // Add/edit patient
            Patient patient = em.Patient;
            if (patient.PatientId == 0) // Insert
            {
                patient.PatientId = MySQLNewPatient(patient);
                if (patient.PatientId == 0) return RedirectToAction("Overview");  // failed
            }
            else    // Update
            {
                int changes = MySQLWriteOnly(UpdatePatientQuery(patient));
                if (changes <= 0) return RedirectToAction("Overview");  // not found
            }
            // Get diagnose changes
            HashSet<uint> diagnoses = new(MySQLPatientDiagnoses(patient.PatientId));
            List<uint> toAdd = new();
            List<uint> toRemove = new();
            foreach (var diagnose in em.Diagnoses)
            {
                if (diagnoses.Contains(diagnose.Key))
                {
                    if (!diagnose.Value.Checked) toRemove.Add(diagnose.Key);
                }
                else if (diagnose.Value.Checked) toAdd.Add(diagnose.Key);
            }
            // Make diagnose changes
            string addQuery = InsertDiagnosesQuery(patient.PatientId, toAdd);
            string removeQuery = DeleteDiagnosesQuery(patient.PatientId, toRemove);
            if (addQuery.Length > 0) MySQLWriteOnly(addQuery);
            if (removeQuery.Length > 0) MySQLWriteOnly(removeQuery);

            return RedirectToAction("Overview");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return RedirectToAction("Error");
        }
    }
    /// <summary>
    /// Delete patient, go to Overview
    /// </summary>
    /// <param name="id">Patient's id</param>
    public IActionResult Delete(uint id)
    {
        try
        {
            MySQLWriteOnly(DeleteAllDiagnosesQuery(id));
            MySQLWriteOnly(DeletePatientQuery(id));
            return RedirectToAction("Overview");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return RedirectToAction("Error");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    #endregion
    #region SqlCommands
    /// <summary>
    /// Execute any query on preset MySQL server
    /// </summary>
    /// <returns>Number of rows affected</returns>
    private int MySQLWriteOnly(string query)
    {
        using MySqlConnection connection = new(_connectionString);
        using MySqlCommand command = new(query, connection);
        connection.Open();
        return command.ExecuteNonQuery();
    }
    /// <summary>
    /// Get all diagnoses from preset MySQL server
    /// </summary>
    /// <returns>Dictionary of diagnose data where key = diagnose id</returns>
    private Dictionary<uint, Diagnose> MySQLGetDiagnoses()
    {
        using MySqlConnection connection = new(_connectionString);
        using MySqlCommand command = new("SELECT * FROM Diagnoses", connection);
        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        Dictionary<uint, Diagnose> diagnoses = new();
        while (reader.Read())
        {
            diagnoses.Add(reader.GetUInt32(0), new() { DiagnoseName = reader.GetString(1) });
        }
        return diagnoses;
    }
    /// <summary>
    /// Get all diagnoses of specified patient from preset MySQL server
    /// </summary>
    /// <returns>List of diagnose ids</returns>
    private List<uint> MySQLPatientDiagnoses(uint patient)
    {
        using MySqlConnection connection = new(_connectionString);
        using MySqlCommand command = new(SelectDiagnosesQuery(patient), connection);
        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        List<uint> diagnoses = new();
        while (reader.Read())
        {
            diagnoses.Add(reader.GetUInt32(0));
        }
        return diagnoses;
    }
    /// <summary>
    /// Insert patient into preset MySQL server
    /// </summary>
    /// <returns>Number of rows affected</returns>
    private uint MySQLNewPatient(Patient patient)
    {
        using MySqlConnection connection = new(_connectionString);
        using MySqlCommand command = new(InsertPatientQuery(patient), connection);
        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetUInt32(0);
        }
        return 0;
    }
    /// <summary>
    /// Get patient by id from preset MySQL server
    /// </summary>
    /// <returns>Patient record</returns>
    private Patient MySQLGetPatient(uint id)
    {
        using MySqlConnection connection = new(_connectionString);
        using MySqlCommand command = new(SelectPatientQuery(id), connection);
        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ReadPatient(reader);
        }
        return new();
    }
    /// <summary>
    /// Get patients from preset MySQL server
    /// </summary>
    /// <returns>List of patients</returns>
    private List<Patient> MySQLGetPatients()
    {
        // to do: add paging?
        using MySqlConnection connection = new(_connectionString);
        using MySqlCommand command = new("SELECT * FROM Patients", connection);
        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        List<Patient> patients = new();
        while (reader.Read())
        {
            patients.Add(ReadPatient(reader));
        }
        return patients;
    }
    private static Patient ReadPatient(MySqlDataReader reader)
    {
        return new()
        {
            PatientId = reader.GetUInt32(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2),
            BirthDate = DateOnly.FromDateTime(reader.GetDateTime(3))
        };
    }
    #endregion
    #region Queries
    private static string InsertPatientQuery(Patient patient) =>
    $"INSERT INTO Patients (first_name, last_name, birth_date) VALUES ('{patient.FirstName}', '{patient.LastName}', '{patient.BirthDate.ToString(_dateFormat)}');" +
    "SELECT LAST_INSERT_ID();";
    private static string UpdatePatientQuery(Patient patient) =>
    $"UPDATE Patients SET first_name='{patient.FirstName}', last_name='{patient.LastName}', birth_date='{patient.BirthDate.ToString(_dateFormat)}' WHERE patient_id = {patient.PatientId};";
    private static string SelectPatientQuery(uint id) =>
    $"SELECT * FROM Patients WHERE patient_id = {id};";
    private static string DeletePatientQuery(uint id) =>
    $"DELETE FROM Patients WHERE patient_id = {id};";
    private static string SelectDiagnosesQuery(uint patient) =>
    $"SELECT diagnose_id FROM Patients_Diagnoses WHERE patient_id = {patient};";
    private static string InsertDiagnosesQuery(uint patient, List<uint> diagnoses)
    {
        StringBuilder sb = new();
        foreach (uint diagnose in diagnoses)
        {
            sb.Append($"INSERT INTO Patients_Diagnoses (patient_id, diagnose_id) VALUES ('{patient}', '{diagnose}');");
        }
        return sb.ToString();
    }
    private static string DeleteDiagnosesQuery(uint patient, List<uint> diagnoses)
    {
        StringBuilder sb = new();
        foreach (uint diagnose in diagnoses)
        {
            sb.Append($"DELETE FROM Patients_Diagnoses WHERE patient_id = {patient} AND diagnose_id = {diagnose};");
        }
        return sb.ToString();
    }
    private static string DeleteAllDiagnosesQuery(uint patient) =>
    $"DELETE FROM Patients_Diagnoses WHERE patient_id = {patient};";
    #endregion
}
