var calculateFullWidth = function () {
    if ($(".inner-container2").get(0) != undefined) {
        var origiWitdh = ($('.datagrid.original-table.datagridFooter').width());
        var fullWidth = 0;

        $(".ui-resizable").each(function () {
            if (!$(this).hasClass("full-width") && !$(this).hasClass("hide")) {
                fullWidth += parseInt($(this).css("width") + 1);
            }
        });
        //table-body
        var browser_info = get_browser_info();
        if (browser_info.name = "MSIE" && browser_info.version < 11 && origiWitdh < fullWidth) {
            origiWitdh += fullWidth;
        }

        if (fullWidth > $(".inner-container2").width()) {
            $('.table-body').css("width", origiWitdh);
            $('.datagrid.original-table.datagridHeader').css("width", origiWitdh - fullWidth);
            $('.datagrid.original-table.datagridFooter').css("width", origiWitdh - fullWidth);
            $('.table-header').css("width", origiWitdh);
        }
        else {

            var offset = 2; //wonderful magicnumber 
            try {
                if (jQuery(".inner-container2").hasScrollBar().vertical
                && origiWitdh < $(".inner-container2").width()) {
                    offset = 19; //wonderful magicnumber 
                }
                if (!jQuery(".inner-container2").hasScrollBar().vertical) {
                    if (origiWitdh > $(".inner-container2").width()) {
                        offset = -15; //wonderful magicnumber
                    }
                    else {
                        offset = 2; //wonderful magicnumber
                    }
                }
            } catch (err) { }


            $('.table-body').css("width", $(".inner-container2").width() - offset);
            $('.datagrid.original-table.datagridHeader').css("width", $('.inner-container2').width() - offset);
            $('.datagrid.original-table.datagridFooter').css("width", $('.inner-container2').width() - offset);
            $('.table-header').css("width", $('.inner-container2').width() - offset);
        }
    }
};

function get_browser_info() {
    var ua = navigator.userAgent, tem, M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
    if (/trident/i.test(M[1])) {
        tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
        return { name: 'IE', version: (tem[1] || '') };
    }
    if (M[1] === 'Chrome') {
        tem = ua.match(/\bOPR\/(\d+)/)
        if (tem != null) { return { name: 'Opera', version: tem[1] }; }
    }
    M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
    if ((tem = ua.match(/version\/(\d+)/i)) != null) { M.splice(1, 1, tem[1]); }
    return {
        name: M[0],
        version: M[1]
    };
};

var UBalance = new function () {
    var getScrollbarWidth = function () {
        var outer = document.createElement("div");
        outer.style.visibility = "hidden";
        outer.style.width = "100px";
        outer.style.msOverflowStyle = "scrollbar"; // needed for WinJS apps

        document.body.appendChild(outer);

        var widthNoScroll = outer.offsetWidth;
        // force scrollbars
        outer.style.overflow = "scroll";

        // add innerdiv
        var inner = document.createElement("div");
        inner.style.width = "100%";
        outer.appendChild(inner);

        var widthWithScroll = inner.offsetWidth;

        // remove divs
        outer.parentNode.removeChild(outer);

        return widthNoScroll - widthWithScroll;
    }

    var calculatedWidth = -1;
    var percentWidth = false;
    var setBody = function () {
        // set height of .ubalance-view
        $('.ubalance-view').height($('.table-container').height() - $('.table-parameters').outerHeight(true) - $('.ubalance-header').outerHeight(true));
        var scrollBarWidth = getScrollbarWidth();

        if ($('.ubalance-cols .ubalance-col').length > 2) {
            // save width calculated by backend
            if (calculatedWidth == -1)
                calculatedWidth = $('.ubalance-tree').width();

            var viewWidth = $('.ubalance-view').width();
            var neededSpace = calculatedWidth;
            // Add scrollbar width
            if ($('.ubalance-view').height() < $('.ubalance-tree').height())
                neededSpace += scrollBarWidth;

            // Set width to 100% if the width of the view is bigger than the width of the tree
            if (!percentWidth && viewWidth > neededSpace) {
                $('.ubalance-tree').css('width', '100%');
                $('.ubalance-cols').css('width', '100%');
                percentWidth = true;
            }
            // Change width to px value to indicate horizontal scrollbar
            else if (percentWidth && viewWidth < neededSpace) {
                $('.ubalance-tree').width(calculatedWidth);
                $('.ubalance-cols').width(calculatedWidth);
                percentWidth = false;
            }
        }
        else
            percentWidth = true;

        if ($('.ubalance-cols .ubalance-col').length >= 2) {
            // depending on whether there is vertical overflow set padding right to .ubalance-cols or to the last column
            // depending on how the width of the container is set (if 100% then padding-right won't work) 
            $('.ubalance-cols .ubalance-col').first().next().css('paddingRight', 0);
            $('.ubalance-cols').css('paddingRight', 0);

            if ($('.ubalance-view').height() < $('.ubalance-tree').height()) {
                if (percentWidth)
                    $('.ubalance-cols .ubalance-col').first().next().css('paddingRight', scrollBarWidth);
                else
                    $('.ubalance-cols').css('paddingRight', scrollBarWidth);
            }
            }

        $('.ubalance-header').scrollLeft($('.ubalance-view').scrollLeft());
        }

    var hasSelection = function () {
        if (typeof window.getSelection != "undefined") {
            var sel = window.getSelection();
            if (sel.rangeCount && $(sel.anchorNode).closest('div.ubalance-view').length) {
                var container = document.createElement("div");
                for (var i = 0, len = sel.rangeCount; i < len; ++i) {
                    container.appendChild(sel.getRangeAt(i).cloneContents());
                }
                return container.innerHTML != "";
            }
        }
        else if (typeof document.selection != "undefined") {
            var sel = document.selection;
            if (sel.type == "Text") {
                return $(sel.createRange().parentElement()).closest('div.ubalance-view').length;
            }
        }
        return false
    }
    //Onereportparamset DIV element - Set top value attribute
    $(document).ready(function () {        
        var height = $('.onereportparamset-header').outerHeight();
        $('.onereportparamset').css('top', height + "px");
    });

    $(document).ready(function () {
        if ($('div.ubalance-view').length) {
            setBody();

            $('.ubalance-view').scroll(function () {
                $('.ubalance-header').scrollLeft($('.ubalance-view').scrollLeft());
            });

            $(window).resize(setBody);

            $('.ubalance-node').click(function () {
                // Node which has no children
                if (!$(this).hasClass('ubalance-node-opened') && !$(this).hasClass('ubalance-node-closed'))
                    return false;

                if (hasSelection())
                    return false;

                if ($(this).hasClass('ubalance-node-opened')) {
                    $(this).removeClass('ubalance-node-opened');
                    $(this).addClass('ubalance-node-closed');
                }
                else {
                    $(this).removeClass('ubalance-node-closed');
                    $(this).addClass('ubalance-node-opened');
                }

                setBody();
                return false;
            });

            jQuery(document).on("click", "#DataGrid a.iconStructur.strukturClose", function () {
                $('.ubalance-node-opened').each(function () {
                    $(this).removeClass('ubalance-node-opened');
                    $(this).addClass('ubalance-node-closed');
                });
                setBody();
                return false;
            });

            jQuery(document).on("click", "#DataGrid a.iconStructur.strukturOpen", function () {
                $('.ubalance-node-closed').each(function () {
                    $(this).removeClass('ubalance-node-closed');
                    $(this).addClass('ubalance-node-opened');
                });
                setBody();
                return false;
            });
        }
    });
}

