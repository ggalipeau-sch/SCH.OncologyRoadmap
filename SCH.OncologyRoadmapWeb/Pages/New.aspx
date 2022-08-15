<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="New.aspx.cs" Inherits="SCH.OncologyRoadmapWeb.New" %>
<%@ Register Assembly="RadPdf" Namespace="RadPdf.Web.UI" TagPrefix="radPdf" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" runat="server" media="screen" href="~/Content/bootstrap.min.css" />
    <link rel="stylesheet" runat="server" media="screen" href="~/Content/Site.css" />

   
    <script type="text/javascript">

        function getParameterByName(name, url = window.location.href) {
            name = name.replace(/[\[\]]/g, '\\$&');
            var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, ' '));
        }


        var api = null;
        function initRadPdf()
        {
            // Get id
            var id = "<%= PdfWebControl1.ClientID%>";
            var isDownload = false;

            // Get api instance
            api = new PdfWebControlApi(id);
            
            var mrn = getParameterByName('mrn'); 
            var mrnField = api.getField("MRN");
            if(mrnField)              
                mrnField.setProperties({ "value": mrn });

            var name = getParameterByName('name'); 
            var nameField = api.getField("Patient_Name");
            if(nameField)              
                nameField.setProperties({ "value": name });

            var dob = getParameterByName('dob'); 
            var dobField = api.getField("DOB");
            if(dobField)              
                dobField.setProperties( { "value" : dob } );
            
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

    <form runat="server">
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
                        <div>MRN: <asp:Label ID="lblMRN" runat="server" />  </div>                
                    </div>
                    <div class="col-xs-12 col-md-5">
                        <asp:DropDownList ID="ddlTemplates" runat="server" AutoPostBack="true" OnSelectedIndexChanged="DdlTemplates_SelectedIndexChanged" CssClass="form-control">
                        </asp:DropDownList>
                    </div>
                    <div class="col-xs-4 col-md-1">
                        <button type="button" class="btn btn-primary btn-lg" onclick="return saveClick()">Save</button><br />
                    </div>                 
                    <div class="col-xs-4 col-md-1">
                        <asp:Button class="btn btn-primary btn-lg" ID="closeBtn" runat="server" OnClick="closeBtn_Click" Text="Close"  ></asp:Button>
                   </div>
                </div>
        
              </div>
            </div>
   

         
            <radPdf:PdfWebControlLite id="PdfWebControl1" runat="server" 
                height="800px" 
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
        </div>
       
        
        <asp:Label ID="lblItemCheckedOut" runat="server" Visible="false">Cannot Open Document. Item is currently checked out to another user.</asp:Label>
            
    </form>
</body>
</html>
