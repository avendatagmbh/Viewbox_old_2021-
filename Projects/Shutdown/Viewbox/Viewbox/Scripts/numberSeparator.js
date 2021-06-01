(function($) {
    $.fn.getCursorPosition = function() {
        var input = this.get(0);
        if (!input) return; // No (input) element found
        if ('selectionStart' in input) {
            // Standard-compliant browsers
            return input.selectionStart;
        } else if (document.selection) {
            // IE
            input.focus();
            var sel = document.selection.createRange();
            var selLen = document.selection.createRange().text.length;
            sel.moveStart('character', -input.value.length);
            return sel.text.length - selLen;
        }
    };
    // Behind the scenes method deals with browser
    // idiosyncrasies and such
    $.caretTo = function(el, index) {
        if (el.createTextRange) {
            var range = el.createTextRange();
            range.move("character", index);
            range.select();
        } else if (el.selectionStart != null) {
            el.focus();
            el.setSelectionRange(index, index);
        }
    };
    // The following methods are queued under fx for more
    // flexibility when combining with $.fn.delay() and
    // jQuery effects.
    // Set caret to a particular index
    $.fn.caretTo = function(index, offset) {
        return this.queue(function(next) {
            if (isNaN(index)) {
                var i = $(this).val().indexOf(index);
                if (offset === true) {
                    i += index.length;
                } else if (offset) {
                    i += offset;
                }
                $.caretTo(this, i);
            } else {
                $.caretTo(this, index);
            }
            next();
        });
    };
    // Set caret to beginning of an element
    $.fn.caretToStart = function() {
        return this.caretTo(0);
    };
    // Set caret to the end of an element
    $.fn.caretToEnd = function() {
        return this.queue(function(next) {
            $.caretTo(this, $(this).val().length);
            next();
        });
    };

    function commafy(val, lang) {

            return String(val).split("").reverse().join("")
                .replace(/(.{3}\B)/g, "$1" + thousandSeparators)
                .split("").reverse().join("");
    }

    $.fn.numberWithSeparators = function () {
        return this.bind("keyup", function () {

            var posofcaret = $(this).getCursorPosition();
            var number = $(this).val();
            var a = number.split(decimalSeparators);
            var integer = a[0];
            //Check for negative sign
            var signed = (integer.indexOf("-") == 0);
            if (signed) integer = integer.replace(/-/g, "");
            var comaBeginCount = integer.split(thousandSeparators).length;
            integer = integer.split(thousandSeparators).join("");
            var endvalue = commafy(integer, thousandSeparators);
            if (a.length > 1) {
                for (var i = 1; i < a.length; ++i)
                    endvalue += decimalSeparators + a[i];
            }
            var comaEndCount = endvalue.split(thousandSeparators).length;
            if (comaEndCount > comaBeginCount) posofcaret++;
            if (comaEndCount < comaBeginCount) posofcaret--;
            if (signed) endvalue = "-" + endvalue;
            $(this).val(endvalue);
            $(this).caretTo(posofcaret);

        }).bind("keydown", function (e) {
            var key = e.charCode || e.keyCode || 0;

            if ((thousandSeparators == "," && (key == 110 || key == 188)) || (thousandSeparators == "." && key == 190)) return; else
            if (key == 46 || key == 8 || key == 9 || key == 27 || key == 13 || key == 109 || key == 173  || key == 189 || key == 191) return;
            else if (key == 65 && e.ctrlKey === true) {
                return;
            } else if (key >= 35 && key <= 39) {
                return;
            } else {
                if (e.shiftKey || ((key < 48 || key > 57) && (key < 96 || key > 105) && key != 188 && key != 110 && key != 190)) {
                    e.preventDefault();
                }
            }
        });
    };
    $.fn.numberWithSeparators2 = function() {
        var number = $(this).html();
        var a = number.split(",");
        var integer = a[0];
        //Check for negative sign
        var signed = (integer.indexOf("-") == 0);
        if (signed) integer = integer.replace( /-/g , "");
        integer = integer.split(".").join("");
        var endvalue = commafy(integer);
        if (a.length > 1) {
            for (var i = 1; i < a.length; ++i)
                endvalue += "," + a[i];
        }
        if (signed) endvalue = "-" + endvalue;
        $(this).html(endvalue);

    };
}(jQuery));