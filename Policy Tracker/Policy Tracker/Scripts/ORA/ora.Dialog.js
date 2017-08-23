/************* Dialogs **************/
ora.Dialog = {
    OverflowVisible: function (dialogId) {
        $('#' + dialogId).css('overflow', 'visible');
        $('[aria-describedby|="' + dialogId +'"]').css("overflow", "visible");
    },

    Type: {
        Simple: "Simple",
        SimpleAjax: "SimpleAjax ",
        Confirm: "Confirm",
        Warning: "Warning",
        WarningAjax: "WarningAjax",
        Lookup: "Lookup",
        View: "View",
        ActionForm: "ActionForm"
    },

    Defaults: {
        id: "ora-ui-dialog",
        title: "Information",
        message: "",
        url: "",
        display: false,
        appendTo: "",
        autoOpen: true,
        modal: true,
        closeOnEscape: true,
        draggable: false,
        resizeable: false,
        height: 'auto',
        AfterContentLoaded: function () { }
    },

    RePosition: function (id, pos) {
        $(id).dialog({ position: pos });
    },

    DefaultCancelButton: function (func, parameters) {
        func = (typeof func === 'undefined') ? function () { } : func;
        parameters = (typeof parameters === 'undefined') ? {} : parameters;

        var cancelButton = {};
            cancelButton = {
                text: "Close", id: "Cancel", name: "Cancel",
                icons: { primary: "ui-icon-close", secondary: "" },
                click: function () {
                    func();
                    $(this).dialog("destroy").remove();
                }
        }
        return cancelButton;
    },

    ClearExistingDialog: function (id) {
        if ($('[aria-describedby|="' + id + '"]').length > 0) {
            $('[aria-describedby|="' + id + '"] .datepicker').datepicker().remove(); //Prevents jQuery from opening datepicker calenders on dialog close.
            $('[aria-describedby|="' + id + '"]').dialog().dialog('destroy').remove();
        }
    },

    GetDefaults: function (obj, dialogDefaults) {
        for (i in dialogDefaults) {
            if (!obj.hasOwnProperty(i)) {
                obj[i] = dialogDefaults[i]
            }
        }

        return obj;
    },

    GetDefaultButtons: function (obj, type) {
        if (typeof obj["buttons"] === 'undefined') {
            obj["buttons"] = ora.Dialog.GetDialogButtons(obj, type)
        };

        return obj;
    },
    
    GetDialogButtons: function (parameters, dialogType) {
        var buttons;
        switch (dialogType) {
            case ora.Dialog.Type.View:
                buttons = [
                    {
                        text: "Save", id: "Save", name: "Save",
                        icons: { primary: "ui-icon-disk", secondary: "" },
                        click: function () {
                            var d = $(this);
                            if ($(d.context).find("form").length == 0) { $(d).dialog('destroy').remove(); }
                            else {
                                $(d.context).closest('[aria-describedby|="' + d.context.id + '"]').find(".ui-dialog-buttonpane").find(".ui-dialog-buttonset").find("#Save").addClass("PostedForm");
                                $(d.context).find("form").submit();
                            }
                        }
                    },
                    {
                        text: "Save/Close", id: "SaveClose", name: "SaveClose",
                        icons: { primary: "ui-icon-disk", secondary: "" },
                        click: function () {
                            var d = $(this);
                            if ($(d.context).find("form").length == 0) { $(d).dialog('destroy').remove(); }
                            else {
                                $(d.context).closest('[aria-describedby|="' + d.context.id + '"]').find(".ui-dialog-buttonpane").find(".ui-dialog-buttonset").find("#Save").removeClass("PostedForm");
                                $(d.context).find("form").submit();
                            }
                        }
                    },
                    {
                        text: "Close", id: "Cancel", name: "Cancel",
                        icons: { primary: "ui-icon-close", secondary: "" },
                        click: function () {
                            parameters.OnClose();
                        }
                    },
                ];
                break;

            case ora.Dialog.Type.Simple:
                buttons = [
                    {
                        text: "Ok", id: "Ok", name: "Ok",
                        click: function () {
                            $(this).dialog('destroy').remove();
                        }
                    }
                ];
                break;

            case ora.Dialog.Type.SimpleAjax:
                buttons = [
                    {
                        text: "Close", id: "Cancel", name: "Ok",
                        click: function () {
                            $(this).dialog('destroy').remove();
                        }
                    }
                ];
                break;

            case ora.Dialog.Type.ActionForm:
                buttons = [
                    {
                        text: "Save", id: "Save", name: "Save",
                        //icons: { primary: "ui-icon-disk", secondary: "" },
                        click: function () {
                            $('[aria-describedby|="' + parameters.id + '"] > .ui-dialog-buttonpane > .ui-dialog-buttonset > #Save').addClass("PostedForm");
                            $(this).addClass("PostedForm");
                            $(this).find("form").submit();
                        }
                    },
                    {
                        text: "Save/Close", id: "SaveClose", name: "SaveClose",
                        //icons: { primary: "ui-icon-disk", secondary: "" },
                        click: function () {
                            $('[aria-describedby|="' + parameters.id + '"] > .ui-dialog-buttonpane > .ui-dialog-buttonset > #Save').removeClass("PostedForm");
                            $(this).find("form").submit();
                        },
                        create: function () {
                            $("#SaveClose").addClass("HotKey");
                        }
                    },
                    {
                        text: "Close", id: "Cancel", name: "Cancel",
                        //icons: { primary: "ui-icon-close", secondary: "" },
                        click: function () {
                            parameters.OnClose();
                        }
                    },
                ];
                break;

            case ora.Dialog.Type.Lookup:
                buttons = [
                    {
                        text: "Ok",
                        click: function () {
                            var context = jQuery('#' + parameters.grid).jqGrid('getGridParam', 'selrow');
                            $("#" + parameters.field).val(context);
                            $("#" + parameters.field).focus();
                            parameters.callback();
                            $(this).dialog("destroy");
                        }
                    },
                    ora.Dialog.DefaultCancelButton()
                ];
                break;

            case ora.Dialog.Type.Warning:
                buttons = [
                    {
                        text: "Ok",
                        click: function () {
                            parameters.AcceptCallback();
                            $(this).dialog().dialog("destroy").remove();
                        }
                    },
                    {
                        text: "Cancel",
                        click: function () {
                            parameters.RejectCallback();
                            $(this).dialog().dialog("destroy").remove();
                        }
                    }
                ];
                break;
            default:
                defaults = ora.Dialog.simpleDefaultParameters;
        }
        return buttons;
    }
}

