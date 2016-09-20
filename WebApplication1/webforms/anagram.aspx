﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="anagram.aspx.cs" Inherits="WebApplication1.WebForm2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Anagram Checker</title>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/2.1.0/jquery.min.js"></script>
    <script src="/AnagramJS.js"></script>
    <link rel="stylesheet" href="../style.css"/>
</head>
<body>
    <input type="hidden" id="FlagHiddenInput" runat="server" />
    
    <div style="width: 50%">
        <form id="form1" runat="server">
        <asp:HiddenField runat="server" ID="UpdateResponseField"/>
        <a href="../default.aspx">Return to Homepage</a>
        <div id="topDiv">
            <p style="text-align: center; background-color: #ff0000; margin-bottom: 2px; color: #ffffff">Comparator</p>
            <asp:HiddenField runat="server" ID="hiddenUpdateOk"/>
            <fieldset id="topFieldSet" style="margin: 0px;">
                    <asp:Label ID="Label1" runat="server" Text="String 1 "></asp:Label>
                        <asp:TextBox ID="String1" runat="server" size="30" maxlength="40"></asp:TextBox>
                        <asp:Label ID="TooLongError1" runat="server" class="errorLabel" style="display:none" Text=" *** String entered is too long. (Greater than 40 characters) *** "></asp:Label>
                        <asp:Label ID="PleaseEnterString1" runat="server" class="errorLabel" style="display:none" Text=" *** Please enter a String *** "></asp:Label>
                        <br/><br/>
                    <asp:Label ID="Label2" runat="server" Text="String 2 "></asp:Label>
                        <asp:TextBox ID="String2" runat="server" size="30" maxlength="40"></asp:TextBox>
                        <asp:Label ID="TooLongError2" runat="server" class="errorLabel" style="display:none" Text=" *** String entered is too long. (Greater than 40 characters) *** "></asp:Label>
                        <asp:Label ID="PleaseEnterString2" runat="server" class="errorLabel" style="display:none" Text=" *** Please enter a String *** "></asp:Label>
                        <asp:Label ID="CharactersMissingError" runat="server" class="errorLabel" style="display:none" />
                        <br/><br/>
                    <asp:Button ID="RunButton" runat="server" Text="Run" OnClientClick="return validateRun()" OnClick="RunButton_Click" />
                    <asp:Button ID="ClearButton" runat="server" Text="Clear" OnClientClick="clearAll()" OnClick="ClearTop_Click"/>
                    <asp:Button ID="LoadButton" runat="server" Text="Load Results" OnClientClick="return validateRun()" OnClick="LoadResults_Click"/>
                    <asp:Button ID="DeleteButton" runat="server" Text="Delete Results" OnClientClick="return OK()" OnClick="DeleteResults_Click"/>
                    <div id="UpdateOKDiv" style="display:none">
                        <asp:Button ID="UpdateOKButton" runat="server" OnClientClick="return validateRun()" OnClick="UpdateResult_Click" Text="Update"/>
                    </div>
                    <button id="CancelUpdate" onclick="cancelUpdate()" style="display:none">Cancel</button>
                    <div id="DeletionDiv" runat="server" visible="false" style="margin-top: 5px;">
                        <asp:Label ID="deleteResult" runat="server" class="isAnagram"/>
                    </div>
                    <div id="NoResultsDiv" runat="server" visible="false" style="margin-top: 5px; color: red">
                        <asp:Label ID="NoResultsLabel" runat="server" />
                    </div>
                    <div id="NoDeleteResultsDiv" runat="server" visible="false" style="margin-top: 5px; color: red">
                        <asp:Label ID="NoDeleteResultsLabel" runat="server" />
                    </div>
            </fieldset>
        </div>
        <div id="resultsDiv" visible="false" runat="server" >
            <fieldset id="tableFieldSet" style="margin: 0px;">
                <h4 style="margin: 2px;">Results</h4>
                <div id="tabledivresults" style="overflow-y: auto; margin-top: 3px; margin-bottom: 6px; overflow-x: auto">
                <asp:Repeater id=Repeater1 runat="server">

                    <HeaderTemplate>

                        <table id="table1">
                          <tr style="display:none">
                            <td></td>
                            <td></td>
                          </tr>

                    </HeaderTemplate>

                    <ItemTemplate>

                        <tr style=" overflow-x: auto;" class="<%# checkLast(Container.ItemIndex) %>">
                          <td style="padding-left: 12px;" class="<%# DataBinder.Eval(Container.DataItem, "Flag") %>">&#8226 <%# DataBinder.Eval(Container.DataItem, "Response") %> </td>
                          <td> <asp:Button runat="server" OnClientClick="FadeOut(this)" OnClick="DeleteResult_Click" CommandArgument="<%# Container.ItemIndex %>" Text="[delete]" class="deleteButton"/> </td>
                          <td> <button id="<%# Container.ItemIndex %>" onclick="EnterUpdates(this); return false;" class="deleteButton">[update]</button></td>
                        </tr>

                    </ItemTemplate>

                    <FooterTemplate>

                        </table>

                    </FooterTemplate>

                </asp:Repeater>
                </div>
                <asp:Button id="clearResultsButton" OnClick="ClearResults_Click" runat="server" Text="Clear"/>
                <asp:Button id="SaveButton" OnClick="SaveResults_Click" runat="server" Text="Save Results"/>
                <div id="SaveSuccessDiv" runat="server" visible="false" style="margin-top: 5px;">
                    <asp:Label ID="SaveSuccessLabel" runat="server" />
                </div>
            </fieldset>
        </div>
        <div id="UpdateDiv" runat="server" visible="false">
            <fieldset style="margin: 0px;">

            </fieldset>
        </div>
        </form>
        <p style="text-align: right; background-color: #008000; margin-top: 2px; color: #ffffff">Brought to you by Lanyon</p>
    </div>
