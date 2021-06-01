$(document).ready(function () {
    var TableObjectIds = {};
    var unselectedTableObjectIds = {};
    var partiallyActiveTableObjectIds = {};
    var TablesByCategoryObj = {};
    $("#exportables").treeTable({ initialState: "collapsed" });
    // dynamically load treenodes

    //*********** Change Expander ****************************
    ChangeExpander();
    function ChangeExpander() {
        setTimeout(function () {
            var that = jQuery("table#exportables .expander");
            var rows = $(that).parents('tr');
            rows.each(function () {
                var count = parseInt(this.lastElementChild.textContent);
                if (count == 0) {
                    $(this).find('td.description span.expander').addClass('expander2');
                    $(this).find('td.description span.expander').removeClass('expander');
                    $("td.description span.expander2").unbind("click");
                }
            });
        }, 2000);
    }
    //********************************************************

    var get_length = function () {
        var length = 0;
        for (var e in TableObjectIds)
            if (TableObjectIds.hasOwnProperty(e))
                length++;
        return length;
    };

    var check_by_category = function (obj, filter) {
        var key, keys = [];
        for (key in obj) {
            if (obj.hasOwnProperty(key) && filter.test(key)) {
                return true;
            }
        }
        return false;
    };
    // ex:
    //var filteredNames = filtered_keys(names, /Peter/);

    var check_active = function (item_id, type, parentId) {
        var cat_id;
        var id;
        var item_is_category = $("#" + item_id).hasClass(".category-row");
        if (item_is_category) {
            cat_id = Number($("#" + item_id).parents("tr").first().find(".item-id").first().text());
        } else {
            cat_id = Number($("#" + item_id).parents("tr").first().prev(".category-row").find(".item-id").first().text());
            id = cat_id + "_" + item_id;
        }

        if (type < 3) {
            // if (!get_length() > 0 && jQuery('#' + parentId).hasClass('active'))//
            if (item_is_category) {
                return check_by_category(TableObjectIds, new RegExp(cat_id + "_\\d+"));
            } else {
                if (jQuery('#' + parentId).hasClass('active') && !unselectedTableObjectIds.hasOwnProperty(item_id))
                    return true;
                if (TableObjectIds[id] && !partiallyActiveTableObjectIds[id])
                    return true;
                else
                    return false;
            }
        } else {
            if (!get_length() > 0 && jQuery('#' + parentId).hasClass('active'))
                return true;
            if (type == 10 && jQuery('#' + parentId).hasClass('active'))
                return true;
            if (!TableObjectIds[parentId.replace('node-tobj-', '')])
                return false;
            var arr = TableObjectIds[parentId.replace('node-tobj-', '')].split("[");
            arr = arr[1].split("]");
            arr = arr[0].split(",");
            if (arr.length == 1)
                return true;
            return arr.indexOf("" + id) >= 0 || arr.indexOf(" " + item_id) >= 0;
        }
    };
    var check_partially_active = function (item_id, type, parentId) {
        var cat_id;
        var parent_item_id;
        var id;
        var item_is_category = $('#' + item_id).hasClass("category-row");
        if (item_is_category) {
            cat_id = Number($('#' + item_id).parents("tr").first().find(".item-id").first().text());
        } else {
            cat_id = Number($('#' + item_id).parents("tr").first().prev(".category-row").find(".item-id").first().text());
            parent_item_id = parentId.replace('node-tobj-', '');
            id = cat_id + "_" + (type < 3 ? item_id : parent_item_id);
        }

        if (type < 3) {
            if (item_is_category) {
                return check_by_category(partiallyActiveTableObjectIds, new RegExp(cat_id + "_\\d+"));
            } else {
                if (partiallyActiveTableObjectIds.hasOwnProperty(id))
                    return true;
            }
        } else {
            if (TableObjectIds.hasOwnProperty(id)) {
                var idList = TableObjectIds[id].replace(/\s/g, '');
                var arr = idList.split('[');
                arr = arr[1].split(']');
                arr = arr[0].split(',');

                //var $col_row = $("#node-col-" + item_id);
                //var index = $col_row.find(".item-id").first().text();
                var ok = $.inArray(item_id + "", arr);
                return ok > -1;
            }

        }
        return false;
    };

    var set_selected_objects = function () {
        var $not_active_tobjs = jQuery("table#exportables .tobject-row:not(.active,.partially-active) .item-id");
        $not_active_tobjs.each(function (i, t) {
            var cat_id = Number($(t).parents("tr").first().prev(".category-row").find(".item-id").first().text());
            var item_id = jQuery(t).text();
            var id = cat_id + "_" + item_id;

            if (TableObjectIds[id])
                delete TableObjectIds[id];
        });
        var $tobjs = jQuery("table#exportables .tobject-row.active, table#exportables .tobject-row.partially-active");
        $tobjs.each(function (i, t) {
            var $tobj = jQuery(t);
            var cat_id = Number($(t).prev(".category_row").find(".item-id").first().text());
            var item_id = $tobj.find('.item-id').text();
            var id = cat_id + "_" + item_id;
            var cselection = '';
            var $columns = 0;

            if ($tobj.hasClass("partially-active")) {
                $active_columns = $tobj.nextAll('.active.child-of-node-tobj-' + item_id);
                $columns = $tobj.nextAll('.child-of-node-tobj-' + item_id + ":not(.loader-row)");
                if (check_any_column_active($tobj)) {

                    if ($active_columns.length) {
                        $active_columns.each(function (j, c) {
                            if (cselection) cselection += ', ';
                            cselection += jQuery(c).find('.item-id').text();
                        });

                        TableObjectIds[id] = item_id + '[' + cselection + ']';
                    }
                } else {
                    if ($columns.length && TableObjectIds.hasOwnProperty(id)) {
                        delete TableObjectIds[id];
                    } else {
                        TableObjectIds[id] = item_id + "[]";
                    }
                }
            } else {
                TableObjectIds[id] = item_id + "[]";
            }
        });
    };

    var mod_unselectedTableObjectIds = function (node, itemId) {
        if (!node.hasClass("category-row")) {
            var cat_id = Number($("#" + itemId).parents("tr").first().prev(".category-row").find(".item-id").first().text());
            var id = cat_id + "_" + itemId;
            if (!node.hasClass("active")) {
                unselectedTableObjectIds[id] = true;
            } else {
                delete unselectedTableObjectIds[id];
            }
        }
    };



    var get_ids_by_category = function (type, catId) {
        var search = jQuery("#search_string").val();
        var url = "/Export/GetAllTableIdsByCatagoryJson";
        $.ajax({
            type: 'POST',
            url: url,
            data: { type: type, categoryId: catId, search: search },
            success: function (obj) {
                TablesByCategoryObj = obj;
            },
            dataType: "json",
            async: false
        });
    };

    var add_ids_by_category_to_TableObjectIds = function (type, catId) {
        get_ids_by_category(type, catId);

        if (TablesByCategoryObj.hasOwnProperty("idList") && TablesByCategoryObj.idList.length > 0) {
            var idListLength = TablesByCategoryObj.idList.length
            for (var i = 0; i < idListLength; i++) {
                var id = Number(TablesByCategoryObj.categoryId) + "_" + Number(TablesByCategoryObj.idList[i]);
                if (!TableObjectIds.hasOwnProperty(id)) {
                    TableObjectIds[id] = TablesByCategoryObj.idList[i] + "[]";
                }
            }
        }
    };

    var check_any_table_in_category = function ($cat_row) {
        var cat_id = $cat_row.find(".item-id").first().text();
        var categoryGotAnActiveState;

        if (Object.keys(TableObjectIds).length > 0) {
            categoryGotAnActiveState = false;
            if (Object.keys(partiallyActiveTableObjectIds).length) {
                for (var prop in partiallyActiveTableObjectIds) {
                    var prop_cat_id = prop.split('_')[0];
                    if (prop_cat_id == cat_id) {
                        categoryGotAnActiveState = true;
                        break;
                    }
                }
            } else {
                categoryGotAnActiveState = false;
            }

            if (!categoryGotAnActiveState) {
                if (Object.keys(TableObjectIds).length) {
                    for (var prop in TableObjectIds) {
                        var prop_cat_id = prop.split('_')[0];
                        if (prop_cat_id == cat_id) {
                            categoryGotAnActiveState = true;
                            break;
                        }
                    }
                } else {
                    categoryGotAnActiveState = false;
                }
            }
        } else {
            categoryGotAnActiveState = false;
        }
        return categoryGotAnActiveState;
    };

    var check_any_column_active = function ($tobj) {
        $columns = $tobj.nextAll(".child-of-" + $tobj.attr("id"));

        var i = 0;
        var got_one = false;
        while (i < $columns.length && !got_one) {
            if ($("#" + $columns[i].id).hasClass("active")) {
                got_one = true;
            }
            i++;
        }

        if (got_one) {
            return true;
        }
        return false;
    };

    var check_partial_in_TableObjectIds = function (partial_id) {
        return TableObjectIds.hasOwnProperty(partial_id);
    };

    var check_all_tables_in_category_active = function ($cat_row) {
        var $tables_in_category = $cat_row.nextAll(".child-of-" + $cat_row.attr("id") + ":not(.loader-row)");

        var i = 0;
        while (i < $tables_in_category.length && $("#" + $tables_in_category[i].id).hasClass("active")) {
            i++;
        }

        if (i === $tables_in_category.length) {
            var categoryId = $cat_row.find(".item-id").first().text();
            var currentType = $("#CurrentType").val();
            get_ids_by_category(currentType, categoryId);

            var index = 0;
            var tableIsActive = true;
            var idList = TablesByCategoryObj.idList;
            var idListLength = idList.length;
            var id;
            while (index < idListLength && tableIsActive) {
                id = Number(TablesByCategoryObj.categoryId) + "_" + Number(idList[index]);
                if (!TableObjectIds.hasOwnProperty(id)) {
                    tableIsActive = false;
                }

                index++;
            }

            if (index === idListLength) {
                return true;
            }
        }
        return false;
    };

    var check_all_columns_in_table_active = function ($table_row) {
        $columns = $table_row.nextAll(".child-of-" + $table_row.attr("id") + ":not(.loader-row)");

        var i = 0;
        var ok = true;

        while (i < $columns.length && ok) {
            $col = $("#" + $columns[i].id);
            if (!$col.hasClass("active")) {
                ok = false;
                return false;
            }
            i++;
        }

        return ok;
    };

    var flush_memory_objects = function () {
        TableObjectIds = {};
        unselectedTableObjectIds = {};
        partiallyActiveTableObjectIds = {};
    };

    var generating_selection_for_export = function () {
        var selection = '';
        for (var i in TableObjectIds) {
            if (selection) selection += ', ';
            selection += TableObjectIds[i];
        }
        var $cats = jQuery("table#exportables .category-row + .loader-row").prev();
        $cats.each(function (i, c) {
            var $cat = jQuery(c),
                id = $cat.find('.item-id').text(),
                link = $cat.find('a').attr('href'),
                type = link.substring(link.lastIndexOf('=') + 1);
            if ($cat.hasClass('active')) {
                if (selection) selection += ', ';
                selection += id + '{' + type + '}';
            }
        });
        return selection;
    };

    var get_export_dialog = function ($btn) {
        $dlg = $("#dialog");

        set_selected_objects();
        var selection = generating_selection_for_export();

        var btnData = $btn.find('.data').text();
        if (btnData) {
            var filter = '';
            var ids = selection.split(', ');
            for (var i = 0; i < ids.length; i++) {
                var id = ids[i];
                if (id.indexOf("[]") === -1) {
                    if (id.indexOf("[") !== -1 && ids.length > 1) {
                        var tableId = id.split("[")[0];
                        var colId = id.replace("]","").split("[")[1];
                        filter += tableId + '(';
                        if ($("#node-col-" + colId + ' .filterInput').val()) {
                            filter += '`' + colId + '`=' + '\'' + $("#node-col-" + colId + ' .filterInput').val() + '\'';
                        }
                    }
                    else {
                        if (ids.length == 1) {
                            id = id.split("[")[1];
                        }
                        if (id.indexOf("]") !== -1) {
                            var colId = id.replace(']', '');
                            if ($("#node-col-" + colId + ' .filterInput').val()) {
                                if (filter.slice(-1) === '(') {
                                    filter += '`' + colId + '`=' + '\'' + $("#node-col-" + colId + ' .filterInput').val() + '\'';
                                }
                                else {
                                    filter += ' AND `' + colId + '`=' + '\'' + $("#node-col-" + colId + ' .filterInput').val() + '\'';
                                }
                            }
                            filter += ');'
                        }
                        else {
                            var colId = id;
                            if ($("#node-col-" + colId + ' .filterInput').val()) {
                                if (filter.slice(-1) === '(') {
                                    filter += '`' + colId + '`=' + '\'' + $("#node-col-" + colId + ' .filterInput').val() + '\'';
                                }
                                else {
                                    filter += ' AND `' + colId + '`=' + '\'' + $("#node-col-" + colId + ' .filterInput').val() + '\'';
                                }
                            }
                        }
                    }
                }
            }
            jQuery.post('/Export/MassExport', { exports: selection, type: btnData, search: jQuery('#search_right').val(), getAllFields: false, filters: filter }, function (html) {
                if (html.search("login") == -1) {
                    jQuery('#exportjobs').html(html);
                } else {
                    window.location = "/Account/LogOn";
                }
            });

            // flush memory objects
            flush_memory_objects();

            // remove selection
            jQuery('#exportables tr.active').removeClass('active');
            jQuery('#exportables tr.partially-active').removeClass('partially-active');
        }
        $dlg.hide();
    };

    //************** TOOLTIP *********************************
    jQuery(document).tooltip({
        items: ".expander, .expander2",
        content: function () {
            var txt = jQuery("#emptytabletooltip").text();
            var element = jQuery(this);
            var count = element[0].parentElement.nextSibling.nextSibling.textContent;
            if (count == "0")
                return txt;
            else
                return "";
        }
    });
    //********************************************************

    jQuery(document).on("click", "table#exportables .expander", function () {
        var row = $(this).parents('tr');
        var ldr = row.next();
        var pageResource = jQuery("#PageResource952").val();
        if (row.hasClass('category-row')) {
            var id = row[0].id.replace('navigation-row-', '');
            jQuery('.child-of-node-category-' + id + ':not(.loader-row)').remove();
            jQuery('#exportables tbody .column-loader').remove();
            jQuery('.tobject-row:not(.template)').remove();
            jQuery('.column-row:not(.template)').remove();
            jQuery('.navigation-row').hide();
        }
        if (row.hasClass('collapsed') || !ldr.hasClass('loader-row')) {
            $(row).siblings('.loader-row').hide();
            return;
        }
        var url = $(this).nextAll('a').attr('href');
        var name = row.hasClass('category-row') ? 'tobject-row' : 'column-row';
        var search = jQuery('#search_string').val();
        if (search) {
            if (url.indexOf('?') != -1)
                url += '&';
            else
                url += '?';
            url += 'search=' + search;
        }
        var before = row;
        var node = row;
        $.get(url, { json: true }, function (result) {
            if (typeof (result) != 'object') return;
            if (result[0].Name == "warning") {
                $('.column-row').html("");
            }
            var template = $('#exportables .template.' + name);
            var count = 0;
            var begin = 0;

            //*********** empty table expand button close ************ 
            var ccount = row[0].lastElementChild.textContent;
            if (ccount == "0") {
                $(row).removeClass('expanded');
                $(row).addClass('collapsed');
                $(row).siblings('.loader-row').hide();
                return;
            }
            //********************************************************

            if (result.length > 0 && [result.length - 1].Type != 10) {
                count = result[0];
                if (typeof (result[0]) != 'object') {
                    begin = 1;
                } else {
                    begin = 0;
                }
            }
            for (var i = begin, iend = result.length; i < iend; i++) {
                var tbl = result[i];
                var tclass = '', ttype = '';
                switch (tbl.Type) {
                    case 0:
                        tclass = 'issue';
                        ttype = 'tobj';
                        break;
                    case 1:
                        tclass = 'view';
                        ttype = 'tobj';
                        break;
                    case 2:
                        tclass = 'table';
                        ttype = 'tobj';
                        break;
                    case 10:
                        tclass = 'column';
                        ttype = 'col';
                        break;
                }
                var newrow = template.clone().removeClass('template').removeClass('initialized');
                newrow.addClass(name).addClass('child-of-' + row.attr('id'));
                if (check_active(tbl.Id, tbl.Type, row.attr('id'))) {
                    newrow.addClass('active');
                } else if (tbl.Type == 10 && check_partially_active(tbl.Id, tbl.Type, row.attr('id'))) {
                    newrow.addClass('active'); // for columns...
                } else if (check_partially_active(tbl.Id, tbl.Type, row.attr('id'))) {
                    newrow.addClass('partially-active');
                }
                newrow.attr('id', 'node-' + ttype + '-' + tbl.Id);
                before = newrow.insertAfter(before);
                if (tbl.Type < 3) {
                    before = ldr.clone().insertAfter(newrow).attr('class', 'loader-row column-loader child-of-node-' + ttype + '-' + tbl.Id);
                }
                var text = newrow.find('.description .text');
                if (ttype == 'tobj') text.attr('href', text.attr('href') + '/' + tbl.Id);
                text.addClass(tclass).text(tbl.Description);            //.removeClass('filterInput');
                newrow.find('.rowcount').text(tbl.RowCount);
                newrow.find('.filter div').text(tbl.Filter);
                newrow.find('.sort div').text(tbl.Sort);
                newrow.find('.item-id').text(tbl.Id);
            }
            $('#exportables').treeTable();
            row.expand();
            ldr.hide();

            if (node.hasClass("category-row")) {
                var $tobjs = jQuery(node).nextAll(".tobject-row.active");
                if (node.hasClass("active") && !$tobjs.length) node.removeClass("active");
                if (!node.hasClass("active") && !node.hasClass("partially-active") && $tobjs.length) node.addClass("active");
            }
            
            // paging navigation
            if (result.length > 1) {
                var nav = $('#navigation-row-' + tbl.CategoryId);
                var page = 0;
                var displ = $("#DisplayRowCount");
                var perPage = 25;
                try {
                    perPage = parseInt(displ.attr("refvalue"));
                } catch (e) {
                }
                var from = page * perPage;
                var lastPage = Math.floor(perPage == 0 ? 0 : count % perPage == 0 ? (count / perPage) - 1 : (count / perPage));
                var to = Math.min(count, from + perPage) - 1;
                var html = '';
                if (page > 0) {
                    html += '<a class="iconx first"><span class="hide">0</span></a>';
                    html += '<a class="iconx back"><span class="hide">' + (page - 1) + '</span></a>';
                } else {
                    html += '<span class="iconx first"></span>';
                    html += '<span class="iconx back"></span>';
                }
                if (count > 0) {
                    html += '<span class="zaehler"> ' + (from + 1).toLocaleString('de-DE') + '-' + (to + 1).toLocaleString('de-DE') + ' / ' + count.toLocaleString('de-DE');
                }
                html += ' | ' + pageResource + ': <input class="pagecount" id="pageCount2" type="text" value="' + (page + 1) + '"> ' + '</span>';
                if (page < lastPage) {
                    html += '<a class="iconx next"><span class="hide">' + (page + 1) + '</span></a>';
                    html += '<a class="iconx last"><span class="hide">' + (lastPage) + '</span></a>';
                } else {
                    html += '<span class="iconx next"></span>';
                    html += '<span class="iconx last"><input type="hidden" value=' + lastPage + '/></span>';
                }
                html += '<input type="hidden" id="page" value="' + page + '" />';
                if (html.search("login") == -1) {
                    nav.find('.navigation').html(html);
                    nav.show();
                } else {
                    window.location = "/Account/LogOn";
                }
            }
        });
    });
    jQuery(document).on("click", "#navigationId.navigation a.iconx", function () {
        set_selected_objects();
        var $obj = jQuery(this);
        var pageResource = jQuery("#PageResource952").val();
        var page = parseInt($obj.children().first().text());        
        var id = $obj.parents('.navigation-row')[0].id.replace('navigation-row-', '');
        var categoryA = jQuery('#node-category-' + id + ' .description a');
        var url = categoryA.attr('href');
        var index = url.indexOf('page');
        var sub = url.substring(index);
        var arr = sub.split('&');
        var oldPage = arr[0];
        url = url.replace(oldPage, 'page=' + page);
        var sortColumn = jQuery('#sort_column').val();
        var sortDirection = jQuery('#sort_direction').val();
        url += '&sortColumn=' + sortColumn + '&direction=' + sortDirection;
        var search = jQuery('#search_string').val();
        if (search) {
            url += '&search=' + search;
        }
        var row = categoryA.parents('tr');
        jQuery('.child-of-node-category-' + id + ':not(.loader-row)').remove();
        jQuery('#exportables tbody .column-loader').remove();
        jQuery('.column-row:not(.template)').remove();
        var ldr = row.next();
        var name = row.hasClass('category-row') ? 'tobject-row' : 'column-row';
        var before = row;
        var category = row;
        $.get(url, { json: true }, function (result) {
            if (typeof (result) != 'object') return;

            var count = result[0];
            var template = $('#exportables .template.' + name);
            for (var i = 1, iend = result.length; i < iend; i++) {
                var tbl = result[i];
                var tclass = '', ttype = '';
                switch (tbl.Type) {
                    case 0:
                        tclass = 'issue';
                        ttype = 'tobj';
                        break;
                    case 1:
                        tclass = 'view';
                        ttype = 'tobj';
                        break;
                    case 2:
                        tclass = 'table';
                        ttype = 'tobj';
                        break;
                    case 10:
                        tclass = 'column';
                        ttype = 'col';
                        break;
                }
                var newrow = template.clone().removeClass('template').removeClass('initialized');
                newrow.addClass(name).addClass('child-of-' + row.attr('id'));
                if (check_active(tbl.Id, tbl.Type, row.attr('id'))) {
                    newrow.addClass('active');
                } else if (check_partially_active(tbl.Id, tbl.Type, row.attr('id'))) {
                    newrow.addClass('partially-active');
                }
                newrow.attr('id', 'node-' + ttype + '-' + tbl.Id);
                before = newrow.insertAfter(before);
                if (tbl.Type < 3) {
                    before = ldr.clone().insertAfter(newrow).attr('class', 'loader-row column-loader child-of-node-' + ttype + '-' + tbl.Id);
                }
                var text = newrow.find('.description .text');
                if (ttype == 'tobj') text.attr('href', text.attr('href') + '/' + tbl.Id);
                text.addClass(tclass).text(tbl.Description);
                newrow.find('.rowcount').text(tbl.RowCount);
                newrow.find('.filter div').text(tbl.Filter);
                newrow.find('.sort div').text(tbl.Sort);
                newrow.find('.item-id').text(tbl.Id);
                //if (tbl.RowCount == "0" )
                //{
                //    $(this).find('td.description span.expander').addClass('expander2');
                //    $(this).find('td.description span.expander').removeClass('expander');
                //    $("td.description span.expander2").unbind("click");
                //}
            }
            $('#exportables').treeTable();
            row.expand();

            //********* Unbind the click from empty table-row ********* 
            var that = jQuery("table#exportables .expander");
            var rowss = $(that).parents('tr');
            rowss.each(function () {
                var countt = parseInt(this.lastElementChild.textContent);
                if (countt == 0) {
                    $(this).find('td.description span.expander').addClass('expander2');
                    $(this).find('td.description span.expander').removeClass('expander');
                    $("td.description span.expander2").unbind("click");
                }
            });
            //********************************************************

            ldr.hide();
            //var $tobjs = jQuery(category).nextAll(".tobject-row.active");
            //if (category.hasClass("active") && !$tobjs.length) category.removeClass("active");
            //if (!category.hasClass("active") && $tobjs.length) category.addClass("active");
            var nav = $('#navigation-row-' + tbl.CategoryId);
            var displ = $("#DisplayRowCount");
            var perPage = 25;
            try {
                perPage = parseInt(displ.attr("refvalue"));
            } catch (e) {
            }
            var from = page * perPage;
            var lastPage = Math.floor(perPage == 0 ? 0 : count % perPage == 0 ? (count / perPage) - 1 : (count / perPage));
            var to = Math.min(count, from + perPage) - 1;
            var html = '';
            if (page > 0) {
                html += '<a class="iconx first"><span class="hide">0</span></a>';
                html += '<a class="iconx back"><span class="hide">' + (page - 1) + '</span></a>';
            } else {
                html += '<span class="iconx first"></span>';
                html += '<span class="iconx back"></span>';
            }
            if (count > 0) {
                html += '<span class="zaehler"> ' + (from + 1).toLocaleString('de-DE') + '-' + (to + 1).toLocaleString('de-DE') + ' / ' + count.toLocaleString('de-DE') ;
            }
            html += ' | ' + pageResource + ': <input class="pagecount" id="pageCount2" type="text" value="' + (page + 1) + '">' + '</span>';
            if (page < lastPage) {
                html += '<a class="iconx next"><span class="hide">' + (page + 1) + '</span></a>';
                html += '<a class="iconx last"><span class="hide">' + (lastPage) + '</span></a>';
            } else {
                html += '<span class="iconx next"></span>';
                html += '<span class="iconx last"><input type="hidden" value=' + lastPage + '/></span>';
            }
            html += '<input type="hidden" id="page" value="' + page + '" />';
            if (html.search("login") == -1) {
                nav.find('.navigation').html(html);
                nav.show();
            } else {
                window.location = "/Account/LogOn";
            }
        });
    });
    jQuery(document).on("change", "#pageCount2", function () {
        set_selected_objects();
        var $obj = jQuery(this);
        var $obj2 = jQuery("#pageCount2");
        var pageResource = jQuery("#PageResource952").val();
        var page = parseInt($obj2.val());
        if (isNaN(page)) {
            page = 1;
        }
        page = page - 1;
        if (page < 0) {
            page = 0;
        }
        var lastPageInt = parseInt(jQuery(".iconx.last").text());
        if (!isNaN(lastPageInt)) {
            if (page > lastPageInt) {
                page = lastPageInt;
            }
        } else {
            var lastPageHidden = jQuery(".iconx.last").children().first().val();
            lastPageInt = parseInt(lastPageHidden);
            if (page > lastPageInt) {
                page = lastPageInt;
            }
        }
        
        var id = $obj.parents('.navigation-row')[0].id.replace('navigation-row-', '');
        var categoryA = jQuery('#node-category-' + id + ' .description a');
        var url = categoryA.attr('href');
        var index = url.indexOf('page');
        var sub = url.substring(index);
        var arr = sub.split('&');
        var oldPage = arr[0];
        url = url.replace(oldPage, 'page=' + page);
        var sortColumn = jQuery('#sort_column').val();
        var sortDirection = jQuery('#sort_direction').val();
        url += '&sortColumn=' + sortColumn + '&direction=' + sortDirection;
        var search = jQuery('#search_string').val();
        if (search) {
            url += '&search=' + search;
        }
        var row = categoryA.parents('tr');
        jQuery('.child-of-node-category-' + id + ':not(.loader-row)').remove();
        jQuery('#exportables tbody .column-loader').remove();
        jQuery('.column-row:not(.template)').remove();
        var ldr = row.next();
        var name = row.hasClass('category-row') ? 'tobject-row' : 'column-row';
        var before = row;
        var category = row;
        $.get(url, { json: true }, function (result) {
            if (typeof (result) != 'object') return;

            var count = result[0];
            var template = $('#exportables .template.' + name);
            for (var i = 1, iend = result.length; i < iend; i++) {
                var tbl = result[i];
                var tclass = '', ttype = '';
                switch (tbl.Type) {
                    case 0:
                        tclass = 'issue';
                        ttype = 'tobj';
                        break;
                    case 1:
                        tclass = 'view';
                        ttype = 'tobj';
                        break;
                    case 2:
                        tclass = 'table';
                        ttype = 'tobj';
                        break;
                    case 10:
                        tclass = 'column';
                        ttype = 'col';
                        break;
                }
                var newrow = template.clone().removeClass('template').removeClass('initialized');
                newrow.addClass(name).addClass('child-of-' + row.attr('id'));
                if (check_active(tbl.Id, tbl.Type, row.attr('id'))) {
                    newrow.addClass('active');
                } else if (check_partially_active(tbl.Id, tbl.Type, row.attr('id'))) {
                    newrow.addClass('partially-active');
                }
                newrow.attr('id', 'node-' + ttype + '-' + tbl.Id);
                before = newrow.insertAfter(before);
                if (tbl.Type < 3) {
                    before = ldr.clone().insertAfter(newrow).attr('class', 'loader-row column-loader child-of-node-' + ttype + '-' + tbl.Id);
                }
                var text = newrow.find('.description .text');
                if (ttype == 'tobj') text.attr('href', text.attr('href') + '/' + tbl.Id);
                text.addClass(tclass).text(tbl.Description);
                newrow.find('.rowcount').text(tbl.RowCount);
                newrow.find('.filter div').text(tbl.Filter);
                newrow.find('.sort div').text(tbl.Sort);
                newrow.find('.item-id').text(tbl.Id);
            }
            $('#exportables').treeTable();
            row.expand();

            //********* Unbind the click from empty table-row ********* 
            var that = jQuery("table#exportables .expander");
            var rowss = $(that).parents('tr');
            rowss.each(function () {
                var countt = parseInt(this.lastElementChild.textContent);
                if (countt == 0) {
                    $(this).find('td.description span.expander').addClass('expander2');
                    $(this).find('td.description span.expander').removeClass('expander');
                    $("td.description span.expander2").unbind("click");
                }
            });
            //********************************************************

            ldr.hide();
            var $tobjs = jQuery(category).nextAll(".tobject-row.active");
            if (category.hasClass("active") && !$tobjs.length) category.removeClass("active");
            if (!category.hasClass("active") && $tobjs.length) category.addClass("active");
            var nav = $('#navigation-row-' + tbl.CategoryId);
            var displ = $("#DisplayRowCount");
            var perPage = 25;
            try {
                perPage = parseInt(displ.attr("refvalue"));
            } catch (e) {
            }
            var from = page * perPage;
            var lastPage = Math.floor(perPage == 0 ? 0 : count % perPage == 0 ? (count / perPage) - 1 : (count / perPage));
            var to = Math.min(count, from + perPage) - 1;
            var html = '';
            if (page > 0) {
                html += '<a class="iconx first"><span class="hide">0</span></a>';
                html += '<a class="iconx back"><span class="hide">' + (page - 1) + '</span></a>';
            } else {
                html += '<span class="iconx first"></span>';
                html += '<span class="iconx back"></span>';
            }
            if (count > 0) {
                html += '<span class="zaehler"> ' + (from + 1).toLocaleString('de-DE') + '-' + (to + 1).toLocaleString('de-DE') + ' / ' + count.toLocaleString('de-DE');
            }
            html += ' | ' + pageResource + ': <input class="pagecount" id="pageCount2" type="text" value="' + (page + 1) + '">' + '</span>';
            if (page < lastPage) {
                html += '<a class="iconx next"><span class="hide">' + (page + 1) + '</span></a>';
                html += '<a class="iconx last"><span class="hide">' + (lastPage) + '</span></a>';
            } else {
                html += '<span class="iconx next"></span>';
                html += '<span class="iconx last"><input type="hidden" value=' + lastPage + '/></span>';
            }
            html += '<input type="hidden" id="page" value="' + page + '" />';
            if (html.search("login") == -1) {
                nav.find('.navigation').html(html);
                nav.show();
            } else {
                window.location = "/Account/LogOn";
            }
        });
    });
    jQuery(document).on("click", "#exportables .sorting a", function () {
        set_selected_objects();
        var $this = jQuery(this);
        var pageResource = jQuery("#PageResource952").val();
        var th = $this.parent().parent();
        var sortColumn = 0;
        var sortDirection = "Ascending";
        if (th.hasClass("description")) sortColumn = 1;
        if (th.hasClass("rowcount")) sortColumn = 2;
        if ($this.hasClass("descending")) sortDirection = "Descending";
        jQuery('#sort_column').val(sortColumn);
        jQuery('#sort_direction').val(sortDirection);
        var page = jQuery('#page').val();
        var url = $this.attr('href');
        var index = url.indexOf('page');
        var sub = url.substring(index);
        var arr = sub.split('&');
        var oldPage = arr[0];
        url = url.replace(oldPage, 'page=' + page);
        var search = jQuery('#search_string').val();
        if (search)
            url += '&search=' + search;
        var row = jQuery('#exportables tbody tr').first();
        if (row.hasClass('category-row')) {
            var id = row[0].id.replace('navigation-row-', '');
            jQuery('.child-of-' + id + ':not(.loader-row)').remove();
            jQuery('#exportables tbody .column-loader').remove();
            jQuery('.tobject-row:not(.template)').remove();
            jQuery('.column-row:not(.template)').remove();
            jQuery('.navigation-row').hide();
        }
        var ldr = row.next();
        var name = row.hasClass('category-row') ? 'tobject-row' : 'column-row';
        var before = row;
        var category = row;
        $.get(url, { json: true }, function (result) {
            if (typeof (result) != 'object') return;
            var count = result[0];
            var template = $('#exportables .template.' + name);
            for (var i = 1, iend = result.length; i < iend; i++) {
                var tbl = result[i];
                var tclass = '', ttype = '';
                switch (tbl.Type) {
                    case 0:
                        tclass = 'issue';
                        ttype = 'tobj';
                        break;
                    case 1:
                        tclass = 'view';
                        ttype = 'tobj';
                        break;
                    case 2:
                        tclass = 'table';
                        ttype = 'tobj';
                        break;
                    case 10:
                        tclass = 'column';
                        ttype = 'col';
                        break;
                }
                var newrow = template.clone().removeClass('template').removeClass('initialized');
                newrow.addClass(name).addClass('child-of-' + row.attr('id'));
                if (check_active(tbl.Id, tbl.Type, row.attr('id'))) {
                    newrow.addClass('active');
                } else if (check_partially_active(tbl.Id, tbl.Type, row.attr('id'))) {
                    newrow.addClass('partially-active');
                }
                newrow.attr('id', 'node-' + ttype + '-' + tbl.Id);
                before = newrow.insertAfter(before);
                if (tbl.Type < 3) {
                    before = ldr.clone().insertAfter(newrow).attr('class', 'loader-row column-loader child-of-node-' + ttype + '-' + tbl.Id);
                }
                var text = newrow.find('.description .text');
                if (ttype == 'tobj') text.attr('href', text.attr('href') + '/' + tbl.Id);
                text.addClass(tclass).text(tbl.Description);
                newrow.find('.rowcount').text(tbl.RowCount);
                newrow.find('.filter div').text(tbl.Filter);
                newrow.find('.sort div').text(tbl.Sort);
                newrow.find('.item-id').text(tbl.Id);
            }
            $('#exportables').treeTable();
            row.expand();
            ldr.hide();
            var $tobjs = jQuery(category).nextAll(".tobject-row.active");
            if (category.hasClass("active") && !$tobjs.length) category.removeClass("active");
            if (!category.hasClass("active") && $tobjs.length) category.addClass("active");
            var nav = $('#navigation-row-' + tbl.CategoryId);
            var displ = $("#DisplayRowCount");
            var perPage = 25;
            try {
                perPage = parseInt(displ.attr("refvalue"));
            } catch (e) {
            }
            var from = page * perPage;
            var lastPage = Math.floor(perPage == 0 ? 0 : count % perPage == 0 ? (count / perPage) - 1 : (count / perPage));
            var to = Math.min(count, from + perPage) - 1;
            var html = '';
            if (page > 0) {
                html += '<a class="iconx first"><span class="hide">0</span></a>';
                html += '<a class="iconx back"><span class="hide">' + (page - 1) + '</span></a>';
            } else {
                html += '<span class="iconx first"></span>';
                html += '<span class="iconx back"></span>';
            }
            if (count > 0) {
                html += '<span class="zaehler"> ' + (from + 1).toLocaleString('de-DE') + '-' + (to + 1).toLocaleString('de-DE') + ' / ' + count.toLocaleString('de-DE');
            }
            html += ' | ' + pageResource + ': <input class="pagecount" id="pageCount2" type="text" value="' + (page + 1) + '">' + '</span>';
            if (page < lastPage) {
                html += '<a class="iconx next"><span class="hide">' + (page + 1) + '</span></a>';
                html += '<a class="iconx last"><span class="hide">' + (lastPage) + '</span></a>';
            } else {
                html += '<span class="iconx next"></span>';
                html += '<span class="iconx last"><input type="hidden" value=' + lastPage + '/></span>';
            }
            html += '<input type="hidden" id="page" value="' + page + '" />';
            if (html.search("login") == -1) {
                nav.find('.navigation').html(html);
                nav.show();
            } else {
                window.location = "/Account/LogOn";
            }
        });
    });
    jQuery(document).on("click", "table#exportables tbody tr:not(.navigation-row) .description", function (e) {
        if ($(e.target).is('.expander')) return;

        var $node = $(this).parent();
        $node.removeClass("partially-active");
        $node.toggleClass("active");

        var itemId = $(this).children(".item-id").first().text();
        var isActive = $node.is('.active');
        var isCategory = $node.hasClass("category-row");
        var isTable = $node.hasClass("tobject-row");
        var isColumn = $node.hasClass("column-row");

        if (!TableObjectIds.length) {
            var categoryId;
            var currentType = $("#CurrentType").val();
            if (isCategory) {
                categoryId = $node.find(".item-id").first().text();
            } else {
                categoryId = $node.prevAll(".category-row").first().find(".item-id").first().text();
            }
            
            get_ids_by_category(currentType, categoryId);
        }

        if (!isCategory) {
            var cat_row_id = $node.prevAll(".category-row").first().attr("id");
            var $cat_row = $("#" + cat_row_id);

            if (isActive) {
                if (isTable) {
                    set_selected_objects();

                    var areAllTablesActive = check_all_tables_in_category_active($cat_row);

                    if (areAllTablesActive) {
                        $cat_row.removeClass("partially-active");
                        $cat_row.addClass("active");
                    } else {
                        $cat_row.addClass("partially-active");
                    }
                } else if (isColumn) {
                    var table_row_id = $node.prevAll(".tobject-row").first().attr("id");
                    var $table_row = $("#" + table_row_id);

                    var areAllColumnsActive = check_all_columns_in_table_active($table_row);

                    var cat_id = Number($cat_row.find(".item-id").first().text());
                    var tobj_id = Number($table_row.find(".item-id").text());
                    var partially_id = cat_id + "_" + tobj_id;

                    if (areAllColumnsActive) {
                        $table_row.removeClass("partially-active");
                        $table_row.addClass("active");

                        if (partiallyActiveTableObjectIds.hasOwnProperty(partially_id))
                            delete partiallyActiveTableObjectIds[partially_id];

                        var areAllTablesActive = check_all_tables_in_category_active($cat_row);

                        if (areAllTablesActive) {
                            $cat_row.removeClass("partially-active");
                            $cat_row.addClass("active");
                        } else {
                            $cat_row.addClass("partially-active");
                        }
                    } else {
                        $table_row.removeClass("active");
                        $cat_row.removeClass("active");
                        $table_row.addClass("partially-active");
                        $cat_row.addClass("partially-active");

                        partiallyActiveTableObjectIds[partially_id] = true;
                    }
                    set_selected_objects();
                }
            } else {
                $cat_row.removeClass("active");
                $cat_row.addClass("partially-active");
                var cat_id = Number($cat_row.find(".item-id").first().text());

                if (isColumn) {
                    var table_row_id = $node.prevAll(".tobject-row").first().attr("id");
                    var $table_row = $("#" + table_row_id);

                    var tobj_id = Number($table_row.find(".item-id").text());
                    var partially_id = cat_id + "_" + tobj_id;

                    $table_row.removeClass("active");

                    if (check_partial_in_TableObjectIds(partially_id)) {
                        if (check_any_column_active($table_row)) {
                            $table_row.addClass("partially-active");
                            partiallyActiveTableObjectIds[partially_id] = true;
                        } else {
                            $table_row.removeClass("partially-active");
                            delete partiallyActiveTableObjectIds[partially_id];

                            set_selected_objects();

                            if (!check_any_table_in_category($cat_row)) {
                                $cat_row.removeClass("partially-active");
                            } else {
                                $cat_row.addClass("partially-active");
                            }
                        }
                    }
                } else if (isTable) {
                    //check_category_partial($cat_row);
                    set_selected_objects();

                    if (!check_any_table_in_category($cat_row)) {
                        $cat_row.removeClass("partially-active");
                    }
                }
            }
        }

        mod_unselectedTableObjectIds($node, itemId);

        // (de)select all children
        
        var $children = $node.nextAll('.child-of-' + $node.attr('id') + ':not(.loader-row)');
        if ($children.length > 0) {
            if (isActive) {
                $children.removeClass('partially-active');
                $children.addClass('active');

                // get all subnode ids with ajax and populate TableObjectIds with them
                if (isCategory) {
                    var categoryId = $node.find(".item-id").first().text();
                    var currentType = $("#CurrentType").val();
                    add_ids_by_category_to_TableObjectIds(currentType, categoryId);

                    $tables = $node.nextAll(".child-of-" + $node.attr("id"));

                    jQuery.each($tables, function (id, table) {
                        var childId = $(this).find(".item-id").first().text();
                        //mod_unselectedTableObjectIds($(this), childId);

                        var $colchildren = $("#" + table.id).nextAll('.child-of-' + table.id + ':not(.loader-row)');
                        $colchildren.addClass('active');
                    });
                }
            } else {
                $children.removeClass('partially-active');
                $children.removeClass('active');

                for (var prop in TableObjectIds) {
                    if (TableObjectIds.hasOwnProperty(prop) && prop.slice(0, itemId.length) == itemId) {
                        delete TableObjectIds[prop];
                    }
                }

                for (var prop in unselectedTableObjectIds) {
                    if (unselectedTableObjectIds.hasOwnProperty(prop) && prop.slice(0, itemId.length) == itemId) {
                        delete unselectedTableObjectIds[prop];
                    }
                }

                for (var prop in partiallyActiveTableObjectIds) {
                    if (partiallyActiveTableObjectIds.hasOwnProperty(prop) && prop.slice(0, itemId.length) == itemId) {
                        delete partiallyActiveTableObjectIds[prop];
                    }
                }
            }
            jQuery.each($children, function (index, child) {
                var childId = $(this).find(".item-id").first().text();
                //mod_unselectedTableObjectIds($(this), childId);

                var $colchildren = $node.nextAll('.child-of-' + $("#" + child.id).attr("id") + ':not(.loader-row)');
                if (isActive) $colchildren.addClass('active');
                else $colchildren.removeClass('active');
            });
        }

        // sanitize parent state
        $node = $(this);
        do {
            var id = $node.attr('class').match(/child-of-([^ ]+)/);
            if (!id) break;
            $node = $('#' + id[1]).addClass('active');
        } while (1);
    });
    jQuery(document).on("click", "table#exportables a", function () {
        $(this).parents('tr').click();
        return false;
    });
    jQuery(document).on("click", "#dialog .dialog-box .dialog-buttons .dialog-btn", function () {
        get_export_dialog($(this));
    });
    jQuery(document).on("click", ".mitte a", function () {
        set_selected_objects();
        var selection = generating_selection_for_export();

        if (selection) {
            jQuery.post('/Export/ExportTypeSelection', { exports: selection }, function (html) {
                var $dlg = jQuery('#dialog');
                if (html.search("login") == -1) {
                    $dlg.html(html).show();
                } else {
                    window.location = "/Account/LogOn";
                }
            });
        }
        return false;
    });
    
    var exportTimer = 0;
    var exportKeyUpFunction = (function () {
        clearTimeout(exportTimer);
        //hier noch die wahl der category verbessern -> category ausgeklappt heißt es wird in dieser gesucht! etc...
        cat = jQuery('.category-row').first();
        jQuery('#search_string').val(jQuery('#searchoverview_export input[type=text]').val());
        if (!cat.hasClass('collapsed'))
            cat.find('.expander').click();
        cat.find('.expander').click();
    });
    jQuery(document).on("keyup", "#searchoverview_export input[type=text]", function () {
        clearTimeout(exportTimer);
        exportTimer = setTimeout(exportKeyUpFunction, 1000);
    });
    var exportJobTimer = 0;
    var exportJobKeyUpFunction = (function () {
        clearTimeout(exportJobTimer);
        jQuery('#search_right').val(jQuery('#search_export_right input[type=text]').val());
        jQuery.get('/Export/ExportJobs', { search: jQuery('#search_export_right input[type=text]').val() }, function (html) {
            if (html.search("login") == -1) {
                jQuery('#exportjobs').html(html);
            } else {
                window.location = "/Account/LogOn";
            }
        });
    });
    jQuery(document).on("keyup", "#search_export_right input[type=text]", function () {
        clearTimeout(exportJobTimer);
        exportJobTimer = setTimeout(exportJobKeyUpFunction, 1000);
    });
    /**************************************************************************
    * SEARCH - END
    **************************************************************************/
    //erste category immer ausklappen!
    jQuery('.category-row .expander').click();
    var refresh = function () {
        jQuery.get('/Export/RefreshJobs', { search: jQuery('#search_right').val() }, function (html) {
            jQuery('#exportjobs').html(html);

            var tooltip = jQuery(this).find('.tooltip-container');
            var body_tooltip = jQuery('body > .tooltip-container');
            if (body_tooltip.length > 0) tooltip = body_tooltip.remove();
            if (!tooltip.hasClass('hide')) tooltip.addClass('hide');
        });
        setTimeout(refresh, 3000);
    };
    setTimeout(refresh, 1500);
    /**************************************************************************
    * EXPORT CENTER - JOBS löschen
    **************************************************************************/
    jQuery(document).on("click", "#exportjobs .overview .action .delete", function () {
        var $delete = jQuery(this);
        jQuery.get("/Export/DeleteExport", { key: $delete.find('.key').text() }, function () {
            $delete.parent().parent().parent().remove();

            var tooltip = jQuery(this).find('.tooltip-container');
            var body_tooltip = jQuery('body > .tooltip-container');
            if (body_tooltip.length > 0) tooltip = body_tooltip.remove();
            if (!tooltip.hasClass('hide')) tooltip.addClass('hide');
        });
    });
    /**************************************************************************
    * EXPORT CENTER - JOBS download
    **************************************************************************/
    jQuery(document).on("click", "#exportjobs .overview .action .submit-download", function () {
        var tooltip = jQuery(this).find('.tooltip-container');
        var body_tooltip = jQuery('body > .tooltip-container');
        if (body_tooltip.length > 0) tooltip = body_tooltip.remove();
        if (!tooltip.hasClass('hide')) tooltip.addClass('hide');
    });
    /**************************************************************************
    * JOBS TAB
    **************************************************************************/
    jQuery(document).on("click", ".export-right .tabheader", function () {
        if (jQuery(this).hasClass('selected')) return;
        var new_block = jQuery(this).attr("class").replace('tabheader', '').replace(' ', '');
        var old_block = jQuery(this).blur().addClass('selected').removeAttr('href')
            .siblings('.selected').removeClass('selected').attr('href', '#').attr('class').replace('tabheader', '').replace(' ', '');
        $(jQuery(this).parent().parent().find('.jobs')[1]).removeClass(old_block).addClass(new_block);
        return false;
    });
});