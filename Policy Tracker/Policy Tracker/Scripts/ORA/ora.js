(function (orAero, $, undefined) {
    var _productLines = [];

    orAero.branches = {
        Atlanta: { Value: 'ATL', DisplayText: 'Atlanta' },
        Dallas: { Value: 'DAL', DisplayText: 'Dallas' },
        Chicago: { Value: 'CHI', DisplayText: 'Chicago' },
        NewYork: { Value: 'NYC', DisplayText: 'New York' },
        Seattle: { Value: 'SEA', DisplayText: 'Seattle' },
    }

    orAero.statuses =  {
        Submission: { Value: 'Submission', DisplayText: 'Submission' },
        Declined: { Value: 'Declined', DisplayText: 'Declined' },
        Quote: { Value: 'Quote', DisplayText: 'Quote' },
        Lost: { Value: 'Lost', DisplayText: 'Lost' },
        Bound: { Value: 'Bound', DisplayText: 'Bound' },
        Issued: { Value: 'Issued', DisplayText: 'Issued' },
        Canceled: { Value: 'Canceled', DisplayText: 'Canceled' },
        AlreadyInvolved: { Value: 'Already Involved', DisplayText: 'Already Involved' },
    }

    orAero.productLines = function () {
        var deferred = $.Deferred();
        if (_productLines.length) {
            deferred.resolve(_productLines);
            deferred.reject(null)
        } else {
            $.get('/policytracker/api/risk/GetProductLines')
            .done(function (res) {
                _productLines = res;
                deferred.resolve(res);
            })
            .fail(function (error) { deferred.reject(error) });
        }
        
        return deferred.promise();
    }
}(window.orAero = window.orAero || {}, jQuery));

(function (workingList, $, undefined) {
    var _commandMenuBarSelector = '#WorkingListCommandMenu';
    var _pageWrapperSelector = '#page-wrapper';

    workingList.initalize = function (s, e) {
        $('#WLMyList').click();
        var commandMenuHeight = $(_commandMenuBarSelector).height();
        var height = $(_pageWrapperSelector).height();
        WorkingListGrid.SetHeight(height - 93 - (commandMenuHeight - 34));
    }

    workingList.onDoubleClickRow = function(s, e) {
        ora.Risk.RiskEdit(s.GetRowKey(e.visibleIndex));
    }

    workingList.onFocusedRowChanged = function (s, e) {
        $("#WLCurrentKey").val(s.GetRowKey(s.GetFocusedRowIndex()));
    }
}(window.orAero.workingList = window.orAero.workingList || {}, jQuery));

//$(window).resize(function () {
//    $('.dialog').each(function () {
//        var pos = $(this).dialog().dialog('option', 'position');
//        var dialogClasses = $(this).dialog().dialog('option', 'dialogClass');
//        //console.log(pos);
//        //$(this).dialog('option', 'position', pos);

//        var getWidth = function () { return $(pos.within).width() + 20};
//        var getHeight = function () { return $(pos.within).height() };
//        console.log(getWidth())
//        console.log(getHeight())
//        $(this).dialog('option', 'maxWidth', getWidth());
//        $(this).dialog('option', 'minWidth', getWidth());
//        $(this).dialog('option', 'width', getWidth());
//        $(this).dialog('option', 'maxHeight', getHeight());
//        $(this).dialog('option', 'minHeight', getHeight());
//        $(this).dialog('option', 'height', getHeight());
        
//    });
//});

if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
              ? args[number]
              : match
            ;
        });
    };
}

ora = {};

ora.UI = {
    ClearanceLookup: function () {
        ora.Dialog.ActionForm(
            {
                id: 'NewRiskClearance_dg',
                title: 'Client & Risk Lookup',
                url: "/policytracker/Policy/ClearanceSearch",
                form: {},
                modal: true,
                WritePermissions: true,
                Type: ora.Dialog.ActionFormSizes.Large,
                buttons: [
                    {
                        text: "New Client", id: "Save", name: "NewRisk",
                        click: function () {
                            ora.Risk.NewRisk();
                        }
                    },
                    {
                        text: "Close", id: "Cancel", name: "CloseRiskClearance",
                        click: function () {
                            $(this).dialog('destroy').remove();
                        }
                    }
                ]
            })
    }
},

ora.UI.Grids = {
},

ora.Utils = {},

ora.UI.Utils = {
    GetFormattedDate: function (date) {
        var month = date.getMonth() + 1;
        var day = date.getDate();
        var year = date.getFullYear();

        var fd = month + '/' + day + '/' + year;

        return fd;
    }
}

ora.Logging = {
    Log: function (error, processName) {
        processName = (typeof processName === 'undefined') ? "" : processName;
        console.error(error.message);
        $.get('/policyTracker/api/AppManagement/LogJavascriptError', { errorMsg: error.message, processName: processName })
        .done(function (data) { console.info('[' + processName + '] Javascript Error Successfully Logged to the Server') })
        .fail(function () { console.error('[' + processName + '] Failed to Log Javascript Error to the Server') })
    }
}

jQuery.extend(jQuery.jgrid.defaults,
    {
        datatype: 'json',
        mtype: 'GET',
        altRows: true,
        prmNames: { page: "pageNumber", rows: "pageSize", order: "sortOrder", sort: "sortProperty" },
        rowNum: 50,
        rowList: [20, 50, 100],
        forceFit: false,
        autowidth: true,
        loadtext: 'Loading',
        selectFirstRowOnLoad: false,
        loadComplete: function () {
            var gridId = $(this).getGridParam('id');
            var grid = jQuery("#" + gridId);
            var selFirstRow = $(this).getGridParam('selectFirstRowOnLoad');

            if ($(this).getGridParam('altRows') == true) {
                $("#" + gridId + " > tbody > tr.jqgrow:odd").addClass('ora-ui-grid-alt-row')
            }
            
            if ($(this).getGridParam('selectFirstRowOnLoad')) {
                ids = grid.jqGrid("getDataIDs");
                if (ids && ids.length > 0)
                    grid.jqGrid("setSelection", ids[0]);
            }
        },
        loadError: function (jqXHR, status, err) {
            ora.Ajax.Handlers.InvalidSession(jqXHR.status)
            ora.Ajax.Handlers.Timeout(jqXHR.status)
            ora.Ajax.Handlers.UnAuthorized(jqXHR.status)
            ora.Ajax.Handlers.UnExpected(jqXHR.status)
        }
    });