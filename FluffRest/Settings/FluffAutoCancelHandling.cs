using System;
using System.Collections.Generic;
using System.Text;

namespace FluffRest.Settings
{
    /// <summary>
    /// Controls how automatic cancellation of request made by the client will work.
    /// </summary>
    public enum FluffAutoCancelHandling : short
    {
        /// <summary>
        /// Each endpoint will be assigned a cancellation source, meaning that if you call the same endpoint the new request will cancel the old one but only for this endpoint.
        /// </summary>
        PerEndpoint = default,
        /// <summary>
        /// Every request made will trigger the cancelation of the previous one, regardless of the endpoint.
        /// </summary>
        PerClient = 1
    }
}
