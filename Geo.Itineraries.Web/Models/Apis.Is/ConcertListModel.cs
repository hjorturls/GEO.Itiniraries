﻿// <copyright file="ConcertListModel.cs" company="CCP hf.">
//     Copyright 2014, JOK All rights reserved.
// </copyright>

namespace Geo.Itineraries.Models.ApisIs
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// The concert list model
    /// </summary>
    [Serializable]
    public class ConcertListModel
    {
        /// <summary>
        /// Gets or sets the results
        /// </summary>
        [DataMember]
        public ICollection<ConcertDateModel> Results { get; set; }
    }
}