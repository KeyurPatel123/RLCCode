/*
Remote Link - Copyright 2017 ABIOMED, Inc.
--------------------------------------------------------
Description:
login.controller.js: Login Controller
--------------------------------------------------------
Author: Alessandro Agnello 
*/
angular
    .module('app')
    .controller('LoginController', LoginController);

LoginController.$inject = ['$state', 'loginService'];


function LoginController($state, loginService) {
    var ViewModel = this;
  
    var activate = function ()
    {
        // Set the default value of inputType
        ViewModel.inputType = 'password';
    }    

    // Hide & show password function
    ViewModel.hideShowPassword = function () {
        if (ViewModel.inputType == 'password')
            ViewModel.inputType = 'text';
        else
            ViewModel.inputType = 'password';
    };

    ViewModel.submit = function()
    {
        loginService.login(ViewModel.username, ViewModel.password).then(function (result) {
            // If successful
            if (result)
            {
                // Redirect, login service updated cookie and scope
                $state.go("devices");
            }
            else
            {
                ViewModel.error = true;
            }
        });
    }

    activate();
}