jQuery(function () {
    var long_running_options;
    var refreshExtendedSortPartial = function (id) {
        var url = "/Command/RefreshExtendedSort";
        jQuery.get(url, { id: id }, function (html) {
            jQuery('#extsortoptions').replaceWith(html);
            viewoptions_loaded();
        });
    };
    var refreshExtendedSummaPartial = function (id) {
        var url = "/Command/RefreshExtendedSum";
        jQuery.get(url, { id: id }, function (html) {
            jQuery('#extsummaoptions').replaceWith(html);
            viewoptions_loaded();
        });
    };
    var refreshExtendedFilterPartial = function (id) {
        var url = "/Command/RefreshExtendedFilter";
        jQuery.get(url, { id: id }, function (html) {
            jQuery('#extfilteroptions').replaceWith(html);
            viewoptions_loaded();
            //timeout needed for treeview, the new filter has to be loaded.
            setTimeout(treeViewFunction, 2000);
        });
    };
    var refreshDataGridForColumnOrder = function (id) {
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
            viewoptions_loaded();
            datagrid_loaded();
            //table_dragdrop();
            vcolResizable();
            if (enlarge_viewoptions) jQuery('#viewoptions .dropdown-menu').removeClass('normal').addClass('viewoptions-enlarged');
            jQuery("#dialog").html('').hide();
        });
    };
    var scrollSave = function () {
        var previousScrollLeft = 0;
        jQuery("#DataGrid .table-scroll").scroll(function () {
            jQuery(document).trigger('click.extendedpanel');
            previousScrollLeft = $(this).scrollLeft();
        });
    };

    var viewoptions_loaded;
    var datagrid_loaded;
    var treeViewFunction;
    long_running_options = {
        target: '#dialog',
        beforeSubmit: function (data, $form) {
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
        jQuery(document).on("click", ".crud-data-column-val .checkb", function (e) {
            if ($(e.target).hasClass('checkboxon')) {
                $(e.target).removeClass('checkboxon');
                $(e.target).siblings('input').val('False');
            } else {
                $(e.target).addClass('checkboxon');
                $(e.target).siblings('input').val('True');
            }
        });
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
        jQuery(document).on("keyup", ".pagecount", function (e) {
            $(this).val($(this).val().replace(/[^0-9]/g, ''));
            var parsevalue = $('#pagecount').html().replace(/[\.,]/g, '').replace(/\s+/g, '').replace(/&nbsp;/g,'');
            var pagecount = parseInt(parsevalue);
            if ($(this).val() > pagecount) {
                $(this).val(pagecount);
            }
            if (($(this).val() != "") && $(this).val() < 1) {
                $(this).val(1);
            }
        });
        jQuery(document).on("keyup", ".rowcount#displayrowcount", function (e) {
            $(this).val($(this).val().replace(/[^0-9]/g, ''));
            if ($(this).val() > 100) {
                $(this).val(100);
            }
            if (($(this).val() != "") && $(this).val() < 1) {
                $(this).val(1);
            }
        });
        //jQuery('.rowcount .pagecount').keypress(function (e) {

        //jQuery("#DataGrid .header .backlink").click(function () {
        //    return goBack();
        //});
        //SORT
        //reads the scrolling position and sends the information to the SortAndFilter method
        jQuery('.datagrid .column-sort a.ascending, .datagrid .column-sort a.descending').click(function () {
            var newLink = $(this).attr('href');
            if (newLink.indexOf('&scrollPosition=') < 0 && newLink.indexOf('?scrollPosition=') < 0)
                newLink += '&scrollPosition=' + jQuery('#DataGrid .table-scroll').scrollLeft();
            $(this).attr('href', newLink);
        });
        //FILTER
        //reads the scrolling position and sends the information to the SortAndFilter method
        jQuery('.datagrid a.column-name').click(function () {
            jQuery('#data-grid-column-filter #scrollPosition').val(jQuery('#DataGrid .table-scroll').scrollLeft());
        });
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

        jQuery(document).on("click" ,"#data-grid-column-filter .exactmatch a", function (e) {
            jQuery(document).trigger('click.filterpanel');
            jQuery(document).trigger('click.excelfilterpanel');
        });
     
        treeViewFunction();
        scrollSave();
        jQuery('#tabfunctions .table-join form, #tabfunctions .table-group form, #data-grid-column-filter form').ajaxForm(long_running_options);
    })(/* execute datagrid_loaded */);


    /**************************************************************************
     * Reset column order
     **************************************************************************/
    jQuery(document).on("click", "#column-order .order-false", function () {
        var $dialog = jQuery('#dialog').show().html(jQuery('#wait-dlg').html());
        var size = jQuery('.nav-controls .size').text();
        jQuery.get("/DataGrid/ResetColumnOrder", {
            id: jQuery('#column-order .table-id').text(),
            start: jQuery('.from').text() - 1,
            useEnlargeProperty: jQuery('#DataGrid').hasClass('enlarged'),
            json: true, size: size
        }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            jQuery('#DataGrid').html(html);
            $("#viewoptions .icon").click();
            $("#viewoptions .dropdown-menu .column-view-props .tabbar a").last().click();
            var replace = jQuery('#DataGrid .page-header').clone();
            jQuery('#DataGrid .page-header').removeClass("replace");
            replace.insertAfter(jQuery('#DataGrid .page-header'));
            vcolResizable();
            $dialog.hide();
            viewoptions_loaded();
        });
        return false;
    });

    /**************************************************************************
        * DRAG & DROP COLUMN ORDER
        **************************************************************************/

    jQuery(document).on("sortstop", "#viewoptions .column-order-content .sortable-columns", function (e, ui) {
        var $dialog = jQuery('#dialog').show().html(jQuery('#wait-dlg').html());
        var columnOrder = "";
        if (jQuery('#viewoptions .dropdown-menu').hasClass('normal')) {
            jQuery("#viewoptions .sortable-columns.normal .column .item-id").each(function (i, el) {
                if (columnOrder.length > 0) columnOrder += ",";
                var id = jQuery(el).text();
                columnOrder += id;
            });
        } else {
            jQuery("#viewoptions .sortable-columns.first .column .item-id").each(function (i, el) {
                if (columnOrder.length > 0) columnOrder += ",";
                var id = jQuery(el).text();
                columnOrder += id;
            });
            jQuery("#viewoptions .sortable-columns.second .column .item-id").each(function (i, el) {
                if (columnOrder.length > 0) columnOrder += ",";
                var id = jQuery(el).text();
                columnOrder += id;
            });
        }
        var table_id = jQuery('#column-order .table-id').text();
        var column_id = ui.item.find('.item-id').text();
        jQuery.get("/DataGrid/UpdateColumnOrder", { id: table_id, columnId: column_id, columnOrder: columnOrder, refreshDataGrid: false }, function (html) {
            refreshDataGridForColumnOrder(table_id);
        }).done(function () {
            viewoptions_loaded();
        });

    });
    function setTableBody() {
        //!!Important: If you need to modify this code, please modify this function in the viewbox.js file.
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
    (viewoptions_loaded = function () {
        $("#viewoptions .column-order-content .sortable-columns").sortable();
        $("#viewoptions .column-order-content .sortable-columns").sortable("option", "distance", 5);
        $("#viewoptions .column-order-content .sortable-columns").sortable("option", "scroll", true);
        $("#viewoptions .column-order-content .sortable-columns").sortable("option", "containment", "#column-order");
        $("#viewoptions .column-order-content .sortable-columns").sortable("option", "placeholder", "drag-helper");
        $("#viewoptions .column-order-content .sortable-columns").sortable("option", "connectWith", ".column-order-content .sortable-columns");
        jQuery("document").off("click", ".operator .delete").on("click", ".operator .delete", function () {
            jQuery(this).parent().parent().remove();
            reload_filter_view();
        });
        setTableBody();
        //Dropdown for and/or selection
        jQuery('.and_or_select').unbind('change').change(function () {
            jQuery(this).parent().prevAll('span.op-command:first').html(jQuery(this).find('option:selected').val());
            reload_filter_view();
        });
        // DRAGABLE (FILTER)
        jQuery(".filter .operators .operator").draggable({
            helper: 'clone',
            containment: '#extfilter',
            revert: 'invalid',
            appendTo: '#extfilter'
        }).disableSelection();
        var drop_options = {
            accept: '.operator',
            activeClass: "op-dock-hover",
            tolerance: 'pointer', //default: intersect
            drop: function (e, ui) {
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
            helper: 'clone',
            containment: '#extfilter',
            revert: 'invalid',
            appendTo: '#extfilter'
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
                $(columns[i]).appendTo($('.sortable-columns1 .bot-sort tbody '));
                $(columns[i]).append("<td class='to-sort'></td>");
                $(columns[i]).find('td.remove-attr').remove();
                $($(columns[i]).find(".from-sort")).remove()
            }
            $(".sort .sorting-columns .placeholder").fadeIn(500);
        });
        //RESET SUMMA
        index = 0;
        jQuery('#resetsumma').unbind("click").click(function () {
            var columns = $('.summing-columns .droparea .top-sum tbody tr');
            for (var i = 0; i < columns.length; i++) {
                index = 0;
                var summable_body = jQuery('.summable-columns1 .bot-sum tbody');
                var shs = $(columns[i]).find('td.from-sum');
                var obj = shs.parent();
                shs.attr('class', 'to-sum');
                var objNumber = parseInt(obj.find(".colnumber").text());
                summable_body.find("tr").each(function () {
                    var number = parseInt($(this).find(".colnumber").text());
                    if ((objNumber < number) && index == 0) {
                        $(this).before(obj);
                        index = 1;
                    }
                });
                if (index == 0) {
                    jQuery('.summable-columns1 .bot-sum tbody').append(shs.parent());
                }
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
        jQuery('#extfilter .buttons form, #extsort .buttons form, #extsumma .buttons form, #subtotaloptions .buttons form, .multipleOptimization form').ajaxForm(long_running_options);
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

            var url = jQuery(this).parent('form').get(0).action.split('?')[0] + '?';
            jQuery(this).parent('form').get(0).action = url + groupColumn + subtotalColumns + onlyExpectedResult;
        });

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
        /**************************************************************************
        * COLUMN SORT
        **************************************************************************/
        //jQuery(".summa .buttons .submit").unbind("click").click(function () {
        jQuery(document).off("click", ".summa .buttons .submit").on("click", ".summa .buttons .submit", function () {
            var summa = '';
            jQuery(".summa .droparea .top-sum tbody tr").each(function (i, el) {
                if (jQuery(el).find('td.item-id').text() != '')
                    summa += '&summaList[' + i + ']=' + jQuery(el).find('td.item-id').text();
            });
            jQuery(this).parent('form').get(0).action += summa;
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
            //console.log("The number of elements: " + numberOfElements);
            // at least one element should exist
            if (numberOfElements > 1) {
                jQuery(this).parent().remove();
            }
        });
        jQuery(".filter .buttons .submit").unbind("click").click(function () {
            $.each(jQuery(".filter-tree .droparea .value .placeholder"), function (key, thiz) {
                console.log("1: " + jQuery(thiz).val());
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
                                $country_code = $dialog.find('form .countryCode');
                                $dialog.find('form input[type=text]').each(function (i, e) {
                                    jQuery('.' + $country_code.val() + '_' + jQuery(e).attr('name')).val(jQuery(e).val());
                                });
                                $dialog.find('form input[type=hidden]').each(function (i, e) {
                                    if (!jQuery(e).hasClass('countryCode')) {
                                        jQuery(e).appendTo($form);
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
    jQuery(document).on("submit", "#searchoverview-documents", function () {
        var $form = jQuery(this);
        var $field = $form.find('.field');
        var value = $field.hasClass('empty') ? '' : $field.val();
        jQuery.get($form.attr("action"), { search: value }, function (html) {
            jQuery('.overview').replaceWith(html);
        });
        return false;
    });
    jQuery(document).on("keyup", "#searchoverview-documents input[type=text]", function () {
        jQuery.get(jQuery(this).parent("form").attr("action"), { search: jQuery(this).val() }, function (html) {
            jQuery('.overview').replaceWith(html);
        });
    });
    jQuery("#s").on("focus", function () {
        jQuery(this).animate({ width: '+=80' });
    }).blur(function () {
        jQuery(this).animate({ width: '-=80' });
    });
    jQuery(document).on("focus", ".search-panel input[type=text]", function () {
        jQuery(this).parents('.search-panel').addClass('focus');
    }).on("focusout", ".search-panel input[type=text]", function () {
        jQuery(this).parents('.search-panel').removeClass('focus');
    });
    jQuery(document).on("submit", "#searchoverview", function () {
        // search form disabled
        var searchForm = $("#searchoverview").parent();
        $("#searchoverview .field.bg-middle").attr("disabled", "disabled");

        // navigation disabled
        var navigation = $("#mainnavigation");
        navigation.find('a').each(function () {
            $(this).css('pointer-events', 'none');
        });
        $('.tab-content').fadeOut(125, function () {
            $('#loading').css('z-index', 10);
            $('#loading').fadeIn();            
        });
        var $form = jQuery(this);
        var $field = $form.find('.field');
        var value = null;
        if($field.val() != "" && $field.val() != $field.attr('placeholder') /* ie 10 */)
        {
            value = $field.val();
            searchForm.find(".field").val(value);
        }
        else
        {
            searchForm.find(".field").val("");
        }
        
        var tabheader = jQuery('.tabbar .selected');
        var showEmpty = tabheader.hasClass('show_empty');
        var showHidden = tabheader.hasClass('show_hidden');
        var showArchived = tabheader.hasClass('show_archived');
        var sortColumn = jQuery('.nav-controls .sort-column').text();
        var direction = jQuery('.nav-controls .direction').text();
        var link = location.href.substring(0, location.href.indexOf('?'));
        setTimeout(function () {
            jQuery.get(link, { json: true, search: value, showEmpty: showEmpty, showArchived: showArchived, showHidden: showHidden, sortColumn: sortColumn, direction: direction, catRefresh: false }, function (html) {
                if (html.search("login") != -1) {
                    window.location = "/Account/LogOn";
                }
                        var $tbody = jQuery('table.overview tbody').html(html);
                        var replace = $tbody.find('.replace');
                jQuery('#mainnavigation.nav-controls').html(replace.find('.nav-controls').html());
                        jQuery('.overview-tabbar').html(replace.find('.tabbar').html());
                        $('#loading').fadeOut(500, function () {
                                $('#loading').css('z-index', '-10');
                                $('.tab-content').fadeIn(500);
                            });
                        navigation.find('a').each(function () {
                                $(this).css('pointer-events', 'auto');
                            });
                        $("#searchoverview .field.bg-middle").removeAttr("disabled");
                    });
        }, 1000);
        return false;
    });
    $(window).unload(function () {
        if (('localStorage' in window) && window['localStorage'] != null) {
            var form = $("#myTable").html();
            localStorage.setItem('myTable', form);
        }
    });

    var searchInitDatePicker;
    (searchInitDatePicker = function () {
        var currentYearRange = jQuery("#currentYearRange").val();
        jQuery('.field.reportdate').datepicker({
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
    });
    //delay the search function so that not every click searches the database
    var keyUpFunction = (function () {
        $('.tab-content').fadeOut(125, function () {
            $('#loading').css('z-index', 10);
            $('#loading').fadeIn(125);
        });
        var $input = jQuery('#searchoverview input[type=text]');
        var value = $input.hasClass('empty') ? '' : $input.val();
        var tabheader = jQuery('.tabbar .selected');
        var showEmpty = tabheader.hasClass('show_empty');
        var showHidden = tabheader.hasClass('show_hidden');
        var showArchived = tabheader.hasClass('show_archived');
        var sortColumn = jQuery('.header .nav-controls .sortColumn').text();
        var direction = jQuery('.header .nav-controls .direction').text();
        var link = location.href.substring(0, location.href.indexOf('?'));
        setTimeout(function () {
            jQuery.get(link, { json: true, search: value, showEmpty: showEmpty, showHidden: showHidden, showArchived: showArchived, sortColumn: sortColumn, direction: direction, catRefresh: false },
                function(html) {
                    if (html.search("login") != -1) {
                        window.location = "/Account/LogOn";
                    }

                    var pdfexportOverview = document.body.querySelector('.icon.pdfexport-overview');
                    var href = pdfexportOverview.getAttribute("href");
                    var hrefsplitQuestionMark = href.split("?")[1];
                    var hrefsplitAndMark = hrefsplitQuestionMark.split("&");
                    var ered = "";
                    var searchadded = false;
                    for (var i = 0, len = hrefsplitAndMark.length; i < len; i++) {
                        var hrefspitQuery = hrefsplitAndMark[i].split("=");
                        if (hrefspitQuery[0] === "search") {
                            hrefspitQuery[1] = value;
                            searchadded = true;
                        }
                        if (i === (length - 1)) {
                            ered += hrefspitQuery[0] + "=" + hrefspitQuery[1];
                        } else {
                            ered += hrefspitQuery[0] + "=" + hrefspitQuery[1] + "&";
                        }
                    }
                    if (!searchadded) {
                        ered += "&search=" + value;
                    }
                    ered = href.split("?")[0] + "?" + ered;
                    pdfexportOverview.setAttribute("href", ered);

                    var $tbody = jQuery('table.overview tbody').html(html);
                    var replace = $tbody.find('.replace');
                    jQuery('.nav-controls').html(replace.find('.nav-controls').html());
                    jQuery('.overview-tabbar').html(replace.find('.tabbar').html());
                    searchInitDatePicker();
                    $('#loading').fadeOut(125, function () {
                        $('.tab-content').fadeIn(125);
                    });
                }
            );
        }, 1000);
        return false;
    });
    var delay = (function () {
        var timer = 0;
        return function (callback, ms) {
            clearTimeout(timer);
            timer = setTimeout(callback, ms);
        };
    })();
    //jQuery(document).on("keyup", "#searchoverview input[type=text]", function () {
    //    delay(function () {
    //        keyUpFunction();
    //    }, 1000);
    //});
    jQuery(document).on("submit", "#searchoverview-jobs", function () {
        var $form = jQuery(this);
        var $field = $form.find('.field');
        var value = $field.hasClass('empty') ? '' : $field.val();
        jQuery.get("/ControlCenter/RefreshJobs", { search: value }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            jQuery('#ControlCenter .content-box .content-inner.content-right .overview.jobs').html(html);
        });
        return false;
    });
    jQuery(document).on("keyup", "#searchoverview-jobs input[type=text]", function () {
        jQuery.get("/ControlCenter/RefreshJobs", { search: jQuery(this).val() }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            jQuery('#ControlCenter .content-box .content-inner.content-right .overview.jobs').html(html);
        });
        return false;
    });
    jQuery(document).on("submit", "#searchoverview-opt", function () {
        var $form = jQuery(this);
        var $field = $form.find('.field');
        var value = $field.hasClass('empty') ? '' : $field.val();
        var optGroupId = $form.find('input[type=hidden]').val();
        var returnUrl = $form.prev('input[type=hidden]').val();
        jQuery.get("/Command/SearchOptimization", { search: value, optGroupId: optGroupId, returnUrl: returnUrl }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $form.parents('.search-opt-panel').next('.optimizations').html(html);
            var opt_count = jQuery(html).next('.opt_count').first();
            var count_text = opt_count.text();
            if (!opt_count.length) count_text = 0;
            $form.find('.opt_count .count-middle span').text(count_text);
        });
        return false;
    });
    jQuery(document).on("change", "#searchoverview-opt input[type=text]", function () {
        var $input = jQuery(this);
        var optGroupId = $input.prev('input[type=hidden]').val();
        var returnUrl = $input.parents('form').prev('input[type=hidden]').val();
        jQuery.get("/Command/SearchOptimization", { search: jQuery(this).val(), optGroupId: optGroupId, returnUrl: returnUrl }, function (html) {
            if (html.search("login") != -1) {
                window.location = "/Account/LogOn";
            }
            $input.parents('.search-opt-panel').next('.optimizations').html(html);
            var opt_count = jQuery(html).next('.opt_count').first();
            var count_text = opt_count.text();
            if (!opt_count.length) count_text = 0;
            $input.parent().find('.opt_count .count-middle span').text(count_text);
        });
    });
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
            viewoptions_loaded();
            datagrid_loaded();
            //table_dragdrop();
            vcolResizable();
            if (enlarge_viewoptions) jQuery('#viewoptions .dropdown-menu').removeClass('normal').addClass('viewoptions-enlarged');
            jQuery("#dialog").html('').hide();
        });
    };

    jQuery(document).on("submit", "#searchoverview-viewoptions", function (e) {
        e.stopPropagation();
        var $form = jQuery(this);
        var $field = $form.find('.field');
        var value = $field.hasClass('empty') ? '' : $field.val();
        if (value != '') {
            if (jQuery('.column-order').hasClass('selected')) {
                jQuery('.tabheader.visible').click();
            }
            jQuery('.column-order').hide();
        } else {
            jQuery('.column-order').show();
        }
        jQuery.get("/DataGrid/SearchColumns", { id: $form.find('.table-object-id').val(), search: value, partial: '_ViewOptionsPartial' }, function (html) {
            var newBlock = jQuery(html).find('ul.dropdown-menu .box').children();
            var oldBlock = jQuery('#viewoptions ul.dropdown-menu .box').children();
            jQuery(oldBlock[1]).html(jQuery(newBlock[1]).html());
            jQuery(oldBlock[2]).html(jQuery(newBlock[2]).html());
            jQuery(oldBlock[3]).html(jQuery(newBlock[3]).html());
            viewoptions_loaded();
        });
        return false;
    });
    jQuery(document).on("submit", "#searchoverview-extsubtotaloptions", function (e) {
        e.stopPropagation();
        var $form = jQuery(this);
        var $field = $form.find('.field');
        var value = $field.hasClass('empty') ? '' : $field.val();

        var alreadyAdded = "";
        jQuery(".subtotaling-columns tr").each(function (i, el) {
            if (jQuery(el).find('td.item-id').text() != '')
                alreadyAdded += jQuery(el).find('td.item-id').text() + '&';
        });

        jQuery.get("/DataGrid/SearchColumns", { id: $form.find('.table-object-id').val(), search: value, partial: '_SubtotalPartial', addedColumns: alreadyAdded }, function (html) {
            var newBlock = jQuery(html).find('ul.dropdown-menu .subtotal').children();
            var oldBlock = jQuery('#subtotaloptions ul.dropdown-menu .subtotal').children();
            jQuery(oldBlock[1]).html(jQuery(newBlock[1]).html());
            jQuery("button").button();
            if (jQuery("#selected-group-column tr").length > 3) {
                jQuery("button.groupcolumn").button("option", "disabled", true);
            }
            if (jQuery("#selected-subtotal-column tr").length > 6) {
                jQuery("button.subtotalcolumn").button("option", "disabled", true);
            }
            jQuery("button.subtotalcolumn.disabled").button("option", "disabled", true);
            jQuery("button.del").button({ text: false, icons: { primary: "ui-icon-closethick white del" } });
            viewoptions_loaded();
        });
        return false;
    });
    jQuery(document).on("submit", "#searchoverview-extsortoptions", function (e) {
        e.stopPropagation();
        var $form = jQuery(this);
        var $field = $form.find('.field');
        var value = $field.hasClass('empty') ? '' : $field.val();

        var alreadyAdded = "";
        jQuery("table.top-sort tr").each(function (i, el) {
            if (jQuery(el).find('td.item-id').text() != '')
                alreadyAdded += jQuery(el).find('td.item-id').text() + '&';
        });

        jQuery.get("/DataGrid/SearchColumns", { id: $form.find('.table-object-id').val(), search: value, partial: '_ExtendedSortPartial', addedColumns: alreadyAdded}, function (html) {
            var newBlock = jQuery(html).find('ul.dropdown-menu .sort').children();
            var oldBlock = jQuery('#extsortoptions ul.dropdown-menu .sort').children();
            jQuery(oldBlock[1]).html(jQuery(newBlock[1]).html());
            viewoptions_loaded();
        });
        return false;
    });
    jQuery(document).on("submit", "#searchoverview-extsummaoptions", function (e) {
        e.stopPropagation();
        var $form = jQuery(this);
        var $field = $form.find('.field');
        var value = $field.hasClass('empty') ? '' : $field.val();

        var $input = jQuery(this);
        var alreadyAdded = "";
        jQuery(".summing-columns tr").each(function (i, el) {
            if (jQuery(el).find('td.item-id').text() != '')
                alreadyAdded += jQuery(el).find('td.item-id').text() + '&';
        });

        jQuery.get("/DataGrid/SearchColumns", { id: $form.find('.table-object-id').val(), search: value, partial: '_SummaPartial', addedColumns: alreadyAdded}, function (html) {
            var newBlock = jQuery(html).find('ul.dropdown-menu .summa').children();
            var oldBlock = jQuery('#extsummaoptions ul.dropdown-menu .summa').children();
            jQuery(oldBlock[1]).html(jQuery(newBlock[1]).html());
            viewoptions_loaded();
        });
        return false;
    });
    jQuery(document).on("change", "#searchoverview-extsubtotaloptions input[type=text]", function () {
        var $input = jQuery(this);
        var alreadyAdded = "";
        jQuery(".subtotaling-columns tr").each(function (i, el) {
            if (jQuery(el).find('td.item-id').text() != '')
                alreadyAdded += jQuery(el).find('td.item-id').text() + '&';
        });

        jQuery.get("/DataGrid/SearchColumns", { id: $input.next('.table-object-id').val(), search: $input.val(), partial: '_SubtotalPartial', addedColumns: alreadyAdded }, function (html) {
            var newBlock = jQuery(html).find('ul.dropdown-menu .subtotal').children();
            var oldBlock = jQuery('#subtotaloptions ul.dropdown-menu .subtotal').children();
            jQuery(oldBlock[1]).html(jQuery(newBlock[1]).html());
            jQuery("button").button();
            if (jQuery("#selected-group-column tr").length > 3) {
                jQuery("button.groupcolumn").button("option", "disabled", true);
            }
            if (jQuery("#selected-subtotal-column tr").length > 6) {
                jQuery("button.subtotalcolumn").button("option", "disabled", true);
            }
            jQuery("button.subtotalcolumn.disabled").button("option", "disabled", true);
            jQuery("button.del").button({ text: false, icons: { primary: "ui-icon-closethick white del" } });
            viewoptions_loaded();
        });
    });
    jQuery(document).on("change", "#searchoverview-extsortoptions input[type=text]", function () {
        var $input = jQuery(this);
        var alreadyAdded = "";
        jQuery("table.top-sort tr").each(function (i, el) {
            if (jQuery(el).find('td.item-id').text() != '')
                alreadyAdded += jQuery(el).find('td.item-id').text() + '&';
        });

        jQuery.get("/DataGrid/SearchColumns", { id: $input.next('.table-object-id').val(), search: $input.val(), partial: '_ExtendedSortPartial', addedColumns: alreadyAdded}, function (html) {
            var newBlock = jQuery(html).find('ul.dropdown-menu .sort').children();
            var oldBlock = jQuery('#extsortoptions ul.dropdown-menu .sort').children();
            jQuery(oldBlock[1]).html(jQuery(newBlock[1]).html());
            viewoptions_loaded();
        });
    });
    jQuery(document).on("change", "#searchoverview-extsummaoptions input[type=text]", function () {
        var $input = jQuery(this);
        var alreadyAdded = "";
        jQuery(".summing-columns tr").each(function (i, el) {
            if (jQuery(el).find('td.item-id').text() != '')
                alreadyAdded += jQuery(el).find('td.item-id').text() + '&';
        });
        jQuery.get("/DataGrid/SearchColumns", { id: $input.next('.table-object-id').val(), search: $input.val(), partial: '_SummaPartial', addedColumns: alreadyAdded}, function (html) {
            var newBlock = jQuery(html).find('ul.dropdown-menu .summa').children();
            var oldBlock = jQuery('#extsummaoptions ul.dropdown-menu .summa').children();
            jQuery(oldBlock[1]).html(jQuery(newBlock[1]).html());
            viewoptions_loaded();
        });
    });
    jQuery('#ArchiveDocuments input').each(function () {
        var el = jQuery(this);
        if (el.val() == "")
            el.val(el.attr('placeholder'));
    });
    jQuery(document).on("drop", ".placeholder", function (event) {
        event.preventDefault();
        event.stopPropagation();
        var $this = jQuery(this);
        if (event.originalEvent.dataTransfer != undefined) {
        var type = (event.originalEvent.dataTransfer.hasOwnProperty('types')) ? event.originalEvent.dataTransfer.types[0] : "Text";
        var value = event.originalEvent.dataTransfer.getData(type);

        if ($this.is('input')) {
            if ($this.val() == $this.attr('placeholder')) $this.val(value);
            else $this.val($this.val() + value);
        } else {
            if ($this.text() == $this.attr('placeholder')) $this.text(value);
            else $this.text($this.text() + value);
        }
        $this.removeClass('empty').select();
        }
    });
    jQuery(document).on("focus", ".placeholder", function () {
        var $this = jQuery(this);
        if ($this.is('input')) {
            if ($this.val() == $this.attr('placeholder')) $this.val('');
        } else {
            if ($this.text() == $this.attr('placeholder')) $this.text('');
        }
        $this.removeClass('empty').select();
    }).on("blur", ".placeholder", function () {
        var $this = jQuery(this);
        if ($this.is('input')) {
            if ($this.val() == '') {
                $this.val($this.attr('placeholder'));
                $this.addClass('empty').removeClass('dirty');
            } else {
                $this.removeClass('empty').addClass('dirty');
            }
        } else {
            if ($this.text() == '') {
                $this.text($this.attr('placeholder'));
                $this.addClass('empty').removeClass('dirty');
            } else {
                $this.removeClass('empty').addClass('dirty');
            }
        }
    }).each(function (i, el) {
        var $el = jQuery(el);
        if ($el.is('input')) {
            if ($el.val() == $el.attr('placeholder')) {
                $el.addClass('empty').removeClass('dirty');
            } else {
                $el.removeClass('empty').addClass('dirty');
            }
        } else {
            if ($el.text() == $el.attr('placeholder')) {
                $el.addClass('empty').removeClass('dirty');
            } else {
                $el.removeClass('empty').addClass('dirty');
            }
        }
    });
});