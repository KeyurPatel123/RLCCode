angular
    .module('app', ['ui.router', 'ngAnimate', 'ui.bootstrap'])
    .config(configure);

configure.$inject = ['$stateProvider', '$urlRouterProvider'];

function configure ($stateProvider, $urlRouterProvider) {
    
    $urlRouterProvider.otherwise('/devices');

    $stateProvider
       .state('devices', {
            url: "/devices",
            templateUrl: "/App/scripts/devices/devices.template.html",
            controller: 'DevicesController',
            controllerAs: 'devices'
        })
}