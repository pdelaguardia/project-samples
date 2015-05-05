using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DocManager
/// </summary>
public static class DocManager
{
    static DocManager()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public static int GetDBId(string pPath)
    {
        if (pPath == "~\\")
            pPath = HttpContext.Current.Session["FilePath"].ToString();
        var data = GetSessionData();
        if (data.Valor == pPath)
            return data.Id;
        var BD = new DataClassesDataContext();
        var path = pPath.Split("\\".ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        var folder = GetRootFolder(BD).FileStorages.First(x => x.FileName == path[0]);
        while (path.Count > 1)
        {
            path.RemoveAt(0);
            folder = folder.FileStorages.First(x => x.FileName == path[0]);
        }
        return folder.Id;
    }
    private static CustomView.SimpleElementData GetSessionData()
    {
        var session = HttpContext.Current.Session;
        var data = session["FSPath"] as CustomView.SimpleElementData;
        if (data == null)
        {
            data = new CustomView.SimpleElementData { Id = 0, Valor = "!" };
            session["FSPath"] = data;
        }
        return data;
    }
    public static string GetWorkerPath(int pWorker)
    {
        var BD = new DataClassesDataContext();
        var worker = BD.Workers.First(x => x.ID == pWorker);
        var folder = BD.FileStorages.FirstOrDefault(x => x.RefID == worker.ID && x.RefType == "WorkerLegalData");
        if (folder != null)
        {
            CreateWorkerLegalFolders(BD, worker.ID, folder);
            return folder.GetPath();
        }
        var homeFolder = GetHomeWorkerFolder(BD, worker);
        folder = GetWorkerFolder(BD, worker, folder, homeFolder);
        return folder.GetPath();
    }
    public static string GetPatientPath(int pPerson)
    {
        var BD = new DataClassesDataContext();
        var person = BD.Persons.First(x => x.ID == pPerson);
        var folder = BD.FileStorages.FirstOrDefault(x => x.RefID == person.ID && x.RefType == "PatientLegalData");
        if (folder != null)
        {
            CreateLegalFolders(BD, person.ID, folder);
            return folder.GetPath();
        }
        var pacient = person.Pacients.First();
        var homeFolder = GetHomePatientsFolder(BD, pacient);
        folder = GetPatientFolder(BD, person, folder, homeFolder);
        return folder.GetPath();
    }
    public static string GetPosterPath(int pHome)
    {
        var BD = new DataClassesDataContext();
        var folder = BD.FileStorages.FirstOrDefault(x => x.RefID == pHome && x.RefType == "Poster");
        if (folder != null)
        {
            CheckPosterSubFolders(folder, BD);
            return folder.GetPath();
        }
        var home = BD.Enterprises.First(x => x.ID == pHome);
        folder = GetHomePatientsFolder(BD, home).FileStorage1;
        var psterFolder = new FileStorage
        {
            RefType = "Poster",
            RefID = pHome,
            FileName = "Poster",
            IsFolder = true,
            LastWriteTime = DateTime.Now,
            Name = "Poster",
            ParentId = folder.Id
        };
        BD.FileStorages.InsertOnSubmit(psterFolder);
        BD.SubmitChanges();
        CheckPosterSubFolders(psterFolder, BD);
        return psterFolder.GetPath();
    }
    public static string GetPoliciesPath(int pHome)
    {
        var BD = new DataClassesDataContext();
        var folder = BD.FileStorages.FirstOrDefault(x => x.RefID == pHome && x.RefType == "Policies");
        if (folder != null)
        {
            CheckSubFolders(folder, BD);
            return folder.GetPath();
        }
        var home = BD.Enterprises.First(x => x.ID == pHome);
        folder = GetHomePatientsFolder(BD, home).FileStorage1;
        var policiesFolder = new FileStorage
        {
            RefType = "Policies",
            RefID = pHome,
            FileName = "Policies",
            IsFolder = true,
            LastWriteTime = DateTime.Now,
            Name = "Policies",
            ParentId = folder.Id
        };
        BD.FileStorages.InsertOnSubmit(policiesFolder);
        BD.SubmitChanges();
        CheckSubFolders(policiesFolder, BD);
        return policiesFolder.GetPath();
    }
    public static string GetPermitsPath(int pHome)
    {
        var BD = new DataClassesDataContext();
        var folder = BD.FileStorages.FirstOrDefault(x => x.RefID == pHome && x.RefType == "Permits");
        if (folder != null)
        {
            CheckPermitsSubFolders(folder, BD);
            return folder.GetPath();
        }
        var home = BD.Enterprises.First(x => x.ID == pHome);
        folder = GetHomePatientsFolder(BD, home).FileStorage1;
        var permitsFolder = new FileStorage
        {
            RefType = "Permits",
            RefID = pHome,
            FileName = "Permits",
            IsFolder = true,
            LastWriteTime = DateTime.Now,
            Name = "Permits",
            ParentId = folder.Id
        };
        BD.FileStorages.InsertOnSubmit(permitsFolder);
        BD.SubmitChanges();
        CheckPermitsSubFolders(permitsFolder, BD);
        return permitsFolder.GetPath();
    }
    public static string GetCEMPCOOPPath(int pHome)
    {
        var BD = new DataClassesDataContext();
        var folder = BD.FileStorages.FirstOrDefault(x => x.RefID == pHome && x.RefType == "CEMPandCOOP");
        if (folder != null)
        {
            CheckCEMPCOOPSubFolders(folder, BD);
            return folder.GetPath();
        }
        var home = BD.Enterprises.First(x => x.ID == pHome);
        folder = GetHomePatientsFolder(BD, home).FileStorage1;
        var permitsFolder = new FileStorage
        {
            RefType = "CEMPandCOOP",
            RefID = pHome,
            FileName = "CEMP and COOP forms",
            IsFolder = true,
            LastWriteTime = DateTime.Now,
            Name = "CEMP and COOP forms",
            ParentId = folder.Id
        };
        BD.FileStorages.InsertOnSubmit(permitsFolder);
        BD.SubmitChanges();
        CheckCEMPCOOPSubFolders(permitsFolder, BD);
        return permitsFolder.GetPath();
    }
    public static string GetMenuPath(int pHome)
    {
        var BD = new DataClassesDataContext();
        var folder = BD.FileStorages.FirstOrDefault(x => x.RefID == pHome && x.RefType == "Menu");
        if (folder != null)
        {
            return folder.GetPath();
        }
        var home = BD.Enterprises.First(x => x.ID == pHome);
        folder = GetHomePatientsFolder(BD, home).FileStorage1;
        var menuFolder = new FileStorage
        {
            RefType = "Menu",
            RefID = pHome,
            FileName = "Menu",
            IsFolder = true,
            LastWriteTime = DateTime.Now,
            Name = "Menu",
            ParentId = folder.Id
        };
        BD.FileStorages.InsertOnSubmit(menuFolder);
        BD.SubmitChanges();
        return menuFolder.GetPath();
    }
    private static void CheckSubFolders(FileStorage folder, DataClassesDataContext BD)
    {
        CreateLegalFolder(BD, folder.Id, folder, "08", "Refund Policy Print");
        CreateLegalFolder(BD, folder.Id, folder, "10", "Facility Rules Print");
        CreateLegalFolder(BD, folder.Id, folder, "11", "Resident Bill of Rights Print");
        CreateLegalFolder(BD, folder.Id, folder, "14", "Admission, Ret. and Discharge Policies and Procedures");
        CreateLegalFolder(BD, folder.Id, folder, "17", "Smoking Policy-Rule");
        CreateLegalFolder(BD, folder.Id, folder, "22", "Social and Leisure Activity Policy");
        CreateLegalFolder(BD, folder.Id, folder, "23", "Central storage of Medication Policy");
        CreateLegalFolder(BD, folder.Id, folder, "24", "Storage of Valuables Policy");
        CreateLegalFolder(BD, folder.Id, folder, "26", "Emergency Evacuation and Transportation Policy");
        BD.SubmitChanges();
    }
    private static void CheckPosterSubFolders(FileStorage folder, DataClassesDataContext BD)
    {
        CreateLegalFolder(BD, folder.Id, folder, "01", "Federal Posters");
        CreateLegalFolder(BD, folder.Id, folder, "02", "Florida");
        CreateLegalFolder(BD, folder.Id, folder, "03", "Food Service");
        CreateLegalFolder(BD, folder.Id, folder, "04", "Other");
        BD.SubmitChanges();
    }
    private static void CheckPermitsSubFolders(FileStorage folder, DataClassesDataContext BD)
    {
        CreateLegalFolder(BD, folder.Id, folder, "01", "Certificate of Insurance");
        CreateLegalFolder(BD, folder.Id, folder, "02", "Surety Bond Certificate");
        CreateLegalFolder(BD, folder.Id, folder, "03", "DOEA Certificate");
        CreateLegalFolder(BD, folder.Id, folder, "04", "Property Diagram");
        CreateLegalFolder(BD, folder.Id, folder, "05", "AHCA Initial Survey Low");
        CreateLegalFolder(BD, folder.Id, folder, "06", "AHCA License");
        CreateLegalFolder(BD, folder.Id, folder, "07", "Fire Inspection");
        CreateLegalFolder(BD, folder.Id, folder, "08", "Fire Permit");
        CreateLegalFolder(BD, folder.Id, folder, "09", "Health Inspection");
        CreateLegalFolder(BD, folder.Id, folder, "10", "Certificate of Use");
        CreateLegalFolder(BD, folder.Id, folder, "11", "Local Biz Tax License");
        CreateLegalFolder(BD, folder.Id, folder, "12", "Annual Filling");
        BD.SubmitChanges();
    }
    private static void CheckCEMPCOOPSubFolders(FileStorage folder, DataClassesDataContext BD)
    {
        CreateLegalFolder(BD, folder.Id, folder, "01", "Water Agreement");
        CreateLegalFolder(BD, folder.Id, folder, "02", "Residential Health Care Facility Mutual Aid Agreement");
        CreateLegalFolder(BD, folder.Id, folder, "03", "Organizational Chart");
        CreateLegalFolder(BD, folder.Id, folder, "04", "Residential Health Care Facility Transportation Agreement");
        CreateLegalFolder(BD, folder.Id, folder, "05", "Standard Operating Procedures");
        CreateLegalFolder(BD, folder.Id, folder, DateTime.Now.Year.ToString(), DateTime.Now.Year.ToString() + " CEMP Plan Renewal Letter");
        CreateLegalFolder(BD, folder.Id, folder, DateTime.Now.Year.ToString(), DateTime.Now.Year.ToString() + " CEMP Plan Compliance Letter"); 
        BD.SubmitChanges();
    }
    private static FileStorage GetPatientFolder(DataClassesDataContext BD, Person person, FileStorage folder, FileStorage homeFolder)
    {
        folder = new FileStorage
        {
            FileName = person.Name,
            IsFolder = true,
            LastWriteTime = DateTime.Now,
            Name = person.Name,
            ParentId = homeFolder.Id,
            RefID = person.ID,
            RefType = "Patient"
        };
        BD.FileStorages.InsertOnSubmit(folder);
        BD.SubmitChanges();
        folder = new FileStorage
        {
            RefType = "PatientLegalData",
            FileName = "Resident Documents",
            IsFolder = true,
            LastWriteTime = DateTime.Now,
            Name = "Resident Documents",
            ParentId = folder.Id,
            RefID = person.ID
        };
        BD.FileStorages.InsertOnSubmit(folder);
        BD.SubmitChanges();
        CreateLegalFolders(BD, person.ID, folder);
        BD.SubmitChanges();
        return folder;
    }
    private static FileStorage GetWorkerFolder(DataClassesDataContext BD, Worker worker, FileStorage folder, FileStorage homeFolder)
    {
        folder = new FileStorage
        {
            FileName = worker.Name,
            IsFolder = true,
            LastWriteTime = DateTime.Now,
            Name = worker.Name,
            ParentId = homeFolder.Id,
            RefID = worker.ID,
            RefType = "Worker"
        };
        BD.FileStorages.InsertOnSubmit(folder);
        BD.SubmitChanges();
        folder = new FileStorage
        {
            RefType = "WorkerLegalData",
            FileName = "Worker Documents",
            IsFolder = true,
            LastWriteTime = DateTime.Now,
            Name = "Worker Documents",
            ParentId = folder.Id,
            RefID = worker.ID
        };
        BD.FileStorages.InsertOnSubmit(folder);
        BD.SubmitChanges();
        CreateWorkerLegalFolders(BD, worker.ID, folder);
        BD.SubmitChanges();
        return folder;
    }
    private static void CreateWorkerLegalFolders(DataClassesDataContext BD, int worker, FileStorage folder)
    {
        CreateLegalFolder(BD, worker, folder, "01", "Picture");
        CreateLegalFolder(BD, worker, folder, "02", "Application for Employment");
        CreateLegalFolder(BD, worker, folder, "03", "Background Check documentation");
        CreateLegalFolder(BD, worker, folder, "04", "Employment Offer Letter");
        CreateLegalFolder(BD, worker, folder, "05", "Current First Aid Certification");
        CreateLegalFolder(BD, worker, folder, "06", "Current CPR Certification");
        CreateLegalFolder(BD, worker, folder, "07", "DMV License and record (if applicable)");
        CreateLegalFolder(BD, worker, folder, "08", "I-9");
        CreateLegalFolder(BD, worker, folder, "09", "W-4");
        CreateLegalFolder(BD, worker, folder, "10", "Employee's Withholding Allowance Certificate");
        CreateLegalFolder(BD, worker, folder, "11", "Personnel Action Request");
        CreateLegalFolder(BD, worker, folder, "12", "Signed Job Description");
        CreateLegalFolder(BD, worker, folder, "13", "Employee Handbook Acknowledgement");
        CreateLegalFolder(BD, worker, folder, "14", "Submission of Physician's Statement"); 
    }
    private static void CreateLegalFolders(DataClassesDataContext BD, int person, FileStorage folder)
    {
        CreateLegalFolder(BD, person, folder, "01", "IDs");
        CreateLegalFolder(BD, person, folder, "02", "Picture");
        CreateLegalFolder(BD, person, folder, "03", "Facesheet");
        CreateLegalFolder(BD, person, folder, "04", "Emergency contact list");
        CreateLegalFolder(BD, person, folder, "05", "Authorization to Assist with Health Care Services");
        CreateLegalFolder(BD, person, folder, "06", "ALF Agreement");
        CreateLegalFolder(BD, person, folder, "07", "AHCA 1823 Assessment Form");
        //CreateLegalFolder(BD, person, folder, "08", "Refund Policy Print");
        CreateLegalFolder(BD, person, folder, "09", "Informed Constent for Medication Assistance");
        //CreateLegalFolder(BD, person, folder, "10", "Facility Rules Print");
        //CreateLegalFolder(BD, person, folder, "11", "Resident Bill of Rights Print");
        CreateLegalFolder(BD, person, folder, "12", "Half Bed Rails Consent");
        CreateLegalFolder(BD, person, folder, "13", "Consent to arrange medical-dental care and treatments");
        //CreateLegalFolder(BD, person, folder, "14", "Admission, Ret. and Discharge Policies and Procedures");
        CreateLegalFolder(BD, person, folder, "15", "Medical Observation Sheet-Template");
        CreateLegalFolder(BD, person, folder, "16", "Admission Property Inventory Sheet");
        //CreateLegalFolder(BD, person, folder, "17", "Smoking Policy-Rule");
        CreateLegalFolder(BD, person, folder, "18", "Exit Permission-Consent");
        CreateLegalFolder(BD, person, folder, "19", "List of Authorized Persons to Remove Resident");
        CreateLegalFolder(BD, person, folder, "20", "sign Out Release of Liability");
        CreateLegalFolder(BD, person, folder, "21", "Refusal of Therapeutic Diet");
        //CreateLegalFolder(BD, person, folder, "22", "Social and Leisure Activity Policy");
        //CreateLegalFolder(BD, person, folder, "23", "Central storage of Medication Policy");
        //CreateLegalFolder(BD, person, folder, "24", "Storage of Valuables Policy");
        CreateLegalFolder(BD, person, folder, "25", "Consent for Appointments and Transportation");
        //CreateLegalFolder(BD, person, folder, "26", "Emergency Evacuation and Transportation Policy");
        CreateLegalFolder(BD, person, folder, "27", "Transportation of Residents to Medical Appts");
        CreateLegalFolder(BD, person, folder, "28", "Liability Release Form to Providing Transportation");
        CreateLegalFolder(BD, person, folder, "29", "Discharge Release Form");
        CreateLegalFolder(BD, person, folder, "DNRO", "Do not Resucitate Order");
        CreateLegalFolder(BD, person, folder, "POA", "Power of Attorney");
        CreateLegalFolder(BD, person, folder, "MCO", "MCODocuments");
        CreateLegalFolder(BD, person, folder, "MED", "Medication");
        CreateLegalFolder(BD, person, folder, "HDP", "Hospital discharge papers");
        BD.SubmitChanges();
    }
    private static void CreateLegalFolder(DataClassesDataContext BD, int person, FileStorage folder, string num, string name)
    {
        if (folder.FileStorages.Any(x => x.RefType == "Legal: " + num))
            return;
        var doc = new FileStorage
        {
            FileName = num + " " + name,
            RefID = person,
            IsFolder = true,
            LastWriteTime = DateTime.Now,
            Name = name,
            ParentId = folder.Id,
            RefType = "Legal: " + num
        };
        BD.FileStorages.InsertOnSubmit(doc);
    }
    private static FileStorage GetHomePatientsFolder(DataClassesDataContext BD, Pacient pacient)
    {
        var home = pacient.Enterprise_Pacients.First().Enterprise;
        return GetHomePatientsFolder(BD, home);
    }
    private static FileStorage GetHomeWorkerFolder(DataClassesDataContext BD, Worker worker)
    {
        var home = worker.Enterprise_Workers.First().Enterprise;
        return GetHomePatientsFolder(BD, home);
    }
    private static FileStorage GetHomePatientsFolder(DataClassesDataContext BD, Enterprise home)
    {
        var homeFolder = BD.FileStorages.FirstOrDefault(x => x.RefID == home.ID && x.RefType == "PatientsFolder");
        if (homeFolder == null)
        {
            var root = GetRootFolder(BD);
            homeFolder = new FileStorage
            {
                RefID = home.ID,
                RefType = "Home",
                IsFolder = true,
                LastWriteTime = DateTime.Now,
                Name = home.Name,
                FileName = home.Name,
                ParentId = root.Id
            };
            BD.FileStorages.InsertOnSubmit(homeFolder);
            BD.SubmitChanges();
            homeFolder = new FileStorage
            {
                RefID = home.ID,
                RefType = "PatientsFolder",
                IsFolder = true,
                LastWriteTime = DateTime.Now,
                Name = "Patients",
                FileName = "Patients",
                ParentId = homeFolder.Id
            };
            BD.FileStorages.InsertOnSubmit(homeFolder);
            BD.SubmitChanges();
        }
        return homeFolder;
    }
    private static FileStorage GetRootFolder(DataClassesDataContext BD)
    {
        var root = BD.FileStorages.FirstOrDefault(x => x.RefType == "Root");
        if (root != null)
            return root;
        root = new FileStorage { FileName = "Root", RefType = "Root", RefID = 0, Name = "Root", LastWriteTime = DateTime.Now, IsFolder = true };
        BD.FileStorages.InsertOnSubmit(root);
        BD.SubmitChanges();
        return root;
    }
    private static string GetPath(this FileStorage folder)
    {
        var path = folder.FileName;
        for (var f = folder.FileStorage1; f != null && f.RefType != "Root"; f = f.FileStorage1)
            path = f.FileName + "\\" + path;
        return path;
    }
}