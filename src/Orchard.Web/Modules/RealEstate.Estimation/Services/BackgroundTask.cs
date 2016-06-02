/*
 * Now we'll see something very interesting!
 * Sometimes you want a specific job to be run automatically periodically or at a given time. Orchard has background tasks for this.
 */

using System.Runtime.InteropServices;
using Orchard.Tasks;
using Orchard.Logging;
using RealEstate.Estimation.Models;
using System;
using Orchard.Data;
using System.Collections.Generic;
using System.Linq;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.Helpers;
using Orchard.ContentManagement;

namespace RealEstate.Estimation.Services
{
    // IBackgroundTask implementations have one method: Sweep(). Orchard calls this every minute, so you can use this to implement periodic background
    // tasks.
    public class BackgroundTask : IBackgroundTask
    {
        private const int Batch = 20;

        // An event! Orchard also has an awesome event handling framework that's quite different from standard .NET events and is loosely coupled.
        // Check out the interface and come back here!
        private readonly IBackgroundEventHandler _eventHandler;
        private readonly IContentManager _contentManager;
        private readonly IRepository<EstimateRecord> _estimateRepository;
        private readonly IEstimationService _estimationService;

        public ILogger Logger { get; set; }

        // Notice that we inject a single IBackgroundEventHandler here although there can be (actually are) multiple implementations. As we'll see
        // in a moment, there is a good amount of magic involved.
        public BackgroundTask(
            IBackgroundEventHandler eventHandler,
            IContentManager contentManager,
            IRepository<EstimateRecord> estimateRepository,
            IEstimationService estimationService
            )
        {
            _eventHandler = eventHandler;
            _contentManager = contentManager;
            _estimateRepository = estimateRepository;
            _estimationService = estimationService;

            Logger = NullLogger.Instance;
        }


        public void Sweep()
        {
            // Calling into the event. Although we call the method on a single IBackgroundEventHandler object really this method of all the
            // implementations is called! Check out: this will fire every minute, so the respective method of ScheduledTaskHandler and 
            // BackgroundTaskHandler will be called too.
            _eventHandler.BackgroundTaskFired();

            //_estimationService.BackgroundEstimate();
        }

        // NEXT STATION: Let's see Services/ScheduledTask! Some more background tasks will follow with more event handling awesomeness.
    }
}