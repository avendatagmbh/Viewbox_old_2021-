$(document).ready(function () {
    var timer;
    var finished = false;

    if (!finished) {
        timer = setInterval(function() {
            var jobKey = $("#jobKey").val();
            updateInformation(jobKey);
        }, 1000);
    }

    var updateInformation = function (jobKey) {
        var actionUrl = "/Documents/ConversionStatusInformations?jobKey=" + jobKey;
        $.getJSON(actionUrl, displayData);
    };

    function displayData(response) {
        if (response != null) {
            for (var i = 0; i < response.length; i++) {
                $("#job-state").html(response[i].Status);
                $("#document-progress").attr("value", parseInt(response[i].DocumentIndex));
                $("#document-progress").attr("max", parseInt(response[i].DocumentCount));
                $("#document-count").html(response[i].DocumentCount + '/' + response[i].DocumentIndex);
                if (parseInt(response[i].PageIndex) == 0 && parseInt(response[i].PageIndex) == 0) {
                    $("#converter-progress").hide();
                    $("#page-count-text").hide();
                    $("#page-count").hide();
                } else {
                    $("#converter-progress").show();
                    $("#page-count-text").show();
                    $("#page-count").show();
                    $("#converter-progress").attr("value", parseInt(response[i].PageIndex));
                    $("#converter-progress").attr("max", parseInt(response[i].PageCount));
                }
                $("#page-count").html(response[i].PageCount + '/' + response[i].PageIndex);
                if (response[i].Status != "Finished" && response[i].Status != "Crashed") {
                    finished = false;
                } else {
                    finished = true;
                    clearInterval(timer);
                }
            }
        }
    }
});
//@ sourceURL=Converter.js