$(function () {
    jQuery.ajaxSetup({
        traditional: true,
        cache: false
    });
    Updater = {
        Listeners: {},
        Pull: function () {
            var maxEntriesLatest = jQuery('#notification-container .content_latest .max-entries').text();
            var maxEntriesHistory = jQuery('#notification-container .content_history .max-entries').text();
            if (maxEntriesLatest == "0") maxEntriesLatest = 5;
            if (maxEntriesHistory == "0") maxEntriesHistory = 5;
            jQuery.get('/Command/GetUpdates', { maxEntriesLatest: maxEntriesLatest, maxEntriesHistory: maxEntriesHistory }, function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                var $html = jQuery(html);
                jQuery('#notification-container .content_latest').html($html.find('.content_latest').html());
                jQuery('#notification-container .content_history').html($html.find('.content_history').html());
                var $list = jQuery(html).find('.content_latest .box');
                $list.each(function (i, e) {
                    var $notification = jQuery(e);
                    var key = $notification.find('.key').text();
                    var type = $notification.find('.type').text();
                    if (Updater.Listeners[key]) {
                        jQuery.get("/Command/RemoveJob", { key: key }, function () {
                            Updater.Listeners[key]($notification);
                            if (type === "DownloadBlueline")
                                jQuery("#dialog").hide();
                        });
                    }
                });
                var count = parseInt(jQuery(html).find('.count-latest').text());
                if (count > 0) {
                    var old_count = jQuery('#notification .count .count-middle').text();
                    jQuery('#notification .count .count-middle').text(count);
                    jQuery('#notification .count').removeClass('hide');
                    jQuery('#notification .count').removeClass('hideimportant');
                    if (old_count < count) { jQuery('#notification .count').cyclicFade({ repeat: 3 }); }
                } else {
                    jQuery('#notification .count .count-middle').text('');
                    jQuery('#notification .count').addClass('hide');
                    jQuery('#notification .count').addClass('hideimportant');
                }
            });

            if (window.UpdaterInterval && window.UpdaterInterval != '') {
                if (window.UpdaterInterval.toLowerCase() != 'false') {
                    setTimeout('Updater.Pull()', window.UpdaterInterval);
                }
            } else {
                setTimeout('Updater.Pull()', 5000);
            }
        }
    };
    Updater.Pull();
    function goBack() {
        //window.location.href = document.referrer;
        var html = jQuery(".isback").html();
        if (html == "false") {
            var pos = window.location.href.search(new RegExp('[?&]'));
            var pos2 = window.location.href.search(new RegExp('[#]'));

            window.location.href = (pos2 > 0 ? window.location.href.replace("#", "") : window.location.href) + (pos > 0 ? "&back=true" : "?back=true");
            jQuery(".isback").replaceWith("<div class=\"isback\" hidden=\"hidden\">true</div>");
        }
        return false;
    }
    var scrollSave = function () {
        var previousScrollLeft;
        jQuery("#DataGrid .table-scroll").scroll(function () {
            jQuery(document).trigger('click.extendedpanel');
            previousScrollLeft = $(this).scrollLeft();
        });
    };

    (function ($) {
    $.fn.hasScrollBar = function () {
        var hasScrollBar = {
    }, e = this.get(0);
            hasScrollBar.vertical = (e.scrollHeight > e.clientHeight) ? true : false;
            hasScrollBar.horizontal = (e.scrollWidth > e.clientWidth) ? true : false;
        return hasScrollBar;
        }
    })(jQuery);

    var calculateFullWidth = function () {
        if ($(".inner-container2").get(0) != undefined)
        {
        var origiWitdh = ($('.datagrid.original-table.datagridFooter').width());
        var fullWidth = 0;

        $(".ui-resizable").each(function () {
            if (!$(this).hasClass("full-width") && !$(this).hasClass("hide")) {
                fullWidth += parseInt($(this).css("width") + 1);
            }
        });
            //table-body
            var browser_info = get_browser_info();
            if (browser_info.name = "MSIE" && browser_info.version < 11 && origiWitdh < fullWidth) {
                origiWitdh += fullWidth;
            }

        if (fullWidth > $(".inner-container2").width()) {
            $('.table-body').css("width", origiWitdh);
                $('.datagrid.original-table.datagridHeader').css("width", origiWitdh - fullWidth);
                $('.datagrid.original-table.datagridFooter').css("width", origiWitdh - fullWidth);
            $('.table-header').css("width", origiWitdh);
        }
        else {

            var offset = 2; //wonderful magicnumber 
            try {
                if (jQuery(".inner-container2").hasScrollBar().vertical
                && origiWitdh < $(".inner-container2").width()) {
                    offset = 19; //wonderful magicnumber 
                }
                if (!jQuery(".inner-container2").hasScrollBar().vertical) {
                    if (origiWitdh > $(".inner-container2").width()) {
                        offset = -15; //wonderful magicnumber
                    }
                    else {
                        offset = 2; //wonderful magicnumber
                    }
                }
            } catch (err) { }


            $('.table-body').css("width", $(".inner-container2").width() - offset);
                $('.datagrid.original-table.datagridHeader').css("width", $('.inner-container2').width() - offset);
            $('.datagrid.original-table.datagridFooter').css("width", $('.inner-container2').width() - offset);
            $('.table-header').css("width", $('.inner-container2').width() - offset);
        }
        }
    
    };
    function get_browser_info() {
        var ua = navigator.userAgent, tem, M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
        if (/trident/i.test(M[1])) {
            tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
            return { name: 'IE', version: (tem[1] || '') };
        }
        if (M[1] === 'Chrome') {
            tem = ua.match(/\bOPR\/(\d+)/)
            if (tem != null) { return { name: 'Opera', version: tem[1] }; }
        }
        M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
        if ((tem = ua.match(/version\/(\d+)/i)) != null) { M.splice(1, 1, tem[1]); }
        return {
            name: M[0],
            version: M[1]
        };
    }
    jQuery(document).ready(function () {
        calculateFullWidth();
        setTableBody();
        $('.jqueryUITooltip').tooltip();
    });
    $(window).resize(function () {
        calculateFullWidth();
        setTableBody();
    });

    function setTableBody() {
        //!!Important: If you need to modify this code, please modify this function in the search.js file.
        if ($(".inner-container2").get(0) != undefined) {
            var tableParamHeight = $('.table-container').find('.table-parameters').outerHeight(true);
            $(".table-scroll").height($(".table-container").height() - tableParamHeight);
        }
        if ($(".balance-container").get(0) != undefined) {
            var outerHeight = $('.content-outer').height();
            var tableParamHeight = $('.table-container').find('.table-parameter .left').parent().parent().height() + 16;
            var pxx = Math.round((tableParamHeight - 37) / 23) * 23;
            $('.content-inner').height(outerHeight - pxx);
            var fullHeight = $('.balance-container').height();
            var headerHeight = $('.balance-parameters').outerHeight(true);
            var headerHeight2 = $('.balance-header').outerHeight(true);

            $('.table-scroll').height(fullHeight - headerHeight - headerHeight2);
            $(".table-header").css('z-index', '10');
        }
        if ($("#tabContentTableId").get(0) != undefined) {
            $("#tabContentTableId").height($(".content-inner").height()
                - $(".header").height()
                - $(".tabbar").height()
                - $("#navigation-row-0").height()
                - $(".overview.exportables.fixedHeader  tr").height() - 10);
        }

        $("#dialog").width($("#gesamt").width());
    }

    //scrollSave();
    var long_running_options;
    var viewoptions_loaded;
    var refreshViewOptionsPartial;
    var refreshExtendedSortPartial;
    var refreshExtendedSummaPartial;
    var refreshExtendedFilterPartial;
    var datagrid_loaded;

    jQuery(document).on("click", "#DataGrid .nav-controls a.iconx", function () {
        doPaging($(this));
        return false;
    });

    var doPaging = function (htmlElement)
    {
        var currentleft = jQuery("#DataGrid .table-scroll").scrollLeft();
        var $waitdialog = null;
        if ($(htmlElement).hasClass('counting')) {
            $waitdialog = jQuery('#wait-dlg-counting');
        } else {
            $waitdialog = jQuery('#wait-dlg-page');
        }

        var actualPage = $("#startpage").val();
        var length = parseInt($("#displayrowcount").val());
        var maxCount = parseInt($("span.count").text().replace('.', ''));
        var href = $(htmlElement).attr('href');


        switch (true) {
            case $(htmlElement).hasClass("next"):
                {
                    window.history.pushState(null, "", '?start=' + (parseInt(actualPage) * length));
                    break;
                }
            case $(htmlElement).hasClass("back"):
                {
                    window.history.pushState(null, "", '?start=' + ((parseInt(actualPage) - 2) * length));
                    break;
                }
            case $(htmlElement).hasClass("first"):
                {
                    window.history.pushState(null, "", '?start=0');
                    break;
                }
            case $(htmlElement).hasClass("last"):
                {
                    window.history.pushState(null, "", '?start=' + ((maxCount % length == 0) ? (maxCount - length) : (maxCount - (maxCount % length))));
                    break;
                }
            case $(htmlElement).hasClass("pagecount"):
                {
                    href = $(htmlElement).attr('link') + "?start=" + parseInt($('#start').val());
                    window.history.pushState(null, "", '?start=' + parseInt($('#start').val()));
                    break;
        }
            case $(htmlElement).hasClass("rowcount"):
                {
                    href = $(htmlElement).attr('link') + "?size=" + parseInt($('#displayrowcount').val());
                    window.history.pushState(null, "", '?size=' + parseInt($('#displayrowcount').val()));
                    break;
                }
        }


        var $dialog = jQuery('#dialog').html($waitdialog.html()).show();

        jQuery.get(href, { json: true }, function (html) {
            if (html.search("<div id=\"login\">") != -1) {
                window.location = "/Account/LogOn";
            }
            if ($('#wait-dlg-counting').html() == $dialog.html()) {
                $(".nav-controls").html(html);
                $dialog.hide();
            } else {
                jQuery('#DataGrid').html(html);
                var replace = jQuery('#DataGrid .page-header').clone();
                jQuery('#DataGrid .page-header').removeClass("replace");
                replace.insertAfter(jQuery('#DataGrid .page-header'));
                viewoptions_loaded();
                datagrid_loaded();
                vcolResizable();
                $dialog.hide();
                calculateFullWidth();
                setTableBody();
                jQuery("#DataGrid .table-scroll").scrollLeft(currentleft);
            }
        });
        return false;
    };

    jQuery(document).on("keyup", ".rowcount, .pagecount#startpage", function (e) {
        if (e.keyCode == 13) {

            this.value = this.value.replace(/[^0-9]/g, '');
            var parsevalue = $('#pagecount').html().replace(/[\.,]/g, '').replace(/\s+/g, '').replace(/&nbsp;/g, '');
            var pagecount = parseInt(parsevalue);
            var page = $('.rowcount input[name=startpage]');
            if (page.val() > pagecount) {
                page.val(pagecount);
            }
            if ((page.val() != "") && page.val() < 1) {
                page.val(1);
            }

            var start = parseInt($('#start').val());
            var startpage = parseInt($('#startpage').val());
            var original_startpage = parseInt($('#original_startpage').val());
            var rowcount = parseInt($('#displayrowcount').val());

            if (startpage != original_startpage) {
                $('#start').val((startpage - 1) * rowcount);
            }
            else if ((startpage - 1) * rowcount != start) {
                $('#start').val(Math.floor(start / rowcount) * rowcount);
            }

            $('#start2').val($('#start').val());


            //window.location.href = $(this).attr("link") + "?start=" + $('#start').val();
            //$(location).attr('href', $(this).attr("link") + "?start=" + $('#start').val() );

            //$("#DataGrid .nav-controls a.iconx").trigger("click");

            if ($("#pageCount").attr("name") != "pageCount") {
            doPaging(this);
            }
            return false;
        }
    });

    jQuery(document).on("click", "#Documents .nav-controls a.iconx", function () {
        if (jQuery(this).hasClass('counting')) {
            var $waitdialog = jQuery('#wait-dlg-counting');
            var $dialog = jQuery('#dialog').html($waitdialog.html()).show();

            jQuery.get(jQuery(this).attr('href'), { json: true }, function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                var actualPage = parseInt($(".from").text());
                $(".nav-controls").html(html);
                var maxLength = parseInt($("span.count").text().replace(new RegExp(",", 'g'), "").replace(new RegExp("/.", 'g'), ""));
                $("a.iconx.first").attr("href", $("#thisUrl").val());
                $("a.iconx.back").attr("href", $("#thisUrl").val().replace("start=0", "start=" + ((parseInt(actualPage) - 2) * 35)));
                $("a.iconx.next").attr("href", $("#thisUrl").val().replace("start=0", "start=" + (parseInt(actualPage) * 35)));
                $("a.iconx.last").attr("href", $("#thisUrl").val().replace("start=0", "start=" + ((maxLength % 35 == 0) ? (maxLength - 35) : (maxLength - (maxLength % 35)))));
                $dialog.hide();
            });
            return false;
        }

        return true;
    });
    //#DataGrid > div.content-box > div > div > div.header.main-header > div > a.iconStructur.strukturClose
    jQuery(document).on("click", "#DataGrid a.iconStructur.strukturClose", function () {
        if ($('div.ubalance-view').length) {
            $('div.ubalance-view table tr').each(function () {
                var level = parseInt($(this).attr('class').match(/level(\d+)/)[1]);
                if (level != 1)
                    $(this).addClass('hide');
                var firstTD = $(this).find('td:first-child');
                if (firstTD.hasClass('node-opened')) {
                    firstTD.removeClass('node-opened');
                    firstTD.addClass('node-closed');
                }
            });
        }
        else {
            jQuery("#DataGrid div.balance-view.scroll").find("div.tree-node").each(function () {
                if ($(this).children(".tree-node").length > 0)
                    $(this).removeClass("open").addClass("closed");
            });
        }
        return false;
    });
    jQuery(document).on("click", "#DataGrid a.iconStructur.strukturOpen", function () {
        if ($('div.ubalance-view').length) {
            $('div.ubalance-view table tr').each(function () {
                $(this).removeClass('hide');
                var firstTD = $(this).find('td:first-child');
                if (firstTD.hasClass('node-closed')) {
                    firstTD.removeClass('node-closed');
                    firstTD.addClass('node-opened');
                }
            });
        }
        else {
            jQuery("#DataGrid div.balance-view.scroll").find("div.tree-node").each(function () {
                if ($(this).children(".tree-node").length > 0)
                    $(this).removeClass("closed").addClass("open");
            });
        }
        return false;
    });
    //to add the treeview method to the 
    (treeViewFunction = function () {
        //console.debug($.jstree);
        if (!($.jstree === undefined)) {
            $.jstree._themes = "/Content/jquery-jstree/themes/";
            jQuery(".filter-subtree ul:first").treeview({
                control: "#treecontrol"
            });
        }
        jQuery('.hitarea').remove();
        jQuery('.filter-subtree li:first').css('background-image', 'none');
    });
    (datagrid_loaded = function () {
        jQuery("#DataGrid .table-scroll").scroll(function () {
            var noData = jQuery(this).find("td.no-data");
            if (noData.length) {
                var scrollLeft = jQuery(this).scrollLeft();
            }
            var relation = jQuery(".clicknclick-box.active .close a");
            if (relation.length) {
                relation.click();
            }
        });
        jQuery(".placeholder").each(function (i, el) {
            var $el = jQuery(el);
            if ($el.is('input')) {
                if ($el.val() == $el.attr('placeholder')) {
                    $el.addClass('empty').removeClass('dirty');
                } else $el.removeClass('empty').addClass('dirty');
            } else {
                if ($el.text() == $el.attr('placeholder')) {
                    $el.addClass('empty').removeClass('dirty');
                } else $el.removeClass('empty').addClass('dirty');
            }
        });

        jQuery("#data-grid-column-filter .exactmatch a").on("click", function (e) {
            //the exact match checkbox checked
            if ($(e.target).hasClass('checkboxon')) {
                $(e.target).removeClass('checkboxon');
                $(e.target).parent().parent().parent().find('#exactmatch').val('false');
            } else {
                $(e.target).addClass('checkboxon');
                $(e.target).parent().parent().parent().find('#exactmatch').val('true');
            }
            return false;
        });

        jQuery(document).on("click", "#data-grid-column-filter .exactmatch a", function (e) {
            //the exact match checkbox checked
            if (jQuery(this).hasClass('checkboxon')) {
                jQuery(this).removeClass('checkboxon');
                jQuery(this).parent().parent().parent().find('#exactmatch').val('false');
                jQuery(this).stopPropagation();
            } else {
                jQuery(this).addClass('checkboxon');
                jQuery(this).parent().parent().parent().find('#exactmatch').val('true');
                jQuery(this).stopPropagation();
            }
            return false;
        });

        jQuery(document).on("click", ".crud-data-column-val .checkb", function () {
            if ($(e.target).hasClass('checkboxon')) {
                $(e.target).removeClass('checkboxon');
                $(e.target).siblings('input').val('False');
            } else {
                $(e.target).addClass('checkboxon');
                $(e.target).siblings('input').val('True');
            }
        });
        //jQuery(document).on("click", '#data-grid-column-filter .reset-button', function (e) {
        //    var $form = jQuery('#data-grid-column-filter').find('form');
        //    $form.find('#reset').val('true');
        //    $form.submit();
        //});
        jQuery('#data-grid-column-filter .reset-button').on("click", function (e) {
            var $form = jQuery('#data-grid-column-filter').find('form');
            $form.find('#reset').val('true');
            $form.submit();
            $form.find('#reset').val('false');
        });

        //jQuery('#data-grid-column-filter .ok-button').bind('click', function (e) {
        //    var $form = jQuery('#data-grid-column-filter').find('form');
        //    $form.find('#reset').val('false');
        //    $form.submit();
        //});
        jQuery('#table_select').hover(function () {
            jQuery(this).parent().addClass('over');
        }, function () {
            jQuery(this).parent().removeClass('over');
        });
        jQuery('.relation-specific .join-relation-tableobjects').hover(function () {
            jQuery(this).parent().addClass('over');
        }, function () {
            jQuery(this).parent().removeClass('over');
        });
        jQuery('.join-specific .join-relation-tableobjects').hover(function () {
            jQuery(this).parent().addClass('over');
        }, function () {
            jQuery(this).parent().removeClass('over');
        });
        jQuery('input.column1.dropdown,input.column2.dropdown').hover(function () {
            jQuery(this).parent().addClass('over');
        }, function () {
            jQuery(this).parent().removeClass('over');
        });
        jQuery('#column-dropdown,#relation-column-dropdown2,#join-column-dropdown2').hover(function () {
            jQuery(this).parent().addClass('over');
        }, function () {
            jQuery(this).parent().removeClass('over');
        });
        jQuery('input.colids.dropdown,input.colids.dropdown').hover(function () {
            jQuery(this).parent().addClass('over');
        }, function () {
            jQuery(this).parent().removeClass('over');
        });
        jQuery('#column-dropdown,#relation-column-dropdown2,#join-column-dropdown2').hover(function () {
            jQuery(this).parent().addClass('over');
        }, function () {
            jQuery(this).parent().removeClass('over');
        });
        jQuery(document).on("keyup", ".rowcount .pagecount", function () {
            $(this).val($(this).val().replace(/[^0-9]/g, ''));
            var parsevalue = $('#pagecount').html().replace(/[\.,]/g, '').replace(/\s+/g, '').replace(/&nbsp;/g, '');
            var pagecount = parseInt(parsevalue);
            if ($(this).val() > pagecount) {
                $(this).val(pagecount);
            }
            if (($(this).val() != "") && $(this).val() < 1) {
                $(this).val(1);
            }
        });
        //jQuery('.rowcount .pagecount').keypress(function (e) {
        jQuery(document).on("keyup", ".rowcount .pagecount", function (e) {
            if (e.keyCode == 13) {
                var start = parseInt($('#start').val());
                var startpage = parseInt($('#startpage').val());
                var original_startpage = parseInt($('#original_startpage').val());
                var rowcount = parseInt($('#displayrowcount').val());

                if (startpage != original_startpage) {
                    $('#start').val((startpage - 1) * rowcount);
                }
                else if ((startpage - 1) * rowcount != start) {
                    $('#start').val(Math.floor(start / rowcount) * rowcount);
                }

                $('#start2').val($('#start').val());
            }
            else if ((e.keyCode >= 48 && e.keyCode <= 57) || (e.keyCode >= 96 && e.keyCode <= 105)) {
                var rowcount = parseInt($('#displayrowcount').val());
                if (rowcount > 100) {
                    $('#displayrowcount').val(100);
                }
            }
            else {
                e.returnValue = false;
            }
        });
        jQuery("#DataGrid .header .backlink").click(function () {
            return goBack();
        });

        jQuery('.datagrid .column-sort a.ascending, .datagrid .column-sort a.descending').click(function () {
            var newLink = $(this).attr('href');
            if (newLink.indexOf('&scrollPosition=') < 0 && newLink.indexOf('?scrollPosition=') < 0)
                newLink += '&scrollPosition=' + jQuery('#DataGrid .table-scroll').scrollLeft();
            $(this).attr('href', newLink);
        });
        jQuery('.column-menu-ascending, .column-menu-descending').click(function () {
            var newLink = $(this).attr('href');
            if (newLink.indexOf('Descending') > 0) {
                newLink += '&';
            } else {
                newLink += '?';
            }
            newLink += 'scrollPosition=' + jQuery('#DataGrid .table-scroll').scrollLeft();
            $(this).attr('href', newLink);
        });
        //FILTER
        //reads the scrolling position and sends the information to the SortAndFilter method
        jQuery('.datagrid .column-href').click(function () {
            $menu = jQuery(this).parent().nextAll('.column-menu');
            if ($menu.hasClass('hide')) {
                $('.column-menu').addClass('hide');
                $menu.css('left', jQuery(this).parent().parent().width() - 20);
                $menu.removeClass('hide');
                if ($menu.offset().left + 277 > $(document).width()) {
                    $menu.css('left', jQuery(this).parent().parent().width() - 182);
                }
            } else {
                $('.column-menu').addClass('hide');
            }
            return false;
        });
        $('.column-filter-menu').click(function () {
            return false;
        });
        //jQuery('.datagrid a.column-name').click(function () {
        //    //jQuery('#data-grid-column-filter #scrollPosition').val(jQuery('#DataGrid .table-scroll').scrollLeft());
        //});
        //if the scrollPosition information is set then, it will set the 
        jQuery("#DataGrid .table-scroll").scrollLeft(jQuery('#DataGrid .table-scroll .scroll-position').text());

        jQuery(document).on("click", ".datagrid a.extended-info", function () {
            var link = $(this);
            var $panel = jQuery('#extended-information').css({ top: $(this).position().top + 70, left: $(this).position().left - 21 });
            $panel.find("#text").html("");
            link.addClass('active');
            $panel.show();
            jQuery.get($(this).attr('href'), function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                $panel.find("#text").html(html);
            });
            jQuery(document).bind('click.extendedpanel', function (e) {
                if (e.handled) return;
                link.removeClass('active');
                jQuery('#extended-information').hide();
                jQuery(document).unbind('click.extendedpanel');
            });
            return false;
        });
        jQuery(".filter-main .outer-container .close-reportlist").on("click", function () {
            jQuery(document).trigger('click.filterpanel');
            jQuery(document).trigger('click.excelfilterpanel');
        });

        jQuery(".excell-like-filter .outer-container .close-reportlist").on("click", function () {
            jQuery(document).trigger('click.filterpanel');
            jQuery(document).trigger('click.excelfilterpanel');
        });

        treeViewFunction();
        scrollSave();
        jQuery('#tabfunctions .table-join form, #tabfunctions .table-group form, #data-grid-column-filter form').ajaxForm(long_running_options);
        // init btns
        jQuery("button").button();
        if (jQuery("#selected-group-column tr").length > 3) {
            jQuery("button.groupcolumn").button("option", "disabled", true);
        }
        if (jQuery("#selected-subtotal-column tr").length > 6) {
            jQuery("button.subtotalcolumn").button("option", "disabled", true);
        }
        jQuery("button.subtotalcolumn.disabled").button("option", "disabled", true);
        jQuery("button.del").button({ text: false, icons: { primary: "ui-icon-closethick white del" } });
    })();
    /**************************************************************************
    * RIGHT MODE
    **************************************************************************/

    jQuery(document).on("click", ".boxheader .right-switch", function () { // opts
        jQuery('#dialog').show().html(jQuery('#wait-dlg-rights').html());
        var $link = jQuery(this);
        jQuery.get($link.attr('href'), function (html) {
            location.reload();
        });
        return false;
    });

    jQuery(document).on("click", ".tabbar .right-switch", function () { // cats
        var $dialog = jQuery('#dialog').show().html(jQuery('#wait-dlg-rights').html());
        var $link = jQuery(this);
        jQuery.get($link.attr('href'), function (html) {
            jQuery('.content-box').replaceWith(html);
            refreshBoxHeader();
            $dialog.hide();
        });
        return false;
    });

    jQuery(document).on("click", ".overview .right-switch, .reportoverview .right-switch", function () { // objs
        var $dialog = jQuery('#dialog').show().html(jQuery('#wait-dlg-rights').html());
        var $link = jQuery(this);
        jQuery.get($link.attr('href') + '&fullRefresh=true', function (retHtml) {
            /*var contentToInsert = $(html + " #gesamnt").find("#gesamt").html();
            jQuery("#gesamt").html(contentToInsert);
            var inner = jQuery('body', $(retHtml));*/
            //jQuery('body').html(retHtml);
            //refreshBoxHeader();
            $dialog.hide();
            //location.reload();

            var showHidden = jQuery('.tabbar .selected').hasClass('show_hidden');
            var showArchived = jQuery('.tabbar .selected').hasClass('show_archived');
            var showEmpty = jQuery('.tabbar .selected').hasClass('show_empty');
            var searchElement = jQuery('#reportlistpopupcontainer input[type=text]');
            var search = searchElement.val();
            var placeholder = searchElement.attr('placeholder');
            if (search == placeholder) search = '';
            var size = jQuery('.header .nav-controls .size').text();
            var sortColumn = jQuery('.header .nav-controls .sortColumn').text();
            var direction = jQuery('.header .nav-controls .direction').text();
            var a_href = $('.actualController').attr('href');
            //if (a_href.indexOf("ViewList") > 0) {
            //    jQuery.get('/ViewList/Index', { json: false, size: size, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: showEmpty, showHidden: showHidden, showArchived: showArchived },
            //        function (html) {
            //            jQuery('.content-box').replaceWith(html);
            //        });
            //}
            //else
            if (a_href.indexOf("TableList") > 0) {
                jQuery.get('/TableList/Index', { json: false, size: size, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: showEmpty, showHidden: showHidden, showArchived: showArchived },
                function (html) {
                    jQuery('.content-box').replaceWith(html);
                });
            }
        });
        return false;
    });

    jQuery(document).on("click", "#showOneReportItem .right-switch", function () { // report list objs
        var $dialog = jQuery('#dialog').show().html(jQuery('#wait-dlg-rights').html());
        var $link = jQuery(this);
        jQuery.get($link.attr('href'), function (html) {
            var tabbar = jQuery('#reportlistpopupcontainer .tabbar', html);
            var resultlist = jQuery('#resultlist', html);
            jQuery('#reportlistpopupcontainer .tabbar').html(tabbar);
            jQuery('#resultlist').html(resultlist);
            $dialog.hide();
        });
        return false;
    });

    jQuery(document).on("click", "#main-report-list .right-switch", function () { // report list objs
        var $dialog = jQuery('#dialog').show().html(jQuery('#wait-dlg-rights').html());
        var $link = jQuery(this);
        var tableid = jQuery(this).closest(".reportresultrow").find("div[data-tableid]").attr("data-tableid");
        var issueid = jQuery('#IssueId').attr("value");
        jQuery.get($link.attr('href'), function (html) {
            var inner = jQuery('.main-table', html);
            jQuery('.main-table').html(inner.html());
            addDragging();
            if (tableid == issueid) {
                var $elFrom = jQuery('div[data-tableid=' + issueid + ']', html).closest("tr");
                var $elTo = jQuery('#IssueId').closest("tr");
                var elTooltip = $elFrom.find(".tooltip-text").text();
                var elLink = $elFrom.find("a.right-switch").attr("href");
                elLink = elLink.replace("UpdateReportListObjectRight", "UpdateTableObjectRight");
                if ($elFrom.hasClass("readable")) {
                    $elTo.addClass("readable").removeClass("not-readable");
                } else {
                    $elTo.addClass("not-readable").removeClass("readable");
                }
                $elTo.find(".tooltip-text").text(elTooltip);
                $elTo.find("a.right-switch").attr("href", elLink);
            }
            var $elFrom = jQuery('.tabfunctions', html);
            jQuery('.tabfunctions').html($elFrom.html());
            addDragging();
            $dialog.hide();
            return false;
        });
        return false;
    });

    jQuery(document).on("click", "#viewoptions .right-switch", function () { // cols
        var $dialog = jQuery('#dialog').show().html(jQuery('#wait-dlg-rights').html());
        var $link = jQuery(this);
        jQuery.get($link.attr('href'), function (html) {
            jQuery('#viewoptions').replaceWith(html);
            jQuery('#viewoptions .tabbar .rights').click();
            var $menu = jQuery('#viewoptions .dropdown-menu').show();
            jQuery(document).bind('click.dropdown', function (e) {
                if (e.isPropagationStopped()) return;
                $menu.fadeOut(100);
                jQuery(document).unbind('click.dropdown');
            });
            refreshBoxHeader();
            $dialog.hide();
        });
        return false;
    });
    /**************************************************************************
    * USER MANAGEMENT
    **************************************************************************/
    (rightsettings_dragdrop = function () {
        jQuery('#rightsettings .roles li.role').draggable({
            appendTo: '#rightsettings', helper: 'clone', revert: "invalid"
        });
        jQuery('#rightsettings .users li.user').droppable({
            accept: 'li.role',
            hoverClass: "user-drop-hover",
            drop: function (e, h) {
                var $dialog = jQuery('#dialog').show().html(jQuery('#assignRole-confirm-dlg').html());
                var user = $(this);
                var userName = user.find("td span.name").text()
                $dialog.find('.dialog-btn').click(function () {
                    $dialog.hide();
                    if (jQuery(this).find('.data').text() == 'True') {
                        var uid = user.children('.item-id').text();
                        var rid = h.helper.find('.item-id').text();
                        var name = h.helper.find('.name').text();
                        var roles = user.find('.user-roles li');
                        var found = false;
                        roles.each(function (i, e) {
                            if (jQuery(e).find('.item-id').text() == rid) {
                                found = true;
                                return;
                            }
                        });
                        if (found) return;
                        jQuery.get('/UserManagement/UserAddRole', { user: uid, role: rid }, function (html) {
                            jQuery('#wait-dlg').remove();
                            jQuery('#rightsettings').replaceWith(html);
                            if (typeof $("#rightsettings ul.users li.user td span:contains(" + userName + ")")[0] !== "undefined") {
                                $("#rightsettings ul.users li.user td span:contains(" + userName + ")")[0].scrollIntoView(true);
                            }
                            rightsettings_dragdrop();
                        });
                        setTimeout('refreshBoxHeader()', 500);
                    }
                });
            }
        });
    })(/* execute rightsettings */);

    jQuery(document).on("click", ".new-user", function () {
        jQuery.get('/UserManagement/CreateUser', function (html) {
            var $dialog = jQuery('#dialog');
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html);
            var $waitDialog = jQuery('#wait-dlg');
            var activate;
            var submit = function () {
                var name = $dialog.find("form input[name='userName']").val();
                $dialog.find('form').ajaxSubmit({
                    //target: '#rightsettings',
                    target: '#rightsettings',
                    url: '/UserManagement/CreateUser',
                    success: function (html) {
                        rightsettings_dragdrop();
                        if (jQuery(html).filter('#show-dlg').length) {
                            activate();
                        } else {
                            $dialog.hide();
                            if (typeof $("#rightsettings ul.users li.user td span:contains(" + name + ")")[0] !== "undefined") {
                                $("#rightsettings ul.users li.user td span:contains(" + name + ")")[0].scrollIntoView(true);
                            }
                        }
                        refreshBoxHeader();
                    },
                    beforeSubmit: function (html) {
                        $dialog.show().html($waitDialog.remove().html());
                    },
                    error: function (html) {
                        rightsettings_dragdrop();
                        $dialog.hide();
                        $waitDialog.hide();
                        $waitDialog.remove();
                        //TODO: Dialog, der Fehler zurückgibt
                        jQuery.get('/Settings/Rights', { json: true }, function (html) {
                            jQuery('#rightsettings').replaceWith(html);
                        });
                        refreshBoxHeader();
                    }
                });
            };
            (activate = function () {
                $dialog.show();
                jQuery('#dialog .dialog-outer-box').css("margin-top", -jQuery('#dialog .dialog-outer-box').height() / 2);

                $dialog.find('.dialog-btn').click(function () {
                    $button = jQuery(this);
                    if ($button.find('.data').text() == 'False') $dialog.hide();
                    else if ($button.find('.data').text() == 'True') {
                        $button.attr("disabled", "true");
                        var $newDialog = jQuery('#dialog1').show().html(jQuery('#add-confirm-dlg').html());
                        $newDialog.find('.dialog-btn').click(function () {
                            $newDialog.hide();
                            if (jQuery(this).find('.data').text() == 'True') {
                                $dialog.find('form').submit();
                            }
                        });
                    }
                });

                $dialog.find('form').submit(function () {
                    submit();
                    refreshBoxHeader();
                    return false;
                });
            })(/* execute activate */);
        });
    });
    // optimization deletion click handler

    jQuery(document).on("click", ".delete-optimizations", function () {
        var optimizationId = jQuery(this).find('.opt-id').text();
        jQuery.get('/UserManagement/CreateOptimizationDeleteDialog', function (html) {
            var $dialog = jQuery('#dialog');
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html).show().find('.dialog-btn').click(function () {
                var $button = jQuery(this);
                var $data = $button.find('.data').text();
                // cancel button clicked
                if ($data == 'False') {
                    $dialog.hide();
                }
                    // OK button clicked
                else if ($data == 'True') {
                    jQuery('#dialog').load('/DataGrid/TestOptimizationDelete/', { optid: optimizationId }, long_running_options.success);
                    return false;
                }
            });
        });
    });

    /* Roles setup menu in settings */
    // Views
    var fromLeafToRoot = function () {
        var children = $(this).parent().parent().find(".table-visibility");
        var root = $(this).parent().parent().parent().parent().find(".root input");
        var visible = 0;
        children.each(function () {
            if ($(this).is(':checked')) {
                visible++;
            }
        });
        if (visible > 0) {
            root.prop('checked', true);
        }
        else {
            root.prop('checked', false);
        }
    }

    var fromRootToLeaf = function () {
        var menuVisible = $(this).is(":checked");
        var root = $(this).parent();
        var children = $(this).parent().parent().find(".children .table-visibility");
        children.each(function () {
            $(this).prop('checked', menuVisible);
        })
    }

    var optFromRoot = function () {
        var menuVisible = $(this).is(":checked");
        var root = $(this).parent();
        var children = root.find(".children .optimization-visibility");
        children.each(function () {
            $(this).prop('checked', menuVisible);
        });
    }

    var optFromLeaf = function () {
        var currentLevel = $(this).parent();
        while (!currentLevel.hasClass("root")) {
            var checked = $(currentLevel).children("input.optimization-visibility").is(":checked");
            var siblings = currentLevel.siblings("li");
            if (!checked) {
                siblings.each(function () {
                    var otherChecked = $(this).children("input.optimization-visibility").is(":checked");
                    if (checked != otherChecked) {
                        checked = true;
                        return false;
                    }
                });
            }
            currentLevel = currentLevel.parent().closest("li");
            currentLevel.children("input.optimization-visibility").prop("checked", checked);
        }
    }

    jQuery(document).on("click", ".views .table-visibility, .issues .table-visibility", fromLeafToRoot);
    jQuery(document).on("click", ".views .root input, .issues .root input", fromRootToLeaf);
    jQuery(document).on("click", ".opt-role .optimization-visibility", optFromRoot);
    jQuery(document).on("click", ".opt-role .optimization-visibility", optFromLeaf);


    jQuery(document).on("click", ".new-role", function () {
        jQuery.get('/UserManagement/CreateRoleDialog', function (html) {
            var $dialog = jQuery('#dialog');
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html);
            var activate;
            var submit = function () {
                var name = $dialog.find("form input[name='name']").val();
                $dialog.find('form').ajaxSubmit({
                    target: '#rightsettings',
                    url: '/UserManagement/CreateRole',
                    success: function (html) {
                        rightsettings_dragdrop();
                        if (jQuery(html).filter('#show-dlg').length) {
                            activate();
                        }
                        else {
                            $dialog.hide();
                            if (typeof $("#rightsettings ul.roles li.role td span:contains(" + name + ")")[0]) {
                                $("#rightsettings ul.roles li.role td span:contains(" + name + ")")[0].scrollIntoView(true);
                            }
                        }
                    }
                });
            };
            (activate = function () {
                $dialog.show().find('.dialog-btn').click(function () {
                    $button = jQuery(this);
                    if ($button.find('.data').text() == 'False') $dialog.hide();
                    else if ($button.find('.data').text() == 'True') {
                        var $newDialog = jQuery('#dialog1').show().html(jQuery('#addRole-confirm-dlg').html());
                        $newDialog.find('.dialog-btn').click(function () {
                            $newDialog.hide();
                            if (jQuery(this).find('.data').text() == 'True') {
                                $dialog.find('form').submit();
                            }
                        });
                    }
                });
                $dialog.find('form').submit(function () {
                    submit();
                    refreshBoxHeader();
                    return false;
                });
            })(/* execute activate */);
        });
    });

    jQuery(document).on("click", "#rightsettings form #Submit", function () {
        var $dialog = jQuery('#dialog');
        jQuery.get("/IssueList/GetValidationDialog", function (html) {
            jQuery('#dialog').html(html).show(0, function () {
                location.reload();
                return false;
            });
        });
    });

    //jQuery(document).on("submit", "#rightsettings form", function () {
    //    var $dialog = jQuery('#dialog');
    //    //$dialog.show();
    //    //var $form = jQuery(this);
    //    //$form.ajaxSubmit(long_running_options);

    //    //jQuery.get("/IssueList/GetValidationDialog", function (html) {
    //    //    if (html.search("login") != -1) {
    //    //        window.location = "/Account/LogOn";
    //    //    }
    //    //    jQuery('#dialog').html(html).show(0, function () {
    //    //        //var tr = jQuery(e.target).parent().siblings('.tab-content-inner-report');
    //    //        var form = jQuery(this);
    //    //        if (form.attr('action').indexOf("ExecuteIssue") > -1) {
    //    //            form.ajaxSubmit(long_running_options);
    //    //            return false;
    //    //        }
    //    //    });
    //    //    //alert("tej");
    //    //});
    //    //return false;
    //});

    var ad_loaded;

    jQuery(document).on("click", ".ad-import", function () {
        jQuery.get('/UserManagement/ImportUserFromADDialog', function (html) {
            var $dialog = jQuery('#dialog');
            var $oldHtml = $dialog.html();
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html);
            var $waitDialog = jQuery('#wait-dlg');
            var activate;
            var loadUserList;
            var submit = function () {
                $dialog.find('form').ajaxSubmit({
                    target: '#rightsettings',
                    url: '/UserManagement/ImportUsersFromAD',
                    success: function (newhtml) {
                        rightsettings_dragdrop();
                        if (jQuery(newhtml).filter('#show-dlg').length) {
                            activate();
                        } else $dialog.hide();
                        refreshBoxHeader();
                    },
                    beforeSerialize: function ($form, options) {
                        $form.find(".sorting-users .droparea .user").each(function (i, e) {
                            var elem = jQuery(e);
                            $form.append("<input type='hidden' name='adUsers[" + i + "].Domain' value='" + elem.find('.domain').text() + "' />");
                            $form.append("<input type='hidden' name='adUsers[" + i + "].ShortName' value='" + elem.find('.kuerzel').text() + "' />");
                            $form.append("<input type='hidden' name='adUsers[" + i + "].Name' value='" + elem.find('.name').text() + "' />");
                        });
                    },
                    beforeSubmit: function (arr, $form, options) {
                        $oldHtml = $dialog.html();
                        $dialog.show().html($waitDialog.remove().html());
                    },
                    error: function (newhtml) {
                        var decoded = jQuery("<div/>").html(newhtml.responseText.substring(newhtml.responseText.indexOf("<p>") + 3, newhtml.responseText.indexOf("</p>"))).text();
                        jQuery.get("/UserManagement/ShowErrorMessage", { error: decoded }, function (newnewhtml) {
                            if (newnewhtml.search("login") != -1) {
                                window.location = "/Account/LogOn";
                            }
                            $dialog.html(newnewhtml);
                            $dialog.find('.dialog-btn').click(function () {
                                if ($oldHtml.search("login") != -1) {
                                    window.location = "/Account/LogOn";
                                }
                                $dialog.html($oldHtml);
                                $dialog.find('form input[type=hidden]').remove();
                                activate();
                            });
                        });
                    }
                });
            };
            loadUserList = function (really) {
                jQuery.get("/UserManagement/GetADUserListPartial", { domainName: jQuery(".active-directory select[name=domains]").val(), filter: jQuery(".active-directory input[name=filter]").val() }, function (newhtml) {
                    jQuery('.active-directory .sortable-users').replaceWith(newhtml);
                    var users = jQuery(".active-directory .sorting-users .droparea .user");
                    jQuery(".active-directory .sortable-users .droparea .user").each(function (i, el) {
                        var elem = jQuery(el);
                        if (users.find('.domain:contains(' + elem.find('.domain').text() + ')').length && users.find('.kuerzel:contains(' + elem.find('.kuerzel').text() + ')').length) elem.remove();
                    });

                    //if (!jQuery(".active-directory .sortable-users .droparea .user").length) jQuery(".active-directory .sortable-users .droparea .placeholder").show();
                    //ad_loaded();
                });
            };
            (activate = function () {
                //ad_loaded();
                $dialog.show();
                jQuery('#dialog .dialog-outer-box').css("margin-top", -jQuery('#dialog .dialog-outer-box').height() / 2);
                $dialog.find('.dialog-btn').click(function () {
                    $button = jQuery(this);
                    if ($button.find('.data').text() == 'False') $dialog.hide();
                    else if ($button.find('.data').text() == 'True') {
                        $button.attr("disabled", "true");
                        submit();
                        refreshBoxHeader();
                    }
                });

                jQuery(document).on("click", ".active-directory .ad-users", function () {
                    loadUserList(false);
                });
                $dialog.find('form').bind("keypress", function (e) {
                    if (e.keyCode == 13) {
                        if (jQuery(e.target).hasClass('ad-input-field')) {
                            loadUserList(false);
                            return false;
                        } else {
                            return false;
                        }
                    }
                });
                $dialog.find('form').submit(function () {
                    submit();
                    refreshBoxHeader();
                    return false;
                });
            })(/* execute activate */);
        });
    });
    //(ad_loaded = function () {
    //    //jQuery(".active-directory .sortable-users .droparea, .active-directory .sorting-users .droparea").sortable({
    //    //    start: function (e, ui) {
    //    //        if (!ui.placeholder.children().length) {
    //    //            ui.placeholder.append('<div class="placeholder-box"><div /></div>');
    //    //            ui.helper.addClass('helper-border');
    //    //        }
    //    //    },
    //    //    connectWith: ".active-directory .sortable-users .droparea, .active-directory .sorting-users .droparea",
    //    //    placeholder: 'drag-helper',
    //    //    containment: '#ad-user-list',
    //    //    scroll: true,
    //    //    scrollSensitivity: 1000,
    //    //    tolerance: "pointer",
    //    //    stop: function (e, ui) {
    //    //        ui.item.removeClass('helper-border');
    //    //        if (jQuery(".active-directory .users .sorting-users .user").length)
    //    //            jQuery(".active-directory .users .sorting-users .placeholder").hide();
    //    //        else jQuery(".active-directory .users .sorting-users .placeholder").show();
    //    //        if (jQuery(".active-directory .users .sortable-users .user").length)
    //    //            jQuery(".active-directory .users .sortable-users .placeholder").hide();
    //    //        else jQuery(".active-directory .users .sortable-users .placeholder").show();
    //    //    }
    //    //}).disableSelection();

    //})(/* execute ad_loaded */);

    $(document).on("click", ".active-directory .sortable-users .droparea .user", function () {
        var element = $(this).clone();
        var container = $(".active-directory .sorting-users .droparea");
        container.append(element);
        $(this).remove();

        SortADUsers(container);
    });

    $(document).on("click", ".active-directory .sorting-users .droparea .user", function () {
        var element = $(this).clone();
        var container = $(".active-directory .sortable-users .droparea");
        container.append(element);
        $(this).remove();

        SortADUsers(container);
    });

    var SortADUsers = function (container) {
        var listitems = container.children(".user");
        listitems.sort(function (a, b) {
            var compA = $(a).find('.name').text().toUpperCase();
            var compB = $(b).find('.name').text().toUpperCase();
            return (compA < compB) ? -1 : (compA > compB) ? 1 : 0;
        })
        $(container).append(listitems);
    };

    jQuery(document).on("click", "#rightsettings .user-roles a", function () {
        var $dialog = jQuery('#dialog').show().html(jQuery('#clearAssignment-confirm-dlg').html());
        var thiz = jQuery(this);
        var name = thiz.parents("li.user").find("span.name").text();
        $dialog.find('.dialog-btn').click(function () {
            $dialog.hide();
            if (jQuery(this).find('.data').text() == 'True') {
                jQuery.get(thiz.attr('href'), function (html) {
                    jQuery('#wait-dlg').remove();
                    jQuery('#rightsettings').replaceWith(html);
                    if (typeof $("#rightsettings ul.users li.user td span:contains(" + name + ")")[0] !== "undefined") {
                        $("#rightsettings ul.users li.user td span:contains(" + name + ")")[0].scrollIntoView(true);
                    }
                    rightsettings_dragdrop();
                });
                setTimeout('refreshBoxHeader()', 500);
            }
        });
        return false;
    });

    jQuery(document).on("click", "#rightsettings a.delete", function () {
        var thiz = jQuery(this);
        var objectName = thiz.attr("obj-name");
        var $dialog = jQuery('#dialog').show().html(jQuery('#delete-confirm-dlg').html());

        $dialog.find(".dialog-text h2").text($dialog.find(".dialog-text h2").text().replace("--NAME--", objectName));
        $dialog.find(".dialog-text span").text($dialog.find(".dialog-text span").text().replace("--NAME--", objectName));

        var name;
        var className;
        if (typeof thiz.parents("li").prev() !== "undefined") {
            name = thiz.parents("li").prev().find("span.name").text();
            className = thiz.parents("li").attr("data-class");
        }
        $dialog.find('.dialog-btn').click(function () {
            $dialog.hide();
            if (jQuery(this).find('.data').text() == 'True') {
                var diag = jQuery('#dialog').show().html($('#delete-wait-dlg').html());
                jQuery.get(thiz.attr('href'), function (html) {
                    jQuery('#wait-dlg').remove();
                    diag.hide();
                    jQuery('#rightsettings').replaceWith(html);
                    if (typeof name !== "undefined" && typeof $("#rightsettings ul li." + className + " td span:contains(" + name + ")")[0] !== "undefined") {
                        $("#rightsettings ul li." + className + " td span:contains(" + name + ")")[0].scrollIntoView(true);
                    }
                    rightsettings_dragdrop();
                });
                setTimeout('refreshBoxHeader()', 500);
            }
        });

        return false;
    });
    refreshBoxHeader = function () {
        jQuery.get('/Command/RefreshBoxHeader', { returnUrl: jQuery('#retUrl').val() }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            jQuery('.boxheader').html(html);
            //add hover functionality
            jQuery('ul.dropdown.small > li').hover(function () {
                $(this).find('> ul').show();
            }, function () {
                $(this).find('> ul').hide();
            });
        });
    };
    refreshTableObjectsPartial = function (catRefresh) {
        if (jQuery('#ViewList').length || jQuery('#IssueList').length || jQuery('#TableList').length) {
            var url = "/Command/RefreshTableObjects";
            //showEmpty
            url += '?showEmpty=';
            if (jQuery('.tabbar .selected').hasClass('show_empty')) url += 'true';
            else url += 'false';
            //showHidden
            url += '?showHidden=';
            if (jQuery('.tabbar .selected').hasClass('show_hidden')) url += 'true';
            else url += 'false';
            //showArchived
            url += '?showArchived=';
            if (jQuery('.tabbar .selected').hasClass('show_archived')) url += 'true';
            else url += 'false';
            //search
            var searchElement = jQuery('#searchoverview input[type=text]');
            var search = searchElement.val();
            var placeholder = searchElement.attr('placeholder');
            if (search == placeholder) search = '';
            url += '&search=' + search;
            //json
            url += '&json=True';
            //type
            var type = 'Issue';
            if (jQuery('#ViewList').length) type = 'View';
            if (jQuery('#TableList').length) type = 'Table';
            url += '&type=' + type;
            //page
            url += '&page=' + jQuery('.header .nav-controls .page').text();
            //size
            url += '&size=' + jQuery('.header .nav-controls .size').text();
            // + sort 
            url += '&sortColumn=' + jQuery('.header .nav-controls .sortColumn').text();
            url += '&direction=' + jQuery('.header .nav-controls .direction').text();
            //catRefresh
            url += '&catRefresh=' + catRefresh;
            jQuery.get(url, function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                if (catRefresh == "False") jQuery('table.overview tbody').html(html);
                else jQuery('.content-box').replaceWith(html);
            });
        }
    };
    refreshViewOptionsPartial = function (id) {
        var url = "/Command/RefreshViewOptions";
        jQuery.get(url, { id: id }, function (html) {
            jQuery('#viewoptions').replaceWith(html);
        });
    };
    refreshViewOptionsPartialForColumnOrder = function (id) {
        var url = "/Command/RefreshViewOptions";
        jQuery.get(url, { id: id }, function (html) {
            jQuery('#viewoptions').replaceWith(html);
            jQuery('#viewoptions .dropdown-btn').click();
            jQuery('#viewoptions .tabheader.column-order').click();
            viewoptions_loaded();
        });
    };
    refreshExtendedSortPartial = function (id) {
        var url = "/Command/RefreshExtendedSort";
        jQuery.get(url, { id: id }, function (html) {
            jQuery('#extsortoptions').replaceWith(html);
            viewoptions_loaded();
        });
    };
    refreshExtendedSummaPartial = function (id) {
        var url = "/Command/RefreshExtendedSum";
        jQuery.get(url, { id: id }, function (html) {
            jQuery('#extsummaoptions').replaceWith(html);
            viewoptions_loaded();
        });
    };
    refreshExtendedFilterPartial = function (id) {
        var url = "/Command/RefreshExtendedFilter";
        jQuery.get(url, { id: id }, function (html) {
            jQuery('#extfilteroptions').replaceWith(html);
            viewoptions_loaded();
            //timeout needed for treeview, the new filter has to be loaded.
            setTimeout(treeViewFunction, 2000);
        });
    };
    refreshDataGridForColumnOrder = function (id) {
        var start = jQuery('#DataGrid .main-header .nav-controls .from').text();
        var size = jQuery('#DataGrid .main-header .nav-controls .size').text();
        var enlarge_viewoptions = jQuery('#viewoptions .dropdown-menu').hasClass('viewoptions-enlarged');
        jQuery.get("/DataGrid/Index", { id: id, start: start - 1, size: size, json: true }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            jQuery('#DataGrid').html(html);
            var replace = jQuery('#DataGrid .page-header').clone();
            jQuery('#DataGrid .page-header').removeClass("replace");
            replace.insertAfter(jQuery('#DataGrid .page-header'));
            jQuery('#viewoptions .dropdown-btn').click();
            jQuery('#viewoptions .tabheader.column-order').click();
            datagrid_loaded();
            //table_dragdrop();
            vcolResizable();
            if (enlarge_viewoptions) jQuery('#viewoptions .dropdown-menu').removeClass('normal').addClass('viewoptions-enlarged');
            jQuery("#dialog").html('').hide();
        });
    };
    /**************************************************************************
    * DRAG'N'DROP VIEWOPTIONS
    **************************************************************************/
    /* Input change needs to be added to the command tree */

    jQuery(document).on("change", ".droparea .treeview .operator input, .droparea .treeview .operator textarea", function () {
        var filterValue = $(this).val();
        $(this).parent().prev('.op-command.value').text(filterValue);
        console.log(filterValue);
    });
    /* ************************************************** */
    (reload_filter_view = function () {
        /*jQuery(".operator .delete").live("click",function () {
        jQuery(this).parent().parent().remove();
        reload_filter_view();
        });*/
        var table_id = jQuery('.table-lines .table-id').text();
        var filter = jQuery('.filter .droparea :not(.optional) > span.op-command').text();

        jQuery('.filter-subtree li:first').load('/DataGrid/RefreshFilterView', { id: table_id, filter: filter }, function () {
            viewoptions_loaded();
            var input_changed = function () {
                var $vinput = jQuery(this),
						$parent = $vinput.parent(),
						$clone = $parent.clone(),
						val = $vinput.text();
                $vinput.prev('span.value').text(val);
                if ($parent.hasClass('optional') && val) {
                    $parent.removeClass('optional');
                    $clone.insertAfter($parent);
                    $clone.text('').blur();
                    $clone.find('span.value').blur(input_changed);
                }
            };
            jQuery('.op-dock.column').each(function (index) {
                var $cinput = $(this).children('.dropdown:first');
                var tx = $cinput.text();
                $cinput.attr("title", tx); // set title for tooltip
                var measuringDiv = $('<div class="measuringDiv" style="position:absolute;visibility:hidden;height:auto;width:auto;"></div>').text(tx); // Hidden div for measuring
                $cinput.parent().append(measuringDiv);
                var txWithDots = tx;
                while ($cinput.parent().children('.measuringDiv').width() > $cinput.width()) {
                    tx = tx.substring(0, tx.length - 1); // decrease the tx length 1 from the end
                    txWithDots = tx + "...";
                    $cinput.parent().children('.measuringDiv').text(txWithDots);
                }
                $cinput.text(txWithDots);
            });
        });
    });
    (viewoptions_loaded = function () {

        $("#viewoptions .column-order-content .sortable-columns").sortable();
        $("#viewoptions .column-order-content .sortable-columns").sortable("option", "distance", 5);
        $("#viewoptions .column-order-content .sortable-columns").sortable("option", "scroll", true);
        $("#viewoptions .column-order-content .sortable-columns").sortable("option", "containment", "#column-order");
        $("#viewoptions .column-order-content .sortable-columns").sortable("option", "placeholder", "drag-helper");
        $("#viewoptions .column-order-content .sortable-columns").sortable("option", "connectWith", ".column-order-content .sortable-columns");

        jQuery(document).off("click", ".operator .delete").on("click", ".operator .delete", function () {
            jQuery(this).parent().parent().remove();
            reload_filter_view();
        });
        //Dropdown for and/or selection
        jQuery('.and_or_select').unbind('change').change(function () {
            jQuery(this).parent().prevAll('span.op-command:first').html(jQuery(this).find('option:selected').val());
            reload_filter_view();
        });
        // SORTABLES (COLUMNS)
        jQuery("#extsort .droparea .top-sort tbody").sortable().disableSelection();
        // SUMMABLES (COLUMNS)
        /*jQuery(".summa .summable-columns, .summa .droparea").sortable({
            start: function (e, ui) {
                if (!ui.placeholder.children().length) {
                    ui.placeholder.append('<div class="placeholder-box"><div /></div>');
                    ui.helper.addClass('helper-border');
                }
            },
            connectWith: ".summa .summable-columns, .summa .droparea",
            placeholder: 'drag-helper',
            containment: '#extsumma',
            scroll: false,
            distance: 5,
            stop: function (e, ui) {
                ui.item.removeClass('helper-border');
                if (jQuery(".summa .summing-columns .column").length)
                    jQuery(".summa .summing-columns .placeholder").hide();
                else jQuery(".summa .summing-columns .placeholder").show();
            }
        }).disableSelection();*/
        // SORTABLES (FILTER)
        // DRAGABLE (FILTER)
        jQuery(".filter .operators .operator").draggable({
            helper: 'clone', containment: '#extfilter', revert: 'invalid', appendTo: '#extfilter'
        }).disableSelection();
        var drop_options = {
            accept: '.operator',
            activeClass: "op-dock-hover",
            tolerance: 'pointer', //default: intersect
            drop: function (e, ui) {
                var $this = jQuery(this);
                var $node = ui.draggable.clone();
                $node.find('.op-text:first').replaceWith($node.find('.op-text:first').html()); // what is this for???
                $node.html('<li>' + $node.html() + '<span class="op-command">,</span></li>');
                $node.insertBefore(jQuery(this).parent());
                reload_filter_view();
            }
        };
        var droppables = jQuery(".drop-condition");
        droppables.droppable(drop_options);
        // DRAG (DBNULL)
        jQuery(".filter .operators .dbnull").draggable({
            helper: 'clone', containment: '#extfilter', revert: 'invalid', appendTo: '#extfilter'
        }).disableSelection();
        var dbnulldrop_options = {
            accept: '.dbnull',
            activeClass: "op-dock-hover",
            tolerance: 'pointer', //default: intersect
            drop: function (e, ui) {
                if (jQuery(this).parent().parent().parent().hasClass('nullable')) {
                    jQuery(this).val('∅');
                    //no keyup event fired on the placeholder input
                    jQuery(this).parent().prev('.value').text('DBNULL');
                }
            }
        };
        var dropabledbnull = jQuery(".placeholder");
        dropabledbnull.droppable(dbnulldrop_options);
        //RESET FILTER
        jQuery('#resetfilter').unbind("click").click(function () {
            jQuery('.filter-subtree li:first').empty();
            reload_filter_view();
        });
        //RESET SORT
        //Returns the selected columns to the original holder
        jQuery('#resetsort').unbind("click").click(function () {
            var columns = $('.sorting-columns .droparea tbody tr');
            for (var i = 0; i < columns.length; i++) {
                var shs = $(columns[i]).find('td.from-sort');
                shs.attr('class', 'to-sort');
                $(columns[i]).appendTo($('.sortable-columns1 .bot-sort tbody '));
                $(columns[i]).find('td.remove-attr').remove();
            }
            $(".sort .sorting-columns .placeholder").show();
        });
        //RESET SUMMA
        //Returns the selected columns to the original holder
        index = 0;
        jQuery('#resetsumma').unbind("click").click(function () {
            var columns = $('.summing-columns .droparea .top-sum tbody tr');
            for (var i = 0; i < columns.length; i++) {
                var summable_body = jQuery('.summable-columns1 .bot-sum tbody');
                var shs = $(columns[i]).find('td.from-sum');
                var obj = shs.parent();
                shs.attr('class', 'to-sum');
                var objNumber = parseInt(obj.find(".colnumber").text());
                summable_body.find("tr").each(function () {
                    var number = parseInt($(this).find(".colnumber").text());
                    if (objNumber < number && index == 0) {
                        $(this).before(obj);
                        index = 1;
                    }
                });
                if (index == 0) {
                    jQuery('.summable-columns1 .bot-sum tbody').append(shs.parent());
                }
                index = 0;

            }
            $(".summa .summing-columns .placeholder").fadeIn(500);
        });
        //RESET SUBTOTAL
        jQuery('#resetsubtotal').unbind("click").click(function () {
            var $originalRows = jQuery(".subtotalable-columns tr");
            $('.subtotaling-columns tbody tr').each(function (newIndex, newRow) {
                var $newRow = jQuery(newRow);
                var isInserted = false;
                var lastItem;
                var newNumber = $newRow.find(".colnumber").text();
                $originalRows.each(function (orIndex, orRow) {
                    var orNumber = $(orRow).find(".colnumber").text();
                    if (Number(orNumber) > Number(newNumber)) {
                        $newRow.find("button").toggle();
                        if ($newRow.closest(".subtotal-target").prev().hasClass("group-target-header")) {
                            $(".subtotal .groupcolumn").button("option", "disabled", false);
                        }
                        else {
                            if (jQuery("#selected-subtotal-column").children().length < 8) {
                                $(".subtotal .subtotalcolumn").not(".disabled").button("option", "disabled", false);
                            }
                        }
                        $newRow.insertBefore(orRow);
                        isInserted = true;
                        return false;
                    }
                    lastItem = orRow;
                });
                if (!isInserted) {
                    $newRow.find("button").toggle();
                    if ($newRow.closest(".subtotal-target").prev().hasClass("group-target-header")) {
                        $(".subtotal .groupcolumn").button("option", "disabled", false);
                    }
                    else {
                        if (jQuery("#selected-subtotal-column").children().length < 8) {
                            $(".subtotal .subtotalcolumn").not(".disabled").button("option", "disabled", false);
                        }
                    }
                    $newRow.insertAfter(lastItem);
                }
            });
            jQuery(".subtotal #onlyExpectedResult").removeAttr('checked');
        });
        jQuery('#extfilter .buttons form, #extsort .buttons form, #extsumma .buttons form, #subtotaloptions .buttons form, #rightsettings form, .multipleOptimization form').ajaxForm(long_running_options);
        /**************************************************************************
         *SUBTOTAL
         **************************************************************************/
        jQuery(".subtotal .buttons .submit").unbind("click").click(function () {
            var groupColumn = "";
            var subtotalColumns = "";
            var onlyExpectedResult = "";

            jQuery(".subtotal #selected-group-column tbody tr").each(function (i, el) {
                if (jQuery(el).find('td.item-id').text() != '')
                    groupColumn += 'groupColumnId=' + jQuery(el).find('td.item-id').text() + '&';
            });

            jQuery(".subtotal #selected-subtotal-column tbody tr").each(function (i, el) {
                if (jQuery(el).find('td.item-id').text() != '')
                    subtotalColumns += 'subtotalList[' + i + ']=' + jQuery(el).find('td.item-id').text() + '&';
            });

            onlyExpectedResult = "onlyExpectedResult=" + jQuery(".subtotal #onlyExpectedResult").prop("checked");
            jQuery(this).parent('form').get(0).action += '?' + groupColumn + subtotalColumns + onlyExpectedResult;
        });

        // init btns
        jQuery("button").button();
        if (jQuery("#selected-group-column tr").length > 3) {
            jQuery("button.groupcolumn").button("option", "disabled", true);
        }
        if(jQuery("#selected-subtotal-column tr").length > 6) {
            jQuery("button.subtotalcolumn").button("option", "disabled", true);
        }
        jQuery("button.subtotalcolumn.disabled").button("option", "disabled", true);
        jQuery("button.del").button({ text : false, icons: {
            primary: "ui-icon-closethick white del" } });
        /**************************************************************************
        * COLUMN SORT
        **************************************************************************/
        jQuery(document).off("click", ".sort .sortdirection").on("click", ".sort .sortdirection", function () {
            jQuery(this).toggleClass('desc');
            return false;
        });
        jQuery(".sort .buttons .submit").unbind("click").click(function () {
            var $submit = $(this);
            jQuery.get("/DataGrid/GetValidationDialog", function (html) {
                jQuery('#dialog').html(html).show(0, function () {
                    var sort = '';
                    jQuery("#extsort .droparea tr").each(function (i, el) {
                        if (jQuery(el).find('.item-id').text() != '')
                            sort += '&sortList[' + i + '].cid=' + jQuery(el).find('.item-id').text() + '&sortList[' + i + '].dir=' + (jQuery(el).find('.sortdirection').is('.desc') ? '1' : '0');
                    });
                    $submit.parent('form').get(0).action += sort;
                    $submit.parent('form').submit();
                });
            });
            return false;
        });
        /**************************************************************************
        * EXTENDED FILTER
        **************************************************************************/
        jQuery(".add-new-element-for-IN-filter").unbind("click").click(function () {
            var element = jQuery('.in-filter-template:last').clone(true); // get the last element and clone it
            element.find("input").attr("value", ""); // remove values from input field
            element.find(".value:last").html(""); // remove value from hidden field too
            var lastelement = jQuery(this).parent().find('.in-filter-template:last');
            element.insertAfter(lastelement); //insert the element
        });
        jQuery(document).off("click", ".delete_element").on("click", ".delete_element", function (event) {
            event.stopPropagation();
            var numberOfElements = jQuery(this).parent().parent().find(".delete_element").length;
            if (numberOfElements > 1) {
                jQuery(this).parent().remove();
            }
        });
        jQuery(".filter .buttons .submit").unbind("click").click(function () {
            $.each(jQuery(".filter-tree .droparea .value .placeholder"), function (key, thiz) { 
                if ($($(thiz).parent().prev('.value').parent().parent().find(".op-command")[0]).text().indexOf("Like") > -1
                  || (jQuery(thiz).parent().prev('.value').text().indexOf("%") == 0
                     && jQuery(thiz).parent().prev('.value').text().indexOf("%", 1) == jQuery(thiz).parent().prev('.value').text().length - 1)) {
                    jQuery(thiz).parent().prev('.value').text("%" + jQuery(thiz).val() + "%");
                }
                else {
                    jQuery(thiz).parent().prev('.value').text(jQuery(thiz).val());
                }
            });
            var filter = jQuery('.filter .droparea   span.op-command').text();
            jQuery('#filter').val(filter);
            var check_save = jQuery('#check_save').is(":checked");;
            jQuery('#save').val(check_save);
            $form = jQuery(this).parent();
            if (check_save) {
                jQuery.get("/DataGrid/ShowIssueDescriptionDialog", { id: jQuery('.tableObjectId').text(), filter: filter }, function (html) {
                    if (html.search("login") != -1) {
                        window.location = "/Account/LogOn";
                    }
                    $dialog = jQuery('#dialog').show().html(html);
                    if (jQuery('#dialog .dialog-outer-box').length) {
                        jQuery('#dialog .dialog-outer-box').css("margin-top", -jQuery('#dialog .dialog-outer-box').height() / 2);
                        $dialog.find('.dialog-btn').click(function () {
                            $button = jQuery(this);
                            if ($button.find('.data').text() == 'False') $dialog.hide();
                            else if ($button.find('.data').text() == 'True') {
                                $inputcontainer = $form.find('.inputcontainer');
                                // Clear div.inputcontainer, because if the backend will return an error message
                                // and data will be sent again then the hidden input fields will be copied to the other form multiple times!
                                $inputcontainer.empty();
                                $country_code = $dialog.find('form .countryCode');
                                $dialog.find('form input[type=text]').each(function (i, e) {
                                    jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                                });
                                $dialog.find('form input[type=checkbox]').each(function (i, e) {
                                    $(e).next('input[type=hidden]').val($(e).is(':checked'));
                                });
                                $dialog.find('form input[type=hidden]').each(function (i, e) {
                                    if (!jQuery(e).hasClass('countryCode')) {
                                        jQuery(e).appendTo($inputcontainer);
                                    }
                                });
                                $button.attr("disabled", "true");

                                $dialog.hide();

                                $('#loading').css('z-index', '1000').show(function () {
                                    $form.submit();
                                });
                            }
                        });
                    } else {
                        $dialog.find('.dialog-btn').click(function () {
                            $dialog.hide();
                        });
                    }
                    $dialog.find('form select[name=language]').change(function () {
                        $country_code = $dialog.find('form .countryCode');
                        $select = jQuery(this);
                        $dialog.find('form input[type=text]').each(function (i, e) {
                            jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                            jQuery(e).val($dialog.find('form .' + $select.val() + "_" + jQuery(e).attr('name')).val());
                        });
                        $country_code.val($select.val());
                    });
                });
            } else {
                $form.submit();
            }
            return false;
        });
    })(/* execute viewoptions_loaded */);
    /**************************************************************************
    * Columns - Drag & Drop
    **************************************************************************/
    (overview_dragdrop = function () {
        jQuery('#IssueList .overview tbody, #ViewList .overview tbody').sortable({
            axis: 'y',
            scroll: true,
            handle: '.handle',
            revert: true,
            forcePlaceholderSize: true,
            tolerance: 'pointer',
            update: function (e, ui) {
                var $dialog = jQuery('#dialog').show().html(jQuery('#wait-dlg').html());
                var controller = "IssueList";
                if (jQuery("#ViewList").length) controller = "ViewList";
                var table_id = ui.item.find('.hide-table span.hide').text();
                var ordinal = 0;
                var check = true;
                jQuery('.overview tbody tr').each(function (i, el) {
                    if (jQuery(el).find('.hide-table span.hide').text() == table_id) check = false;
                    if (check) ordinal++;
                });
                jQuery.get("/" + controller + "/ChangeTableOrder", { id: table_id, ordinal: ordinal }, function (html) { $dialog.html('').hide(); });
            }
        }).disableSelection();
    })();
    /**************************************************************
    * COLUMN RESIZE
    ***************************************************************/
    //var vcolSizes = [];
    var vcolSizes2 = [];
    (vcolResizableResize = function () {
        $.widget("ih.resizeableColumns", {
            _create: function () {
                this._initResizable();
            },
            _initResizable: function () {
                var i = 0;
                $('.datagridHeader th').each(function () {
                    if ($(this).attr("class").match(/col_[0-9]+/) == null)
                        return;

                    $(this).resizable({
                        handles: "e",
                        minWidth: 120,

                        create: function (event, ui) {
                            var number = $(event.target).attr("class").match(/col_([0-9]+)/)[1];

                            var name = $(event.target.getElementsByClassName("lastColName")).attr("value");
                            if (typeof vcolSizes2[name] == "undefined")
                                vcolSizes2[name] = $(this).width();
                            else {
                                $(this).css("width", vcolSizes2[name]);
                                $(".table-body .col_" + number).css("width", vcolSizes2[name]);
                                $(".table-body .col_" + number + " .value-on-rights").css("width", vcolSizes2[name] - 40);
                            }
                            calculateFullWidth();
                        },
                        resize: function (event, ui) {
                            var number = $(event.target).attr("class").match(/col_([0-9]+)/)[1];
                            $(".table-body .col_" + number).css("width", $(event.target).width());
                            $(".table-body .col_" + number + " .value-on-rights").css("width", $(event.target).width() - 40);
                            var name = $(event.target.getElementsByClassName("lastColName")).attr("value");
                            if (typeof vcolSizes2[name] != "undefined")
                                vcolSizes2[name] = $(this).width();

                            calculateFullWidth();
                            // height must be set in order to prevent IE9 to set wrong height
                            $(this).css("height", "auto");
                        },
                        stop: function (event, ui) {
                            //upload the current size;
                            var table_id = jQuery('#column-order .table-id').text();
                            var colID = $(event.target.getElementsByClassName("item-id")).text();
                            var Width = $(this).width();

                            jQuery.post("/DataGrid/UpdateColumnWidth", { id: table_id, columnID: colID, newWidth: Width }, function (html) {
                            })
                        }
                    });
                });
            }
        });
        $(".datagridHeader").resizeableColumns();
    });

    (vcolResizable = function () {
        $(".datagrid").attr("style", "table-layout: fixed;");
        vcolResizableResize();
        $('.table-container').css('z-index', '1');
        $('#loading').css('z-index', '-1000');
        $('.table-container').css('visibility', 'visible');

        $(".ui-resizable-handle.ui-resizable-e").dblclick(function () {
            //var items = $("." + $(this).parent().attr("class").split(' ')[0] + ".datagrid-value div");
            var size = parseInt($(this).parent().attr("originalWidth"));

            var number = $(this).parent().attr("class").match(/col_([0-9]+)/)[1];
            $(".table-body .col_" + number).css("width", size);
            $(this).parent().css("width", size);

            calculateFullWidth();

            
            var name = $(".col_" + number + " .th-content .lastColName").val();
            vcolSizes2[name] = size;

            var table_id = jQuery('#column-order .table-id').text();
            var colID = $($(".col_" + number + " .th-content .item-id")[0]).text();

            jQuery.post("/DataGrid/UpdateColumnWidth", { id: table_id, columnID: colID, newWidth: size }, function (html) {
            })
        });

    })();
    /**************************************************************************
    * LONG RUNNING OPERATIONS DIALOG
    **************************************************************************/
    long_running_options = {
        target: '#dialog',
        beforeSubmit: function (data, $form, options) {
            $form.parents('ul.dropdown-menu').hide();
        },
        success: function () {
            var $dialog = jQuery('#dialog');
            var link = $dialog.find('.link');
            if (link.length) location.href = link.text();
            var key = $dialog.find('.key').text();
            $('#DataGrid .content-inner').hide();
            $dialog.show().find('.dialog-btn').click(function () {
                delete Updater.Listeners[key];
                $('#loading').css('z-index', '-1000');
                jQuery('#dialog').hide();
                $('#DataGrid .content-inner').show();
            });
            Updater.Listeners[key] = function ($notification) {
                location.href = $notification.find('a.link').attr('href');
            };
        }
    };
    jQuery('#extfilter .buttons form, #extsort .buttons form, #extsumma .buttons form, #tabfunctions .table-join form, #tabfunctions .table-group form, #data-grid-column-filter form, .multipleOptimization form, .role-settings form, #downloadMoreFiles').ajaxForm(long_running_options);
    //Klick 'n Klick Dialog, Dialog für einfaches Sortieren, Export aus Datagrid
    //We can delete the '#DataGrid .content-box .datagrid .column-sort a' element
    jQuery(document).on("click", ".clicknclick-dropdown a, #DataGrid .content-box .datagrid .column-sort a, .pdfexport-overview, #Documents .sorting a, #ArchiveDocuments .sorting a, .column-menu-ascending, .column-menu-descending, .column-menu-clear-filter", function () {
        jQuery.get(jQuery(this).attr('href'), function (html) {
            var $dialog = jQuery('#dialog');
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html).show();
            var key = $dialog.find('.key').text();
            $dialog.find('.dialog-btn').click(function () {
                delete Updater.Listeners[key];
                jQuery('#dialog').hide();
            });
            Updater.Listeners[key] = function ($notification) {
                location.href = $notification.find('a.link').attr('href');
            };
        });
        return false;
    });
    var start_export = function (sender, checkSize, checkAllFields, getAllFields) {
        var url = jQuery(sender).attr('href');
        if (url.indexOf('?') > -1) url += "&";
        else url += "?";
        url += "checkSize=" + checkSize;
        url += "&checkAllFields=" + checkAllFields;
        url += "&getAllFields=" + getAllFields;
        jQuery.get(url, function (html) {
            var $dialog = jQuery('#dialog');
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html);
            $('#DataGrid .content-inner').hide();
            $dialog.show().find('.dialog-btn').click(function () {
                $button = jQuery(this);
                if ($button.find('.data').text().length) {
                    $dialog.hide();
                    $('#DataGrid .content-inner').show();
                    if ($button.find('.data').text() == 'True') {
                        start_export(sender, false, checkAllFields, getAllFields);
                    }
                    if ($button.find('.data').text() == 'needall') {
                        start_export(sender, checkSize, false, true);
                    }
                    if ($button.find('.data').text() == 'notall') {
                        start_export(sender, checkSize, false, false);
                    }
                } else {
                    delete Updater.Listeners[key];
                    $dialog.hide();
                    $('#DataGrid .content-inner').show();
                }
            });
            var key = $dialog.find('.key').text();
            Updater.Listeners[key] = function ($notification) {
                location.href = $notification.find('a.link').attr('href');
            };
        });
        return false;
    };
    jQuery(document).on("click", "#DataGrid .content-box .functions .iconx:not(.disabled), #DataGrid .content-box .functionsBalance .iconx", function (e) {
        return start_export(this, true, true, false);
    });

    jQuery(document).on("click", "#DataGrid .content-box .functions .iconx.disabled", function (e) {
        return false;
    });

    /**************************************************************************
    * COLUMN FILTER
    **************************************************************************/
    jQuery('#data-grid-column-filter').click(function (e) {
        e.stopPropagation();
    });
    jQuery('#excell-like-filter').click(function (e) {
        if ($(e.target).not('a'))
            e.stopPropagation();
    });
    jQuery(document).on("click", ".datagrid .excell-like-filter-link", function (e) {
        var $panel = jQuery('#excell-like-filter').insertAfter(jQuery(this).siblings('a.column-name')[0]).show();
        var itemId = jQuery(e.target).siblings('.item-id').html();
        $panel.find('#excell-text').html('<div class="loading"></div>');
        jQuery.get('/ValueList/ExcelValues', { Id: 0, columnId: itemId, search: "", exactMatch: false, showEmpty: true, sortColumn: 0, direction: 0, json: false, page: 0, size: 25 }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $panel.find('#excell-text').html(html);
            $panel.find('input[name=columnId]').val(itemId);
            jQuery("#selectAll").click(function (e) {
                var $link = jQuery(this);
                var $checkboxes = $link.parent().siblings(".gray-on-mouseover").children("a");
                if ($link.hasClass("chkboxon")) {
                    $link.removeClass("chkboxon").addClass("chkboxoff");
                    for (var i = 0; i < $checkboxes.length; i++) {
                        $($checkboxes[i]).removeClass("chkboxon").removeClass("chkboxoff").addClass("chkboxoff");
                        $($checkboxes[i]).siblings("input[type=hidden]").remove();
                    }
                } else {
                    $link.removeClass("chkboxoff").addClass("chkboxon");
                    for (var i = 0; i < $checkboxes.length; i++) {
                        $($checkboxes[i]).removeClass("chkboxon").removeClass("chkboxoff").addClass("chkboxon");
                        $($checkboxes[i]).siblings("input[type=hidden]").remove();
                        var value = $($checkboxes[i]).siblings(".distinct-value").html();
                        $($checkboxes[i]).after('<input type="hidden" name="filtervalues" value="' + value + '"/>');
                    }
                }
                e.stopPropagation();
                return false;
            });
            jQuery("#excell-like-filter .gray-on-mouseover a").click(function (e) {
                var $link = jQuery(this);
                var $checkboxes = $link.parent().siblings(".gray-on-mouseover").children("a");
                if ($link.hasClass("chkboxon")) {
                    $link.removeClass("chkboxon").addClass("chkboxoff");
                    $link.siblings("input[type=hidden]").remove();
                    jQuery("#selectAll").removeClass("chkboxon").removeClass("chkboxoff").addClass("chkboxoff");
                } else {
                    $link.removeClass("chkboxoff").addClass("chkboxon");
                    var value = $link.siblings(".distinct-value").html();
                    $link.after('<input type="hidden" name="filtervalues" value="' + value + '"/>');
                    if ($checkboxes.hasClass("chkboxoff")) {
                        jQuery("#selectAll").removeClass("chkboxon").removeClass("chkboxoff").addClass("chkboxoff");
                    } else {
                        jQuery("#selectAll").removeClass("chkboxon").removeClass("chkboxoff").addClass("chkboxon");
                    }
                }
                e.stopPropagation();
                return false;
            });
        });
        //position the filter so that its in the right place.
        if ($panel.offset().left < jQuery(window).width() / 2) {
            $panel.find('#excell-triangle').removeClass('triangleright').addClass('triangleleft');
            $panel.css("left", 0);
        } else {
            $panel.find('#excell-triangle').removeClass('triangleleft').addClass('triangleright');
            $panel.css("left", -145);
        }
        jQuery(document).bind('click.excelfilterpanel', function (e) {
            if (e.handled) return;
            jQuery('#excell-like-filter').hide();
            jQuery('#excell-like-filter').css("left", "");
            jQuery(document).unbind('click.excelfilterpanel');
        });
    });
    jQuery(document).on("click", ".datagrid a.column-name", function (e) {
        if ($(this).parent().parent().parent().find('.subtgroup').length == 0) {
            var id = jQuery(this).nextAll('.item-id').text();
            var lastFilterValue = jQuery(this).nextAll('.filter-value').text();
            var lastFilterExactChecked = jQuery(this).nextAll('#exactCheckboxChecked').text();
            var itWasHidden = jQuery('#data-grid-column-filter').is(':visible');
            var $panel = jQuery('#data-grid-column-filter').insertAfter(this).show().select();
            //$panel.find("input[type=text]").val(lastFilterValue).focus();

            $panel.find("#filter").val(lastFilterValue).focus();
            

            /*Changed it id column, because there are other inputs that are hidden*/
            var needMoveThePopUpWindow = (jQuery(this).nextAll('.item-id').text() != $panel.find("#column").val() || !itWasHidden);
            $panel.find("#column").val(id);
            var $checkbox = $panel.find('.exactmatch a');
            if (lastFilterExactChecked == "False") {
                $checkbox.removeClass('checkboxon');
                $panel.find('#exactmatch').val('false');
            } else if (lastFilterExactChecked == "True") {
                $checkbox.addClass('checkboxon');
                $panel.find('#exactmatch').val('true');
            }
            //position the filter so that its in the right place.
            if (needMoveThePopUpWindow) {
                if (e.pageX < jQuery(window).width() / 2) {
                    $panel.find('#triangle').removeClass('triangleright').addClass('triangleleft');
                    $panel.css("left", (jQuery(this).nextAll('.triangle-filter').position().left - 40));
                } else {
                    $panel.find('#triangle').removeClass('triangleleft').addClass('triangleright');
                    $panel.css("left", (jQuery(this).nextAll('.triangle-filter').position().left - 323));
                }
            }
            jQuery(document).bind('click.filterpanel', function (e) {
                if (e.handled || $(e.target).closest('#data-grid-column-filter').length != 0) return;
                jQuery('#data-grid-column-filter').hide();
                jQuery('#data-grid-column-filter').css("left", "");
                jQuery(document).unbind('click.filterpanel');
            });
        }
        return false;
    });
    jQuery(document).on("click", ".close-reportlist", function () {
        jQuery('#data-grid-column-filter').hide();
        jQuery('#data-grid-column-filter').css("left", "");
    });
    //jQuery(document).on("click", ".reset-button", function () {
    //    jQuery('#data-grid-column-filter').hide();
    //    jQuery('#data-grid-column-filter').css("left", "");
    //});

    jQuery("#data-grid-column-filter #filter").keydown(function handleEnter(event) {
        if (event.keyCode == 13)
            if (!event.shiftKey) { $(this.form).submit(); return false; }
    });

    /**************************************************************************
    * TABLE-OBJECT SELECTOR
    **************************************************************************/
    jQuery(document).on("click", "#IssueList .overview .viewoptions", function (e) {
        var tr = jQuery(this).parent().parent();
        jQuery.get("/IssueList/GetConfirmationDialog", function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            jQuery('#dialog').html(html).show().find('.dialog-btn').click(function () {
                $button = jQuery(this);
                if ($button.find('.data').text() == 'False') {
                    jQuery('#dialog').hide();
                }
                if ($button.find('.data').text() == 'True') {
                    jQuery('#dialog').hide();
                    var form = tr.find('form');
                    if (form.attr('action').indexOf("ExecuteIssue") > -1) {
                        form.ajaxSubmit(long_running_options);
                        return false;
                    }
                }
            });;
        });
    });
    jQuery(document).on("click", "#IssueList .showviewoptions", function (e) {
        jQuery.get("/IssueList/GetValidationDialog", function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            jQuery('#dialog').html(html).show(0, function () {
                if ($('#dialog').hasClass('forcedToQuit'))
                    return false;
                var tr = jQuery(e.target).parent().siblings('.tab-content-inner-report');
                var form = tr.find('form');
                $("#dialog").width($("#gesamt").width());
                if (form.attr('action').indexOf("ExecuteIssue") > -1) {
                    form.ajaxSubmit(long_running_options);
                    return false;
                }
            });
        });
    });
    jQuery(document).on("click", "#ArchiveDocuments .sort-column", function () {
        var id = jQuery(this).nextAll('.item-id').text();
        var $panel = jQuery('#data-grid-column-filter').insertAfter(this).show().select();
        $panel.find("input[type=text]").val("").focus();
        $panel.find("input[type=hidden]").val(id);
        jQuery(document).bind('click.filterpanel', function (e) {
            if (e.handled || $(e.target).closest('#data-grid-column-filter').length != 0) return;
            jQuery('#data-grid-column-filter').hide();
            jQuery(document).unbind('click.filterpanel');
        });
        return false;
    });
    jQuery(document).on("click", "#Documents .showviewoptions", function (e) {
        var form = jQuery(e.target).parent().parent().parent().parent().find('form');
        form.ajaxSubmit(long_running_options);
        return false;
    });
    jQuery(document).on("submit", "#IssueList .overview form", function (e) {
        var $form = jQuery(this);
        jQuery.get("/IssueList/GetConfirmationDialog", function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            jQuery('#dialog').html(html).show().find('.dialog-btn').click(function () {
                $button = jQuery(this);
                if ($button.find('.data').text() == 'False') {
                    jQuery('#dialog').hide();
                }
                if ($button.find('.data').text() == 'True') {
                    jQuery('#dialog').hide();
                    if ($form.attr('action').indexOf("ExecuteIssue") > -1) {
                        $form.ajaxSubmit(long_running_options);
                        return false;
                    }
                }
            });;
        });
    });
    jQuery(document).on("click", ".DeleteText", function () {
        var parId = parseInt($(this).attr('data-id'));
        var fullParId = $(this).attr('data-id');
        var tableId = parseInt($('#IssueId').val());
        var inputBox = $(this).parent().parent().find("[id*=TextBoxName" + fullParId + "]");
        inputBox.val('');
        inputBox.addClass('empty');
    });


});
var focusOut = (function (e) {
    var $target = jQuery(e.target);
    var $form = $('.parameter-table').parent();
    var action = $form.get(0).action;
    var issueId = action.substring(action.indexOf('ExecuteIssue/') + 13);
    var parid = $target.siblings('.item-id').text();
    $target.parent().siblings('.report-parameter-error').hide();
    //jQuery.get('/Datagrid/ValidateIssueParameterInput', { id: issueId, parameterId: parid, value: $target.val() }, function (html) {
    //    if (html != "") {
    //        if (html.search("login") != -1) {
    //            window.location = "/Account/LogOn";
    //        }
    //        if (html.toString().indexOf($target.val()) > 0) {
    //            $target.parent().siblings('.report-parameter-error').html(html);
    //            $target.parent().siblings('.report-parameter-error').show();
    //        }
    //    }
    //});
    //jQuery.get('/Datagrid/SaveParameterValueInHistory', { id: issueId, parameterId: parid, value: $target.val() });
});