ora.Dialog.Simple = function (parameters) {
    var Defaults = {
        message: "",
        MinWidth: function () { return 500 },
        MaxWidth: function () { return 500 },
        Width: function () { return 500 },
        MaxHeight: function () { return 400 },
        MinHeight: function () { return 400 },
        Height: function () { return 400 },
        Position: { my: "center", at: "center", of: window },
        AfterContentLoaded: function () { },
        OpenCallback: function () { }
    };

    var dialogOptions = ora.Dialog.GetDefaults(parameters, ora.Dialog.Defaults);
    dialogOptions = ora.Dialog.GetDefaults(dialogOptions, Defaults);
    dialogOptions = ora.Dialog.GetDefaultButtons(dialogOptions, ora.Dialog.Type.Simple);

    ora.Dialog.ClearExistingDialog(dialogOptions.id)

    var dialog = $('<div id="' + dialogOptions.id + '" class="dialog"></div>')
        .dialog({
            title: dialogOptions.title,
            position: dialogOptions.Position,
            maxWidth: dialogOptions.MaxWidth(),
            minWidth: dialogOptions.MinWidth(),
            width: dialogOptions.Width(),
            height: dialogOptions.Height(),
            minHeight: dialogOptions.MinHeight(),
            maxHeight: dialogOptions.MaxHeight(),
            draggable: true,
            resizeable: true,
            resize: 'auto',
            buttons: dialogOptions.buttons,
            open: function () {
                dialogOptions.OpenCallback();
            }
        }).html(dialogOptions.message, dialogOptions.AfterContentLoaded())
        .addClass("dialog-content-simple");

    return dialog;
}

