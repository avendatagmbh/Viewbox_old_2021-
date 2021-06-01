jQuery(function () {
    columnInfos = {};
    /**************************************************************************
    * COLUMN SUBTOTAL
    **************************************************************************/
    jQuery("button").button();
    if (jQuery("#selected-group-column tr").length > 3) {
        jQuery("button.groupcolumn").button("option", "disabled", true);
    }
    if (jQuery("#selected-subtotal-column tr").length > 19) { //6
        jQuery("button.subtotalcolumn").button("option", "disabled", true);
    }
    jQuery("button.subtotalcolumn.disabled").button("option", "disabled", true);
    jQuery("button.del").button({ text: false, icons: { primary: "ui-icon-closethick white del" } });

    jQuery(document).on("click", ".subtotal .groupcolumn", function () {
        var $btn = jQuery(this);
        var $td = $btn.parent();
        var $row = $btn.closest("tr");
        $td.find("button").toggle();
        $row.appendTo("#selected-group-column");
        if (jQuery("#selected-group-column tr").length > 3)
        {
            jQuery("#subtotaloptions .groupcolumn").button("option", "disabled", true);
        }
    });

    jQuery(document).on("click", ".subtotal .subtotalcolumn", function () {
        var $btn = jQuery(this);
        var $td = $btn.parent();
        var $row = $btn.closest("tr");
        if (jQuery("#selected-subtotal-column tr").length > 18) {
            $(".subtotal .subtotalcolumn").button("option", "disabled", true);
        }
        $td.find("button").toggle();
        $row.appendTo("#selected-subtotal-column");
    });

    var subtotalColumnDelete = function () {
        var $btn = jQuery(this);
        var $td = $btn.parent();
        var $row = $btn.closest("tr");
        var colnumber = $row.find(".colnumber").text();
        var lastItem;
        var isInserted = false;

        jQuery(".subtotalable-columns tr").each(function (index, item) {
            var itemColnumber = $(item).find(".colnumber").text();
            if (Number(itemColnumber) > Number(colnumber)) {
                $td.find("button").toggle();
                if ($row.closest(".subtotal-target").prev().hasClass("group-target-header")) {
                    $(".subtotal .groupcolumn").button("option", "disabled", false);
                }
                else {
                    if (jQuery("#selected-subtotal-column").children().length < 8) {
                        $(".subtotal .subtotalcolumn").not(".disabled").button("option", "disabled", false);
                    }
                }
                $row.insertBefore(item);
                isInserted = true;
                return false;
            }
            lastItem = item;
        });

        if (!isInserted) {
            $td.find("button").toggle();
            if ($row.closest(".subtotal-target").prev().hasClass("group-target-header")) {
                $(".subtotal .groupcolumn").button("option", "disabled", false);
            }
            else {
                if (jQuery("#selected-subtotal-column").children().length < 8) {
                    $(".subtotal .subtotalcolumn").not(".disabled").button("option", "disabled", true);
                }
            }
            $row.insertAfter(lastItem);
        }
    }

    jQuery(document).on("click", "#subtotaloptions .del", subtotalColumnDelete);

    /**************************************************************************
    * COLUMN SUM
    * COLUMN VISIBILITY
    **************************************************************************/
    ind = true;
    before = false;
    index = 0;
    jQuery(document).on("click", "td.to-sum", function () {
        jQuery('#resetsumma').attr('disabled', 'true');
        function complete()
        {
          jQuery('.summing-columns .top-sum tbody').append(tr);
          jQuery('.summable-columns1 .bot-sum tbody tr .to-disab').attr('class', 'to-sum');
          jQuery('#resetsumma').removeAttr('disabled');
        }
        var $btn = jQuery(this), $row = $btn.parents('tr').first();
        if($btn.hasClass("disabled") == false)
        {
            var tr = $row.remove().clone();
            var summing_body = jQuery('.summing-columns .top-sum tbody tr');
            var shs = tr.find('td.to-sum');
            shs.attr('class', 'from-sum');
            if (summing_body.length == 0) {
                jQuery('.summable-columns1 .bot-sum tbody tr .to-sum[class!="to-sum disabled"]').attr('class', 'to-disab');
                jQuery('.summing-columns .placeholder').fadeOut(500, complete);
                
            }
            else
            {
                index = 0;
                var obj = $(this).parent();
                obj.find(".to-sum").attr('class', 'from-sum');
                jQuery('.summable-columns1 .bot-sum tbody tr .to-sum[class!="to-sum disabled"]').attr('class', 'to-disab');
                var objNumber = parseInt($(this).parent().find(".colnumber").text());
                jQuery('.summing-columns .top-sum tbody tr').each(function () {
                    var number = parseInt($(this).find(".colnumber").text());
                    if (objNumber < number && index == 0) {
                        $(this).before(obj);
                        index = 1;
                        jQuery('#resetsumma').removeAttr('disabled');
                    }
                });
                if (index == 0)
                {
                    complete();
                }
            }
            if (index != 0)
            {
                jQuery('.summable-columns1 .bot-sum tbody tr .to-disab').attr('class', 'to-sum');
            }
        }
       
   });		 

    var sumColumnDelete = function () {
        jQuery('.summable-columns1 .bot-sum tbody tr .to-disab').attr('class', 'to-sum');
        var $btn = jQuery(this), $row = $btn.parents('tr').first();
        var tr = $row.remove().clone();
        var summable_body = jQuery('.summable-columns1 .bot-sum tbody');
        var shs = tr.find('td.from-sum')
        shs.attr('class', 'to-sum');
        var index = 0;
        var obj = $(this).parent();
        obj.find(".from-sum").attr('class', 'to-sum');
        var objNumber = parseInt($(this).parent().find(".colnumber").text());
        summable_body.find("tr").each(function () {
            var number = parseInt($(this).find(".colnumber").text());
            if (objNumber < number && index == 0) {
                $(this).before(obj);
                index = 1;
            }
        });
        if (index == 0) {
            jQuery('.summable-columns1 .bot-sum tbody').append(tr);
        }
        var summing_body = jQuery('.summing-columns .top-sum tbody tr');
        if (summing_body.length < 1)
            jQuery('.summing-columns .placeholder').fadeIn(500);
    }

    jQuery(document).on("click", "td.from-sum", sumColumnDelete);

    
    /**************************************************************************
    * COLUMN SORT
    **************************************************************************/
    jQuery(document).on("click", "td.to-sort", function () {
        var $btn = jQuery(this), $row = $btn.parents('tr').first();
        var tr = $row.remove().clone();
        tr.find(".colname").append("<div class='from-sort'></div>")
        var sorting_body = jQuery('.sorting-columns .top-sort tbody');
        var shs = tr.find('td.to-sort');
        shs.attr('class', 'from-sort')
        tr.append("<td class='remove-attr'><a href='#' class='sortdirection' style='float:left'></a></td>");
        sorting_body.append(tr);
        tr.find("td").filter(".from-sort").remove();
       
        if (sorting_body.length)
            jQuery('.sorting-columns .placeholder').fadeOut(500);
    });

    jQuery(document).on("click", ".from-sort", function () {
        var $btn = jQuery(this), $row = $btn.parents('tr').first();
        var tr = $row.remove().clone();
        tr.find(".colname .from-sort").remove();
        tr.append("<td class='from-sort'></td>");
        tr.find(".remove-attr").remove();
        var sortable_body = jQuery('.sortable-columns1 .bot-sort tbody');
        var shs = tr.find('td.from-sort');
        shs.attr('class', 'to-sort');
        sortable_body.append(tr);
        var sorting_body = jQuery('.sorting-columns .top-sort tbody tr');
        if (sorting_body.length < 1)
            jQuery('.sorting-columns .placeholder').fadeIn(500);
    });

    /**************************************************************************
    **************************************************************************/
    //jQuery('td.to-sum').live('click', function () {
    //    var $btn = jQuery(this), $row = $btn.parents('tr').first();
    //    jQuery.get($btn.attr('href'));
    //    var tr = $row.remove().clone();
    //    var summing_body = jQuery('.summing-columns .top-sum tbody');
    //    var shs = tr.find('td.to-sum');        
    //    shs.attr('class', 'from-sum');
    //    summing_body.append(tr);
    //    if (summing_body.length)
    //        jQuery('.summing-columns .placeholder').fadeOut();
    //});
    //jQuery('td.from-sum').live('click', function () {
    //    var $btn = jQuery(this), $row = $btn.parents('tr').first();
    //    var tr = $row.remove().clone();
    //    var summable_body = jQuery('.summable-columns1 .bot-sum tbody');
    //    var shs = tr.find('td.from-sum')
    //    shs.attr('class', 'to-sum');
    //    summable_body.append(tr);
    //    var summing_body = jQuery('.summing-columns .top-sum tbody');
    //    if (summing_body.length < 1)
    //        jQuery('.summing-columns .placeholder').fadeIn();
    //});

    var columnClick = function () {
        $(this).hide();
        //$('td.visibility a').die("click", columnClick);
        var $btn = jQuery(this), $row = $btn.parents('tr').first();
        $row.addClass('fading').animate({ opacity: 0 }, function () {
            $row.removeClass('fading').toggleClass('hidden').toggleClass('visible').css('opacity', 1);
        });
        var visible = $row.hasClass('hidden'); // old state
        var no_column = jQuery('#DataGrid .no-column-container');
        if (visible && !no_column.hasClass('hide')) {
            no_column.addClass('hide');
            var $trs = jQuery('.datagrid tr[class^="odd"], .datagrid tr[class^="even"]');
            $trs.each(function (i, e) {
                jQuery(e).removeClass("hide");
            });
        }
        if (!visible && no_column.hasClass('hide')) {
            var $rows;
            if (jQuery('#viewoptions .dropdown-menu').hasClass('viewoptions-enlarged')) {
                $rows = jQuery('.middle.columns .scroll .first tr[class^="visible"], .middle.columns .scroll .second tr[class^="visible"]');
            } else {
                $rows = jQuery('.middle.columns .scroll .normal tr[class^="visible"]');
            }
            if ($rows.length == 1) {
                jQuery('#DataGrid .no-column-container').removeClass('hide');
                var $trs = jQuery('.datagrid tr[class^="odd"], .datagrid tr[class^="even"]');
                $trs.each(function (i, e) {
                    jQuery(e).addClass("hide");
                });
            }
        }
        var ordinal = $row.find('.colnumber').text() - 1;
        jQuery('.datagrid .col_' + ordinal).fadeToggle();
        var col_id = jQuery(this).parent().parent().children("#col-id").val();
        jQuery.get("/DataGrid/UpdateColumn", { id: col_id, is_visible: visible }, function (html) {
        });

        var items;
        if (jQuery('#viewoptions .dropdown-menu').hasClass('viewoptions-enlarged')) {
            items = jQuery('.middle.columns .scroll .normal tr');
        } else {
            items = jQuery('.middle.columns .scroll .first tr, .middle.columns .scroll .second tr');
        }
        items.each(function (i, e) {
            if (jQuery(e).find('.colnumber').text() - 1 == ordinal) jQuery(e).toggleClass('hidden').toggleClass('visible');
        });
        var invisible_items = jQuery('.middle.columns .scroll .normal tr.hidden:not(.fading), .middle.columns .scroll .normal tr.fading.visible');
        var visible_items = jQuery('.middle.columns .scroll .normal tr.visible:not(.fading), .middle.columns .scroll .normal tr.fading.hidden');
        var invisibleColumnsHalf = invisible_items.length % 2 == 0 ? parseInt(invisible_items.length / 2) : parseInt(invisible_items.length / 2) + 1;
        var visibleColumnsHalf = visible_items.length % 2 == 0 ? parseInt(visible_items.length / 2) : parseInt(visible_items.length / 2) + 1;
        var tbody_first = jQuery('.middle.columns .scroll .first tbody').html('');
        var tbody_second = jQuery('.middle.columns .scroll .second tbody').html('');
        invisible_items.each(function (i, e) {
            var clone = jQuery(e).clone();
            if (clone.hasClass('fading')) {
                clone.removeClass('fading').css('opacity', 1).toggleClass('hidden').toggleClass('visible');
            }
            if (i < invisibleColumnsHalf) clone.appendTo(tbody_first);
            else clone.appendTo(tbody_second);
        });
        visible_items.each(function (i, e) {
            var clone = jQuery(e).clone();
            if (clone.hasClass('fading')) {
                clone.removeClass('fading').css('opacity', 1).toggleClass('hidden').toggleClass('visible');
            }
            if (i < visibleColumnsHalf) clone.appendTo(tbody_first);
            else clone.appendTo(tbody_second);
        });

        var columnOrderableVisible = jQuery('.middle.columns .scroll .first tr.visible, .middle.columns .scroll .second tr.visible');
        var columnOrderable = jQuery('.middle.column-order-content .scroll .normal .column');
        for (var i = 0; i < columnOrderable.length; i++) {
            var column = jQuery(columnOrderable.get(i));
            column.fadeOut();
            for (var j = 0; j < columnOrderableVisible.length; j++) {
                var columnVisible = jQuery(columnOrderableVisible.get(j));
                if (columnVisible.find('.colnumber').text() == column.find('.colnumber').text()) {
                    column.fadeIn();
                    break;
                }
            }
        }

        var orderingItems; //ordering items visiblity
        if (jQuery('#viewoptions .dropdown-menu').hasClass('viewoptions-enlarged')) {
            orderingItems = jQuery('.middle.columns .scroll .normal tr');
        } else {
            orderingItems = jQuery('#column-order .scroll .sortable-columns .column');
        }

        /*
        for (var i = 0; i < orderingItems.length; i++) {//set the table body's width
            var column = jQuery(orderingItems.get(i));
            var width = $('th.col_' + i + ":visible").last().width();
            tempwidth += (width);
            if (width != null)
                columnCount += 1;
        }*/

        var tempwidth = 0;
        var columnCount = 0;
        var search = "[class^='col_" + ordinal + "']";
        jQuery(".datagrid.original-table.datagridHeader").find("[class^='col_']").filter(":visible").each(function() {
            tempwidth += $(this).width();
            columnCount++;
        });

        //Last version (2014.09.17)
        /*  if (jQuery("div.table-header").width() > tempwidth + columnCount)
              jQuery("div.table-body").width(jQuery("div.table-header").width());
          else
              jQuery("div.table-body").width(tempwidth + columnCount);*/
        
        if (($(window).width()) > (jQuery(".datagrid.original-table.datagridHeader").width() ) ) {
            jQuery(".datagrid.original-table.datagridHeader").width(tempwidth);
        }

        if (($(window).width()) <= (jQuery(".datagrid.original-table.datagridHeader").width())) {
            //tempwidth += columnCount;

            if (!visible && no_column.hasClass('hide')) {
                tempwidth -= jQuery(".datagrid.original-table.datagridHeader").find(search).filter(":visible").width();
            }
        }
        else {
            tempwidth = $(".inner-container").width() - columnCount;// - $.scrollbarWidth();
        }

        if ((tempwidth) <= $(".content-inner").width() - columnCount) {
            tempwidth = $(".content-inner").width() - columnCount;// - $.scrollbarWidth();
        }
        if ((tempwidth) <= $(window).width()) {
            tempwidth = $(window).width() + columnCount;
        }
        
        jQuery(".inner-container2 .table-body").width(tempwidth + columnCount + 5);
        jQuery(".inner-container2 .table-header").width(tempwidth + columnCount);
        jQuery(".datagrid.original-table.datagridFooter").width(tempwidth);
        jQuery(".datagrid.original-table.datagridHeader").width(tempwidth);

        //$("td.visibility a").live("click", columnClick);
        //$(this).fadeIn();

        setTimeout(function () {
            jQuery(".content-box .content-outer .content-inner .header.main-header .functions .dropdown.big.view .panel .dropdown-menu.normal .column-view-props.visible .box .clickHidden").removeAttr("id", "hide-column");
        }, 300);

        var selectedRow = $(this).parent().parent().find("#col-id").val();

        //sum - row visible/hide
        jQuery(document).find("#extsummaoptions").find(".item-id").each(function () {
            if ($(this).text() == selectedRow)
            {
                $(this).parent().toggle();
                var tmp = $(this).parent().find(".from-sum");
                if (tmp != undefined && tmp.length != 0) {
                    tmp.click();
                }
            }
        });

        //subtotal - row visible/hide
        jQuery(document).find("#subtotaloptions").find(".item-id").each(function () {
            if ($(this).text() == selectedRow) {
                $(this).parent().toggle();
                var tmp = $(this).parent().find(".del").filter(":button");
                if (tmp != undefined && tmp.length != 0) {
                    tmp.click();
                    $(this).parent().find(":button").show();
                    tmp.hide();
                }
            }
        });

        return false;
    }

    $(window).resize(function () {
        var tempwidth = 0;
        var columnCount = 0;
        jQuery('.datagrid.original-table.datagridHeader').find('[class^=\'col_\']').filter(':visible').each(function () {
            tempwidth += $(this).width();
            columnCount++;
        });
        
        if ((tempwidth) <= $('.content-inner').width() - columnCount) {
            tempwidth = $('.content-inner').width() - columnCount; // - $.scrollbarWidth();
        }
    });

    $.scrollbarWidth = function () {
        var parent, child, width;

        if (width === undefined) {
            parent = $('<div style="width:50px;height:50px;overflow:auto"><div/></div>').appendTo('body');
            child = parent.children();
            width = child.innerWidth() - child.height(99).innerWidth();
            parent.remove();
        }

        return width;
    };
    jQuery(document).on("click", "td.visibility a", columnClick);

    jQuery(document).on("click", "th.visibility a", function () {
        $("th.visibility a").unbind("click");
        var $btn = jQuery(this);
        var $rows = jQuery('.middle.columns .scroll tr');
        var $tabheader = jQuery('.column-view-props .tabheader.selected');
        var visible = !$tabheader.hasClass('visible');
        jQuery.get($btn.attr('href'), { is_visible: visible, forAll : true });
        if (visible)
            jQuery('#DataGrid .no-column-container').addClass('hide');
        else
            jQuery('#DataGrid .no-column-container').removeClass('hide');
        $rows.each(function (i, e) {
            var $row = jQuery(e);
            if (visible) $row.removeClass('hidden').addClass('visible');
            else $row.removeClass('visible').addClass('hidden');
        });
        var $columns = jQuery('.datagrid th[class^="col"], .datagrid td[class^="col"]');
        $columns.each(function (i, e) {
            var col = jQuery(e);
            if (visible) col.fadeIn();
            else col.fadeOut();
        });
        var $trs = jQuery('.datagrid tr[class^="odd"], .datagrid tr[class^="even"]');
        $trs.each(function (i, e) {
            var row = jQuery(e);
            if (visible) row.removeClass("hide");
            else row.addClass("hide");
        });

        var orderingItems; //ordering items visiblity
        if (jQuery('#viewoptions .dropdown-menu').hasClass('viewoptions-enlarged')) {
            orderingItems = jQuery('.middle.columns .scroll .normal tr');
        } else {
            orderingItems = jQuery('#column-order .scroll .sortable-columns .column');
        }
        var tempwidth = 0;
        var columnCount = 0;
        for (var i = 0; i < orderingItems.length; i++) {//set the table body's width
            var column = jQuery(orderingItems.get(i));
            var width = $('th.col_' + i).last().width();
            if (visible) {
                column.fadeIn();
                tempwidth += (width);
                if (width != null)
                    columnCount += 1;
            }
            else {
                column.fadeOut();
            }
        }

        //Last version (2014.09.17)
        /*  if (jQuery("div.table-header").width() > tempwidth + columnCount)
              jQuery("div.table-body").width(jQuery("div.table-header").width());
          else
              jQuery("div.table-body").width(tempwidth + columnCount);*/
        var tempwidth = 0;
        var columnCount = 0;
        jQuery('.datagrid.original-table.datagridHeader').find('[class^=\'col_\']').filter(':visible').each(function () {
            tempwidth += $(this).width();
            columnCount++;
        });

        if ((tempwidth) <= $('.content-inner').width() - columnCount) {
            tempwidth = $('.content-inner').width() - columnCount; // - $.scrollbarWidth();
        }


        jQuery(".inner-container2 .table-body").width(tempwidth + columnCount);
        jQuery(".inner-container2 .table-header").width(tempwidth + columnCount);
        jQuery(".datagrid.original-table.datagridFooter").width(tempwidth + columnCount + 5);
        jQuery(".datagrid.original-table.datagridHeader").width(tempwidth + columnCount);


        var invisible_items = jQuery('.middle.columns .scroll .normal tr[class^="hidden"]');
        var visible_items = jQuery('.middle.columns .scroll .normal tr[class^="visible"]');
        var invisibleColumnsHalf = invisible_items.length % 2 == 0 ? parseInt(invisible_items.length / 2) : parseInt(invisible_items.length / 2) + 1;
        var visibleColumnsHalf = visible_items.length % 2 == 0 ? parseInt(visible_items.length / 2) : parseInt(visible_items.length / 2) + 1;
        var tbody_first = jQuery('.middle.columns .scroll .first tbody').html('');
        var tbody_second = jQuery('.middle.columns .scroll .second tbody').html('');
        invisible_items.each(function (i, e) {
            var clone = jQuery(e).clone();
            if (i < invisibleColumnsHalf) clone.appendTo(tbody_first);
            else clone.appendTo(tbody_second);
        });
        visible_items.each(function (i, e) {
            var clone = jQuery(e).clone();
            if (i < visibleColumnsHalf) clone.appendTo(tbody_first);
            else clone.appendTo(tbody_second);
        });
        $("th.visibility a").bind("click");
        return false;
    });
    /**************************************************************************
    * COLUMN OPTIONS TAB
    **************************************************************************/
    jQuery(document).on("click", ".column-view-props .tabheader", function () {
        if (jQuery(this).hasClass('selected')) return;
        if (jQuery(this).hasClass('filter')) jQuery('.search-viewoptions-panel').fadeOut();
        else jQuery('.search-viewoptions-panel').fadeIn();
        var new_block = jQuery(this).attr("class").replace('tabheader', '').replace(' ', '');
        var old_block = jQuery(this).blur().addClass('selected').removeAttr('href')
            .siblings('.selected').removeClass('selected').attr('href', '#').attr('class').replace('tabheader', '').replace(' ', '');
        jQuery(this).parents('.column-view-props').removeClass(old_block).addClass(new_block);
        $("td.visibility a").fadeIn();
        return false;
    });
    /**************************************************************************
    * FUNCTIONS TAB
    **************************************************************************/
    jQuery(document).on("click", ".tabfunctions .tabheader", function () {
        if (jQuery(this).hasClass('selected')) return; // if the tabheader is already selected, nothing to do
        var new_block = jQuery(this).attr("class").replace('tabheader', '').replace(' ', ''); // remove tabheader class on the tabpage to be selected
        var old_block = jQuery(this).blur().addClass('selected').removeAttr('href')         // add the selected class, remove link,
            .siblings('.selected').removeClass('selected').attr('href', '#').attr('class').replace('tabheader', '').replace(' ', ''); // remove siblings "selected"
        if (jQuery(this).parents('.relation-specific .join-relation-tableobjects').length)
            jQuery('.relation-specific .join-relation-tableobjects').removeClass(old_block).addClass(new_block);
        else jQuery(this).parents('.box').removeClass(old_block).addClass(new_block);
        if (jQuery(this).parents('.join-specific .join-relation-tableobjects').length)
            jQuery('.join-specific .join-relation-tableobjects').removeClass(old_block).addClass(new_block);
        else jQuery(this).parents('.box').removeClass(old_block).addClass(new_block);
        return false;
    });
    /**************************************************************************
    * RELATION FUNCTIONS
    **************************************************************************/
    // get the input field for second table selection... subscribing methods for mouse enter and leave events. 
    jQuery('#table_select_').hover(function () {
        jQuery(this).parent().addClass('over');
    }, function () {
        jQuery(this).parent().removeClass('over');
    });
    //get the input field... subscribe event for OnFocus. After the element get focused, the actual list fade in. If it is getting unfocused, the list fade out
    jQuery(document).on("focus", "#table_select_", function () {
        jQuery('.relation-specific .join-relation-tableobjects').fadeIn();
    }).on("focusout", "#table_select_", function (e) {
        if (!jQuery(this).parent().is('.over'))
            jQuery('.relation-specific .join-relation-tableobjects').fadeOut();
    });
    //subscribe method for a "selection changed" event of the table selection drop down list... get table id,and table name... put table id to the "table2id" hidden input field... GET available columns.
    jQuery(document).on("click", ".table-relations .join-relation-tableobjects .TableObject", function () {
        var $tobj = jQuery(this),
            id = $tobj.find('.item-id').text(),
            name = $tobj.find('.description').text();
        jQuery('#relationtable2Id').val(id);
        jQuery('#table_select_').val(name).blur();
        jQuery('.table-relations .join-relation-tableobjects').fadeOut();
        jQuery.get('/DataGrid/ColumnList/' + id, function (html) {
            if (html.search("login") == -1) {
                jQuery('#relation-column-dropdown2').html(html); // fill up dropdown list
                jQuery('input.relation-column2.dropdown').val(jQuery('input.relation-column2.dropdown').attr('placeholder')).addClass("empty").removeClass("dirty");
            } else {
                window.location = "/Account/LogOn";
            }
        });
    });
    jQuery(document).on("click", ".add-new-relation-condition", function () {
        var element = jQuery('.table-relation-condition.replace').clone().removeClass('replace');
        var id = jQuery('.table-relation-condition').length - 1;
        element.find('input[type=hidden].column1').attr('name', 'columns[' + id + '].column1');
        element.find('input[type=hidden].dir').attr('name', 'columns[' + id + '].dir');
        element.find('input[type=hidden].column2').attr('name', 'columns[' + id + '].column2');
        element.insertAfter(jQuery('.table-relation-condition:last'));
    });
    jQuery(document).on("click", ".table-relation-condition .delete", function () {
        var tableJoinConditionContainer = jQuery(this).parent();
        var columnDropDown1 = tableJoinConditionContainer.find("#column-dropdown1");
        var columnDropDown2 = tableJoinConditionContainer.find("#relation-column-dropdown2");
        if (columnDropDown1.length) columnDropDown1.appendTo(tableJoinConditionContainer.parent());
        if (columnDropDown2.length) columnDropDown2.appendTo(tableJoinConditionContainer.parent());
        tableJoinConditionContainer.remove();
    });
    /**************************************************************************
    * JOIN FUNCTIONS
    **************************************************************************/
    // get the input field for second table selection... subscribing methods for mouse enter and leave events. 
    jQuery('#table_select').hover(function () {
        jQuery(this).parent().addClass('over');
    }, function () {
        jQuery(this).parent().removeClass('over');
    });
    // get the dropdownlist for tables (the actual dropdownlist, with id="tableobjects")... subscribing methods for mouse enter and leave events.
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
    //get the input field... subscribe event for OnFocus. After the element get focused, the actual list fade in. If it is getting unfocused, the list fade out
    jQuery(document).on("focus", "#table_select", function () {
        jQuery('.join-specific .join-relation-tableobjects').fadeIn();
    });
    jQuery(document).on("focusout", "#table_select", function (e) {
        if (!jQuery(this).parent().is('.over'))
            jQuery('.join-specific .join-relation-tableobjects').fadeOut();
    });
    jQuery(document).on("focusout", ".join-specific .join-relation-tableobjects", function (e) {
        if (!jQuery(this).parent().is('.over'))
            jQuery(this).fadeOut();
    });
    //subscribe method for a "selection changed" event of the table selection drop down list... get table id,and table name... put table id to the "table2id" hidden input field... GET available columns.
    jQuery(document).on("click", ".table-join .join-relation-tableobjects .TableObject", function () {
        var $tobj = jQuery(this),
            id = $tobj.find('.item-id').text(),
            name = $tobj.find('.description').text();
        jQuery('#jointable2Id').val(id);
        jQuery('#table_select, table-join').val(name).blur();
        jQuery('.table-join .join-relation-tableobjects').fadeOut();
        jQuery.get('/DataGrid/ColumnList/' + id, function (html) {
            if (html.search("login") == -1) {
                jQuery('#join-column-dropdown2').html(html); // fill up dropdown list
                jQuery('input.join-column2.dropdown').val(jQuery('input.join-column2.dropdown').attr('placeholder')).addClass("empty").removeClass("dirty");
            } else {
                window.location = "/Account/LogOn";
            }
        });
    });
    jQuery(document).ready(function () {
        var id;
        var select = $('#jointable2Id');
        if (select.length > 0) {
            id = parseInt($('#jointable2Id').val().trim());
        }
        $tableSelect = $('#table_select');
        if (isNaN(id) || id == 0) {
            $tableSelect.val($tableSelect.attr('placeholder')).addClass('empty');
        } else {
            $tableSelect.removeClass('empty');
            $.get('/DataGrid/ColumnList/' + id, function (html) {
                if (html.search("login") == -1) {
                    $('#join-column-dropdown2').html(html); // fill up dropdown list
                    $('input.join-column2.dropdown').val($('input.join-column2.dropdown').attr('placeholder')).addClass("empty").removeClass("dirty");
                } else {
                    window.location = "/Account/LogOn";
                }
            });
        }
    });
    jQuery('input.column1.dropdown,input.relation-column2.dropdown,input.join-column2.dropdown').hover(function () {
        jQuery(this).parent().addClass('over');
    }, function () {
        jQuery(this).parent().removeClass('over');
    });
    jQuery('#column-dropdown,#relation-column-dropdown2,#join-column-dropdown2').hover(function () {
        jQuery(this).parent().addClass('over');
    }, function () {
        jQuery(this).parent().removeClass('over');
    });
    // select the columns for table 1.
    jQuery(document).on("focus", "input.column1.dropdown", function () {
        var $input = jQuery(this);
        jQuery('#column-dropdown1').insertAfter(this).fadeIn()
            .find('.column').unbind('click.column').bind('click.column', function () {
                var $column = jQuery(this),
                    id = $column.find('.item-id').text(),
                    name = $column.find('.colname').text(),
                      dataType = $column.find('.columntype').val(),
                    dataLength = $column.find('.columnlength').val();

            var datalength2 = parseInt(dataLength);

            if (dataType == "String" && (dataLength == "0" || datalength2 > 255 )) {
                    var somexx1 = document.getElementsByClassName("functionJoinColumn1");
                    for (i = 0; i < somexx1.length; i++) {
                        somexx1[i].style.visibility = 'visible';
                    }
                } else {
                    var somexx2 = document.getElementsByClassName("functionJoinColumn1");
                    for (i = 0; i < somexx2.length; i++) {
                         somexx2[i].style.visibility = 'hidden';
                    }
                }

                $input.parent().prev().val(id);
                $input.val(name).blur();
                jQuery('#column-dropdown1').fadeOut();
                $column.unbind('click.column');
            });
    }).on("focusout", "input.column1.dropdown", function (e) {
        if (!jQuery(this).parent().is('.over'))
            jQuery('#column-dropdown1').fadeOut();
    });
    $(".droparea").mouseup(function (e) {
        var container = $("#column-dropdown");

        if (!container.is(e.target) // if the target of the click isn't the container...
            && container.has(e.target).length === 0) // ... nor a descendant of the container
        {
            container.fadeOut();
        }
    });

    // forbid typing into column selector fields
    $(document).on("keypress", ".table-relations .column-container input[type=text], .table-join .column-container input[type=text]", function (ev) {
        ev.stopPropagation();
        ev.preventDefault();
    });

    // select column for table 2 when addig relations
    jQuery(document).on("focus", "input.relation-column2.dropdown", function () {
        var $input = jQuery(this);
        jQuery('#relation-column-dropdown2').insertAfter(this).fadeIn()
            .find('.column').unbind('click.relation-column2').bind('click.relation-column2', function () {
                var $column = jQuery(this),
                    id = $column.find('.item-id').text(),
                    name = $column.find('.colname').text();
                $input.parent().prev().val(id);
                $input.val(name).blur();
                jQuery('#relation-column-dropdown2').fadeOut();
                $column.unbind('click.relation-column2');
            });
    }).on("focusout", "input.relation-column2.dropdown", function (e) {
        if (!jQuery(this).parent().is('.over'))
            jQuery('#relation-column-dropdown2').fadeOut();
    });
    // select column for table 2 when joining
    jQuery(document).on("focus", "input.join-column2.dropdown", function () {
        var $input = jQuery(this);
        jQuery('#join-column-dropdown2').insertAfter(this).fadeIn()
            .find('.column').unbind('click.join-column2').bind('click.join-column2', function () {
                var $column = jQuery(this),
                    id = $column.find('.item-id').text(),
                    name = $column.find('.colname').text(),
                    dataType = $column.find('.columntype').val(),
                    dataLength = $column.find('.columnlength').val();

                var datalength2 = parseInt(dataLength);

                if (dataType == "String" && (dataLength == "0" || datalength2 > 255 )) {
                    var somexx1 = document.getElementsByClassName("functionJoinColumn2");
                    for (i = 0; i < somexx1.length; i++) {
                        somexx1[i].style.visibility = 'visible';
                    }
                } else {
                    var somexx2 = document.getElementsByClassName("functionJoinColumn2");
                    for (i = 0; i < somexx2.length; i++) {
                        somexx2[i].style.visibility = 'hidden';
                    }
                }

                $input.parent().prev().val(id);
                $input.val(name).blur();
                jQuery('#join-column-dropdown2').fadeOut();
                $column.unbind('click.join-column2');
            });
    }).on("focusout", "input.join-column2.dropdown", function (e) {
        if (!jQuery(this).parent().is('.over'))
            jQuery('#join-column-dropdown2').fadeOut();
    });
    jQuery(document).on("click", ".relation-specific .join-relation-tableobjects .navigation a.iconx, .relation-specific .join-relation-tableobjects .tabbar a.tabheader, .relation-specific .ownSubmit", function () {
        var inputItem = $(".relation-specific .ownfield");
        var search = inputItem.attr("placeholder") != inputItem.val() ? inputItem.val() : "";
        var $tobj = jQuery(this);
        if ($tobj.hasClass('selected') && !$tobj.is('.relation-specific .join-relation-tableobjects .tabbar a.tabheader')) return;
        var $to = jQuery('.relation-specific .join-relation-tableobjects');
        var type = $to.hasClass('Views') ? 2 : 0;
        type = $to.hasClass('Tables') ? 1 : type;
        var page = $tobj.children().first().text();
        $tobj.children().first().text(page);
        jQuery.get('/DataGrid/TableObjectList/', { type: type, page: page, search: search }, function (html) {
            $html = jQuery(html);
            for (var i = 2; i < $to.children().length; i++) {
                $to.children()[i].innerHTML = $html.children()[i].innerHTML;
            }
        });
    });
    jQuery(document).on("click", ".join-specific .join-relation-tableobjects .navigation a.iconx, .join-specific .join-relation-tableobjects .tabbar a.tabheader,.join-specific .ownSubmit", function () {
        var inputItem = $(".join-specific .ownfield");
        var search = inputItem.attr("placeholder") != inputItem.val() ? inputItem.val() : "";
        var $tobj = jQuery(this);
        if ($tobj.hasClass('selected') && !$tobj.is('.join-specific .join-relation-tableobjects .tabbar a.tabheader')) return;
        var $to = jQuery('.join-specific .join-relation-tableobjects');
        var type = $to.hasClass('Views') ? 2 : 0;
        type = $to.hasClass('Tables') ? 1 : type;
        var page = $tobj.children().first().text();
        $tobj.children().first().text(page);
        jQuery.get('/DataGrid/TableObjectList/', { type: type, page: page, search: search }, function (html) {
            $html = jQuery(html);
            for (var i = 2; i < $to.children().length; i++) {
                $to.children()[i].innerHTML = $html.children()[i].innerHTML;
            }
        });
    });

    jQuery(document).on("keyup", ".relation-specific .ownfield", function (e) {
        if (e.which == 13) {
            $(".relation-specific .ownSubmit").trigger("click");
        }
    });
    jQuery(document).on("keyup", ".join-specific .ownfield", function (e) {
        if (e.which == 13) {
            $(".join-specific .ownSubmit").trigger("click");
        }
    });

    // click event for "add new condition" button. get an element, clone it, and insert after the last one.
    jQuery(document).on("click", ".add-new-join-condition", function () {
        var element = jQuery('.table-join-condition.replace').clone().removeClass('replace');
        var id = jQuery('.table-join-condition').length - 1;
        element.find('input[type=hidden].column1').attr('name', 'columns[' + id + '].column1');
        element.find('input[type=hidden].dir').attr('name', 'columns[' + id + '].dir');
        element.find('input[type=hidden].column2').attr('name', 'columns[' + id + '].column2');
        element.insertAfter(jQuery('.table-join-condition:last'));
    });
    $(document).ready(function () {
        $('.TextTypeIndexjQueryUITooltip').tooltip();
    });
    
    var parseColumnDropdownInfos = function () {
        var $infos = $("#column-dropdown-info");

        if ($infos.length > 0) {
            var $rows = $infos.children(".column");
            $.each($rows, function () {
                var $row = $(this);
                var id = $($row).children(".item-id").first().html();
                var ordinal = $($row).children(".colnumber").first().html();
                var type = $($row).children(".columntype").first().val();
                var length = $($row).children(".columnlength").first().val();
                var visible = $($row).children(".columnvisible").first().val() == "true" ? true : false ;
                columnInfos[id] = { id: id, ordinal: ordinal, type: type, length: length, visible: visible };
            });
        }
    };

    $(document).ready(function () {
        parseColumnDropdownInfos();
        fillColumnDropdown();
        $('.UserDefReljQueryUITooltip').tooltip();
    });
    // it seems the DataGrid/Dialog method not exist, so cannot be GET requested, the submit event is handled by DataGrid/Join 
    jQuery(document).on("click", ".table-join .submit", function () {

        if ($(".join-specific .search-panel").hasClass("focus"))
        { return false; }

        var save = jQuery('#save_result_join_cb').is(':checked');
        jQuery('#saveJoin').val(save);
        if (save && jQuery('#tableJoinName').val == '') {
            jQuery.get('/DataGrid/Dialog/', function (html) {
                if (html.search("login") == -1) {
                    jQuery('#dialog').html(html);
                } else {
                    window.location = "/Account/LogOn";
                }
            });
            return false;
        }
    });
    //just check if every information is entered
    jQuery(document).on("click", ".table-relations .submit", function () {

        if ($(".relation-specific .search-panel").hasClass("focus"))
        { return false; }

    });
    // remove the join condition 
    jQuery(document).on("click", ".table-join-condition .delete", function () {
        var tableJoinConditionContainer = jQuery(this).parent();
        var columnDropDown1 = tableJoinConditionContainer.find("#column-dropdown1");
        var columnDropDown2 = tableJoinConditionContainer.find("#join-column-dropdown2");
        if (columnDropDown1.length) columnDropDown1.appendTo(tableJoinConditionContainer.parent());
        if (columnDropDown2.length) columnDropDown2.appendTo(tableJoinConditionContainer.parent());
        tableJoinConditionContainer.remove();
    });
    /**************************************************************************
    * GROUP FUNCTIONS
    **************************************************************************/
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
    jQuery(document).on("focus", "input.colids.dropdown", function() {
        var $input = jQuery(this);
        // cleanup when selected
        var selectedLines = document.getElementsByClassName("dropdown_sqlfunction_column_line");
        for (i = 0; i < selectedLines.length; i++) {
            var selectedLine = selectedLines[i];
            var selectedLineValue = selectedLine.value;
            var selectedRows = document.getElementsByClassName("dropdown_sqlfunction_column_row");
            for (j = 0; j < selectedRows.length; j++) {
                var selectedRow = selectedRows[j];
                var selectedRowValue = selectedRow.getElementsByClassName("item-id")[0].innerHTML;
                if (selectedLineValue == selectedRowValue) {
                    selectedRow.style = "display: none";
                }
            }
        }
        jQuery('#column-dropdown').insertAfter(this).fadeIn()
            .find('.column').unbind('click.column').bind('click.column', function (e) {
                var $column = jQuery(this),
                    id = $column.find('.item-id').text(),
                    name = $column.find('.colname').text();
                // cleanup when change column
                var $inputDropDown = $column.parent().prev().prev();
                var lineValue = $inputDropDown.val();
                var selectedORows = document.getElementsByClassName("dropdown_sqlfunction_column_row");
                for (j = 0; j < selectedORows.length; j++) {
                    var selectedORow = selectedRows[j];
                    var selectedORowValue = selectedORow.getElementsByClassName("item-id")[0].innerHTML;
                    if (lineValue == selectedORowValue) {
                        selectedORow.style = "";
                    }
                }
                $input.prev().val(id).attr('name', 'colIds');
                $input.val(name).blur();
                jQuery('#column-dropdown').fadeOut();
                $column.unbind('click.column');
                if (!$input.parent().next('li').length) {
                    var $template = $input.parent().prevAll('.template'),
                        $clone = $template.clone().removeClass('template').insertAfter($input.parent());
                }
            });
    }).on("focusout", "input.colids.dropdown", function (e) {
        if (!jQuery(this).parent().is('.over')) {
            jQuery('#column-dropdown').fadeOut();
            sleep(500);
        }
        // cleanup when deleted
        var $this = jQuery(this);
        if ($this.hasClass("empty")) {
            var $cleanup = $this.prev();
            var selectedLineValue = $cleanup.val();
            var selectedRows = document.getElementsByClassName("dropdown_sqlfunction_column_row");
            for (j = 0; j < selectedRows.length; j++) {
                var selectedRow = selectedRows[j];
                var selectedRowValue = selectedRow.getElementsByClassName("item-id")[0].innerHTML;
                if (selectedLineValue == selectedRowValue) {
                    selectedRow.style = "";
                }
            }
            $cleanup.removeAttr("name");
            $cleanup.removeAttr("value");
        }
    });
    function sleep(milliseconds) {
        var start = new Date().getTime();
        for (var i = 0; i < 1e7; i++) {
            if ((new Date().getTime() - start) > milliseconds) {
                break;
            }
        }
    }
    // save group
    jQuery(document).on("click", ".table-group .submit", function () {
        var save = jQuery('#save_result_group_cb').is(':checked');
        jQuery('#saveGroup').val(save);
        if (save && jQuery('#tableGroupName').val == '') {
            jQuery.get('/DataGrid/Dialog/', function (html) {
                if (html.search("login") == -1) {
                    jQuery('#dialog').html(html);
                } else {
                    window.location = "/Account/LogOn";
                }
            });
            return false;
        }
    });
    // column dropdown
    jQuery(document).off("click", "#extfilter .column .dropdown").on("click", "#extfilter .column .dropdown", function () {
        var $cinput = jQuery(this),
            $parent = jQuery('#extfilter'),
            $dropdown = jQuery('#column-dropdown').appendTo($parent).fadeIn(),
            $columns = $dropdown.find('.column'),
            offset_p = $parent.offset(),
            offset_c = $cinput.offset();
        // positioning
        $dropdown.css({
            left: Math.round(offset_c.left - offset_p.left) + 'px',
            top: Math.round(offset_c.top - offset_p.top + $cinput.height()) + 'px'
        });
        // maintain closing
        jQuery(document).bind('click.extfilter_columndropdown', function (e) {
            if (e.handled) return;
            jQuery('#column-dropdown').fadeOut();
            jQuery(document).unbind('click.extfilter_columndropdown');
        });
        // entry click
        $columns.unbind('click').click(function () {
            var $column = jQuery(this);
            //            console.info("The number of input fields: " + $cinput.length);
            //            var filterbefore = jQuery('.filter .droparea :not(.optional) > span.op-command').text();
            //            console.info("Filter string BEFORE column selection: " + filterbefore);
            $cinput.prev('span.column').text('%' + $column.find('.item-id').text());
            var tx = $column.find('.colname').text().replace('"', '\"');
            $cinput.attr("title", tx); // set title for tooltip
            var measuringDiv = $('<div class="measuringDiv" style="position:absolute;visibility:hidden;height:auto;width:auto;"></div>').text(tx); // Hidden div for measuring
            // If measureDiv not exist,
            if ($cinput.parent().children('.measuringDiv').length == 0) {
                $cinput.parent().append(measuringDiv); // then add it,
            } else {
                $cinput.parent().children('.measuringDiv').replaceWith(measuringDiv); // else replace this
            }
            var txWithDots = tx; // var for dotted text
            while ($cinput.parent().children('.measuringDiv').width() > $cinput.width()) {
                tx = tx.substring(0, tx.length - 1); // decrease the tx length 1 from the end
                txWithDots = tx + "...";
                $cinput.parent().children('.measuringDiv').text(txWithDots);
            }
            $cinput.text(txWithDots);
            $cinput.css({ color: '#037db0' });
            jQuery(document).trigger('click.extfilter_columndropdown');
        });
    });

    jQuery(document).on("click", "#viewoptions .close-popup, #extfilteroptions .close-popup, #extsortoptions .close-popup, #extsummaoptions .close-popup, #tabfunctions .close-popup, #rolebasedvisibilityoptions .close-popup, #subtotaloptions .close-popup", function () {
        jQuery(document).trigger('click.dropdown');
    });

    /**************************************************************************
    * MULTI OPTIMIZATIONS FUNCTIONS
    **************************************************************************/
    jQuery(document).on("click", "#multiOptimization", function () {
        $("#multiOptimization").parent().submit();
    });

    jQuery(".multipleOptimization .lefty select").on("change", function () {
        var useFromSelection = $(this).parent();
        var level = parseInt($(useFromSelection)[0].id.replace(/[a-z]+_/, ""));
        refreshMultiOptimization(level, ".lefty", $(this).val());
    });

    jQuery(".multipleOptimization .rigthy select").on("change", function () {
        var useFromSelection = $(this).parent();
        var level = parseInt($(useFromSelection)[0].id.replace(/[a-z]+_/, ""));
        refreshMultiOptimization(level, ".rigthy", $(this).val());
    });

    function refreshMultiOptimization(level, side, actually) {
        var select = $(side + " #select_" + (level + 1));
        if (select[0] != undefined) {
            var oldValue = $(select[0]).find('option:selected').text();
            jQuery.getJSON("/DataGrid/OptimisationList", { level: level + 1, id: actually }, function (options) {
                select.find('option').remove();
                for (var i = 0; i < options.length; i++) {
                    if ($.trim(oldValue) == $.trim(options[i][1]))
                    {
                        select.find('select').append('<option value="' + options[i][0] + '" selected>' + options[i][1] + '</option>');
                    }
                    else
                    {
                        select.find('select').append('<option value="' + options[i][0] + '">' + options[i][1] + '</option>');
                    }
                }
                //jQuery(" " + side + " #select_" + (level + 1) +" option").on("click", function () {
                //    var useFromSelection = $(this).parent().parent();
                //    var level = parseInt($(useFromSelection)[0].id.replace(/[a-z]+_/, ""));
                //    refreshMultiOptimization(level, side, $(this).val());
                //});
            });
            var actuallyNext = $(side + " #select_" + (level + 1) + " select").val();
            refreshMultiOptimization(level + 1, side, actuallyNext);
        }
    }

    jQuery(document).on("click", "#visibleButton #open-save-dialog", function () {
        $.get("/DataGrid/GetVisibilitySaveDialog", function (html) {
            var $dialog = jQuery('#dialog').html(html).fadeIn(500);
        });
    });

    $(document).on("click", "#dialog #cancel-save", function () {
        $("#dialog").fadeOut(500);
    });

    var fillColumnDropdown = function () {
        var $columns = $("#column-dropdown-info .column");
        var htmlColumnString = "";
        $.each($columns, function () {
            var id = $(this).find(".item-id").text();
            if (columnInfos.hasOwnProperty(id) && columnInfos[id]["visible"] == true) {
                htmlColumnString += $(this).prop("outerHTML");
            }
        });

        $("#column-dropdown").html(htmlColumnString);
        $("#column-dropdown1").html(htmlColumnString);
    };

    var updateColumn = function (forAll) { 
        var $waitdialog = null;
        $waitdialog = jQuery('#wait-dlg-visibility');
        var $dialog = jQuery('#dialog').html($waitdialog.html()).show();

        var id = [];
        var isVisible = [];
        $("#viewOptionPartial_columnList .normal tr").each(function () {
            id.push($(this).find("#col-id").val());
            isVisible.push($(this).hasClass("visible"));
        });

        $.ajax({
            url: "/DataGrid/UpdateColumnArray",
            data: { id: id, is_visible: isVisible, forAll: forAll, save: true },
            dataType: "json",
            success: function () {
                $dialog.hide();

                var currentTableId = parseInt($("#currentTableId").html());
                $.getJSON("/DataGrid/ReloadVisibleColumns/" + currentTableId, function (columns) {
                    for (var i = 0; i < columns.length; i++) {
                        columnInfos[columns[i][0]]["visible"] = columns[i][3] == "" ? true : false;
                    }
                    fillColumnDropdown();
                });
            },
            method: "post"
        });
    };

    $(document).on("click", "#dialog #save-for-me", function () {
        updateColumn(false);
    });

    $(document).on("click", "#dialog #save-for-all", function () {
        updateColumn(true);
    });
    //jQuery(".inner-container form .ok-button").on("click", function () {
    //    if ($(this).parent().parent().parent().find("input[type='text']").val() != "")
    //    {
    //        $(this).parent().parent().parent().submit();
    //        $(this).attr("disabled", true);
    //        $(this).parent().find(".reset-button").attr("disabled", true);
    //        $(this).parent().find(".checkboxon").off("click");
    //        $(this).parent().parent().parent().find("input[type='text']").attr("disabled", true);
    //    } 
    //});
    //jQuery(".inner-container form .reset-button").on("click", function () {
    //    if ($(this).parent().parent().parent().find("input[type='text']").val() != "")
    //    {
    //        $(this).parent().parent().parent().submit();
    //        $(this).attr("disabled", true);
    //        $(this).parent().find(".ok-button").attr("disabled", true);
    //        $(this).parent().find(".checkboxon").off("click");
    //        $(this).parent().parent().parent().find("input[type='text']").attr("disabled", true);
    //    }
    //});
});