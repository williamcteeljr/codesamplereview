(function () {
    var app = angular.module('BranchConsole', ['dx']);

    app.controller('branchController', ['$scope', '$http', function ($scope, $http) {
        $scope.branch = {};
        $http.get('/policytracker/api/risk/GetBranches').then(function (response) { $scope.branches = response.data; });

        //#region Ratios
        $scope.quoteRatioSettings = {
            //title: { text: "Quote Ratio", font: { size: 25 } },
            palette: 'Pastel',
            animation: { enabled: false },
            scale: {
                startValue: 0,
                endValue: 100,
                majorTick: { tickInterval: 5 }
            },
            rangeContainer: {
                ranges: [
                    { startValue: 30, endValue: 100 },
                    { startValue: 15, endValue: 30 },
                    { startValue: 0, endValue: 15 },
                ]
            },
            size: { height: 300 },
            tooltip: { enabled: true }
        };
        $scope.hitRatioSettings = {
            //title: { text: "Hit Ratio", font: { size: 25 } },
            palette: 'Pastel',
            animation: { enabled: true },
            scale: {
                startValue: 0,
                endValue: 50,
                majorTick: { tickInterval: 1 }
            },
            rangeContainer: {
                ranges: [
                    { startValue: 20, endValue: 50 },
                    { startValue: 10, endValue: 20 },
                    { startValue: 0, endValue: 10, },
                ]
            },
            size: { height: 300 },
            tooltip: { enabled: true },
        };
        $scope.writtenRatioSettings = {
            //title: { text: "Written Ratio", font: { size: 25 } },
            palette: 'Pastel',
            animation: { enabled: false },
            scale: {
                startValue: 0,
                endValue: 20,
                majorTick: { tickInterval: 1 }
            },
            rangeContainer: {
                ranges: [
                    { startValue: 5, endValue: 20 },
                    { startValue: 3, endValue: 5 },
                    { startValue: 0, endValue: 3 },
                ]
            },
            size: { height: 300 },
            tooltip: { enabled: true }
        }
        //#endregion

        //#region Statuses By Month Chart
        $scope.statusCountByMonthData = [];
        $scope.statusCountByMonthChart = {
            animation: { duration: 300 },
            //title: { text: "Risk Status Count Summary By Month", font: { size: 25 } },
            dataSource: $scope.statusCountByMonthData,
            bindingOptions: {
                dataSource: 'statusCountByMonthData'
            },
            commonSeriesSettings: {
                argumentField: 'Month',
                type: 'stackedBar',
            },
            valueAxis: {
                label: { format: 'largeNumber' },
                title: {
                    text: 'Total Count',
                    font: { weight: 100 }
                }
            },
            argumentAxis: { label: { format: 'largeNumber' } },
            legend: {
                horizontalAlignment: 'center',
                verticalAlignment: 'bottom',
            },
            tooltip: { enabled: true },
            series: [{
                valueField: 'Submissions',
                name: 'Submissions'
            }, {
                valueField: 'Quotes',
                name: 'Quotes'
            }, {
                valueField: 'Declines',
                name: 'Declined'
            }, {
                valueField: 'Loses',
                name: 'Lost'
            }, {
                valueField: 'Cancelled',
                name: 'Cancelled'
            }, {
                valueField: 'Issued',
                name: 'Issued/Bound'
            }]
        };
        //#endregion

        //#region New Business Vs Renewal Charts
        $scope.businessTypeChartData = [];
        $scope.businessTypeCountChart = {
            animation: { duration: 300 },
            dataSource: $scope.businessTypeChartData,
            bindingOptions: {
                dataSource: 'businessTypeChartData'
            },
            commonSeriesSettings: {
                argumentField: 'Month',
                type: 'line',
            },
            argumentAxis: { label: { format: 'largeNumber' } },
            legend: {
                horizontalAlignment: 'center',
                verticalAlignment: 'bottom',
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
            series: [{
                axis: 'CountAxis',
                valueField: 'NewBusiness',
                name: 'Policy Count',
                //label: { alignment: 'left', visible: true }
            }, {
                axis: 'PremiumAxis',
                valueField: 'NewBusinessValue',
                name: 'Premium $ Value',
                //label: { format: 'currency', alignment: 'right', visible: true }
            }],
            valueAxis: [
                {
                    name: 'CountAxis',
                    label: { format: 'largeNumber' },
                    title: {
                        text: 'Total Count',
                        font: { weight: 100 }
                    }
                },
                {
                    name: 'PremiumAxis',
                    position: 'right',
                    label: { format: 'currency' },
                    title: {
                        text: '$ Premium',
                        font: { weight: 100 }
                    }
                }
            ]
        };

        $scope.businessTypeValueChart = {
            animation: { duration: 300 },
            dataSource: $scope.businessTypeChartData,
            bindingOptions: {
                dataSource: 'businessTypeChartData'
            },
            commonSeriesSettings: {
                argumentField: 'Month',
                type: 'line',
            },
            argumentAxis: { label: { format: 'largeNumber' } },
            legend: {
                horizontalAlignment: 'center',
                verticalAlignment: 'bottom'
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
            series: [{
                axis: 'CountAxis',
                valueField: 'Renewals',
                name: 'Policy Count',
                //label: { alignment: 'left', visible: true }
            }, {
                axis: 'PremiumAxis',
                valueField: 'RenewalsValue',
                name: 'Premium $ Value',
                //label: { format: 'currency', alignment: 'right', visible: true }
            }],
            valueAxis: [
                {
                    name: 'CountAxis',
                    label: { format: 'largeNumber' },
                    title: {
                        text: 'Total Count',
                        font: { weight: 100 }
                    }
                },
                {
                    name: 'PremiumAxis',
                    position: 'right',
                    label: { format: 'currency' },
                    title: {
                        text: '$ Premium',
                        font: { weight: 100 }
                    }
                }
            ]
        };
        //#endregion

        //#region Top Twenty Accounts
        $scope.topTwenty = [];

        $scope.topTwentyAccountsSettings = {
            dataSource: $scope.topTwenty,
            bindingOptions: { dataSource: 'topTwenty' },
            columns: [
                { dataField: "Name", caption: 'Named Insured' },
                { dataField: "AgencyName", caption: 'Broker' },
                { dataField: 'ExpirationDate', dataType: 'date', format: 'shortDate', width: 100 },
                { dataField: 'PolicyNumber', width: 120 },
                { dataField: "AnnualizedPremium", caption: 'Annual Prem', format: 'currency', width: 100 }],
            paging: { enabled: true, pageSize: 20 },
            pager: {
                showPageSizeSelector: false,
                allowedPageSizes: [10, 20],
                showInfo: true,
                visible: false
            },
            selection: { mode: 'single' },
            onSelectionChanged: function (e) {
                var row = e.selectedRowKeys[0];
                if (row) {
                    $scope.topInforceRiskId = row.RiskId;
                } else {
                    $scope.topInforceRiskId = 0;
                }
            }
        };

        $scope.topInforceView = function (e) {
            if ($scope.topInforceRiskId) {
                ora.Risk.RiskEdit($scope.topInforceRiskId);
            }
        };
        //#endregion

        //#region Top 10 Quoted Prospects
        $scope.topTenQuotesData = [];
        $scope.topTenQuotesSettings = {
            dataSource: $scope.topTenQuotesData,
            bindingOptions: { dataSource: 'topTenQuotesData' },
            columns: [
                { dataField: "Name", caption: 'Named Insured' },
                { dataField: "AgencyName", caption: 'Broker' },
                { dataField: 'ExpirationDate', dataType: 'date', format: 'shortDate', width: 100 },
                { dataField: 'PolicyNumber', width: 120 },
                { dataField: "AnnualizedPremium", caption: 'Annual Prem', format: 'currency', width: 100 }],
            paging: { enabled: false , pageSize: 10 },
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

        $scope.topQuoteView = function (e) {
            if ($scope.topQuoteRiskId) {
                ora.Risk.RiskEdit($scope.topQuoteRiskId);
            }
        };
        //#endregion

        //#region Top Brokers By Count and Premium
        $scope.TopTenBrokerCount = [];
        $scope.TopTenBrokerCountSettings = {
            dataSource: $scope.TopTenBrokerCount,
            bindingOptions: { dataSource: 'TopTenBrokerCount' },
            columns: [
                { dataField: "AgencyName", caption: 'Broker' },
                { dataField: "TotalRisks", width: 100 }],
            paging: { pageSize: 10 },
            selection: { mode: 'single' },
        };

        $scope.TopTenBrokerPremium = [];
        $scope.TopTenBrokerPremiumSettings = {
            dataSource: $scope.TopTenBrokerPremium,
            bindingOptions: { dataSource: 'TopTenBrokerPremium' },
            columns: [
                { dataField: "AgencyName", caption: 'Broker' },
                { dataField: "TotalPremium", caption: 'Premium', format: 'currency', width: 100 }],
            paging: { pageSize: 10 },
            selection: { mode: 'single' },
        };
        //#endregion

        //#region Branch Product Line Premium Value (Pie Chart)
        $scope.productLinePremiumData = [];
        $scope.productLinePremiumValues = {
            type: 'donut',
            dataSource: $scope.productLinePremiumData,
            bindingOptions: { dataSource: 'productLinePremiumData' },
            resolveLabelOverlapping: "shift",
            series: {
                argumentField: "ProductLine",
                valueField: "TotalPremium",
                label: {
                    visible: true,
                    format: "currency",
                    connector: {
                        visible: true
                    },
                }
            },
            tooltip: {
                enabled: true, format: 'currency', font: {}
            }
        };

        $scope.productLinePolicyCountData = [];
        $scope.productLineCountValues = {
            type: 'doughnut',
            dataSource: $scope.productLinePolicyCountData,
            bindingOptions: { dataSource: 'productLinePolicyCountData' },
            resolveLabelOverlapping: "shift",
            series: {
                argumentField: "ProductLine",
                valueField: "TotalRisks",
                label: {
                    visible: true,
                    format: "largeNumber",
                    connector: {
                        visible: true
                    },
                }
            },
            tooltip: { enabled: true }
        };
        //#endregion

        //#region Workload by Underwriter Count and by $ Value
        $scope.underwriterWorkload = [];
        $scope.underwriterWorkloadSettings = {
            title: { text: "", font: { size: 25 } },
            dataSource: $scope.underwriterWorkload,
            bindingOptions: {
                dataSource: 'underwriterWorkload'
            },
            commonSeriesSettings: {
                argumentField: 'UW',
                type: 'bar',
            },
            valueAxis: {
                title: {
                    text: 'Total Count',
                    font: { weight: 100 }
                }
            },
            legend: { horizontalAlignment: 'center', verticalAlignment: 'bottom', },
            tooltip: { enabled: true },
            argumentAxis: { label: { format: 'largeNumber' } },
            series: [{
                valueField: 'TotalInforce',
                name: 'Total'
            }],
            rotated: true
        };
        //#endregion

        $scope.$watch("branch", function (newValue, oldValue) {
            if ($scope.branch.Value != '' && $scope.branch.Value != null) {
                $scope.setDataForBranch($scope.branch.Value);
            }
        });

        $scope.setDataForBranch = function (branch) {
            $http.get('/policytracker/api/Console/GetRiskStatusCounts', { params: { branch: branch } }).then(function (response) {
                $scope.riskStatusCounts = response.data;
            });

            $http.get('/policytracker/api/Console/GetRatios', { params: { branch: branch } }).then(function (response) {
                $scope.quoteRatio = response.data.QuoteRatio;
                $scope.hitRatio = response.data.HitRatio;
                $scope.writtenRatio = response.data.WrittenRatio;
                $("#quoteRatioGauge").dxCircularGauge("instance").value(response.data.QuoteRatio);
                $("#hitRatioGauge").dxCircularGauge("instance").value(response.data.HitRatio);
                $("#writtenRatioGauge").dxCircularGauge("instance").value(response.data.WrittenRatio);
            });

            $http.get('/policytracker/api/Console/GetCounts', { params: { branch: branch } }).then(function (response) {
                $scope.businessTypeChartData = response.data.BusinessTypeCount;
                $scope.statusCountByMonthData = response.data.StatusCountByMonth;
            });

            $http.get('/policytracker/api/console/GetTopTwentyPolicies', { params: { Branch: branch, pageSize: 20 } }).then(function (response) {
                $scope.topTwenty = response.data.Results;
            });

            $http.get('/policytracker/api/console/GetTopTenQuotedProspects', { params: { Branch: branch, pageSize: 10 } }).then(function (response) {
                $scope.topTenQuotesData = response.data.Results;
            });

            $http.get('/policytracker/api/Console/GetTopBrokers', { params: { branch: branch } }).then(function (response) {
                $scope.TopTenBrokerCount = response.data.TopTenCount;
                $scope.TopTenBrokerPremium = response.data.TopTenPremium;
            });

            $http.get('/policytracker/api/Console/GetProductLineData', { params: { branch: branch } }).then(function (response) {
                $scope.productLinePremiumData = response.data.ProductLinePremiums;
                $scope.productLinePolicyCountData = response.data.ProductLineCounts;
            });

            $http.get('/policytracker/api/Console/GetBranchUnderwriterWorkload', { params: { branch: branch } }).then(function (response) {
                $scope.underwriterWorkload = response.data;
            });
        };

        //Get the Current User When the Page is Loading & Init Values
        $http.get('/policytracker/api/User/GetCurrentUser').then(function (response) {
            $scope.user = response.data;
            $scope.branch = $scope.user.Branch;
            $scope.setDataForBranch($scope.user.Branch.Value);
        });
    }]);
})();