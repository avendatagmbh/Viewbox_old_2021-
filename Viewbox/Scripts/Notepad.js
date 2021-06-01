jQuery(function() {
    /**************************************************************************
    * Notepad
    **************************************************************************/
    jQuery(document).on("click", "#notepad", function (e) {
        e.stopPropagation();
    });
    jQuery(document).on("click", "#notepad .close-popup", function (e) {
        jQuery(document).trigger('click.notepad-container');
    });
    jQuery(document).on("click", ".boxheader #right .button.note", function () {
        jQuery(document).trigger('click.notepad-container');
        var $notepad = jQuery('#notepad').fadeIn();
        jQuery(document).bind('click.notepad-container', function(e) {
            if (e.isPropagationStopped()) return;
            $notepad.fadeOut(100);
            jQuery(document).unbind('click.notepad-container');
        });
        return false;
    });
    jQuery(document).on("click", "#notepad .add", function (e) {
        var replace = jQuery('#notepad .box.left .replace');
        var new_entry = replace.clone().insertBefore(replace).removeClass('replace');
        jQuery('#notepad .box.left .inner-box .content').each(function(i, e) {
            jQuery(e).removeClass('active').addClass('inactive');
        });
        var entry_before = new_entry.prev('.inner-box');
        var new_entry_date = new_entry.find('.head .right').text();
        if (entry_before.length > 0 && entry_before.find('.head .right').text() == new_entry_date) {
            new_entry.find('.head').addClass('hide');
        }
        var entry_title = new_entry.find('.content').addClass('active').removeClass('inactive').find('.title').text();
        jQuery('#notepad .right-side .headline').attr('contenteditable', 'true').text(entry_title);
        jQuery('#notepad .right-side .date').text(new_entry_date);
        jQuery('#notepad .right-side .content-inner .text').attr('contenteditable', 'true').html('').removeClass('normal');
        jQuery.post("/Notepad/SaveNote", { id: 0, text: '', title: entry_title }, function(note_id) {
            new_entry.find('.item-id').text(note_id);
        });
        return false;
    });
    jQuery(document).on("keyup", "#notepad .right-side .headline", function () {
        jQuery('#notepad .left-side .content-inner .content.active .title').text(jQuery(this).text());
        return false;
    });
    jQuery(document).on("keyup", "#notepad .content .left-side .inner-box .content .title", function () {
        jQuery('#notepad .right-side .headline').text(jQuery(this).text());
        return false;
    });
    jQuery(document).on("click", "#notepad .left-side .content-inner .content.inactive", function () {
        jQuery('#notepad .box.left .inner-box .content').each(function (i, e) {
            jQuery(e).removeClass('active').addClass('inactive');
        });
        var entry_title = jQuery(this).addClass('active').removeClass('inactive').find('.title').text();
        jQuery('#notepad .right-side .headline').attr('contenteditable', 'true').text(entry_title);
        jQuery('#notepad .right-side .date').text(jQuery(this).prev().find('.right').text());
        var textArea = jQuery('#notepad .right-side .content-inner .text').attr('contenteditable', 'true').removeClass('normal');
        var id = jQuery(this).next().text();
        if (!id.length) id = 0;
        jQuery.get("/Notepad/GetNoteText", { id: id }, function(text) {
            textArea.html(text);
        });
        return false;
    });
    jQuery(document).on("blur", "#notepad .content .right-side .text, #notepad .content .right-side .headline, #notepad .content .left-side .inner-box .content .title", function () {
        var note = jQuery('#notepad .left-side .content-inner .content.active');
        if (note.length) {
            var id = note.next().text();
            if (!id.length) id = 0;
            jQuery.post("/Notepad/SaveNote", { id: id, text: jQuery('#notepad .right-side .content-inner .text').html(), title: note.find('.title').text() }, function(note_id) {
                note.next().text(note_id);
            });
        }
    });
    jQuery(document).on("click", "#notepad .delete", function () {
        var note = jQuery(this).parent();
        if (note.length) {
            var id = note.next().text();
            if (!id.length) id = 0;
            jQuery.get("/Notepad/DeleteNote", { id: id }, function() {
                jQuery('#notepad .right-side .content-inner .text').attr('contenteditable', 'false').addClass('normal').html('');
                jQuery('#notepad .right-side .headline').attr('contenteditable', 'false').text('');
                jQuery('#notepad .right-side .date').text('');
                var $box = note.parent();
                if (!$box.find('.head').hasClass('hide')) {
                    var $next_head = $box.next().find('.head');
                    if ($next_head.hasClass('hide')) $next_head.removeClass('hide');
                }
                $box.remove();
            });
        }
    });
});