ora.Dialog.SimpleAjax = function (parameters) {
    var Defaults = {
        MinWidth: function () { return ($(window).width()) },
        MaxWidth: function () { return ($(window).width()) },
        Width: function () { return ($(window).width()) },
        MaxHeight: function () { return ($(window).height()) },
        MinHeight: function () { return ($(window).height()) },
        Height: function () { return ($(window).height()) },
        Position: { my: "top left", at: "top left", of: window },
        dialogClass: "",
    };

    var dialogOptions = ora.Dialog.GetDefaults(parameters, ora.Dialog.Defaults);
    dialogOptions = ora.Dialog.GetDefaults(dialogOptions, Defaults);
    dialogOptions = ora.Dialog.GetDefaultButtons(dialogOptions, ora.Dialog.Type.SimpleAjax);

    var dialog = $('<div id="' + dialogOptions.Id + '" class="dialog"><div id="ajax-loader" class="ui-front ui-ajax-dialogloader"><div class="panel panel-default"><div class="panel-heading btn-danger" style="text-align:center"><i class="fa fa-circle-o-notch fa-spin fa-2x"></i></div></div></div></div>')
        .dialog({
            title: dialogOptions.Title,
            position: dialogOptions.Position,
            maxWidth: dialogOptions.MaxWidth(),
            minWidth: dialogOptions.MinWidth(),
            width: dialogOptions.Width(),
            height: dialogOptions.Height(),
            minHeight: dialogOptions.MinHeight(),
            maxHeight: dialogOptions.MaxHeight(),
            draggable: false,
            resizable: false,
            modal: dialogOptions.modal,
            buttons: dialogOptions.buttons,
            dialogClass: dialogOptions.dialogClass
        }).load(dialogOptions.Url, function (rsp, status, jqXHR) {
            if (jqXHR.status >= 400) {
                ora.Ajax.Handlers.InvalidSession(jqXHR.status)
                ora.Ajax.Handlers.Timeout(jqXHR.status)
                ora.Ajax.Handlers.UnAuthorized(jqXHR.status)
                ora.Ajax.Handlers.UnExpected(jqXHR.status)
                ora.Ajax.Handlers.LogicalError(jqXHR.status, rsp);
            }
            else { dialogOptions.AfterContentLoaded(); }
        });
    //.addClass("dialog-content-simple");

    return dialog;
}

ora.Dialog.Confirm = function (parameters) {
    var Defaults = {
        id: "ConfirmDialogBox",
        message: "",
        AcceptCallback: function () { },
        RejectCallback: function () { }
    };

    var dialogOptions = ora.Dialog.GetDefaults(parameters, ora.Dialog.Defaults);
    dialogOptions = ora.Dialog.GetDefaults(dialogOptions, Defaults);
    dialogOptions = ora.Dialog.GetDefaultButtons(dialogOptions, ora.Dialog.Type.Confirm);

    var dialog = $('<div id="' + dialogOptions.id + '" class="dialog"></div>')
        .dialog({
            title: "Confirm",
            position: { my: "center", at: "center", of: window },
            width:500,
            maxWidth: 500,
            height: 300,
            modal: true,
            draggable: true,
            buttons: [
                {
                    text: "Yes", id: "Save", name: "Save",
                    click: function () {
                        dialogOptions.AcceptCallback();
                        $(this).dialog("destroy");
                    },
                },
                {
                    text: "No", id: "Cancel", name: "Cancel",
                    click: function () {
                        dialogOptions.RejectCallback();
                        $(this).dialog("destroy");
                    }
                }
            ],
        }).html(dialogOptions.message)
        .addClass("dialog-content-simple");

    return dialog;
}

