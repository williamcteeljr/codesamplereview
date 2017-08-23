$.extend($.jgrid, {
    viewModal: function (selector, o) {
        ora.Dialog.Simple({ id: "PleaseSelectGridRow", title: "Oops...", message: "Must Select a Grid Row" });
    }
});

ora.UI.Grid = {
    GridDefaults: {
        Parent: "body",
        id: "LookupGrid",
        pager: "LookupGridPager",
        width: 1024,
        height: 250,
        caption: "",
        sortorder: 'asc',
        gridType: "",
        caption: "",
        modal: true,
        rowId: "recordId",
        sortname: '',
        autowidth: true,
        afterInsertRow: function (rowId, rowData, rowElement) { }
    },

    PagerDefaults: {
        Parent: "body",
        id: "LookupGrid",
        pager: "LookupGridPager",
        getURL: "",
        form: { id: "", grid: "", submitURL: "", container: "", dialog: "", autoFocus: "" },
        editParams: [],
        type: ora.Dialog.ActionFormSizes.Large,
        position: { my: "center", at: "center", of: window },
        deleteCallback: function () { },
        blockAdd: false,
        dialogClass: "",
        modal: true,
        search: false
    },

    Grid: function (options) {
        options = ora.Utilities.MergeOptionObjects(options, ora.UI.Grid.GridDefaults);
        var parentSelector = (options.Parent != "body") ? "#" + options.Parent : options.Parent;
        $(parentSelector).find("#" + options.id).jqGrid({
            oraGridType: options.gridType,
            url: options.restUrl,
            colNames: options.columnNames,
            colModel: options.columnModel,
            jsonReader: { root: "Results", page: "CurrentPage", total: "TotalPages", records: "TotalResults", id: options.rowId, cell: "", repeatitems: false },
            prmNames: { page: "pageNumber", rows: "pageSize", order: "sortOrder", sort: "sortProperty" },
            sortname: options.sortName,
            pager: '#' + options.pager,
            sortorder: options.sortorder,
            autowidth: options.autowidth,
            width: options.width,
            height: options.height,
            caption: options.caption,
            autowidth: options.autowidth,
            afterInsertRow: options.afterInsertRow
        });
    },

    Pager: function (options, security) {
        options = ora.Utilities.MergeOptionObjects(options, ora.UI.Grid.PagerDefaults);
        options.deleteParams = (typeof options.deleteParams === 'undefined') ? options.editParams : options.deleteParams;

        if (options.deleteURL == "") {
            security.hasDeleteAccess = false;
        }
        var parentSelector = (options.Parent != "body") ? "#" + options.Parent : options.Parent;
        jQuery(parentSelector).find("#" + options.id).navGrid('#' + options.pager,
            {
                search: options.search,
                add: (options.blockAdd) ? false : security.hasWriteAccess,
                del: security.hasDeleteAccess,
                edit: security.hasWriteAccess,
                view: false,
                addfunc: function () {
                    var getURL = options.getURL;
                    options.title = 'Add New';
                    ora.UI.Grid.FormDialog(options, getURL, security);
                },
                editfunc: function (id) {
                    var rowData = $(this).getRowData(id);
                    var getURL = ora.UI.Grid.BuildQueryString(options.editParams, rowData, options.getURL);
                    options.title = 'Edit';
                    ora.UI.Grid.FormDialog(options, getURL, security);
                },
                delfunc: function (id) {
                    var rowData = $(this).getRowData(id);
                    var deleteURL = ora.UI.Grid.PluginDeleteParams(options.deleteParams, rowData, options.deleteURL);
                    ora.UI.jQGrid.RowDelete(deleteURL, options.id, options.deleteCallback);
                },
            }
        );
        jQuery("#" + options.id).setGridParam({
            ondblClickRow: function (id) {
                var rowData = $(this).getRowData(id);
                var getURL = ora.UI.Grid.BuildQueryString(options.editParams, rowData, options.getURL);
                options.title = 'Edit';
                ora.UI.Grid.FormDialog(options, getURL, security);
            }
        });
    },

    FormDialog: function (options, getURL, security) {
        if (options.type == ora.Dialog.ActionFormSizes.Large) {
            options.position = {
                my: "top left",
                at: "top left",
                of: "#page-wrapper",
                collision: "fit",
                within: "#page-wrapper"
            };
        }

        ora.Dialog.ActionForm(
            {
                id: options.form.dialog,
                title: options.title,
                form: options.form,
                url: getURL,
                modal: options.modal,
                position: options.position,
                WritePermissions: security.hasWriteAccess,
                Type: options.type,
                dialogClass: options.dialogClass,
                autoFocus: options.form.autoFocus
            }
        )
    },

    BuildQueryString: function (qStringArr, data, url) {
        var preExistingParams = false;
        var iteration = 0;

        if (url.indexOf("?") > 0) {
            preExistingParams = true;
        }
        else {
            url += "?";
        }

        for (var key in qStringArr) {
            var val = data[key];
            if (iteration > 0 || preExistingParams == true) {
                url += "&";
            }
            url += qStringArr[key] + "=" + encodeURIComponent(val);

            iteration++;
        }

        return url;
    },

    PluginDeleteParams: function (params, data, url) {
        var count = 0;
        for (var key in params) {
            var val = data[key];
            url = url.replace("{" + count + "}", val);
            count++;
        }

        return url;
    },

    Column: {
        Checkbox: function(colName, colWidth) {
            return {
                name: colName,
                width: colWidth,
                align: 'center',
                formatter: 'checkbox',
                search: false
            };
        },

        Currency: function (colName, colWidth) {
            return {
                name: colName,
                width: colWidth,
                align: 'right',
                formatter: 'currency',
                formatoptions: { prefix: '$', decimalPlaces: 0, defaultValue: 0 }
            };
        },

        LongDate: function (colName, colWidth) {
            return {
                name: colName,
                width: colWidth,
                align: 'left',
                formatter: 'date',
                formatoptions: { srcformat: 'ISO8601Long', newformat: 'ISO8601Long' }
            };
        },

        ShortDate: function (colName, colWidth) {
            return {
                name: colName,
                width: colWidth,
                align: 'center',
                formatter: 'date',
                formatoptions: { srcformat: 'Y-m-d' }
            };
        },

        YesNoBoolean: function (colName, colWidth) {
            return {
                name: colName,
                width: colWidth,
                align: 'center',
                formatter: 'select',
                editoptions: { value: "true:Yes; false:No" }
            };
        },
         
        FormatPhone: function (cellvalue, options, obj) {
            var result = new String();

            if (obj.PhoneNumber != null && obj.PhoneNumber != "") {
                result = rowObject.PhoneNumber.replace(/(\d{3})(\d{3})(\d{4})/, "($1) $2-$3");
            }
            else if (obj.PhoneAreaCode != null && obj.PhoneAreaCode != "") {
                result = ('(' + obj.PhoneAreaCode + ')' + ' ' + obj.PhonePrefix + '-' + obj.PhoneSuffix)
                if (obj.PhoneExtension != null && obj.PhoneExtension != "") { result = (result + ' x' + obj.PhoneExtension) };
            } else {
                result = "";
            }

            return result;
        },

        PhoneNumber: function (colName, colWidth) {
            return {
                name: colName,
                width: colWidth,
                align: 'center',
                formatter: ora.UI.Grid.Column.FormatPhone
            };
        }
    }
}

