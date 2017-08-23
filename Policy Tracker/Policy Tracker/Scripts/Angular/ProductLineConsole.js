(function () {
    var app = angular.module('ProductLineConsole', ['dx', 'appFactories', 'ui.bootstrap']);

    app.controller('consoleController', ['$scope', '$http', 'orAero', 'dateTimeFactory', '$filter', function ($scope, $http, orAero, dtf, $filter) {
        var oceanPalette = DevExpress.viz.getPalette('Pastelle');
        $scope.productLine = {};
        $scope.workloadStats = [];
        $scope.monthDetails = [];
        $scope.topTwentyOutstandingQuotes = [];
        $scope.workloadViews = ['monthSummary', 'uwSummary', 'graphicalMonthSummary', 'graphicalMonthDeail', 'graphicalUWDetail']
        $scope.workloadView = $scope.workloadViews[2];

        orAero.productLines().then(function (response) { $scope.productLines = response.data; });

        $scope.outStandingQuotesGridConfig = {
            dataSource: $scope.topTwentyOutstandingQuotes,
            bindingOptions: { dataSource: 'topTwentyOutstandingQuotes' },
            columns: [
                { dataField: "Name", caption: 'Named Insured' },
                { dataField: "AgencyName", caption: 'Broker' },
                { dataField: 'EffectiveDate', dataType: 'date', format: 'shortDate', width: 100 },
                { dataField: 'PolicyNumber', width: 120 },
                { dataField: "AnnualizedPremium", caption: 'Annual Prem', format: 'currency', width: 100 }],
            paging: { enabled: true, pageSize: 10 },
            pager: {
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20],
                showInfo: true,
                visible: true
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.topQuoteRiskId = row.RiskId;
                } else {
                    $scope.topQuoteRiskId = 0;
                }
            }
        };

        $scope.workloadChartConfig = {
            dataSource: $scope.workloadStats,
            bindingOptions: { dataSource: 'workloadStats' },
            commonSeriesSettings: {
                argumentField: 'MonthText',
                
            },
            series: [{
                    type: 'bar',
                    axis: 'CountAxis',
                    valueField: 'TotalEstimatedWorkload',
                    name: 'Risk Count',
                },
                {
                    type: 'line',
                    axis: 'PremiumAxis',
                    valueField: 'TotalEstimatedWorkloadPremium',
                    name: 'Opportunity Premium',
                },
                {
                    type: 'line',
                    axis: 'PremiumAxis',
                    valueField: 'TotalWrittenPremium',
                    name: 'Written Premium',
                }
            ],
            argumentAxis: {
                label: { overlappingBehavior: 'stagger' }
            },
            valueAxis: [{
                    name: 'CountAxis',
                    title: {
                        text: 'Total',
                        font: { weight: 100 }
                    }
                },
                {
                    name: 'PremiumAxis',
                    position: 'right',
                    label: { format: 'currency' },
                    title: {
                        text: 'Premium',
                        font: { weight: 100 }
                    }
                }
            ],
            tooltip: {
                enabled: true,
                format: 'largeNumber',
                customizeTooltip: function (point) {
                    if (point.seriesName > 1000) {
                        return {
                            text: '$' + point.value
                        }
                    };
                }
            },
            //customizePoint: function (pointInfo) {
            //    return { color: oceanPalette.simpleSet[pointInfo.index % 7] }
            //},
            legend: {
                visible: true,
                horizontalAlignment: 'center',
                verticalAlignment: 'bottom',
            },
            onPointClick: function (e) {
                $scope.workloadView = $scope.workloadViews[3];
                $scope.monthDetails = $filter('filter')($scope.workloadStats, { MonthText: e.target.initialArgument }, true)[0].Underwriters;
                $scope.workloadSelectedMonth = $filter('filter')(dtf.getMonths(), { name: e.target.initialArgument }, true)[0].month;
                $scope.$apply()
            },
        }

        $scope.workloadChartForMonthConfig = {
            dataSource: $scope.monthDetails,
            bindingOptions: { dataSource: 'monthDetails' },
            commonSeriesSettings: {
                type: 'bar',
                argumentField: 'Name',
            }, 
            series: [{
                axis: 'CountAxis',
                valueField: 'Total',
                name: 'Count',
            },
            {
                axis: 'PremiumAxis',
                valueField: 'TotalPremium',
                name: 'Premium',
            }],
            argumentAxis: {
                label: { overlappingBehavior: 'stagger' }
            },
            valueAxis: [{
                name: 'CountAxis',
                title: {
                    text: 'Total',
                    font: { weight: 100 }
                }
            },
            {
                name: 'PremiumAxis',
                position: 'right',
                label: { format: 'currency' },
                title: {
                    text: 'Premium',
                    font: { weight: 100 }
                }
            }],
            //customizePoint: function (pointInfo) {
            //    return { color: oceanPalette.simpleSet[pointInfo.index % 7] }
            //},
            legend: {
                visible: true,
                horizontalAlignment: 'center',
                verticalAlignment: 'bottom',
            },
            onPointClick: function (e) {
                $scope.getUnderwriterDetails(e.target.originalArgument, $scope.workloadSelectedMonth);
                $scope.workloadView = $scope.workloadViews[4];
                $scope.$apply()
            },
            tooltip: {
                enabled: true,
                format: 'largeNumber',
                customizeTooltip: function (point) {
                    if (point.seriesName > 1000) {
                        return {
                            text: '$' + point.value
                        }
                    };
                }
            },
        }

        $scope.topQuoteView = function (e) {
            if ($scope.topQuoteRiskId) {
                ora.Risk.RiskEdit($scope.topQuoteRiskId);
            }
        };

        $scope.goToUnderwriterDetail = function (uw, month) {
            $scope.getUnderwriterDetails(uw, month);
            $scope.workloadView = $scope.workloadViews[1];
        }

        $scope.getUnderwriterDetails = function (uw, month) {
            $http.get('/policytracker/api/console/UnderwriterWorkloadRisksForProductLineForMonth', { params: { underwriter: uw, year: dtf.getCurrentYear(), month: month, ProductLine: $scope.productLine.Name } }).then(function (response) {
                $scope.uwDetails = {
                    uw: uw,
                    month: $filter('filter')(dtf.getMonths(), { month: month }, true)[0].name,
                    risks: response.data
                }
            });
        }

        $scope.buildReport = function () {
            $http.get('/policytracker/api/console/GetTopTwentyOutstandingQuotes', { params: { ProductLine: $scope.productLine.Name, pageSize: 20 } }).then(function (response) {
                $scope.topTwentyOutstandingQuotes = response.data.Results;
            });

            $http.get('/policytracker/api/console/GetWorkloadStatistics', { params: { productLine: $scope.productLine.Name, year: dtf.getCurrentYear() } }).then(function (res) { $scope.workloadStats = res.data })

            $http.get('/policytracker/api/Console/GetRiskStatusCounts', { params: { productLine: $scope.productLine.Name } }).then(function (response) {
                $scope.riskStatusCounts = response.data;
            });

            $http.get('/policytracker/api/Console/GetRenewalRetentionData', { params: { productLine: $scope.productLine.Name } }).then(function (response) {
                $scope.retention = response.data;
            });

            $http.get('/policytracker/api/Console/GetImpactfulEvents', { params: { productLineId: $scope.productLine.ProductLineId } }).then(function (response) {
                $scope.impactNotes = response.data;
            });
        };

        $scope.$watch("productLine", function (newValue, oldValue) {
            if ($scope.productLine.ProductLineId != 0 && $scope.productLine.ProductLineId != null) {
                $scope.buildReport();
            }
        });

        $http.get('/policytracker/api/User/GetCurrentUser').then(function (response) {
            var underwriter = response.data;

            orAero.productLines().then(function (response) { 
                $scope.productLine = $filter('filter')(response.data, { ProductLineId: underwriter.ProductLine })[0];
                if (!angular.isUndefined($scope.productLine))
                    $scope.buildReport();    
            });
        });
    }]);
})();