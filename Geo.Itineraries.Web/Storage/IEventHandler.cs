﻿using Geo.Itineraries.Web.Models;
using ServiceStack.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Geo.Itineraries.Web.Storage
{
    public abstract class IEventHandler
    {
        public abstract void GetEvents();

        protected void UpdateRedis(EventListModel eventModels)
        {
            // TODO: KRAPP THIS SHOULD BE AN UPDATE NOT A BLIND STORE
            
            try
            {
                var redisClient = new RedisClient("localhost");
                var eventClient = redisClient.As<EventListModel>();

                eventClient.Store(eventModels);
            }
            catch (System.Exception)
            {
                // TODO: KRAPP SWALLOW ALL EXCEPTIONS
            }
            
        }
    }
}
