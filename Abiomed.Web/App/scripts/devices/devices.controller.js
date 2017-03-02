/*
Remote Link - Copyright 2017 ABIOMED, Inc.
--------------------------------------------------------
Description:
devices.controller.js: Devices Controller
--------------------------------------------------------
Author: Alessandro Agnello 
*/

angular
    .module('app')
    .controller('DevicesController', DevicesController);

DevicesController.$inject = ['$uibModal', '$q', '$timeout', 'dataService'];


function DevicesController($uibModal, $q, $timeout, dataService) {
    var ViewModel = this;
    var WOWZA;
    var promises = [dataService.getDevices(), dataService.getStreams()];

    function displayDevices(data)
    {
        // First Array is live WOWZA streams
        // Second is live RLM devices
        d.WifiType = "";
        // Update Bearer Info to match fonts
        _.each(data, function (d) {
            if (d.Bearer === "Ethernet")
                d.Bearer = "desktop";
            else if (d.Bearer === "Wifi24Ghz")
            {
                d.Bearer = "wifi";
                d.WifiType = "2.4";
            }
            else if (d.Bearer === "Wifi5Ghz")
            {
                d.Bearer = "wifi";
                d.WifiType = "5";
            }
            else if (d.Bearer === "LTE")
                d.Bearer = "mobile";

            var time = moment(d.ConnectionTime);
            d.ConnectionTime = time.fromNow();
        })
        ViewModel.Devices = data;
    }

    function getData()
    {
        $q.all(promises).then(function (data) {

            if (data[0] !== null)
            {
                displayDevices(data[0]);
            }
            
            if (data[1].length > 0)
            {
                ViewModel.Streams = data[1];
            }
        });

        // Start Polling
        poller();
    }

    var activate = function () {
        getData();        
    }

    var poller = function () {
        dataService.getDevices().then(function(data)
        {
            displayDevices(data);
            $timeout(poller, 3000);
        });
    };

    ViewModel.Enlarge = function (device) {        
        var modalInstance = $uibModal.open({
            //template: "<h3>Hospital: {{Hospital}}, Alarm: {{Alarm}}, Last Alarm Time: {{AlarmTime}} </h3><hr>    <div id='playerElement' style='width:100%; height:0; padding:0 0 56.25% 0'></div>",
            template: "<i class='fa fa-{{connectionType}} fa-3x ModalConnectionTypeLogo' aria-hidden='true'></i><h3 class='text-center'>{{RLMSerial}} - {{start}}</h3><hr><div id='playerElement' style='width:100%; height:0; padding:0 0 56.25% 0'></div>",
            size: 'lg',
            controller: function ($scope) {                
                $scope.RLMSerial = device.SerialNumber;
                $scope.connectionType = device.Bearer;
                $scope.start = device.ConnectionTime;
                // Wrap in function to pass paramter
                setTimeout(function () { StreamDevice(device.SerialNumber) }, 250);

                $timeout(poller, 3000);

                function UpdateTime()
                {
                    //var time = moment(d.ConnectionTime);
                    //d.ConnectionTime = time.fromNow();
                }                
            },
            
        });
        
        // Modal closing, clean up Wowza
        modalInstance.result.then(function () { },
        function () {
            WOWZA.destroy();
        });

    }

    // todo delete when linked
    ViewModel.EnlargeStream = function (device) {
        var modalInstance = $uibModal.open({
            //template: "<h3>Hospital: {{Hospital}}, Alarm: {{Alarm}}, Last Alarm Time: {{AlarmTime}} </h3><hr>    <div id='playerElement' style='width:100%; height:0; padding:0 0 56.25% 0'></div>",
            template: "<div id='playerElement' style='width:100%; height:0; padding:0 0 56.25% 0'></div>",
            size: 'lg',
            controller: function ($scope) {
                // Wrap in function to pass paramter
                setTimeout(function () { StreamDevice(device.name) }, 250);
            },

        });

        // Modal closing, clean up Wowza
        modalInstance.result.then(function () { },
        function () {
            WOWZA.destroy();
        });

    }
    function StreamDevice(streamName)
    {
        WOWZA = WowzaPlayer.create('playerElement',
               {
                   "license": "PLAY1-8kFhe-MmMCt-pf9YE-G3P7D-xXX89",
                   "title": "",
                   "description": "",
                   "sourceURL": "http://13.92.255.38:443/live/" + streamName + "/playlist.m3u8",
                   "autoPlay": true,
                   "volume": "0",
                   "mute": false,
                   "loop": false,
                   "audioOnly": false,
                   "uiShowQuickRewind": true,
                   "uiQuickRewindSeconds": "30"
               });
    }

    activate();
}