jQuery('#IssueList .report-parameter-validate').bind('focusout', focusOut);
//The new version does not user the .overview tr version
jQuery(document).on("click", "#IssueList .parameter .dropdown", function (e) {
    trigger_dropdown(jQuery(e.target));
    return false;
});
//Because of dropdown inputs, the double click does not automaticaly select the input.
jQuery(document).on("dblclick", "#IssueList .overview input", function (e) {
    jQuery(e.target).select();
});
jQuery(document).on("click", "#IssueList .overview tr, #ViewList .overview tr.tableobject, #TableList .overview tr", function (e) {
    var currentYearRange = document.getElementById("currentYearRange");
    var previous = jQuery(this).addClass('aktiv').siblings('.aktiv');
    previous.removeClass('aktiv').find('.parameters').slideUp();
    previous.find('.arrow-orange-bottom-container').show();
    jQuery(this).find('.parameters').slideDown();
    jQuery(this).find('.arrow-orange-bottom-container').hide();
    var $target = jQuery(e.target);
    // don't follow the link directly
    if ($target.is("A")) return true;
    if ($target.is('.dropdown'))
        trigger_dropdown($target);
    // datepicker support
    if ($target.is('.field.reportdate'))
        $target.datepicker({
            showOn: "button",
            buttonImage: "/Content/img/icons/datepick.png",
            buttonImageOnly: true,
            showOtherMonths: true,
            selectOtherMonths: true,
            changeMonth: true,
            changeYear: true,
            yearRange: currentYearRange,
            onSelect: function (dateText, inst) {
                $(this).trigger('keyup');
            }
        });
    // input bugfix
    if ($target.is('input')) {
        $target.focus();
    }
}).on("mouseover", "#IssueList .overview tr, #ViewList .overview tr.tableobject, #TableList .overview tr", function () {
    jQuery(this).addClass('hover');
}).on("mouseout", "#IssueList .overview tr, #ViewList .overview tr.tableobject, #TableList .overview tr", function () {
    jQuery(this).removeClass('hover');
});
/* Belegarchive table body name changed and setting up rollenrechte menu as well */
$(document).ready(function () {
    if ($("#Documents").length > 0) {
        $('#loading').css('z-index', '50');
        $(window).resize(setBelegTableBody);
        $("#Documents .table-body").scroll(function () {
            $("#Documents .table-header").css("left", -1 * (this.scrollLeft + 20) + 20);
        });

        setBelegTableBody();

        setColWidths($("#belegHeaderHead > tr > th"));
        $(".belegHeader").css("table-layout", "fixed");
        $(".belegBody").css("table-layout", "fixed");

        $('#loading').css('z-index', '-1000');
        $('#Documents .document-content-outer').css('z-index', '50');
    }
    if ($(".role-settings").length > 0) {
        $('#loading').css('z-index', '50');
        $('#loading').fadeOut(1000, 0, function () {
            $(".role-settings").css("z-index", "1");
            $(".role-settings").fadeIn(500);
        });
    }
});

