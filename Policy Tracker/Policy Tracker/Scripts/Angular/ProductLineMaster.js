(function () {
    var app = angular.module('ProductLineMaster', ['dx', 'fcsa-number', 'ui.bootstrap', 'appFactories', 'appFilters', 'ngRoute']);

    app.controller('consoleController', ['$scope', '$document', '$http', '$uibModal', 'orAero', '$filter', function ($scope, $document, $http, $uibModal, orAero, $filter) {
        $scope.NBAR = {};
        $scope.praseInt = parseInt;
        $scope.year = new Date().getFullYear();
        $scope.Years = [];
        $scope.showDetail = true;
        $scope.views = ['loading', 'error', 'summary']
        $scope.viewing = $scope.views[0];
        $scope.branches = [{ branch: '', displayText: 'None' }].concat(orAero.branches);
        $scope.showNotes = false;

        $scope.reloadPage = function () {
            location.reload();
        };

        for (var i = new Date().getFullYear() + 1 ; i >= 2013; i--) {
            $scope.Years.push(i);
        }

        $scope.consoleSettings = {
            productLine: {},
            underwriter: {},
            year: 0,
            month: {},
            branch: {}
        };

        $scope.consoleSettings.branch = $filter('filter')($scope.branches, { branch: '' }, true)[0];
        $scope.branch = $filter('filter')($scope.branches, { branch: '' }, true)[0];

        $scope.openSettingsWarning = function () {
            $scope.modalInstance = $uibModal.open({
                animation: false,
                templateUrl: 'myModalContent.html',
                scope: $scope
            });

            $scope.modalInstance.result.then(function (selectedItem) { }, function () { });
        };

        $scope.ok = function () { $scope.modalInstance.close(); };

        var currentMonth = (new Date().getMonth() + 1).toString();
        $scope.month = {};
        $scope.Months = [
            { Month: 0, Name: 'Year to Date', DisplayText: 'Summary' },
            { Month: 1, Name: 'January', DisplayText: 'Jan' },
            { Month: 2, Name: 'February', DisplayText: 'Feb' },
            { Month: 3, Name: 'March', DisplayText: 'Mar' },
            { Month: 4, Name: 'April', DisplayText: 'Apr' },
            { Month: 5, Name: 'May', DisplayText: 'May' },
            { Month: 6, Name: 'June', DisplayText: 'Jun' },
            { Month: 7, Name: 'July', DisplayText: 'Jul' },
            { Month: 8, Name: 'August', DisplayText: 'Aug' },
            { Month: 9, Name: 'September', DisplayText: 'Sept' },
            { Month: 10, Name: 'October', DisplayText: 'Oct' },
            { Month: 11, Name: 'November', DisplayText: 'Nov' },
            { Month: 12, Name: 'December', DisplayText: 'Dec' },
        ];

        for (var i = 0; i < $scope.Months.length; i++) {
            if ($scope.Months[i].Month == currentMonth) $scope.month = $scope.Months[i];
        }

        $scope.productLine = {};
        $scope.underwriter = {};

        $scope.selectedTab = $scope.month.Month;
        $scope.scrolledTabsOptions = {
            dataSource: [
                { text: $scope.Months[0].DisplayText },
                { text: $scope.Months[1].DisplayText },
                { text: $scope.Months[2].DisplayText },
                { text: $scope.Months[3].DisplayText },
                { text: $scope.Months[4].DisplayText },
                { text: $scope.Months[5].DisplayText },
                { text: $scope.Months[6].DisplayText },
                { text: $scope.Months[7].DisplayText },
                { text: $scope.Months[8].DisplayText },
                { text: $scope.Months[9].DisplayText },
                { text: $scope.Months[10].DisplayText },
                { text: $scope.Months[11].DisplayText },
                { text: $scope.Months[12].DisplayText },

            ],
            bindingOptions: {
                selectedIndex: 'selectedTab'
            },
            onSelectionChanged: function (e) {
                $scope.consoleSettings.month = $scope.Months[$scope.selectedTab];
                $scope.updateConsoleSettings();
            },
            scrollByContent: true,
            showNavButtons: true
        };

        $scope.riskGridCurrRiskId = 0;

        $scope.Renewals = [];
        $scope.NewBusiness = [];

        var calculateCustomSummaries = function (options) {
            if (options.name == 'customWrittenPremium') {
                if (options.summaryProcess == 'start') {
                    options.totalValue = 0;
                }
                if (options.summaryProcess == 'calculate') {
                    var writtenPremium = (options.value.Status == "Issued") ? options.value.WrittenPremium : 0;
                    options.totalValue = options.totalValue + writtenPremium;
                }
            } else if (options.name == 'customBookedPremium') {
                if (options.summaryProcess == 'start') {
                    options.totalValue = 0;
                }
                if (options.summaryProcess == 'calculate') {
                    var bookedPremium = 0;
                    if (options.value.Status == "Issued") {
                        bookedPremium = (options.value.IsPaidInInstallments || options.value.IsReporter) ? options.value.DepositPremium : options.value.WrittenPremium;
                    }
                    options.totalValue = options.totalValue + bookedPremium;
                }
            } else if (options.name == 'customExpiringPremium') {
                if (options.summaryProcess == 'start') {
                    options.totalValue = 0;
                }
                if (options.summaryProcess == 'calculate') {
                    var expiringWrittenPremium = (options.value.Status == "Issued") ? options.value.ExpiringWrittenPremium : 0;
                    options.totalValue = options.totalValue + expiringWrittenPremium;
                }
            } else if (options.name == 'customExpiringPayroll') {
                if (options.summaryProcess == 'start') {
                    options.totalValue = 0;
                }
                if (options.summaryProcess == 'calculate') {
                    var expiringPayroll = (options.value.Status == "Issued") ? options.value.ExpiringPayroll : 0;
                    options.totalValue = options.totalValue + expiringPayroll;
                }
            } else if (options.name == 'customPayroll') {
                if (options.summaryProcess == 'start') {
                    options.totalValue = 0;
                }
                if (options.summaryProcess == 'calculate') {
                    var payroll = (options.value.Status == "Issued") ? options.value.Payroll : 0;
                    options.totalValue = options.totalValue + payroll;
                }
            }
        };

        $scope.showHideColumns = function () {
            if ($scope.productLine.Name == "Workers Comp") {
                //Renewals
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'ScheduledRating', 'visible', true);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'ExpirienceModifier', 'visible', true);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'ExpiringPayroll', 'visible', true);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'Payroll', 'visible', true);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'payChg', 'visible', true);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'expNR', 'visible', true);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'netRate', 'visible', true);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'netRateChange', 'visible', true);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'Name', 'visible', true);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'OtherName', 'visible', false);

                //New Business
                $('#plc_newBusinessRisks').dxDataGrid('columnOption', 'AccountDescription', 'visible', true);
                $('#plc_newBusinessRisks').dxDataGrid('columnOption', 'ScheduledRating', 'visible', true);
                $('#plc_newBusinessRisks').dxDataGrid('columnOption', 'ExpirienceModifier', 'visible', true);
                $('#plc_newBusinessRisks').dxDataGrid('columnOption', 'Payroll', 'visible', true);
                $('#plc_newBusinessRisks').dxDataGrid('columnOption', 'PurposeOfUse', 'visible', false);
            } else {
                //Renewals
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'ScheduledRating', 'visible', false);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'ExpirienceModifier', 'visible', false);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'ExpiringPayroll', 'visible', false);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'Payroll', 'visible', false);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'payChg', 'visible', false);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'expNR', 'visible', false);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'netRate', 'visible', false);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'netRateChange', 'visible', false);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'Name', 'visible', false);
                $('#plc_renewalRisks').dxDataGrid('columnOption', 'OtherName', 'visible', true);

                //New Business
                $('#plc_newBusinessRisks').dxDataGrid('columnOption', 'AccountDescription', 'visible', false);
                $('#plc_newBusinessRisks').dxDataGrid('columnOption', 'ScheduledRating', 'visible', false);
                $('#plc_newBusinessRisks').dxDataGrid('columnOption', 'ExpirienceModifier', 'visible', false);
                $('#plc_newBusinessRisks').dxDataGrid('columnOption', 'Payroll', 'visible', false);
                $('#plc_newBusinessRisks').dxDataGrid('columnOption', 'PurposeOfUse', 'visible', true);
            }
        }

        //Renewal Grid Settings'
        //Product Line Console
        $scope.renewalGridSettings = {
            dataSource: $scope.Renewals,
            bindingOptions: { dataSource: 'Renewals' },
            onContentReady: function (obj, ele, model) {
                $scope.viewing = $scope.views[2];
                //$scope.$apply()
            },
            loadPanel: { enabled: false },
            columns: [
                { dataField: "RiskId", caption: 'Id', width: 50, visible: false },
                //{ dataField: "BusinessType", caption: 'Type', groupIndex: 1, sortOrder: 'desc' },
                { dataField: "Branch", caption: 'Branch', groupIndex: 1 },
                //{ dataField: "ProgramType", caption: 'Prgm Type', width: 75 },
                { dataField: "UW", caption: 'UW', width: 75 },
                { dataField: 'DisplayEffectiveDate', caption: 'Effective', width: 80, dataType: 'date' },
                { dataField: 'PolicyNumber', caption: 'Policy #', width: 110 },
                { dataField: "Name", caption: 'Named Insured', width: 230 },
                { dataField: "Name", name: "OtherName", caption: 'Named Insured' },
                //{ dataField: "AccountDesc", caption: 'Desc', width: 100 },
                { dataField: "AgencyName", caption: 'Broker', width: 230 },
                { dataField: "Status", caption: 'Status', width: 50 },
                { dataField: "ScheduledRating", caption: 'Sch Rating', width: 60 },
                { dataField: "ExpirienceModifier", caption: 'X Mod', width: 60 },
                { dataField: "ExpiringWrittenPremium", caption: 'Exp Written', format: 'currency', width: 100 },
                {
                    caption: 'Exp Booked', format: 'currency', width: 80, calculateCellValue: function (e) {
                        return (e.IsPaidInInstallments || e.IsReporter) ? e.ExpiringDepositPremium : e.ExpiringWrittenPremium;
                    }
                },
                { dataField: "ExpiringPayroll", caption: 'Exp Pay', format: 'currency', width: 100 },
                {
                    name: "expNR", caption: 'Exp NR', format: 'fixedPoint', precision: 4, width: 60, calculateCellValue: function (e) {
                        if (e.ExpiringPayroll == 0 && e.ExpiringPayroll != null) {
                            return 0;
                        } else {
                            return (e.ExpiredAnnualizedPremium / e.ExpiringPayroll);
                        }
                    }
                },
                { dataField: "WrittenPremium", caption: 'Written', format: 'currency', width: 100 },
                { dataField: "DepositPremium", caption: 'Deposit', format: 'currency', width: 100 },
                { dataField: "Payroll", caption: 'Payroll', format: 'currency', width: 100 },
                {
                    name: 'payChg', caption: 'Pay Chg', format: 'FixedPoint', precision: 4, width: 60, calculateCellValue: function (e) {
                        return (e.ExpiringPayroll != 0) ? (((e.Payroll / e.ExpiringPayroll) - 1) * 100) : 0;
                    }
                },
                {
                    name: "Booked", caption: 'Booked', format: 'currency', width: 80, calculateCellValue: function (e) {
                        return (e.IsPaidInInstallments || e.IsReporter) ? e.DepositPremium : e.WrittenPremium;
                    }
                },
                {
                    name: "netRate", caption: 'NR', format: 'fixedPoint', precision: 3, width: 50, calculateCellValue: function (e) {
                        if (e.Payroll == 0 && e.Payroll != null) {
                            return 0;
                        } else {
                            return (e.AnnualizedPremium / (e.Payroll / 100));
                        }
                    }
                },
                {
                    name: "netRateChange", caption: 'NR Chg', format: 'fixedPoint', precision: 3, width: 60, calculateCellValue: function (e) {
                        var expRate = (e.ExpiringPayroll != 0 && !isNaN(e.ExpiringPayroll)) ? e.ExpiredAnnualizedPremium / (e.ExpiringPayroll / 100) : 0
                        var rate = (e.Payroll != 0 && !isNaN(e.ExpiringPayroll)) ? e.AnnualizedPremium / (e.Payroll / 100) : 0
                        return (expRate != 0) ? (rate / expRate) - 1 : "";
                    }
                },
                { dataField: "AppReceived", caption: 'App?', visible: false },
            ],
            "export": {
                fileName: 'Renewals'
            },
            filterRow: {
                visible: true,
                showOperationChooser: false
            },
            summary: {
                texts: { sum: "{0}" },
                calculateCustomSummary: calculateCustomSummaries,
                totalItems: [
                    {
                        name: 'customWrittenPremium',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'WrittenPremium',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customBookedPremium',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'Booked',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customExpiringPremium',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'ExpiringWrittenPremium',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customExpiringPayroll',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'ExpiringPayroll',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customPayroll',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'Payroll',
                        valueFormat: 'currency'
                    },
                ],
                groupItems: [
                    {
                        name: 'customWrittenPremium',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'WrittenPremium',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customBookedPremium',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'Booked',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customExpiringPremium',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'ExpiringWrittenPremium',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customExpiringPayroll',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'ExpiringPayroll',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customPayroll',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'Payroll',
                        valueFormat: 'currency'
                    },
                ]
            },
            onRowClick: function (e) {
                var component = e.component,
                    prevClickTime = component.lastClickTime;
                component.lastClickTime = new Date();
                if (prevClickTime && (component.lastClickTime - prevClickTime < 300)) {
                    ora.Risk.RiskEdit(e.data.RiskId);
                }
            },
            paging: { enabled: false },
            pager: {
                showPageSizeSelector: false,
                visible: false
            },
            onRowPrepared: function (info) {

                if (info.rowType === 'data') {
                    //Record Date
                    var policyDate = new Date(info.data.DisplayEffectiveDate);
                    var policyDay = policyDate.getDay();
                    if (policyDay <= 9)
                        policyDay = '0' + policyDay;
                    //Filter Date
                    var smonth = $scope.consoleSettings.month.Month;
                    if (smonth <= 9)
                        smonth = '0' + smonth;
                    var FilterDate = new Date($scope.year, smonth, policyDay);

                    //Get Remaining Days
                    var oneDay = 24*60*60*1000;
                    var diffDays = Math.round(Math.abs((FilterDate.getTime() - policyDate.getTime()) / (oneDay)));

                    console.log(diffDays);

                    if (info.data.Status == "Canceled" || info.data.Status == "Cancelled") {
                        //if (diffDays <= 90) {
                            info.rowElement.addClass('danger');
                        //}
                    }
                    if (info.data.AppReceived) {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('warning');//Orange
                        }
                    }
                    if (info.data.Status == "Declined") {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('');
                        }
                    }
                    if (info.data.Status == "Issued") {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('success');//Green
                        }
                    }
                    if (info.data.Status == "Quote") {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('primary'); //Blue
                        }
                    }
                    if (info.data.Status == "Lost") {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('danger'); //Red
                        }
                    }
                    if (info.data.Status == "Bound") {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('bound');
                        }
                    }
                    else {
                        info.rowElement.addClass('');
                    }
                }
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.riskGridCurrRiskId = row.RiskId;
                } else {
                    $scope.riskGridCurrRiskId = 0;
                }
            }
        };

        //New Business Grid Settings
        //Product Line Console
        $scope.newBusinessGridSettings = {
            dataSource: $scope.NewBusiness,
            bindingOptions: { dataSource: 'NewBusiness' },
            onContentReady: function (obj, ele, model) {
                $scope.viewing = $scope.views[2];
                //$scope.$apply()
            },
            loadPanel: { enabled: false },
            columns: [
                { dataField: "RiskId", caption: 'Id', width: 50, visible: false },
                //{ dataField: "BusinessType", caption: 'Type', groupIndex: 1, sortOrder: 'desc' },
                { dataField: "Branch", caption: 'Branch', groupIndex: 1 },
                //{ dataField: "ProgramType", caption: 'Prgm Type', width: 75 },
                { dataField: "UW", caption: 'UW', width: 75 },
                { dataField: 'DisplayEffectiveDate', caption: 'Effective', width: 80, dataType: 'date' },
                //{ dataField: 'PolicyNumber', caption: 'Policy #', width: 110 },
                { dataField: "Name", caption: 'Named Insured' },
                { dataField: "AccountDescription", caption: 'Acct Desc', width: 100 },
                { dataField: "PurposeOfUse", caption: 'Use', width: 100 },
                { dataField: "AgencyName", caption: 'Broker' },
                { dataField: "Status", caption: 'Status', width: 130 },
                { dataField: "ScheduledRating", caption: 'Sch Rating', width: 100 },
                { dataField: "ExpirienceModifier", caption: 'X Mod', width: 100 },
                { dataField: "Payroll", caption: 'Payroll', format: 'currency', width: 130 },
                { dataField: "WrittenPremium", caption: 'Written', format: 'currency', width: 120 },
                { dataField: "DepositPremium", caption: 'Deposit', format: 'currency', width: 120 },
                {
                    caption: 'Booked', format: 'currency', width: 100, calculateCellValue: function (e) {
                        return (e.IsPaidInInstallments || e.IsReporter) ? e.DepositPremium : e.WrittenPremium;
                    }
                },
                { dataField: "AppReceived", caption: 'App?', width: 80, visible: false },
                { dataField: "Market", caption: 'Market', width: 150 },
            ],
            'export': {
                fileName: 'NewBusiness'
            },
            filterRow: {
                visible: true,
                showOperationChooser: false
            },
            summary: {
                texts: { sum: "{0}" },
                calculateCustomSummary: calculateCustomSummaries,
                totalItems: [
                    {
                        name: 'customWrittenPremium',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'WrittenPremium',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customBookedPremium',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'Booked',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customPayroll',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'Payroll',
                        valueFormat: 'currency'
                    },
                ],
                groupItems: [
                    {
                        name: 'customWrittenPremium',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'WrittenPremium',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customBookedPremium',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'Booked',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'customPayroll',
                        summaryType: 'custom',
                        showInGroupFooter: true,
                        showInColumn: 'Payroll',
                        valueFormat: 'currency'
                    },
                ]
            },
            onRowClick: function (e) {
                var component = e.component,
                    prevClickTime = component.lastClickTime;
                component.lastClickTime = new Date();
                if (prevClickTime && (component.lastClickTime - prevClickTime < 300)) {
                    ora.Risk.RiskEdit(e.data.RiskId);
                }
            },
            paging: { enabled: false },
            pager: {
                showPageSizeSelector: false,
                visible: false
            },
            onRowPrepared: function (info) {
                
                if (info.rowType === 'data') {

                    //Record Date
                    var policyDate = new Date(info.data.DisplayEffectiveDate);
                    var policyDay = policyDate.getDay();
                    if (policyDay <= 9)
                        policyDay = '0' + policyDay;
                    //Filter Date
                    var smonth = $scope.consoleSettings.month.Month;
                    if (smonth <= 9)
                        smonth = '0' + smonth;
                    var FilterDate = new Date($scope.year, smonth, policyDay);

                    //Get Remaining Days
                    var oneDay = 24 * 60 * 60 * 1000;
                    var diffDays = Math.round(Math.abs((FilterDate.getTime() - policyDate.getTime()) / (oneDay)));

                    console.log(diffDays);

                    if (info.data.Status == "Declined") {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('default');
                        }
                    }
                    else if (info.data.Status == "Lost" || info.data.Status == "Canceled") {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('danger'); //Red
                        }
                    }
                    else if (info.data.Status == "Quote") {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('primary'); //Blue
                        }
                    }
                    else if (info.data.Status == "Issued") {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('success'); //Green
                        }
                    }
                    else if (info.data.Status == "Submission") {
                        if (diffDays <= 90) {
                            info.rowElement.addClass('warning'); //Orange
                        }
                    }
                    else if (info.data.Status == "Bound") {
                        if (diffDays <= 90) {
                            if (diffDays <= 90) {
                                info.rowElement.addClass('primary'); //Blue
                            }
                        }
                    }

                    else {
                        info.rowElement.addClass('')
                    }
                }
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.riskGridCurrRiskId = row.RiskId;
                } else {
                    $scope.riskGridCurrRiskId = 0;
                }
            }
        };

        $scope.exportRenewals = function () { $('#plc_renewalRisks').dxDataGrid('instance').exportToExcel(false); };
        $scope.exportNewBusiness = function () { $('#plc_newBusinessRisks').dxDataGrid('instance').exportToExcel(false); };

        $scope.riskView = function (e) {
            if ($scope.riskGridCurrRiskId) {
                ora.Risk.RiskEdit($scope.riskGridCurrRiskId);
            }
        };

        $scope.parseFloat = function (value) {
            return parseFloat(value);
        }

        $scope.formatNumeric = function (value) {
            var number = (value != null && value != "undefined") ? value : 0;
            return parseFloat(number.toString().replace(/[^\d]/, "").replace(",", ""));
        }

        $scope.forecastedPremiums = [];

        function getForecastAmount() {
            var amount;
            var forecastedPremium = $filter('filter')($scope.forecastedPremiums, { month: $scope.month.Month }, true)[0];
            if (forecastedPremium == null) {
                $scope.forecastedPremiums.push({ month: $scope.month.Month, amount: $scope.data.REOutstandingPremium });
                amount = $scope.data.REOutstandingPremium;
            } else {
                amount = forecastedPremium.amount;
            }
            return amount;
        }

        function setForecastAmount(newValue) {
            var amount;
            if (newValue != undefined) {
                var forecastedPremiums = $filter('filter')($scope.forecastedPremiums, { month: '!' + $scope.month.Month });
                //Don't add the year summary amount into the array. We only need it month by month.
                if ($scope.month.Month != 0) {
                    forecastedPremiums.push({ month: $scope.month.Month, amount: newValue })
                }
                $scope.forecastedPremiums = forecastedPremiums;
                $scope.totalForecastAmount = $filter('sumList')($filter('filter')($scope.forecastedPremiums, function (value) { return value.month < $scope.month.Month }), 'amount');
            }
        }

        $scope.$watch("forecastAmount", function (newValue, oldValue) {
            setForecastAmount(newValue);
        });

        function getPriorOutstanding() {
            $scope.priorOutstanding = $filter('sumList')(data.priorOutstanding, 'amount');
        }

        $scope.initConsole = function () {
            $scope.viewing = $scope.views[0];
            $http.get('/policytracker/api/Console/ProductLineConsole', {
                params: { uwId: $scope.underwriter.UserId, pl: $scope.productLine.Name, year: $scope.year, month: $scope.month.Month, branch: $scope.branch.branch }
            }).then(function (response, textStatus, jqXHR) {
                $scope.data = response.data.data;
                $scope.branchSummary = response.data.BranchSummary;
                $scope.impactNotes = response.data.ImpactNotes;

                //Setting and calculating the forecast premium amount
                angular.forEach($scope.data.priorOutstanding, function (obj, key) {
                    if ($filter('filter')($scope.forecastedPremiums, { month: obj.month })[0] == null)
                        $scope.forecastedPremiums.push(obj);
                })

                $scope.forecastAmount = getForecastAmount();
                $scope.totalForecastAmount = $filter('sumList')($filter('filter')($scope.forecastedPremiums, function (value) { return value.month < $scope.month.Month }), 'amount');

                //Setting the risk list grids
                if ($scope.month.Month != 0) {
                    $scope.Renewals = response.data.Renewals;
                    if (orAero.getLastGeneratedRenewalDate() >= new Date($scope.year, $scope.month.Month - 1, 1)) {
                        $scope.NewBusiness = response.data.NewBusiness;
                    } else {
                        console.log('nope')
                        $scope.NewBusiness = [];
                    }
                }

                $scope.showHideColumns();

                if (($scope.Renewals.length == 0 && $scope.NewBusiness.length == 0) || $scope.month.Month == 0)
                    $scope.viewing = $scope.views[2];

                //Syncing console settings with month select toolbar
                $scope.consoleSettings.productLine = $scope.productLine;
                $scope.consoleSettings.underwriter = $scope.underwriter;
                $scope.consoleSettings.year = $scope.year;
                $scope.consoleSettings.month = $scope.month;
            }, function (response, textStatus, errorThrown) {
                $scope.viewing = $scope.views[1];
            });
        };

        //Get the Current User When the Page is Loading & Init Values
        $http.get('/policytracker/api/User/GetUnderwriters').then(function (response) {
            $scope.Underwriters = response.data;

            $http.get('/policytracker/api/User/GetCurrentUser').then(function (response) {
                $scope.underwriter = $scope.Underwriters[0];

                for (var i = 0; i < $scope.Underwriters.length; i++) {
                    if (response.data.UserId == $scope.Underwriters[i].UserId) {
                        $scope.underwriter = response.data;
                    }
                }

                $http.get('/policytracker/api/Risk/GetProductLines').then(function (response) {
                    $scope.ProductLines = response.data;
                    var userPL = $scope.underwriter.ProductLine;

                    if (userPL) {
                        for (var i = 0; i < $scope.ProductLines.length; i++) {
                            if ($scope.ProductLines[i].ProductLineId == userPL) $scope.productLine = $scope.ProductLines[i];
                        }
                    } else if (!userPL) {
                        $scope.productLine = $scope.ProductLines[0];
                    }

                    $scope.initConsole();
                });
            });
        });

        $scope.updateConsoleSettings = function () {
            if ($scope.resetForecast) {
                $scope.forecastedPremiums = [];
                $scope.resetForecast = false;
            }

            $scope.month = $scope.consoleSettings.month;
            $scope.productLine = $scope.consoleSettings.productLine;
            $scope.underwriter = $scope.consoleSettings.underwriter;
            if ($scope.year != $scope.consoleSettings.year) {
                $scope.forecastedPremiums = [];
            }
            $scope.year = $scope.consoleSettings.year;
            $scope.branch = $scope.consoleSettings.branch;

            $scope.initConsole();
        }
    }]);
})();