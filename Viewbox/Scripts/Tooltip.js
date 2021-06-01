jQuery(function() {
    /**************************************************************************
    * Tooltip
    **************************************************************************/
    jQuery("#DataGrid a.csvexport, #DataGrid a.ideaexport, #DataGrid a.pdfexport, #DataGrid a.sum, #DataGrid a.enlarge, #DataGrid a.strukturOpen, #DataGrid a.strukturClose, #TableList .functions a.enlarge, #ViewList .functions a.enlarge, #IssueList .functions a.enlarge, .boxheader #right .button, .functions .pdfexport-overview, #extfilter .operators .op-symbol, .right-switch, .alma, #viewoptions .distinct-info, #listvisibleoptions .distinct-info, .tabletyperight-switch, .userlogright-switch, #notification .button, .viewoptions-enlarge, #extfilter .dbnull :first(div),.listarchiveoptionsselectall,.listarchiveoptionsselectnone, .onereportparamset-header .exactmatch, #DataGrid .header .backlink, a.infobtn").on("mousemove", function (e) {
            var tooltip = jQuery(this).next().clone();
            var body_tooltip = jQuery('body > .tooltip-container');
            if (body_tooltip.length > 0) tooltip = body_tooltip;
            else tooltip.appendTo('body');
            if (tooltip.hasClass('hide')) tooltip.removeClass('hide');
            var screenWidth = $('body').width();
            var screenHeight = $("body").height();

            if (screenHeight > e.pageY + tooltip.height() + 20) {
                tooltip.css("top", e.pageY + 10);
            } else {
                tooltip.css("top", e.pageY - tooltip.height() - 10);
            }

            if (screenWidth > e.pageX + tooltip.width() + 20) {
                tooltip.css("left", e.pageX + 10);
            } else {
                tooltip.css("left", e.pageX - tooltip.width() - 10);
            }
        });
    jQuery("#DataGrid a.csvexport, #DataGrid a.ideaexport, #DataGrid a.pdfexport, #DataGrid a.sum, #DataGrid a.enlarge,#DataGrid a.strukturOpen, #DataGrid a.strukturClose,  #TableList .functions a.enlarge, #ViewList .functions a.enlarge, #IssueList .functions a.enlarge, .boxheader #right .button, .functions .pdfexport-overview, #extfilter .operators .op-symbol, .right-switch, .alma,  #viewoptions .distinct-info, #listvisibleoptions .distinct-info, .tabletyperight-switch, .userlogright-switch , #notification .button, .viewoptions-enlarge, #extfilter .dbnull :first(div),.listarchiveoptionsselectall,.listarchiveoptionsselectnone, .onereportparamset-header .exactmatch,#DataGrid .header .backlink, a.infobtn").on("mouseout", function () {
            var tooltip = jQuery(this).next();
            var body_tooltip = jQuery('body > .tooltip-container');
            if (body_tooltip.length > 0) tooltip = body_tooltip.remove();
            if (!tooltip.hasClass('hide')) tooltip.addClass('hide');
        });
    jQuery("#viewoptions .dropdown-btn, #extsortoptions .dropdown-btn, #extsummaoptions .dropdown-btn, #extfilteroptions .dropdown-btn, #subtotaloptions .dropdown-btn").on("mousemove", function (e) {
        var tooltip = jQuery(this).prev().clone();
        var body_tooltip = jQuery('body > .tooltip-container');
        if (body_tooltip.length > 0) tooltip = body_tooltip;
        else tooltip.appendTo('body');
        if (tooltip.hasClass('hide')) tooltip.removeClass('hide');

        var screenWidth = $('body').width();
        var screenHeight = $("body").height();

        if (screenHeight > e.pageY + tooltip.height() + 20) {
            tooltip.css("top", e.pageY + 10);
        } else {
            tooltip.css("top", e.pageY - tooltip.height() - 10);
        }

        if (screenWidth > e.pageX + tooltip.width() + 20) {
            tooltip.css("left", e.pageX + 10);
        } else {
            tooltip.css("left", e.pageX - tooltip.width() - 10);
        }
    });
    jQuery("#viewoptions .dropdown-btn, #extsortoptions .dropdown-btn, #extsummaoptions .dropdown-btn, #extfilteroptions .dropdown-btn, #subtotaloptions .dropdown-btn").on("mouseout", function () {
        var tooltip = jQuery(this).prev();
        var body_tooltip = jQuery('body > .tooltip-container');
        if (body_tooltip.length > 0) tooltip = body_tooltip.remove();
        if (!tooltip.hasClass('hide')) tooltip.addClass('hide');
    });

    jQuery(document).on("mouseover", "#ControlCenter .overview.jobs tr, #Export .overview.jobs tr", function (e) {
        var tooltip = jQuery(this).find('.tooltip-container').clone();
        var body_tooltip = jQuery('body > .tooltip-container');
        if (body_tooltip.length > 0) tooltip = body_tooltip;
        else tooltip.appendTo('body');
        if (tooltip.hasClass('hide')) tooltip.removeClass('hide');

        var screenWidth = $('body').width();
        var screenHeight = $("body").height();

        if (screenHeight > e.pageY + tooltip.height() + 20) {
            tooltip.css("top", e.pageY + 10);
        } else {
            tooltip.css("top", e.pageY - tooltip.height() - 10);
        }

        if (screenWidth > e.pageX + tooltip.width() + 20) {
            tooltip.css("left", e.pageX + 10);
        } else {
            tooltip.css("left", e.pageX - tooltip.width() - 10);
        }
    });
    jQuery(document).on("mouseout", "#ControlCenter .overview.jobs tr, #Export .overview.jobs tr", function () {
        var tooltip = jQuery(this).find('.tooltip-container');
        var body_tooltip = jQuery('body > .tooltip-container');
        if (body_tooltip.length > 0) tooltip = body_tooltip.remove();
        if (!tooltip.hasClass('hide')) tooltip.addClass('hide');
    });
});