function setBelegTableBody() {
    var displayHeight = $("#Documents .tab-content").height() - 43;
    $("#Documents .table-body").height(displayHeight);
}

function setColWidths(headerElements) {
    for (var i = 0; i < headerElements.length; i++) {
        var tempWidth = $(headerElements[i]).width();
        var classList = $(headerElements[i]).attr("class").split(/\s+/);
        $("." + classList[0]).width(tempWidth + 105);
    }
}

var timer;
jQuery(document).on("mouseover", ".belegBody tbody tr", function () {
    jQuery(this).siblings('.hover').removeClass('hover');
    jQuery(this).addClass('hover');
    var urlParam = location.href.split('/');
    var urlItem = urlParam[urlParam.length - 1];
    var that = $(this);
    clearTimeout(timer);
    timer = setTimeout(function () {
        jQuery('#document-detail').css("width", "100%");
        jQuery('#document-detail').css("height", "100%");
        if (!that.hasClass("remote")) {
            jQuery('#document-detail').parent().html('<img id="document-detail" style="margin-top: 150px; margin-left: 120px;" src="/Content/img/gif-load.gif" />');
        if (jQuery('#document-detail').length) {
            jQuery.get("/Documents/CheckThumbnailAfterEventClick", { id: that.find('.path').text(), filename: that.find('.thumbnail').text(), start: jQuery('.from').text() - 1, tobjId: urlItem }, function (path) {
                that.find('.thumbnail').text(path);
                jQuery.get("/Documents/Thumbnail", { filename: that.find('.thumbnail').text() }, function (file) {
                    jQuery('#document-detail').attr('src', 'data:image/jpg;base64,' + file.image);
                    jQuery('#document-detail').css("margin", "0");
                    jQuery('#previewImageBlock').zoom({ magnify: 2, callback: true, on: 'click' });
                    jQuery('#document-detail').css("width", "362px");
                    jQuery('#document-detail').css("height", "512px");
                });
            }).done();
        }
        }
    }, 500);
}).on("mouseout", function () {
    clearTimeout(timer);
}).on("click", ".belegBody tbody tr", function () {
    var that = $(this);
    if (!that.hasClass("remote")) {
    if (jQuery(this).hasClass('forexport')) {
        jQuery(this).removeClass('forexport');
    }
    else {
        jQuery(this).addClass('forexport');
    }
    }
});
jQuery(document).on("click", "img.download_belegs", function () {
    var $bobjs = $(document).find('.forexport');
    var virtualIdList = new Array();
    var arr = new Array();
    var urlParam = location.href.split('/');
    var urlItem = urlParam[urlParam.length - 1];
    $("#tobjId").val(urlItem);
    $bobjs.each(function () {
        arr.push($(this).find('.path.datagrid-value input')[0].value);
        $(this).removeClass('forexport');
    });
    jQuery(this).siblings('#ids').val(arr);
    if ($bobjs.length > 0) {
        var form = jQuery(this).parent();
        form.submit();
    }
    else {
        $.get("/Documents/DownloadWarning", function (warning) {
            $("#dialog").html(warning);
            $("#dialog").show();
            $("#dialog .dialog-btn").click(function () {
                $("#dialog").hide();
            });
        });
    }
});