ora.Dialog.Warning = function (parameters) {
    var Defaults = {
        id: "WarningDialogBox",
        message: "",
        AcceptCallback: function () { },
        RejectCallback: function () { },
        AfterContentLoaded: function () { },
        minWidth: 500,
        maxWidth: 500,
        Width: 500,
        maxHeight: 500,
        minHeight: 500,
    };

    var dialogOptions = ora.Dialog.GetDefaults(parameters, ora.Dialog.Defaults);
    dialogOptions = ora.Dialog.GetDefaults(dialogOptions, Defaults);
    dialogOptions = ora.Dialog.GetDefaultButtons(dialogOptions, ora.Dialog.Type.Warning);

    var dialog = $('<div id="' + dialogOptions.id + '" class="dialog"></div>')
        .dialog({
            title: "Confirm",
            position: { my: "center", at: "center", of: window },
            minWidth: dialogOptions.minWidth,
            maxWidth: dialogOptions.maxWidth,
            Width: dialogOptions.Width,
            minHeight: dialogOptions.minHeight,
            maxHeight: dialogOptions.maxHeight,
            draggable: false,
            buttons: dialogOptions.buttons,
        }).load(dialogOptions.url, function (rsp, status, jqXHR) {
            if (jqXHR.status >= 400) {
                ora.Ajax.Handlers.InvalidSession(jqXHR.status)
                ora.Ajax.Handlers.Timeout(jqXHR.status)
                ora.Ajax.Handlers.UnAuthorized(jqXHR.status)
                ora.Ajax.Handlers.UnExpected(jqXHR.status)
                ora.Ajax.Handlers.LogicalError(jqXHR.status, rsp);
            }
            else {
                var content = "<ul>";
                rsp = jQuery.parseJSON(rsp);
                for (var i = 0; i < rsp.length; i++) {
                    content += "<li>" + rsp[i] + "</li>";
                }
                content += "</ul>";
                $(this).html(content);
                dialogOptions.AfterContentLoaded();
            }
        });

    return dialog;
}

ora.Dialog.Lookup = function (parameters) {
    var Defaults = {
        width: 1024,
        grid: "LookupGrid",
        callback: function () { },
        entityType: ""
    };

    var dialogOptions = ora.Dialog.GetDefaults(parameters, ora.Dialog.Defaults);
    dialogOptions = ora.Dialog.GetDefaults(dialogOptions, Defaults);
    dialogOptions = ora.Dialog.GetDefaultButtons(dialogOptions, ora.Dialog.Type.Lookup);

    if ($('[aria-describedby|="' + dialogOptions.id + '"]').length > 0) {
        var lookupDialogs = ($(".ui-dialog.lookup").length + 1) + "";
        dialogOptions.id = "LookupDialog" + lookupDialogs;
        dialogOptions.title = dialogOptions.entityType + " Lookup";
    }

    var dialog = $("<div id='" + dialogOptions.id + "' class='dialog'></div>")
    .dialog({
        title: dialogOptions.title,
        width: dialogOptions.width,
        position: { my: "center", at: "center center-150", of: "#mainContent", collision: "flipfit" },
        draggable: true,
        buttons: dialogOptions.buttons,
        dialogClass: "lookup",
        close: function () {
            $(this).dialog('destroy').remove();
        },
    }).load(dialogOptions.url, function (rsp, status, jqXHR) {
        if (jqXHR.status >= 400) {
            ora.Ajax.Handlers.InvalidSession(jqXHR.status)
            ora.Ajax.Handlers.Timeout(jqXHR.status)
            ora.Ajax.Handlers.UnAuthorized(jqXHR.status)
            ora.Ajax.Handlers.UnExpected(jqXHR.status)
            ora.Ajax.Handlers.LogicalError(jqXHR.status, rsp);
        }
        else {
            dialogOptions.AfterContentLoaded();
            $('#' + dialogOptions.grid).jqGrid('setGridParam', {
                ondblClickRow: function (id) {
                    var context = jQuery('#' + dialogOptions.grid).jqGrid('getGridParam', 'selrow');
                    $("#" + dialogOptions.field).val(context);
                    dialogOptions.callback();
                    $("#" + dialogOptions.id).dialog().dialog("destroy").remove();
                }
            });
        }
    });

    return dialog
}

