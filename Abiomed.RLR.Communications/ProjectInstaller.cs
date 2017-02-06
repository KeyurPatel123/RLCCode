using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace Abiomed.RLR.Communications
{
    [RunInstaller(true)]
    public partial class f : System.Configuration.Install.Installer
    {
        public f()
        {
            InitializeComponent();
        }
    }
}