//jQuery(document).on("click", "#document-detail", function () {
//    jQuery("#Documents tr.hover form").submit();
//});

jQuery(document).on("submit", "#IssueList .tab-content form", function (e) {
    return false;
});
var validateAndSave = (function (e) {
    var $target = jQuery(e);
    $target.parent().siblings('.report-parameter-error').hide();
    jQuery(document).trigger('click.dropdown');
    var $form = $(".reportoverview form");
    var action = $form.get(0).action;
    var issueId = action.substring(action.indexOf('ExecuteIssue/') + 13);
    var parid = $target.siblings('.item-id').text();
    //jQuery.get('/Datagrid/ValidateIssueParameterInput', { id: issueId, parameterId: parid, value: $target.val() }, function (html) {
    //    if (html != "") {
    //        if (html.search("login") != -1) {
    //            window.location = "/Account/LogOn";
    //        }
    //        if (html.toString().indexOf($target.val()) > 0) {
    //            $target.parent().siblings('.report-parameter-error').html(html);
    //            $target.parent().siblings('.report-parameter-error').show();
    //        }
    //    }
    //});
    //jQuery.get('/Datagrid/SaveParameterValueInHistory', { id: issueId, parameterId: parid, value: $target.val() });
    //getIssueRecordCount();
});
jQuery(document).on("ValidateAndSave", "#IssueList .report-parameter-validate", function (e) {
    validateAndSave(e.target);
});
trigger_dropdown = function (el) {
    jQuery(document).trigger('click.dropdown');
    jQuery(document).trigger('click.distinct');
    jQuery(document).trigger('click.reportdropdown');
    jQuery(document).trigger('click.reportgroup');
    var $input = jQuery(el);
    $input.datepicker("hide");
    var $menu = $input.siblings('.dropdown-menu-container').show();
    if ($menu.length == 0)
    {
        $menu = $input.parent().siblings('.dropdown-menu-container').show();
    }
    jQuery(document).bind('click.dropdown', function (e) {
        var isPropStopped = e.isPropagationStopped();
        if (!(typeof isPropStopped === 'undefined') && isPropStopped) return;
        $menu.hide();
        jQuery(document).unbind('click.dropdown');
    });
    $menu.find('.entry').bind('click.entry', function () {
        var value = jQuery(this).find('.value, .value-only').text();
        var comboValue = jQuery(this).find('.value, .value-only-hidden').text();
        var comboBox = jQuery(this).find('.value, .value-only').parent().parent().parent().parent().parent().parent().find('#selectionType');
        comboBox.find("option[value='" + comboValue + "']").selected();
        $input.val(value);
        // validateAndSave($input);
        //to save the value into history
        $input.removeClass('empty');
        jQuery(document).trigger('click.dropdown');
        $menu.find('.entry').unbind('click.entry');
    });
};
/**************************************************************************
* NOTIFICATION - BOXHEADER
**************************************************************************/
jQuery(document).on("click", ".edit-optimizations", function () {
    var id = jQuery(this).find('.opt-id').text();
    jQuery.get("/Header/ShowEditOptimizationTextDialog", { id: id }, function (html) {
        if (html.search("login") != -1) {
            window.location = "/Account/LogOn";
        }
        $dialog = jQuery('#dialog').show().html(html);
        if (jQuery('#dialog .dialog-outer-box').length) {
            jQuery('#dialog .dialog-outer-box').css("margin-top", -jQuery('#dialog .dialog-outer-box').height() / 2);
            $dialog.find('.dialog-btn').click(function () {
                $button = jQuery(this);
                if ($button.find('.data').text() == 'False') $dialog.hide();
                else if ($button.find('.data').text() == 'True') {
                    $country_code = $dialog.find('form .countryCode');
                    $dialog.find('form input[type=text]').each(function (i, e) {
                        jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                    });
                    $button.attr("disabled", "true");
                    $dialog.find('form').ajaxForm({
                        success: function () {
                            jQuery('#dialog').hide();
                            refreshBoxHeader();
                            return false;
                        }
                    });
                    $dialog.find('form').attr("action", "/Header/ChangedOptimizationTexts");
                    $dialog.find('form').submit();
                }
            });
        } else {
            $dialog.find('.dialog-btn').click(function () {
                $dialog.hide();
            });
        }
        $dialog.find('form select[name=language]').change(function () {
            $country_code = $dialog.find('form .countryCode');
            $select = jQuery(this);
            $dialog.find('form input[type=text]').each(function (i, e) {
                jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                jQuery(e).val($dialog.find('form .' + $select.val() + "_" + jQuery(e).attr('name')).val());
            });
            $country_code.val($select.val());
        });
    });
});
jQuery(document).on("click", "#notification-container", function (e) {
    e.stopPropagation();
});
jQuery(document).on("click", "#notification-container .close-popup", function (e) {
    jQuery(document).trigger('click.notification-container');
});
jQuery(document).on("click", "#notification .button", function () {
    jQuery(document).trigger('click.notification-container');
    var $notification = jQuery('#notification-container').fadeIn();
    jQuery(document).bind('click.notification-container', function (e) {
        if (e.isPropagationStopped()) return;
        $notification.fadeOut(100);
        jQuery(document).unbind('click.notification-container');
    });
    Updater.Pull();
    return false;
});
jQuery(document).on("click", "#notification .tabbar .tabheader", function () {
    var $this = jQuery(this);
    if ($this.hasClass('selected')) return;
    if ($this.hasClass('latest')) {
        jQuery('#notification-container .middle').removeClass('history').addClass('latest');
        jQuery('#notification-container .tabbar .history').removeClass('selected');
    } else {
        jQuery('#notification-container .middle').removeClass('latest').addClass('history');
        jQuery('#notification-container .tabbar .latest').removeClass('selected');
    }
    $this.addClass('selected');
});
jQuery(document).on("click", "#notification-container .middle .box a.link", function () {
    var $link = jQuery(this).hasClass('link') ? jQuery(this) : null;
    var key = jQuery(this).prev(".key").text();
    delete Updater.Listeners[key];
    jQuery.get("/Command/RemoveJob", { key: key }, function () {
        if ($link != null && typeof $link.attr('href') !== 'undefined') location.href = $link.attr('href');
    });
    return false;
});
jQuery(document).on("click", "#notification-container .more .text", function () {
    var max_entries = jQuery(this).parent().prevAll('.max-entries');
    max_entries.text(parseInt(max_entries.text()) + 5);
    Updater.Pull();
});
jQuery(document).on("click", ".optimizations a", function (e) {
    jQuery.blockUI({
        css: { opacity: 0.0 }
    });
});

