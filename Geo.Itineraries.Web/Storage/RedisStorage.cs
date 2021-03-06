﻿// <copyright file="RedisStorage.cs" company="CCP hf.">
//     Copyright 2014, JOK All rights reserved.
// </copyright>

namespace Geo.Itineraries.Web.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Threading.Tasks;
    using Geo.Itineraries.Web.Helpers;
    using Geo.Itineraries.Web.Models;
    using ServiceStack.Redis;

    /// <summary>
    /// REDIS storage
    /// </summary>
    public class RedisStorage : IItineraryStorage
    {
        /// <summary>
        /// This method gets events from REDIS and filters out based on parameters
        /// </summary>
        /// <param name="position">GEO coordination</param>
        /// <param name="hourRange">Hour range</param>
        /// <param name="radiusRange">Radius range</param>
        /// <param name="categories">Event categories</param>
        /// <returns>An event list model</returns>
        public EventListModel GetEvents(GeoCoordinate position, TimeRanges hourRange, RadiusRanges radiusRange, IList<EventTypes> categories)
        {
            EventListModel list = new EventListModel();
            foreach (var category in categories)
            {
                var eventListModels = this.FetchFromRedis(category) ?? new EventListModel();
                
                list.EventModels.AddRange(eventListModels.EventModels);
            }

            list.EventModels.RemoveAll(x => DateTime.UtcNow.AddHours((double)hourRange) < x.EventDate);

            list.EventModels.RemoveAll(x => x.Venue == null);
            list.EventModels.RemoveAll(x => !VenueHelper.IsVenueWithinRadius(x.Venue, position, (int)radiusRange));

            return list;
        }

        /// <summary>
        /// Stores a missing venue in REDIS
        /// </summary>
        /// <param name="venueName">Venue that is missing</param>
        public void StoreMissingVenue(string venueName)
        {
            var redisClient = new RedisClient("localhost");
            var eventClient = redisClient.As<MissingVenueModel>();

            MissingVenueModel missingVenue = new MissingVenueModel { VenueName = venueName, DateMissing = DateTime.UtcNow };

            eventClient.Store(missingVenue);
        }

        /// <summary>
        /// Primes the REDIS storage layer with data from APIS.is
        /// </summary>
        public void PrimeCache()
        {
            Task.Factory.StartNew(() => new ApisIs.MovieHandler().GetEvents(this.UpdateRedis));
            Task.Factory.StartNew(() => new ApisIs.SportHandler().GetEvents(this.UpdateRedis));
            Task.Factory.StartNew(() => new ApisIs.ConcertHandler().GetEvents(this.UpdateRedis));
            Task.Factory.StartNew(() => new ApisIs.TheaterHandler().GetEvents(this.UpdateRedis));
        }

        /// <summary>
        /// Gets event list models by event type
        /// </summary>
        /// <param name="eventType">Event type to get by</param>
        /// <returns>An event list model</returns>
        private EventListModel FetchFromRedis(EventTypes eventType)
        {
            try
            {
                var redisClient = new RedisClient("localhost");
                var eventClient = redisClient.As<EventListModel>();

                return eventClient.GetById((int)eventType);
            }
            catch (Exception)
            {
                return new EventListModel();
            }
        }

        /// <summary>
        /// Updates REDIS with the event list model
        /// </summary>
        /// <param name="eventModels">Event list model to update REDIS with</param>
        private void UpdateRedis(EventListModel eventModels)
        {
            try
            {
                // TODO: KRAPP THE REDIS CLIENT HOST SHOULD BE CONFIGURABLE
                var redisClient = new RedisClient("localhost");
                var eventClient = redisClient.As<EventListModel>();

                eventClient.Store(eventModels);
            }
            catch (Exception)
            {
                // TODO: KRAPP LOG AND SWALLOW ALL EXCEPTIONS
            }
        }
    }
}