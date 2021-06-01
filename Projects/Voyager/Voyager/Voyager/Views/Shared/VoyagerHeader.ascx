<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

        <div id="header">
			<div class="header-top">
				<div class="header-top-controls">
					<div class="separator"></div>
					<a href="<%: Url.Action("LogOff", "Account") %>" class="header-button button-close"></a>
				</div>
				<div class="header-top-login">
                    <% if (Request.IsAuthenticated) {%>
					    <a href="#" class="login"><%: Voyager.CurrentUser.User.Name %></a>
                    <% } else { %><a href="#" class="login">Anmelden</a>
                    <%} %>
				</div>
			</div>
			<div class="header-top-shadow"></div>
		</div>
