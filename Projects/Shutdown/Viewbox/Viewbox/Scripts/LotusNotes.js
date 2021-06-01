jQuery(function () {

    $('.filterFor a').click(function () {
        var filter = $(this).parent().find("#searchWord").val();
        window.location.href = "/LotusNotes/Index?page=1&size=35&filter="+filter;
    });

    function InitSize()
    {
        /* Header widths*/
        var headerSorterWidth = $('.header-sorterfeld').width();
        var headerBUWidth = $('.header-bu').width();
        var headerDokartWidth = $('.header-dokart').width();
        var headerKategorieWidth = $('.header-kategorie').width();
        var headerDokdatumWidth = $('.header-dokdatum').width();
        var headerKwtermiWidth = $('.header-kwtermin').width();
        var headerAuftragsNrSapWidth = $('.header-auftragsnrsap').width();
        var headerProduktWidth = $('.header-produkt').width();
        var headerBetreffWidth = $('.header-betreff').width();
        var headerDetailsWidth = $('.header-details').width();

        /* Set widths*/
        $('.sorterfeld').width(headerSorterWidth);
        $('.bu').width(headerBUWidth);
        $('.dokart').width(headerDokartWidth);
        $('.kategorie').width(headerKategorieWidth);
        $('.dokdatum').width(headerDokdatumWidth);
        $('.kwtermin').width(headerKwtermiWidth);
        $('.auftragsnrsap').width(headerAuftragsNrSapWidth);
        $('.produkt').width(headerProduktWidth);
        $('.betreff').width(headerBetreffWidth);
        $('.details').width(headerDetailsWidth);

        /* Set Paddings*/
        $('.bu').css('padding-left', headerSorterWidth);
        $('.dokart').css('padding-left', headerSorterWidth + headerBUWidth);
        $('.kategorie').css('padding-left', headerSorterWidth + headerDokartWidth + headerBUWidth + 2);
        $('.dokdatum').css('padding-left', headerSorterWidth + headerDokartWidth + headerBUWidth + headerKategorieWidth + 4);

        /* Set heights */
        $(".node div").each(function ()
        {
            if ($(this).height() > 20 && !$(this).parent().hasClass('level-1'))
            {
                $(this).parent().height($(this).height());
            }
        });

        var contentBoxHeight = $('.content-box').height();
        $('.lotusNotes-container').height(contentBoxHeight * 0.91);
    }

    $(window).resize(function () {
        InitSize();
    });

    $(".sorterfeld, .bu, .kategorie, .dokart").click(function () {
        if ($(this).parent().find('.children').css("display") == "none") {
            var that = $(this);
            $(this).parent().find('.children').fadeIn(500, function ()
            {
                that.removeClass('closed');
                that.addClass('open');
            });
            
        }
        else {
            var that = $(this);
            var children = $(this).parent().find('.children');

            children.fadeOut(500, function () {

                that.removeClass('open');
                that.addClass('closed');
            });
        }
    });    

    InitSize();

    $(".node.level-5 a").click(function () {
        var url = "/LotusNotes/EmailDetails?unid=" + $(this).parent().find('#notesUnid').val();
        $.get(url, function (data) {
            $('#lotus-details-dialog').html(data);
            $('#lotus-details-dialog').css('margin-top', '-900px');
            $('#lotus-details-shadow').fadeIn(750, function ()
            {
                $('#lotus-details-dialog').show(500);
            });
        });
    });

    $('#lotus-details-shadow').click(function () {
        $('#lotus-details-dialog').hide(500, function () {
            $('#lotus-details-shadow').fadeOut(750);
        });
    });

    $('#change-page').change(function ()
    {
        var filter = $('#searchWord').val();
        var page = $(this).val();
        window.location = '/LotusNotes/Index/?page=' + page + '&filter=' + filter;
    });

    $(document).on("click", "#downloadAttachment", function () {
        $(this).parent().find('form').submit();        
    });
});