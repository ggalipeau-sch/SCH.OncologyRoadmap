using iTextSharp.text.pdf;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SCH.OncologyRoadmapWeb.Models;

namespace SCH.OncologyRoadmapWeb
{
    public partial class Edit : System.Web.UI.Page
    {
        protected void Page_PreInit(object sender, EventArgs e)
        {
            Uri redirectUrl;
            switch (SharePointContextProvider.CheckRedirectionStatus(Context, out redirectUrl))
            {
                case RedirectionStatus.Ok:
                    return;
                case RedirectionStatus.ShouldRedirect:
                    Response.Redirect(redirectUrl.AbsoluteUri, endResponse: true);
                    break;
                case RedirectionStatus.CanNotRedirect:
                    Response.Write("An error occurred while processing your request.");
                    Response.End();
                    break;
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            
            int pdfLockInterval = Convert.ToInt32(ConfigurationManager.AppSettings["PDFLockInterval"]);         
            TimeSpan result = TimeSpan.FromMilliseconds(pdfLockInterval);
            string fromTimeString = result.ToString(@"hh\:mm\:ss");
            lblTimer.Text = fromTimeString;    
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Response.Buffer = true;
                Response.CacheControl = "no-cache";
                Response.AddHeader("Pragma", "no-cache");
                Response.AppendHeader("Cache-Control", "no-store");
                Response.Expires = -1441;
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
                Response.Cache.SetNoStore();


                if (Request.QueryString["mrn"] != null && Request.QueryString["id"] != null)
                {  
                    var listName = ConfigurationManager.AppSettings["ListName"];

                    var mrn = Request.QueryString["mrn"];
                    var id = Request.QueryString["id"];
                    var name = "";
                    if (Request.QueryString["name"] != null)
                        name = Request.QueryString["name"];


                    HttpContext.Current.Session["mrn"] = mrn;
                    HttpContext.Current.Session["id"] = id;

                    lblPatientName.Text = name;
                    lblMRN.Text = mrn;                 


                    var spContext = SharePointContextProvider.Current.GetSharePointContext(Context);
                    using (var clientContext = spContext.CreateUserClientContextForSPHost())
                    {
                        var library = clientContext.Web.Lists.GetByTitle(listName);
                        var item = library.GetItemById(Convert.ToInt32(id));
                        clientContext.Load(clientContext.Web.CurrentUser);
                        clientContext.Load(library);
                        clientContext.Load(item);                
                        clientContext.Load(item.File, f => f.CheckedOutByUser, f => f.ServerRelativeUrl, f => f.CheckOutType, f => f.Name);
                        var statusField = clientContext.CastTo<FieldChoice>(library.Fields.GetByInternalNameOrTitle("Status"));
                        clientContext.Load(statusField);
                        clientContext.ExecuteQuery();

                        HttpContext.Current.Session["CurrentUserEmail"] = clientContext.Web.CurrentUser.Email;
                        HttpContext.Current.Session["Uri"] = item.File.ServerRelativeUrl;
                        lblFile.Text = item["FileLeafRef"] as string;

                    
                        ddlStatus.Items.Clear();
                        List<string> options = new List<string>(statusField.Choices);
                        foreach (string option in options)
                        {
                            bool selected = false;
                            if (item["Status"] as string == option)
                                selected = true;
                              
                            ddlStatus.Items.Add(new System.Web.UI.WebControls.ListItem() { Text = option, Value = option, Selected= selected });
                        }

                        //If file is checked out, check if the time interval has passed and check it in for them
                        bool autoChecking = false;
                        if(item.File.CheckOutType != CheckOutType.None)
                        {
                            if (item["CheckedOutTime"] != null)
                            {
                                DateTime checkedOutTime = Convert.ToDateTime(item["CheckedOutTime"]);                              
                                TimeSpan ts = checkedOutTime - DateTime.UtcNow;
                                int pdfLockInterval = Convert.ToInt32(ConfigurationManager.AppSettings["PDFLockInterval"]);
                                if (ts.TotalMilliseconds > pdfLockInterval)
                                {
                                    CheckInFile();
                                    autoChecking = true;
                                }                                
                            }
                        }


                        //If file isn't checked out let them edit
                        if (item.File.CheckOutType == CheckOutType.None || autoChecking)
                        {                           
                            item.File.CheckOut();
                            item["CheckedOutTime"] = System.DateTime.Now;
                            item.SystemUpdate();
                            clientContext.ExecuteQuery();

                            var data = item.File.OpenBinaryStream();
                            clientContext.ExecuteQuery();
                            using (var memoryStream = new MemoryStream())
                            {
                                data.Value.CopyTo(memoryStream);
                                byte[] pdfData = memoryStream.ToArray();

                                this.PdfWebControl1.CreateDocument(item.File.Name, pdfData);
                            }
                        }
                        else
                        {
                            lblItemCheckedOut.Visible = true;
                        }
                       
                    }

                }

            } else if (Request["__EVENTARGUMENT"] == "save")
            {
                Save();
            }
            
        }

      

