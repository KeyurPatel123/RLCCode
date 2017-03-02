/*
Remote Link - Copyright 2017 ABIOMED, Inc.
--------------------------------------------------------
Description:
app.run.js: App Run
--------------------------------------------------------
Author: Alessandro Agnello 
*/
angular
    .module('app')
    .run(run);

run.$inject = ['$rootScope', '$cookies', '$location'];

function run($rootScope, $cookies, $location)
{
    var loggedIn = $cookies.get('loggedIn');
    if(loggedIn)
    {
        $rootScope.loggedIn = true;
        $location.path("/devices");
    }
    else
    {
        $rootScope.loggedIn = false;
    }
}