/**************************************************************************
* Click n Click 2
**************************************************************************/
jQuery(document).on("click", ".clicknclick-box2", function (e) {
    e.stopPropagation();
});
jQuery(document).on("click", "#DataGrid div.clicknclick a", function (ev) {
    var click = $(this);
    ev.stopPropagation();
    if (click.text().trim() != "") {
    jQuery(document).trigger('click.clicknclick');
    var $menu;
    var url = jQuery(this).next(".url");

    if (url.attr("belegarchive") == "true") {
        var conds = new Array();
        url.find(".target").each(function (i, e) {
            var el = jQuery(e);
            conds.push("Equal(%" + el.attr("colid") + ",\"" + el.text() + "\")");
        });
        var table_key = url.find(".key").text();
        var filter = encodeURIComponent("And(" + conds.join(",") + ")");

            if ($(".document-preview").length > 1) {
                $($(".document-preview")[1]).remove();
            }

        $(".document-preview").dialog({
            modal: true,
                hide: 'fade',
                show: 'fade',
            width: 850,
            height: 675,
            resizable: false,
            draggable: false,
            closeOnEscape: true,
            close: function (event, ui) {
                    //$(".document-preview").hide(function () {
                    //    var wrapper = jQuery('.document-preview .preview .wrapper');
                    //    wrapper.empty();
                    //    wrapper.hide();
                    //    var loader = jQuery('.document-preview .preview .loader');
                    //    loader.show();
                    //});
                    var wrapper = jQuery('.document-preview .preview .wrapper');
                    wrapper.empty();
                    wrapper.hide();
                    var loader = jQuery('.document-preview .preview .loader');
                    loader.show();
            }
        });

            jQuery.get("/Documents/GetDocumentsOverView?id=" + table_key + "&filter=" + filter, function (html) {
                
                var wrapper = jQuery('.document-preview .preview .wrapper');
                wrapper.html(html);
        });

        jQuery(".document-preview #go-to-archiv").click(function () {
                var link = "/DataGrid/SortAndFilter/" + table_key + "?relationType=3&candc=True&filter=" + filter;
            jQuery('body .document-preview').dialog("close");
            jQuery.get(link, function (html) {
                jQuery('body > .clicknclick-box2').remove();
                $dialog = jQuery('#dialog');
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                $dialog.html(html).show();
                $('#DataGrid .content-inner').hide();
                $dialog.find('.dialog-btn').click(function () {
                    delete Updater.Listeners[key];
                    jQuery('#dialog').hide();
                    $('#DataGrid .content-inner').show();
                });
                var key = $dialog.find('.key').text();
                Updater.Listeners[key] = function ($notification) {
                        location.href = $notification.find('a.link').attr('href');
                };
            });

        });
        return false;
    }
        else {
    if (url.hasClass("immediate")) {
        jQuery.get(url.text(), function (html) {
            var $dialog = jQuery('#dialog');
                    if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html).show();
            $('#DataGrid .content-inner').hide();
            $dialog.find('.dialog-btn').click(function () {
                delete Updater.Listeners[key];
                jQuery('#dialog').hide();
                $('#DataGrid .content-inner').show();
            });
            var key = $dialog.find('.key').text();
                    Updater.Listeners[key] = function ($notification) {
                var href = $notification.find('a.link').attr('href');
                if (href === null || href === "") {
                    jQuery("#dialog").hide();
                    $('#DataGrid .content-inner').show();
                    return;
                }
                location.href = href;
            };
        });
        return false;
    }
    jQuery.get(url.text(), function (html) {
            var temp = $(html);
            $menu = temp.first();
            $preview = temp.last();

            var body_clicknclick = jQuery('body > .clicknclick-box2');
            if (body_clicknclick[0]) return;
            if (body_clicknclick.length > 0) $menu = body_clicknclick;
            else $menu.appendTo('body');

            var body_preview = jQuery('body > .document-preview');
            if (body_preview.length > 0) $preview = body_preview;
            else $preview.appendTo('body');

        SetLeftRightElementHeight();
        if (ev.pageX + $menu.width() <= jQuery(window).width()) {
            if (ev.pageY + $menu.height() + 35 < jQuery(document).height()) {
                $menu.addClass("left-side");
                $menu.css("left", ev.pageX - 35);
                $menu.find(".background-arrow").css("background-position", "10px 50%");
                $menu.find(".background-arrow-down").css("display", "none");
                $menu.find(".background-arrow").css("display", "block");
            } else {
                $menu.addClass("left-side");
                $menu.css("left", ev.pageX - 35);
                var padding = 5 * 2;
                var topPos = $menu.height() + padding;
                $menu.find(".background-arrow-down").css("background-position", "15px 50%");
                $menu.find(".background-arrow-down").css("top", topPos);
                $menu.find(".background-arrow").css("display", "none");
                $menu.find(".background-arrow-down").css("display", "block");
            }
        } else {
            if (ev.pageY + $menu.height() + 35 < jQuery(document).height()) {
                $menu.css("z-index", 0);
                $menu.show();
                var width = jQuery(".clicknclick-box2 ").width();
                if (ev.pageX + width > jQuery(document).width()) {
                    $menu.css("left", ev.pageX - width);
                    $menu.find(".background-arrow").css("left", "321px");
                }
                else {
                    $menu.css("left", ev.pageX - 35);
                }
                $menu.hide();
                $menu.css("z-index", "10002");
                $menu.removeClass("left-side");
                $menu.find(".background-arrow").css("background-position", "15px 50%");
                $menu.find(".background-arrow-down").css("display", "none");
                $menu.find(".background-arrow").css("display", "block");
            } else {
                $menu.addClass("left-side");
                if (ev.pageX + $menu.width() > jQuery(document).width()) {
                    $menu.css("left", ev.pageX - $menu.width());
                    $menu.find(".background-arrow-down").css("left", "321px");
                }
                else {
                    $menu.css("left", ev.pageX - 35);
                }
                var padding = 5 * 2;
                var topPos = $menu.height() + padding;
                $menu.find(".background-arrow-down").css("background-position", "15px 50%");
                $menu.find(".background-arrow-down").css("top", topPos);
                $menu.find(".background-arrow").css("display", "none");
                $menu.find(".background-arrow-down").css("display", "block");
            }
        }
        if (ev.pageY + $menu.height() + 35 < jQuery(document).height()) {
            $menu.css("top", ev.pageY + 10);
        } else {
            $menu.css("top", ev.pageY - $menu.height() - 40);
        }
        $menu.addClass("active");
        $menu.fadeIn();
    
    });
    jQuery(document).bind('click.clicknclick', function (e) {
        if (e.isPropagationStopped()) return;
        var body_clicknclick = jQuery('body > .clicknclick-box2');
        if (body_clicknclick.length > 0) $menu = body_clicknclick.remove();
        jQuery(document).unbind('click.clicknclick');
    });
        }
    }
    return false;
});
function SetLeftRightElementHeight() {
    $(document).find(".scroll-left").css('height', 'auto');
    var boxHeight = $(document).find(".clicknclick-box2 .head").height() + $(document).find(".scroll-left").height() + 25
        + $(document).find(".clicknclick-box2 .back").height() + 25;
    if (boxHeight > 370)
        boxHeight = 370;
    var innerHeight = boxHeight - $(document).find(".clicknclick-box2 .head").height() - $(document).find(".search-panel").height() - $(document).find(".clicknclick-box2 .back").height() - 30;
    $(document).find(".clicknclick-box2").height(boxHeight);
    if (innerHeight > 297)
        $(document).find(".scroll-left").height(innerHeight);
    else
        $(document).find(".scroll-left").css('height', 'auto');
    $(document).find(".contentrelationpartial").height(innerHeight);
}

function SetArrowDownPosition(ev) {
    var offset = $(document).find(".background-arrow-down").offset();
    var $menu = jQuery('body > .clicknclick-box2');
        var topY = offset.top - $menu.height() - 20;
        $menu.css("top", topY);
        $(document).find(".background-arrow-down").offset({ top: offset.top, left: offset.left });
    }

function SetBoxPositionToArrowDown(ev) {
    var $menu = jQuery('body > .clicknclick-box2');
    var offset = $(document).find(".background-arrow-down").offset();
    var topY = offset.top - $menu.height() - 20;
    $menu.css("top", topY);
    $(document).find(".background-arrow-down").offset({ top: offset.top, left: offset.left });
}

function ChangeTopArrow(ev) {
    var $menu = $(document).find(".clicknclick-box2");
    var offset = $(document).find(".background-arrow").offset();
    var arrowTop = offset.top - 20;
    $(document).find(".background-arrow-down").css("display", "block");
    $(document).find(".background-arrow").css("display", "none");
    var topY = arrowTop - $menu.height() - 20;
    $menu.css("top", topY);
    $(document).find(".background-arrow-down").offset({ top: offset.top - 20, left: offset.left - 25 });
}

jQuery(document).on("click", ".clicknclick-box2.active .close", function () {
    jQuery('body > .clicknclick-box2').remove();
    jQuery('body > .document-preview').remove();
});
jQuery(document).on("click", ".clicknclick-box2.active .table", function () {
    var rowNo = jQuery(this).parent().find(".row_no").text();
    var values = jQuery(".datagrid tbody td.item-id:contains(" + rowNo + ")").parent().find(".datagrid-value");
    var link = jQuery(this).find(".link").text();
    var table_key = link.substring(link.lastIndexOf("/") + 1, link.indexOf("?"));
    var targetName = $(this).find('.name').text().toLowerCase();
    var conds = new Array();
    jQuery(this).find(".target").each(function (i, e) {
        var el = jQuery(e);
        conds.push("Equal(%" + el.text() + ",\"" + jQuery(values[el.next().text()]).text() + "\")");
    });
    var filter = encodeURIComponent("And(" + conds.join(",") + ")");
    $new_tab = jQuery(this).find(".is_new_tab").text();
    var isDynamic = $(this).find(".is_dynamic").text();
    var isHtmlRender = $(this).find(".is_belegarchiv").text();
    if (isDynamic === "true") {
        link += "?rowNo=" + rowNo;
        jQuery(this).find(".source").each(function () {
            link += "&sourceColumnId=" + $(this).text();
        });
        jQuery(this).find(".target").each(function () {
            link += "&targetColumnId=" + $(this).text();
        });

        jQuery.get("/IssueList/GetValidationDialog", function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            jQuery('#dialog').html(html).show(0, function () {
                jQuery('body > .clicknclick-box2').remove();
                jQuery.get(link, function (html) {
                    jQuery('#dialog').html(html);
                    var $dialog = jQuery('#dialog');
                    var link = $dialog.find('.link');
                    if (link.length) location.href = link.text();
                    var key = $dialog.find('.key').text();
                    $('#DataGrid .content-inner').hide();
                    $dialog.show().find('.dialog-btn').click(function () {
                        delete Updater.Listeners[key];
                        $('#loading').css('z-index', '-1000');
                        jQuery('#dialog').hide();
                        $('#DataGrid .content-inner').show();
                    });
                    Updater.Listeners[key] = function ($notification) {
                        location.href = $notification.find('a.link').attr('href');
                    };
                })
                return false;
            });
        });
    }
    else if (targetName !== "belegarchiv" || isHtmlRender === "false") {
        link += "&filter=" + filter;
        jQuery.get(link, function (html) {
            jQuery('body > .clicknclick-box2').remove();
            jQuery('body > .document-preview').remove();
            $dialog = jQuery('#dialog');
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html).show();
            $('#DataGrid .content-inner').hide();
            $dialog.find('.dialog-btn').click(function () {
                delete Updater.Listeners[key];
                jQuery('#dialog').hide();
                $('#DataGrid .content-inner').show();
            });
            var key = $dialog.find('.key').text();
            Updater.Listeners[key] = function ($notification) {
                if ($new_tab == "true") {
                    window.open($notification.find('a.link').attr('href'), '_blank');
                    jQuery('#dialog').hide();
                    $('#DataGrid .content-inner').show();
                } else {
                    location.href = $notification.find('a.link').attr('href');
                }
            };
        });
    }
    else {
        $(".document-preview").dialog({
            modal: true,
            hide: 'blind',
            show: 'blind',
            width: 850,
            height: 675,
            resizable: false,
            draggable: false,
            closeOnEscape: true,
            close: function (event, ui) {
                var wrapper = jQuery('.document-preview .preview .wrapper');
                wrapper.empty();
                wrapper.hide();
                var loader = jQuery('.document-preview .preview .loader');
                loader.show();
            }
        });

        jQuery.get("/Documents/GetDocumentsOverView?id=" + table_key + "&filter=" + filter, function (html) {

            var wrapper = jQuery('.document-preview .preview .wrapper');
            wrapper.html(html);
        });

        jQuery(".document-preview #go-to-archiv").click(function () {
        link += "&filter=" + filter;
            jQuery('body .document-preview').dialog("close");
        jQuery.get(link, function (html) {
            jQuery('body > .clicknclick-box2').remove();
            $dialog = jQuery('#dialog');
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html).show();
            $('#DataGrid .content-inner').hide();
            $dialog.find('.dialog-btn').click(function () {
                delete Updater.Listeners[key];
                jQuery('#dialog').hide();
                $('#DataGrid .content-inner').show();
            });
            var key = $dialog.find('.key').text();
            Updater.Listeners[key] = function ($notification) {
                if ($new_tab == "true") {
                    window.open($notification.find('a.link').attr('href'), '_blank');
                    jQuery('#dialog').hide();
                    $('#DataGrid .content-inner').show();
                } else {
                    location.href = $notification.find('a.link').attr('href');
                }
            };
        });

        });
    }
    return false;
});
jQuery(document).on("click", ".clicknclick-line", function () {
    jQuery(".clicknclick-line-link" + jQuery(this).attr("refRow")).trigger("click");
});
jQuery(document).on("click", ".clicknclick-box2.active .description", function (ev) {
    jQuery(".clicknclick-box2.active .first-description").hide();
    jQuery(this).parent().addClass("active");
    jQuery(this).parent().find(".table").show();
    jQuery(".clicknclick-box2.active .description").hide();
    jQuery(".search-panel .search-clicknclick-panel input").val(jQuery(".search-panel .search-clicknclick-panel input").attr("placeholder")).removeClass("dirty");
    jQuery(".search-panel").show();
    $(document).find(".search-panel .visible")[0].value = "1";
    SetLeftRightElementHeight();
    if ($(document).find(".background-arrow-down")[0].style.display != "none")
        SetArrowDownPosition(ev);
    else {
        var $menu = jQuery('body > .clicknclick-box2');
        var offset = $(document).find(".background-arrow").offset();
        if (offset.top + $menu.height() + 50 >= jQuery(document).height())
            ChangeTopArrow(ev);
    }
});
jQuery(document).on("click", ".clicknclick-box2.active .back", function (ev) {
    jQuery(".clicknclick-box2.active .first-description").show();
    jQuery(".clicknclick-box2.active .option.active .no-tables").hide();
    jQuery(".clicknclick-box2.active .container-left .option").removeClass("active");
    jQuery(".clicknclick-box2.active .table").hide();
    jQuery(".clicknclick-box2.active .description").show();
    jQuery(".search-panel .search-clicknclick-panel input").val(jQuery(".search-panel .search-clicknclick-panel input").attr("placeholder")).removeClass("dirty");
    jQuery(".search-panel").hide().val();
    $(document).find(".search-panel .visible")[0].value = "0";
    SetLeftRightElementHeight();
    if ($(document).find(".background-arrow-down")[0].style.display != "none")
        SetBoxPositionToArrowDown(ev);
});
jQuery(document).on("mouseover", ".clicknclick-box2.active .table, .clicknclick-box2.active .description", function () {
    jQuery(this).addClass("hover");
});
jQuery(document).on("mouseout", ".clicknclick-box2.active .table, .clicknclick-box2.active .description", function () {
    jQuery(this).removeClass("hover");
});
jQuery(document).on("keyup", ".search-clicknclick-panel input[type=text]", function () {
    var value = jQuery(this).val();
    var count = 0;
    jQuery(".clicknclick-box2.active .option.active .table").each(function (i, e) {
        var el = jQuery(e);
        if (el.find(".name").text().toLowerCase().indexOf(value.toLowerCase()) > -1) {
            el.show();
            count++;
        } else {
            el.hide();
        }
    });
    if (count == 0) {
        jQuery(".clicknclick-box2.active .option.active .no-tables").show();
        $(".scroll-left").css("overflow", "hidden");
    } else {
        jQuery(".clicknclick-box2.active .option.active .no-tables").hide();
        $(".scroll-left").css("overflow", "auto");
    }   

    var currentHeight = $(".clicknclick-box2").height()
        - $(".clicknclick-box2 .head").height()
        - parseInt($(".clicknclick-box2 .head").css("margin-bottom"))
        - $(".clicknclick-box2 .search-panel").height()
        - 7;
    if (currentHeight > 270)
        currentHeight = 270;

    var width = $(".scroll-left").width()
    $(".no-tables").height(currentHeight);
    $(".no-tables").width(width);
    $(".no-tables .innerBox").height(currentHeight);
    $(".no-tables .innerBox").width(width);
});
//----------------------------------------------------------------
jQuery(document).on("click", "#showOneReportItem a.viewoptions", function (e) {
    location.href = this.attributes['href'].value;
});
// for avoid closing the popup window
jQuery(document).on("click", "#reportlistpopupcontainer .search-opt-panel input.bg-middle", function (e) {
    return false;
});
reportListHeaderFunction = function () {
    jQuery(document).on("click", "#reportlistpopupcontainer .search-opt-panel .submit,#reportlistpopupcontainer .reporttabheader", function (e) {
        var showHidden = jQuery(this).hasClass('show_hidden');
        var searchElement = jQuery('#reportlistpopupcontainer input[type=text]');
        var search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) search = '';
        var size = jQuery('.header .nav-controls .size').text();
        var sortColumn = jQuery('.header .nav-controls .sortColumn').text();
        var direction = jQuery('.header .nav-controls .direction').text();
        jQuery.get('/IssueList/IndexList', { json: true, size: size, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: false, showHidden: showHidden, showArchived: false },
            function (html) {
                var tabbar = jQuery('#reportlistpopupcontainer .tabbar', html);
                var resultlist = jQuery('#resultlist', html);
                jQuery('#reportlistpopupcontainer .tabbar').html(tabbar);
                jQuery('#resultlist').html(resultlist);
            }
        );
        return false;
    });
}();
/* ---------------------------------------------------------------------------------------------------
INDEX VIEW OPTION
------------------------------------------------------------------------------------------------------*/
/// removes hightlighted background for all columns who have indexes
clearIndexedColumnsHighlight = function () {
    var columnRows = $('#viewOptionPartial_columnList *[data-tableObjectId]');
    $.each(columnRows, function (i, columnRow) {
        var tr = $(columnRow).closest('tr');
        if (tr.hasClass('column-with-index-shown'))
            tr.removeClass("column-with-index-shown");
    });
};
highlightIndexedColumns = function (columnName) {
    clearIndexedColumnsHighlight();
    var indexNameElements = $('#columnIndexView_content *[data-indexName]');
    var columnRows = $('#viewOptionPartial_columnList *[data-tableObjectId]');
    $.each(indexNameElements, function (i, indexNameElement) {
        var currentIndexName = $(indexNameElement).data('indexname');
        $.each(columnRows, function (j, columnRow) {
            $.each($(columnRow).data('indexesofcolumn'), function (j, currentColumnIndexName) {
                if (currentIndexName == currentColumnIndexName)
                    $(columnRow).closest('tr').addClass("column-with-index-shown");
            });
        });
    });
};
showIndexView = function (show) {
    var indexContainer = $('#gesamt #DataGrid.page-content #columnIndexView');
    var columnContainer = $('#gesamt #DataGrid.page-content #viewoptions.dropdown div.scroll');
    if (show) {
        indexContainer.css('display', 'block');
        indexContainer.height('21%');
        columnContainer.height('73%');
    }
    else {
        clearIndexedColumnsHighlight();
        indexContainer.css('display', 'none');
        indexContainer.height('0%');
        columnContainer.height('98%');
        $.Index_LastColumnName = null;
    }
};
// shows indexes in the view options partial view for tables/views/arcgives
showIndexFunction = function () {
    $('#gesamt #DataGrid.page-content #columnIndexView').css('display', 'none');
    // hides the view index when the view is enlarged
    jQuery(document).on("click", ".viewoptions-enlarge", function () {
        var $viewoptions = jQuery("#viewoptions .dropdown-menu");
        return false;
    });
    jQuery(document).on("click", "#gesamt #DataGrid.page-content #viewoptions.dropdown #closeIndexViewOption", function () {
        showIndexView(false);
        return false;
    });
    // display the index view handler
    jQuery(document).on("click", "#gesamt #DataGrid.page-content #viewoptions.dropdown a.column-index-info", function (e) {
        // gets the id and the column name
        var tableId = $(this).data('tableobjectid'); // gets the id of the TableObject
        var colName = $(this).data('columnname');
        var columnContainer = $('#gesamt #DataGrid.page-content #viewoptions.dropdown div.scroll');
        var indexContainer = $('#gesamt #DataGrid.page-content #columnIndexView');
        // toggle the index view div and adapt the height considering own space of the column view div
        if (indexContainer.css('display') != 'block') {
            $.Index_LastColumnName = colName; // keeps track of tha last column clicked
            showIndexView(true);
        }
            // if the user clicks on the same column name then hide the div
        else if (indexContainer.css('display') == 'block' &&
            (!(typeof $.Index_LastColumnName === 'undefined') && $.Index_LastColumnName == colName)) {
            showIndexView(false);
            $.Index_LastColumnName = null;
            return false;
        }
        else {
            $.Index_LastColumnName = colName;
        }
        // ajax call  + callback
        jQuery.get('/DataGrid/TableIndexes', { json: true, id: tableId, columnName: colName },
            function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                $("#columnIndexView").html(html);
                highlightIndexedColumns($.Index_LastColumnName);
            }
        );
        return false;
    });
}();

