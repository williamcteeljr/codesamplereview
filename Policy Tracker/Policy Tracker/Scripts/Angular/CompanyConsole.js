(function () {
    var app = angular.module('CompanyConsole', ['dx', 'ui.bootstrap', 'app.directives']);

    app.controller('companyController', ['$scope', '$http', function ($scope, $http) {
        $scope.budgetTimeframe = 'Monthly';
        $scope.retention = {};
        $scope.loadingBudget = true;

        $scope.budgetTimeframeChange = function () {
            $scope.loadingBudget = true;
            $http.get('/policytracker/api/Console/GetBudgetStats', { params: { isYearly: ($scope.budgetTimeframe == 'Yearly') } }).then(function (response) {
                $scope.budgetData = response.data;
                $scope.loadingBudget = false;
            });
        }

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
            paging: { enabled: false, pageSize: 10 },
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

        $scope.setupCompanyConsole = function () {
            $http.get('/policytracker/api/Console/GetProductLineData', { }).then(function (response) {
                $scope.productLinePremiumData = response.data.ProductLinePremiums;
                $scope.productLinePolicyCountData = response.data.ProductLineCounts;
            });
            $http.get('/policytracker/api/Console/GetBudgetStats', {}).then(function (response) {
                $scope.budgetData = response.data;
                $scope.loadingBudget = false;
            });
            $http.get('/policytracker/api/console/GetTopTwentyPolicies', { params: { pageSize: 20 } }).then(function (response) {
                $scope.topTwenty = response.data.Results;
            });
            $http.get('/policytracker/api/console/GetTopTenQuotedProspects', { params: { pageSize: 10 } }).then(function (response) {
                $scope.topTenQuotesData = response.data.Results;
            });
            $http.get('/policytracker/api/Console/GetGrowthData', {}).then(function (response) {
                $scope.growthData = response.data;
            });
            $http.get('/policytracker/api/Console/GetRenewalRetentionData', {}).then(function (response) {
                $scope.retention = response.data;
            });
        };

        //Initialize Model
        $scope.setupCompanyConsole();
    }]);
})();