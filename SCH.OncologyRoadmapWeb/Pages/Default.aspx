<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SCH.OncologyRoadmapWeb.Default" %>
<%@ Register Assembly="RadPdf" Namespace="RadPdf.Web.UI" TagPrefix="radPdf" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" runat="server" media="screen" href="~/Content/bootstrap.min.css" />
    <link rel="stylesheet" runat="server" media="screen" href="~/Content/Site.css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" integrity="sha512-9usAa10IRO0HhonpyAIVpjrylPvoDwiPUiKdWk5t3PyolY1cOd4DSE0Ga+ri4AuTroPR5aQvXU9xC6qOPnzFeg==" crossorigin="anonymous" referrerpolicy="no-referrer" />
 

</head>
<body>

    <form runat="server">     
        <asp:ScriptManager ID="SM" runat="server">
        </asp:ScriptManager>

        <div class="container">

          <div class="row">
            <div class="col-xs-12">
                <div class="jumbotron">
                    <h1>Oncology Roadmap</h1>
                    <div>Patient: <asp:Label ID="lblPatientName" runat="server" />  </div>
                    <div>MRN: <asp:Label ID="lblMRN" runat="server" />  </div>
                </div>
            </div>              
          </div>

          <div class="row">
           
            <div class="col-xs-12">

               <asp:HyperLink ID="createPDF" runat="server" ><i class="fas fa-plus"></i> Create New</asp:HyperLink>

                <h3>Existing:</h3>
              <asp:Repeater ID="itemsRepeater" runat="server">  
                <HeaderTemplate>
                    <table class="records">
                        <tr>
                            <th>File</th>
                            <th>Status</th>
                            <th>Checked Out To</th>
                            <th>History</th>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>     
                    <tr>
                        <td>    
                            <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl='<%#Eval("URL") %>' > 
                               <asp:Panel runat="server"  Visible='<%#Eval("isAllowEdit").ToString().Length > 0 %>' ToolTip="Edit"> <i class="fa-solid fa-edit"></i> <asp:Label ID="Label1" runat="server" Text='<%#Eval("Name") %>'/>  </asp:Panel>
                               <asp:Panel runat="server"  Visible='<%#Eval("isAllowEdit").ToString().Length <= 0 %>' ToolTip="View"> <i class="fa-solid fa-file"></i> <asp:Label ID="Label2" runat="server" Text='<%#Eval("Name") %>'/>   </asp:Panel>
                            </asp:HyperLink>                        
                        <td>
                            <asp:Label runat="server" Text='<%#Eval("Status") %>'></asp:Label>
                        </td>
                         <td>
                            <asp:Label runat="server" Text='<%#Eval("CheckedOut") %>'></asp:Label>
                        </td>
                        <td>
                            <asp:LinkButton ID="lnkHistory" CausesValidation="false" runat="server" OnClick="lnkHistory_Click"
                                CommandName="ShowHistory" CommandArgument='<%#Eval("History") %>' ToolTip="show audit history">
                                <i class="fa-solid fa-history" style="padding-left: 10px;"></i>
                            </asp:LinkButton>
                         </td>  
                    </tr>
                </ItemTemplate>  
                <FooterTemplate>
                    </table>
                </FooterTemplate>
              </asp:Repeater> 

            </div>              
          </div>

        </div>
   
       <asp:HiddenField ID="hidForModel" runat="server" />
       <ajaxToolkit:ModalPopupExtender ID="modalHistory" runat="server" PopupControlID="ModalPanel" CancelControlID="btnClose" TargetControlID="hidForModel" BackgroundCssClass="modalBackground"></ajaxToolkit:ModalPopupExtender>
       <asp:Panel ID="ModalPanel" runat="server" CssClass="modalPopup" align="center" style="display:none">
             <div style="height: 90%">
                <h2>History</h2>
                <div ID="lblHistory1" class="historyTableWrapper" runat="server" ></div>
             </div>
             <asp:Button ID="btnClose" runat="server" Text="Close" class="btn btn-primary" />
       </asp:Panel>

    </form>
</body>
</html>
