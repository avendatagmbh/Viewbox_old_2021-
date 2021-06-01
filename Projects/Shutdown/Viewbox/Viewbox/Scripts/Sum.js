$(document).ready(function() {
    var summaTabs = [
        ['summaHeader', 'summaContent'],
        ['summaSelectColumnsHeader', 'summaSelectColumnsContent']];
    for (var i = 0; i < summaTabs.length; i++) {
        var header = summaTabs[i][0];
        $("#" + header).click(function() {
            for (var j = 0; j < summaTabs.length; j++) {
                var headerName = summaTabs[j][0];
                var contentName = summaTabs[j][1];
                var content = $("#" + contentName);
                if (headerName == this.id) {
                    content.show();
                } else {
                    content.hide();
                }
            }
        });
    }
    $("#refreshSums").click(function() {
        jQuery.post("DataGrid/SummarizeColumns",
            { tableName: $("#tableName").val(), columnNames: decimalColumns },
            function(data) {
                console.debug($('#summaTable > tbody > tr'));
                $('#summaTable > tbody > tr').remove();
                for (var k = 0; k < data.length; k++) {
                    var column = data[k].Key;
                    var sum = data[k].Value;
                    $('#summaTable > tbody').html('<tr><td>' + column + '</td><td>' + sum + '</td></tr>');
                }
            });
    });

});