<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Voyager.Models.LogOnModel>" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Anmelden
</asp:Content>

<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">

  <div class="login-outer">
        <div class="login-inner"></div>
        <div class="login-controls">
            <% using (Html.BeginForm()) { %>
            <%: Html.ValidationSummary(true, "Die Anmeldung war nicht erfolgreich.") %>
            <div class="form-email">
                <%: Html.LabelFor(m => m.Email) %>
                <%: Html.TextBoxFor(m => m.Email)%>
            </div>
            <div class="form-password">
                <%: Html.LabelFor(m => m.Password)%>
                <%: Html.PasswordFor(m => m.Password)%>
            </div>
            <div class="form-rememberme">
                <%: Html.CheckBoxFor(m => m.RememberMe) %>
                <%: Html.LabelFor(m => m.RememberMe)%>
                <p><a href="#">ForgotPassword</a></p>
            </div>

           
            <div class="form-submit">
                <input type="submit" value="Logon" />
            </div>

            <% } %>
        </div>
    </div>
</asp:Content>
