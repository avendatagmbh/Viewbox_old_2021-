<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Voyager.Models.ToursModels>" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
	Voyager
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript" >


        $(function () {
            /***************MAP****************************************/
            var map;
            var geocode;
            var geo;
            var directionsRenderer;
            var directionsService = new google.maps.DirectionsService();
            var distanceErg, durationErg;
            var myMarkerArray = [];
            var statuslist = [];

            $(document).ready(function () {
                var m = $("#map")[0];
                directionsRenderer = new google.maps.DirectionsRenderer();
                var BerlinLatlng = new google.maps.LatLng(52.310, 13.249);
                var myOptions = {
                    zoom: 7,
                    center: BerlinLatlng,
                    mapTypeId: google.maps.MapTypeId.ROADMAP
                };
                map = new google.maps.Map(m, myOptions);
                directionsRenderer.setMap(map);
            });

            function calcRoute(parm) {

                var p = parm - 1;
                var pcont = $($('.tr_grid', '.main_grid').get(p));
                if (pcont.find('input').val() != "" && parm != 1) {
                    if (myMarkerArray) {
                        for (var i in myMarkerArray) {
                            myMarkerArray[i].setMap(null);
                        }
                    }
                    var start = $($('.tr_grid', '.main_grid').get(0)).find('input').val();
                    var end = $($('.tr_grid', '.main_grid').get(p)).find('input').val();
                    var waypointsArray = [];
                    if (parm > 2) {
                        for (var j = 1; j < p; j++) {
                            waypointsArray.push({
                                location: $($('.tr_grid', '.main_grid').get(j)).find('input').val(),
                                stopover: true
                            });
                        }
                    }

                    var request = {
                        origin: start,
                        destination: end,
                        waypoints: waypointsArray,
                        travelMode: google.maps.DirectionsTravelMode.DRIVING
                    };

                    directionsService.route(request, function (result, status) {
                        if (status == google.maps.DirectionsStatus.OK) {
                            directionsRenderer.setDirections(result);

                        }
                    });
                }
                //                 else if(){
                //                    
                //                }
            }



            /***************DESTINATION******************************/

            $('.destlink').live('click', function () {
                if ($('.main_grid').find('.address_form').length != 0) {
                    if ($('.window-outer').find('.select-client').length != 0) {
                        $(".select-client H2").css("color", "red").text(" (Kunde auswählen oder abbrechen!) ");
                        return;
                    }
                    if ($('.window-outer').find('.edit-address: visible').length != 0) {
                        $(".edit-addresst H2").css("color", "red").text(" (Adresse einfühgen oder abbrechen!) ");
                        return;
                    }
                    $('.address_form').remove();
                }
                var text = $(this).text();
                var container = $(this).parents('tr').find('.destlinkresult');
                $.get('/Tours/AddressForm', { text: text }, function (html) {
                    container.append(html);
                }, 'html');
                $("DIV.address_form").show();
                return false;
            });

            $('.address').live('click', function () {
                var text = $(this).text();
                $(this).parents('.destlinkresult').parents('tr').find('input').val(text);
                var item1 = $(this).prev().text();
                $('.address_form').remove();
                $('.select-client').remove();
                $("#map").show();

                $.get('/Tours/Address', { ID: item1 }, function (response) {
                    $('.main').replaceWith($('.main', response));
                });
                calcRoute($('.tr_grid').length);
                return false;
            });

            $('.address_client').live('click', function () {
                if ($('.window-outer').find('.select-client').length != 0) {
                    $('.address_form').hide();
                    return;
                }
                if ($('.window-outer').find('.edit-address').length != 0) {
                    $('.address_form').hide();
                    return;
                }

                var text = "";
                $(this).parents('.destlinkresult').parents('tr').find('input').val(text);
                $.get('/Tours/ClientSelect', function (html) {
                    $('.window-outer').append(html);
                }, 'html');
                $("#map").hide();
                $('.address_form').hide();
                return false;
            });

            $('.address_edit').live('click', function () {
                if ($('.window-outer').find('.edit-address').length != 0) {
                    $('.address_form').hide();
                    return;
                }
                if ($('.window-outer').find('.select-client').length != 0) {
                    $('.address_form').hide();
                    return;
                }
                var text = "";
                $(this).parents('.destlinkresult').parents('tr').find('input').val(text);
                $.get('/Tours/EditAddress', function (html) {
                    $('.window-outer').append(html);
                }, 'html');
                $("#map").hide();
                $('.address_form').hide();
            });


            $('.client_cancel').live('click', function () {
                $('.select-client').remove();
                $("#map").show();
                $('.address_form').show();
            });

            $('.edit_cancel').live('click', function () {
                $('.edit-address').remove();
                $("#map").show();
                $('.address_form').show();
            });

            $('.client').live('click', function () {
                var ID = $(this).parents('td').prev().text();
                $('.select-client').remove();
                $("#map").show();
                $.get('Tours/ClientSelected', { selectedclientID: ID }, function (response) {
                    $('.main').replaceWith($('.main', response));
                    calcRoute($('.tr_grid').length);
                });

                return false;
            });

            $('form', '.edit-address').live('submit', function () {
                geo = new google.maps.Geocoder();
                statuslist[google.maps.GeocoderStatus.OK] = "Erfolgreich";
                statuslist[google.maps.GeocoderStatus.ZERO_RESULTS] = "Keine gültige Adresse angegeben";
                statuslist[google.maps.GeocoderStatus.UNKNOWN_ERROR] = "Eine Geocodierungsanfrage konnte aufgrund eines Serverfehlers nicht verarbeitet werden";
                statuslist[google.maps.GeocoderStatus.INVALID_REQUEST] = "Diese Adresse war ungültig.";
                statuslist[google.maps.GeocoderStatus.REQUEST_DENIED] = "Bad API Key";
                statuslist[google.maps.GeocoderStatus.OVER_QUERY_LIMIT] = "zu viel Anfragen";
                statuslist[google.maps.GeocoderStatus.ERROR] = "Server error";
                geoEncode();
                return false;
            });

            function geoEncode() {

                var address = $('.edit-form_street').find('input').val();
                address += ", " + $('.edit-form_code').find('input').val();
                address += ", " + $('.edit-form_land').find('input').val();
                if (geo) {
                    geo.geocode({ 'address': address }, function (results, status) {
                        if (status == google.maps.GeocoderStatus.OK) {
                            $('.edit-address').remove();
                            $("#map").show();
                            geocode = results[0].geometry.location;
                            savePoint(geocode);
                        } else {
                            var thisstatus = status;
                            if (statuslist[status]) {
                                thisstatus = statuslist[status];
                            }
                            $('.error').html(thisstatus).fadeIn();
                            geocode = false;
                        }
                    });
                }
            }

            function savePoint(geocode) {
                var data = $('.edit_form : input : text').serializeArray();
                data[data.length] = { name: "lng", value: geocode[0] };
                data[data.length] = { name: "lat", value: geocode[1] };
                $('.error').fadeOut();
                $("#map").fadeIn();
                calcRoute($('dest_route').length);
                $.post('Tours/EditAddress', data, function () {
                });
            }

            $('#main_destination form').live('submit', function () {
                calcRoute($('.tr_grid').length);
                $.post('Tours/Index', data, function () {
                });
            });
            /***************DATE*************************************************/

            $('.date_button').live('click', function () {
        
                var container = $(this).parents('tr').find('.dateformshow');
                $.get('/Tours/DateForm', function (html) {
                    container.append(html);
                }, 'html');
                $("DIV.date_form").show();
            });

            $('.date_cancel').live('click', function () {
                $('.date_form').remove();
            });

            $('.dateOK').live('click', function () {
                var radioresult = $(".date_radio input:checked").val();
                var selectresulthour = $(".hour_select select option:selected").val();
                var selectresultmin = $(".min_select select option:selected").val();
                var datetext = $('.datepicker').val();
                var container = $(this).parents('.dateformshow').parents('tr');
                $('.date_state').text(container.find('a').text());
                $.get('/Tours/DateEdit', { rr: radioresult, srhour: selectresulthour, srmin: selectresultmin, date: datetext, a:$('.date_state').text() }, function () {
                    var text = datetext + " um " + selectresulthour + " : " + selectresultmin;
                    (radioresult == "Startzeit") ? container.next().find('.endresult_start').text(text) : container.prev().find('.endresult_end').text(text);
                });
                $('.date_form').remove();
            });
            /*********************NEWDESTINATION********************************************/

            $('.newdestination').live('click', function () {
                $.get($(this).attr("href"), function (response) {
                    $('#main_destination').replaceWith($('#main_destination', response));
                });
                //calcRoute($('.tr_grid').length - 1);
                return false;
            });
        });

    </script>
    <div class="main">

            <div class="main-header">
                <h2 class="title left">Neue Reise</h2>
            </div>

            <div class="dest_form" >
                <% Html.RenderPartial("MainDestination");%>
            </div>

    </div>
					
            <div class="window-outer" >
            <%if (!Model.EditState) { %>
                    <% Html.RenderPartial("Map");%>
                 <%}else{ %>
                 <% Html.RenderPartial("EditAddress");%>
                 <%} %>
            </div>

            <% Html.RenderPartial("NavBar"); %>

</asp:Content>
