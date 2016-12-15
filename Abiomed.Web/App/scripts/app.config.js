angular
    .module('app', ['ui.router', 'ngAnimate', 'ngCookies', 'ui.bootstrap'])
    .config(configure);

configure.$inject = ['$stateProvider', '$urlRouterProvider', '$locationProvider'];

function configure($stateProvider, $urlRouterProvider, $locationProvider) {
    
    $urlRouterProvider.otherwise('/');

    $stateProvider
       .state('devices', {
            url: "/devices",
            templateUrl: "/App/scripts/devices/devices.template.html",
            controller: 'DevicesController',
            controllerAs: 'devices'
       })

    $stateProvider
       .state('login', {
           url: "/",
           templateUrl: "/App/scripts/login/login.template.html",
           controller: 'LoginController',
           controllerAs: 'login'
       })

    // use the HTML5 History API
    $locationProvider.html5Mode(true);
}