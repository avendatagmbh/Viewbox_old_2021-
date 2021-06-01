<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Voyager.Models.ToursModels>" %>


 <div class="destination" id="main_destination">

    <% using (Html.BeginForm("Index", "Tours")) { %>	
           
            <div class="date_state" style="display: none"></div>
            <table class="main_grid">
                
                <tr class="tr_grid">
                    <%--<td><a href="<%: Url.Action("AddressForm", "Tours", new{text= 'S'})%>"  class="destlink">Start</a>: </td>--%>
                    <td><a href="#"  class="destlink">Start</a>: </td>
                    <td><% if (!Model.Addressbook.ContainsKey(0)) Model.Addressbook.Add(0, new Domain.Entities.Tables.Address()); %>
                    <%:Html.TextBoxFor(m => m.Addressbook[0].AddressString, new { ReadOnly = "1" })%></td>
                    <td><div class="destlinkresult"></div></td>
                    <td><div class="startresult"></div></td>
                </tr>
                
               <% for (int i = 0; i < Model.Counter; i++) {
                        uint j = (uint)i + 1; %>
                        <tr >
                            <td></td>
                            <td><div class="endresult_end"></div></td>
                        </tr>
                        <tr class="tr_grid">
                            <%--<td><a href="<%: Url.Action("AddressForm", "Tours", new{text=j})%>" class="destlink"><%: j%>.Ziel</a>: </td>--%>
                            <td><a href="#" class="destlink"><%: j%>.Ziel</a>: </td>
                            <td><% if (!Model.Addressbook.ContainsKey(j)) Model.Addressbook.Add(j, new Domain.Entities.Tables.Address()); %>
                            <%:Html.TextBoxFor(m => m.Addressbook[j].AddressString, new { ReadOnly = "1" })%></td>  
                                <td><button type="button" class="date_button">Termin</button></td>
                            <td><div class="destlinkresult"></div></td>
                            <td><div class="dateformshow"></div></td>
                        </tr>
                        <tr >
                            <td></td>
                            <td><div class="endresult_start"></div></td>
                        </tr>
                <%} %>

            </table>
          
            <a href="/Tours/Add" class="newdestination" ><input type="button" class="main_button" value="+ Ziel"/></a>
            
            <input type="submit" value="Übernehmen" class="main_button" />
           
     <%} %>   
  </div>