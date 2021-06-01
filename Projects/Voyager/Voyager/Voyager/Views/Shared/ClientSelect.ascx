<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Voyager.Models.ToursModels>" %>

<div class="select-client">
    <h1>Alle Kunden</h1>
    <h2></h2>
    <table>
        <tr>
            <th>ID</th>
            <th>Kunde</th>
            <th></th>
        </tr>
        <% foreach (var item in Model.Clients) { %>
            <tr>
                <td><%:item.Key %></td>
                <td><a href="<%:Url.Action("ClientSelected", "Tours", new {selectedclientID=item.Key}) %>" class="client"><%:item.Value.Name%></a></td>
                <td></td>
            </tr>
        <%} %>
    </table>
    <p><input type="button" class="client_cancel" value="Abbrechen" onclick="calcRoute($('.destlink', '.main_grid').length)"/></p>
</div>