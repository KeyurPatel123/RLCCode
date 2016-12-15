angular
    .module('app')
    .controller('DevicesController', DevicesController);

DevicesController.$inject = ['$uibModal', 'videoService'];


function DevicesController($uibModal, videoService) {
    var ViewModel = this;
    var WOWZA;
    var activate = function () {
        videoService.getStreams().then(function (data) {
            ViewModel.Devices = data;

            // Fake data

            // Add as default!
            //fa-question-circle-o
            
            ViewModel.Devices = [];
            var type = "wifi";

            var tempDevices = [];
            for (var i = 12300; i < 12400; i++)
            {
                tempDevices.push({ name: "RL" + i, connectionType: type, start: new Date().toLocaleString() });
                
                if (type === "wifi")
                    type = "desktop";
                else if (type === "desktop")
                    type = "mobile";
                else if (type === "mobile")
                    type = "wifi";
            }

            ViewModel.Devices = tempDevices;            
            
        });
    }

    ViewModel.Enlarge = function (device) {        
        var modalInstance = $uibModal.open({
            //template: "<h3>Hospital: {{Hospital}}, Alarm: {{Alarm}}, Last Alarm Time: {{AlarmTime}} </h3><hr>    <div id='playerElement' style='width:100%; height:0; padding:0 0 56.25% 0'></div>",
            template: "<i class='fa fa-{{connectionType}} fa-3x ModalConnectionTypeLogo' aria-hidden='true'></i><h3 class='text-center'>{{RLMSerial}} - {{start}}</h3><hr><div id='playerElement' style='width:100%; height:0; padding:0 0 56.25% 0'></div>",
            size: 'lg',
            controller: function ($scope) {                
                $scope.RLMSerial = device.name;
                $scope.connectionType = device.connectionType;
                $scope.start = device.start;
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
                   "sourceURL": "http://10.11.0.16:1935/live/" + streamName + "/playlist.m3u8",
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