ora.Dialog.View = function (parameters) {
    var Defaults = {
        form: "",
        autoFocus: "",
        WritePermission: false,
        OnClose: function () {
            if ($('#' + parameters.form).hasClass('dirty')) {
                ora.Dialog.Confirm({
                    id: "", title: "", message: "There are unsaved changes on this screen. Are you sure you want to leave without saving?",
                    AcceptCallback: function () {
                        $("#" + parameters.id).dialog().dialog('destroy').remove();
                    }
                });
            }
            else {
                $("#" + parameters.id).dialog().dialog('destroy').remove();
            }
        }
    };

    var dialogOptions = ora.Dialog.GetDefaults(parameters, ora.Dialog.Defaults);
    dialogOptions = ora.Dialog.GetDefaults(dialogOptions, Defaults);
    dialogOptions = ora.Dialog.GetDefaultButtons(dialogOptions, ora.Dialog.Type.View);

    if (!dialogOptions.WritePermissions) {
        dialogOptions.buttons = dialogOptions.buttons.remove(function (el) { return el.id === "SaveClose"; });
        dialogOptions.buttons = dialogOptions.buttons.remove(function (el) { return el.id === "Save"; });
    }

    if ($('[aria-describedby|="' + dialogOptions.id + '"]').length > 0) {
        $('[aria-describedby|="' + dialogOptions.id + '"]').dialog().dialog('destroy').remove();
    }

    var dialog = $("<div id='" + dialogOptions.id + "' class='dialog'></div>")
    .dialog({
        title: dialogOptions.title,
        position: { my: "top left", at: "top left", of: "#mainContent", collision: "fit", within: "#mainContent" },
        minWidth: $("#mainContent").width(),
        maxWidth: $("#mainContent").width(),
        Width: $("#mainContent").width(),
        minHeight: $("#mainContent").height(),
        maxHeight: $("#mainContent").height(),
        height: $("#mainContent").height(),
        dialogClass: "dialog-content-noBorder ui-dialog-resize",
        open: function () {
            $("body").css("overflow", "hidden");
            $(".ui-widget-overlay").removeClass("ui-widget-overlay");
            if (dialogOptions.display) {
                $('[aria-describedby|="' + dialogOptions.id + '"] > .ui-dialog-buttonpane > .ui-dialog-buttonset > #Save').hide();
                $('[aria-describedby|="' + dialogOptions.id + '"] > .ui-dialog-buttonpane > .ui-dialog-buttonset > #SaveClose').hide();
            }
        },
        close: function () {
            $("body").css("overflow", "auto");
            $(this).dialog('destroy').remove();
        },
        buttons: dialogOptions.buttons,
    }).load(dialogOptions.url, function (rsp, status, jqXHR) {        
        if (jqXHR.status >= 400) {
            ora.Ajax.Handlers.InvalidSession(jqXHR.status)
            ora.Ajax.Handlers.Timeout(jqXHR.status)
            ora.Ajax.Handlers.UnAuthorized(jqXHR.status)
            ora.Ajax.Handlers.UnExpected(jqXHR.status)
            ora.Ajax.Handlers.LogicalError(jqXHR.status, rsp);
        }
        else {
            dialogOptions.AfterContentLoaded();
            if (dialogOptions.autoFocus != "") { $("form#" + dialogOptions.form + " #" + dialogOptions.autoFocus).focus() }
        }
    });
    
    return dialog;
}

ora.Dialog.ActionFormSizes = {
    Small: {
        minWidth: function () { return 600 },
        maxWidth: function () { return 600 },
        Width: function () { return 600 },
        minHeight: function () { return 350 },
        maxHeight: function () { return 350 },
    },
    Medium: {
        minWidth: function () { return 1000 },
        maxWidth: function () { return 1000 },
        Width: function () { return 1000 },
        minHeight: function () { return 750 },
        maxHeight: function () { return 750 },
    },
    Large: {

    },
    Full: {
        minWidth: function () { $(window).width() },
        Width: function () { return $(window).width() },
        minHeight: function () { return $(window).height() },
        Height: function () { return $(window).height() },
    },
    Custom: {

    }
},

