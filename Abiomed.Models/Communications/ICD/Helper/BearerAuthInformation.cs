/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * BearerAuthInformation.cs: [Software Unit Description]
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/
using System;
using static Abiomed.Models.Definitions;

namespace Abiomed.Models
{
    public class BearerAuthInformation
    {
        private Bearer _bearerType =  Bearer.Unknown;
        private UInt16 _slot = UInt16.MaxValue;
        private AuthenicationType _authType = AuthenicationType.Unknown;
        private string _SSID = string.Empty;
        private string _PSK = string.Empty;
        private bool _deleteCredential = false;
        
        public AuthenicationType AuthType
        {
            get { return _authType; }
            set { _authType = value; }
        }


        public UInt16 Slot
        {
            get { return _slot; }
            set { _slot = value; }
        }


        public Bearer BearerType
        {
            get { return _bearerType; }
            set { _bearerType = value; }
        }

        public string SSID
        {
            get { return _SSID; }
            set { _SSID = value; }
        }

        public string PSK
        {
            get { return _PSK; }
            set { _PSK = value; }
        }

        public bool DeleteCredential
        {
            get { return _deleteCredential; }
            set { _deleteCredential = value; }
        }
    }
}
