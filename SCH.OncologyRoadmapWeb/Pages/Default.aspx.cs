using iTextSharp.text.pdf;
using Microsoft.SharePoint.Client;
using RadPdf.Integration;
using SCH.OncologyRoadmapWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Web;
using System.Web.UI;

namespace SCH.OncologyRoadmapWeb
{
    public partial class Default : System.Web.UI.Page
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
                 
                    HttpContext.Current.Session["mrn"] = mrn;
                    HttpContext.Current.Session["name"] = name;

                    lblPatientName.Text = name;
                    lblMRN.Text = mrn;
                    createPDF.NavigateUrl = "New.aspx?" + Request.QueryString;

                    var spContext = SharePointContextProvider.Current.GetSharePointContext(Context);
                    using (var clientContext = spContext.CreateUserClientContextForSPHost())
                    {

                        var library = clientContext.Web.Lists.GetByTitle(listName);
                        CamlQuery qry = new CamlQuery();
                        qry.ViewXml =
                        @"<View>  
                            <Query> 
                                <Where>                                   
                                    <Eq><FieldRef Name='MRN' /><Value Type='Text'>" + mrn + @"</Value></Eq>                                                             
                                </Where>
                            </Query> 
                            <ViewFields><FieldRef Name='FileLeafRef' /><FieldRef Name='Status' /><FieldRef Name='ID' /><FieldRef Name='History' /><FieldRef Name='CheckoutUser' /></ViewFields> 
                          </View>";
                        ListItemCollection listItems = library.GetItems(qry);
                        clientContext.Load(listItems);
                        clientContext.Load(clientContext.Web.CurrentUser);
                        clientContext.ExecuteQuery();

                        List<PDFListItem> pdfListItems = new List<PDFListItem>();
                        foreach(ListItem item in listItems)
                        {
                            var pdfListItem = new PDFListItem();
                            pdfListItem.Name = item["FileLeafRef"] as string;
                     
                            if (item["CheckoutUser"] == null || ((FieldUserValue)item["CheckoutUser"]).Email == clientContext.Web.CurrentUser.Email)
                            {
                                pdfListItem.isAllowEdit = "true";
                                pdfListItem.URL = "Edit.aspx?" + Request.QueryString + "&id=" + item.Id;
                            }
                            else
                            {                       
                                pdfListItem.isAllowEdit = "";
                                pdfListItem.URL = "View.aspx?" + Request.QueryString + "&id=" + item.Id; ;
                            }

                            List<AuditHistory> auditHistoryList = new List<AuditHistory>();
                            if (!string.IsNullOrEmpty(item["History"] as string))
                                auditHistoryList = JsonSerializer.Deserialize<List<AuditHistory>>(item["History"] as string);

                            pdfListItem.History = BuildHistory(auditHistoryList);

                            pdfListItem.Status = item["Status"] as string;          
                            if(item["CheckoutUser"] != null)
                                pdfListItem.CheckedOut = ((FieldUserValue)item["CheckoutUser"]).LookupValue;      
                            
                            pdfListItems.Add(pdfListItem);
                        }                   
                        itemsRepeater.DataSource = pdfListItems;
                        itemsRepeater.DataBind();

                    }

                }

            } 
            
        }

        private string BuildHistory(List<AuditHistory> auditHistoryList)
        {
            string history = "";

            history = @"<table>
                        <tr>
                            <th>Modified Date</th>
                            <th>Modified By</th>
                            <th>Audit</th>
                        </tr>";

            foreach(AuditHistory auditHistory in auditHistoryList)
            {
                history += "<tr>";
                history += "<td>" + auditHistory.ModifiedDate.ToLocalTime() + "</td>";
                history += "<td>" + auditHistory.ModifiedBy + "</td>";
                history += "<td>" + BuildAuditRecord(auditHistory.Record) + "</td>";
                history += "</tr>";
            }

            history += "</table>";

            return history;
        }

        private string BuildAuditRecord(List<AuditRecord> auditRecordList)
        {
            string audit = @"<table>
                        <tr>
                            <th>Field Name</th>
                            <th>Old Value</th>
                            <th>New Value</th>
                        </tr>";
            foreach (AuditRecord auditRecord in auditRecordList)
            {
                audit += "<tr>";
                audit += "<td>" + auditRecord.FieldName + "</td>";
                audit += "<td>" + auditRecord.OldValue + "</td>";
                audit += "<td>" + auditRecord.NewValue + "</td>";
                audit += "</tr>";
            }
            audit += "</table>";

            return audit;
        }


        protected void lnkHistory_Click(object sender, EventArgs e)
        {
            string history = ((System.Web.UI.WebControls.LinkButton)sender).CommandArgument;
            lblHistory1.InnerHtml = history;
            this.modalHistory.Show();
        }
    }

    public class PDFListItem
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string History { get; set; }
        public string Status { get; set; }
        public string CheckedOut { get; set; }

        public string isAllowEdit { get; set; }
    }
}