// <auto-generated/>
#pragma warning disable CS0618
using LGDXRobotCloud.UI.Client.Automation.AutoTasks;
using LGDXRobotCloud.UI.Client.Automation.AutoTasksNext;
using LGDXRobotCloud.UI.Client.Automation.Flows;
using LGDXRobotCloud.UI.Client.Automation.Progresses;
using LGDXRobotCloud.UI.Client.Automation.TriggerRetries;
using LGDXRobotCloud.UI.Client.Automation.Triggers;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
namespace LGDXRobotCloud.UI.Client.Automation
{
    /// <summary>
    /// Builds and executes requests for operations under \Automation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    public partial class AutomationRequestBuilder : BaseRequestBuilder
    {
        /// <summary>The AutoTasks property</summary>
        public global::LGDXRobotCloud.UI.Client.Automation.AutoTasks.AutoTasksRequestBuilder AutoTasks
        {
            get => new global::LGDXRobotCloud.UI.Client.Automation.AutoTasks.AutoTasksRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The AutoTasksNext property</summary>
        public global::LGDXRobotCloud.UI.Client.Automation.AutoTasksNext.AutoTasksNextRequestBuilder AutoTasksNext
        {
            get => new global::LGDXRobotCloud.UI.Client.Automation.AutoTasksNext.AutoTasksNextRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The Flows property</summary>
        public global::LGDXRobotCloud.UI.Client.Automation.Flows.FlowsRequestBuilder Flows
        {
            get => new global::LGDXRobotCloud.UI.Client.Automation.Flows.FlowsRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The Progresses property</summary>
        public global::LGDXRobotCloud.UI.Client.Automation.Progresses.ProgressesRequestBuilder Progresses
        {
            get => new global::LGDXRobotCloud.UI.Client.Automation.Progresses.ProgressesRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The TriggerRetries property</summary>
        public global::LGDXRobotCloud.UI.Client.Automation.TriggerRetries.TriggerRetriesRequestBuilder TriggerRetries
        {
            get => new global::LGDXRobotCloud.UI.Client.Automation.TriggerRetries.TriggerRetriesRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The Triggers property</summary>
        public global::LGDXRobotCloud.UI.Client.Automation.Triggers.TriggersRequestBuilder Triggers
        {
            get => new global::LGDXRobotCloud.UI.Client.Automation.Triggers.TriggersRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>
        /// Instantiates a new <see cref="global::LGDXRobotCloud.UI.Client.Automation.AutomationRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public AutomationRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/Automation", pathParameters)
        {
        }
        /// <summary>
        /// Instantiates a new <see cref="global::LGDXRobotCloud.UI.Client.Automation.AutomationRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public AutomationRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/Automation", rawUrl)
        {
        }
    }
}
#pragma warning restore CS0618
