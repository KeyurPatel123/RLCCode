angular
    .module('app')
    .factory('dataService', dataService);

dataService.$inject = ['$http'];

function dataService($http) {
    return {
        getStreams: getStreams,
        getDevices: getDevices
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
}