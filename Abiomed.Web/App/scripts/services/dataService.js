/*
Remote Link - Copyright 2017 ABIOMED, Inc.
--------------------------------------------------------
Description:
dataService.js: Data Service
--------------------------------------------------------
Author: Alessandro Agnello 
*/
angular
    .module('app')
    .factory('dataService', dataService);

dataService.$inject = ['$http'];

function dataService($http) {
    return {
        getStreams: getStreams,
        getDevices: getDevices,
        sendKeepAlive: sendKeepAlive,
        sendRLRLog: sendRLRLog,
        getImageNames: getImageNames,
        sendVideoStart: sendVideoStart,
        sendVideoStop: sendVideoStop,
        sendImageStart: sendImageStart,
        sendImageStop: sendImageStop,
        getBearerInfo: getBearerInfo,
        sendCloseSession: sendCloseSession,
        sendUpdatedBearer: sendUpdatedBearer,
        createCredential: createCredential,
        deleteCredential: deleteCredential
    };

    function getStreams() {
        return $http.get('/api/Video')
            .then(getVideoComplete)
            .catch(getVideoFailed);

        function getVideoComplete(response) {
            return response.data.incomingStreams;
        }

        function getVideoFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function getDevices() {
        return $http.get('/api/DeviceStatus', {cache:false})
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function sendKeepAlive(serialNumber)
    {
        return $http.post('/api/DeviceStatus/SendKeepAlive/' + serialNumber)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function sendRLRLog(serialNumber) {

        return $http.post('/api/DeviceStatus/RLRLog/' + serialNumber)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function getImageNames(serialNumber)
    {
        return $http.get('/api/Image/GetImageNames/' + serialNumber)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function sendVideoStart(serialNumber)
    {
        return $http.post('/api/DeviceStatus/SendVideoStart/' + serialNumber)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function sendVideoStop(serialNumber) {
        return $http.post('/api/DeviceStatus/SendVideoStop/' + serialNumber)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function sendImageStart(serialNumber) {
        return $http.post('/api/DeviceStatus/SendImageStart/' + serialNumber)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function sendImageStop(serialNumber) {
        return $http.post('/api/DeviceStatus/SendImageStop/' + serialNumber)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function getBearerInfo(serialNumber) {
        return $http.post('/api/DeviceStatus/GetBearerInfo/' + serialNumber)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function sendCloseSession(serialNumber) {
        return $http.post('/api/DeviceStatus/CloseSessionIndication/' + serialNumber)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function sendUpdatedBearer(serialNumber, bearer) {
        return $http.post('/api/DeviceStatus/BearerChangeIndication/' + serialNumber + '/' + bearer)
           .then(getDevicesComplete)
           .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }
   
    function createCredential(device, authorization) {
        var data =
        {
            'SerialNumber': device.SerialNumber,
            'AuthorizationInfo': Authorization
        };

        return $http.post('/api/DeviceStatus/CreateCredential/', data)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }

    function deleteCredential(device, authorization) {
        var data =
        {
            'SerialNumber': device.SerialNumber,
            'AuthorizationInfo': Authorization
        };

        return $http.post('/api/DeviceStatus/DeleteCredential/', data)
            .then(getDevicesComplete)
            .catch(getDevicesFailed);

        function getDevicesComplete(response) {
            return response.data;
        }

        function getDevicesFailed(error) {
            //logger.error('XHR Failed for getAvengers.' + error.data);
        }
    }
}