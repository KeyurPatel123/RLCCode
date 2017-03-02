/*
Remote Link - Copyright 2017 ABIOMED, Inc.
--------------------------------------------------------
Description:
loginService.js: Login Service
--------------------------------------------------------
Author: Alessandro Agnello 
*/
angular
    .module('app')
    .factory('loginService', loginService);

loginService.$inject = ['$http', '$cookies', '$rootScope'];

function loginService($http, $cookies, $rootScope) {
    return {
        login: login
    };

    function login(username, password) {
        var credentials = { Username: username, Password: password };
        return $http.post('/api/Login', credentials)
            .then(getLoginSuccess)
            .catch(getLoginFailed);

        function getLoginSuccess(response) {
            var result = response.data;

            // Update Cookie
            if (result) {
                $cookies.put('loggedIn', true);
                $rootScope.loggedIn = true;
            }
            return result;
        }

        function getLoginFailed(error) {
            return false;
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }
}