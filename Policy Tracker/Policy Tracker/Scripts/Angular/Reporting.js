//(function () {
//    angular.module('reporting.templates', ['/template/reports/testreport.html']);

//    angular.module('/template/reports/testreport.html', []).run(["$templateCache", function ($templateCache) {
//        $templateCache.put('/template/reports/testreport.html',
//            '<div>Test Report Template</div>');
//    }]);
//})();

(function () {
    var app = angular.module('ORA.Reporting', ['ngRoute', 'dx', 'appFactories']);

    app.controller('reportsCtrl', ['$scope', function ($scope) {}]);

    app.controller('aircraftCtrl', ['$scope', '$http', 'dateTimeFactory', 'aircraftFactory', 'orAero',
        function ($scope, $http, dateTimeFactory, aircraftFactory, orAero) {
        $scope.summary = {};
        $scope.filters = {};
        var engineTypes = aircraftFactory.engineTypes;
        var months = dateTimeFactory.getMonths();
        var years = dateTimeFactory.getYears();

        orAero.productLines().then(function (data) {
            var grid = $("#aircraftLookup").dxDataGrid("instance");
            var lookup = grid.columnOption("ProductLine", "lookup");
            $scope.productLines = data.data;
            lookup.dataSource = data.data;
            grid.columnOption("ProductLine", "lookup", lookup);
        })

        orAero.purposesOfUse().then(function (data) {
            var grid = $("#aircraftLookup").dxDataGrid("instance");
            var lookup = grid.columnOption("PurposeOfUse", "lookup");
            $scope.purposesOfUse = data.data;
            lookup.dataSource = data.data;
            grid.columnOption("PurposeOfUse", "lookup", lookup);
        })

        $scope.updateSummary = function (params) {
            $http.get('/policytracker/api/aircraft/GetAircraftSummary', { params: params }).then(
                    function (response) {
                        $scope.summary = response.data;
                    });
        };

        $scope.gridRefresh = function () {
            var dataGrid = $('#aircraftLookup').dxDataGrid('instance');
            dataGrid.refresh();
        };

        $scope.grid = {
            dataSource: {
                load: function (loadOptions) {
                    var d = $.Deferred();
                    var params = {
                        pageNumber: (loadOptions.skip / loadOptions.take) + 1,
                        pageSize: loadOptions.take,
                    };
                    
                    if (loadOptions.sort) {
                        //Remove the word Display from the property name to use the actual property and not the customized Facade Property
                        params.sortProperty = loadOptions.sort[0].selector.replace('Display', '');
                        params.sortOrder = loadOptions.sort[0].desc ? 'desc' : 'asc';
                    }

                    if (loadOptions.filter) {
                        for (var i = 0; i < loadOptions.filter.length; i++) {
                            if (typeof loadOptions.filter[0] === 'string') {
                                params[loadOptions.filter[0]] = loadOptions.filter[2];
                            } else {
                                if (i % 2 == 0) {
                                    var array = loadOptions.filter[i];
                                    params[array[0]] = array[2];
                                }
                            }
                        }
                    }
                    
                    $scope.updateSummary(params);

                    //Look at effective date object and plug into the params. Remove effective date if already in the list
                    if ($scope.filters.effectiveStart && $scope.filters.effectiveEnd) {
                        console.log('Applying effective date range filters');
                        params.EffectiveDate = $scope.filters.effectiveStart + '|' + $scope.filters.effectiveEnd
                    }
                    console.log(params)
                    $.getJSON('/policytracker/api/aircraft/LookupAircraft', params)
                        .done(function (data) {
                            d.resolve(data.Results, { totalCount: data.TotalResults });
                        });

                    return d.promise();
                }
            },
            columns: [
                { dataField: "RiskId", caption: 'Id', width: 50, visible: false },
                { dataField: "FAANo", caption: 'FAA #' },
                { dataField: 'PurposeOfUse', caption: 'Use', lookup: { dataSource: $scope.purposesOfUse, valueExpr: 'Code', displayExpr: 'Name' } },
                { dataField: "Year", },
                { dataField: 'Make', },
                { dataField: 'Model' },
                { dataField: 'EngineType', caption: 'Engine', lookup: { dataSource: engineTypes, valueExpr: 'engineType', displayExpr: 'name' } },
                { dataField: 'DisplayEffectiveDate', allowFiltering: false, caption: 'Effective On', dataType: 'date', format: 'shortDate' },
                //{ dataField: 'EffectiveMonth', caption: 'Effective Month', lookup: { dataSource: months, valueExpr: 'month', displayExpr: 'name' } },
                //{ dataField: 'EffectiveYear', caption: 'Effective Year', lookup: { dataSource: years } },
                { name: 'ProductLine', dataField: 'ProductLine', caption: 'PL', lookup: { dataSource: $scope.productLines, valueExpr: 'Name', displayExpr: 'Name' } },
                { dataField: "Value", caption: 'Hull Value', format: 'currency', width: 100 },
                { dataField: "HullRate", caption: 'Rate', width: 100 },
                { dataField: "HullPrem", caption: 'Hull Prem', format: 'currency', width: 100 },
                { dataField: 'LiabPrem', caption: 'Liability Prem', format: 'currency', width: 100 },
                { dataField: 'Limit', caption: 'Liab Limit', format: 'currency', width: 100 },
                { dataField: 'isCSL', caption: 'CSL', width: 50, dataType: 'boolean' },
                { dataField: 'IsRenewal', caption: 'Renewal?', width: 50, dataType: 'boolean' },
                { dataField: 'AnnualPrem', caption: 'Annual Prem', format: 'currency', width: 100 },
                { dataField: 'Branch', caption: 'Branch', width: 70 },
                { dataField: 'Status', caption: 'Status', width: 100 },
                { dataField: 'UW', caption: 'UW', width: 100 },
            ],
            filterRow: {
                visible: true
            },
            paging: { enabled: true, pageSize: 10 },
            pager: {
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showInfo: true,
                visible: true
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.aircraftGridCurrRiskId = row.RiskId;
                } else {
                    $scope.aircraftGridCurrRiskId = 0;
                }
            },
            onRowClick: function (e) {
                var component = e.component,
                    prevClickTime = component.lastClickTime;
                component.lastClickTime = new Date();
                if (prevClickTime && (component.lastClickTime - prevClickTime < 300)) {
                    ora.Risk.RiskEdit(e.data.RiskId);
                }
            },
        };
    }])
    app.controller('installmentsCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.inforceInstallments = [];
        $scope.showingType = "Installments";
        $scope.inforceReporters = [];
        $scope.unpaid = [];
        $scope.installmentRiskId = 0;
        $scope.reporterRiskId = 0;
        $scope.popupVisible = false;
        $scope.Years = [];
        $scope.Months = [{ Month: 1, Name: 'January' }, { Month: 2, Name: 'February' }, { Month: 3, Name: 'March' }, { Month: 4, Name: 'April' }, { Month: 5, Name: 'May' }, { Month: 6, Name: 'June' }, { Month: 7, Name: 'July' }, { Month: 8, Name: 'August' }, { Month: 9, Name: 'September' }, { Month: 10, Name: 'October' }, { Month: 11, Name: 'November' }, { Month: 12, Name: 'December' }, ];
        for (var i = new Date().getFullYear() ; i >= 2013; i--) {
            $scope.Years.push(i);
        }
        $scope.showPolicies = function (type) { $scope.showingType = type; };

        //#region Installment/Reporter Policies
        $scope.installments = {
            dataSource: $scope.inforceInstallments,
            showRowLines: true,
            bindingOptions: { dataSource: 'inforceInstallments' },
            columns: [
                { dataField: 'RiskId', width: 80 },
                { dataField: 'PolicyNumber' },
                { dataField: 'UW' },
                //{ dataField: 'IsPaidInInstallments', dataType: 'boolean' },
                //{ dataField: 'IsReporter', dataType: 'boolean' },
                { dataField: 'Name' },
                { dataField: 'AgencyName' },
                { dataField: 'Branch' },
                { dataField: 'ProductLine' },
            ],
            filterRow: {
                visible: true,
                showOperationChooser: false
            },
            paging: { enabled: true, pageSize: 10 },
            pager: {
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showInfo: true,
                visible: true
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.installmentRiskId = row.RiskId;
                } else {
                    $scope.installmentRiskId = 0;
                }
            },
            masterDetail: {
                enabled: true,
                template: "detail"
            }
        };

        $scope.reporters = {
            dataSource: $scope.inforceReporters,
            showRowLines: true,
            bindingOptions: { dataSource: 'inforceReporters' },
            columns: [
                { dataField: 'RiskId', width: 80 },
                { dataField: 'PolicyNumber' },
                { dataField: 'UW' },
                //{ dataField: 'IsPaidInInstallments', dataType: 'boolean' },
                //{ dataField: 'IsReporter', dataType: 'boolean' },
                { dataField: 'Name' },
                { dataField: 'AgencyName' },
                { dataField: 'Branch' },
                { dataField: 'ProductLine' },
            ],
            filterRow: {
                visible: true,
                showOperationChooser: false
            },
            paging: { enabled: true, pageSize: 10 },
            pager: {
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showInfo: true,
                visible: true
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.reporterRiskId = row.RiskId;
                } else {
                    $scope.reporterRiskId = 0;
                }
            },
            masterDetail: {
                enabled: true,
                template: "detail"
            }
        };

        $scope.viewPolicy = function (e) {
            if (e) {
                var riskId = e.element[0].id == 'viewInstallment' ? $scope.installmentRiskId : $scope.reporterRiskId;
                if (riskId) {
                    ora.Risk.RiskEdit(riskId);
                }
            }  
        };
        //#endregion
        $scope.exportPayments = function () {
            var grid = $('#paymentsGrid').dxDataGrid('instance');
            console.log(grid);
            grid.exportToExcel(false);
        };

        $scope.payments = {
            dataSource: $scope.unpaid,
            showRowLines: true,
            bindingOptions: { dataSource: 'unpaid' },
            columns: [
                { dataField: 'Insured' },
                { dataField: 'PolicyNumber', width: 75 },
                { dataField: 'Branch' },
                { dataField: 'ProductLine' },
                { dataField: 'AgencyName' },
                { dataField: 'Type' },
                { dataField: 'UA', groupIndex: 1 },
                { dataField: 'AnticipatedAmount', format: 'currency' },
                { dataField: 'ActualAmount', format: 'currency' },
                //{ dataField: 'InvoicedDate', dataType: 'date' },
                { dataField: 'DueDate', dataType: 'date', width: 110 },
                { dataField: 'ReportReceived', dataType: 'boolean', width: 115 },
                { dataField: 'DueDateMonth', caption: 'Month Due', lookup: { dataSource: $scope.Months, valueExpr: 'Month', displayExpr: 'Name' }, width: 75 },
                { dataField: 'DueDateYear', caption: 'Year Due', lookup: { dataSource: $scope.Years }, width: 75 },
            ],
            "export": { enabled: false, fileName: "Unpaid Installments and Reporters", allowExportSelectedData: false },
            filterRow: {
                visible: true,
                showOperationChooser: true
            },
            paging: { enabled: true, pageSize: 10 },
            pager: {
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showInfo: true,
                visible: true
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.topInforceRiskId = row.RiskId;
                } else {
                    $scope.topInforceRiskId = 0;
                }
            },
            summary: {
                texts: { sum: "{0}" },
                totalItems: [
                    {
                        name: 'AnticipatedAmount',
                        summaryType: 'sum',
                        showInGroupFooter: true,
                        column: 'AnticipatedAmount',
                        showInColumn: 'AnticipatedAmount',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'ActualAmount',
                        summaryType: 'sum',
                        showInGroupFooter: true,
                        showInColumn: 'ActualAmount',
                        column: 'ActualAmount',
                        valueFormat: 'currency'
                    }
                ],
                groupItems: [
                    {
                        name: 'AnticipatedAmount',
                        summaryType: 'sum',
                        showInGroupFooter: true,
                        column: 'AnticipatedAmount',
                        showInColumn: 'AnticipatedAmount',
                        valueFormat: 'currency'
                    },
                    {
                        name: 'ActualAmount',
                        summaryType: 'sum',
                        showInGroupFooter: true,
                        showInColumn: 'ActualAmount',
                        column: 'ActualAmount',
                        valueFormat: 'currency'
                    }
                ]
            }
        };

        $scope.initialize = function () {
            $http.get('/policytracker/api/Installments/InforceInstallments', {}).then(function (response) { $scope.inforceInstallments = response.data; });
            $http.get('/policytracker/api/Installments/InforceReporters', {}).then(function (response) { $scope.inforceReporters = response.data; });
            $http.get('/policytracker/api/Installments/unpaid', {}).then(function (response) { $scope.unpaid = response.data; });
        };

        $scope.initialize();
    }]);
    app.controller('unResolvedCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.export = function () { $('#unResolvedRisks').dxDataGrid('instance').exportToExcel(false); };
        $scope.risks = [];
        $scope.gridSettings = {
            dataSource: $scope.risks,
            showRowLines: true,
            bindingOptions: { dataSource: 'risks' },
            columns: [
                { dataField: 'Status' },
                { dataField: 'RiskId', width: 80 },
                { dataField: 'PolicyNumber' },
                { dataField: 'UW' },
                { dataField: 'UA' },
                //{ dataField: 'IsPaidInInstallments', dataType: 'boolean' },
                //{ dataField: 'IsReporter', dataType: 'boolean' },
                { dataField: 'Name' },
                { dataField: 'AgencyName' },
                { dataField: 'Branch' },
                { dataField: 'EffectiveDate', dataType: 'date', format: 'shortDate' },
            ],
            "export": { enabled: false, fileName: "UnResolvedRisks", allowExportSelectedData: false },
            filterRow: {
                visible: true,
                showOperationChooser: false
            },
            paging: { enabled: true, pageSize: 20 },
            pager: {
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showInfo: true,
                visible: true
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.riskId = row.RiskId;
                } else {
                    $scope.riskId = 0;
                }
            },
            onRowPrepared: function (info) {
                if (info.rowType === 'data') {
                    var today = new Date();
                    
                    var effective = new Date(info.data.EffectiveDate.replace('T00:00:00', ''));
                    var diff = (today - effective) / 86400000;

                    if (diff < 0 && diff < -15)
                        info.rowElement.addClass('success');
                    else if (diff < 0 && diff >= -15)
                        info.rowElement.addClass('warning');
                    else if (diff >= 0)
                        info.rowElement.addClass('danger');
                }
            },
            height: '100%'
        };

        $scope.viewRisk = function () {
            if ($scope.riskId) {
                ora.Risk.RiskEdit($scope.riskId);
            }
        };

        $http.get('/policytracker/api/risk/GetUnResolvedRisks', {}).then(function (response) { $scope.risks = response.data; });
    }]);
    app.controller('targetAccountsCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.export = function () { $('#targetAccounts').dxDataGrid('instance').exportToExcel(false); };
        $scope.risks = [];
        $scope.gridSettings = {
            dataSource: $scope.risks,
            showRowLines: true,
            bindingOptions: { dataSource: 'risks' },
            columns: [
                { dataField: 'RiskId', width: 80 },
                { dataField: 'Name', caption: 'Named Insured' },
                { dataField: 'AgencyName' },
                { dataField: 'EffectiveDate', dataType: 'date', format: 'shortDate', selectedFilterOperation: 'between' },
                { dataField: "WrittenPremium", caption: 'Premium', format: 'currency' },
                { dataField: 'UW' },
                { dataField: 'Branch' },
                
            ],
            "export": { enabled: false, fileName: "TargetAccounts", allowExportSelectedData: false },
            filterRow: {
                visible: true,
                showOperationChooser: false
            },
            paging: { enabled: true, pageSize: 20 },
            pager: {
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showInfo: true,
                visible: true
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.riskId = row.RiskId;
                } else {
                    $scope.riskId = 0;
                }
            },
            height: '100%'
        };

        $scope.viewRisk = function () {
            if ($scope.riskId) {
                ora.Risk.RiskEdit($scope.riskId);
            }
        };

        $http.get('/policytracker/api/risk/GetTargetAccounts', {}).then(function (response) { $scope.risks = response.data; });
    }]);
    app.controller('monthlyExpiringAccountsCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.export = function () { $('#monthlyExpiringAccounts').dxDataGrid('instance').exportToExcel(false); };
        $scope.risks = [];
        $scope.gridSettings = {
            dataSource: $scope.risks,
            showRowLines: true,
            bindingOptions: { dataSource: 'risks' },
            columns: [
                { dataField: 'RiskId', width: 80, visible: false },
                { dataField: 'DisplayEffectiveDate', caption: 'Effective', width: 80, dataType: 'date' },
                { dataField: 'PolicyNumber', caption: 'Policy #' },
                { dataField: 'UW' },
                { dataField: 'Name' },
                { dataField: 'Status', },
                { dataField: "AnnualizedPremium", caption: 'Premium', format: 'currency' },
            ],
            "export": { enabled: false, fileName: "monthlyExpiringAccounts", allowExportSelectedData: false },
            filterRow: {
                visible: true,
                showOperationChooser: true
            },
            paging: { enabled: true, pageSize: 20 },
            pager: {
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showInfo: true,
                visible: true
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.riskId = row.RiskId;
                } else {
                    $scope.riskId = 0;
                }
            },
            height: '100%'
        };

        $scope.viewRisk = function () {
            if ($scope.riskId) {
                ora.Risk.RiskEdit($scope.riskId);
            }
        };

        $http.get('/policytracker/api/risk/GetMonthlyExpiringAccounts', {}).then(function (response) { $scope.risks = response.data; });
    }]);

    app.config(['$locationProvider', '$routeProvider', function ($locationProvider, $routeProvider) {
        $routeProvider
            .when('/aircraftWorkingList', {
                templateUrl: '/Policytracker/Reporting/AircraftLookup',
                controller: 'aircraftCtrl',
            })
            .when('/installments', {
                templateUrl: '/Policytracker/Reporting/installments',
                controller: 'installmentsCtrl',
            })
            .when('/unresolved', {
                templateUrl: '/Policytracker/Reporting/unresolved',
                controller: 'unResolvedCtrl',
            })
            .when('/targetaccounts', {
                templateUrl: '/Policytracker/Reporting/targetaccounts',
                controller: 'targetAccountsCtrl',
            })
            .when('/monthlyexpiringaccounts', {
                templateUrl: '/Policytracker/Reporting/monthlyexpiringaccounts',
                controller: 'monthlyExpiringAccountsCtrl',
            })
            .otherwise({
                templateUrl: '/Policytracker/Reporting/ReportListing',
                //controller: 'testCtrl',
            })
        //.otherwise({ redirectTo: '/policytracker/reporting/about' })
    }]);
    app.run(['$rootScope', '$templateCache', function ($rootScope, $templateCache) {
        $templateCache.put('template/reporting/test.html', '<div>Test</div>');
        $templateCache.put('template/reporting/aircraftWorkingList.html', '<div> aircraft Working List and Stats</div>');

        $templateCache.put('/Policytracker/Reporting/unresolved',
            '<div class="col-lg-12" style="margin:5px 0px"> ' +
                '<a href="#" role="button" class="btn btn-danger"><i class="fa fa-arrow-left"></i> Reports</a>' +
            '</div>' +
            '<div class="col-lg-12"> ' +
                '<div class="box box-solid">' +
                    '<div class="box-header with-border">' +
                        '<h3 class="box-title">Un-Resolved Risks</h3> ' +
                        '<div class="pull-right"><button type="button" class="btn btn-primary btn-sm" ng-click="export()"><i class="fa fa-file-excel-o"></i></button></div>' +
                    '</div>' +
                    '<div class="box-body" >' +
                        '<div id="unResolvedRisks" dx-data-grid="gridSettings"></div>' +
                    '</div>' +
                    '<div class="box-footer">' +
                        '<button type="button" class="btn btn-primary btn-block btn-lg" ng-click="viewRisk()"><i class="fa fa-search"></i> View Selected Risk</button>' +
                    '</div>' +
                '</div>' +
            '</div>'
        );

        $templateCache.put('/Policytracker/Reporting/targetaccounts',
            '<style> .dx-datagrid-rowsview .dx-row:nth-child(odd) { background-color: #D3D3D3; color: black; } </style>' +
            '<div class="col-lg-12" style="margin:5px 0px"> ' +
                '<a href="#" role="button" class="btn btn-danger"><i class="fa fa-arrow-left"></i> Reports</a>' +
            '</div>' +
            '<div class="col-lg-12"> ' +
                '<div class="box box-solid">' +
                    '<div class="box-header with-border">' +
                        '<h3 class="box-title">Target Accounts</h3> ' +
                        '<div class="pull-right"><button class="btn btn-primary btn-sm" ng-click="export()"><i class="fa fa-file-excel-o"></i></button></div>' +
                    '</div>' +
                    '<div class="box-body" >' +
                        '<div id="targetAccounts" dx-data-grid="gridSettings"></div>' +
                    '</div>' +
                    '<div class="box-footer">' +
                        '<button type="button" class="btn btn-primary btn-block btn-lg" ng-click="viewRisk()"><i class="fa fa-search"></i> View Selected Risk</button>' +
                    '</div>' +
                '</div>' +
            '</div>'
        );

        $templateCache.put('/Policytracker/Reporting/monthlyexpiringaccounts',
            '<style> .dx-datagrid-rowsview .dx-row:nth-child(odd) { background-color: #D3D3D3; color: black; } </style>' +
            '<div class="col-lg-12" style="margin:5px 0px"> ' +
                '<a href="#" role="button" class="btn btn-danger"><i class="fa fa-arrow-left"></i> Reports</a>' +
                '<div class="pull-right"><button class="btn btn-primary btn-sm" ng-click="export()"><i class="fa fa-file-excel-o"></i></button></div>' +
            '</div>' +
            '<div class="col-lg-12"> ' +
                '<div class="box box-solid">' +
                    '<div class="box-header with-border">' +
                        '<h3 class="box-title">Monthly Expiring Accounts</h3> ' +
                    '</div>' +
                    '<div class="box-body" >' +
                        '<div id="monthlyExpiringAccounts" dx-data-grid="gridSettings"></div>' +
                    '</div>' +
                    '<div class="box-footer">' +
                        '<button type="button" class="btn btn-primary btn-block" ng-click="viewRisk()"><i class="fa fa-search"></i> View Selected Risk</button>' +
                    '</div>' +
                '</div>' +
            '</div>'
        );

        $rootScope.$on('$locationChangeStart', function (event, next, current) {
            console.log(next);
        });
    }]);
})();
