using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PlaceOrder : ICommand
    {
        public Guid Id { get; set; }

        public string Product { get; set; }
    }
}
