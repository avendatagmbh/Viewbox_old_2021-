function closeCrud() {
    var popup = jQuery('#crud-popup');
    var popupinner = jQuery('#crud-popup-inner');
    popupinner.html("");
    popup.hide();
    location.reload();
}
function initCrud() {
    initCrud(null, null);
}
function initCrud(xhr) {
    if (xhr !== undefined && xhr.responseText == "") {
        closeCrud();
    }
    jQuery("#crud-popup .thousand-separator").numberWithSeparators();
    var currentYearRange = jQuery("#currentYearRange").val();

    jQuery('#crud-popup .reportdate').datepicker({
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
        }
    });
}
jQuery(function () {
    /**************************************************************************
    * BALANCE VIEW
    **************************************************************************/
    jQuery(document).on("click", ".tree-node", function () {
        if ($(this).children(".tree-node").length > 0)
            jQuery(this).toggleClass('open').toggleClass('closed');
        return false;
    });
    /**************************************************************************
    * FORGOT PASSWORD
    **************************************************************************/
    jQuery(document).on("click", "#login .forgot_password", function () {
        var $link = jQuery(this);
        jQuery.get($link.attr('href'), { email: jQuery('#Email').val(), url: window.location.href }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            var $dialog = jQuery('#dialog');
            $dialog.html(html);
            $dialog.show().find('.dialog-btn').click(function () {
                $button = jQuery(this);
                if ($button.find('.data').text() == 'False') {
                    $dialog.hide();
                }
                if ($button.find('.data').text() == 'True') {
                    $dialog.find('form').ajaxSubmit({
                        url: '/Account/SendInvitation',
                        success: function (html) {
                            if (html.search("login") != -1) {
                                window.location = "/Account/LogOn";
                            }
                            $dialog.html(html);
                            $dialog.show().find('.dialog-btn').click(function () { $dialog.hide(); });
                        }
                    });
                }
            });
        });
        return false;
    });
    /*--------------------------------------------------------------------
    PASSWORD POLICY
    --------------------------------------------------------------------*/
    jQuery(document).on("click", "div#dialog div.dialog-buttons a.dialog-btn:has(>span[class=data]:contains(NewPassword))", function (e) {
        var dialogForm = $('#login form');
        //passwordSubmitObject = new PasswordSubmit();
        //passwordSubmitObject.loadForm(dialogForm);
        var newPassword = $("#dialogform [name=newPassword]").val();
        var confirmNewPassword = $("#dialogform [name=confirmNewPassword]").val();
        var userName = $("#dialogform [name=userName]").val();
        var passWord = $("#dialogform [name=passWord]").val();
        var key = $("#dialogform [name=key]").val();
        $("#dialog span[class=errorMsg]").remove();
        jQuery(document).off("click", "#dialog .user-name .dialog-btn");
        $.ajax({
            type: "GET",
            url: "/Account/ValidateNewPassword",
            async: true,
            data: {
                userName: userName,
                newPassword: newPassword,
                confirmNewPassword: confirmNewPassword
            },
            success:
                function (response) {
                    // display error message if not succeed
                    if (response.error !== 'undefined' && response.error != undefined) {
                        e.stopPropagation();
                        $("#dialog [class=dialog-text] > span").append("<span class='errorMsg' style='color:yellow;display:block'>" + response.error + "</span>"); // .get("/Settings/ResultDialog", )
                        $("#dialog").show();
                        return false;
                    } else { // save password , display save confirmation popup
                        $.post("/Account/SaveNewPassword", {
                            newPassword: newPassword,
                            userName: userName,
                            password: passWord,
                            key: key
                        }, function (succeed) {
                            //if (succeed) {
                            //    // load confirmation dialog
                            //    $("#dialog").load("/Settings/ResultDialog", { wrongPassword: false },
                            //    function () {
                            //        // upon click on OK, change the password form to the new one + submit the form to the original logon post 
                            //        $("#dialog").show();
                            //        $("#dialog .dialog-btn").click(function () {
                            $('#login form input#UserName').val(userName);
                            $('#login form input#Password').val(newPassword);
                            $('#login form').submit();
                            //        });
                            //    });
                            //}
                        });
                    }
                }
        });
        return false;
    });
    // check for new password validity according to policy before sending the form
    jQuery(document).on("click", "#settings form > div[class*=Password] .link a.update", function (e) {
        var dialogForm = $('#settings form');
        passwordSubmitObject = new PasswordSubmit();
        passwordSubmitObject.loadForm(dialogForm);
        var succeed = false;
        $.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            url: "/Account/ValidateNewPassword",
            async: false,
            data: { userName: passwordSubmitObject.userName, newPassword: passwordSubmitObject.password },
            success: function (response) {
                if (!response.success) {
                    e.stopPropagation();
                    $("#dialog").load("/Settings/ResultDialog", { wrongPassword: true, newPassword: true, message: response.error }); // .get("/Settings/ResultDialog", )
                    $("#dialog").show();
                    return false;
                } else {
                    succeed = response;
                }
            }
        });
        return succeed;
    });
    /*--------------------------------------------------------------------
    END PASSWORD POLICY
    --------------------------------------------------------------------*/
    /**************************************************************************
    * SETTINGS
    **************************************************************************/
    // Tabs for Rollenrechte menu
    $("#role-tabs").tabs();

    // Set heights
    var roleResize = function () {
        var h = $("#settings").height();
        $("#role-tabs-1, #role-tabs-2, #role-tabs-3").height(h - 85);
    };
    roleResize();
    $("#settings").resize(function () {
        roleResize();
    });

    // Expand optimizations
    $(document).on("click", "#role-tabs-1 .expand-optimization", function () {
        var $ul = $(this).parent().find("ul").first();
        if ($ul.length > 0) {
            if ($ul.hasClass("hide"))
                $ul.removeClass("hide");
            else
                $ul.addClass("hide");
        }
    });
    // Submit Form
    $("#role-tabs-1 .buttons input").click(function () {
        var optForm = $("#role-tabs-1 form");
        var formElements = $("#role-tabs-1 form input");
        var parameterDictonary = [];
        var lastId = "";
        var i = 0;
        formElements.each(function () {
            var element = $(this);
            if (element.hasClass("opt-id")) {
                lastId = element.val();
                parameterDictonary.push({
                    Key: lastId.toString(),
                    Value: ""
                });
            }
            if (element.hasClass("optimization-visibility")) {
                parameterDictonary[i].Value = element.is(":checked");
                i++;
            }
        });

        $.post(optForm.attr("action"), { settings: JSON.stringify(parameterDictonary) }, function (msg) {
            $("#dialog1").html(msg);
            $("#dialog1 .dialog-buttons .dialog-btn").click(function () {
                $("#dialog1").html("");
                $("#dialog1").hide();
            });
            $("#dialog1").show();
            $("#notification .count").bind("DOMSubtreeModified", function () {
                $("#notification-container .box").first().find("a").click();
            });
        });
        return false;
    });

    var searchForTablesRunning = false;
    var loadingTimer = 0;
    var searchForTables = function (search) {
        if (searchForTablesRunning)
            return;
        searchForTablesRunning = true;

        var container = $("#role-tabs-3 .children");
        loadingTimer = setTimeout(function () {
            if (searchForTablesRunning)
                $('#role-tabs-3 .loadinggg').show();
        }, 1000);
        $.post("/UserManagement/SearchForTables", { search: search }, function (tables) {
            searchForTablesRunning = false;
            $('#role-tabs-3 .loadinggg').hide();
            container.html(tables);
            clearTimeout(loadingTimer);

            // If search value has been changed during this call, then reload data with the actual value
            var actVal = $('#role-tabs-3 #search-tables').val();
            if (search != actVal)
                searchForTables(actVal);
        });
    };

    if ($('#role-tabs-3 #search-tables').length) {
        $(document).ready(function () {
            searchForTables("");
        });
    }

    var displayErrorMsg = function(msg) {
        $("#dialog1").html(msg);
        $("#dialog1").show();
        $("#dialog1 .dialog-buttons .dialog-btn").unbind('click').click(function () {
            $("#dialog1").html("");
            $("#dialog1").hide();
        });
    }

    $("#role-tabs-3 #search-tables").on("input", function () {
        searchForTables(this.value);
    });

    /* 
    jQuery(document).on("change", "#role-tabs-3 .table-visibility", function () {
        var keyId = $(this).attr('id').replace('__Value', '__Key');
        var tableId = $('#role-tabs-3 #' + keyId).val();
        $('#role-tabs-3 .loadinggg').show();
        var that = this;
        $.post("/UserManagement/SetRightForOneTable", { id: tableId, visible: $(this).is(':checked') }, function (html) {
            $('#role-tabs-3 .loadinggg').hide();
            if (html.length > 0) {
                displayErrorMsg(html);
                $(that).prop('checked', !($(that).is(':checked')));
            }
        });
    });
    */
    $('#tableVisibility').change(function () {
        var that = this;
        $.post("/UserManagement/SetRightsForTables", { visible: $(this).is(':checked') }, function (html) {
            if (html.length > 0) {
                displayErrorMsg(html);
                $(that).prop('checked', !($(that).is(':checked')));
            }
            else // reload tables
                searchForTables($('#role-tabs-3 #search-tables').val());
        });
    });

    jQuery("#settings .property .link a").click(function (e) {
        if ($(this).hasClass("change-disabled")) return false;
        if (e.isPropagationStopped()) return false;
        var $link = jQuery(this);
        var $property = $link.parents('.property').toggleClass('change');
        if ($link.is('.update')) {
            if ($property.is('.Password'))
                $property.find('input[type=hidden]').val(true);
            $property.parents('form').submit();
            return false;
        }
        if ($link.is('.cancel')) {
            if (!$property.is('.Password'))
                $property.find('input[type=text]').val($property.find('.display-value').text());
            else $property.find('input[type=password]').val('');
            return false;
        }
        if ($property.is('.String')) {
            $property.find('input[type=text], input[type=password]').select();
        } else if ($property.is('.Bool')) {
            var $input = $property.find('input.hidden-value');
            var bool = $input.val() == 'false';
            $input.val(bool.toString());
            $property.toggleClass('change').parents('form').submit();
        }
        //It checks that the input of the field is a number, and deletes it if it is not
        if ($property.is('.Numeric')) {
            $property.find('input[type=text]').keyup(function () {
                this.value = this.value.replace(/[^0-9]/g, '');
            });
        }
        return false;
    });
    /**************************************************************************
    * Log-Off
    **************************************************************************/
    jQuery(document).on("click", ".boxheader .logoff", function () {
        var url = location.href;
        var arr = url.split('/');
        var rightSide = arr[3];
        var controller = rightSide.split('?')[0];
        var newurl = jQuery(this).attr("href") + "?usercontroller=" + controller;
        window.location.href = newurl;
        return false;
    });
    /**************************************************************************
   * Dead-Url-Dialog
   **************************************************************************/
    jQuery(document).on("click", ".dynamic-url .dialog-btn", function () {
        location.href = "/";
    });
    /**************************************************************************
    * Dead-Url-Dialog
    **************************************************************************/
    jQuery(document).on("click", ".dead-url .dialog-btn", function () {
        location.href = "/";
    });
    /**************************************************************************
    * Maintance-Url-Dialog
    **************************************************************************/
    jQuery(document).on("click", ".maintance-url .dialog-btn", function () {
        location.href = "/Error/HandleAppRestart";
    });
    /**************************************************************************
    * Update-Url-Dialog
    **************************************************************************/
    jQuery(document).on("click", ".update-url .dialog-btn", function () {
        location.href = "/Error/UpdateMode";
    });
    /**************************************************************************
    * Not enough freespace-Dialog
    **************************************************************************/
    jQuery(document).on("click", ".notEnoughFreeSpace-dlg .dialog-btn", function () {
        jQuery(this).hide();
        location.href = "/Error/NotEnoughFreeSpaceInformation/";
    });
    /**************************************************************************
    * MySQL auto-config Dialog
    **************************************************************************/
    jQuery().on("click", ".mySqlAutoConfig-dlg .dialog-btn", function () {
        jQuery(this).hide();
        location.href = "/Error/MySqlAutoConfig/";
    });
    /**************************************************************************
    * DatabaseOutOfDate-Dialog
    **************************************************************************/
    jQuery(document).on("click", ".databaseOutOfDate .dialog-btn", function () {
        jQuery(this).hide();
        location.href = "/Error/UpgradeDatabase";
    });
    /**************************************************************************
    * DatabaseOutOfDate-Dialog
    **************************************************************************/
    jQuery(document).on("click", ".databaseUpgradeSuccessful .dialog-btn", function () {
        location.href = "/Error/UpgradeDatabaseSuccessful";
    });
    /**************************************************************************
    * Wrong-Browser-Dialog
    **************************************************************************/
    jQuery(document).on("click", ".wrongBrowser .dialog-btn", function () {
        location.href = "/Account/LogOff";
    });
    /**************************************************************************
    * User-Already-Logged-In-Dialog
    **************************************************************************/
    jQuery(document).on("click", ".useralreadyloggedin .dialog-btn:eq(1)", function () {
        location.href = "/Account/LogOn";
    });
    jQuery(document).on("click", ".useralreadyloggedin .dialog-btn:eq(2)", function () {
        $('.useralreadyloggedin form').submit();
    });
    /**************************************************************************
    * Forced-To-Quit-Dialog
    **************************************************************************/
    jQuery(document).on("click", ".forcedToQuit .dialog-btn", function () {
        location.href = "/Home/Index";
    });
    /**************************************************************************
    * ISSUES - CHECKBOX
    **************************************************************************/
    jQuery(document).on("click", "#IssueList input[type=checkbox]", function () {
        var value = jQuery(this).attr('checked') ? 'true' : 'false';
        jQuery(this).prev().val(value);
    });
    /**************************************************************************
    * VIEW OPTIONS
    **************************************************************************/
    jQuery(document).on("click", "#viewoptions .box, #tabfunctions .box, #listarchiveoptions .box, #listvisibleoptions .box", function (e) {
        e.stopPropagation();
    });
    jQuery(document).on("click", "#extsortoptions .box, #extsummaoptions .box, #extfilteroptions .box, #rolebasedvisibilityoptions .box, #subtotaloptions .box", function (e) {
        e.stopPropagation();
    });
    jQuery(document).on("click", "#listarchiveoptions .close-popup", function () {
        jQuery(document).trigger('click.dropdown');
        jQuery(document).trigger('click.archivedropdown');
    });
    jQuery(document).on("click", ".dropdown .dropdown-btn:not(.disabled), #tabfunctions .dropdown-btn", function () {
        jQuery(document).trigger('click.dropdown');
        var $menu = jQuery(this).next('ul.dropdown-menu').fadeIn();
        jQuery(document).bind('click.dropdown', function (e) {
            if (e.isPropagationStopped()) return;
            $menu.fadeOut(100);
            jQuery(document).unbind('click.dropdown');
        });
        return false;
    });
    var previousTooltipInstance = null;
    jQuery(document).on("mouseenter", ".visible  searchvisible", function () {
        
        if (previousTooltipInstance != null) {
            try {
                previousTooltipInstance.tooltip("close");
            } catch (err) { }
        }

        var $this = $(this);
        previousTooltipInstance = $this;
        var description = $(this).find(".description");
        var originalText = $(this).text().trim();

        if ($(description) != undefined && $(description).css("text-overflow") == "ellipsis"&&
            ($(this).parent().parent().parent().attr("id") != "selected-subtotal-column" 
            && $(this).parent().parent().parent().attr("id") != "selected-group-column"
            && $(this).parent().parent().parent().parent().parent().attr("id")!= "extsubtotal"
            )) {
            //Extended sort, Sichtbar (expect reihenfolge), Total, Subtotal ()
            $(this).find(".description").html("<span>" + originalText + "</span>");
            if ($(description).find("span").width() > 275) {

                $('.colname').tooltip();
            }
            return true;
        }
        else if ($(this).parent().parent().parent().attr("id") == "selected-subtotal-column"
            || $(this).parent().parent().parent().attr("id") == "selected-group-column"
            || $(this).parent().parent().parent().parent().parent().attr("id") == "extsubtotal")
        {
            var width = parseInt($(this).css("width"));
            $(this).find(".description").html("<span>" + originalText + "</span>");
            if ($(description).find("span").width() > 100 && !$this.attr('title')) {

                $this.attr('title', '');
                $this.tooltip({ content: originalText });
                $this.tooltip("open");
            } else { $this.attr('title', ""); }
            return true;
        }
        else if ($(this).parent().parent().attr("id") == "column-dropdown") {
            //Extended filter dropdown list
            if ($($this).find("span").width() > 160 && !$this.attr('title')) {
                $this.attr('title', '');
                previousTooltipInstance = $this.tooltip({ content: originalText });
                $this.tooltip("open");
            }
        }
        else {
            //Reihenfolge
            if ($(this).find("span").width() > 275 && !$this.attr('title')) {
                $this.attr('title', '');
                previousTooltipInstance = $this.tooltip({ content: originalText });
                $this.tooltip("open");
            }
            else { $this.attr('title', ""); }
            return true;
        }
    });

    jQuery(document).on("click", ".dropdown .dropdown-btn:not(.disabled)", function () {
        jQuery(document).trigger('click.archivedropdown');
        var $menu = jQuery(this).next('.archive-dropdown-main').fadeIn();
        var currentTableId = parseInt($("#currentTableId").html());
        var summableColumns = jQuery(this).next().find('.summable-columns');
        if (summableColumns.html()) {
            $(summableColumns).html('<img style="margin-top: 190px;" src="/Content/img/loadera64.gif" />');
            $.getJSON("/DataGrid/ReloadVisibleColumns/" + currentTableId, function (columns) {
                $(summableColumns).html('');
                for (var i = 0; i < columns.length; i++) {
                    $(summableColumns).append(
                    '<div class="column dropdown_sqlfunction_column_row"' + columns[i][3] + '>' +
                        '<div class="item-id">' + columns[i][0] + '</div>' +
                        '<div class="colnumber">' + columns[i][1] + '</div>' +
                        '<div class="colname">' + columns[i][2] + '</div>' +
                    '</div>');
                }
                if (!summableColumns.html()) {
                    summableColumns.html('<div style="display: hidden;"/>');
                    $('#extsort .sortable-columns').html('<div style="display: hidden;"/>');
                    $('#column-dropdown').html('<div style="display: hidden;"/>');
                    $('#column-dropdown1').html('<div style="display: hidden;"/>');
                }
            });
        }
        else {
            var sortableColumns = jQuery(this).next().find('.sortable-columns');
            if (sortableColumns.html()) {
                var sortierung = jQuery(this).next().find('.sortierung');
                $(sortableColumns).html('<img style="margin-top: 190px;" src="/Content/img/loadera64.gif" />');
                if (sortierung.html()) {
                    $.getJSON("/DataGrid/ReloadVisibleColumns/", { id: currentTableId, sortierung: true }, function (columns) {
                        $(sortableColumns).html('');
                        for (var i = 0; i < columns.length; i++) {
                            $(sortableColumns).append(
                    '<div class="column dropdown_sqlfunction_column_row"' + columns[i][3] + '>' +
                        '<div class="item-id">' + columns[i][0] + '</div>' +
                        '<div class="colnumber">' + columns[i][1] + '</div>' +
                        '<a href="#" class="sortdirection"></a>' +
                        '<div class="colname">' + columns[i][2] + '</div>' +
                        '<div class="to-sort"></div>' +
                    '</div>');
                        }
                        if (!sortableColumns.html()) {
                            sortableColumns.html('<div style="display: hidden;"/>');
                            $('#extsumma .sortable-columns').html('<div style="display: hidden;"/>');
                            $('#column-dropdown').html('<div style="display: hidden;"/>');
                            $('#column-dropdown1').html('<div style="display: hidden;"/>');                            
                        }
                    });
                }
                else {
                    $.getJSON("/DataGrid/ReloadVisibleColumns/" + currentTableId, function (columns) {
                        $(sortableColumns).html('');
                        for (var i = 0; i < columns.length; i++) {
                            $(sortableColumns).append(
                    '<div class="column dropdown_sqlfunction_column_row"' + columns[i][3] + '>' +
                        '<div class="item-id">' + columns[i][0] + '</div>' +
                        '<div class="colnumber">' + columns[i][1] + '</div>' +
                        '<a href="#" class="sortdirection"></a>' +
                        '<div class="colname"><span>' + columns[i][2] + '</span></div>' +
                    '</div>');
                        }
                        if (!sortableColumns.html()) {
                            sortableColumns.html('<div style="display: hidden;"/>');
                            $('#extsumma .sortable-columns').html('<div style="display: hidden;"/>');
                            $('#column-dropdown').html('<div style="display: hidden;"/>');
                            $('#column-dropdown1').html('<div style="display: hidden;"/>');
                        }
                    });
                }
            }
            else {
                //var filterableColumns = jQuery(this).next().find('#column-dropdown');
                //if (filterableColumns.html()) {
                //    $.getJSON("/DataGrid/ReloadVisibleColumns/" + currentTableId, function (columns) {
                //        //$(filterableColumns).html('');
                //        for (var i = 0; i < columns.length; i++) {
                //        //    $(filterableColumns).append(
                //        //'<div class="column dropdown_sqlfunction_column_row">' +
                //        //    '<div class="item-id">' + columns[i][0] + '</div>' +
                //        //    '<div class="colnumber">' + columns[i][1] + '</div>' +
                //        //    '<div class="colname"><span>' + columns[i][2] + '</span></div>' +
                //            //'</div>');
                //        }
                //        if (!filterableColumns.html()) {
                //            //filterableColumns.html('<div style="display: hidden;"/>');
                //            $('#extsort .sortable-columns').html('<div style="display: hidden;"/>');
                //            $('#extsumma .sortable-columns').html('<div style="display: hidden;"/>');
                //        }
                //    });
                    jQuery('.op-dock.column').each(function (index) {
                        var $cinput = $(this).children('.dropdown:first');
                        var tx = $cinput.text();
                        $cinput.attr("title", tx); // set title for tooltip
                        var measuringDiv = $('<div class="measuringDiv" style="position:absolute;visibility:hidden;height:auto;width:auto;"></div>').text(tx); // Hidden div for measuring
                        if ($cinput.parent().children('.measuringDiv').length == 0) {
                            $cinput.parent().append(measuringDiv); // then add it,
                        } else {
                            $cinput.parent().children('.measuringDiv').replaceWith(measuringDiv); // ,else replace this
                        }
                        var txWithDots = tx;
                        while ($cinput.parent().children('.measuringDiv').width() > $cinput.width()) {
                            tx = tx.substring(0, tx.length - 1); // decrease the tx length 1 from the end
                            txWithDots = tx + "...";
                            $cinput.parent().children('.measuringDiv').text(txWithDots);
                        }
                        $cinput.text(txWithDots);
                    });
                //}
            }
        }
        jQuery(document).bind('click.archivedropdown', function (e) {
            if (e.isPropagationStopped()) return;
            $menu.fadeOut(100);
            jQuery(document).unbind('click.archivedropdown');
        });
        return false;
    });
    jQuery(document).on("click", ".header .reporttabfunctions", function () {
        var $button = jQuery(this).addClass('hover');
        jQuery(document).trigger('click.reportdropdown');
        jQuery(document).trigger('click.dropdown');
        jQuery(document).trigger('click.distinct');
        jQuery(document).trigger('click.reportgroup');
        var $menu = jQuery('#main-report-list').fadeIn();
        jQuery(document).bind('click.reportdropdown', function (e) {
            if (e.isPropagationStopped()) return;
            $menu.fadeOut(100);
            $button.removeClass('hover');
            jQuery(document).unbind('click.reportdropdown');
        });
        return false;
    });
    /*****************************************************************************
    * APPLY SETTINGS FOR ALL USERS
    ******************************************************************************/
    //jQuery('.apply-settings').click(function () {
    //    jQuery.get('/UserManagement/CreateTableSettingsCopyDialog', function (html) {
    //        var $dialog = jQuery('#dialog');
    //        if (html.search("login") != -1) {
    //            window.location = "/Account/LogOn";
    //        }
    //        $dialog.html(html).show().find('.dialog-btn').click(function () {
    //            var $button = jQuery(this);
    //            var $data = $button.find('.data').text();
    //            if ($data == 'False') {
    //                $dialog.hide();
    //            }
    //            else if ($data == 'True') {
    //                location.href = '/UserManagement/TableVisibilityCopy';
    //            }
    //        });
    //    });
    //});
    /******************************************************************************
    * ADD TABLE TYPE RIGHTS TO USER
    *******************************************************************************/
    jQuery(document).on("click", ".tabletyperight-switch", function () {
        var $link = jQuery(this);
        var tableType = $link.attr('href').substring($link.attr('href').indexOf('?') + 6, $link.attr('href').indexOf('&'));
        jQuery.get('/UserManagement/CreateTableTypeRightSwitchDialog', { tableType: tableType }, function (html) {
            var $dialog = jQuery('#dialog');
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html).show().find('.dialog-btn').click(function () {
                var $button = jQuery(this);
                var $data = $button.find('.data').text();
                if ($data == 'False') {
                    $dialog.hide();
                }
                else if ($data == 'True') {
                    console.log($link);
                    jQuery.get($link.attr('href'), function (htmll) {
                        if (htmll.search("login") != -1) {
                            window.location = "/Account/LogOn";
                        }
                        var $dialog = jQuery('#dialog').html(htmll).show();
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
                }
            });
        });
        return false;
    });
    /******************************************************************************
       * ADD SIMPLE RIGHTS TO ROLE
       *******************************************************************************/
    jQuery(document).on("click", ".simpleright-switch", function () {
        var $link = jQuery(this);
        var rolesettingType = $link.attr('href').substring($link.attr('href').indexOf('?') + 6, $link.attr('href').indexOf('&right='));
        var rightType = $link.attr('href').substring($link.attr('href').indexOf('&right=') + 7, $link.attr('href').indexOf('&tableId='));
        var tableId = $link.attr('href').substring($link.attr('href').indexOf('&tableId=') + 9);

        var $dialog = jQuery('#dialog');
        $dialog.show();

        jQuery.get('/UserManagement/UpdateRoleSettingDialog', { type: rolesettingType, right: rightType,  tableId: tableId }, function (html) {
            $dialog.html(html).show().find('.dialog-btn').click(function () {
                delete Updater.Listeners[key];
                jQuery('#dialog').hide();
            });

            var key = $dialog.find('.key').text();
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }

            Updater.Listeners[key] = function ($notification) {
                location.href = $notification.find('a.link').attr('href');
            };
        });
        return false;
    });
    /****************************************************************************
    * Reports methods
    ******************************************************************************/
    jQuery(document).on("click", ".dropdown-main", function (e) {
        var $target = jQuery(e.target);
        if ($target.is('input[type=submit]'))
            return true;
        if ($target.is('a'))
            return true;
        if ($target.hasClass('close-reportlist')) {
            jQuery(document).trigger('click.reportdropdown');
            $(document).trigger('click.distinct');
        }
        return false;
    });
    //Sort in popuplist
    jQuery(document).on("click", ".inner-container .reportsorting a", function () {
        var searchElement = jQuery('#searchForm input[type=text]');
        var search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) search = '';
        var url = jQuery(this).attr('href');
        url += '&search=' + search;
        jQuery.get(url, function (html) {
            var sortRow = jQuery('.reportsorting', html);
            var inner = jQuery('.main-table', html);
            jQuery('.reportsorting').html(sortRow.html());
            jQuery('.main-table').html(inner.html());
            addDragging();
        });
        return false;
    });    
    $("#search-tables").submit(function (e) {
        return false;
    });
    //Search Method
    var IssueSearchFunction = function () {
        jQuery('.main-table').fadeOut(500, function () {
            jQuery('.main-table').html('<div class="fill-loading"></div><div class="loading"></div>');
        });
        jQuery('.main-table').fadeIn(500);
        var searchElement = jQuery('#searchForm input[type=text]');
        var search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) search = '';
        var page = 0; //parseInt(jQuery('.inner-container .nav-controls .page').text());
        var size = jQuery('.inner-container .nav-controls .size').text();
        var sortColumn = jQuery('.inner-container .nav-controls .sortColumn').text();
        var direction = jQuery('.inner-container .nav-controls .direction').text();
        jQuery.get('/IssueList/IndexList', { json: true, size: size, page: page, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: false, showHidden: false, showArchived: false, operation: "filter" }, function (html) {
            jQuery('.main-table').fadeOut(500, function () {
                var inner = jQuery('.main-table', html);
                jQuery('.main-table').html(inner.html());
                addDragging();
            });
            jQuery('.main-table').fadeIn(500);
        });
        return false;
    };

    var delay = (function () {
        var timer = 0;
        return function (callback, ms) {
            clearTimeout(timer);
            timer = setTimeout(callback, ms);
        };
    })();

    // Search on button pressed
    jQuery(document).on("click", "#searchForm .submit", function () {
        IssueSearchFunction();
    });

    // Search on text change
    jQuery(document).on("change", "#searchForm [name=search]", function () {
        IssueSearchFunction();
    });
    jQuery(document).on("click", ".reportNavControls .activefirst,.reportNavControls .activeback,.reportNavControls .activenext,.reportNavControls .activelast", function (e) {
        jQuery('.main-table').fadeOut(500, function () {
            jQuery('.main-table').html('<div class="fill-loading"></div><div class="loading"></div>');
        });
        jQuery('.main-table').fadeIn(500);
        var searchElement = jQuery('#searchForm input[type=text]');
        var search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) search = '';
        var page = parseInt(jQuery('.inner-container .nav-controls .page').text());
        var size = jQuery('.inner-container .nav-controls .size').text();
        var sortColumn = jQuery('.inner-container .nav-controls .sortColumn').text();
        var direction = jQuery('.inner-container .nav-controls .direction').text();
        var navElement = e.target.getAttribute('nav');
        if (navElement == 'activefirst') {
            page = 0;
        } else if (navElement == 'activeback') {
            page = page - 1;
        } else if (navElement == 'activenext') {
            page = page + 1;
        } else if (navElement == 'activelast') {
            page = parseInt(e.target.getAttribute('navlast'));
        }
        jQuery.get('/IssueList/IndexList', { json: true, size: size, page: page, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: false, showHidden: false, showArchived: false, operation: "page" },
            function (html) {
                jQuery('.main-table').fadeOut(500, function () {
                    var inner = jQuery('.main-table', html);
                    jQuery('.main-table').html(inner.html());
                    addDragging();
                });
                jQuery('.main-table').fadeIn(500);
            }
        );
        return false;
    });
    jQuery(document).on("click", "#distinct-popup.dropdown-main .close-reportlist", function () {
        jQuery(document).trigger('click.distinct');
    });
    jQuery(document).on("click", "#distinct-popup2.dropdown-main .close-reportlist", function (e) {
        jQuery(document).trigger('click.distinct');
        // added to avoid all popups to close on a single close click
        e.stopPropagation();
    });
    $("body").on("mousedown", function (e) {
        var popup = $("#distinct-popup2.dropdown-main");
        if (popup.is(':visible') && !popup.is(e.target) && popup.has(e.target).length === 0) {
            $(document).trigger('click.distinct');
        }
    });
    jQuery(document).on("mouseover", ".gray-on-mouseover", function () {
        $(this).addClass('hover');
    });
    jQuery(document).on("mouseout", ".gray-on-mouseover", function () {
        $(this).removeClass('hover');
    });
    var validateAndSave = (function (e) {
        var $target = jQuery(e);
        $target.parent().siblings('.report-parameter-error').hide();
        jQuery(document).trigger('click.dropdown');
        var $form = $(".reportoverview form");
        var action = $form.get(0).action;
        var issueId = action.substring(action.indexOf('ExecuteIssue/') + 13);


        var parid = $target.parent().parent().find(".item-id").text();
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
    jQuery('.preload-record-count a').bind('click', function (e) {
        //the exact match checkbox checked
        if ($(e.target).hasClass('checkboxon')) {
            $(e.target).removeClass('checkboxon');
            $(e.target).addClass('checkboxoff');
        } else {
            $(e.target).addClass('checkboxon');
            $(e.target).removeClass('checkboxoff');
        }
        //  getIssueRecordCount();
    });
    jQuery('.preload-record-count .checkboxon').each(function () {
        //   getIssueRecordCount();
    });
    jQuery("#execute_report input").change(function () {
        //   getIssueRecordCount();
    });
    var selectedPopupInputfield;
    jQuery(document).on("click", ".report-distinct-menu .gray-on-mouseover", function (el) {
        $(this).siblings('.gray-on-mouseover').removeClass('aktiv');
        $(this).addClass('aktiv');
        var selectedvalue = $(this).find(".value").text();
        var paramId = $(this).parent().parent().attr("parameterid");
        var combobox = $(".selection-type select").parent().parent().find(".parameter-item input:hidden[value='" + paramId + "']").parent().parent().find(".selection-type select")
        var comboboxSelected = combobox.val();
        if (comboboxSelected == "2" || comboboxSelected == "3") {
            var currentValues = $(selectedPopupInputfield).val().split(";");
            var hasValueAlready = false;
            for (var i = 0; i < currentValues.length; i++) {
                if (currentValues[i] == selectedvalue) {
                    hasValueAlready = true;
                }
            }
            if (currentValues.length > 1) {
                if (!hasValueAlready) {
                    $(selectedPopupInputfield).val($(selectedPopupInputfield).val() + ";" + selectedvalue.replace(";", "\;"));
                }
            }
            else {
                if (!hasValueAlready) {
                    if ($(selectedPopupInputfield).hasClass('empty')) {
                        $(selectedPopupInputfield).val(selectedvalue.replace(";", "\;"));
                    } else {
                        $(selectedPopupInputfield).val($(selectedPopupInputfield).val() + ";" + selectedvalue.replace(";", "\;"));
                    }
                }
            }
        }
        else {
            $(selectedPopupInputfield).val(selectedvalue);
        }
        $(selectedPopupInputfield).removeClass('empty');

        if (comboboxSelected == "2" || comboboxSelected == "3") {
            $(this).removeClass('aktiv');
        }
        else {
            jQuery(document).trigger('click.distinct');
        }

        $(selectedPopupInputfield).trigger("change");
        //  getIssueRecordCount();
        return false;
    });
    jQuery(document).on("click", "#IssueList .parameter .select-distinct, #execute_report .parameter .select-distinct", function (e) {
        selectedPopupInputfield = $(e.target).parent().parent().find(".text.field");
        trigger_distinct_dropdown(e);
        return false;
    });
    //scrolling the div would not trigger the event.
    jQuery(".onereportparamset").scroll(function () {
        jQuery(document).trigger('click.distinct');
    });
    //jQuery(document).on("click", "#main-report-list table .reportresultrow td.toparams3 > span", function (e) {
    //    popupContext = new popup_context();
    //    popupContext.popupBindingMethod = update_report_objecttype;
    //    popupContext.target = $(e.target);
    //    popupContext.searchContext = null;
    //    trigger_distinct_column_value_dropdown($(e.target), update_report_objecttype, null);
    //    return false;
    //});
    // VIEWS:  binds the ObjectType value cell of the row in the list to display the dropdown popup for editing
    //jQuery(document).on("click", "#gesamt #ViewList.page-content .content-inner .tab-content .overview-header .tab-content-inner td.toobjecttype > span", function (e) {
    //    popupContext = new popup_context();
    //    popupContext.popupBindingMethod = update_view_objecttype;
    //    popupContext.target = $(e.target);
    //    popupContext.searchContext = new search_context();
    //    trigger_distinct_column_value_dropdown($(e.target), update_view_objecttype, popupContext.searchContext);
    //    return false;
    //});
    // ---------------- Object Type column Filtering for Report/Issues
    //binds the object types column click label (2nd div selector is the "Object Type" text)
    jQuery(document).on("click", "div#main-report-list.dropdown-main  tr.reportsorting td.toobjecttype div:nth-child(2)", function (e) {
        popupContext = new popup_context();
        popupContext.popupBindingMethod = create_report_distinct_column_data;
        popupContext.target = $(e.target);
        popupContext.searchContext = new search_context();
        trigger_distinct_column_value_dropdown($(e.target), create_report_distinct_column_data, popupContext.searchContext);
        return false;
    });
    jQuery(document).on("click", "div.tab-content table.overview-header thead tr th.toobjecttype div.sort-column", function (e) {
        pageContext = new page_context();
        popupContext = new popup_context();
        popupContext.popupBindingMethod = create_distinct_column_data;
        popupContext.target = $(e.target);
        popupContext.searchContext = new search_context();
        trigger_distinct_column_value_dropdown(jQuery(e.target), create_distinct_column_data, popupContext.searchContext);
        return false;
    });
    // ---------------- Object Type column Filtering  for Views + common
    trigger_distinct_column_value_dropdown = function (el, fillPopupAction, searchContext) {
        // displays and positions the popup according to the element beeing clicked (el)        
        var $target = $(el);
        var $popup = $("#distinct-popup2.dropdown-main").show();
        var popupHeight = $popup.height() == 0 ? 300 : $popup.height(); // sets a 300px height if none
        var popupWidth = $popup.width();
        var leftSpace = $target.offset().left;
        var rightSpace = $(document).width() - $target.offset().left;
        var topSpace = $target.offset().top;
        var bottomSpace = $(document).height() - $target.offset().top;
        var showAtTop = topSpace > bottomSpace;
        var showAtLeft = leftSpace > rightSpace;
        var topClass = "";
        var bottomClass = "";
        var topPosition = showAtTop ? topSpace - popupHeight - 30 : topSpace + 30;
        var leftPosition = showAtLeft ? leftSpace - popupWidth - 40 : leftSpace + 40;
        $popup.offset({ top: topPosition, left: leftPosition }).css("z-index", "1002");
        $("#distinct-popup2 .distinct-main > [class^='triangle']").hide();
        if (showAtTop) {
            bottomClass = showAtLeft ? "trianglerightdown" : "triangleleftdown";
            $("#distinct-popup2 ." + bottomClass).show();
        } else {
            topClass = showAtLeft ? "triangleright" : "triangleleft";
            $("#distinct-popup2 ." + topClass).show();
        }
        if (fillPopupAction != null)
            fillPopupAction($popup.find('.inner-container > div'), el, searchContext)
        jQuery(document).on('click.distinct', function (e) {
            var isPropStopped = e.isPropagationStopped();
            if (!(typeof isPropStopped === 'undefined') && isPropStopped) return;
                $popup.hide();
            jQuery(document).off('click.distinct');
            e.stopPropagation();
        });
    };
    update_view_objecttype = function (popupInnerContainer, objectTypeCell, searchContext, sortDirection) {
        var $inner = $(popupInnerContainer);
        var oldObjectType = $(objectTypeCell).html();
        var tableId = $(objectTypeCell).parents("tr.tableobject").find(".tono [data-tableId]").data("tableid");
        $inner.html('<div class="fill-loading"></div><div class="loading"></div>');
        pageContext = new page_context();
        $.get('/ObjectType/Index', { sortDirection: sortDirection },
            function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                $inner.html(html);
                $("#distinct-popup2.dropdown-main .column-filter-menu .distinct").bind('click', function (e) {
                    $.get("/ObjectType/Save",
                        {
                            tableTypeKey: new page_context().table_type,
                            oldObjectType: oldObjectType,
                            newObjectTypeId: $(e.target).data("id"),
                            tableId: tableId,
                            searchContext: JSON.stringify(searchContext)
                        },
                        function (html) {
                            if (html.search("login") != -1) {
                                window.location = "/Account/LogOn";
                            }
                            var modifiedRowSelector = ".tab-content-inner > table tr.tableobject:has(div[data-tableId='" + tableId + "'])";
                            if ($(modifiedRowSelector).length == 1) {
                                $(modifiedRowSelector).html($(modifiedRowSelector, html).html());
                                $(modifiedRowSelector + " .toobjecttype span").effect("highlight", {}, 2000);
                            }
                            return false;
                        })
                });
            });
        return false;
    };
    update_report_objecttype = function (popupInnerContainer, objectTypeCell, searchContext, sortDirection) {
        var $inner = $(popupInnerContainer);
        var oldObjectType = $(objectTypeCell).html();
        var tableId = $(objectTypeCell).parents("tr.reportresultrow").find(".tono [data-tableId]").data("tableid");
        $inner.html('<div class="fill-loading"></div><div class="loading"></div>');
        pageContext = new page_context();
        $.get('/ObjectType/Index', { sortDirection: sortDirection },
            function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                $inner.html(html);
                $("#distinct-popup2.dropdown-main .column-filter-menu .distinct").bind('click', function (e) {
                    $.get("/ObjectType/Save",
                        {
                            tableTypeKey: new page_context().table_type,
                            oldObjectType: oldObjectType,
                            newObjectTypeId: $(e.target).data("id"),
                            tableId: tableId
                        },
                        function (html) {
                            if (html.search("login") != -1) {
                                window.location = "/Account/LogOn";
                            }
                            $('.main-table:has(>#destinationTable)').html($('.main-table:has(>#destinationTable)', html).html());
                            $("[data-tableId='" + tableId + "']").parents(".reportresultrow").find(".toparams3 span").effect("highlight", {}, 2000);
                            addDragging();
                            return false;
                        })
                });
            });
        return false;
    };
    create_report_distinct_column_data = function (popupInnerContainer, objectTypeCell, searchContext, sortDirection) {
        var inner = jQuery(popupInnerContainer);
        inner.html('<div class="fill-loading"></div><div class="loading"></div>');
        pageContext = new page_context();
        // quick fix to set the column sorting for the popup distinct on column (not for the parent search context)
        jQuery.get('/' + pageContext.page_content_type + "/" + pageContext.column_filter_content_action, { direction: sortDirection },
        function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            inner.html(html);
        });
        return false;
    };
    $("#distinct-popup2.dropdown-main .inner-container .column-filter-menu").on('click', "#IssueColumnValueList .distinct", function () {
        var searchElement = jQuery('#searchForm input[type=text]');
        search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) search = '';

        popupContext.searchContext.search = search;
        popupContext.searchContext.objectTypeFilter = $(this).find("[data-id]").data('id');
        popupContext.searchContext.json = false;
        popupContext.searchContext.catRefresh = false;
        popupContext.searchContext.operation = "filter";

        $("#distinct-popup2").hide();
        $.get('/' + pageContext.page_content_type + "/" + pageContext.main_content_action, popupContext.searchContext,
            function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                $('#main-report-list.dropdown-main').html($('#main-report-list.dropdown-main', html).html());
                addDragging();
                return false;
            }
        );
    });
    create_distinct_column_data = function (popupInnerContainer, objectTypeCell, searchContext, sortDirection) {
        var inner = jQuery(popupInnerContainer);
        inner.html('<div class="fill-loading"></div><div class="loading"></div>');
        var pageContext = new page_context();
        var searchContext = popupContext.searchContext;
        // quick fix to set the column sorting for the popup distinct on column (not for the parent search context)
        jQuery.get(
            '/' + pageContext.page_content_type + '/' + pageContext.column_filter_content_action,
            {
                search: searchContext.search,
                direction: sortDirection,
                showArchived: searchContext.showArchived,
                showEmpty: searchContext.showEmpty,
                showHidden: searchContext.showHidden,
                showEmptyHidden: searchContext.showEmptyHidden
            },
            function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                inner.html(html);
            }
        );
        return false;
    };

    $("#distinct-popup2.dropdown-main .inner-container .column-filter-menu").on('click', "#ViewColumnValueList .distinct", function () {
        popupContext.searchContext.objectTypeFilter = $(this).find("[data-id]").data('id');
        popupContext.searchContext.json = true;
        popupContext.searchContext.catRefresh = true;
        popupContext.searchContext.operation = "filter";

        $("#distinct-popup2").hide();
        $.get('/' + pageContext.page_content_type + '/' + pageContext.main_content_action, popupContext.searchContext,
            function (html) {
                jQuery('.content-box').replaceWith(html);
                return false;
            }
        );
    });
    // tracks clicks on the sorting arrows
    jQuery("#distinct-popup2.dropdown-main .inner-container .column-filter-menu").on("click", ".distinct-sorting > a", false,  function (e) {
        if (e.target.hasAttribute('nav')) {
            if (popupContext != null && popupContext.popupBindingMethod != null) {
                var sortDirection = $(this).hasClass("active") ? "none" : $(this).attr("nav");
                popupContext.popupBindingMethod($("#distinct-popup2.dropdown-main .inner-container > div"), popupContext.target, popupContext.searchContext, sortDirection);
            }
        }
    });
    // Gathers information about the page context (controller, the main action...)
    // enables object to be reused among different page context (Issues/Reports, Archive, VIew, etc.)
    page_context = function () {
        this.page_content_type = jQuery('.page-content')[0].id;
        if (typeof (this.page_content_type) !== "undefined") {
            // gets the mainContentAction attribute value
            this.main_content_action = $("#" + this.page_content_type).find("[data-mainContentAction]").data("maincontentaction");
        }
        this.table_type = $("#" + this.page_content_type).find("[data-tableType]").data("tabletype");
        this.column_filter_content_action = $("#" + this.page_content_type).find("[data-columnFilterContentAction]").data("columnfiltercontentaction");
    };
    var pageContext = null;
    // holds info to fill the list of the popup
    // used for sorting the popup list
    popup_context = function () {
        // method to fill the popup list
        this.popupBindingMethod = null;
        // clicked element
        this.target = null;
        this.searchContext = null;
    };
    var popupContext = null;
    search_context = function (json, catRefresh) {
        this.json = json;
        this.catRefresh = catRefresh;
        //showEmpty
        this.showEmpty = jQuery('.tabbar .selected').hasClass('show_empty');
        //showHidden
        this.showHidden = $('.tabbar .selected').hasClass('show_hidden');
        //showArchived
        this.showArchived = $('.tabbar .selected').hasClass('show_archived');
        //showEmptyHidden
        this.showEmptyHidden = $('.tabbar .selected').hasClass('show_empty_hidden');
        //search
        this.search = $('#searchoverview input[type=text]').val();
        // clear the search value if value = placeholder
        if (this.search == $('#searchoverview input').attr('placeholder')) this.search = '';
        //size (for some kind of weired reason ".nav-controls > div.size" returns many elements, the first one havinf wrong values, so we get the lst one )
        this.size = jQuery('.nav-controls:last > div.size').text();
        // currentPage
        this.page = jQuery('.nav-controls:last > div.page').text();
        //sortColumn
        this.sortColumn = jQuery('.nav-controls:last  > div.sortColumn').text();
        //sort 
        this.direction = jQuery('.nav-controls:last  > div.direction').text();
        this.objectTypeFilter = "";
    };
    /* -------------------- END Object Type Popup ------------------------------  */
    trigger_distinct_dropdown = function (el) {
        jQuery(document).trigger('click.distinct');
        jQuery(document).trigger('click.dropdown');
        jQuery(document).trigger('click.reportdropdown');
        jQuery(document).trigger('click.reportgroup');
        var $book = jQuery(el.target);

        var $menu = $("#distinct-popup").show();

        // set initial position
        $("#distinct-popup").offset({ top: 10, left: 200 });

        $menu.find('.report-distinct-menu').find('#searchBar').val('');
        var addTop = 0;
        var addLeft = 0;
        // removes all triangle (to compensate the triangles added afterwards in later version)
        $("#distinct-popup .distinct-main > [class^='triangle']").hide();
        var body = jQuery(".onereportparamset");
        var distinctMain = jQuery("#distinct-popup");
        var correction = ($(document).width() - $(".content-box").width()) / 2;
        if ($(".onereportparamset").height() + $book.offset().top - 125 > $(document).height()) {
            //down
            addTop = -3 - $(".distinct-main").height();
            if ($book.offset().left + $(".distinct-main").width() - correction > $(".content-box").width()) {
                //left
                addLeft = -393;
                $("#distinct-popup .triangleleftdown").hide();
                $("#distinct-popup .trianglerightdown").show();
                $("#distinct-popup .triangleleft").hide();
                $("#distinct-popup .triangleright").hide();
            }
            else {
                //right
                $("#distinct-popup .triangleleftdown").show();
                $("#distinct-popup .trianglerightdown").hide();
                $("#distinct-popup .triangleleft").hide();
                $("#distinct-popup .triangleright").hide();
            }
        }
        else {
            //up
            addTop = 19;
            if ($book.offset().left + $(".distinct-main").width() - correction > $(".content-box").width()) {
                //left
                addLeft = -393;
                $("#distinct-popup .triangleleftdown").hide();
                $("#distinct-popup .trianglerightdown").hide();
                $("#distinct-popup .triangleleft").hide();
                $("#distinct-popup .triangleright").show();
            }
            else {
                //right
                $("#distinct-popup .triangleleftdown").hide();
                $("#distinct-popup .trianglerightdown").hide();
                $("#distinct-popup .triangleleft").show();
                $("#distinct-popup .triangleright").hide();
            }
        }

        //Add the parameter to the 
        $menu.find('.report-distinct-menu').attr("parameterId", $book.attr("parameterId"));
        $menu.find('.report-distinct-menu').attr("columnId", $book.attr("columnId"));
        $menu.offset({ top: $book.offset().top + addTop, left: $book.offset().left - 110 + addLeft });
        //bug with setting the offset top in ie in a scrollable container.
        //it's fixed with, just calling the function again.
        $menu.offset({ top: $book.offset().top + addTop, left: $book.offset().left - 110 + addLeft });
        el.target.setAttribute('nav', 'activefirst');
        create_distinct_data($menu.find('.report-distinct-menu'), el);
        jQuery(document).bind('click.distinct', function (e) {
            var isPropStopped = e.isPropagationStopped();
            if (!(typeof isPropStopped === 'undefined') && isPropStopped) return;
            $menu.hide();
            jQuery(document).unbind('click.distinct');
        });
    };
    jQuery(document).on("click", ".dropdown-main .outer-container .inner-container .report-distinct-menu .navigation .iconx, .dropdown-main .outer-container .inner-container .report-distinct-menu .ascending, .dropdown-main .outer-container .inner-container .report-distinct-menu .descending", function (e) {
        var $control = jQuery(e.target);
        if (e.target.hasAttribute('nav'))
            create_distinct_data($control.parents('.report-distinct-menu'), e);
        return false;
    });
    jQuery(document).on("click", "#searchFormDistinct .submit", function (e) {
        var $control = jQuery(e.target);
        create_distinct_data($control.parents('.report-distinct-menu'), e);
        return false;
    });
    var actualParamId = 0;
    create_distinct_data = function (el, comp) {
        var inner = jQuery(el);
        var parameterId = parseInt(inner.attr("parameterId"));
        actualParamId = parameterId;
        var checkboxChecked = inner.find('.exactmatch a').hasClass('checkboxon');
        var searchElement = inner.find('#searchFormDistinct input[type=text]');
        var search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) search = '';
        var start = parseInt(inner.find('#value-distinct-start').text());
        var rowsPerPage = parseInt(inner.find("#value-per-page").text());
        var directionElement = inner.find('#value-distinct-direction').text();
        var directions = [];
        var sortColumns = [];
        var oldSearch = inner.find('#value-distinct-search').text();
        if (!(typeof comp.target === 'undefined') && comp.target.hasAttribute('nav')) {
            var navElement = comp.target.getAttribute('nav');
            if (navElement == 'activefirst') {
                start = 0;
                if (directionElement == "value__DESC") {
                    directions.push("DESC");
                    sortColumns.push("value");
                }
                else if (directionElement == "value__ASC") {
                    directions.push("ASC");
                    sortColumns.push("value");
                }
                else if (directionElement == "description__DESC") {
                    directions.push("DESC");
                    sortColumns.push("description");
                }
                else if (directionElement == "description__ASC") {
                    directions.push("ASC");
                    sortColumns.push("description");
                }
            } else if (navElement == 'activeback') {
                start = (start - rowsPerPage) < 0 ? 1 : (start - rowsPerPage);
                if (directionElement == "value__DESC") {
                    directions.push("DESC");
                    sortColumns.push("value");
                }
                else if (directionElement == "value__ASC") {
                    directions.push("ASC");
                    sortColumns.push("value");
                }
                else if (directionElement == "description__DESC") {
                    directions.push("DESC");
                    sortColumns.push("description");
                }
                else if (directionElement == "description__ASC") {
                    directions.push("ASC");
                    sortColumns.push("description");
                }
            } else if (navElement == 'activenext') {
                start = start + rowsPerPage;
                if (directionElement == "value__DESC") {
                    directions.push("DESC");
                    sortColumns.push("value");
                }
                else if (directionElement == "value__ASC") {
                    directions.push("ASC");
                    sortColumns.push("value");
                }
                else if (directionElement == "description__DESC") {
                    directions.push("DESC");
                    sortColumns.push("description");
                }
                else if (directionElement == "description__ASC") {
                    directions.push("ASC");
                    sortColumns.push("description");
                }
            } else if (navElement == 'activelast') {
                start = parseInt(comp.target.getAttribute('navlast'));
                if (directionElement == "value__DESC") {
                    directions.push("DESC");
                    sortColumns.push("value");
                }
                else if (directionElement == "value__ASC") {
                    directions.push("ASC");
                    sortColumns.push("value");
                }
                else if (directionElement == "description__DESC") {
                    directions.push("DESC");
                    sortColumns.push("description");
                }
                else if (directionElement == "description__ASC") {
                    directions.push("ASC");
                    sortColumns.push("description");
                }
            } else if (navElement == 'ascending_val') {
                directions.push("ASC");
                sortColumns.push("value")
                start = 0;
            } else if (navElement == 'descending_val') {
                directions.push("DESC");
                sortColumns.push("value")
                start = 0;
            } else if (navElement == 'ascending_desc') {
                directions.push("ASC");
                sortColumns.push("description")
                start = 0;
            } else if (navElement == 'descending_desc') {
                directions.push("DESC");
                sortColumns.push("description")
                start = 0;
            }
        }
        if (oldSearch != search)
            start = 0;

        inner.html('<div class="fill-loading"></div><div class="loading"></div>');
        var selectedParameters = new Array();
        var i = 0;
        $('.parameter-table tbody tr').each(function () {
            var key = parseInt($(this).find('.parameter-item .item-id').html());
            var value = $(this).find('.parameter-item .text').val();
            if (!$(this).find('.parameter-item .text').hasClass('empty') && key != parameterId && value != "geben Sie Wert ein..." && value !== null && value != '') {
                selectedParameters[i] = key + ";" + value;
                i++
            }
        });
        jQuery.get('/Wertehilfe/Index', { parameterId: parameterId, search: search, isExact: checkboxChecked, start: start, currentValues: selectedParameters, sortColumns: sortColumns, directions: directions },
            function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                var pid = parseInt($(html).find("#paramId").val());

                if (actualParamId == 0 || actualParamId == pid) {
                    inner.html(html);
                    jQuery(".thousand-separator2").each(function () { jQuery(this).numberWithSeparators2(); });

                    var select;
                    $(".parameter-item").each(function () {
                        var itemId = $(this).find("#itemId").val();
                        if (itemId == parameterId) {
                            select = $(this).parent().find("#selectionType");
                        }
                    });
                    if (select.val() >= 2) {
                        $("#append-all").show();
                    }
                    actualParamId = 0;
                }
            });
        return false;
    };
    trigger_parametergroups = function () {
        jQuery(document).trigger('click.reportgroup');
        jQuery(document).trigger('click.distinct');
        jQuery(document).trigger('click.dropdown');
        jQuery(document).trigger('click.reportdropdown');
        var $popup = jQuery('#reportgroup').show();
        jQuery(document).bind('click.reportgroup', function (e) {
            var isPropStopped = e.isPropagationStopped();
            if (!(typeof isPropStopped === 'undefined') && isPropStopped) return;
            $popup.hide();
            jQuery(document).unbind('click.reportgroup');
        });
    };
    jQuery(document).on("click", "#loadParameterGroup", function (e) {
        trigger_parametergroups(jQuery(e.target));
        return false;
    });
    jQuery(document).on("click", "#reportgroup .close-reportlist", function () {
        jQuery(document).trigger('click.reportgroup');
    });
    jQuery(document).on("click", ".button-crud", function () {
        var th = jQuery(this);
        var id = th.attr('ref-id');
        var tableid = th.attr('ref-table-id');
        var popup = jQuery('#crud-popup');
        var popupinner = jQuery('#crud-popup-inner');
        var popuptitle = jQuery('#dropdown-title');
        jQuery.get('/IssueList/_TableCrud', { id: id, tableId: tableid }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            popupinner.html(html);
            var title = "";
            var titles = popupinner.find(".crud-data-header");
            if (titles.length > 0)
                title = titles[0].innerHTML;
            popuptitle.html(title);
            popup.show();
            initCrud();
        });
    });
    jQuery(document).on("click", ".button-crud-small.edit", function () {
        var th = jQuery(this);
        var tableid = th.attr('ref-table-id');
        var rowno = th.attr('ref-rowno');
        var popup = jQuery('#crud-popup');
        var popupinner = jQuery('#crud-popup-inner');
        var popuptitle = jQuery('#dropdown-title');
        jQuery.get('/IssueList/_TableCrudEdit', { tableId: tableid, rowno: rowno }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            popupinner.html(html);
            var title = "";
            var titles = popupinner.find(".crud-data-header");
            if (titles.length > 0)
                title = titles[0].innerHTML;
            popuptitle.html(title);
            popup.show();
            initCrud();
        });
    });
    jQuery(document).on("click", ".button-crud-small.delete", function () {
        var th = jQuery(this);
        var tableid = th.attr('ref-table-id');
        var rowno = th.attr('ref-rowno');
        jQuery.get('/IssueList/_DeleteCrud', { tableId: tableid, rowno: rowno }, function () {
            location.reload();
        });
    });
    jQuery(document).on("click", '#reportgroup', function (e) {
        selectedPopupInputfield = jQuery(e.target).siblings('.text');
        trigger_distinct_dropdown(e);
        return false;
    });
    jQuery(document).on("click", ".cancel-crud", function () {
        closeCrud();
    });
    jQuery(document).on("click", "#crud-popup .close-reportlist", function () {
        closeCrud();
    });
    jQuery(document).on("change", ".crud-data-column-val .calculate-value", function () {
        jQuery("#AddCrudForm").trigger("submit");
    });
    jQuery("#crud-data-loading").bind()
    /****************************************************************************
    * Dragging method in reports.
    ****************************************************************************/
    addDragging = function () {
        jQuery("#sourceTable tr.draggable").draggable({
            containment: '#drag-wrapper',
            handle: '.drag-drop-button',
            revert: 'invalid',
            cursor: 'move',
            helper: 'clone',
            scroll: true,
            axis: "y"
        });
        jQuery("#destinationTable tr.draggable").draggable({
            containment: '#drag-wrapper',
            handle: '.drag-drop-button',
            revert: 'invalid',
            cursor: 'move',
            helper: 'clone',
            scroll: true,
            axis: "y"
        });
        $("#destinationDiv").droppable({
            accept: "#sourceTable tr",
            drop: function (event, ui) {
                var $item = ui.draggable;
                var link = $item.find('a.viewoptions').attr('href');
                var id = link.substring(link.indexOf('ShowOne') + 8);
                jQuery.get('/IssueList/SaveFavorite', { id: id }, function () {
                    jQuery('#searchForm .submit').click();
                });
            }
        });
        $("#destinationTable").droppable({
            accept: "#sourceTable tr",
            drop: function (event, ui) {
                var $item = ui.draggable;
                var link = $item.find('a.viewoptions').attr('href');
                var id = link.substring(link.indexOf('ShowOne') + 8);
                jQuery.get('/IssueList/SaveFavorite', { id: id }, function () {
                    jQuery('#searchForm .submit').click();
                });
            }
        });
        $("#sourceTable").droppable({
            accept: "#destinationTable tr",
            drop: function (event, ui) {
                var $item = ui.draggable;
                var link = $item.find('a.viewoptions').attr('href');
                var id = link.substring(link.indexOf('ShowOne') + 8);
                jQuery.get('/IssueList/DeleteFavorite', { id: id }, function () {
                    jQuery('#searchForm .submit').click();
                });
            }
        });
    };
    addDragging();

    /****************************************************************************
    * ModifyStartScreen
    ****************************************************************************/
    jQuery("#modifystartscreen div.delete-picture form").on("submit", function (e) {
        $form = $(this);
        if ($form.find("select").val()) {
            var $newDialog = jQuery('#dialog1').show().html(jQuery('#deleteConfirmDialog').html());
            $newDialog.find('.dialog-btn').click(function () {
                $newDialog.hide();
                if (jQuery(this).find('.data').text() == 'True') {
                    $form.unbind("submit");
                    $form.submit();
                }
            });
        }
        e.preventDefault();
    });

    jQuery("#modifystartscreen #setdefault").on("click", function () {
        var selected = jQuery("#Pictures").find('option:selected');
        jQuery.get('/Settings/SetDeafultPicture', { pic: selected.val() },
            function () { /*Nothing to do*/ }
        );
    });
});