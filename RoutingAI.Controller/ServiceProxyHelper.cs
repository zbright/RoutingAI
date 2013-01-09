﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.ServiceContracts;
using System.ServiceModel;
using System.Net;

namespace RoutingAI.Controller
{
    /// <summary>
    /// Static class with utility methods for getting WCF service proxies
    /// </summary>
    public static class ServiceProxyHelper
    {
        public static IRoutingAiSlaveService GetSlaveProxy(IPEndPoint ep)
        {
            EndpointAddress endpoint = new EndpointAddress(String.Format("http://{0}/RoutingAi/Slave", ep.ToString()));
            return ChannelFactory<IRoutingAiSlaveService>.CreateChannel(new BasicHttpBinding(), endpoint);
        }
    }
}