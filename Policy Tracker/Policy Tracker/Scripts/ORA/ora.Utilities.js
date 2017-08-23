(function (appUtilities, $, undefined) {
    var _toastSelector = "#globalToast";

    appUtilities.formatString = function (string, params) {
        if (params.length) {
            for (var i = 0; i < params.length; i++) {
                console.log(params[i]);
                string = string.replace('{' + i + '}', params[i].value);
            }
        }

        return string;
    };
    appUtilities.isNullOrWhitespace = function (input) {
        if (typeof input === 'undefined' || input == null) return true;
        return input.replace(/\s/g, '').length < 1;
    };

    /*
        Toast Popup Message
        Types: 'info' | 'warning' | 'error' | 'success' | 'custom'
    */
    appUtilities.toast = function (msg, type) {
        if (type === 'undefined')
            type = 'info';

        $(_toastSelector).dxToast('instance').option('message', msg);
        $(_toastSelector).dxToast('instance').option('type', type);
        $(_toastSelector).dxToast("instance").show();
    };
    appUtilities.firstOrDefault = function(arr, key, val) {
        for (var i = 0, iLen = arr.length; i < iLen; i++) {
            if (arr[i][key] == val) return arr[i];
        }
    };
    appUtilities.where = function(arr, key, value) {
        var result = [];
        arr.forEach(function (o) { if (o[key] == value) result.push(o); });
        return result ? result : null; // or undefined
    };
}(window.appUtilities = window.appUtilities || {}, jQuery));


(function(webAPI, $, undefined) {
    webAPI.submitForm = function(form, url, data) {
        $.ajax({
                method: "POST",
                url: url,
                data: data
            })
            .done(function(data, textStatus, jqXHR) {
                $(form)
                    .find("[class=validation-summary-errors]")
                    .removeClass("validation-summary-errors")
                    .addClass("validation-summary-valid");
                $(form)[0].reset();
                appUtilities.toast("Success!", "success");
            })
            .fail(function(jqXHR, textStatus, error) {
                var errorList = $("<ul>");
                for (var i = 0, l = jqXHR.responseJSON.Error.length; i < l; ++i) {
                    errorList.append("<li>" + jqXHR.responseJSON.Error[i] + "</li>");
                }
                $(form).find("[class^=validation-summary]").html("");
                $(form).find("[class^=validation-summary]").html(errorList);
                $(form)
                    .find("[class^=validation-summary]")
                    .removeClass("validation-summary-valid")
                    .addClass("validation-summary-errors");
                appUtilities.toast("Fail!", "error");
            });
    };
}(window.webAPI = window.webAPI || {}, jQuery));


ora.Utilities = {
    MergeOptionObjects: function (obj, dialogDefaults) {
        for (i in dialogDefaults) {
            if (!obj.hasOwnProperty(i)) {
                obj[i] = dialogDefaults[i];
            }
        }

        return obj;
    },

    URIEncode: function (uri) {
        return encodeURIComponent(uri);
    },

    UnMaskFormValues: function (formId) {
        $("#" + formId).find("input.masked").each(function () {
            var value = $(this).inputmask('unmaskedvalue');

            $(this).val(value);
            $(this).attr("value", value);
        });
    },

    RegisterFormSubmit: function (formInfo) {
        formInfo.closeOnSave = (typeof formInfo.closeOnSave === 'undefined') ? true : formInfo.closeOnSave;
        formInfo.successCallback = (typeof formInfo.successCallback === 'undefined') ? function (data) { } : formInfo.successCallback;

        $("#" + formInfo.id).submit(function (event) {
            event.preventDefault();
            $("form#" + formInfo.id + " .selectList option").attr("selected", "selected");
            ora.Ajax.FormPost(
                {
                    url: formInfo.submitURL,
                    container: formInfo.container,
                    grid: formInfo.grid,
                    dialog: formInfo.dialog,
                    form: formInfo.id,
                    data: $("#" + formInfo.id).serializeArray(),
                    closeOnSave: formInfo.closeOnSave,
                    successCallback: formInfo.successCallback
                }
            );
        });
    },

    RegisterFormValidation: function (formInfo) {
        $.validator.unobtrusive.parse("#" + formInfo.id);
        $('#' + formInfo.id).areYouSure({ 'silent': true });
    },

    RegisterInputEvents: function (form) {
        $("form#" + form + " .datepicker").datepicker();
        //$("form#" + form + " .datepicker").datepicker({ showOn: 'both', buttonImageOnly: false }).next('.ui-datepicker-trigger').attr("tabIndex", "-1");
        //$("form#" + form + " .ui-datepicker-trigger").button({ icons: { primary: "ui-icon-calendar" }, text: false }).addClass('datepickerButton');
        $("form#" + form + " .numeric").numeric({ negative: true });
        $("form#" + form + " .integer").numeric({ decimal: false, negative: false });
        //$("form#" + form + " .ui-integer, .ui-numeric").defaultZero();
        $("form#" + form + " .button").button();
        $("form#" + form + " .spinner").spinner();
        $("form#" + form + " .searchButton").button({ icons: { primary: "ui-icon-circle-zoomin" }, text: false });
    },

    Warnings: {
        Add: function (cont, msg) {
            $("#" + cont).find('#' + msg.Id).remove();
            $("#" + cont).append('<p id="' + msg.Id + '"><span class="ui-icon ui-icon-info" style="float: left; margin-right: .3em; margin-top: .1em"></span><b>' + msg.Msg + '</b></p>');
            this.ShowWarnings(cont);
        },
        Remove: function (cont, msg) {
            $("#" + cont).find('#' + msg).remove();
            this.ShowWarnings(cont);
        },
        HideMsg: function (cont) {
            $(cont).find('#' + msg).hide();
        },
        ShowMsg: function (cont) {
            $("#" + cont).find('#' + msg).show();
        },
        HideWarnings: function (cont) {
            $("#" + cont).hide();
        },
        ShowWarnings: function (cont) {
            var total = parseInt($("#" + cont).find('p').size());
            if (total > 0) { $("#" + cont).show(); }
            else { $("#" + cont).hide(); }
        },
        Clear: function (cont) {
            $("#" + cont).each("p").remove();
        }
    }
};