jQuery(function () {
    /**************************************************************************
    * DATEPICKER
    **************************************************************************/
    var initDatePicker;
    var dateFormatka = jQuery("#currentLanguage").val() === "en" ? "dd/mm/yy" : "dd.mm.yy";
    var currentYearRange = jQuery("#currentYearRange").val();

    (initDatePicker = function () {
        jQuery('.field.reportdate').datepicker({
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
    jQuery(document).on("click", "#TableList .header>.nav-controls a.iconx, #IssueList .header>.nav-controls a.iconx, #ViewList .header>.nav-controls a.iconx", function () {
        // var $dialog = jQuery('#dialog').html('').show();

        // searchForm disable
        var searchForm = $("#searchoverview").parent();
        $("#searchoverview .field.bg-middle").attr("disabled", "disabled");

        var navigation = $(this).parent();
        navigation.find('a').each(function () {
            $(this).css('pointer-events', 'none');
        });

        jQuery('.tab-content').fadeOut(125, function () {
            jQuery("#loading").css('z-index', '10');
            jQuery('#loading').fadeIn();
        });

        //showEmpty
        var showEmpty = false;
        if (jQuery('.tabbar .selected').hasClass('show_empty')) showEmpty = true;
        //showHidden
        var showHidden = false;
        if (jQuery('.tabbar .selected').hasClass('show_hidden')) showHidden = true;
        //showArchived
        var showArchived = false;
        if (jQuery('.tabbar .selected').hasClass('show_archived')) showArchived = true;
        //search
        var searchElement = jQuery('#searchoverview input[type=text]');
        var search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) search = '';
        //size
        var size = jQuery('.header>.nav-controls .size').text();
        //sort 
        var sortColumn = jQuery('.header>.nav-controls .sortColumn').text();
        var direction = jQuery('.header>.nav-controls .direction').text();
        var link = jQuery(this).attr('href');

        setTimeout(function () {
            jQuery.get(link, { json: true, size: size, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: showEmpty, showHidden: showHidden, showArchived: showArchived }, function (html) {
                jQuery('.content-box').replaceWith(html, function () {
                    jQuery('#loading').fadeOut(125);
                    initDatePicker();
                    jQuery('.tab-content').fadeIn(125);
                    navigation.find('a').each(function () {
                        $(this).css('pointer-events', 'auto');
                    });
                    $("#searchoverview .field.bg-middle").removeAttr("disabled");
                });
            });
        }, 500);

        return false;
    });
    jQuery(".thousand-separator").numberWithSeparators();

    /**************************************************************************
    * TableList, IssueList, ViewList - enlarge
    **************************************************************************/
    jQuery(document).on("click", ".enlarge", function () {
        var $list = jQuery(".page-content");
        if ($list.hasClass("enlarged")) {
            jQuery('#DataGrid').removeClass("enlarged");
            jQuery('#page').removeClass("page_enlarged");
            jQuery('#page').addClass("page");
            jQuery('#navbottom').removeClass("enlarged_navbottom");
            jQuery('#navbottom').addClass("navbottom");
            return false;
        } else {
            jQuery('#DataGrid').addClass("enlarged");
            jQuery('#page').addClass("page_enlarged");
            jQuery('#page').removeClass("page");
            jQuery('#navbottom').addClass("enlarged_navbottom");
            jQuery('#navbottom').removeClass("navbottom");
            return false;
        }
    });
    /**************************************************************************
    * ViewOptions - enlarge
    **************************************************************************/
    jQuery(document).on("click", ".viewoptions-enlarge", function () {
        var $viewoptions = jQuery("#viewoptions .dropdown-menu");
        if ($viewoptions.hasClass("viewoptions-enlarged")) {
            $viewoptions.removeClass("viewoptions-enlarged").addClass("normal");
            return false;
        } else {
            $viewoptions.removeClass("normal").addClass("viewoptions-enlarged");
            return false;
        }
    });
    /**************************************************************************
    * TABLE FUNCTIONS
    **************************************************************************/
    jQuery(document).on("focus", "#table_select", function () {
        jQuery('tableobjects').insertAfter(jQuery(this)).fadeIn();
    }).on("blur", "#table_select", function () {
        jQuery('tableobjects').insertAfter(jQuery(this)).fadeOut();
    });

    var tabHeaderClick = function (tabHeaderElement) {
        if (!jQuery(tabHeaderElement).hasClass('selected')) {
            var $dialog = jQuery('#dialog').html('').show();
            var showEmpty = jQuery(tabHeaderElement).hasClass('show_empty');
            var showHidden = jQuery(tabHeaderElement).hasClass('show_hidden');
            var showArchived = jQuery(tabHeaderElement).hasClass('show_archived');
            var showEmptyHidden = jQuery(tabHeaderElement).hasClass('show_empty_hidden');
            var searchElement = jQuery('#searchoverview input[type=text]');
            var tabChange = true;
            var search = searchElement.val();
            if (search == null) {
                searchElement = jQuery('#reportlistpopupcontainer input[type=text]');
                search = searchElement.val();
            }
            var placeholder = searchElement.attr('placeholder');
            if (search == placeholder) search = '';
            var sortColumn = jQuery('.header .nav-controls .sortColumn').text();
            var direction = jQuery('.header .nav-controls .direction').text();
            var url;

            url = jQuery('.page-content')[0].id;
            jQuery.get('/' + url + '/Index/', {
                json: true, search: search, showHidden: showHidden, showEmpty: showEmpty, showEmptyHidden: showEmptyHidden, showArchived: showArchived, catRefresh: true, sortColumn: sortColumn, direction: direction,
                showEmptyHidden: showEmptyHidden, tabChange: tabChange
            }, function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                jQuery('.content-box').replaceWith(html);
                $dialog.hide();
            });

            return false;
        }
    };

    jQuery(document).on("click", "#TableList .tabbar div.tabheader, #ViewList .tabbar div.tabheader, #IssueList .tabbar div.tabheader", function () {
        tabHeaderClick(this);
        return false;
    });
    jQuery(document).on("click", "#TableList .tabbar-new .tabheader, #ViewList .tabbar-new .tabheader, #IssueList .tabbar-new .tabheader", function () {
        tabHeaderClick(this);
        return false;
    });

    jQuery(document).on("click", ".overview-header .sorting a", function () {
        var $dialog = jQuery('#dialog').show();
        var searchElement = jQuery('#searchoverview input[type=text]');
        var search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) {
            search = '';
        }
        var url = jQuery(this).attr('href');
        var tabheader = jQuery('.tabbar .selected');
        url += '&showEmpty=';
        if (tabheader.hasClass('show_empty')) {
            url += 'true';
        } else {
            url += 'false';
        }
        url += '&showHidden=';
        if (tabheader.hasClass('show_hidden')) {
            url += 'true';
        } else {
            url += 'false';
        }
        url += '&showArchived=';
        if (tabheader.hasClass('show_archived')) {
            url += 'true';
        } else {
            url += 'false';
        }
        url += '&showEmptyHidden=';
        if (tabheader.hasClass('show_empty_hidden')) {
            url += 'true';
        } else {
            url += 'false';
        }
        url += '&search=' + search;
        jQuery.get(url, { catRefresh: true }, function (html) {
            jQuery('.content-box').replaceWith(html);
            $dialog.hide();
        });
        return false;
    });
    jQuery(document).on("click", "#IssueList .toparams .delete a, .reporttoparams .delete a", function () {
        var $dialog = jQuery('#dialog').show();
        var id = jQuery(this).prev().text();
        jQuery.get('/IssueList/DeleteConfirmationDialog/', function (dialog_html) {
            if (dialog_html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(dialog_html);
            $dialog.find('.dialog-btn').click(function () {
                $button = jQuery(this);
                if ($button.find('.data').text() == 'False') $dialog.hide();
                else if ($button.find('.data').text() == 'True') {
                    $dialog.find('.dialog-btn').hide();
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
                    jQuery.get('/IssueList/DeleteIssue/', { id: id, json: true, search: search, catRefresh: true, sortColumn: sortColumn, direction: direction, size: size, page: page },
                        function (html) {
                            if (html == "") {
                                window.location = "/Home/Index";
                            } else {
                                window.location = "/IssueList";
                            }
                        }
                    );
                }
            });
        });
        return false;
    });
    jQuery(document).on("click", "#ViewList .toparams2 .delete a, #ViewList .toparams4 .delete a", function () {
        var $dialog = jQuery('#dialog').show();
        var id = jQuery(this).prev().text();
        jQuery.get('/ViewList/DeleteConfirmationDialog/', function (dialog_html) {
            if (dialog_html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(dialog_html);
            $dialog.find('.dialog-btn').click(function () {
                $button = jQuery(this);
                if ($button.find('.data').text() == 'False') $dialog.hide();
                else if ($button.find('.data').text() == 'True') {
                    $dialog.find('.dialog-btn').hide();
                    var tabheader = jQuery('.tabbar .selected');
                    var showEmpty = tabheader.hasClass('show_empty');
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
                    jQuery.get('/ViewList/DeleteView/', { id: id, json: true, search: search, showEmpty: showEmpty, catRefresh: true, sortColumn: sortColumn, direction: direction, size: size, page: page },
                        function (html) {
                            if (html == "") {
                                window.location = "/Home/Index";
                            } else {
                                jQuery('.content-box').replaceWith(html);
                                $dialog.html('').hide();
                            }
                        }
                    );
                }
            });
        });
        return false;
    });
    jQuery(document).on("click", ".toparams .hide-table a, .toparams2 .hide-table a, .toparams4 .hide-table a, .reporttoparams .hide-table a", function () {
        var $dialog = jQuery('#dialog').show();
        var parent_tr = jQuery(this).parent().parent().parent().parent().parent();
        var tab = jQuery('.tabbar .selected');
        var showEmpty = tab.hasClass('show_empty');
        var showHidden = tab.hasClass('show_hidden');
        var showEmptyHidden = tab.hasClass('show_empty_hidden');
        var showArchived = tab.hasClass('show_archived');
        var searchElement = jQuery('#searchoverview input[type=text]');
        var search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) search = '';
        var sortColumn = jQuery('.header #mainnavigation.nav-controls .sortColumn').text();
        var direction = jQuery('.header #mainnavigation.nav-controls .direction').text();
        var page = jQuery('.header #mainnavigation.nav-controls .page').text();
        var size = jQuery('.header #mainnavigation.nav-controls .size').text();
        if (!page) page = 0;
        if (!size) size = 25;
        var url = jQuery('.page-content')[0].id;
        jQuery.get('/Command/UpdateTable/',
            {
                id: jQuery(this).prev('span').text(), json: true, search: search, showHidden: showHidden, showEmpty: showEmpty, showArchived: showArchived, catRefresh: true,
                sortColumn: sortColumn, direction: direction, url: url, showEmptyHidden: showEmptyHidden, size: size, page: page,
            },
            function (html) {
                parent_tr.fadeOut(250);

                if ($dialog.html().search("login") == -1) {
                    jQuery('.overview-tabbar').html(jQuery(html).find('.overview-tabbar').html());
                    $dialog.hide();
                } else {
                    window.location = "/Account/LogOn";
                }

                return false;
            }
        );
    });

    //Rename table
    jQuery(document).on("click", ".toname.view a.rename-table", function () {
        var $parent = $(this).parent();

        if ($(this).attr("data-state") == "rename") {
            $parent.find('a.viewoptions').hide();
            $parent.find('input.editTableName').css("display", "block");
            $(this).hide();
            $parent.find(".rename-table[data-state='done']").show();
            $parent.find(".rename-table[data-state='cancel']").show();
        }
        else if ($(this).attr("data-state") == "done" || $(this).attr("data-state") == "cancel") {
            $parent.find('.editTableName').hide();
            $parent.find('a.viewoptions').show();
            $parent.find(".rename-table[data-state='done']").hide();
            $parent.find(".rename-table[data-state='cancel']").hide();
            $parent.find(".rename-table[data-state='rename']").show();
            if ($(this).attr("data-state") == "done") {
                var id = $parent.find("span").text();
                var name = $parent.find('input.editTableName').val();
                $(this).parent().find('a.viewoptions').text(name);
                jQuery.getJSON('/DataGrid/UpdateTableText?tableId=' + id + '&newName=' + name);
            }

        }
        if (window.console) console.log("Rename working(" + id + ", " + name + ")");
    });

    //Rename issue
    jQuery(document).on("click", ".reporttablename a.rename-issue", function () {
        var $parent = $(this).parent();

        if ($(this).attr("data-state") == "rename") {
            $(this).hide();
            $parent.find(".rename-issue[data-state='done']").show();
            $parent.find(".rename-issue[data-state='cancel']").show();
            $parent.find('span').hide();
            $parent.find('input').show();

        }
        else if ($(this).attr("data-state") == "done" || $(this).attr("data-state") == "cancel") {
            $parent.find(".rename-issue[data-state='done']").hide();
            $parent.find(".rename-issue[data-state='cancel']").hide();
            $parent.find(".rename-issue[data-state='rename']").show();
            $parent.find('span').show();
            $parent.find('input').hide();

            if ($(this).attr("data-state") == "done") {
                var id = $('.params.parameters #IssueId').val();
                var trn = $parent.find('.tr-number').val();
                var name = $parent.find('.edit-name').val();

                $parent.find('.last-name').val(name);
                $parent.find('span').html(trn + " " + name.toUpperCase());
                jQuery.getJSON('/IssueList/UpdateIssueText?issueId=' + id + '&newName=' + name);

                //Onereportparamset DIV element - Set top value attribute
                var height = $('.onereportparamset-header').outerHeight();
                $('.onereportparamset').css('top', height + "px");
            }
        }
    });

    //Dialogs for generating empty distinc table 
    jQuery(document).on("click", ".generate-table a", function () {
        var $link = jQuery(this);
        jQuery.get("/DataGrid/GenerateConfirmationDialog", function (html) {
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
                    jQuery.get($link.attr('href'), function (html) {
                        if (html.search("login") != -1) {
                            window.location = "/Account/LogOn";
                        }
                        var $dialog = jQuery('#dialog').html(html).show();
                        var key = $dialog.find('.key').text();
                        $dialog.find('.dialog-btn').click(function () {
                            delete Updater.Listeners[key];
                            jQuery('#dialog').hide();
                        });
                        Updater.Listeners[key] = function ($notification) {
                            location.href = $notification.find('a.link').attr('href');
                        };
                    });
                }
                return false;
            });
        });
        return false;
    });
    //Dialogs for generating table indexes
    jQuery(document).on("click", "#populateTableIndexes", function () {
        var $link = $(this).children('a');
        jQuery.get("/DataGrid/GenerateConfirmationDialogOverloaded",
        { dialogTitle: 'PopulateIndexesJob', dialogContent: 'PopulateIndexesJob' },
        function (html) {
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
                    jQuery.get($link.attr('href'), function (html) {
                        if (html.search("login") != -1) {
                            window.location = "/Account/LogOn";
                        }
                        var $dialog = jQuery('#dialog').html(html).show();
                        var key = $dialog.find('.key').text();
                        $dialog.find('.dialog-btn').click(function () {
                            delete Updater.Listeners[key];
                            jQuery('#dialog').hide();
                        });
                        Updater.Listeners[key] = function ($notification) {
                            location.href = $notification.find('a.link').attr('href');
                        };
                    });
                }
                return false;
            });
        });
        return false;
    });
    //Dialogs for archiving
    jQuery(document).on("click", ".toname .tryarchive-table a", function () {
        jQuery.get("/TableList/TryArchive", function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            jQuery('#dialog').html(html).show().find('.dialog-btn').click(function () {
                jQuery('#dialog').hide();
            });
            return false;
        });
    });
    // refresh TableObjects
    jQuery(document).on("click", "#listarchiveoptions .tabbar a.tabheader, #listarchiveoptions .submit ,#listarchiveoptions .pagination>.nav-controls .iconx, #listarchiveoptions .listarchiveoptionsselectall, #listarchiveoptions .listarchiveoptionsselectnone, #listarchiveoptions .sortbutton", function (e) {
        // default viewstate
        var tabHeader = jQuery(this).parents(".box").find(".tabheader.selected");
        var viewstate = jQuery(this).parents(".box .columns");
        var page = viewstate.attr('page');
        var size = viewstate.attr('size');
        var sortcolumn = viewstate.attr('sortcolumn');
        var direction = viewstate.attr('direction');
        var doselect = "";
        // this event modifies viewstate
        if (jQuery(this).hasClass('listarchiveoptionsselectall')) {
            // select all chkbox click event
            if (jQuery(this).hasClass('chkbox2on')) {
                doselect = "all";
            } else if (jQuery(this).hasClass('chkbox2off')) {
                doselect = "none";
            }
        } else if (jQuery(this).hasClass('sortbutton')) {
            // change sort click event
            var newsortcolumn = jQuery(this).attr('sortcolumn');
            if (sortcolumn == newsortcolumn) {
                if (jQuery(this).hasClass('Noscending')) direction = "Ascending";
                if (jQuery(this).hasClass('Ascending')) direction = "Descending";
                if (jQuery(this).hasClass('Descending')) direction = "Ascending";
            } else {
                sortcolumn = newsortcolumn;
                direction = "Ascending";
            }
        } else if (jQuery(this).hasClass('iconx')) {
            // pagination icon click event
            page = jQuery(this).attr("page");
            if (!page) return false;
        } else if (jQuery(this).hasClass('tabheader')) {
            // tab header click event
            if (jQuery(this).hasClass('selected')) return false;
            doselect = "none";
            tabHeader = jQuery(this);
        } else if (jQuery(this).hasClass('submit')) {
            // submit button click event
            e.stopPropagation();
        }
        //drop-down window
        var dropdown = tabHeader.parents(".dropdown");
        //search
        var searchElement = jQuery('input[name=search_viewoptions]', dropdown);
        var search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) search = '';
        var url = jQuery('.page-content')[0].id;
        var tableType = jQuery('.content-box').attr('tabletype');
        jQuery.get('/' + url + '/GetTableObjects/', { search: search, type: tableType, showEmpty: tabHeader.hasClass('showempty'), showHidden: tabHeader.hasClass('showhidden'), showArchived: tabHeader.hasClass('showarchived'), page: page, size: size, doselect: doselect, sortcolumn: sortcolumn, direction: direction }, function (html) {
            var newBlock = jQuery(html).find(".box");
            var oldBlock = jQuery(dropdown).find(".box");
            jQuery(oldBlock).html(jQuery(newBlock).html());
        });
        return false;
    });
    //one item selection
    jQuery(document).on("click", ".listarchiveoptionsselect", function () {
        var selectall = jQuery('.listarchiveoptionsselectall');
        var chkbox = jQuery(this);
        var chk = chkbox.hasClass('chkboxon');
        if (selectall.hasClass('') && selectall.hasClass('chkbox2off')) {
            doselect = "none";
            selectall.removeClass('chkbox2off').addClass('chkbox2on');
        }
        var url = jQuery('.page-content')[0].id;
        jQuery.get('/' + url + '/Select/', { id: chkbox.attr('uid'), chk: !chk }, function () {
            if (chk)
                chkbox.addClass('chkboxoff').removeClass('chkboxon');
            else
                chkbox.addClass('chkboxon').removeClass('chkboxoff');
        });
        return false;
    });
    // Archive / restore button
    jQuery(document).on("click", "#listarchiveoptions .submitbutton", function () {
        var tabHeader = jQuery(this).parents(".box").find(".tabheader.selected");
        var archive = 'Archive';
        if (tabHeader.hasClass('showarchived')) {
            archive = 'Restore';
        }
        var url = jQuery('.page-content')[0].id;
        var TableType = jQuery('.content-box').attr('tabletype');
        jQuery.get('/' + url + '/GetConfirmationDialog/', { archive: archive }, function (html) {
            var $dialog = jQuery('#dialog');
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog.html(html).show();
            $dialog.find('.dialog-btn').click(function () {
                var $button = jQuery(this);
                if ($button.find('.data').text() == 'False') {
                    jQuery('#dialog').hide();
                }
                if ($button.find('.data').text() == 'True') {
                    jQuery('#dialog').hide();
                    jQuery.get('/' + url + '/ArchiveSelected/', { type: TableType, archive: archive }, function (html) {
                        if (html.search("login") != -1) {
                            window.location = "/Account/LogOn";
                        }
                        var dialog = jQuery('#dialog').html(html).show();
                        var key = $dialog.find('.key').text();
                        dialog.find('.dialog-btn').click(function () {
                            delete Updater.Listeners[key];
                            jQuery('#dialog').hide();
                        });
                        Updater.Listeners[key] = function ($notification) {
                            location.href = $notification.find('a.link').attr('href');
                        };
                    });
                }
                return false;
            });
        });
        return false;
    });

    var doArchive = function (link) {
        jQuery.get(link, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }

            var $dialog = jQuery('#dialog');

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
    };

    jQuery(document).on("click", ".toparams2 .archive-table a, .toparams4 .archive-table a, .toname .archive-table a", function () {
        var $link = jQuery(this);
        var archiveType = $link.attr('href').substring($link.attr('href').indexOf('?') + 9, $link.attr('href').length);
        var id = $link.attr('href').substring($link.attr('href').lastIndexOf('/') + 1, $link.attr('href').indexOf('?'));
        jQuery.get("/TableList/GetConfirmationDialog", { archive: archiveType }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            var $dialog = jQuery('#dialog');
            $dialog.html(html).show().find('.dialog-btn').click(function () {
                $button = jQuery(this);
                if ($button.find('.data').text() == 'False') {
                    jQuery('#dialog').hide();
                }
                if ($button.find('.data').text() == 'True') {
                    jQuery('#dialog').hide();
                    $.getJSON("TableList/CheckEngineType", { id: id }, function (data) {
                        if (data.isInnoDB) {
                            $.get("TableList/GetInnoDBWarningDialog", function (partial) {
                                $dialog.html(partial).show();
                                $dialog.find(".dialog-btn").click(function () {
                                    if ($(this).find('.data').text() == 'True') {
                                        doArchive($link.attr('href'));
                                    }
                                    jQuery('#dialog').hide();
                                    return false;
                                });
                            });
                        } else {
                            doArchive($link.attr('href'));
                        }
                    });
                }
                return false;
            });
        });
        return false;
    });
    jQuery(document).on("click", ".toparams2 .repair-table a, .toparams4 .repair-table a, .toname .repair-table a", function () {
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
                var href = $notification.find('a.link').attr('href');
                if (typeof href !== 'undefined') {
                    alert("changing location");
                    location.href = href;
                }
                else {
                    jQuery('#dialog').hide();
                }
            };
        });
        return false;
    });
    jQuery(document).on("click", ".toparams .edit a, .reporttoparams .edit a", function () {
        var id = jQuery(this).prev().text();
        var showHidden = false;
        if (jQuery('.tabbar .selected').hasClass('show_hidden')) showHidden = true;
        jQuery.get('/IssueList/EditIssueDialog/', { id: id }, function (dialog_html) {
            if (dialog_html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $dialog = jQuery('#dialog').show().html(dialog_html);
            $outer_box = $dialog.find('.dialog-outer-box');
            $outer_box.css("margin-top", -$outer_box.height() / 2);
            $dialog.find('.dialog-btn').click(function () {
                $button = jQuery(this);
                if ($button.find('.data').text() == 'False') $dialog.hide();
                else if ($button.find('.data').text() == 'True') {
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
                        target: $dialog,
                        success: function () {
                            //Could change this, but then the edit method has to be changed
                            $dialog.hide();
                            window.location = "/IssueList/ShowOne/" + id + "?showHidden=" + showHidden;
                        }
                    });
                    $form.submit();
                }
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
        });
        return false;
    });
    var reportdelay = (function () {
        var timer = 0;
        return function (callback, ms) {
            clearTimeout(timer);
            timer = setTimeout(callback, ms);
        };
    })();
    var validate = (function (e) {
        var $target = jQuery(e.target);
        $target.parent().siblings('.report-parameter-error').hide();
        jQuery(document).trigger('click.dropdown');
        var $form = $('.parameter-table').parent();
        var action = $form.get(0).action;
        var issueId = action.substring(action.indexOf('ExecuteIssue/') + 13);
        var parid = $target.siblings('.item-id').text();
        //jQuery.get('/Datagrid/ValidateIssueParameterInput', { id: issueId, parameterId: parid, value: $target.val() }, function (html) {
        //    if (html != "") {
        //        if (html.toString().indexOf($target.val()) > 0) {
        //            if (html.search("login") != -1) {
        //                window.location = "/Account/LogOn";
        //            }
        //            $target.parent().siblings('.report-parameter-error').html(html);
        //            $target.parent().siblings('.report-parameter-error').show();
        //        }
        //    }
        //});
        getIssueRecordCount();
    });
    jQuery(document).on("keyup", "#IssueList .report-parameter-validate", function (e) {
        reportdelay(function () {
            validate(e);
        }, 1000);
    });
    jQuery(document).on("validate", "#IssueList .report-parameter-validate", function (e) {
        validate(e);
    });
    jQuery('ul.dropdown.small > li').hover(function () {
        if ($('#main-report-list .dirty').is(":focus")) {
            $('#main-report-list .dirty').attr('autocomplete', 'off').autocomplete();
            $('#main-report-list .dirty').attr('autocomplete', 'on').autocomplete();
        }
        $(this).find('> ul').show();
    }, function () {
        $(this).find('> ul').hide();
    });
    jQuery('ul.dropdown.small .close-popup').click(function () {
        $(this).parent().parent().hide();
    });

    jQuery(document).on("click", ".exactmatch a", function (e) {
        if ($(e.target).hasClass('checkboxon')) {
            $(e.target).removeClass('checkboxon');
            $(e.target).parent().parent().parent().find('#exactmatch').val('false');
        } else {
            $(e.target).addClass('checkboxon');
            $(e.target).parent().parent().parent().find('#exactmatch').val('true');
        }
        return false;
    });
    jQuery('#Documents input').trigger('focus');
    jQuery('#Documents input').trigger('focusout');
    jQuery(document).on("click", ".immediate-filter .exactmatch a", function (e) {
        $(e.target).siblings(".checkboxhidden")[0].value = $(e.target).hasClass('checkboxon');
        return false;
    });
    var ParameterGroupSave = {
        target: '#dialog',
        success: function () {
            var $dialog = jQuery('#dialog');
            $dialog.show().find('.dialog-btn').click(function () {
                jQuery('#dialog').hide();
            });
        }
    };
    /* Table and View */
    var showTransactionViewTableEdit = function () {
        if (jQuery(this).html().search("input") == -1) {
            var prevContent = jQuery(this).html();
            jQuery(this).html('<input id="value" type="text" size="14" value="' + prevContent + '" /><br><input type="button" value="Save" id="save" />');
        }
        jQuery(this).off("click");
    };
    jQuery(document).on("click", ".overview .tableobject .tono div #save", function () {
        var transactionCode = jQuery(this).parent().find("#value").val();
        var divContainer = jQuery(this).parent();
        var id = jQuery(this).parent().attr("data-tableid");
        var url = "/TableList/AddOrUpdateTransactionNumber?id=" + id + "&transactionNumber=" + transactionCode;
        jQuery.get(url, function () {
            divContainer.html(transactionCode);
        });
    });
    $(document).on("mouseover", ".overview .tableobject .tono div", function () {
        if (this.offsetWidth < this.scrollWidth) {
            var tx = jQuery(this).text();
            $(this).tooltip({
                content: tx,
                position: {
                    my: " top",
                    at: " top"
                }
            });
        }
    });
    jQuery(document).on("click", ".overview .tableobject .tono div", showTransactionViewTableEdit);
    /******************************************************************************/

    /* Report - favourites*/
    jQuery(document).on("click", "#destinationTable .tono #save, #sourceTable .tono #save", function () {
        var transactionCode = jQuery(this).parent().find("input").val();
        var id = jQuery(this).parent().attr("data-tableid");
        var url = "/TableList/AddOrUpdateTransactionNumber?id=" + id + "&transactionNumber=" + transactionCode;
        var divContainer = jQuery(this).parent();
        jQuery.get(url, function () {
            divContainer.html(transactionCode);
        });
    });
    jQuery(document).on("click", "#destinationTable .tono div, #sourceTable .tono div", function () {
        if (jQuery(this).html().search("input") == -1) {
            var prevContent = jQuery(this).parent().children().last().html();
            jQuery(this).parent().children().last().html('<input type="text" value="' + prevContent + '" /><input type="button" value="Save" id="save" />');
        }
    });
    /******************************************************************************/

    /* Pager Tables/Views */
    jQuery(document).on("keypress", "#pageCount", function (e) {
        if (e.keyCode == 13) {
            // search form
            var searchForm = $("#searchoverview").parent();
            searchForm.fadeOut();

            // navigation disabled
            var navigation = $(this).parent();
            navigation.find('a').each(function () {
                $(this).css('pointer-events', 'none');
            });

            jQuery('.tab-content').fadeOut(125, function () {
                jQuery("#loading").css('z-index', '10');
                jQuery('#loading').fadeIn();
            });
            //showEmpty
            var showEmpty = false;
            if (jQuery('.tabbar .selected').hasClass('show_empty')) showEmpty = true;
            //showHidden
            var showHidden = false;
            if (jQuery('.tabbar .selected').hasClass('show_hidden')) showHidden = true;
            //showArchived
            var showArchived = false;
            if (jQuery('.tabbar .selected').hasClass('show_archived')) showArchived = true;
            //search
            var searchElement = jQuery('#searchoverview input[type=text]');
            var maxCount = jQuery("#maxPagesCount").val();
            var page = $("#pageCount").val().replace(/[^0-9]/g, '');

            if (!page.match("[0-9]*")) {
                page = 0;
            }
            else {
                page = parseInt(page) - 1;
            }

            if (maxCount < page) {
                page = (maxCount);
            }

            if (page < 0) {
                page = 0;
            }


            var search = searchElement.val();
            var placeholder = searchElement.attr('placeholder');
            if (search == placeholder) search = '';
            //size
            var size = jQuery('.header>.nav-controls .size').text();
            //sort 
            var sortColumn = jQuery('.header>.nav-controls .sortColumn').text();
            var direction = jQuery('.header>.nav-controls .direction').text();

            jQuery.get(jQuery("#listUrl").val(), { json: true, size: size, sortColumn: sortColumn, direction: direction, catRefresh: true, showEmpty: showEmpty, showHidden: showHidden, showArchived: showArchived, page: page }, function (html) {
                jQuery('#loading').fadeOut(125, function () {
                    jQuery('.content-box').replaceWith(html, function () {
                        initDatePicker();
                        jQuery('.tab-content').fadeIn(125);
                        navigation.find('a').each(function () {
                            $(this).css('pointer-events', 'auto');
                        });
                        searchForm.fadeIn();
                    });
                    jQuery("#loading").css('z-index', '-1');
                });
            });
            history.pushState(null, "", '?page=' + (page).toString());
            return false;
        }
    });

    /* Pager Reports */
    jQuery(document).on("change", "#pageIssueCount", function () {
        jQuery('.main-table').fadeOut(500, function () {
            jQuery('.main-table').html('<div class="fill-loading"></div><div class="loading"></div>');
        });
        jQuery('.main-table').fadeIn(500);
        var searchElement = jQuery('#searchForm input[type=text]');
        var search = searchElement.val();
        var placeholder = searchElement.attr('placeholder');
        if (search == placeholder) search = '';
        var page = $(this).val();
        var maxCount = jQuery("#maxPagesCount").val();

        if (!page.match("[0-9]*")) {
            page = 0;
        }
        else {
            page = parseInt(page) - 1;
        }

        if (maxCount < page) {
            page = (maxCount);
        }

        if (page < 0) {
            page = 0;
        }

        var size = jQuery('.inner-container .nav-controls .size').text();
        var sortColumn = jQuery('.inner-container .nav-controls .sortColumn').text();
        var direction = jQuery('.inner-container .nav-controls .direction').text();
        jQuery.get('/IssueList/IndexList', { json: true, size: size, page: page, sortColumn: sortColumn, direction: direction, catRefresh: true, search: search, showEmpty: false, showHidden: false, showArchived: false, operation: 2 }, function (html) {
            jQuery('.main-table').fadeOut(500, function () {
                var inner = jQuery('.main-table', html);
                jQuery('.main-table').html(inner.html());
                addDragging();
            });
            jQuery('.main-table').fadeIn(500);
        });
        return false;
    });
    // a.last a.back a.first
    jQuery(document).on("click", "#mainnavigation a.next", function () {
        var pagecount = jQuery("#pageCount").val();
        window.history.pushState(null, "", '?page=' + (pagecount).toString());
        return false;
    });
    jQuery(document).on("click", "#mainnavigation a.back", function () {
        var pagecount = jQuery("#pageCount").val();
        pagecount = pagecount - 2;
        if (pagecount < 0) pagecount = 0;
        window.history.pushState(null, "", '?page=' + (pagecount).toString());
        return false;
    });

    jQuery(document).on("click", "#mainnavigation a.last", function () {
        window.history.pushState(null, "", '?page=' + jQuery("#maxPagesCount").val());
        return false;
    });
    jQuery(document).on("click", "#mainnavigation a.first", function () {
        window.history.pushState(null, "", '?page=0');
        return false;
    });

    jQuery('#saveParameterGroup').click(function () {
        var $form = jQuery('#execute_report');
        var arr = [];
        $form.find('input[type=text]').each(function () {
            arr.push($(this).val());
        });
        var action = $form.get(0).action;
        var issueId = action.substring(action.indexOf('ExecuteIssue/') + 13);
        jQuery.get("/IssueList/ShowParameterGroupSaveDialog", { issueId: issueId, parameterGroup: arr }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            var $dialog = jQuery('#dialog').show().html(html);
            var $dialogform = jQuery('#dialogform');
            $dialog.find('.dialog-btn').click(function () {
                $button = jQuery(this);
                if ($button.find('.data').text() == 'False') $dialog.hide();
                else if ($button.find('.data').text() == 'True') {
                    $country_code = $dialog.find('#dialogform .countryCode');
                    $dialog.find('#dialogform input[type=text]').each(function (i, e) {
                        jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                    });
                    $dialog.find('#dialogform input[type=hidden]').each(function (i, e) {
                        if (!jQuery(e).hasClass('countryCode')) {
                            jQuery(e).appendTo($dialogform);
                        }
                    });
                    $button.attr("disabled", "true");
                    $dialogform.ajaxSubmit(ParameterGroupSave);
                    $dialog.hide();
                    return false;
                }
            });
            $dialog.find('#dialogform select[name=language]').change(function () {
                var $country_code = $dialog.find('#dialogform .countryCode');
                var $select = jQuery(this);
                $dialog.find('#dialogform input[type=text]').each(function (i, e) {
                    jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                    jQuery(e).val($dialog.find('#dialogform .' + $select.val() + "_" + jQuery(e).attr('name')).val());
                });
                $country_code.val($select.val());
            });
        });
    });
});