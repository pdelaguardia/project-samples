using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using System.Drawing;
using System.Web.Security;

/// <summary>
/// Summary description for Utils
/// </summary>
public static class Utils
{
    static eHealthInfo aInfo;
    static Dictionary<string, string> aLabels = new Dictionary<string, string>();
    static string LabelCulture { get; set; }
    static System.Threading.Timer aTimer;
    public static string Culture
    {
        get{
            if (HttpContext.Current.Session["Culture"] == "es")
                return "es";
            else if (HttpContext.Current.Session["Culture"] == "en")
                return "en";
            else
                return "en";
        }
    }
    public static string Theme
    {
        get
        {
            return "PlasticBlue";
        }
    }
    static Utils()
    {
        aInfo = new eHealthInfo();
    aTimer = new System.Threading.Timer(x => { GC.Collect(); }, null, 120000, 120000);
    aInfo.RootFolder = HttpContext.Current.Request.PhysicalApplicationPath;
    aInfo.DataFolder = "Data";
    if (HttpContext.Current.Session["entityId"] == null)
      HttpContext.Current.Session["entityId"] = 1;
   // LoadLabels();
    }
    private static void LoadLabels()
    {
        try
        {
            aLabels.Clear();
            var res = "~/App_Languages/{0}.txt".SetFormat(LabelCulture).GetPath();
            if (!File.Exists(res))
                File.Create(res);
            var lista = File.ReadAllLines(res);
            foreach (var rs in lista)
            {
                var partes = rs.Split('|');
                aLabels.Add(partes[0], partes[1]);
            }
        }
        catch (Exception ex)
        {
            "Error loading labels".CreateLog(ex);
        }
    }
    private static void SaveLabels()
    {
        try
        {
            var res = "~/App_Languages/{0}.txt".SetFormat(LabelCulture).GetPath();
            var lista = new List<string>(aLabels.Count);
            foreach (var item in aLabels)
                lista.Add("{0}|{1}".SetFormat(item.Key, item.Value));
            File.WriteAllLines(res, lista.ToArray());
        }
        catch (Exception ex)
        {
            "Error saving labels".CreateLog(ex);
        }
    }
    public static void CheckOutsider()
    {
        if ((string)HttpContext.Current.Session["NeedsAuth"] == "True")
        {
            HttpContext.Current.Response.Redirect("~/Account/login.aspx");
        }
    }
    public static void CreateLog(this string pMessage, Exception pException, DataClassesDataContext pBD = null)
    {
        try
        {
            var BD = pBD ?? new DataClassesDataContext();
            BD.Logs.InsertOnSubmit(new Log { Message = pMessage, LogDate = DateTime.Now, Username = HttpContext.Current.User.ToString(), Info1 = pException.Message });
            BD.SubmitChanges();
        }
        catch (Exception ex)
        {
            File.AppendAllText(aInfo.RootFolder + "Logs.txt", String.Format("Dia:{0:d/MM/yyyy}{2}Hora:{0:H:mm:ss}{2}Mensaje:{1}{2}Exception:{3}{2}", DateTime.Now, pMessage, Environment.NewLine, pException.Message));
        }
    }
    public static string SetFormat(this string pString, params object[] args)
    {
        return string.Format(pString, args);
    }
    public static string GetPath(this string pURL)
    {
        if (string.IsNullOrEmpty(pURL))
            return pURL;
        return pURL.Replace("~/", aInfo.RootFolder).Replace('/', '\\');
    }
    public static void PrepareGrid(this DevExpress.Web.ASPxGridView.ASPxGridView grid, bool autoSize = false, bool setFilters = false, string cookies = null, bool Disable = false, bool AutoExpand = true, string popupcaption = "Edición")
    {
        InterfaceMethods.Manager.PrepareGrid(grid, new InterfaceMethods.GridParam
        {
            AutoExpand = AutoExpand,
            Disable = Disable,
            Cookies = cookies,
            AutoSize = autoSize,
            SetFilters = setFilters,
        });
    }
    public static void PreparePopup(this DevExpress.Web.ASPxMenu.ASPxPopupMenu popup, string pElementID)
    {
        InterfaceMethods.Manager.PreparePopup(popup, new InterfaceMethods.PopupMenuParam { PopupElementID = pElementID });
    }
    public static int ConvertFromReportUnitToPixels(float value, ReportUnit pUnit)
    {
        var unit = pUnit == ReportUnit.HundredthsOfAnInch ? GraphicsUnit.Inch : GraphicsUnit.Millimeter;
        var mult = pUnit == ReportUnit.HundredthsOfAnInch ? 100 : 10;
        return Convert.ToInt32(Math.Round(GraphicsUnitConverter.Convert(value, unit, GraphicsUnit.Pixel) * 0.8 / mult));
    }
    public static string SingleLine(XRLabel xrLabel, ReportUnit pUnit = ReportUnit.HundredthsOfAnInch)
    {
        try
        {
            var extra = aspnet_TextTruncator.TextTruncator.TruncateText(xrLabel.Text, Utils.ConvertFromReportUnitToPixels(xrLabel.WidthF, pUnit), xrLabel.Font.Name, Convert.ToInt32(xrLabel.Font.Size));
            if (extra.Length == xrLabel.Text.Length)
                return "";
            var res = xrLabel.Text.Substring(extra.Length);
            xrLabel.Text = extra;
            return res;
        }
        catch (Exception ex)
        {
            "Error truncating text".CreateLog(ex);
            return "";
        }
    }
    public static string MakeShorter(this string pName, int pSize = 20)
    {
        if (pName.Length <= pSize)
            return pName;
        var pos = pName.IndexOf(' ', pSize);
        if (pos < 0)
            return pName;
        return pName.Substring(0, pos) + "...";
    }
    public static string GetLabel(this string pCode, string pCulture = null)
    {
        var culture = pCulture ?? Utils.Culture;
        if (LabelCulture != culture)
        {
            LabelCulture = culture;
            LoadLabels();
        }
        if (aLabels.ContainsKey(pCode))
            return aLabels[pCode];
        aLabels.Add(pCode, pCode);
        SaveLabels();
        return pCode;
    }
    public static void SetTableLabels(this DevExpress.Web.ASPxGridView.ASPxGridView grid, string table = "", string pCulture = null)
    {
        var culture = pCulture ?? Utils.Culture;
        foreach (var item in grid.Columns)
        {
            if (item is DevExpress.Web.ASPxGridView.GridViewDataColumn)
            {
                var col = item as DevExpress.Web.ASPxGridView.GridViewDataColumn;
                col.Caption = "col{0}.{1}".SetFormat(table, col.FieldName).GetLabel(culture);
            }
            else if (item is DevExpress.Web.ASPxGridView.GridViewBandColumn)
            {
                var col = item as DevExpress.Web.ASPxGridView.GridViewBandColumn;
                col.Caption = "col{0}.{1}".SetFormat(table, col.Caption).GetLabel(culture);
                foreach (var subitem in col.Columns)
                {
                    if (subitem is DevExpress.Web.ASPxGridView.GridViewDataColumn)
                    {
                        var subcol = subitem as DevExpress.Web.ASPxGridView.GridViewDataColumn;
                        subcol.Caption = "col{0}.{1}".SetFormat(table, subcol.FieldName).GetLabel(culture);
                    }
                }
            }
        }
    }
    public static void SetLabel(dynamic label, string culture)
      {
      label.Text = GetLabel(label.ID, culture);
      }
    public static void SetPanelLabel(this DevExpress.Web.ASPxRoundPanel.ASPxRoundPanel panel, string culture)
      {
      panel.HeaderText = panel.ID.GetLabel(culture);
      }
  }
[Serializable]
public class eHealthInfo
{
    private string aRootFolder = "";
    private string aDataFolder = "";
    public string RootFolder
    {
        get { return aRootFolder; }
        set { aRootFolder = value; }
    }
    public string DataFolder
    {
        get { return aDataFolder; }
        set { aDataFolder = value; }
    }
    /// <summary>
    /// Returns the path of the user folder
    /// </summary>
    /// <param name="pID">The ID of the folder</param>
    /// <returns>Full path of the folder</returns>
    public string GetEntityFolder(string pID)
    {
        return RootFolder + DataFolder + "\\" + pID;
    }
    public string GetID()
    {
        return "";
    }
}