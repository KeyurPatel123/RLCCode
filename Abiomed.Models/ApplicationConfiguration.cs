
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abiomed.Models
{
    public class ApplicationConfiguration : TableEntity
    {
 
        /// <summary>
        /// Gets or Sets the Value Property 
        /// </summary>
        public string Value { get; set; }
    }
}