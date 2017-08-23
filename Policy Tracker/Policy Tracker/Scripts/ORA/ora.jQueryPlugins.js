(function ($) {
    $.fn.ActionLink = function (options) {
        options = (typeof options === 'undefined') ? {} : options;
        options.confirmCallback = (typeof options.confirmCallback === 'undefined') ? function () { } : options.confirmCallback;
        options.beforeSendCallback = (typeof options.beforeSendCallback === 'undefined') ? function () { } : options.beforeSendCallback;
        options.completeCallback = (typeof options.completeCallback === 'undefined') ? function () { } : options.completeCallback;
        

        $(this).click(function () {
            options.obj = $(this);
            options.url = $(this).attr("act-loc");

            if (typeof options.successCallback === 'undefined') {
                options.updateTarget = "#" + $(this).attr("UpdateTarget");
                options.successCallback = function (data) { $(options.updateTarget).html(data); }
            }

            if ("" + options.confirmCallback == "" + function () { }) {
                ResponseNG.Ajax.ActionLink(
                {
                    url: options.url,
                    type: "GET",
                    beforeSendCallback: options.beforeSendCallback,
                    completeCallback: options.completeCallback,
                    successCallback: options.successCallback
                });
            } else {
                options.confirmCallback(options);
            }
        });
        return this;
    };
}(jQuery));

(function ($) {
    $.fn.ActionDialog = function () {
        var url = $(this).attr("ContentURL");

        $(this).click(function () {
            ResponseNG.Dialog.SimpleAjax(
                {
                    id: "test", title: "test", url: url
                }
            );
        });

        return this;
    };
}(jQuery));

(function ($) {
    $.fn.currency = function (options) {
        $(this).inputmask("decimal", { digits: 2, allowMinus: false });

        return this;
    };
}(jQuery));

//https://raw.github.com/spencertipping/jquery.fix.clone/master/jquery.fix.clone.js
(function (original) {
    jQuery.fn.clone = function () {
        var result = original.apply(this, arguments),
            my_textareas = this.find('textarea').add(this.filter('textarea')),
            result_textareas = result.find('textarea').add(result.filter('textarea')),
            my_selects = this.find('select').add(this.filter('select')),
            result_selects = result.find('select').add(result.filter('select'));

        for (var i = 0, l = my_textareas.length; i < l; ++i) $(result_textareas[i]).val($(my_textareas[i]).val());
        for (var i = 0, l = my_selects.length; i < l; ++i) result_selects[i].selectedIndex = my_selects[i].selectedIndex;

        return result;
    };
})(jQuery.fn.clone);

(function ($) {
    $.fn.defaultZero = function () {
        $(this).blur(function () {
            var val = $(this).val();
            if (val == null || val == ""){
                $(this).val(0);
            }
        });
        return this;
    };
}(jQuery));

(function ($) {
    $.fn.SelectListAdd = function () {
        var origin = $(this).attr("origin");
        var target = $(this).attr("target");
        var type = $("#" + origin).attr("type");

        if (type != "text") {
            $(this).click(function () {
                var selectedOpts = $("#" + origin + " option:selected");
                if (selectedOpts.length == 0) {
                    return false;
                }

                // Guard Against Duplicates
                var exists = $("#" + target + " option[value='" + selectedOpts.val() + "']");
                console.log(exists);
                if (exists.length == 0) {
                    $("#" + target).append($(selectedOpts).clone());
                }
            });
        }
        else {
            $(this).click(function () {
                var selectedOpt = $("#" + origin).val().toUpperCase();
                if (selectedOpt.length == 0) {
                    return false;
                }

                // Guard Against Duplicates
                var exists = $("#" + target + " option[value='" + selectedOpt + "']");
                selectedOpt = selectedOpt.toUpperCase();
                if (exists.length == 0) {
                    $("#" + target).append($('<option/>', { value: selectedOpt, text: selectedOpt }));
                }
            });
        }

        return this;
    };
}(jQuery));

(function ($) {
    $.fn.SelectListRemove = function () {
        var list = $(this).attr("list");;
        $(this).click(function () {
            $("#" + list + " option:selected").remove();
        });
        return this;
    };
}(jQuery));

(function ($) {
    $.fn.SelectListClear = function () {
        var list = $(this).attr("list");
        
        $(this).click(function () {
            ora.Dialog.Confirm({
                id: "ConfirmDialogBox",
                message: "Are you sure you want to clear the list?",
                AcceptCallback: function () {
                    $("#" + list).empty();
                }
            })
        });
        return this;
    };
}(jQuery));

/*
    Gives a checkbox control over showing and hiding another element on screen. Set the checkbox data-control-selector to the   
    jQuery selector needed to find the element needing to be toggled. The element will be hidden or shown on initialization
    based on the current check state of the checkbox.
*/
(function ($) {
    var showElement = function (selector) {
        $(selector).show();
    }

    var hideElement = function (selector) {
        $(selector).hide();
    }

    $.fn.CheckBoxControl = function () {
        this.each(function () {
            var selector = $(this).attr("data-control-selector");
            if ($(this).is(":checked")) {
                showElement(selector);
            } else {
                hideElement(selector);
            }
        });

        return this.each(function (o) {
            $(this).change(function () {
                var selector = $(this).attr("data-control-selector");
                if ($(this).is(":checked")) {
                    showElement(selector);
                } else {
                    hideElement(selector);
                }
            });
        });
    };
}(jQuery));
