/*
Remote Link - Copyright 2017 ABIOMED, Inc.
--------------------------------------------------------
Description:
poller.factory.js: Poller Factory
--------------------------------------------------------
Author: Alessandro Agnello 
*/
angular
    .module('app')
    .factory('Poller', PollerFactory);

PollerFactory.$inject = ['$http', '$timeout'];

function PollerFactory ($http, $timeout) {
    var data = { response: {}, calls: 0 };
    var poller = function () {
        $http.get('/api/DeviceStatus').then(function (r) {
            data.response = r.data;
            data.calls++;
            $timeout(poller, 3000);
        });
    };
    poller();

    return {
        data: data
    };
};