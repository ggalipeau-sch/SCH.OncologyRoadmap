<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="View.aspx.cs" Inherits="SCH.OncologyRoadmapWeb.View" %>
<%@ Register Assembly="RadPdf" Namespace="RadPdf.Web.UI" TagPrefix="radPdf" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" runat="server" media="screen" href="~/Content/bootstrap.min.css" />
    <link rel="stylesheet" runat="server" media="screen" href="~/Content/Site.css" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js" type="text/javascript" integrity="sha256-/xUj+3OJU5yExlq6GSYGSHk7tPXikynS7ogEvDej/m4=" crossorigin="anonymous"></script>
   
    <script type="text/javascript">

    </script>
</head>
<body>
    <form runat="server" id="myForm">
      

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
                    <div class="col-xs-12 col-md-6">
                       Opened in Read-Only mode. File is currently checked out by: <asp:Label ID="lblCheckedOUt" runat="server"></asp:Label>               

                    </div>             
                    <div class="col-xs-4 col-md-1">
                         <asp:Button class="btn btn-primary btn-lg" ID="closeBtn" runat="server" OnClick="closeBtn_Click" Text="Close"  style="margin-top: 13px"></asp:Button>
                   </div>
                </div>        
              </div>
            </div>   

         
            <radPdf:PdfWebControlLite id="PdfWebControl1" runat="server" 
                height="800px" 
                width="100%"     
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
        
            
    </form>
</body>
</html>
