(function () {
    var app = angular.module('appFactories', []);

    app.factory('orAero', ['$http', '$q', function ($http, $q) {
        var factory = {};
        var _productLines = {};
        var _purposesOfUse = {};

        factory.statuses = [
            { status: 'Submission', displayText: 'Submission' },
            { status: 'Declined', displayText: 'Declined' },
            { status: 'Quote', displayText: 'Quote' },
            { status: 'Bound', displayText: 'Bound' },
            { status: 'Canceled', displayText: 'Canceled' },
            { status: 'Issued', displayText: 'Issued' },
            { status: 'Lost', displayText: 'Lost' },
            { status: 'Already Involved', displayText: 'Already Involved' },
        ];

        factory.branches = [
            { branch: 'ATL', displayText: 'Atlanta' },
            { branch: 'CHI', displayText: 'Chicago' },
            { branch: 'DAL', displayText: 'Dallas' },
            { branch: 'NYC', displayText: 'New York' },
            { branch: 'SEA', displayText: 'Seattle' }
        ];

        factory.purposesOfUse = function () {
            var deferred = $q.defer();

            if (_purposesOfUse.data) {
                deferred.resolve(_purposesOfUse);
                deferred.reject(null)
            } else {
                $http.get('/policytracker/api/risk/getPurposesOfUse')
                    .then(function (res) {
                        _purposesOfUse = res;
                        deferred.resolve(res)
                    }, function (error) {
                        deferred.reject(error)
                    });
            }
            
            return deferred.promise;
        };

        factory.productLines = function () {
            var deferred = $q.defer();

            if (_productLines.data) {
                deferred.resolve(_productLines);
                deferred.reject(null);
            } else {
                $http.get('/policytracker/api/risk/GetProductLines')
                    .then(function (res) {
                        _productLines = res;
                        deferred.resolve(res)
                    }, function (error) {
                        deferred.reject(error)
                    });
            }
            
            return deferred.promise;
        };

        factory.getLastGeneratedRenewalDate = function () {
            var date = new Date();
            date.setMonth(date.getMonth() + 4)
            return new Date(date.getFullYear(), date.getMonth(), 0)
        }

        return factory;
    }]);

    app.factory('riskService', ['$http', function ($http) {
        var service = {};

        service.getNotes = function (id) {
            return $http.get('/policytracker/api/risk/getnotes', { params: { id: id } })
        };

        service.getPeriodImpactNotes = function (startDate, endDate, productLineId) {
            return $http.get('/policytracker/api/console/GetPremiumImpactNotes', { params: { startDate: startDate, endDate: endDate, productLineId: productLineId } })
        }

        return service;
    }]);

    app.factory('dateTimeFactory', function () {
        var service = {};
        var _years = [];

        var _months = [
            { month: 1, name: 'January', shortName: 'Jan' },
            { month: 2, name: 'February', shortName: 'Feb' },
            { month: 3, name: 'March', shortName: 'Mar' },
            { month: 4, name: 'April', shortName: 'Apr' },
            { month: 5, name: 'May', shortName: 'May' },
            { month: 6, name: 'June', shortName: 'Jun' },
            { month: 7, name: 'July', shortName: 'Jul' },
            { month: 8, name: 'August', shortName: 'Aug' },
            { month: 9, name: 'September', shortName: 'Sep' },
            { month: 10, name: 'October', shortName: 'Oct' },
            { month: 11, name: 'November', shortName: 'Nov' },
            { month: 12, name: 'December', shortName: 'Dec' }
        ]

        for (var i = 2013; i <= new Date().getFullYear() ; i++) {
            _years.push(i);
        }

        service.getMonths = function () { return _months; };
        service.getYears = function () { return _years; };
        // Add 1 because getMonth is 0 based (ie Jan = 0);
        service.getCurrentMonth = function () { return new Date().getMonth() + 1 };
        service.getCurrentYear = function () { return new Date().getFullYear(); };

        return service;
    });

    app.factory('aircraftFactory', function () {
        var service = {
            engineTypes: [
                { engineType: 'MEJ', name: 'Multi Engine Jet' },
                { engineType: 'MEP', name: 'Multi Engine Piston' },
                { engineType: 'MET', name: 'Multi Engine Turbine' },
                { engineType: 'RWP', name: 'Rotorwing Piston' },
                { engineType: 'RWT', name: 'Rotorwing Turbine' },
                { engineType: 'SEP', name: 'Single Engine Piston' },
                { engineType: 'SET', name: 'Single Engine Turbine' },
                { engineType: 'UAV', name: 'UAV' }
            ]
        };

        return service;
    })
})();