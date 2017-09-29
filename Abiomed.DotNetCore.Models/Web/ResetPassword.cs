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
    public class ResetPassword
    {
        private string _id;
        private string _token;
        private string _password;

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
    }
}