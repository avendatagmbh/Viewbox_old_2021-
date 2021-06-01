$(document).ready(function() {
    var refresh = function() {
        var $field = jQuery('#searchoverview-jobs').find('.field');
        var value = $field.hasClass('empty') ? '' : $field.val();
        jQuery.get('/ControlCenter/RefreshJobs', { search: value }, function(html) {
            if (html.search("login") == -1) {
                jQuery('#ControlCenter .content-box .content-inner.content-right .overview.jobs').html(html);
            } else {
                window.location = "/Account/LogOn";
            }
        });
        setTimeout(refresh, 1500);
    };
    setTimeout(refresh, 1500);
    /**************************************************************************
    * CONTROL CENTER - JOBS TAB
    **************************************************************************/
    jQuery(document).on("click", "#ControlCenter .content-right .tabheader", function () {
        if (jQuery(this).hasClass('selected')) return;
        var new_block = jQuery(this).attr("class").replace('tabheader', '').replace(' ', '');
        var old_block = jQuery(this).blur().addClass('selected').removeAttr('href')
            .siblings('.selected').removeClass('selected').attr('href', '#').attr('class').replace('tabheader', '').replace(' ', '');
        jQuery(this).parent().next('.jobs').removeClass(old_block).addClass(new_block);
        return false;
    });
    jQuery(document).on("click", "#ControlCenter .overview .action .delete", function () {
        var $delete = jQuery(this);
        jQuery.get("/ControlCenter/DeleteTransformation", { key: $delete.find('.key').text() }, function() {
            $delete.parent().parent().parent().remove();
        });
    });

    var currentYearRange = jQuery("#currentYearRange").val();
    jQuery('#ControlCenter .actionfield.date').datepicker({
        showOtherMonths: true,
        selectOtherMonths: true,
        changeMonth: true,
        changeYear: true,
        yearRange: currentYearRange,
        onSelect: function(dateText) {
            var dateFormat = jQuery('#ControlCenter .actionfield.date').datepicker("option", "dateFormat");
            var today = new Date();
            if ($.datepicker.parseDate(dateFormat, dateText) > today) {
                jQuery('#ControlCenter .actionfield.date').datepicker("disable");
                jQuery('#ControlCenter .actionfield.date').datepicker("setDate", today);
                jQuery('#ControlCenter .actionfield.date').datepicker("enable");
            }
        }
    });
});