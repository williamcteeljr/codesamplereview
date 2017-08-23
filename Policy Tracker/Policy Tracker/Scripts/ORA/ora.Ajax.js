ora.Ajax = {
    Type: { Ajax: "Ajax", FormPost: "FormPost", ActionLink: "ActionLink" },

    OpenLoading: function (message) {
        //message = (typeof message === 'undefined') ? "" : message;
        //var ajaxLoader = '<div id="ajax-loader" class="ui-front ui-ajax-loader"><img src=\"../Images/loader.gif\" /><p>' + message + '</p></div>'
        //$("#mainContent").append(ajaxLoader);
        DXloadingPanel.Show();
    },

    CloseLoading: function () {
        //$("#ajax-loader").remove();
        DXloadingPanel.Hide();
    },

    ContentLoading: function () {
        //message = (typeof message === 'undefined') ? "" : message;
        //var ajaxLoader = '<div id="ajax-loader" class="ui-front ui-ajax-loader"><p>... Loading ... </p><i class="fa fa-circle-o-notch fa-spin fa-5x"></i></div>'
        //$(document.body).append(ajaxLoader);
        $("#ajax-loader").dialog({
            title: "Loading", width: 300, height: 90, modal: true, dialogClass: 'noTitleBar'
        });
    },

    CloseContentLoading: function () {
        if ($("#ajax-loader").length) {
            $("#ajax-loader").dialog('close');
        }
    },

    IsURLValid: function (url) {
        if ((typeof url === 'undefined') || url == null || url == "") {
            return false;
        }
        else {
            return true;
        }
    },

    parseValidationErrors: function(data) {
        var ul = ('<ul>');
        var json = $.parseJSON(data);
        $(json.errors).each(function (index, item) {
            ul = ul + ("<li>" + item + "</li>");
        });
        ul = ul + "</ul>"
        return ul;
    },

    NoReturnFailures: [ 401, 403, 500, 408, 0 ],

    Handlers: {
        InvalidSession: function (code) {
            if (code == 401) {
                console.log("InvalidSession");
                window.location.replace("/");
                window.location.reload();
            }
        },

        LogicalError: function (code, response) {
            if (code == 400) {
                console.log("LogicalError");
                response = ora.Ajax.parseValidationErrors(response);
                ora.Dialog.Simple({ title: "Error", message: response });
            }
        },

        UnAuthorized: function (code) {
            if (code == 403) {
                console.log("UnAuthorized");
                ora.Dialog.Simple({ title: "Un-Authorized", message: "Required access permissions not met. Please contact a Supervisor." });
            }
        },

        UnExpected: function (code) {
            if (code == 500) {
                console.log("UnExpected");
                ora.Dialog.Simple({ title: "Un-Expected Error", message: "An Unexpected Error has occured" });
            }
        },

        Timeout: function (code) {
            if (code == 0) {
                console.log("Timeout");
                ora.Dialog.Simple({ title: "Connection Issue...", message: 'The request timed out. Please try again.' })
            }
        }
    },

    Defaults: {
        preventDefault: false,
        url: "",
        dataType: "json",
        type: "GET",
        data: {},
        successCallbackData: {},
        completeCallback: function (jqXHR, textStatus) {},
        beforeSendCallback: function () { },
        successCallback: function (data) { },
        InvalidSessionCallback: function (code) { ora.Ajax.Handlers.InvalidSession(code) },
        showLoading: false,
        loadMsg: "",
        cache: true,
        isLocal: true
    },

    FormPostDeafults: {
        dialog: "",
        url: "",
        form: "",
        type: "POST",
        data: {},
        container: "",
        closeOnSave: true,
        reloadContent: true,
        completeCallback: function (jqXHR, textStatus) { },
        successCallback: function (data) { },
        ErrorCallback: function(jqXHR, statusCode, err) {},
        beforeSendCallback: function (jqXHR, statusCode, err) { },
        PreventDefault: false,
        PreventDefaultError: false,
        showLoading: true
    },

    ActionLinkDeafults: {
        type: "GET",
        UpdateTargetId: "",
        completeCallback: function (jqXHR, textStatus) { },
        errorCallback: function (request, status, error) { },
        beforeSendCallback: function () { },
        successCallback: function (data) { },
    },

    GetDefaults: function (obj, type) {
        var defaults;

        if (type == ora.Ajax.Type.Ajax) { defaults = ora.Ajax.Defaults }
        if (type == ora.Ajax.Type.FormPost) { defaults = ora.Ajax.FormPostDeafults }
        else if (type == ora.Ajax.Type.ActionLink) { defaults = ora.Ajax.ActionLinkDeafults }

        for (i in defaults) {
            if (!obj.hasOwnProperty(i)) { obj[i] = defaults[i]; }
        }
        return obj;
    },

    //--------------------------------------------------------------------------\\
    Ajax: function (parameters) {
        var ajaxOptions = ora.Ajax.GetDefaults(parameters, ora.Ajax.Type.Ajax);
        $.ajax({
            url: ajaxOptions.url,
            dataType: ajaxOptions.dataType,
            type: ajaxOptions.type,
            data: ajaxOptions.data,
            cache: ajaxOptions.cache,
            beforeSend: function () {
                if (ajaxOptions.showLoading) { ora.Ajax.ContentLoading(); }
                ajaxOptions.beforeSendCallback();
            },
            success: function (data) {
                ajaxOptions.successCallback(data, ajaxOptions.successCallbackData);
            },
            error: function (jqXHR, statusCode, err) {
                ora.Ajax.Handlers.InvalidSession(jqXHR.status)
                ora.Ajax.Handlers.Timeout(jqXHR.status)
                ora.Ajax.Handlers.UnAuthorized(jqXHR.status)
                ora.Ajax.Handlers.UnExpected(jqXHR.status)
                ora.Ajax.Handlers.LogicalError(jqXHR.status, jqXHR.responseText);
            },
            complete: function (jqXHR, textStatus) {
                if (ajaxOptions.showLoading) { ora.Ajax.CloseContentLoading(); }
                ajaxOptions.completeCallback(jqXHR, textStatus);
            }
        })
    },

    FormPost: function (parameters) {
        var ajaxOptions = ora.Ajax.GetDefaults(parameters, ora.Ajax.Type.FormPost);
        if (ajaxOptions.dialog == "") { ajaxOptions.dialog = ajaxOptions.container; }

        $.ajax({
            url: ajaxOptions.url,
            dataType: ajaxOptions.dataType,
            data: ajaxOptions.data,
            type: "POST",
            beforeSend: function () {
                var valid = $("#" + ajaxOptions.form).valid();
                if (!valid) {
                    $("#" + ajaxOptions.form).scrollTop(0);
                    return false;
                };
                if (ajaxOptions.showLoading) { ora.Ajax.ContentLoading(); }
                ajaxOptions.beforeSendCallback();
            },
            success: function (data) {
                if (ajaxOptions.showLoading) { ora.Ajax.CloseContentLoading(); }
                var saveOnly = $('[aria-describedby|="' + ajaxOptions.dialog + '"]').find(".ui-dialog-buttonpane").find(".ui-dialog-buttonset").find("#Save.PostedForm").length;
                
                $("#" + ajaxOptions.grid).trigger("reloadGrid");

                if (!saveOnly && ajaxOptions.closeOnSave) {
                    $("#" + ajaxOptions.form).parents(".dialog").dialog("close");
                    $("#" + ajaxOptions.dialog).dialog('destroy').remove();
                } else {
                    $('#' + ajaxOptions.container).html(data);
                }
                
                ajaxOptions.successCallback(data, ajaxOptions.successCallbackData);
            },
            error: function (jqXHR, statusCode, err) {
                ora.Ajax.CloseContentLoading();

                if (jqXHR.status == 400 && !ajaxOptions.PreventDefaultError) {
                    $('#' + ajaxOptions.container).html(jqXHR.responseText);
                    $("#" + ajaxOptions.container).scrollTop(0);
                    ora.Utilities.RegisterFormValidation({ id: ajaxOptions.form })
                    ora.Utilities.RegisterInputEvents(ajaxOptions.form);
                }
                else if (jqXHR.status == 400 && ajaxOptions.PreventDefaultError) {
                    ajaxOptions.ErrorCallback(jqXHR, statusCode, err);
                }
                else {
                    ora.Ajax.Handlers.UnAuthorized(jqXHR.status)
                    ora.Ajax.Handlers.InvalidSession(jqXHR.status)
                    ora.Ajax.Handlers.Timeout(jqXHR.status)
                    ora.Ajax.Handlers.UnExpected(jqXHR.status)
                }
            },
            complete: function (jqXHR, textStatus) {
                //ReRegister the form submit on return
                ora.Ajax.Handlers.Timeout(jqXHR.status)
                if (!(ora.Ajax.NoReturnFailures.indexOf(jqXHR.status) > -1) && !ajaxOptions.PreventDefault && textStatus != "timeout") {
                    $("#" + ajaxOptions.form).submit(function (event) {
                        event.preventDefault();
                        ora.Ajax.FormPost({
                            url: ajaxOptions.url,
                            container: ajaxOptions.container,
                            grid: ajaxOptions.grid,
                            dialog: ajaxOptions.dialog,
                            form: ajaxOptions.form,
                            data: $("#" + ajaxOptions.form).serializeArray(),
                            successCallback: ajaxOptions.successCallback
                        });
                    })
                }
            }
        })
    },

    ActionLink: function (parameters) {
        var ajaxOptions = ora.Ajax.GetDefaults(parameters, ora.Ajax.Type.ActionLink);

        $.ajax({
            url: ajaxOptions.url,
            dataType: ajaxOptions.dataType,
            data: ajaxOptions.data,
            type: ajaxOptions.type,
            beforeSend: function () {
                if (!ora.Ajax.IsURLValid(ajaxOptions.url)) { return false; };
                ajaxOptions.beforeSendCallback();
            },
            success: function (data) {
                ora.Ajax.CloseLoading();
                ajaxOptions.successCallback(data, ajaxOptions.successCallbackData);
            },
            error: function (jqXHR, statusCode, err) {
                //ora.Logging.Log(ora.Logging.LogLevel.DEBUG, jqXHR);
                //ora.Logging.Log(ora.Logging.LogLevel.DEBUG, "Action Link Ajax Status Code = " + jqXHR.status);
                ora.Ajax.Handlers.InvalidSession(jqXHR.status)
                ora.Ajax.Handlers.Timeout(jqXHR.status)
                ora.Ajax.Handlers.UnAuthorized(jqXHR.status)
                ora.Ajax.Handlers.UnExpected(jqXHR.status)
                ora.Ajax.Handlers.LogicalError(jqXHR.status, jqXHR.responseText);
            },
            complete: function (jqXHR, status) {
                ajaxOptions.completeCallback(jqXHR, status);
            }
        })
    },

    TabChangeConfirm: function (options) {
        if ($("#" + options.container + " form").first().hasClass('dirty')) {
            ora.Dialog.Confirm({
                id: "", title: "", message: "There are unsaved changes on this screen. Are you sure you want to leave without saving?",
                AcceptCallback: function () {
                    ora.Ajax.ActionLink(
                    {
                        url: options.url,
                        type: "GET",
                        beforeSendCallback: options.beforeSendCallback,
                        completeCallback: options.completeCallback,
                        successCallback: function (data) { $(options.updateTarget).html(data) }
                    });
                    ora.ActiveTabToggle(options.obj);
                }
            });
        } else {
            ora.Ajax.ActionLink(
            {
                url: options.url,
                type: "GET",
                beforeSendCallback: options.beforeSendCallback,
                completeCallback: options.completeCallback,
                successCallback: function (data) { $(options.updateTarget).html(data) }
            });
            ora.ActiveTabToggle(options.obj);
        }
    }
}


// JQUERY GLOBAL DEFAULTS
$.ajaxSetup({
    timeout: 20000
});