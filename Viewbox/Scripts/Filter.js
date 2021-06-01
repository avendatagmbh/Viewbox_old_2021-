//$(document).ready(function () {
jQuery(function() {
    $.jstree._themes = "/Content/jquery-jstree/themes/";
    jQuery(".filter-subtree ul:first").treeview({
        control: "#treecontrol"
    });
    jQuery('.hitarea').remove();
    //Delete plus for the first condition
    jQuery('.filter-subtree li:first').css('background-image', 'none');
    //jQuery('.op-number-box').removeClass('hide');
    //    jQuery(".filter-subtree ul:first").treeview({
    //        control: "#treecontrol",
    //        persist: "cookie",
    //        cookieId: "treeview-black"
    //    });
    //    //    jQuery(".filter-subtree")
    //        // call `.jstree` with the options object
    //        .jstree({
    //            "themes": {
    //                "theme": "default",
    //                "icons": false
    //            },
    //            // the `plugins` array allows you to configure the active plugins on this instance
    //            "plugins": ["themes", "html_data", "ui", "crrm"],
    //            // each plugin you have included can have its own config object
    //            "core": { "initially_open": ["phtml_1"] }
    //        // it makes sense to configure a plugin only if overriding the defaults
    //        });
});