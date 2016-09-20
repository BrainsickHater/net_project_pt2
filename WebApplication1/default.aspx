<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="WebApplication1.WebForm3" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" type="text/css" href="style.css" />

    <title>Homepage</title>
    <meta charset="utf-8" />
</head>
<body>
    <div id="header">Homepage</div>

        <form runat="server"><div class="form">

            Enter a username: <asp:TextBox ID="UserText" runat="server"></asp:TextBox> 
            <span ID="ErrorMessage" class="error" runat="Server"></span> <br />
            <asp:Button Text="Submit" ID="UserSubmit" OnClick="UserSubmit_Click" runat="server"/>   

            <ul id="Links" runat="server">
                <li><a href="webforms/palindrome.aspx">Palindrome Test</a></li>
                <li><a href="webforms/anagram.aspx">Anagram Test</a></li>
            </ul>

        </div></form>

    <div id="footer">Brought to you by Lanyon</div>
</body>
</html>