ora.Dialog.ActionForm = function (parameters) {
    var Defaults = {
        layoutContainerId: "",
        modal: true,
        Type: ora.Dialog.ActionFormSizes.Large,
        form: { id: "NoForm", container: "", submitURL: "", dialog: parameters.id, grid: "", closeOnSave: false, successCallback: function (data) { } },
        WritePermission: false,
        HideSaveButton: false,
        autoFocus: "",
        position: { my: "top left", at: "top left", of: window, collision: "flipfit", within: window },
        //position: { my: "center", at: "center", of: window, within: window },
        //position: { my: "center", at: "center center-150", of: "#mainContent", collision: "flipfit" },
        minWidth: function () { return ($(Defaults.position.within).width()) },
        maxWidth: function () { return ($(Defaults.position.within).width()) },
        Width: function () { return ($(Defaults.position.within).width()) },
        maxHeight: function () { return ($(Defaults.position.within).height() + 14) },
        minHeight: function () { return ($(Defaults.position.within).height() + 14) },
        dialogClass: "",
        openCallback: function () { },
        closeCallback: function () { },
        createCallback: function () { },
        AfterContentLoaded: function () { },
        OnClose: function () {
            if ($('#' + parameters.form.id).hasClass('dirty')) {
                ora.Dialog.Confirm({
                    id: "", title: "", message: "There are unsaved changes on this screen. Are you sure you want to leave without saving?",
                    AcceptCallback: function () {
                        $("#" + parameters.id).dialog().dialog('destroy').remove();
                    }
                });
            }
            else {
                $("#" + parameters.id).dialog().dialog('destroy').remove();
            }
        },
        CustomFormSubmit: false
    };

    if (typeof parameters.form.dialog === 'undefined' || parameters.form.dialog == "" || parameters.form.dialog == null) {
        parameters.form.dialog = parameters.form.container;
    }

    var dialogOptions = ora.Dialog.GetDefaults(parameters, ora.Dialog.Defaults);
    dialogOptions = ora.Dialog.GetDefaults(dialogOptions, Defaults);
    dialogOptions = ora.Dialog.GetDefaultButtons(dialogOptions, ora.Dialog.Type.ActionForm);

    if (dialogOptions.Type == ora.Dialog.ActionFormSizes.Large) {
        dialogOptions.position = Defaults.position;
        dialogOptions.dialogClass += " DialogSizeLarge";
    } else if (dialogOptions.Type == ora.Dialog.ActionFormSizes.Small) {
        dialogOptions = Object.merge(dialogOptions, ora.Dialog.ActionFormSizes.Small, false, true);
        dialogOptions.dialogClass += " DialogSizeSmall";
    } else if (dialogOptions.Type == ora.Dialog.ActionFormSizes.Medium) {
        dialogOptions = Object.merge(dialogOptions, ora.Dialog.ActionFormSizes.Medium, false, true);
        dialogOptions.dialogClass += " DialogSizeMedium";
    } else if (dialogOptions.Type == ora.Dialog.ActionFormSizes.Custom) {
        dialogOptions.dialogClass += " DialogSizeCustom";
        dialogOptions.minWidth = function () { return ($(dialogOptions.position.within).width()) }
        dialogOptions.maxWidth = function () { return ($(dialogOptions.position.within).width()) }
        dialogOptions.Width = function () { return ($(dialogOptions.position.within).width()) }
        dialogOptions.maxHeight = function () { return ($(dialogOptions.position.within).height()) }
        dialogOptions.minHeight = function () { return ($(dialogOptions.position.within).height()) }
    } else if (dialogOptions.Type == ora.Dialog.ActionFormSizes.Full) {
        dialogOptions.position = {};
        dialogOptions.dialogClass += " DialogSizeFull";
    }

    if (!dialogOptions.WritePermissions) {
        dialogOptions.buttons = dialogOptions.buttons.remove(function (el) { return el.id === "SaveClose"; });
        dialogOptions.buttons = dialogOptions.buttons.remove(function (el) { return el.id === "Save"; });
    }

    if (dialogOptions.HideSaveButton) {
        dialogOptions.buttons = dialogOptions.buttons.remove(function (el) { return el.id === "Save"; });
    }

    ora.Dialog.ClearExistingDialog(dialogOptions.id)

    var dialog = $('<div id="' + dialogOptions.id + '" class="dialog" title="' + dialogOptions.title + '"><div id="ajax-loader" class="ui-front ui-ajax-dialogloader"><div class="panel panel-default"><div class="panel-heading btn-danger" style="text-align:center"><i class="fa fa-circle-o-notch fa-spin fa-2x"></i></div></div></div></div>')
    .dialog({
        modal: dialogOptions.modal,
        closeOnEscape: false,
        position: dialogOptions.position,
        minWidth: dialogOptions.minWidth(),
        maxWidth: dialogOptions.maxWidth(),
        Width: dialogOptions.Width(),
        minHeight: dialogOptions.minHeight(),
        maxHeight: dialogOptions.maxHeight(),
        buttons: dialogOptions.buttons,
        appendTo: dialogOptions.appendTo,
        autoOpen: dialogOptions.autoOpen,
        dialogClass: dialogOptions.dialogClass + " ui-dialog-resize",
        draggable: false,
        resizable: false,
        open: function (event, ui) {
            $("body").css("overflow", "hidden");
            dialogOptions.openCallback();
        },
        close: function (e, ui) {
            $("body").css("overflow", "auto");
            dialogOptions.closeCallback();
        },
        create: function () {
            dialogOptions.createCallback();
        }
    }).load(dialogOptions.url, function (rsp, status, jqXHR) {
        if (jqXHR.statusText == 0) {
            ora.Ajax.Handlers.Timeout(jqXHR.status)
        }
        if (jqXHR.status >= 400) {
            ora.Ajax.Handlers.InvalidSession(jqXHR.status)
            ora.Ajax.Handlers.UnAuthorized(jqXHR.status)
            ora.Ajax.Handlers.UnExpected(jqXHR.status)
            ora.Ajax.Handlers.LogicalError(jqXHR.status, rsp);
        }
        else {
            dialogOptions.AfterContentLoaded();
            if (!dialogOptions.CustomFormSubmit) { ora.Utilities.RegisterFormSubmit(dialogOptions.form); };
            ora.Utilities.RegisterFormValidation(dialogOptions.form)
            ora.Utilities.RegisterInputEvents(dialogOptions.form.id);
            if (dialogOptions.autoFocus != "") {
                $("form#" + dialogOptions.form.id + " #" + dialogOptions.autoFocus).focus();
            }
        }
    });

    return dialog;
}

ora.Dialog.ToggleFormActions = function(moduleId, form) {
    if ($('[aria-describedby|="' + moduleId + '"] > #' + moduleId + ' > .ui-module-content > .ui-details > #' + form).length <= 0) {
        $('[aria-describedby|="' + moduleId + '"] > .ui-dialog-buttonpane > .ui-dialog-buttonset > #Save').hide();
        $('[aria-describedby|="' + moduleId + '"] > .ui-dialog-buttonpane > .ui-dialog-buttonset > #SaveClose').hide();
    }
    else {
        $('[aria-describedby|="' + moduleId + '"] > .ui-dialog-buttonpane > .ui-dialog-buttonset > #Save').show();
        $('[aria-describedby|="' + moduleId + '"] > .ui-dialog-buttonpane > .ui-dialog-buttonset > #SaveClose').show();
    }
}

ora.Dialog.ClearDialogStack = function() {
    $(".ui-dialog").dialog().dialog('destroy').remove();
}