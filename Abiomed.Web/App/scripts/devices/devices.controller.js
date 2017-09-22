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

DevicesController.$inject = ['$uibModal', '$q', '$timeout', 'dataService', 'SignalRFactory', '$scope'];


function DevicesController($uibModal, $q, $timeout, dataService, SignalRFactory, $scope) {
    var ViewModel = this;
    var WOWZA;
    var promises = [dataService.getDevices()];

    function displayDevices(data) {
        // First Array is live WOWZA streams
        // Second is live RLM devices

        // Update Bearer Info to match fonts
        _.each(data, function (d) {
            d.WifiType = "";

            if (d.Bearer === "Ethernet")
                d.Bearer = "Ethernet";
            else if (d.Bearer === "Wifi24Ghz") {
                d.Bearer = "Wifi 2.4Ghz";
                d.WifiType = "2.4";
            }
            else if (d.Bearer === "Wifi5Ghz") {
                d.Bearer = "Wifi 5Ghz";
                d.WifiType = "5";
            }
            else if (d.Bearer === "LTE")
                d.Bearer = "LTE";

            // Convert Date
            var date = moment.utc(d.ConnectionTime).format('YYYY-MM-DD HH:mm:ss');            
            var utc = moment.utc(date).toDate();
            var local = moment(utc).local().format("MM/DD/YYYY | hh:mm:ss A");//fromNow();
            d.ConnectionTime = local;
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
            
            /*
            if (data[1].length > 0)
            {
                ViewModel.Streams = data[1];
            }*/
        });

        // Start Polling
        //poller();
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

    ViewModel.Settings = function (device) {        
        var modalInstance = $uibModal.open({
            //template: "<h3>Hospital: {{Hospital}}, Alarm: {{Alarm}}, Last Alarm Time: {{AlarmTime}} </h3><hr>    <div id='playerElement' style='width:100%; height:0; padding:0 0 56.25% 0'></div>",
            templateUrl: "/App/scripts/devices/devices.settings.template.html",
            size: 'lg',
            controller: function ($scope) {
                // Wrap in function to pass paramter
                //setTimeout(function () { StreamDevice(device.name) }, 250);
                $scope.RLMSerial = device.SerialNumber;
                $scope.connectionType = device.Bearer;
                $scope.start = device.ConnectionTime;

                // Keep Alive
                $scope.KeepAliveClick = function()
                {
                    dataService.sendKeepAlive(device.SerialNumber);
                }

                $scope.StartVideoClick = function ()
                {
                    dataService.sendVideoStart(device.SerialNumber);
                }

                $scope.StopVideoClick = function () {
                    dataService.sendVideoStop(device.SerialNumber);
                }

                $scope.StartImageClick = function () {
                    dataService.sendImageStart(device.SerialNumber);
                }

                $scope.StopImageClick = function () {
                    dataService.sendImageStop(device.SerialNumber);
                }
                
                // RLR Log click
                $scope.RLRLogClick = function () {
                    dataService.sendRLRLog(device.SerialNumber).then(function(returnData){
                        $scope.RLRLog = returnData;
                    });
                }
                
                $scope.RLRCloseSession = function () {                    
                    dataService.sendCloseSession(device.SerialNumber);
                }

                // Bearer selection
                $scope.radioModel = $scope.connectionType;
                $scope.checkModel = {
                    ethernet: false,
                    wifi24: false,
                    wifi5: false,
                    lte: false
                };

                $scope.checkResults = [];

                $scope.$watchCollection('checkModel', function () {
                    $scope.checkResults = [];
                    angular.forEach($scope.checkModel, function (value, key) {
                        if (value) {
                            $scope.checkResults.push(key);
                        }
                    });
                });

                $scope.UpdateBearerClick = function () {
                    dataService.sendUpdatedBearer(device.SerialNumber, $scope.radioModel);
                }

                $scope.UpdateBearerPriority = function()
                {
                    dataService.priorityBearerUpdate(device.SerialNumber, $scope.BearerSelection);
                }

                $scope.GetAuthInfoClick = function()
                {
                    dataService.getBearerInfo(device.SerialNumber);
                }

                $scope.Bearers = ["Wifi 2.4Ghz", "Wifi 5Ghz"];
                $scope.Bearer = $scope.connectionType;

                $scope.BearerSelectionList = ["Ethernet", "Wifi", "LTE"];
                $scope.BearerSelected = "Ethernet";
                $scope.BearerSelection = [];
                $scope.EnableBearer = false;
                $scope.AddBearer = function ()
                {
                    var found = _.contains($scope.BearerSelection, $scope.BearerSelected);

                    if (!found) {
                        $scope.BearerSelection.push($scope.BearerSelected);

                        if ($scope.BearerSelection.length >= 3) {
                            $scope.EnableBearer = true;
                        }
                        $scope.$digest();
                    }
                }

                $scope.UpdateBearer = function ()
                {
                    // Get List and bring over MVC

                }

                $scope.AuthTypes = ["None", "WEP", "WPA", "WPAPSK"];
                $scope.AuthType = "None";

                $scope.CreateCredential = function(credentials)
                {
                    dataService.createCredential(device, credentials);
                }

                $scope.DeleteCredential = function (credentials) {
                    dataService.deleteCredential(device, credentials);
                }

                $scope.$on('BearerSettings', function (event, data) {
                    console.log(data);
                    _.each(data.bearerInfoList, function (d) {
                        console.log(d.BearerAuthInformation.AuthType);
                        switch (d.BearerAuthInformation.AuthType)
                        {
                            case 0:
                                d.BearerAuthInformation.BearerType = "None";
                                break;
                            default:
                            case 4:
                                d.BearerAuthInformation.BearerType = "WPAPSK";
                                break;

                        }
                    });
                    $scope.AuthorizationList = data;
                    $scope.$digest();
                });
                
            },

        });
    }

    ViewModel.Enlarge = function (device) {        
        var modalInstance = $uibModal.open({
            template: "<i class='fa fa-{{connectionType}} fa-3x ModalConnectionTypeLogo' aria-hidden='true'></i><h3 class='text-center'>{{RLMSerial}} - {{start}}</h3><hr><div id='playerElement' style='width:100%; height:0; padding:0 0 56.25% 0'></div>",
            size: 'lg',
            controller: function ($scope) {                
                $scope.RLMSerial = device.SerialNumber;
                $scope.connectionType = device.Bearer;
                $scope.start = device.ConnectionTime;

                if($scope.connectionType !== "LTE")
                {
                    // Wrap in function to pass parameter
                    setTimeout(function () { StreamDevice(device.SerialNumber) }, 250);
                }
                else
                {
                    // Send off request to start video
                    dataService.sendVideoStart($scope.RLMSerial);

                    // Wait 5 seconds for now. Need to test
                    setTimeout(function () { StreamDevice(device.SerialNumber) }, 5000);
                }
            },
        });
        
        // Modal closing
        /*modalInstance.result.then(function () { },
        function () {
       
        });
        */
    }

    function StreamDevice(streamName)
    {
        var playerInstance = jwplayer("playerElement");
        playerInstance.setup({
            playlist: [{
                sources: [
                    { file: "rtmps://rlv.abiomed.com:443/live/" + streamName},
                    { file: "https://rlv.abiomed.com:443/live/" + streamName + "/playlist.m3u8" },
                ],
            }],
            width: "100%",
            aspectratio: "4:3",
            autostart: true,
            stretching: 'exactfit',
            preload: "none",
            androidhls: true,
            primary: "flash",
            rtmp: {
                bufferlength : 1
            }
        });
        /*
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
               */
    }

    ViewModel.ShowImages = function (device) {
        var modalInstance = $uibModal.open({
            templateUrl: "/App/scripts/devices/devices.image.template.html",
            size: 'lg',
            controller: function ($scope) {
                $scope.RLMSerial = device.SerialNumber;
                $scope.connectionType = device.Bearer;
                $scope.start = device.ConnectionTime;
                
                dataService.getImageNames(device.SerialNumber).then(function(data){
                    $scope.ImagesAvailable = true;
                    if (data.length == 0)
                    {
                        $scope.ImagesAvailable = false;
                    }                    
                    $scope.ImageNames = data;
                });

            },

        });    
    }

    // Register for events
    $scope.$on('AddedRemoteLink', function (event, data) {
        var device = AddUpdateDevice(data.data);
        ViewModel.Devices.push(device);
        $scope.$digest();
    });

    $scope.$on('UpdatedRemoteLink', function (event, data) {
        var device = AddUpdateDevice(data.data);
        var index = _.findIndex(ViewModel.Devices, function (serial) { return serial.SerialNumber == device.SerialNumber; });
        // Check if found
        if (index !== -1) {
            ViewModel.Devices[index] = device;
        }
        $scope.$digest();
    });

    $scope.$on('DeletedRemoteLink', function (event, data) {
        var index = _.findIndex(ViewModel.Devices, function (serial) { return serial.SerialNumber == data.data.SerialNumber; });

        // Check if found
        if (index !== -1)
        {
            ViewModel.Devices.splice(index, 1);
        }
        $scope.$digest();
    });

    function AddUpdateDevice(d)
    {
        if (d.Bearer === "Ethernet")
            d.Bearer = "Ethernet";
        else if (d.Bearer === "Wifi24Ghz") {
            d.Bearer = "Wifi 2.4Ghz";
            d.WifiType = "2.4";
        }
        else if (d.Bearer === "Wifi5Ghz") {
            d.Bearer = "Wifi 5Ghz";
            d.WifiType = "5";
        }
        else if (d.Bearer === "LTE")
            d.Bearer = "LTE";

        // Convert Date
        var date = moment.utc(d.ConnectionTime).format('YYYY-MM-DD HH:mm:ss');
        var utc = moment.utc(date).toDate();
        var local = moment(utc).local().format("MM/DD/YYYY | hh:mm:ss A");//fromNow();
        d.ConnectionTime = local;

        return d;
    }

    activate();
}