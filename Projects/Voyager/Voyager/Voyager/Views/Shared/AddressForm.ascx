<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Voyager.Models.ToursModels>" %>

    <div class="address_form">
        <table>
        <tr>
            <th></th>
            <th>Adresse wählen</th>
        </tr>
        <tr>
            <td>1.</td>
            <td>von Firma</td>
        </tr>
        <tr>
            <td></td>
            <td>    
                <div class="address_id"><%: Model.UserFirmAddress.AddressID %></div>
                <a href="<%: Url.Action("Address", "Tours", new{ID=Model.UserFirmAddress.AddressID})%>" class="address" ><%: Model.UserFirmAddress.AddressString%></a>
            </td>
        </tr>
        <tr>
            <td>2.</td>
            <td>von Wohnort</td>
        </tr>
        <tr>
            <td></td>
            <td>
                <div class="address_id"><%: Model.UserAddress.AddressID %></div>
                <a href="<%: Url.Action("Address", "Tours", new{ID=Model.UserAddress.AddressID})%>" class="address" ><%:Model.UserAddress.AddressString %></a>
            </td>    
        </tr>
        <tr>
            <td>3.</td>
            <td>
                <a href="#" class="address_client">von Kunde</a>
            </td>
        </tr>
        <tr>
            <td>4.</td>
            <td>
               <%-- <%: Html.ActionLink("Adresse eingeben", "Edit") %>--%>
                <a href="#" class="address_edit">Adresse eingeben</a>
            </td>
        </tr>
    </table>
    </div>



