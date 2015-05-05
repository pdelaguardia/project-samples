using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using DevExpress.Web.ASPxScheduler;
using System.Net.Mail;
using System.Net;

/// <summary>
/// Summary description for CustomView
/// </summary>
public class CustomView
{
    public class SimpleElementData
    {
        public int Id { get; set; }
        public string Valor { get; set; }
    }
    public class TrainingTracker
    {
        public string Worker { get; set; }
        public string Training { get; set; }
        public static List<TrainingTracker> GetFromSession(string pSession)
        {
            var session = HttpContext.Current.Session;
            var lista = session[pSession] as List<TrainingTracker>;
            if (lista == null)
            {
                lista = new List<TrainingTracker>();
                session[pSession] = lista;
            }
            return lista;
        }
        public static List<TrainingTracker> Get(int pHome = 0)
        {
            var lista = new List<TrainingTracker>();
            if (pHome == 0)
                return lista;
            var BD = new DataClassesDataContext();
            foreach (var training in BD.TrainingRequirements)
            {
                var workers = GetWorkersOfTraining(BD, training, pHome);
                foreach (var worker in workers)
                {
                    lista.Add(new TrainingTracker { Training = training.Name, Worker = worker.Name });
                }
            }
            return lista;
        }
        public static void Set(string pSession, int pHome = 0)
        {
            var lista = GetFromSession(pSession);
            lista.Clear();
            lista.AddRange(Get(pHome));
        }
        private static IQueryable<Worker> GetWorkersOfTraining(DataClassesDataContext BD, TrainingRequirement training, int pHome)
        {
            var exceptions = training.TrainingExceptions.Select(x => x.IDCategory).ToArray();
            var home = BD.Enterprises.First(x => x.ID == pHome);
            var workers = from w in BD.Workers
                          where w.Enterprise_Workers.Any(x => x.IDEnterprise == pHome)
                          select w;
            if (training.Name == "Alzheimer's Level I")
            {
                var start = new DateTime(1988, 4, 20);
                var end = new DateTime(2003, 7, 1);
                workers = from w in workers
                          where !w.EarnedCategories.Any(x => x.WorkerCategory.Name == "CORE Trained" && x.DateEarned >= start && x.DateEarned <= end)
                          select w;
            }
            else
            {
                workers = from w in workers
                          where !w.EarnedCategories.Any(x => exceptions.Contains(x.IDCategory))
                          select w;
            }
            if (training.Name.StartsWith("LMH ") && !home.EnterpriseLicences.Any(x => x.Name.StartsWith("LMH")))
            {
                workers = from w in BD.Workers
                          where w.ID < 0
                          select w;
            }
            if (training.MonthCycle == null)
                workers = workers.Where(x => !x.Trainings.Any(t => t.IDTraining == training.ID));
            else
            {
                var date = DateTime.Now.AddMonths(-training.MonthCycle.Value);
                workers = workers.Where(x => !x.Trainings.Any(t => t.IDTraining == training.ID && t.EndDate >= date));
            }
            return workers;
        }
    }
    public class AppointmentLabel
    {
        public int ID { get; set; }
        public string Caption { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public static List<AppointmentLabel> GetFromSession(string pSession)
        {
            var session = HttpContext.Current.Session;
            var lista = session[pSession] as List<AppointmentLabel>;
            if (lista == null)
            {
                lista = new List<AppointmentLabel>();
                session[pSession] = lista;
            }
            return lista;
        }
        public static List<AppointmentLabel> Get()
        {
            var BD = new DataClassesDataContext();
            var lista = new List<AppointmentLabel>();
            foreach (var item in BD.TrainingRequirements)
                lista.Add(new AppointmentLabel { Caption = item.Name.MakeShorter(), Color = GetColor(item), Name = item.Name });
            return lista;
        }
        public static void Set(string pSession)
        {
            var lista = GetFromSession(pSession);
            lista.Clear();
            lista.AddRange(Get());
        }
        private DevExpress.XtraScheduler.AppointmentLabelBase GetLabel(TrainingRequirement item)
        {
            var label = new DevExpress.XtraScheduler.AppointmentLabelBase();
            label.Color = GetColor(item);
            label.DisplayName = item.Name;
            label.MenuCaption = item.Name.MakeShorter();
            return label;
        }
        private static Color GetColor(TrainingRequirement item)
        {
            switch (item.ID)
            {
                case 1:
                    return Color.LightPink;
                case 2:
                case 3:
                case 4:
                    return Color.White;
                case 5:
                    return Color.LightGreen;
                case 6:
                    return Color.White;
                case 7:
                    return Color.LightGoldenrodYellow;
                case 8:
                    return Color.LightPink;
                case 9:
                    return Color.LightPink;
                case 10:
                    return Color.LightSalmon;
                case 11:
                    return Color.LightSalmon;
                case 12:
                    return Color.LightGreen;
                case 13:
                    return Color.LightGray;
                case 14:
                    return Color.LightGray;
                case 15:
                    return Color.LightGray;
                case 16:
                    return Color.LightSeaGreen;
                case 17:
                    return Color.LightBlue;
                case 18:
                    return Color.LightCyan;
                case 19:
                    return Color.LightCyan;
                default:
                    return Color.White;
            }
        }
    }
}

public class ScheduleCalc
{
    public int Id { get; set; }
    public int? IdApp { get; set; }
    public string Subject { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Location { get; set; }

    public static List<ScheduleCalc> GetFromScheduler(ASPxScheduler sched, DateTime pInicio, DateTime pFin)
    {
        var inicio = pInicio.Date;
        sched.LimitInterval.AllDay = true;
        var viewType = sched.ActiveViewType;
        sched.ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Month;
        sched.LimitInterval.End = pFin;
        sched.LimitInterval.Start = pInicio;
        sched.DataBind();
        var lista = new List<ScheduleCalc>(sched.Storage.Appointments.Count);
        while (inicio < pFin)
        {
            sched.LimitInterval.AllDay = true;
            sched.LimitInterval.End = inicio.AddMonths(1);
            sched.LimitInterval.Start = inicio;
            sched.DataBind();
            var apps = sched.ActiveView.GetAppointments();
            for (int i = 0; i < apps.Count; i++)
            {

                try
                {
                    var app = apps[i];
                    var act = new ScheduleCalc { IdApp = (int)app.LabelId, Subject = app.Subject, End = app.End, Start = app.Start, Location = app.Location };
                    act.Id = app.RecurrencePattern == null ? (int)app.Id : (int)app.RecurrencePattern.Id;
                    lista.Add(act);
                }
                catch (Exception)
                {


                }
            }
            inicio = inicio.AddMonths(1);
        }
        return lista;
    }
}

public class NotificationArea
{
    public string Texto { get; set; }
    public string URL { get; set; }
    public bool Importante { get; set; }
    public string Grupo { get; set; }
    public string Module { get; set; }

