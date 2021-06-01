<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Voyager.Models.ToursModels>" %>
<script type="text/javascript">
    var geocoder;
    Sys.Mvc.ValidatorRegistry.validators.MyValidation = function (rule) {
    geocoder= new google.maps.Geocoder();
    var str = $('.edit-form_street').find('input').val();
    var code = $('.edit-form_code').find('input').val();
    var land = $('.edit-form_land').find('input').val();
    var address= str + ", " +code + " "+ land;
    var message;
        if (geocoder) {
            geocoder.geocode({'address': address}, function(results, status){
                if(status == goolge.maps.GeocoderStatus.ZERO_RESULTS){
                    message= "Diese Addresse existiert nicht";
                }else if(status == google.maps.GeocoderStatus.OK){
                    Model.newEditedAddresse.Latitude = results[0].geometry.location.lat();
                    Model.newEditedAddresse.Longitude = results[0].geometry.location.lng();
                    message = "Geokodierung war erfolgreich";
                }else {
                    message = "Geokodierung war nicht erforgreich aus folgendem Grund:" + status;
                }
            });
        }
    return function(value, context){
        ('.edit-address').find('h2').text(message);
    };
    }
</script>


<div class="edit-address">
<h1> neue Adresse editieren</h1>
<h2></h2>
       <%-- <%Html.EnableClientValidation();%>--%>
        <%using (Html.BeginForm("EditAddress", "Tours")) { %>
            <%: Html.ValidationSummary(true, "Nicht alle Felder geben einen Wert zurück.")%>
            <div class="edit_form">
            <div class="error" style="display:none"></div>
                <div class="edit-form_street">
                    <%: Html.LabelFor(m => m.newEditedAddresse.Street)%>
                    <%: Html.TextBoxFor(m =>m.newEditedAddresse.Street)%>
                </div>
                <div class="edit-form_code">
                    <%: Html.LabelFor(m => m.newEditedAddresse.Postcode)%>
                    <%: Html.TextBoxFor(m => m.newEditedAddresse.Postcode)%>
                </div>
                <div class="edit-form_land">
                    <%: Html.LabelFor(m => m.newEditedAddresse.Land)%>
                    <%: Html.TextBoxFor(m => m.newEditedAddresse.Land)%>
                </div>
                <div class="edit-form_desc">
                    <%: Html.LabelFor(m => m.newEditedAddresse.description)%>
                    <%: Html.TextBoxFor(m => m.newEditedAddresse.description)%>
                </div>
            </div>
       <div class="edit-submit">
            <p><input type="submit" value="Übernehmen" id="edit"/>
            <input type="button" class="edit_cancel" value="Abbrechen"/>
            </p>
       </div>
    <%} %>
</div>