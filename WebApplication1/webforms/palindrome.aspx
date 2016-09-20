<%@ Page Language="C#" CodeBehind="palindrome.aspx.cs" EnableEventValidation="false"
    Inherits="WebApplication1.WebForm1" AutoEventWireup="true" %>

<!DOCTYPE html>

<head>
    <link rel="stylesheet" type="text/css" href="../style.css" />
    <script src="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.12.4.min.js"></script>

    <script type="text/javascript">

        function setSize() {
            var maxHeight = window.innerHeight - $("#InputDiv").height() - 2 * $("#upperArea").height()
                            - 2 * $("lowerArea").height() - 2 * $("#header").height() - 100;

            $("#ResultList").css('max-width', window.innerWidth + 'px');
            $("#ResultList").css('max-height', maxHeight + 'px');
        }

        $(document).ready(setSize);
        $(window).resize(setSize);

        $(document).ready(function () {
            $("#fadeIn").fadeIn();
            $('#deleted').fadeOut();
        })
    </script>

    <title>Palindrome Test</title>
</head>

<body>
    <a href="../default.aspx">Return to Homepage</a>
    <div id="header">Comparator</div>

    <form runat="server">

        <div class="form" id="InputDiv">
            String 1 <asp:TextBox ID="Input" runat="server"></asp:TextBox> 
            <span ID="ErrorMessage" runat="server" class="error"><!--Error message goes here--></span>

            <br />
            <br />

            <asp:Button ID="Run" runat="server" Text="Run" OnClick="Run_Click" />
            <asp:Button ID="ClearInput" runat="server" Text="Clear" OnClick="ClearInput_Click"/>
            <asp:Button ID="Search" runat="server" Text="Search" Onclick="Search_Click" />
            <asp:Button ID="LoadResults" runat="server" Text="Load Results" OnClick="LoadResults_Click" />
        </div>

        <div runat="server" class="form" id="ResultsDiv">
            <div id="upperArea"> Results <br /> </div>

            <div><asp:Repeater ID="ResultsRepeater" runat="server">

                <HeaderTemplate><ul ID="ResultList"></HeaderTemplate>

                <ItemTemplate>
                    <li class=<%#((String[])Container.DataItem)[1] %> id=<%#((String[])Container.DataItem)[2] %>> 
                        <%#((String[])Container.DataItem)[0] %>
                        <asp:Button class="deleteButton" runat="server" Text="[delete]" 
                            CommandArgument="<%#Container.ItemIndex %>" OnClick="Delete_Click" />
                    </li>
                </ItemTemplate>

                <FooterTemplate> </ul> </FooterTemplate>

            </asp:Repeater></div>
            
            <div id="lowerArea"> 
                <asp:Button ID="ClearResults" runat="server" text="Clear" onclick="ClearResults_Click"/> 
                <asp:Button ID="SaveResults" runat="server" text="Save Results" onclick="SaveResults_Click"/> 
            </div>
        </div>

        <div runat="server" class="form" ID="PrevResultsDiv">
            Previous Results <br />
            
            <div id="UpdateDiv" runat="server">
                <br />
                Enter new String:
                <asp:TextBox ID="UpdateInput" runat="server"></asp:TextBox>
                <asp:Button ID="SubmitUpdate" Text="Update" OnClick="SubmitUpdate_Click" runat="server"/>
                <span ID="UpdateErrorMessage" runat="server" class="error"><!--Error message goes here--></span>
            </div>

            <asp:Repeater ID="PrevResultsRepeater" runat="server">
                <HeaderTemplate><ul id="PrevResultList"></HeaderTemplate>
                
                <ItemTemplate>
                    <li class=<%#((String[])Container.DataItem)[1] %> id=<%#((String[])Container.DataItem)[2] %>> 
                        <%#((String[])Container.DataItem)[0] %>

                        <asp:Button class="deleteButton" runat="server" Text="[delete]" 
                            CommandArgument="<%#Container.ItemIndex %>" OnClick="PrevDelete_Click" />

                        <asp:Button class="deleteButton" runat="server" Text="[update]" 
                            CommandArgument="<%#Container.ItemIndex %>" OnClick="PrevUpdate_Click" />
                    </li>
                </ItemTemplate>    

                <FooterTemplate></ul></FooterTemplate>
            </asp:Repeater>
            <asp:Button ID="HidePrev" runat="server" Text="Hide" OnClick="Hide_Click" />
        </div>
    </form>

</body>

<footer>
    <div id="footer">Brought to you by Lanyon</div>
</footer>


