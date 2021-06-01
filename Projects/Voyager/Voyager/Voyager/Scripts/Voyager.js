var map;
	var directionsRenderer;
	var directionsService;
	var distanceErg, durationErg;
	var myMarkerArray = [];
	var myPolyline; 
	var myPathArray = [];
	var myInfoWindow;
	
//	function mapInitialize() {
//		directionsRenderer = new google.maps.DirectionsRenderer();
//		directionsService = new google.maps.DirectionsService();    // Instantiate a  DirectionsService class
//		//myInfoWindow = new google.maps.InfoWindow();				// Instantiate a  InfoWindow class
//		var BerlinLatlng = new google.maps.LatLng(52.310, 13.249);
//		var myOptions = {
//		  zoom: 7,
//		  center: BerlinLatlng ,
//		  mapTypeId: google.maps.MapTypeId.ROADMAP
//		};
//		map = new google.maps.Map(document.getElementById("map"), myOptions);	// Instantiate a  Map class
//		directionsRenderer.setMap(map);
//	}
	

	
	function TasteGedrueckt (Ereignis) {
	if (!Ereignis)
		Ereignis = window.event;
	  if (Ereignis.which) {
		Tastencode = Ereignis.which;
	  } else if (Ereignis.keyCode) {
		Tastencode = Ereignis.keyCode;
	  }
	  if(Tastencode ==13){
		calcRoute();
	  }
	}

	function calcRoute(){  
		if(myMarkerArray){	
			for (var i in myMarkerArray) {
			myMarkerArray[i].setMap(null);
			}
		}
		if(myPolyline){
			myPolyline.setMap(null);
			myPathArray.length = 0;
		}
		var count = 0;
		for(var i=0; i< document.getElementById("dest_form").childNodes.length; i++){
			if(document.getElementById("dest_form").childNodes[i].nodeName == "DIV"){
				count++;
				}
		}
		
		var start = document.getElementById("add_0").value;
		var endid = count-1;
		endid = "add_"+ String(endid);
		var end = document.getElementById(endid).value;
		var waypointsArray =[];
		if( count > 2){
			for(var i=1; i<count-1; i++){
				var str = "add_"+ i;
				waypointsArray.push({
				location:document.getElementById(str).value,
				stopover:true
				});
			}
		}
		var request = {
			origin:start, 
			destination:end,
			waypoints: waypointsArray,
			travelMode: google.maps.DirectionsTravelMode.DRIVING
		};
		directionsService.route(request, function(result, status){
		if(status == google.maps.DirectionsStatus.OK){
			directionsRenderer.setDirections(result);
			showDistDurat(result);
			//setMarkers(result);
			//setPolyline(result);
		}else if(status == google.maps.DirectionsStatus.NOT_FOUND){
			alert("Mindestens einer der Orte (Ursprungsort, Zielort oder Wegpunkte) konnte nicht geocodiert werden. Geben Sie neuen Ort ein.");
		}else if(status == google.maps.DirectionsStatus.ZERO_RESULTS){
			alert("Zwischen Ursprungsort und Zielort konnte keine Route gefunden werden.");
		}else if(status == google.maps.DirectionsStatus.UNKNOWN_ERROR){
			alert("Eine Routenanfrage konnte aufgrund eines Serverfehlers nicht verarbeitet werden. Die Anfrage ist moeglicherweise erfolgreich, wenn Sie es erneut versuchen.");
		}else if(status == google.maps.DirectionsStatus.MAX_WAYPOINTS_EXCEEDED){
			alert("Es sind maximal 8 Wegpunkte zulaessig, plus Ursprungsort und Zielort.");
		}
		});	
	}

	/*function setMarkers(directionResult){
			
		var myRoute = directionResult.routes[0];
		var myMarker;
		var myimage = new google.maps.MarkerImage(
			'style/Content/img/icons/csv.png',
			new google.maps.Size(30,35),
			new google.maps.Point(0,0),
			new google.maps.Point(0,35)
		);		
		for (var i = 0; i < myRoute.legs.length; i++) {
			var j= i+1;
			if(i==0){
				myMarker = new google.maps.Marker({
					position: myRoute.legs[i].start_location,
					map: map,
					icon: myimage
				});				
				InfoText(myMarker, myRoute.legs[i].start_address);
				myMarkerArray[i] = myMarker;
			}
			
			myMarker = new google.maps.Marker({
				position: myRoute.legs[i].end_location,
				map:map,
				icon: myimage
			});
			InfoText(myMarker, myRoute.legs[i].end_addresse);
			myMarkerArray[j] = myMarker;	
		}	
	}

	function InfoText(marker, text) {
	  google.maps.event.addListener(marker, 'click', function() {
		myInfoWindow.setContent(text);
		myInfoWindow.open(map, marker);
	  });
	}*/
	
		/*function setPolyline(directionResult){
		var myRoute = directionResult.routes[0];
		myPathArray = myRoute.overview_path;
		myPolyline = new google.maps.Polyline({
			path: myPathArray,
			strokeColor: '#00AAFF',
			strokeOpacity: 0.6,
			strokeWeight: 3
		});
		myPolyline.setMap(map);
	}*/
	
	function showDistDurat(directionResult){
		var myRoute = directionResult.routes[0];
		for (var i = 0; i < myRoute.legs.length; i++) {
			j=i+1;
			var diststr = "distance_"+ j;
			var durastr = "duration_"+ j;
			distanceErg = document.getElementById(diststr);
			durationErg = document.getElementById(durastr);
			distanceErg.value = myRoute.legs[i].distance.text;
			durationErg.value = myRoute.legs[i].duration.text;
			document.getElementById(diststr).style.display = "inline";
		}
	}
		
	function removeElem(element){
		var elcount =  element.name.replace(/destclose_/g, "");
		var count = 0;
		for(var i=0; i< document.getElementById("dest_form").childNodes.length; i++){
			if(document.getElementById("dest_form").childNodes[i].nodeName == "DIV"){
				count++;
				}
		}
		for(var i =0; i<element.parentNode.childNodes.length; i++){
			if(element.parentNode.childNodes[i].nodeName =="DIV" && element.parentNode.childNodes[i].id == "date"){
				document.getElementById("dest_edit_form").insertBefore(element.parentNode.childNodes[i], document.getElementById("dest_form"));
			}
		}
		document.getElementById("dest_form").removeChild(document.getElementById(element.parentNode.id));
		for(var i=elcount; i<count-1;i++){
			var newstr1 ="dest_form_"+ i;
			var newstr2 = "distance_"+ i;
			var newstr3 = "duration_"+ i;
			var newstr4 = "add_"+i;
			var newstr5 = "destclose_"+i;
			var newstr6 = i+ ".Ziel: ";
			i++;
			var oldstr1 ="dest_form_"+ i;
			var oldstr2 = "distance_"+ i;
			var oldstr3 = "duration_"+ i;
			var oldstr4 = "add_"+i;
			var oldstr5 = "destclose_"+i;
			var oldstr6 = i+ ".Ziel: ";
			document.getElementById(oldstr1).id = newstr1;
			document.getElementById(oldstr2).id = newstr2;
			document.getElementById(oldstr3).id = newstr3;
			document.getElementById(oldstr4).id = newstr4;
			document.getElementsByName(oldstr5)[0].name = newstr5;
			i--;
		}
		calcRoute();
	}
	
	function newDestinationAdd(){
		var Anzahl = document.getElementById("dest_form").childNodes.length;
		var anz = 0;
		for(var i=0; i< Anzahl; i++){
			if(document.getElementById("dest_form").childNodes[i].nodeName == "DIV"){
				anz++;
				}
		}
		var DIVknoten = document.createElement("div");
		var idString = "dest_form_" + anz;
		DIVknoten.setAttribute("id", idString);		
		DIVknoten.setAttribute("class", "dest_form");
			var textareaDistKnoten = document.createElement("textarea");
			var textareaDistIdStr = "distance_" + anz;
			textareaDistKnoten.setAttribute("id", textareaDistIdStr);	
			textareaDistKnoten.setAttribute("style", "display:none");	
			textareaDistKnoten.setAttribute("disabled", "disabled");
			var brKnoten1 = document.createElement("br");			
			var textareaDuraKnoten = document.createElement("textarea");
			var textareaDuraIdStr = "duration_" + anz;
			textareaDuraKnoten.setAttribute("id", textareaDuraIdStr);
			textareaDuraKnoten.setAttribute("style", "display:none");
			textareaDuraKnoten.setAttribute("disabled", "disabled");
			var brKnoten2 = document.createElement("br");
		DIVknoten.appendChild(textareaDistKnoten);
		DIVknoten.appendChild(brKnoten1);
		DIVknoten.appendChild(textareaDuraKnoten);
		DIVknoten.appendChild(brKnoten2);
			var LabelKnoten = document.createElement("label");
			var InputKnoten = document.createElement("input");
			var inputIdStr = "add_" + anz;
			LabelKnoten.setAttribute("for",inputIdStr);
				var textStr = anz + ".Ziel:  ";
				var TextKnoten  = document.createTextNode(textStr);
			LabelKnoten.appendChild(TextKnoten);
		DIVknoten.appendChild(LabelKnoten);
			InputKnoten.setAttribute("id", inputIdStr);
			InputKnoten.setAttribute("type", "text");
			InputKnoten.setAttribute("onkeydown", "TasteGedrueckt(event)");
		DIVknoten.appendChild(InputKnoten);	
			var Buttstr ="destclose_" + anz;
			ButtKnoten = document.createElement("button");
			ButtKnoten.setAttribute("name", Buttstr);
			ButtKnoten.setAttribute("type", "button");
			ButtKnoten.setAttribute("onclick", "removeElem(this)");
				var buttTextKnoten = document.createTextNode("x");
				ButtKnoten.appendChild(buttTextKnoten);
		DIVknoten.appendChild(ButtKnoten);	
		var bdatestr ="date_" + anz;
			bDateKnoten = document.createElement("button");
			bDateKnoten.setAttribute("name", bdatestr);
			bDateKnoten.setAttribute("type", "button");
			bDateKnoten.setAttribute("onclick", "date(this)");
				var bDateTextKnoten = document.createTextNode("(:)");
				bDateKnoten.appendChild(bDateTextKnoten);
		DIVknoten.appendChild(bDateKnoten);	
		var spanNode =document.createElement("span");
		DIVknoten.appendChild(spanNode);
		document.getElementById("dest_form").appendChild(DIVknoten);
	}
	
	function date(element){
		var dialog = document.getElementById("date");
		document.getElementById(element.parentNode.id).appendChild(dialog);
		dialog.style.display = "inline";
		 var date = document.getElementById("date_cal");
		 var now = new Date();
		 var month = now.getMonth()+1;
		 if(month<10)
			month= "0" + month;
		date.value = now.getDate()+ "." + month +"." + now.getFullYear(); 
		selectTimeFill();
	}
	
	function selectTimeFill(){
		for(var i=6; i< 24; i++){
		var str;
			for(var j=0; j < 6;j++){
				if(i<10){
					str = "0"+i +":"+j+"0";
				}else{
					str = i+":"+j+"0";
				}
				var optionNodeB = document.createElement("option");
				optionNodeB.setAttribute("value", str);
				var textNodeB = document.createTextNode(str);
				optionNodeB.appendChild(textNodeB);
				var optionNodeE = document.createElement("option");
				optionNodeE.setAttribute("value", str);
				var textNodeE = document.createTextNode(str);
				optionNodeE.appendChild(textNodeE);
				document.getElementById("time_begin").appendChild(optionNodeB);
				document.getElementById("time_end").appendChild(optionNodeE);
			}
		}
	}
	
	
	function date_Ok(element){
		
		if(document.getElementById("date_cal").value ==""){
			alert("Datum auswaehlen !");
			document.getElementById("date_cal").focus();
			return;
		}
		var str;
		var date = document.getElementById("date_cal").value;
		if(document.dest_edit_form.date_time[0].checked == true){
		str= date+ "    von ";
			for(var i =0; i<document.dest_edit_form.time_begin.length; i++ ){
				if(document.dest_edit_form.time_begin.options[i].selected ==true){
					str += document.dest_edit_form.time_begin.options[i].value;
				}
			}
		str += "  bis ";
			for(var i =0; i<document.dest_edit_form.time_end.length; i++ ){
				if(document.dest_edit_form.time_end.options[i].selected ==true){
					str += document.dest_edit_form.time_end.options[i].value;
				}
			}		
		}else{
			str = date + "   " +document.dest_edit_form.date_time[1].value;
			
		}
		
		var elparent = element.parentNode;
		var i=1;
		while(elparent.id !="date"){
			elparent = elparent.parentNode;
			i++;
		}
		for(var i =0; i<elparent.parentNode.childNodes.length; i++){
			if(elparent.parentNode.childNodes[i].nodeName =="SPAN"){
				elparent.parentNode.childNodes[i].textContent = str;
				elparent.parentNode.childNodes[i].style.display = "inline";
			}
		}
		document.getElementById("date").style.display = "none";
	}

	function date_Cancel(){
		document.getElementById("date").style.display = "none";
	}

	function sel(element){
		var dialog = document.getElementById("select_mode");
		document.getElementById(element.parentNode.id).appendChild(dialog);
		dialog.style.display = "inline";
	}
	function select_Ok(el){
		var str = "Abreise: ";
		if(document.dest_edit_form.mode[0].checked == true){
			str += document.dest_edit_form.mode[0].nextSibling.textContent;
		}else if(document.dest_edit_form.mode[1].checked == true){
			str += document.dest_edit_form.mode[1].nextSibling.textContent;	
		}else if(document.dest_edit_form.mode[2].checked == true){
			str += document.dest_edit_form.mode[2].nextSibling.textContent;
		}
		for(var i =0; i<document.getElementById("dest_form_0").childNodes.length; i++){
			if(document.getElementById("dest_form_0").childNodes[i].nodeName =="SPAN"){
				document.getElementById("dest_form_0").childNodes[i].textContent = str;
			}
		}
		document.getElementById("select_mode").style.display = "none";
	}
	
	function select_Cancel(){
		document.getElementById("select_mode").style.display = "none";
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	