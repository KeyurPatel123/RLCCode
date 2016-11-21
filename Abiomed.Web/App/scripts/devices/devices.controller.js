angular
    .module('app')
    .controller('DevicesController', DevicesController);

function DevicesController($uibModal) {
    var ViewModel = this;

    var activate = function () {
        window.navigator.vibrate([100, 30, 100, 30, 100, 200, 200, 30, 200, 30, 200, 200, 100, 30, 100, 30, 100]);
    }

    ViewModel.Enlarge = function (device) {
        WowzaPlayer.create('playerElement',
    {
        "license": "PLAY1-8kFhe-MmMCt-pf9YE-G3P7D-xXX89",
        "title": "",
        "description": "",
        "sourceURL": "http://10.11.0.16:1935/live/myStream/playlist.m3u8",
        "autoPlay": true,
        "volume": "0",
        "mute": false,
        "loop": false,
        "audioOnly": false,
        "uiShowQuickRewind": true,
        "uiQuickRewindSeconds": "30"
    }
);

        $uibModal.open({
            template: "<h3>Hospital: {{Hospital}}, Alarm: {{Alarm}}, Last Alarm Time: {{AlarmTime}} </h3><hr><img style='width:100%' src='/Images/RLM.png'>",
            size: 'lg',
            controller: function ($scope) {
                $scope.Alarm = device.Type;
                $scope.Hospital = device.Hospital;
                $scope.AlarmTime = device.AlarmTime;
            }
        });

    }

    activate();


}