/* --------- END INDEX VIEW OPTION    -------------------------------------------------------------- */
jQuery(document).on("mouseover", "#showOneReportItem tr", function () {
    jQuery(this).addClass('hover');
}).on("mouseout", "#showOneReportItem tr", function () {
    jQuery(this).removeClass('hover');
}).on("click", "#showOneReportItem tr", function () {
    var $prev = jQuery(this).addClass('aktiv').siblings('.aktiv');
    $prev.removeClass('aktiv');
    return false;
});
/* Show / Hide */
jQuery(document).on("click", "#resultlist td.toparams3 .hide-table a", function () {
    var id = jQuery(this).prev().text();
    var parent_tr = jQuery(this).parent().parent().parent().parent();
    var tab = jQuery('.tabbar .selected');
    var showHidden = tab.hasClass('show_hidden');
    var searchElement = jQuery('#reportlistpopupcontainer .search-opt-panel input[type=text]');
    var search = searchElement.val();
    var placeholder = searchElement.attr('placeholder');
    if (search == placeholder) search = '';
    var sortColumn = jQuery('.header .nav-controls .sortColumn').text();
    var direction = jQuery('.header .nav-controls .direction').text();
    var page = 0;
    var size = 25;
    var url = jQuery('.page-content')[0].id;
    jQuery.get('/Command/UpdateTable/',
        { id: id, json: true, search: search, showHidden: showHidden, showEmpty: false, showArchived: false, catRefresh: true, sortColumn: sortColumn, direction: direction, url: url, size: size, page: page },
        function () {
            jQuery.get('/IssueList/IndexList', { json: true, size: size, page: page, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: false, showHidden: showHidden, showArchived: false },
                function (html) {
                    var tabbar = jQuery('#reportlistpopupcontainer .tabbar', html);
                    var resultlist = jQuery('#resultlist', html);
                    jQuery('#reportlistpopupcontainer .tabbar').html(tabbar);
                    jQuery('#resultlist').html(resultlist);
                    $dialog.hide();
                    return false;
                }
            );
            parent_tr.fadeout();
            return false;
        }
    );
    return false;
});
/* Delete */
jQuery(document).on("click", "#resultlist td.toparams3 .delete a", function () {
    var $dialog = jQuery('#dialog').show();
    var id = jQuery(this).prev().text();
    jQuery.get('/IssueList/DeleteConfirmationDialog/', function (dialog_html) {
        if (dialog_html.search("login") != -1) {
            window.location = "/Account/LogOn";
        }
        $dialog.html(dialog_html);
        $dialog.find('.dialog-btn').click(function () {
            $button = jQuery(this);
            if ($button.find('.data').text() == 'False') {
                $dialog.hide();
                return false;
            } else if ($button.find('.data').text() == 'True') {
                $dialog.find('.dialog-btn').hide();
                var searchElement = jQuery('#reportlistpopupcontainer .search-opt-panel input[type=text]');
                var search = searchElement.val();
                var placeholder = searchElement.attr('placeholder');
                if (search == placeholder) search = '';
                var sortColumn = jQuery('.header .nav-controls .sortColumn').text();
                var direction = jQuery('.header .nav-controls .direction').text();
                var page = 0;
                var size = 25;
                jQuery.get('/IssueList/DeleteIssue/', { id: id, json: true, search: search, catRefresh: true, sortColumn: sortColumn, direction: direction, size: size, page: page },
                    function (html) {
                        if (html == "") {
                            window.location = "/Home/Index";
                        } else {
                            jQuery('.content-box').replaceWith(html);
                            $dialog.html('').hide();
                            jQuery('#reportlistpopupcontainer a.dropdown-btn').click();
                        }
                    }
                );
                var tab = jQuery('.tabbar .selected');
                var showHidden = tab.hasClass('show_hidden');
                jQuery.get('/IssueList/IndexList', { json: true, size: size, page: page, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: false, showHidden: showHidden, showArchived: false },
                    function (html) {
                        var tabbar = jQuery('#reportlistpopupcontainer .tabbar', html);
                        var resultlist = jQuery('#resultlist', html);
                        jQuery('#reportlistpopupcontainer .tabbar').html(tabbar);
                        jQuery('#resultlist').html(resultlist);
                    }
                );
                return false;
            }
            return false;
        });
    });
    return false;
});
/* Edit */
jQuery(document).on("click", "#resultlist td.toparams3 .edit a", function () {
    var id = jQuery(this).prev().text();
    jQuery.get('/IssueList/EditIssueDialog/', { id: id }, function (dialog_html) {
        if (dialog_html.search("login") != -1) {
            window.location = "/Account/LogOn";
        }
        $dialog = jQuery('#dialog').show().html(dialog_html);
        $outer_box = $dialog.find('.dialog-outer-box');
        $outer_box.css("margin-top", -$outer_box.height() / 2);
        $dialog.find('.dialog-btn').click(function () {
            $button = jQuery(this);
            if ($button.find('.data').text() == 'False') {
                $dialog.hide();
                return false;
            } else if ($button.find('.data').text() == 'True') {
                var searchElement = jQuery('#searchoverview input[type=text]');
                var search = searchElement.val();
                var placeholder = searchElement.attr('placeholder');
                if (search == placeholder) search = '';
                var sortColumn = jQuery('.header .nav-controls .sortColumn').text();
                var direction = jQuery('.header .nav-controls .direction').text();
                var page = jQuery('.header .nav-controls .page').text();
                var size = jQuery('.header .nav-controls .size').text();
                if (!page) page = 0;
                if (!size) size = 25;
                $country_code = $dialog.find('form .countryCode');
                $dialog.find('form input[type=text]').each(function (i, e) {
                    jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                });
                $button.attr("disabled", "true");
                $form = $dialog.find('form');
                $form.get(0).action += "/IssueList/EditIssue?id=" + id + "&json=true&search=" + search + "&catRefresh=true&sortColumn=" +
                                    sortColumn + "&direction=" + direction + "&size=" + size + "&page=" + page;
                $form.find('input [type=text]').remove();
                $form.find('input.countryCode').remove();
                $form.ajaxForm({
                    success: function (html) {
                        if (html.search("login") != -1) {
                            window.location = "/Account/LogOn";
                        }
                        var tabbar = jQuery('#reportlistpopupcontainer .tabbar', html);
                        var resultlist = jQuery('#resultlist', html);
                        jQuery('#reportlistpopupcontainer .tabbar').html(tabbar);
                        jQuery('#resultlist').html(resultlist);
                        $dialog.hide();
                        jQuery('#reportlistpopupcontainer a.dropdown-btn').click();
                        return false;
                    }
                });
                $form.submit();
                return false;
            }
            return false;
        });
        $dialog.find('form select[name=language]').change(function () {
            $country_code = $dialog.find('form .countryCode');
            $select = jQuery(this);
            $dialog.find('form input[type=text]').each(function (i, e) {
                jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                jQuery(e).val($dialog.find('form .' + $select.val() + "_" + jQuery(e).attr('name')).val());
            });
            $country_code.val($select.val());
        });
        return false;
    });
    return false;
});
function SetContentTop() {
    var item = jQuery(".master-header");
    if (item.length > 0) {
        var height = jQuery(".master-header")[0].offsetHeight;
        jQuery("#set-top").css("top", height);
        setBelegTableBody();
    }
}
jQuery(document).on("keydown", "input.report-parameter-validate", function (e) {
    if (e.keyCode == 13) {
        return false;
    }
    return true;
});
jQuery(document).on("click", "#ArchiveDocuments .showviewoptions", function (e) {
    var form = jQuery(e.target).parent().parent().parent().parent().find('form');
    form.ajaxSubmit(long_running_options);
    return false;
});
jQuery(document).on("click", "#clear-report", function () {
    var inputs = $('#execute_report').find('input[type=text]');
    for (var i = 0; i < inputs.length; i++) {
        $(inputs[i]).val($(inputs[i]).attr('placeholder'));
        $(inputs[i]).addClass('empty');
        $(inputs[i]).trigger('validate');
    }
});
jQuery(document).on("click", ".immediate-filter #clear-report", function () {
    jQuery(this).siblings('#open-report').trigger('click');
});
jQuery(document).on("click", ".immediate-filter #open-report", function () {
    jQuery(".immediate-filter-controller").trigger('click');
});
//Dialogs for getting blueline documents
jQuery(document).on("click", ".get-blue-line-documents a", function () {
    var $link = jQuery(this);
    jQuery.get($link.attr('href'), function (html) {
        if (html.search("login") != -1) {
            window.location = "/Account/LogOn";
        }
        var $dialog = jQuery('#dialog').html(html).show();
        var key = $dialog.find('.key').text();
        $('#DataGrid .content-inner').hide();
        $dialog.find('.dialog-btn').click(function () {
            delete Updater.Listeners[key];
            jQuery('#dialog').hide();
            $('#DataGrid .content-inner').show();
        });
        Updater.Listeners[key] = function ($notification) {
            location.href = $notification.find('a.link').attr('href');
        };
    });
    return false;
});
jQuery(document).on("click", ".hide-every-unreachable-tables a", function () {
    var $link = jQuery(this);
    jQuery.get($link.attr('href'), function (html) {
        if (html.search("login") != -1) {
            window.location = "/Account/LogOn";
        }
        var $dialog = jQuery('#dialog').html(html).show();
        var key = $dialog.find('.key').text();
        $('#DataGrid .content-inner').hide();
        $dialog.find('.dialog-btn').click(function () {
            delete Updater.Listeners[key];
            jQuery('#dialog').hide();
            $('#DataGrid .content-inner').show();
        });
        Updater.Listeners[key] = function ($notification) {
            location.href = $notification.find('a.link').attr('href');
        };
    });
    return false;
});
//Dialogs for getting blueline documents
jQuery(document).on("click", ".download-blueline-documents a", function () {
    var $link = jQuery(this);
    jQuery.get($link.attr('href'), function (html) {
        if (html.search("login") != -1) {
            window.location = "/Account/LogOn";
        }
        var $dialog = jQuery('#dialog').html(html).show();
        var key = $dialog.find('.key').text();
        $('#DataGrid .content-inner').hide();
        $dialog.find('.dialog-btn').click(function () {
            delete Updater.Listeners[key];
            jQuery('#dialog').hide();
            $('#DataGrid .content-inner').show();
        });
        Updater.Listeners[key] = function ($notification) {
            location.href = $notification.find('a.link').attr('href');
        };
    });
    return false;
});
/**************************************************************************
* don't follow fake links (this block has to be the last one)
**************************************************************************/
jQuery(document).on("click", "a", function (e) {
    return !e.target.attributes['href'] || e.target.attributes['href'].value != '#';
});
jQuery(document).on("click", "#showdownload-xml", function () {
    var elements = [];
    $(".parameter-table .parameter").each(function () {
        if ($(this).find('.parameter-item').length > 1) {
            if (!($(this).find('span').hasClass('disabled-parameter'))) {

                if ($($(this).first().find("span")[0]).text().indexOf("von") > -1) {
                    var valueV = $($(this).find("input")[2]);
                    if (valueV.attr('id') == 'itemId') {
                        valueV = $($(this).find("input")[1]);
                    }
                    var elementV = {
                        Name: $($(this).first().find("span")[0]).text().replace(" von:", ""),
                        Value: valueV.val(),
                        IsVonBisParameter: true,
                        IsVon: true
                    };
                    elements.push(elementV);
                }
                if ($($(this).first().find("span")[1]).text().indexOf("Bis") > -1) {
                    var valueB = $($(this).find("input")[3]);
                    if (valueB.attr('id') == 'itemId') {
                        valueB = $($(this).find("input")[4]);
                    }
                    var elementB = {
                        Name: $($(this).first().find("span")[0]).text().replace(" von:", ""),
                        Value: valueB.val(),
                        IsVonBisParameter: true,
                        IsVon: false
                    };
                    elements.push(elementB);
                }
            }
        } else {
            if (!($(this).find('span').hasClass('disabled-parameter'))) {
                var valueN = $($(this).find(".parameter-item input")[1]);
                var nameN = $(this).find("span").text();
                var elementN = {
                    Name: nameN.substring(0, nameN.length - 1),
                    Value: valueN.val(),
                    IsVonBisParameter: false,
                    IsVon: false
                };
                elements.push(elementN);
            }
        }
    });
    var System = "";
    if ($('li.panel.System .panellink').text().length > 0) {
        System = '\t<System>' + $('li.panel.System .panellink').text() + '</System>\r\n';
    }
    var MANDT = "";
    if ($('li.panel.IndexTable .panellink').text().length > 0) {
        MANDT = '\t<MANDT>' + $('li.panel.IndexTable .panellink').text() + '</MANDT>\r\n';
    }
    var BUKRS = "";
    if ($('li.panel.SplitTable .panellink').text().length > 0) {
        BUKRS = '\t<BUKRS>' + $('li.panel.SplitTable .panellink').text() + '</BUKRS>\r\n';
    }
    var GJAHR = "";
    if ($('li.panel.SortColumn .panellink').text().length > 0) {
        GJAHR = '\t<GJAHR>' + $('li.panel.SortColumn .panellink').text() + '</GJAHR>\r\n';
    }
    var issueName = $('.reporttablename .last-name').val();
    var filedata = '<?xml version="1.0" encoding="utf-8"?>\r\n' +
                      '<Parameter xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">\r\n' +
                      '\t<IssueName>' + issueName + '</IssueName>\r\n' +
                      System + MANDT + BUKRS + GJAHR +
                     '\t<RowCount></RowCount>\r\n' +
                     '\t<Parameters>\r\n';
    for (var i = 0; i < elements.length; i++) {
        if (typeof elements[i].Value === "undefined") {
            continue;
        }
        if (typeof elements[i].Name === "undefined") {
            continue;
        }
        if (elements[i].Value != 'geben Sie Wert ein...' && elements[i].Value != 'enter a value...' && elements[i].Value != '') {
            elements[i].Name = elements[i].Name.replace('&', "&amp;").replace('"', "&quot;").replace("'", "&#39;").replace('>', "&gt;").replace('<', "&lt;");
            elements[i].Value = elements[i].Value.replace('&', "&amp;").replace('"', "&quot;").replace("'", "&#39;").replace('>', "&gt;").replace('<', "&lt;");
            filedata +=
                '\t\t<ParameterElement>\r\n' +
                '\t\t\t<Name>' + elements[i].Name + '</Name>\r\n' +
                '\t\t\t<Value>' + elements[i].Value + '</Value>\r\n' +
                '\t\t\t<IsVonBisParameter>' + elements[i].IsVonBisParameter + '</IsVonBisParameter>\r\n' +
                '\t\t\t<IsVon>' + elements[i].IsVon + '</IsVon>\r\n' +
                '\t\t</ParameterElement>\r\n';
        }
    }
    filedata += '\t</Parameters>\r\n' +
              '</Parameter>';
    var mainDate = new Date();
    var date = new Date(mainDate.getFullYear(), mainDate.getMonth(), mainDate.getDate(), mainDate.getHours(), mainDate.getMinutes(), mainDate.getSeconds());
    var fileName = issueName + "_Parameters_" + date.toLocaleString().replace(/\. /g, '_').replace(/:/g, '-') + ".xml";

    var specialChars = new Array();
    specialChars.push('\\', '/', ':', '*', '?', '"', '>', '<', '|');

    for (var j = 0; j < specialChars.length; j++) {
        if (fileName.indexOf(specialChars[j]) != -1) {
            fileName = fileName.replace(specialChars[j], '');
        }            
    }
    downloadXmlFile(fileName, filedata);
});

function downloadXmlFile(filename, text) {
    var aElement = document.createElement('a');
    aElement.setAttribute('href', 'data:text/xml;charset=utf-8,' + encodeURIComponent(text));
    aElement.setAttribute('download', filename);
    if (document.createEvent) {
        var event = document.createEvent('MouseEvents');
        event.initEvent('click', true, true);
        aElement.dispatchEvent(event);
    }
    else {
        aElement.click();
    }
}
/* --------- END INDEX VIEW OPTION    -------------------------------------------------------------- */
jQuery(document).on("mouseover", "#showOneReportItem tr", function () {
    jQuery(this).addClass('hover');
}).on("mouseout", "#showOneReportItem tr", function () {
    jQuery(this).removeClass('hover');
}).on("click", "#showOneReportItem tr", function () {
    var $prev = jQuery(this).addClass('aktiv').siblings('.aktiv');
    $prev.removeClass('aktiv');
    return false;
});
/* Show / Hide */
jQuery(document).on("click", "#resultlist td.toparams3 .hide-table a", function () {
    var id = jQuery(this).prev().text();
    var parent_tr = jQuery(this).parent().parent().parent().parent();
    var tab = jQuery('.tabbar .selected');
    var showHidden = tab.hasClass('show_hidden');
    var searchElement = jQuery('#reportlistpopupcontainer .search-opt-panel input[type=text]');
    var search = searchElement.val();
    var placeholder = searchElement.attr('placeholder');
    if (search == placeholder) search = '';
    var sortColumn = jQuery('.header .nav-controls .sortColumn').text();
    var direction = jQuery('.header .nav-controls .direction').text();
    var page = 0;
    var size = 25;
    var url = jQuery('.page-content')[0].id;
    jQuery.get('/Command/UpdateTable/',
        { id: id, json: true, search: search, showHidden: showHidden, showEmpty: false, showArchived: false, catRefresh: true, sortColumn: sortColumn, direction: direction, url: url, size: size, page: page },
        function () {
            jQuery.get('/IssueList/IndexList', { json: true, size: size, page: page, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: false, showHidden: showHidden, showArchived: false },
                function (html) {
                    var tabbar = jQuery('#reportlistpopupcontainer .tabbar', html);
                    var resultlist = jQuery('#resultlist', html);
                    jQuery('#reportlistpopupcontainer .tabbar').html(tabbar);
                    jQuery('#resultlist').html(resultlist);
                    $dialog.hide();
                    return false;
                }
            );
            parent_tr.fadeout();
            return false;
        }
    );
    return false;
});
/* Delete */
jQuery(document).on("click", "#resultlist td.toparams3 .delete a", function () {
    var $dialog = jQuery('#dialog').show();
    var id = jQuery(this).prev().text();
    jQuery.get('/IssueList/DeleteConfirmationDialog/', function (dialog_html) {
        if (dialog_html.search("login") != -1) {
            window.location = "/Account/LogOn";
        }
        $dialog.html(dialog_html);
        $dialog.find('.dialog-btn').click(function () {
            $button = jQuery(this);
            if ($button.find('.data').text() == 'False') {
                $dialog.hide();
                return false;
            } else if ($button.find('.data').text() == 'True') {
                $dialog.find('.dialog-btn').hide();
                var searchElement = jQuery('#reportlistpopupcontainer .search-opt-panel input[type=text]');
                var search = searchElement.val();
                var placeholder = searchElement.attr('placeholder');
                if (search == placeholder) search = '';
                var sortColumn = jQuery('.header .nav-controls .sortColumn').text();
                var direction = jQuery('.header .nav-controls .direction').text();
                var page = 0;
                var size = 25;
                jQuery.get('/IssueList/DeleteIssue/', { id: id, json: true, search: search, catRefresh: true, sortColumn: sortColumn, direction: direction, size: size, page: page },
                    function (html) {
                        if (html == "") {
                            window.location = "/Home/Index";
                        } else {
                            jQuery('.content-box').replaceWith(html);
                            $dialog.html('').hide();
                            jQuery('#reportlistpopupcontainer a.dropdown-btn').click();
                        }
                    }
                );
                var tab = jQuery('.tabbar .selected');
                var showHidden = tab.hasClass('show_hidden');
                jQuery.get('/IssueList/IndexList', { json: true, size: size, page: page, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: false, showHidden: showHidden, showArchived: false },
                    function (html) {
                        var tabbar = jQuery('#reportlistpopupcontainer .tabbar', html);
                        var resultlist = jQuery('#resultlist', html);
                        jQuery('#reportlistpopupcontainer .tabbar').html(tabbar);
                        jQuery('#resultlist').html(resultlist);
                    }
                );
                return false;
            }
            return false;
        });
    });
    return false;
});
/* Edit */
jQuery(document).on("click", "#resultlist td.toparams3 .edit a", function () {
    var id = jQuery(this).prev().text();
    jQuery.get('/IssueList/EditIssueDialog/', { id: id }, function (dialog_html) {
        if (dialog_html.search("login") != -1) {
            window.location = "/Account/LogOn";
        }
        $dialog = jQuery('#dialog').show().html(dialog_html);
        $outer_box = $dialog.find('.dialog-outer-box');
        $outer_box.css("margin-top", -$outer_box.height() / 2);
        $dialog.find('.dialog-btn').click(function () {
            $button = jQuery(this);
            if ($button.find('.data').text() == 'False') {
                $dialog.hide();
                return false;
            } else if ($button.find('.data').text() == 'True') {
                var searchElement = jQuery('#searchoverview input[type=text]');
                var search = searchElement.val();
                var placeholder = searchElement.attr('placeholder');
                if (search == placeholder) search = '';
                var sortColumn = jQuery('.header .nav-controls .sortColumn').text();
                var direction = jQuery('.header .nav-controls .direction').text();
                var page = jQuery('.header .nav-controls .page').text();
                var size = jQuery('.header .nav-controls .size').text();
                if (!page) page = 0;
                if (!size) size = 25;
                $country_code = $dialog.find('form .countryCode');
                $dialog.find('form input[type=text]').each(function (i, e) {
                    jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                });
                $button.attr("disabled", "true");
                $form = $dialog.find('form');
                $form.get(0).action += "/IssueList/EditIssue?id=" + id + "&json=true&search=" + search + "&catRefresh=true&sortColumn=" +
                                    sortColumn + "&direction=" + direction + "&size=" + size + "&page=" + page;
                $form.find('input [type=text]').remove();
                $form.find('input.countryCode').remove();
                $form.ajaxForm({
                    success: function (html) {
                        if (html.search("login") != -1) {
                            window.location = "/Account/LogOn";
                        }
                        var tabbar = jQuery('#reportlistpopupcontainer .tabbar', html);
                        var resultlist = jQuery('#resultlist', html);
                        jQuery('#reportlistpopupcontainer .tabbar').html(tabbar);
                        jQuery('#resultlist').html(resultlist);
                        $dialog.hide();
                        jQuery('#reportlistpopupcontainer a.dropdown-btn').click();
                        return false;
                    }
                });
                $form.submit();
                return false;
            }
            return false;
        });
        $dialog.find('form select[name=language]').change(function () {
            $country_code = $dialog.find('form .countryCode');
            $select = jQuery(this);
            $dialog.find('form input[type=text]').each(function (i, e) {
                jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                jQuery(e).val($dialog.find('form .' + $select.val() + "_" + jQuery(e).attr('name')).val());
            });
            $country_code.val($select.val());
        });
        return false;
    });
    return false;
});
function SetContentTop() {
    var item = jQuery(".master-header");
    if (item.length > 0) {
        var height = jQuery(".master-header")[0].offsetHeight;
        jQuery("#set-top").css("top", height);
        setBelegTableBody();
    }
}
jQuery(document).on("click", ".immediate-filter-controller", function () {
    var timer = null;
    jQuery(this).parent().parent().find(".immediate-filter-content")
        .slideToggle('slow', 'swing', function () {
            jQuery(this).parent().parent().find('.immediate-filter-controller').toggleClass("expanded");
            jQuery(".master-header").trigger('resize');
            clearInterval(timer);
        });
    timer = setInterval(SetContentTop, 1);
});
jQuery(document).on("keydown", "input.report-parameter-validate", function (e) {
    if (e.keyCode == 13) {
        return false;
    }
    return true;
});
jQuery(document).on("click", "#ArchiveDocuments .showviewoptions", function (e) {
    var form = jQuery(e.target).parent().parent().parent().parent().find('form');
    form.ajaxSubmit(long_running_options);
    return false;
});
jQuery(document).on("click", "#clear-report", function () {
    var inputs = $('#execute_report').find('input[type=text]');
    for (var i = 0; i < inputs.length; i++) {
        $(inputs[i]).val($(inputs[i]).attr('placeholder'));
        $(inputs[i]).addClass('empty');
        $(inputs[i]).trigger('validate');
    }
});
jQuery(document).on("click", ".immediate-filter #clear-report", function () {
    jQuery(this).siblings('#open-report').trigger('click');
});
jQuery(document).on("click", ".immediate-filter #clear-belegarchive", function () {
    $.ajax({
        url: "/Documents/FilterDelete"
    }).done(function () {
        location.href = "/Documents";
    });
});
//Dialogs for getting blueline documents
jQuery(document).on("click", ".get-blue-line-documents a", function () {
    var $link = jQuery(this);
    jQuery.get($link.attr('href'), function (html) {
        if (html.search("login") != -1) {
            window.location = "/Account/LogOn";
        }
        var $dialog = jQuery('#dialog').html(html).show();
        var key = $dialog.find('.key').text();
        $('#DataGrid .content-inner').hide();
        $dialog.find('.dialog-btn').click(function () {
            delete Updater.Listeners[key];
            jQuery('#dialog').hide();
            $('#DataGrid .content-inner').show();
        });
        Updater.Listeners[key] = function ($notification) {
            location.href = $notification.find('a.link').attr('href');
        };
    });
    return false;
});
//Dialogs for getting blueline documents
jQuery(document).on("click", ".download-blueline-documents a", function () {
    var $link = jQuery(this);
    jQuery.get($link.attr('href'), function (html) {
        if (html.search("login") != -1) {
            window.location = "/Account/LogOn";
        }
        var $dialog = jQuery('#dialog').html(html).show();
        var key = $dialog.find('.key').text();
        $('#DataGrid .content-inner').hide();
        $dialog.find('.dialog-btn').click(function () {
            delete Updater.Listeners[key];
            jQuery('#dialog').hide();
            $('#DataGrid .content-inner').show();
        });
        Updater.Listeners[key] = function ($notification) {
            location.href = $notification.find('a.link').attr('href');
        };
    });
    return false;
});
/**************************************************************************
* don't follow fake links (this block has to be the last one)
**************************************************************************/
jQuery(document).on("click", "a", function (e) {
    return !e.target.attributes['href'] || e.target.attributes['href'].value != '#';
});


