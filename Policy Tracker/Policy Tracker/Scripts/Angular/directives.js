(function () {
    var app = angular.module('app.directives', []);

    app.directive('loadingIndicator', [function () {
        return {
            restrict: 'E',
            template: '<div class="loading-indicator col-sm-12 bg-light-blue disabled text-center"><i class="fa fa-refresh fa-spin fa-3x"></i></div>'
        };
    }])
})();