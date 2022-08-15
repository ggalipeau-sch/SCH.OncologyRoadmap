<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="SCH.OncologyRoadmapWeb.Edit" %>
<%@ Register Assembly="RadPdf" Namespace="RadPdf.Web.UI" TagPrefix="radPdf" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" runat="server" media="screen" href="~/Content/bootstrap.min.css" />
    <link rel="stylesheet" runat="server" media="screen" href="~/Content/Site.css" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js" type="text/javascript" integrity="sha256-/xUj+3OJU5yExlq6GSYGSHk7tPXikynS7ogEvDej/m4=" crossorigin="anonymous"></script>

   
    <script type="text/javascript">

    
        var api = null;
        function initRadPdf()
        {
            // Get id
            var id = "<%= PdfWebControl1.ClientID%>";
            var isDownload = false;

            // Get api instance
            api = new PdfWebControlApi(id);          

            
            api.addEventListener(
                "saved",
                function(evt) {
                    // We don't want to show this prompt if we initiated a download
                    if (!isDownload) {                       
                        // Use ASP.NET's built in JavaScript function to initiate the PostBack
                        <%=Page.ClientScript.GetPostBackEventReference(PdfWebControl1, "save")%>;                        
                    }
                }
            );
        }

        function saveClick() {
            if (api) {
                isDownload = false;

                document.getElementById('overlay').style.display = "block";
                document.getElementById('modalSaving').style.display = "block";

                api.save();
            } else {
                window.alert('Error finding PDF API');
            }

            return false;
        }

      

    </script>
</head>
<body>
    <form runat="server" id="myForm">
        <div class="overlay" id="overlay"></div>
        <div class="modalSaving" id="modalSaving">Saving...</div>

        <asp:ScriptManager ID="SM" runat="server">
        </asp:ScriptManager>

        <div class="wrapper">

            <div class="container header">
              <div class="jumbotron">
                <div class="row">  
                    <div class="col-xs-12 col-md-5">                  
                        <div>Patient: <asp:Label ID="lblPatientName" runat="server" /></div>
                        <div>MRN: <asp:Label ID="lblMRN" runat="server" /></div>   
                        <div>File: <asp:Label ID="lblFile" runat="server" /> </div>
                    </div>
                    <div class="col-xs-12 col-md-3">
                        <asp:Timer ID="Timer1" runat="server" OnTick="Timer1_Tick" Interval="1000">
                        </asp:Timer>
                        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                            <ContentTemplate>
                                <span id="CodeAsk">Time Left to Finish:</span><br />
                                <asp:Label ID="lblTimer" runat="server" Text="00:00:00"></asp:Label>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                     <div class="col-xs-4 col-md-2">
                        <asp:Label ID="Label2" runat="server" Text="Status:" /><br /> 
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                             <asp:ListItem Value="In Progress">In Progress</asp:ListItem>
                             <asp:ListItem Value="Complete">Complete</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="col-xs-4 col-md-1">                   
                        <button type="button" class="btn btn-primary btn-lg" onclick="return saveClick()" style="margin-top: 13px">Save</button>      
                    </div>
                    <div class="col-xs-4 col-md-1">
                         <asp:Button class="btn btn-primary btn-lg" ID="closeBtn" runat="server" OnClick="closeBtn_Click" Text="Close"  style="margin-top: 13px"></asp:Button>
                   </div>
                </div>        
              </div>
            </div>
   

         
            <radPdf:PdfWebControlLite id="PdfWebControl1" runat="server" 
                height="100%" 
                width="100%" 
                OnClientLoad="initRadPdf();"           
                HideSideBar="True"
                HideBottomBar = "True"
                HideTopBar = "True"
                HideBookmarks = "True"
                HideThumbnails = "True"
                HideToolsTabs  = "True"
                ViewerPageLayoutDefault = "SinglePageContinuous"
                ViewerZoomDefault = "ZoomFitWidth100"
            />     
       
        
            <asp:Label ID="lblItemCheckedOut" runat="server" Visible="false">Cannot Open Document. Item is currently checked out to another user.</asp:Label>
        </div>
    </form>
</body>
</html>