jQuery(document).on("click", "#clear-report", function () {
    var disabled = $(".selectionCheckbox:disabled");
    $(".parameter-table #selectionType").each(function () {
        $(this).find("option[value='0']").selected();
        $(this).change();
    });
    canGetIssueRecordCount = false;
    var inputs = $('#execute_report').find('input[type=text]');
    var trigger = false;
    for (var i = 0; i < inputs.length; i++) {
        if ($(inputs[i]).val() != $(inputs[i]).attr('placeholder')) {
            trigger = true;
        }
        $(inputs[i]).val($(inputs[i]).attr('placeholder'));
        $(inputs[i]).addClass('empty');
        if (trigger == true) {
            //$(inputs[i]).trigger('ValidateAndSave');
            trigger = false;
        }
    }
    canGetIssueRecordCount = true;
    getIssueRecordCount();
    disabled.each(function () {
        $(this).prop('disabled', true);
    });
});
jQuery(document).on("click", ".extended-optimization-values .visibility input", function () {
    var that = $(this);
    var optId = that.parent().parent().find(".optId").val();
    var roleId = that.parent().parent().find(".roleId").val();
    var valueId = that.parent().parent().find(".valueId").val();
    var visibility = that.is(':checked');
    var requestUrl = "/UserManagement/SetExOptimizationValue?optId=" + optId + "&roleId=" + roleId + "&optimizationValueId=" + valueId + "&visible=" + visibility;
    $.getJSON(requestUrl, function (vis) {
        that.val(vis);
    });
});
jQuery(document).on("click", ".row-rights-on, .row-rights-off", function () {
    var tableId = $("#currentTableId").html();
    var className = $(this).parent().parent().attr('class').split(' ');
    var columnId = $('.' + className[0]).find('.item-id').first().html();
    var val = $(this).parent().find('span.value-on-rights').html();
    val = encodeURI(val);
    var $dialog = jQuery('#dialog').fadeIn(750).html(jQuery('#wait-dlg-rights').html());

    $.get("/UserManagement/SetRoleBasedFilter?tableId=" + tableId + "&columnId=" + columnId + "&filterValue=" + val, function (wasSuccessful) {
        if (wasSuccessful) {
            location.reload();
        }
        else {
            $dialog.fadeOut(750);
            alert("Setting up role completed, but creating the row count cache table was unsuccessful!");
        }
    });
});

var canGetIssueRecordCount = true;
function getIssueRecordCount() {
    if (!canGetIssueRecordCount)
        return false;
    var th = $("#GetIssueRecordCount");
    var cont = $(".report-issue-count-container");
    if (!jQuery('.preload-record-count a').hasClass('checkboxon')) {
        th.html("");
        cont.attr("style", "display:none;");
        return false;
    }
    cont.attr("style", "");
    var form = th.parent().parent().find('form');
    th.html("<div class='refreshIssueRecordCount'></div>");
    jQuery.ajax(
            {
                url: "/DataGrid/StartGetIssueRecordCount",
                data: form.serialize(),
                success: function (Key) {
                    if (Key.Key === 1) {
                        th.html("<div>" + Key.Value + "</div>");
                    }
                    else if (Key.Key === 0) {
                        th.attr("key", Key.Value);
                        Updater.Listeners[Key.Value] = function () {
                            jQuery.ajax({
                                url: "/DataGrid/GetIssueRecordCount",
                                data: { "Key": Key.Value },
                                success: function (html2) {
                                    if (dialog_html.search("login") != -1) {
                                        window.location = "/Account/LogOn";
                                    }
                                    th.html("<div>" + html2 + "</div>");
                                }
                            });
                        };
                    }
                    else
                        th.html("<div>0</div>");
                }
            });
    return true;
};

window.onunload = function () {
    jQuery('#dialog').hide();
};
function gup(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(window.location.href);
    if (results == null)
        return "";
    else
        return results[1];
}
function removeParamsFromBrowserURL() {
    return window.location.href.replace(/\?.*/, '');
}

$(document).ready(function () {
    var param = gup('isNoResult');
    if (param == 'true' || ($("#IsThereEmptyMessage").length == 1 && $("#IsThereEmptyMessage").val() == 'true')) {
        var replaceLocation = location.href;
        try {
            window.history.pushState(null, "", replaceLocation.replace("?isNoResult=true", ""));
        } catch (e) {

        }
        $.blockUI({ message: $('#question'), css: { width: '100%', border: 'none', backgroundColor: 'none' } });
        $("#IsThereEmptyMessage").val('false');
    }
});

jQuery(document).on("click", "#clear-report", function () {
    $(":root").find(".parameter-table .parameter span").each(function () {
        if ($(this).hasClass("disabled-parameter") == false)
            $(this).attr('style', "color: black");
    });
});

function usedParameters() {
    var parameterList = $(":root").find(".parameter-table .parameter");
    for (var i = 0; i < parameterList.length; i++) {
        var inputField = $(parameterList[i]).find('input[type=text]');
        for (var j = 0; j < inputField.length; j++) {
            var placeholder = inputField[j].placeholder;
            var value = $(inputField[j]).val();
            var checkValue = value.replace(/ /g, "");
            if (value != "" && checkValue.length > 0 && placeholder != value) {
                var inputText = $(parameterList[i]).find('span');
                $(inputText[j]).attr('style', "color: red; font-weight: bold");
            }
        }
    }
}

function yesClick() {
    $.unblockUI();
    jQuery.get("/DataGrid/ClearEmptyMessage");
    usedParameters();
}

function noClick() {
    jQuery.get("/IssueList/CleareParamaterList");
    $.unblockUI();
    jQuery.get("/DataGrid/ClearEmptyMessage");
    $(":root").find(".parameter-table .parameter input[type=text]").val("");
}

function noresClick() {
    $.unblockUI();
    jQuery.get("/DataGrid/ClearEmptyMessage");
}

//"Tree Library"
$(".simplenode").click(function () {
    if ($(this).first().css("display") == "none") {
        $(this).first().show();
    }
    else {
        $(this).first().hide();
    }
});
jQuery(document).ready(function () {
    jQuery(".searchDropDownfuncion").autocomplete({
        source: function (request, response) {
            var columnId = parseInt($(this.element).parent().attr("id"));
            $.ajax({
                url: "/ValueListBeleg/Index",
                data: { columnId: columnId, tableId: jQuery('.immediate-filter-fields-table').attr('id'), value: request.term },
                success: function (data) {
                    response(data);
                },
                error: function () {
                    response([]);
                }
            });
        },
        delay: 500
    });
});

/*Start preview attachment functions (Belegarchiv preview)*/
jQuery(document).on("click", "#ItemNumberPaging a.iconx", function () {
    var that = $(this);
    var tableId = jQuery("div#tableId").text();
    var ticketnumber = jQuery("span#Ticketnumber").text();
    var ticketnumberColid = jQuery("span#Ticketnumber").attr("colid");
    var itemnumberColid = jQuery("span#Itemnumber").attr("colid");
    var itemnumber = 0;

    if (that.hasClass("first")) {
        itemnumber = that.attr("page");
    }
    else if (that.hasClass("back")) {
        itemnumber = that.attr("page");
    }
    else if (that.hasClass("next")) {
        itemnumber = that.attr("page");
    }
    else if (that.hasClass("last")) {
        itemnumber = that.attr("page");
    }

    var filter = encodeURIComponent("And(Equal(%" + ticketnumberColid + ",\"" + ticketnumber + "\")," + "Equal(%" + itemnumberColid + ",\"" + itemnumber + "\"))");

    jQuery.get("/Documents/GetDocumentsOverView?id=" + tableId + "&filter=" + filter, function (html) {
        var wrapper = jQuery('.document-preview .preview .wrapper');
        wrapper.html(html);
    });
});

jQuery(document).on("click", "img#attachment-download-icon", function () {
    var url = jQuery(this).closest("tr").find('div.url[belegarchive="true"]');
    var conds = new Array();
    url.find(".target").each(function (i, e) {
        var el = jQuery(e);
        conds.push("Equal(%" + el.attr("colid") + ",\"" + el.text() + "\")");
    });
    var table_key = url.find(".key").text();
    var filter = encodeURIComponent("And(" + conds.join(",") + ")");

    jQuery.get("/Documents/DocumentOverviewAttachmentsDownload?filter=" + filter, function (html) {
        jQuery("#preview-attachment-popup").show();
        jQuery("#preview-attachment-popup .inner-container").html(html);
        var previewdiv = jQuery("#preview-attachment-popup .distinct-main");
        previewdiv.offset({ top: (jQuery(document).height() / 2) - (previewdiv.height() / 2), left: (jQuery(document).width() / 2) - (previewdiv.width() / 2) });
    });
});

jQuery(document).on("click", "#preview-attachment-popup .close-reportlist", function () {
    jQuery("#preview-attachment-popup").hide();
});
/*End preview attachment functions*/

jQuery(function () {
    (initDatePicker = function (element) {
        var dateFormatka = jQuery("#currentLanguage").val() === "en" ? "dd/mm/yy" : "dd.mm.yy";
        var currentYearRange = jQuery("#currentYearRange").val();
        jQuery(element).datepicker({
            showOn: "button",
            buttonImage: "/Content/img/icons/datepick.png",
            buttonImageOnly: true,
            showOtherMonths: true,
            selectOtherMonths: true,
            changeMonth: true,
            changeYear: true,
            yearRange: currentYearRange,
            onSelect: function () {
                $(this).trigger('keyup');
            },
            beforeShow: function () {
                jQuery(document).trigger('click.dropdown');
                jQuery(document).trigger('click.distinct');
                jQuery(document).trigger('click.reportdropdown');
            },
            dateFormat: dateFormatka
        });
    })(/* execute viewoptions_loaded */);
    //FreeSelection
    var initDatePicker;
    jQuery(document).on("click", ".selectionCheckbox", function () {
        if ($(this).attr("checked")) {
            var row = $(this).parent().parent();
            $(this).removeAttr("checked");
            if ($(this).parent().parent().filter("[id*='_']").length > 0) {
                var id = $(this).parent().parent().attr("id").split("_")[0];
                var index = $(this).parent().parent().attr("id").split("_")[1];
                $(".parameter-table [id*='" + id + "_']").each(function () {
                    var element = $(this);
                    if (parseInt(element.attr("id").split("_")[1]) > parseInt(index)) {
                        element.remove();
                    }
                });
            }
            else {
                var id = $(this).parent().parent().find(".parameter-item:first .item-id").text();
                $(".parameter-table [id*='" + id + "_']").each(function () {
                    var element = $(this);
                    element.remove();
                });
            }

        }
        else {
            var row = $(this).parent().parent();
            row.parent().find(".dropdown-menu-container").hide()
            $(this).attr("checked", "true");
            var newElement = row.clone().insertAfter(row);
            newElement.find(".selectionCheckbox").attr("checked", false);
            var id = parseInt(newElement.find(".parameter-item:first .item-id").text());
            var elementCount = $(".parameter-table [id*='" + id + "_']").length;
            if (elementCount != 0) {
                elementCount--;
            }
            id += "_" + elementCount;
            newElement.attr("id", id);
            newElement.attr("class", "parameter it-s-a-clone");
            newElement.find(".selection-type option:first").selected();
            newElement.find(".parameter-item input").not(":hidden").each(function () {
                var parId = parseInt($(this).parent().parent().parent().find(".item-id").html());
                $(this).val("");
            });
            // Until DatePicker cloning input is solved
            var number = 0;
            newElement.find(".parameter-item .hasDatepicker").each(function () {
                var element = $(this);
                var tempParId = parseInt($(newElement.find(".item-id")[number]).text());
                var idTemp = tempParId + "_" + elementCount + "_" + number;
                element.attr("id", "TextBoxName" + idTemp);
                $(newElement.find(".item-id")[number]).text(idTemp);
                element.removeClass("hasDatepicker");
                element.parent().find("img.ui-datepicker-trigger").remove();
                initDatePicker(element);
                number++;
            });
            // check cloning
            id = parseInt(newElement.find(".parameter-item:first .item-id").text());
            var datepicker = newElement.find(".parameter-item .hasDatepicker");
            elementCount = $(".parameter-table [id*='" + id + "_']").length;
            if (elementCount != 0 && datepicker.length == 0) {
                // Regular Case
                elementCount--;
            }
            else {
                // DatePicker Case
                elementCount = (elementCount - 2) / 2;
            }
            if (elementCount >= 2) {
                newElement.find(".selectionCheckbox").attr("disabled", "disabled");
            }
        }
    });
    $(document).on("change", ".parameter-table #selectionType", function () {
        //check if von-bis
        var selectionType = $(this);
        var parameters = selectionType.parent().parent().find(".parameter-item");
        if (parameters.length == 2) {
            // 
            if (selectionType.val() > 1) {
                var secondParameter = $(parameters[1]);
                secondParameter.find(".AdditionButton").hide();
                secondParameter.find(".DeleteText").hide();
                secondParameter.find(".field").val("");
                var parameterContainer = secondParameter.parent();
                parameterContainer.find(".selectionCheckbox").prop('disabled', true);
                parameterContainer.find(".selectionCheckbox").prop("checked", false);
                var id = $(parameters[0]).find('.item-id').text();
                var clone_id = parameterContainer.prop("id");
                parameters.parent().parent().find(".parameter").each(function () {
                    var clone = $(this);
                    if (clone.hasClass("it-s-a-clone")) {
                        if (clone.attr("id").indexOf(id) > -1) {
                            if (clone.attr("id") != clone_id) {
                                clone.remove();
                            }
                            else {
                                clone.removeClass("it-s-a-clone");
                                clone.removeAttr("id");
                            }
                        }
                    }
                    else {
                        if (clone.find(".item-id").first().text() == id && clone_id != "") {
                            clone.remove();
                        }
                    }
                });

            }
            else {
                var secondParameter = $(parameters[1]);
                secondParameter.find(".AdditionButton").show();
                secondParameter.find(".DeleteText").show();
                var parameterContainer = secondParameter.parent();
                parameterContainer.find(".selectionCheckbox").prop('disabled', false);
            }
        }
    });

    // Append all from wertehilfe
    $(document).on("click", "#distinct-popup .exactmatch #append-all", function () {
        var elements = $(".report-distinct-menu .entry .value");
        var inputId = $(".report-distinct-menu").attr("parameterid");
        var input;
        $(".parameter-item").each(function () {
            var itemId = $(this).find("#itemId").val();
            if (itemId == inputId) {
                input = $(this).find(".field");
            }
        });
        input.val("");
        elements.each(function () {
            var value = $(this).text();
            input.val(input.val() + value + ";");
        });
        jQuery(document).trigger('click.distinct');
        $(".distinct").last().click();
    });

    $(document).on("paste", ".parameter-table .parameter .parameter-item .value", function (event) {
        var input = $(this);
        var selectionType = input.parent().parent().parent().find("#selectionType").val();
        event.preventDefault();
        var clipboardData = "";
        if (event.originalEvent.clipboardData) {
            clipboardData = event.originalEvent.clipboardData.getData('text/plain');
        }
        else {
            clipboardData = window.clipboardData.getData('text');
        }

        if (selectionType == "2" || selectionType == "3") {
            var baseVal = input.val();
            var values = clipboardData.split("\n");
            var toParse = [];
            var limit = 100 > values.length ? values.length : 100;
            for (var i = 0; i < limit; i++) {
                toParse.push(values[i]);
            }
            //input.val((baseVal == "" ? "" : baseVal + ";") + toParse.join(";"));
            input.val((baseVal != "" ? (baseVal[baseVal.length - 1] == ";" ? baseVal : baseVal + ";") : "") + toParse.join(";"))
        }
        else {
            input.val(clipboardData);
        }
    });

});
function setIFrameLoaded() {
    var loader = jQuery('.document-preview .preview .loader');
    loader.hide();
    $(".document-preview .preview .wrapper").show();
}

$(document).ajaxComplete(function (event, jqXHR) {
    if (jqXHR.responseText.indexOf("/") === 0) {
        window.location = window.location.protocol + "//" + window.location.host + jqXHR.responseText;
        jqXHR.abort();
    }
});
