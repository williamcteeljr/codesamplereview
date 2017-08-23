(function () {
    var app = angular.module('UnderwriterConsole', ['dx']);

    app.controller('consoleController', ['$scope', '$document', '$http', function ($scope, $document, $http) {
        $http.get('/policytracker/api/User/GetUnderwriters').then(function (response) { $scope.Underwriters = response.data; });
        $http.get('/policytracker/api/Risk/GetProductLines').then(function (response) { $scope.ProductLines = response.data; });
        $scope.year = new Date().getFullYear();
        $scope.month = (new Date().getMonth() + 1).toString();
        $scope.Years = [];
        $scope.Months = [{ Month: 1, Name: 'January' }, { Month: 2, Name: 'February' }, { Month: 3, Name: 'March' }, { Month: 4, Name: 'April' }, { Month: 5, Name: 'May' }, { Month: 6, Name: 'June' }, { Month: 7, Name: 'July' }, { Month: 8, Name: 'August' }, { Month: 9, Name: 'September' }, { Month: 10, Name: 'October' }, { Month: 11, Name: 'November' }, { Month: 12, Name: 'December' }, ];

        $scope.productLine = '';
        $scope.underwriter = {};
        $scope.riskRatios = { QuoteRatio: 10 };
        
        $scope.$watch("underwriter", function (newValue, oldValue) {
            if ($scope.underwriter.UserId != 0 && $scope.underwriter.UserId != null) {
                $scope.setDataForUnderwriter($scope.underwriter.UserId);
            }
        });
        
        for (var i = new Date().getFullYear() ; i >= 2013; i--) {
            $scope.Years.push(i);
        }

        //#region Ratios
        $scope.quoteRatioSettings = {
            //title: { text: "Quote Ratio", font: { size: 20 } },
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
                ]},
            size: { height:  200 },
            tooltip: { enabled: true }
        };
        $scope.hitRatioSettings = {
            //title: { text: "Hit Ratio", font: { size: 25 } },
            palette: 'Pastel',
            animation: { enabled: false },
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
                ]},
            size: { height: 200 },
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
                ]},
            size: { height: 200 },
            tooltip: { enabled: true }
        }
        //#endregion

        //#region Top Twenty Accounts
        $scope.topTwenty = [];

        $scope.topTwentyAccounts = {
            dataSource: $scope.topTwenty,
            bindingOptions: { dataSource: 'topTwenty' },
            //dataSource: {
            //    load: function (loadOptions) {
            //        var d = $.Deferred();
            //        $.getJSON('/policytracker/api/Dashboard/GetTopTwentyPolicies', {
            //            UnderwriterId: $scope.underwriter.UserId ? $scope.underwriter.UserId : 0,
            //            pageNumber: (loadOptions.skip / loadOptions.take) + 1,
            //            pageSize: loadOptions.take
            //        }).done(function (data) { d.resolve(data.Results, { totalCount: data.TotalResults }); });
            //        return d.promise();
            //    }
            //},
            columns: [
                { dataField: "Name", caption: 'Named Insured' },
                { dataField: "AgencyName", caption: 'Broker' },
                { dataField: 'ExpirationDate', dataType: 'date', format: 'shortDate' },
                { dataField: 'PolicyNumber' },
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

        //#region Statuses By Month Chart
        $scope.statusCountByMonthData = [];
        $scope.statusCountByMonthChart = {
            size: { height: 280 },
            animation: { enabled: false },
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
            size: { height: 280 },
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
                    if (point.seriesName > 1000 ) {
                        return {
                            text: '$'+point.value
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
            size: { height: 280 },
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
                    if (point.seriesName > 1000 ) {
                        return {
                            text: '$'+point.value
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

        //#region Top Brokers By Count and Premium
        $scope.TopTenBrokerCount = [];
        $scope.TopTenBrokerPremium = [];
        //#endregion

        $scope.setDataForUnderwriter = function (underwriter) {
            $http.get('/policytracker/api/Console/GetUnderwriterStats', { params: { underwriterId: underwriter } }).then(function (response) {
                $scope.businessTypeChartData = response.data.BusinessTypeCount;
                $scope.statusCountByMonthData = response.data.StatusCountByMonth;

                $scope.riskStatusCounts = response.data.RiskStatusCounts;

                $scope.quoteRatio = response.data.QuoteRatio;
                $scope.hitRatio = response.data.HitRatio;
                $scope.writtenRatio = response.data.WrittenRatio;
                $("#quoteRatioGauge").dxCircularGauge("instance").value(response.data.QuoteRatio);
                $("#hitRatioGauge").dxCircularGauge("instance").value(response.data.HitRatio);
                $("#writtenRatioGauge").dxCircularGauge("instance").value(response.data.WrittenRatio);
            });

            $http.get('/policytracker/api/console/GetTopTwentyPolicies', { params: { UnderwriterId: underwriter, pageSize: 20 } }).then(function (response) {
                $scope.topTwenty = response.data.Results;
            });

            $http.get('/policytracker/api/Console/GetTopBrokers', { params: { UnderwriterId: underwriter } }).then(function (response) {
                $scope.TopTenBrokerCount = response.data.TopTenCount;
                $scope.TopTenBrokerPremium = response.data.TopTenPremium;
            });
        };

        //Get the Current User When the Page is Loading & Init Values
        $http.get('/policytracker/api/User/GetCurrentUser').then(function (response) {
            $scope.underwriter = response.data;
            $scope.setDataForUnderwriter($scope.underwriter.UserId);
        });
    }]);
})();