        private void CheckInFile()
        {
            try
            {
                var spContext = SharePointContextProvider.Current.GetSharePointContext(Context);
                using (var clientContext = spContext.CreateUserClientContextForSPHost())
                {
                    var id = HttpContext.Current.Session["id"] as string;
                    var listName = ConfigurationManager.AppSettings["ListName"];


                    var library = clientContext.Web.Lists.GetByTitle(listName);
                    var item = library.GetItemById(Convert.ToInt32(id));

                    clientContext.Load(library);
                    clientContext.Load(item, i => i.File, i => i.File.CheckedOutByUser);
                    clientContext.ExecuteQuery();

                    item.File.CheckIn("", CheckinType.MajorCheckIn);
                    clientContext.ExecuteQuery();
                }
            }
            catch { }
        }

        private void Save()
        {
            //Get saved PDF
            byte[] pdfData = this.PdfWebControl1.GetPdf();

            var fileCreationInfo = new FileCreationInformation
            {
                Content = pdfData,
                Overwrite = true,
                Url = Path.GetFileName(HttpContext.Current.Session["Uri"] as string)
            };

            var spContext = SharePointContextProvider.Current.GetSharePointContext(Context);
            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                //Upload File
                var folderPath = Path.GetDirectoryName(HttpContext.Current.Session["Uri"] as string);
                var targetFolder = clientContext.Web.GetFolderByServerRelativeUrl(folderPath);
                var uploadFile = targetFolder.Files.Add(fileCreationInfo);
                clientContext.Load(uploadFile);
                clientContext.ExecuteQuery();

                //Get Differences
                var originalpdfReader = new PdfReader(this.PdfWebControl1.GetOriginalPdf());
                var originalFields = from field in originalpdfReader.AcroFields.Fields
                                     select new
                                     {
                                         FieldName = field.Key,
                                         FieldValue = originalpdfReader.AcroFields.GetField(field.Key)
                                     };

                var newPDFReader = new PdfReader(this.PdfWebControl1.GetPdf());
                var newFields = from field in newPDFReader.AcroFields.Fields
                                select new
                                {
                                    FieldName = field.Key,
                                    FieldValue = newPDFReader.AcroFields.GetField(field.Key)
                                };


                var fieldDifference = originalFields.Except(newFields);

                ListItem item = uploadFile.ListItemAllFields;
                clientContext.Load(item);
                clientContext.ExecuteQuery();

                if (fieldDifference.Count() > 0 || item["Status"] as string != ddlStatus.SelectedValue)
                {
                    //Get the Audit History from SharePoint
                    List<AuditHistory> auditHistoryList = new List<AuditHistory>();
                    if(!string.IsNullOrEmpty(item["History"] as string))
                        auditHistoryList =  JsonSerializer.Deserialize<List<AuditHistory>> (item["History"] as string);


                    //Add a Row for this change
                    AuditHistory auditHistory = new AuditHistory();
                    auditHistory.ModifiedDate = System.DateTime.UtcNow;
                    auditHistory.ModifiedBy = HttpContext.Current.Session["CurrentUserEmail"] as string;

                    List<AuditRecord> auditRecordList = new List<AuditRecord>();
                    if (fieldDifference.Count() > 0)
                    {
                        foreach (var difference in fieldDifference)
                        {
                            var field = difference.FieldName;
                            var originalValue = difference.FieldValue;
                            var newValue = newFields.Where(f => f.FieldName == field).FirstOrDefault().FieldValue;
                            auditRecordList.Add(new AuditRecord { FieldName = field, OldValue = originalValue, NewValue = newValue });                          
                        }
                    } 
                    
                    if (item["Status"] as string != ddlStatus.SelectedValue)
                    {
                        auditRecordList.Add(new AuditRecord { FieldName = "Status", OldValue = item["Status"] as string, NewValue = ddlStatus.SelectedValue });
                        item["Status"] = ddlStatus.SelectedValue;
                    }

                    auditHistory.Record = auditRecordList;
                    auditHistoryList.Add(auditHistory);
                    item["History"] = JsonSerializer.Serialize<List<AuditHistory>>(auditHistoryList);

                    item.Update();
                    clientContext.ExecuteQuery();
                }  
            }

          
            CheckInFile();  
            Response.Redirect(GetUrlWithOutParameter("Default.aspx", "id"));
        }

       
        protected void Timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan result = TimeSpan.FromSeconds(TimeSpan.Parse(lblTimer.Text).TotalSeconds - 1);
            string fromTimeString = result.ToString(@"hh\:mm\:ss");
            lblTimer.Text = fromTimeString;

            if (fromTimeString == "00:00:00")
            {
                CheckInFile();
                Response.Redirect(GetUrlWithOutParameter("Default.aspx", "id"));
            }
        }

     

        private  string GetUrlWithOutParameter(string url, string parameter)
        {
            var nameValueCollection = System.Web.HttpUtility.ParseQueryString(HttpContext.Current.Request.QueryString.ToString());
            nameValueCollection.Remove(parameter);
            return url + "?" + nameValueCollection;
        }

        private string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }

    

        protected void closeBtn_Click(object sender, EventArgs e)
        {
            CheckInFile();
            Response.Redirect(GetUrlWithOutParameter("Default.aspx", "id"));
        }
    }
}