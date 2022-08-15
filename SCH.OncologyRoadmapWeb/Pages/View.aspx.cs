using iTextSharp.text.pdf;
using Microsoft.SharePoint.Client;
using RadPdf.Data.Document;
using RadPdf.Integration;
using RadPdf.Lite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;


namespace SCH.OncologyRoadmapWeb
{
    public partial class View : System.Web.UI.Page
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
                if (Request.QueryString["mrn"] != null && Request.QueryString["id"] != null)
                {
                    var listName = ConfigurationManager.AppSettings["ListName"];

                    var mrn = Request.QueryString["mrn"];
                    var id = Request.QueryString["id"];
                    var name = "";
                    if (Request.QueryString["name"] != null)
                        name = Request.QueryString["name"];


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
                        clientContext.Load(item.File, f => f.CheckedOutByUser.Email, f => f.ServerRelativeUrl, f => f.CheckOutType, f => f.Name);
                        var statusField = clientContext.CastTo<FieldChoice>(library.Fields.GetByInternalNameOrTitle("Status"));
                        clientContext.Load(statusField);
                        clientContext.ExecuteQuery();
                                            
                        lblFile.Text = item["FileLeafRef"] as string;

                        try
                        {
                            lblCheckedOUt.Text = item.File.CheckedOutByUser.Email;
                        }
                        catch { }

                        if (item.File.CheckOutType == CheckOutType.None)
                            item.File.CheckOut();

                        var data = item.File.OpenBinaryStream();
                        clientContext.ExecuteQuery();
                        using (var memoryStream = new MemoryStream())
                        {
                            data.Value.CopyTo(memoryStream);
                            byte[] pdfData = memoryStream.ToArray();

                            PdfLiteSettings settings = new PdfLiteSettings();
                            settings.DocumentSettings = PdfDocumentSettings.IsReadOnly;

                            this.PdfWebControl1.CreateDocument(item.File.Name, pdfData, settings);
                        }

                    }

                }

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
            Response.Redirect(GetUrlWithOutParameter("Default.aspx", "id"));
        }
    }
}