$(document).ready(function () {

    Loading();
    
    jQuery(document).on("click", ".theTree .dirLink", function () {
        var that = $(this);
        var item = that.find("div.item-id");
        var value = item.first().text();
        var selectedDir = jQuery(".selectedDirectory");

        if (selectedDir.length > 0) {
            selectedDir.siblings("img").attr('src', '../../Content/images/folder_grey.png');
            selectedDir.css("font-weight", "normal");
            selectedDir.removeClass("selectedDirectory");
        }

        that.css("font-weight", "bold");
        that.addClass("selectedDirectory");
        that.siblings("img").attr('src', '../../Content/images/folder_blue.png');

        jQuery.get("FileSystem/ShowFiles?id=" + value, function (data) {
            jQuery("#FilesContainer").html(data);
         });
    });

    jQuery(document).on("click", ".clickHere", function () {
        CollapseOrExpand($(this));
    });

    //jQuery(document).on("input", "#folderFilter", function () {
    //    setTimeout(function () {
    //        var filter = jQuery("#folderFilter").val();
    //        if (filter != "") {
    //            jQuery.get("FileSystem/FilterDirectories?filter=" + filter, function (data) {
    //                jQuery("#DirectoriesContainer").html(data);
    //                jQuery("#folderFilter").val(filter);
    //            });
    //        }
    //        else {
    //            jQuery.get("FileSystem/FilterDirectories", function (data) {
    //                jQuery("#DirectoriesContainer").html(data);
    //                jQuery("#folderFilter").val(filter);
    //            });
    //        }
    //    }, 1000);
    //});

    //jQuery(document).on("input", "#fileFilter", function () {
    //    setTimeout(function () {
    //        var filter = jQuery("#fileFilter").val();
    //        if (filter != "") {
    //            jQuery.get("FileSystem/FilterFiles?filter=" + filter, function (data) {
    //                var x = jQuery(data);
    //                jQuery("#FilesContainer").html(data);
    //                jQuery("#fileFilter").val(filter);
    //            });
    //        }
    //        else {
    //            jQuery.get("FileSystem/FilterFiles", function (data) {
    //                jQuery("#FilesContainer").html(data);
    //                jQuery("#fileFilter").val(filter);
    //            });
    //        }
    //    }, 1000);
    //});

    jQuery(document).on("input", "#fileFilter", function () {
        setTimeout(function () {
            var filter = jQuery("#fileFilter").val();
            if (filter != "") {
                jQuery.get("FileSystem/FilterAll?filter=" + filter, function (data) {
                    FillInResultsOfFilter(data, filter);
                });
            }
            else {
                jQuery.get("FileSystem/FilterAll", function (data) {
                    FillInResultsOfFilter(data, filter);
                });
            }
        }, 1000);
    });

});
    function Loading()
    {
        $('.loading-container').css('z-index', 1000).fadeIn(500, function () {
            $('.loading-container').css('z-index', -1000).fadeOut(500).addClass("hide", function () {
                $('.content-inner .left, .mitte').fadeIn(500).removeClass("hide");
                setDirTableBody();
            });
        });
    }

    function FillInResultsOfFilter(data, filter)
    {
        var files = jQuery(data).find("div[id=FilesContainer]");
        var dirs = jQuery(data).find("div[id=DirectoriesContainer]");
        files.removeClass("hide");
        dirs.removeClass("hide");
        jQuery("#DirectoriesContainer").html(dirs);
        jQuery("#FilesContainer").html(files);
        jQuery("#fileFilter").val(filter);
    }

    function setDirTableBody() {
        var displayHeight = $("#DirectoriesContainer").height() - ($(".header").height() + 1);
        $(".treebody").height(displayHeight);
    }

    function CollapseOrExpand(that)
    {
        var child = that.siblings('ul');
        if (child.hasClass("closed")) {
            child.fadeIn(500, function () {
                child.removeClass("closed")
                that.css("background", "url(Content/img/treeview/toggle-collapse-dark.png) 0 0 no-repeat");
            });
        }
        else {
            child.fadeOut(500, function () {
                child.addClass("closed");
                that.css("background", "url(Content/img/treeview/toggle-expand-dark.png) 0 0 no-repeat");
            });
        }
    }

    //function Expand(that)
    //{
    //    var par = that.siblings('div');
    //    if (that.hasClass("closed")) {
    //        that.fadeIn(500, function () {
    //            that.removeClass("closed")
    //            par.css("background", "url(Content/img/treeview/toggle-collapse-dark.png) 0 0 no-repeat");
    //        });
    //    }
    //}