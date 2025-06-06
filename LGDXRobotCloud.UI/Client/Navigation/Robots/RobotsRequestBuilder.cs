// <auto-generated/>
#pragma warning disable CS0618
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Client.Navigation.Robots.Item;
using LGDXRobotCloud.UI.Client.Navigation.Robots.Search;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace LGDXRobotCloud.UI.Client.Navigation.Robots
{
    /// <summary>
    /// Builds and executes requests for operations under \Navigation\Robots
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    public partial class RobotsRequestBuilder : BaseRequestBuilder
    {
        /// <summary>The Search property</summary>
        public global::LGDXRobotCloud.UI.Client.Navigation.Robots.Search.SearchRequestBuilder Search
        {
            get => new global::LGDXRobotCloud.UI.Client.Navigation.Robots.Search.SearchRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>Gets an item from the LGDXRobotCloud.UI.Client.Navigation.Robots.item collection</summary>
        /// <param name="position">Unique identifier of the item</param>
        /// <returns>A <see cref="global::LGDXRobotCloud.UI.Client.Navigation.Robots.Item.RobotsItemRequestBuilder"/></returns>
        public global::LGDXRobotCloud.UI.Client.Navigation.Robots.Item.RobotsItemRequestBuilder this[Guid position]
        {
            get
            {
                var urlTplParams = new Dictionary<string, object>(PathParameters);
                urlTplParams.Add("id", position);
                return new global::LGDXRobotCloud.UI.Client.Navigation.Robots.Item.RobotsItemRequestBuilder(urlTplParams, RequestAdapter);
            }
        }
        /// <summary>Gets an item from the LGDXRobotCloud.UI.Client.Navigation.Robots.item collection</summary>
        /// <param name="position">Unique identifier of the item</param>
        /// <returns>A <see cref="global::LGDXRobotCloud.UI.Client.Navigation.Robots.Item.RobotsItemRequestBuilder"/></returns>
        [Obsolete("This indexer is deprecated and will be removed in the next major version. Use the one with the typed parameter instead.")]
        public global::LGDXRobotCloud.UI.Client.Navigation.Robots.Item.RobotsItemRequestBuilder this[string position]
        {
            get
            {
                var urlTplParams = new Dictionary<string, object>(PathParameters);
                if (!string.IsNullOrWhiteSpace(position)) urlTplParams.Add("id", position);
                return new global::LGDXRobotCloud.UI.Client.Navigation.Robots.Item.RobotsItemRequestBuilder(urlTplParams, RequestAdapter);
            }
        }
        /// <summary>
        /// Instantiates a new <see cref="global::LGDXRobotCloud.UI.Client.Navigation.Robots.RobotsRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public RobotsRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/Navigation/Robots{?name*,pageNumber*,pageSize*,realmId*}", pathParameters)
        {
        }
        /// <summary>
        /// Instantiates a new <see cref="global::LGDXRobotCloud.UI.Client.Navigation.Robots.RobotsRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public RobotsRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/Navigation/Robots{?name*,pageNumber*,pageSize*,realmId*}", rawUrl)
        {
        }
        /// <returns>A List&lt;global::LGDXRobotCloud.UI.Client.Models.RobotListDto&gt;</returns>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public async Task<List<global::LGDXRobotCloud.UI.Client.Models.RobotListDto>?> GetAsync(Action<RequestConfiguration<global::LGDXRobotCloud.UI.Client.Navigation.Robots.RobotsRequestBuilder.RobotsRequestBuilderGetQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
#nullable restore
#else
        public async Task<List<global::LGDXRobotCloud.UI.Client.Models.RobotListDto>> GetAsync(Action<RequestConfiguration<global::LGDXRobotCloud.UI.Client.Navigation.Robots.RobotsRequestBuilder.RobotsRequestBuilderGetQueryParameters>> requestConfiguration = default, CancellationToken cancellationToken = default)
        {
#endif
            var requestInfo = ToGetRequestInformation(requestConfiguration);
            var collectionResult = await RequestAdapter.SendCollectionAsync<global::LGDXRobotCloud.UI.Client.Models.RobotListDto>(requestInfo, global::LGDXRobotCloud.UI.Client.Models.RobotListDto.CreateFromDiscriminatorValue, default, cancellationToken).ConfigureAwait(false);
            return collectionResult?.AsList();
        }
        /// <returns>A <see cref="global::LGDXRobotCloud.UI.Client.Models.RobotCertificateIssueDto"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::LGDXRobotCloud.UI.Client.Models.ValidationProblemDetails">When receiving a 400 status code</exception>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public async Task<global::LGDXRobotCloud.UI.Client.Models.RobotCertificateIssueDto?> PostAsync(global::LGDXRobotCloud.UI.Client.Models.RobotCreateDto body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
#nullable restore
#else
        public async Task<global::LGDXRobotCloud.UI.Client.Models.RobotCertificateIssueDto> PostAsync(global::LGDXRobotCloud.UI.Client.Models.RobotCreateDto body, Action<RequestConfiguration<DefaultQueryParameters>> requestConfiguration = default, CancellationToken cancellationToken = default)
        {
#endif
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = ToPostRequestInformation(body, requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", global::LGDXRobotCloud.UI.Client.Models.ValidationProblemDetails.CreateFromDiscriminatorValue },
            };
            return await RequestAdapter.SendAsync<global::LGDXRobotCloud.UI.Client.Models.RobotCertificateIssueDto>(requestInfo, global::LGDXRobotCloud.UI.Client.Models.RobotCertificateIssueDto.CreateFromDiscriminatorValue, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public RequestInformation ToGetRequestInformation(Action<RequestConfiguration<global::LGDXRobotCloud.UI.Client.Navigation.Robots.RobotsRequestBuilder.RobotsRequestBuilderGetQueryParameters>>? requestConfiguration = default)
        {
#nullable restore
#else
        public RequestInformation ToGetRequestInformation(Action<RequestConfiguration<global::LGDXRobotCloud.UI.Client.Navigation.Robots.RobotsRequestBuilder.RobotsRequestBuilderGetQueryParameters>> requestConfiguration = default)
        {
#endif
            var requestInfo = new RequestInformation(Method.GET, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "text/plain;q=0.9");
            return requestInfo;
        }
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public RequestInformation ToPostRequestInformation(global::LGDXRobotCloud.UI.Client.Models.RobotCreateDto body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default)
        {
#nullable restore
#else
        public RequestInformation ToPostRequestInformation(global::LGDXRobotCloud.UI.Client.Models.RobotCreateDto body, Action<RequestConfiguration<DefaultQueryParameters>> requestConfiguration = default)
        {
#endif
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = new RequestInformation(Method.POST, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/json, text/plain;q=0.9");
            requestInfo.SetContentFromParsable(RequestAdapter, "application/json", body);
            return requestInfo;
        }
        /// <summary>
        /// Returns a request builder with the provided arbitrary URL. Using this method means any other path or query parameters are ignored.
        /// </summary>
        /// <returns>A <see cref="global::LGDXRobotCloud.UI.Client.Navigation.Robots.RobotsRequestBuilder"/></returns>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        public global::LGDXRobotCloud.UI.Client.Navigation.Robots.RobotsRequestBuilder WithUrl(string rawUrl)
        {
            return new global::LGDXRobotCloud.UI.Client.Navigation.Robots.RobotsRequestBuilder(rawUrl, RequestAdapter);
        }
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        #pragma warning disable CS1591
        public partial class RobotsRequestBuilderGetQueryParameters 
        #pragma warning restore CS1591
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
            [QueryParameter("name")]
            public string? Name { get; set; }
#nullable restore
#else
            [QueryParameter("name")]
            public string Name { get; set; }
#endif
            [QueryParameter("pageNumber")]
            public int? PageNumber { get; set; }
            [QueryParameter("pageSize")]
            public int? PageSize { get; set; }
            [QueryParameter("realmId")]
            public int? RealmId { get; set; }
        }
        /// <summary>
        /// Configuration for the request such as headers, query parameters, and middleware options.
        /// </summary>
        [Obsolete("This class is deprecated. Please use the generic RequestConfiguration class generated by the generator.")]
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        public partial class RobotsRequestBuilderGetRequestConfiguration : RequestConfiguration<global::LGDXRobotCloud.UI.Client.Navigation.Robots.RobotsRequestBuilder.RobotsRequestBuilderGetQueryParameters>
        {
        }
        /// <summary>
        /// Configuration for the request such as headers, query parameters, and middleware options.
        /// </summary>
        [Obsolete("This class is deprecated. Please use the generic RequestConfiguration class generated by the generator.")]
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        public partial class RobotsRequestBuilderPostRequestConfiguration : RequestConfiguration<DefaultQueryParameters>
        {
        }
    }
}
#pragma warning restore CS0618
