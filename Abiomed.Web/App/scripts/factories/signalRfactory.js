/*
Remote Link - Copyright 2017 ABIOMED, Inc.
--------------------------------------------------------
Description:
poller.factory.js: SignalRFactory
--------------------------------------------------------
Author: Alessandro Agnello 
*/
angular
    .module('app')
    .factory('SignalRFactory', SignalRFactory);

SignalRFactory.$inject = ['$rootScope'];

function SignalRFactory($rootScope)
{
    return {

    };
        //Set the hubs URL for the connection
        $.connection.hub.url = "http://localhost:8080/signalr";

        // Declare a proxy to reference the hub.
        var remoteLinkHub = $.connection.remoteLinkHub;

        // Create a function that the hub can call to broadcast messages.
        remoteLinkHub.client.AddedRemoteLink = function (message) {            
            $rootScope.$broadcast('AddedRemoteLink', {
                data: message 
            });
        };

        remoteLinkHub.client.UpdatedRemoteLink = function (message) {
            $rootScope.$broadcast('UpdatedRemoteLink', {
                data: message 
            });
        };

        remoteLinkHub.client.DeletedRemoteLink = function (message) {
            $rootScope.$broadcast('DeletedRemoteLink', {
                data: message
            });
        };

        remoteLinkHub.client.BearerSettings = function (deviceSerialNo, bearerInfoList) {
            $rootScope.$broadcast('BearerSettings', {
                deviceSerialNo: deviceSerialNo,
                bearerInfoList: bearerInfoList
            });
        };

        // Start the connection.
        $.connection.hub.start().done(function () {

        });

        return {

        };
};