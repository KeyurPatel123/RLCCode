/*
 * Remote Link - Copyright 2017 ABIOMED, Inc.
 * --------------------------------------------------------
 * Description:
 * Credentials.cs: Login Credentials Model
 * --------------------------------------------------------
 * Author: Alessandro Agnello 
*/

namespace Abiomed.DotNetCore.Models
{
    public class Credentials
    {
        private string _username;
        private string _password;

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
    }
}