<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
        <script type="text/javascript" >
            $(document).ready(function () {
                $("#datepicker").datepicker();
            });
        </script>

<div class="date_form" >
    <h3> Termin</h3>
        <%using (Html.BeginForm()) { %>
        <div class="date_radio">
	        <input type="radio" name="date_time" value = "Startzeit" id="date_start" checked="checked" />
	        <label for="date_start">Startzeit</label><br />
	        <input type="radio" name="date_time" id="date_end" value = "Endzeit" />
	        <label for="date_end">Endzeit</label>
        </div>

        <div class="hour_select">
	        <select name="hour_select">
                <%for (int i = 5; i < 24; i++) {%>
                <option value="<%:i %>"><%:i%> </option>
                <%} %>   
	        </select>
	        <span>h</span> 
            </div>
             <div class="min_select">
	        <select name="min_select">
            <% for (int i = 0; i < 12; i++) {
                   if (i == 0 || i == 1) {%>
                <option value="<%:i*5 %>">0<%:i * 5%></option>
                <%} else { %>
                <option value="<%:i*5%>"><%:i * 5%></option>
            <%  }
               }%>
	        </select>
            <span >min</span>
        </div>	
            <label for="datepicker">Datum:</label>
            <input type="text" id="datepicker" class="datepicker" value="<%:DateTime.Now.ToShortDateString() %>"/>
        <div class="date-submit">
            <p>
                <input type="button" value="Ok" class="dateOK" />
                <input type="button" value="Abbrechen" class="date_cancel" />
            </p>
        </div>
        <%} %>
</div>