using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignalR.API.Models;

namespace SignalR.API.Hubs
{
    public interface IProductHub
    {
        Task RecieveProduct(Product p);
        
    }
}