</body>
<script type="text/javascript">
    function hideAllErrors() {
        $(".errorLabel").hide();
    }

    $("#String1").keypress(function () {
        $("#PleaseEnterString1").hide();
    })

    $("#String2").keypress(function () {
        $("#PleaseEnterString2").hide();
    })

    function EnterUpdates(element) {
        var i = 0;
        console.log(element);
        $('#hiddenUpdateOk').val($(element).attr('id'));
        $('#table1 > tbody  > tr').hide();
        $(element).hide();
        $(element).parent().parent().show();
        $('#UpdateOKDiv').css("display", "inline");
        $("#CancelUpdate").show();
        $('#LoadButton').hide();
        $('#ClearButton').hide();
        $('#RunButton').hide();
        $('#DeleteButton').hide();
        $('#CharactersMissingError').hide();
        $('#SaveSuccessDiv').hide();
    }

    function CancelUpdate() {
        $('#table1 > tbody  > tr').show();
        $("#UpdateOKButton").hide();
        $("#CancelUpdate").show();
        $('#LoadButton').show();
        $('#ClearButton').show();
        $('#RunButton').show();
        $('#DeleteButton').show();
        $('#CharacterMissingError').show();
    }

    function OK() {
        var answer = confirm("Are you sure you want to delete results?");
        if (answer == true) {
            return validateRun();
        } else {
            return false;
        }
    }

    function validateRun() {
        var flag = 0;
        hideAllErrors();
        console.log("Hey");
        if ($("#String1").val().length > 40) {
            $("#TooLongError1").show();
            flag = 1;
        }
        if ($("#String2").val().length > 40) {
            $("#TooLongError2").show();
            flag = 1;
        }
        if ($("#String1").val().length < 1) {
            $("#PleaseEnterString1").show();
            flag = 1;
        }
        if ($("#String2").val().length < 1) {
            $("#PleaseEnterString2").show();
            flag = 1;
        }
        if (flag == 1) {
            return false;
        }
        else {
            console.log("true");
            return true;
        }

    }



    function clearAll() {
        hideAllErrors();
        $("#String1").val("");
        $("#String2").val("");

    }

    function deleteRow(element) {
        $(element).parent().remove();
        console.log($("#resultsDynamicDiv").html());
    }

    function FadeOut(element) {
        $(element).parent().parent().fadeOut();
    }

    function setMaxHeightAndWidth() {
        var winH = window.innerHeight;
        var topDivHeight = $("#topDiv").height();
        var resultsMaxHeight = winH - topDivHeight - 120;

        var resultsMaxWidth = $("#topDiv").width() - 40;

        $("#tabledivresults").css("max-height", resultsMaxHeight + "px");
        $("#tabledivresults").css("max-width", resultsMaxWidth + "px");

    }

    function setMinTDWidth() {
        $("div").css("min-width", "400px");
        $("fieldset").css("min-width", "420px");
        $("p").css("min-width", "448px");
    }

    $(document).ready(function () {
        console.log("ready");
        setMaxHeightAndWidth();
        setMinTDWidth();
        var id = $("#table1 tr:last");
        if (!($("#FlagHiddenInput").val() == "clear")) {
            $(id).fadeIn('slow');
        } else {
            $(id).show();
        }

        if ($('#table1 tr').length < 2) {
            $("#resultsDiv").hide();
        }
    });

    $(window).resize(function () {
        setMaxHeightAndWidth();
    });
</script>
</html>
