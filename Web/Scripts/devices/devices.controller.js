angular
    .module('app')
    .controller('DevicesController', DevicesController);

function DevicesController($uibModal) {
    var ViewModel = this;    

    var activate = function() {                
        var devices = [];
        // Create 500
        for(var i = 0; i < 500; i++)
        {                        
            devices.push(CreateRandomAlarm(i));            
        }

        // Set Master list
        ViewModel.MasterDevices = devices;
        ViewModel.devices = devices;

        // Default set of Radio buttons
        ViewModel.filter =
        {
            type: "all"
        }

        ViewModel.sort =
        {
            type: "none"
        }

        // Abiomed location
        initmap();        

    }    

function initmap() {
	// set up the map
	map = new L.Map('map');

	// create the tile layer with correct attribution
	var osmUrl='http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
	var osmAttrib='Map data © <a href="http://openstreetmap.org">OpenStreetMap</a> contributors';
	var osm = new L.TileLayer(osmUrl, {minZoom: 4, maxZoom: 15, attribution: osmAttrib});		

    // start the map at Abiomed
	map.setView(new L.LatLng(42.5769047,-70.9197126),25);
	map.addLayer(osm);

    new L.marker([42.5769047,-70.9197126])
				//.bindPopup("Sample Text")                
                .on('click', onClick)
				.addTo(map);

                map.setView([42.5769047,-70.9197126], 25, {
    reset: true
});
}

function onClick(e)
{
    ViewModel.Enlarge(ViewModel.devices[0]);
}

    function CreateRandomAlarm(index)
    {
        var obj = {};

        // if greater then 300 then make it no alarm
        if(index > 300)
        {
            obj.class = 'None';
            obj.Type = 'None';
        }
        else
        {
            if(index % 5 == 0)
            {
                obj.class = 'Red';
                obj.Type = 'Serious';
            }
            else if(index % 3 == 0)
            {
                obj.class = 'Yellow';
                obj.Type = 'Warning';
            }
            else
            {
                obj.class = 'Grey';
                obj.Type = 'Advisory';
            }
        }

        // Supplemental info
        obj.Hospital = "XYZ";
        obj.AlarmTime = "20Oct16 - 10:45 AM";
        
        return obj;
    }

    ViewModel.Filter = function(type)
    {
        switch(type)
        {
            case 'all':
             ViewModel.devices = ViewModel.MasterDevices;
            break;

            case 'red':            
            ViewModel.devices = _.where(ViewModel.MasterDevices, {class: "Red", Type: "Serious"} );
            break;

            case 'yellow':
            ViewModel.devices = _.where(ViewModel.MasterDevices, {class: "Yellow", Type: "Warning"} );
            break;
            
            case 'grey':
            ViewModel.devices = _.where(ViewModel.MasterDevices, {class: "Grey", Type: "Advisory"} );
            break;

            case 'not':
            ViewModel.devices = _.where(ViewModel.MasterDevices, {class: "None", Type: "None"} );
            break;
        }
        
    } 

    ViewModel.CheckClass = function(type)
    {
        var color = 'WhiteText';
        if(type === 'Yellow')
            color = 'BlueText';

        return color;
    }

    ViewModel.Enlarge = function(device)
    {
        WowzaPlayer.create('playerElement',
    {
    "license":"PLAY1-8kFhe-MmMCt-pf9YE-G3P7D-xXX89",
    "title":"",
    "description":"",
    "sourceURL":"http://10.11.0.16:1935/live/myStream/playlist.m3u8", 
    "autoPlay":true,
    "volume":"0",
    "mute":false,
    "loop":false,
    "audioOnly":false,
    "uiShowQuickRewind":true,
    "uiQuickRewindSeconds":"30"
    }
);

        $uibModal.open({      
            template: "<h3>Hospital: {{Hospital}}, Alarm: {{Alarm}}, Last Alarm Time: {{AlarmTime}} </h3><hr><img style='width:100%' src='/Images/RLM.png'>",
            size: 'lg',
            controller: function($scope) 
            {
                $scope.Alarm = device.Type;
                $scope.Hospital = device.Hospital;  
                $scope.AlarmTime = device.AlarmTime;
            }
    });

    }
    
    activate();

    
}