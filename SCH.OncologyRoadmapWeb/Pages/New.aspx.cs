using iTextSharp.text.pdf;
using Microsoft.SharePoint.Client;
using RadPdf.Integration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;


namespace SCH.OncologyRoadmapWeb
{
    public partial class New : System.Web.UI.Page
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

        protected void Page_Load(object sender, EventArgs e)
        {   

            if (!IsPostBack)
            {

                if (Request.QueryString["mrn"] != null)
                {
                    var listName = ConfigurationManager.AppSettings["ListName"];

                    var mrn = Request.QueryString["mrn"];
                    var name = "";
                    if (Request.QueryString["name"] != null)
                        name = Request.QueryString["name"];

                    HttpContext.Current.Session["name"] = name;
                    HttpContext.Current.Session["mrn"] = mrn;

                    lblPatientName.Text = name;
                    lblMRN.Text = mrn;                

                    var spContext = SharePointContextProvider.Current.GetSharePointContext(Context);
                    using (var clientContext = spContext.CreateUserClientContextForSPHost())
                    {
                        var templatesLibrary = clientContext.Web.Lists.GetByTitle(ConfigurationManager.AppSettings["TemplatesLibraryName"]);
                        var library = clientContext.Web.Lists.GetByTitle(listName);
                        CamlQuery qry = CamlQuery.CreateAllItemsQuery();
                        ListItemCollection items = templatesLibrary.GetItems(qry);
                        clientContext.Load(library, l => l.RootFolder);
                        clientContext.Load(templatesLibrary);
                        clientContext.Load(items);
                        clientContext.Load(clientContext.Web.CurrentUser);                     
                        clientContext.ExecuteQuery();

                        HttpContext.Current.Session["CurrentUserEmail"] = clientContext.Web.CurrentUser.Email;
                       HttpContext.Current.Session["LibraryFolderPath"] = library.RootFolder.ServerRelativeUrl;


                        ddlTemplates.Items.Clear();
                        ddlTemplates.Items.Add(new System.Web.UI.WebControls.ListItem() { Text = "-- Select a Roadmap --", Value =""});
                        foreach (ListItem I in items)
                        {
                             ddlTemplates.Items.Add(new System.Web.UI.WebControls.ListItem() { Text = I["FileLeafRef"].ToString().Replace(".pdf" , ""), Value = I["ID"].ToString() });
                        }
                    }

                }

            } else if (Request["__EVENTARGUMENT"] == "save")
            {
                Save();
            }
            
        }

        protected void DdlTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            var templateID = ddlTemplates.SelectedItem.Value;
            if (!string.IsNullOrEmpty(templateID))
            {
                var spContext = SharePointContextProvider.Current.GetSharePointContext(Context);
                using (var clientContext = spContext.CreateUserClientContextForSPHost())
                {
                    var library = clientContext.Web.Lists.GetByTitle(ConfigurationManager.AppSettings["TemplatesLibraryName"]);
                    var item = library.GetItemById(templateID);
                    clientContext.Load(clientContext.Web.CurrentUser);
                    clientContext.Load(library, l => l.RootFolder);
                    clientContext.Load(item);
                    clientContext.Load(item.File);
                    clientContext.ExecuteQuery();
                  

                    HttpContext.Current.Session["FileName"] = Uri.EscapeDataString(item["FileLeafRef"].ToString().Replace(".pdf", "")) + "_" + Uri.EscapeDataString(HttpContext.Current.Session["name"] as string) + "_" + HttpContext.Current.Session["mrn"] + ".pdf";

                    var data = item.File.OpenBinaryStream();
                    clientContext.ExecuteQuery();
                    using (var memoryStream = new MemoryStream())
                    {
                        data.Value.CopyTo(memoryStream);
                        byte[] pdfData = memoryStream.ToArray();

                        this.PdfWebControl1.CreateDocument(item.File.Name, pdfData);
                    }
                }
            }
        }

        private void Save()
        {          

            //Get saved PDF
            byte[] pdfData = this.PdfWebControl1.GetPdf();

        
            var fileCreationInfo = new FileCreationInformation
            {
                Content = pdfData,
                Overwrite = true,
                Url = HttpContext.Current.Session["FileName"] as string
            };

            var spContext = SharePointContextProvider.Current.GetSharePointContext(Context);
            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                //Upload File
                var folderPath = HttpContext.Current.Session["LibraryFolderPath"] as string;
                var targetFolder = clientContext.Web.GetFolderByServerRelativeUrl(folderPath);
                var uploadFile = targetFolder.Files.Add(fileCreationInfo);
                clientContext.Load(uploadFile);
                clientContext.ExecuteQuery();
                       

                ListItem item = uploadFile.ListItemAllFields;
                item["MRN"] = HttpContext.Current.Session["mrn"];
                item["PatientName"] = HttpContext.Current.Session["name"];
                item["Status"] = "In Progress";
                item["History"] = 
                   String.Format(@"<table>
                        <tr>
                            <th>Modified Date (UTC)</th>
                            <th>Modified By</th>
                            <th>Audit</th>
                        </tr>
                        <tr>
                            <td>{0}</td>
                            <td>{1}</td>
                            <td>{2}</td>
                        </tr>
                    </table>", 
                    System.DateTime.UtcNow, 
                    HttpContext.Current.Session["CurrentUserEmail"],
                    "Roadmap Initially Created"
                 );
                item.Update();
                clientContext.ExecuteQuery();

            

            }
          
            Response.Redirect(GetUrlWithOutParameter("Default.aspx", "id"));
        }

        private string GetUrlWithOutParameter(string url, string parameter)
        {
            var nameValueCollection = System.Web.HttpUtility.ParseQueryString(HttpContext.Current.Request.QueryString.ToString());
            nameValueCollection.Remove(parameter);
            return url + "?" + nameValueCollection;
        }


        protected void closeBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect(GetUrlWithOutParameter("Default.aspx", "id"));
        }


    }
}