ora.UI.jQGrid = {
    type: {
        search: "Search",
        details: "Details",
        main: "Main"
    },
    Height: {
        Details: 103,
        QuickFind: 167,
        mainContent: 100
    },

    SelectRowDialog: function (id) {
        ora.Dialog.Simple({ id: "PleaseSelectGridRow", message: "Must Select a Grid Row" });
    },

    CustomAutoWidth: function (e, diff) {
        var width = $(e).width();
        var grid = "#" + $(e).attr('id');
        $(grid).setGridWidth(width + diff);
    },

    RowDelete: function (url, grid, callback) {
        callback = (typeof callback === 'undefined') ? function () { } : callback;

        ora.Dialog.Confirm(
            {
                id: "ConfirmRowDelete",
                Title: "Confirm",
                message: "Are you sure you want to delete this record?",
                AcceptCallback: function () {
                    ora.Ajax.Ajax({
                        url: url, type: "DELETE",
                        successCallback: function () {
                            $("#" + grid).trigger("reloadGrid");
                            callback();
                        }
                    });
                },
            }
        )

    }
}

ora.CompanySetup = {}

ora.CompanySetup.UI = {
    SetupGrid: function (restURL, gridTitle, columnNames, columnModel) {
        ora.UI.Grid.Grid({
            id: 'EntityListGrid',
            pager: 'EntityListGridPager',
            autowidth: true,
            caption: gridTitle,
            height: $('#GridPane').height() - 76,
            restUrl: restURL,
            rowId: 'RecordId',
            columnNames: columnNames,
            columnModel: columnModel
        });
    },

    SetupGridPager: function (restURL, formURL, uniqueParams, hasWriteAccess, hasDeleteAccess) {
        ora.UI.Grid.Pager(
            {
                id: 'EntityListGrid',
                pager: 'EntityListGridPager',
                modal: false,
                dialogClass: "ConfigDialog ui-dialog-top",
                deleteURL: restURL + '/{0}',
                editParams: uniqueParams,
                getURL: formURL,
                form: {
                    id: 'EntityForm',
                    grid: 'EntityListGrid',
                    submitURL: formURL, container:
                        'EntityFormDialog',
                    dialog: 'EntityFormDialog'
                },
                type: ora.Dialog.ActionFormSizes.Custom,
                position: {
                    my: 'top left',
                    at: 'top left',
                    of: '#FormPane',
                    within: '#FormPane'
                },
                deleteCallback: function () {
                    $('#EntityFormDialog').dialog().dialog('destroy').remove();
                }
            },
            { hasWriteAccess: hasWriteAccess, hasDeleteAccess: hasDeleteAccess }
        );
    }
}