    public static List<NotificationArea> GetFromSession(string pSession)
    {
        var lista = HttpContext.Current.Session[pSession] as List<NotificationArea>;
        if (lista == null)
        {
            lista = new List<NotificationArea>();
            HttpContext.Current.Session[pSession] = lista;
        }
        return lista;
    }
    public static List<NotificationArea> GetNots(ASPxScheduler schedule = null, int pUsuario = 0)
    {
        int ID = 0;
        if (int.TryParse(HttpContext.Current.Session["EnterpriseID"].ToString(), out ID))
        {
            var BD = new DataClassesDataContext();
            var list = new List<NotificationArea>(8);
            MissingDNRO(BD, list, ID);
            MissingPOA(BD, list, ID);
            Permits(BD, list, ID);
            CempComplianceLetter(BD, list, ID);
            Menu(BD, list, ID);
            MissingBackground(BD, list, ID);
            MissingMedClear(BD, list, ID);
            TodayActivities(BD, list, schedule);
            BD.Notifications.DeleteAllOnSubmit(BD.Notifications.Where(x=>x.IDEnterprise == int.Parse(HttpContext.Current.Session["EnterpriseID"].ToString())));
            BD.SubmitChanges();
            if (!BD.Notifications.Any())
            {
                foreach (var item in list)
                {
                    Notification not = new Notification { IDEnterprise = int.Parse(HttpContext.Current.Session["EnterpriseID"].ToString()), Item = item.Grupo, Message = item.Texto, Module = item.Module };
                    BD.Notifications.InsertOnSubmit(not);
                    BD.SubmitChanges();
                }
            }
            return list;
        }
        else
        {
            var list = new List<NotificationArea>(8);
            return list;
        }
    }
    private static void TodayActivities(DataClassesDataContext BD, List<NotificationArea> list, ASPxScheduler scheduler)
    {
        if (scheduler != null)
        {
            List<ScheduleCalc> listActivities = ScheduleCalc.GetFromScheduler(scheduler, DateTime.Now, DateTime.Now);
            var notification = new NotificationArea();
            foreach (var item in listActivities)
            {
                var work = from u in BD.Workers
                           where u.ID == int.Parse(item.Location)
                           select u;

                string workername = work.First().Name + " " + work.First().LastName;

                var messages = from u in BD.WorkerMails
                               where u.IDWorker == work.First().ID && u.SendDate.Day == DateTime.Now.Day && u.SendDate.Month == DateTime.Now.Month && u.SendDate.Year == DateTime.Now.Year
                               select u;
                if (!messages.Any())
                {
                    try
                    {
                        SmtpClient client = new SmtpClient("smtp.gmail.com");
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential("ehealthsystemnotifications", "assisted");
                        MailAddress from = new MailAddress("Admin@eHealth.com", "Notifications", System.Text.Encoding.UTF8);
                        MailAddress to = new MailAddress(work.First().eMail);
                        MailMessage message = new MailMessage(from, to);
                        message.Body = workername + ": " + item.Subject + " Case Manager visit pending";
                        message.BodyEncoding = System.Text.Encoding.UTF8;
                        message.Subject = "Case Manager visit pending";
                        message.SubjectEncoding = System.Text.Encoding.UTF8;
                        client.Send(message);
                        WorkerMail wm = new WorkerMail { IDWorker = work.First().ID, SendDate = DateTime.Now, Text = message.Body };
                        BD.WorkerMails.InsertOnSubmit(wm);
                        BD.SubmitChanges();
                    }
                    catch (Exception ce)
                    {
                        ce.Message.CreateLog(ce);
                    }
                }
                
                if (HttpContext.Current.Session["Culture"] == "en")
                {
                    notification = new NotificationArea
                    {
                        Importante = true,
                        Grupo = workername,
                        Module = "Health Plans",
                        Texto = item.Subject + " Case Manager visit pending",
                        URL = "~/HealthPlan/CaseManagerScheduler.aspx"
                    };
                    list.Add(notification);
                }
                else
                {
                    notification = new NotificationArea
                    {
                        Importante = true,
                        Grupo = workername,
                        Module = "Planes de Salud",
                        Texto = item.Subject + " visita del CM pendiente",
                        URL = "~/HealthPlan/CaseManagerScheduler.aspx"
                    };
                    list.Add(notification);
                }
            }
        }
    }
    private static void Permits(DataClassesDataContext BD, List<NotificationArea> list, int EntID)
    {
        var notification = new NotificationArea();
        var permits = from check in BD.PermitsCheckLists
                      from perm in BD.Permits
                      where check.IDPermit == perm.ID && check.IDEnterprise == EntID
                      select new { FacilityPermit = perm.Name, Date = check.LastRenewalDate };
        foreach (var item in permits)
        {
            if (HttpContext.Current.Session["Culture"] == "en")
            {
                TimeSpan diff = item.Date.Value - DateTime.Now;
                if (diff.Days < 30 && diff.Days > 0)
                {
                    notification = new NotificationArea
                    {
                        Importante = true,
                        Grupo = item.FacilityPermit,
                        Module = "Facility Manager",
                        Texto = "Permit Renewal (" + diff.Days.ToString() + " days to renew)",
                        URL = "~/FacilityManager/PermitsCheckList.aspx"
                    };
                    list.Add(notification);
                }
                if (diff.Days <= 0)
                {
                    notification = new NotificationArea
                    {
                        Importante = true,
                        Grupo = item.FacilityPermit,
                        Module = "Facility Manager",
                        Texto = "Permit Renewal (Past Due)",
                        URL = "~/FacilityManager/PermitsCheckList.aspx"
                    };
                    list.Add(notification);
                }
            }
            else
            {
                TimeSpan diff = item.Date.Value - DateTime.Now;
                if (diff.Days < 30 && diff.Days > 0)
                {
                    notification = new NotificationArea
                    {
                        Importante = true,
                        Grupo = item.FacilityPermit,
                        Module = "Entidad",
                        Texto = "Renovación de Permisos (" + diff.Days.ToString() + " días restantes)",
                        URL = "~/FacilityManager/PermitsCheckList.aspx"
                    };
                    list.Add(notification);
                }
                if (diff.Days <= 0)
                {
                    notification = new NotificationArea
                    {
                        Importante = true,
                        Grupo = item.FacilityPermit,
                        Module = "Entidad",
                        Texto = "Renovación de Permisos (Vencida)",
                        URL = "~/FacilityManager/PermitsCheckList.aspx"
                    };
                    list.Add(notification);
                }
            }
        }
    }
    private static void CempComplianceLetter(DataClassesDataContext BD, List<NotificationArea> list, int EntID)
    {
        var db = new DataClassesDataContext();
        var notification = new NotificationArea();

        var cempcoop = from u in db.FileStorages
                   where u.FileName == "CEMP and COOP forms" && u.RefType == "CEMPandCOOP" && u.RefID == (int)HttpContext.Current.Session["EnterpriseID"]
                   select u.Id;
        if (cempcoop.Any())
        {
            int cempcoopID = cempcoop.First();
            var ComplianceLetterFolder = from d in db.FileStorages
                            where d.ParentId == cempcoopID && d.Name.Contains("CEMP Plan Compliance Letter")
                            select d;
            foreach (var item in ComplianceLetterFolder)
            {
                int ComplianceLetterFolderID = item.Id;
                var LetterFiles = from a in db.FileStorages
                                  where a.ParentId == ComplianceLetterFolderID 
                                  select a;
                if (!LetterFiles.Any())
                {
                    string year = item.Name.Substring(0, 4);
                    if (HttpContext.Current.Session["Culture"] == "en")
                    {
                        try
                        {
                            if (DateTime.Now >= new DateTime(int.Parse(year), 11, 20) && DateTime.Now <= new DateTime(int.Parse(year), 11, 30))
                            {
                                TimeSpan diff = new DateTime(DateTime.Now.Year, 11, 30) - DateTime.Now;
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "CEMP Plan & COOP Forms",
                                    Module = "Facility Manager",
                                    Texto = year + " Compliance Letter Missing (" + diff.Days.ToString() + " days to upload)",
                                    URL = "~/FacilityManager/CEMPandCOOP.aspx"
                                };
                                list.Add(notification);
                            }
                            else if (DateTime.Now >= new DateTime(int.Parse(year), 12, 1))
                            {
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "CEMP Plan & COOP Forms",
                                    Module = "Facility Manager",
                                    Texto = year + " Compliance Letter Missing (Past Due)",
                                    URL = "~/FacilityManager/CEMPandCOOP.aspx"
                                };
                                list.Add(notification);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            if (DateTime.Now >= new DateTime(DateTime.Now.Year, 11, 20) && DateTime.Now <= new DateTime(DateTime.Now.Year, 11, 30))
                            {
                                TimeSpan diff = new DateTime(DateTime.Now.Year, 11, 30) - DateTime.Now;
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Planes CEMP & Formularios COOP",
                                    Module = "Entidad",
                                    Texto = year + " Carta de aprobación (" + diff.Days.ToString() + " días restantes)",
                                    URL = "~/FacilityManager/CEMPandCOOP.aspx"
                                };
                                list.Add(notification);
                            }
                            else
                            {
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Planes CEMP & Formularios COOP",
                                    Module = "Entidad",
                                    Texto = year + " Carta de aprobación (Vencida)",
                                    URL = "~/FacilityManager/CEMPandCOOP.aspx"
                                };
                                list.Add(notification);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }
    }
    private static void Menu(DataClassesDataContext BD, List<NotificationArea> list, int EntID)
    {
        var notification = new NotificationArea();
        var menu = from men in BD.MenuManagements
                   where men.IDEnterprise == EntID
                   select men;
        foreach (var item in menu)
        {
            if (HttpContext.Current.Session["Culture"] == "en")
            {
                TimeSpan diff = item.NextRenewalDate.Value - DateTime.Now;
                if (diff.Days < 30 && diff.Days > 0)
                {
                    notification = new NotificationArea
                    {
                        Importante = true,
                        Grupo = "Menu",
                        Module = "Facility Manager",
                        Texto = "Menu Renewal (" + diff.Days.ToString() + " days to renew)",
                        URL = "~/FacilityManager/PermitsCheckList.aspx"
                    };
                    list.Add(notification);
                }
                if (diff.Days <= 0)
                {
                    notification = new NotificationArea
                    {
                        Importante = true,
                        Grupo = "Menu",
                        Module = "Facility Manager",
                        Texto = "Menu Renewal (Past Due)",
                        URL = "~/FacilityManager/PermitsCheckList.aspx"
                    };
                    list.Add(notification);
                }
            }
            else
            {
                TimeSpan diff = item.NextRenewalDate.Value - DateTime.Now;
                if (diff.Days < 30 && diff.Days > 0)
                {
                    notification = new NotificationArea
                    {
                        Importante = true,
                        Grupo = "Menú",
                        Module = "Entidad",
                        Texto = "Renovación del Menú (" + diff.Days.ToString() + " días restantes)",
                        URL = "~/FacilityManager/PermitsCheckList.aspx"
                    };
                    list.Add(notification);
                }
                if (diff.Days <= 0)
                {
                    notification = new NotificationArea
                    {
                        Importante = true,
                        Grupo = "Menú",
                        Module = "Entidad",
                        Texto = "Renovación del Menú (Vencida)",
                        URL = "~/FacilityManager/PermitsCheckList.aspx"
                    };
                    list.Add(notification);
                }
            }
        }
    }
    private static void MissingPOA(DataClassesDataContext db, List<NotificationArea> list, int EntID)
    {
        var notification = new NotificationArea();
        var pacients = from p in db.Pacients
                       from ep in db.Enterprise_Pacients
                       where p.ID == ep.IDPacient && ep.IDEnterprise == EntID
                       select p;

        foreach (var pacient in pacients)
        {
            var dnro = from u in db.FileStorages
                       where u.FileName == "POA Power of Attorney" && u.RefType == "Legal: POA" && u.RefID == pacient.Person.ID
                       select u.Id;
            if (dnro.Any())
            {
                int dnroID = dnro.First();
                var dnroFiles = from d in db.FileStorages
                                where d.ParentId == dnroID
                                select d;
                if (!dnroFiles.Any() && pacient.IDPOA.HasValue)
                {
                    if (HttpContext.Current.Session["Culture"] == "en")
                    {
                        try
                        {
                            var legaldoc = db.LegalDocsDateControls.First(x => x.IDPerson == pacient.Person.ID && x.IDLegalDocument == 30);
                            int days = ((int)legaldoc.LegalDoc.DaysToUpload - (int)DateTime.Now.Subtract(legaldoc.RegisteredDate.Value.Date).TotalDays) >= 0 ? ((int)legaldoc.LegalDoc.DaysToUpload - (int)DateTime.Now.Subtract(legaldoc.RegisteredDate.Value.Date).TotalDays) : 0;
                            if (days > 0)
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Resident: " + pacient.Person.Name,
                                    Module = "Resident Manager",
                                    Texto = "POA Missing (" + days.ToString() + " days to upload)",
                                    URL = "~/ResidentManager/PatientDocumentation.aspx?ID=" + pacient.Person.ID.ToString()
                                };
                            else
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Resident: " + pacient.Person.Name,
                                    Module = "Resident Manager",
                                    Texto = "POA Missing (Past Due)",
                                    URL = "~/ResidentManager/PatientDocumentation.aspx?ID=" + pacient.Person.ID.ToString()
                                };
                            list.Add(notification);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            var legaldoc = db.LegalDocsDateControls.First(x => x.IDPerson == pacient.Person.ID && x.IDLegalDocument == 30);
                            int days = ((int)legaldoc.LegalDoc.DaysToUpload - (int)DateTime.Now.Subtract(legaldoc.RegisteredDate.Value.Date).TotalDays) >= 0 ? ((int)legaldoc.LegalDoc.DaysToUpload - (int)DateTime.Now.Subtract(legaldoc.RegisteredDate.Value.Date).TotalDays) : 0;
                            if (days > 0)
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Residente: " + pacient.Person.Name,
                                    Module = "Residentes",
                                    Texto = "Falta POA (" + days.ToString() + " días restantes)",
                                    URL = "~/ResidentManager/PatientDocumentation.aspx?ID=" + pacient.Person.ID.ToString()
                                };
                            else
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Residente: " + pacient.Person.Name,
                                    Module = "Residentes",
                                    Texto = "Falta POA (Vencida)",
                                    URL = "~/ResidentManager/PatientDocumentation.aspx?ID=" + pacient.Person.ID.ToString()
                                };
                            list.Add(notification);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }
    }
    private static void MissingDNRO(DataClassesDataContext db, List<NotificationArea> list, int EntID)
    {
        var notification = new NotificationArea();
        var pacients = from p in db.Pacients
                       from ep in db.Enterprise_Pacients
                       where p.ID == ep.IDPacient && ep.IDEnterprise == EntID
                       select p;

        foreach (var pacient in pacients)
        {
            var dnro = from u in db.FileStorages
                       where u.FileName == "DNRO Do not Resucitate Order" && u.RefType == "Legal: DNRO" && u.RefID == pacient.Person.ID
                       select u.Id;
            if (dnro.Any())
            {
                int dnroID = dnro.First();
                var dnroFiles = from d in db.FileStorages
                                where d.ParentId == dnroID
                                select d;
                bool cpr = pacient.CPRPreference.HasValue ? pacient.CPRPreference.Value : false;
                if (!dnroFiles.Any() && !cpr)
                {
                    if (HttpContext.Current.Session["Culture"] == "en")
                    {
                        try
                        {
                            var legaldoc = db.LegalDocsDateControls.First(x => x.IDPerson == pacient.Person.ID && x.IDLegalDocument == 29);
                            int days = ((int)legaldoc.LegalDoc.DaysToUpload - (int)DateTime.Now.Subtract(legaldoc.RegisteredDate.Value.Date).TotalDays) >= 0 ? ((int)legaldoc.LegalDoc.DaysToUpload - (int)DateTime.Now.Subtract(legaldoc.RegisteredDate.Value.Date).TotalDays) : 0;
                            if (days > 0)
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Resident: " + pacient.Person.Name,
                                    Module = "Resident Manager",
                                    Texto = "DNRO Missing (" + days.ToString() + " days to upload)",
                                    URL = "~/ResidentManager/PatientDocumentation.aspx?ID=" + pacient.Person.ID.ToString()
                                };
                            else
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Resident: " + pacient.Person.Name,
                                    Module = "Resident Manager",
                                    Texto = "DNRO Missing (Past Due)",
                                    URL = "~/ResidentManager/PatientDocumentation.aspx?ID=" + pacient.Person.ID.ToString()
                                };
                            list.Add(notification);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            var legaldoc = db.LegalDocsDateControls.First(x => x.IDPerson == pacient.Person.ID && x.IDLegalDocument == 29);
                            int days = ((int)legaldoc.LegalDoc.DaysToUpload - (int)DateTime.Now.Subtract(legaldoc.RegisteredDate.Value.Date).TotalDays) >= 0 ? ((int)legaldoc.LegalDoc.DaysToUpload - (int)DateTime.Now.Subtract(legaldoc.RegisteredDate.Value.Date).TotalDays) : 0;
                            if (days > 0)
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Residente: " + pacient.Person.Name,
                                    Module = "Residentes",
                                    Texto = "Falta DNRO (" + days.ToString() + " días restantes)",
                                    URL = "~/ResidentManager/PatientDocumentation.aspx?ID=" + pacient.Person.ID.ToString()
                                };
                            else
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Residente: " + pacient.Person.Name,
                                    Module = "Residentes",
                                    Texto = "Falta DNRO (Vencida)",
                                    URL = "~/ResidentManager/PatientDocumentation.aspx?ID=" + pacient.Person.ID.ToString()
                                };
                            list.Add(notification);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }
    }
    private static void MissingMedClear(DataClassesDataContext db, List<NotificationArea> list, int EntID)
    {
        var notification = new NotificationArea();
        var workers = from p in db.Workers
                      from ep in db.Enterprise_Workers
                      where p.ID == ep.IDWorker && ep.IDEnterprise == EntID
                      select p;

        foreach (var worker in workers)
        {
            var medclear = from u in db.FileStorages
                           where u.FileName == "14 Submission of Physician's Statement" && u.RefType == "Legal: 14" && u.RefID == worker.ID
                           select u.Id;
            if (medclear.Any())
            {
                int medclearID = medclear.First();
                var medclearFiles = from d in db.FileStorages
                                    where d.ParentId == medclearID
                                    select d;
                if (!medclearFiles.Any())
                {
                    if (HttpContext.Current.Session["Culture"] == "en")
                    {
                        try
                        {
                            var legaldoc = db.WorkerDocTrackers.First(x => x.IDWorker == worker.ID && x.IDDocumentation == 2);
                            TimeSpan diff = legaldoc.NextRenewalDate.Value - DateTime.Now;
                            if (diff.Days < 30 && diff.Days > 0)
                            {
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Worker: " + worker.Name,
                                    Module = "Staff Manager",
                                    Texto = "MedClear Renewal (" + diff.Days.ToString() + " days to renew)",
                                    URL = "~/StaffManager/WorkerDocumentation.aspx?ID=" + worker.ID.ToString()
                                };
                                list.Add(notification);
                            }
                            if (diff.Days <= 0)
                            {
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Worker: " + worker.Name,
                                    Module = "Staff Manager",
                                    Texto = "MedClear Renewal (Past Due)",
                                    URL = "~/StaffManager/WorkerDocumentation.aspx?ID=" + worker.ID.ToString()
                                };
                                list.Add(notification);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            var legaldoc = db.WorkerDocTrackers.First(x => x.IDWorker == worker.ID && x.IDDocumentation == 2);
                            TimeSpan diff = legaldoc.NextRenewalDate.Value - DateTime.Now;
                            if (diff.Days < 30 && diff.Days > 0)
                            {
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Trabajador: " + worker.Name,
                                    Module = "Personal",
                                    Texto = "Renovación MedClear (" + diff.Days.ToString() + " días restantes)",
                                    URL = "~/StaffManager/WorkerDocumentation.aspx?ID=" + worker.ID.ToString()
                                };
                                list.Add(notification);
                            }
                            if (diff.Days <= 0)
                            {
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Trabajador: " + worker.Name,
                                    Module = "Personal",
                                    Texto = "Renovación MedClear (Vencida)",
                                    URL = "~/StaffManager/WorkerDocumentation.aspx?ID=" + worker.ID.ToString()
                                };
                                list.Add(notification);
                            }
                            
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }
    }
    private static void MissingBackground(DataClassesDataContext db, List<NotificationArea> list, int EntID)
    {
        var notification = new NotificationArea();
        var workers = from p in db.Workers
                      from ep in db.Enterprise_Workers
                      where p.ID == ep.IDWorker && ep.IDEnterprise == EntID
                      select p;

        foreach (var worker in workers)
        {
            var medclear = from u in db.FileStorages
                           where u.FileName == "03 Background Check documentation" && u.RefType == "Legal: 03" && u.RefID == worker.ID
                           select u.Id;
            if (medclear.Any())
            {
                int medclearID = medclear.First();
                var medclearFiles = from d in db.FileStorages
                                    where d.ParentId == medclearID
                                    select d;
                if (!medclearFiles.Any())
                {
                    if (HttpContext.Current.Session["Culture"] == "en")
                    {
                        try
                        {
                            var legaldoc = db.WorkerDocTrackers.First(x => x.IDWorker == worker.ID && x.IDDocumentation == 1);
                            TimeSpan diff = legaldoc.NextRenewalDate.Value - DateTime.Now;
                            if (diff.Days < 30 && diff.Days > 0)
                            {
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Worker: " + worker.Name,
                                    Module = "Staff Manager",
                                    Texto = "Level 2 Background Renewal (" + diff.Days.ToString() + " days to renew)",
                                    URL = "~/StaffManager/WorkerDocumentation.aspx?ID=" + worker.ID.ToString()
                                };
                                list.Add(notification);
                            }
                            if (diff.Days <= 0)
                            {
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Worker: " + worker.Name,
                                    Module = "Staff Manager",
                                    Texto = "Level 2 Background Renewal  (Past Due)",
                                    URL = "~/StaffManager/WorkerDocumentation.aspx?ID=" + worker.ID.ToString()
                                };
                                list.Add(notification);
                            } 
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            var legaldoc = db.WorkerDocTrackers.First(x => x.IDWorker == worker.ID && x.IDDocumentation == 1);
                            TimeSpan diff = legaldoc.NextRenewalDate.Value - DateTime.Now;
                            if (diff.Days < 30 && diff.Days > 0)
                            {
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Trabajador: " + worker.Name,
                                    Module = "Personal",
                                    Texto = "Antecedentes Nivel 2 (" + diff.Days.ToString() + " días restantes)",
                                    URL = "~/StaffManager/WorkerDocumentation.aspx?ID=" + worker.ID.ToString()
                                };
                                list.Add(notification);
                            }
                            if (diff.Days <= 0)
                            {
                                notification = new NotificationArea
                                {
                                    Importante = true,
                                    Grupo = "Trabajador: " + worker.Name,
                                    Module = "Personal",
                                    Texto = "Antecedentes Nivel 2 (Vencida)",
                                    URL = "~/StaffManager/WorkerDocumentation.aspx?ID=" + worker.ID.ToString()
                                };
                                list.Add(notification);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }
    }
}