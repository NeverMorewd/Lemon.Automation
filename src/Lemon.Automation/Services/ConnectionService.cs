using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Services
{
    public class ConnectionService : IConnection
    {
        private readonly Guid _guid;
        public ConnectionService() 
        {
            _guid = new Guid();
        }
        public string ConnectionKey
        {
            get
            {
                return _guid.ToString();
            }
        }
    }
}
