(function () {
    var app = angular.module('ORA.UA', ['ngRoute', 'dx', 'ui.bootstrap']);

    app.controller('moduleController', ['$scope', '$route', function ($scope, $route) {
        $scope.route = $route;
    }]);

    app.controller('riskLookupController', ['$scope', '$http', function ($scope, $http) {
        $scope.assistant = {};
        $scope.riskId = 0;
        $scope.risk = {};
        $scope.productLines = [];

        var statuses = [
            { Value: 'Submission', DisplayText: 'Submission' },
            { Value: 'Already Involved', DisplayText: 'Already Involved' },
            { Value: 'Declined', DisplayText: 'Declined' },
            { Value: 'Quote', DisplayText: 'Quote' },
            { Value: 'Bound', DisplayText: 'Bound' },
            { Value: 'Issued', DisplayText: 'Issued' },
            { Value: 'Canceled', DisplayText: 'Canceled' },
            { Value: 'Lost', DisplayText: 'Lost' },
        ];
        var underwriterDataSource = [];
        var underwriterLoad = function (loadOptions) {
            var d = $.Deferred();
            $.getJSON('/policytracker/api/User/GetUnderwriters', {}).done(function (data) { d.resolve(data, { totalCount: data.TotalResults }); });
            return d.promise();
        }
        var underwriterByKey = function(key) {
            var d = new $.Deferred();
            $.get('http://data.example.com/products?id=' + key)
                .done(function (result) {
                    d.resolve(result[i]);
                });
            return d.promise();
        };

        //#region Risk Lookup Grid Settings
        $scope.riskLookupRisks = [];
        $scope.riskLookupSettings = {
            dataSource: {
                load: function (loadOptions) {
                    var d = $.Deferred();
                    var params = {
                        pageNumber: (loadOptions.skip / loadOptions.take) + 1,
                        pageSize: loadOptions.take,
                    };

                    if (loadOptions.sort) {
                        params.sortProperty = loadOptions.sort[0].selector;
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

                    $scope.searchCriteria = params;
                    $.getJSON('/policytracker/api/Assistants/LookupRisks', params).done(function (data) { d.resolve(data.Results, { totalCount: data.TotalResults }); });
                    return d.promise();
                }
            },
            columns: [
                { dataField: "Status", lookup: { dataSource: statuses, valueExpr: 'Value', displayExpr: 'DisplayText' } },
                { dataField: "ImageRightId", },
                { dataField: "Name", },
                { dataField: "AgencyName", },
                { dataField: 'UW', caption: 'Underwriter', lookup: { dataSource: { load: underwriterLoad, byKey: underwriterByKey }, valueExpr: 'Name', displayExpr: 'Name' } },
                { dataField: 'UA', caption: 'Assistant', lookup: { dataSource: { load: underwriterLoad, byKey: underwriterByKey }, valueExpr: 'Name', displayExpr: 'Name' } },
                { dataField: "PolicyNumber", },
                { dataField: 'EffectiveDate', caption: 'Effective', dataType: 'date', format: 'shortDate' },
                { dataField: "RiskId", caption: 'Risk Id' },
                { dataField: 'IsRenewal', caption: 'Is Renewal', width: 90, dataType: 'boolean' },
            ],
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
                    $http.get('/policytracker/api/Risk/getrisk', { params: { id: row.RiskId } }).then(function (response) { $scope.risk = response.data; });
                } else {
                    $scope.riskId = 0;
                }
            },
            cacheEnabled: false,
            height: '100%'
        };
        //#endregion

        $scope.viewRisk = function (e) {
            if ($scope.riskId) {
                $scope.visiblePopup = true;
            }
        }

        $scope.editRisk = function (e) {
            if ($scope.riskId) {
                ora.Risk.RiskEdit($scope.riskId);
            }
        }

        $scope.refreshRisks = function () {
            $('#RiskLookup').dxDataGrid('instance').columnOption('UA', 'filterValue', '');
            $('#RiskLookup').dxDataGrid('instance').filter('');
            //$('#RiskLookup').dxDataGrid('instance').refresh();
        };

        $scope.popupOptions = {
            width: '80%',
            height: '60%',
            animation: false,
            contentTemplate: "info",
            showTitle: false,
            title: "Information",
            dragEnabled: false,
            closeOnOutsideClick: true,
            bindingOptions: {
                visible: "visiblePopup",
            },
        };

        //#region Filters
        $scope.filterPopupOptions = {
            width: '80%',
            height: '60%',
            animation: false,
            contentTemplate: "info",
            showTitle: false,
            title: "Information",
            dragEnabled: false,
            closeOnOutsideClick: true,
            bindingOptions: {
                visible: "filterPopupVisible",
            },
        };

        $scope.viewfilters = function (e) {
                $scope.filterPopupVisible = true;
        }

        $scope.minDate = {
            opened: false,
            value: null
        };

        $scope.maxDate = {
            opened: false,
            value: null
        };
        $scope.openMinDate = function () { $scope.minDate.opened = true; };
        $scope.openMaxDate = function () { $scope.maxDate.opened = true; };
        //#endregion

        $scope.init = function () {
            $http.get('/policytracker/api/User/GetCurrentUser').then(function (response) {
                $scope.assistant = response.data;
                $('#RiskLookup').dxDataGrid('instance').columnOption('UA', 'filterValue', $scope.assistant.Name);
            });
            $http.get('/policytracker/api/risk/GetProductLines').then(function (response) {
                $scope.productLines = response.data;
            });
        };

        $scope.init();
    }]);

    app.config(['$locationProvider', '$routeProvider', function ($locationProvider, $routeProvider) {
        $routeProvider
            .when('/WorkingList', {
                templateUrl: '/Policytracker/assistants/risklookup',
                controller: 'riskLookupController',
                activetab: 'WorkingList'
            })
            .when('/Payments', {
                templateUrl: '/Policytracker/assistants/Payments',
                activetab: 'Payments'
            })
            .when('/Endorsements', {
                templateUrl: '/Policytracker/assistants/Endorsements',
                activetab: 'Endorsements'
            })
            .otherwise({
                templateUrl: '/Policytracker/assistants/risklookup',
                activetab: 'WorkingList'
            })
        //.otherwise({ redirectTo: '/policytracker/reporting/about' })
    }]);

    app.run(['$rootScope', '$templateCache', function ($rootScope, $templateCache) {
        $rootScope.$on('$locationChangeStart', function (event, next, current) {});
    }]);
})();