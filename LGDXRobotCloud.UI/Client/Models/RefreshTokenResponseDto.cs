// <auto-generated/>
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
namespace LGDXRobotCloud.UI.Client.Models
{
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    #pragma warning disable CS1591
    public partial class RefreshTokenResponseDto : IAdditionalDataHolder, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The accessToken property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? AccessToken { get; set; }
#nullable restore
#else
        public string AccessToken { get; set; }
#endif
        /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
        public IDictionary<string, object> AdditionalData { get; set; }
        /// <summary>The expiresMins property</summary>
        public int? ExpiresMins { get; set; }
        /// <summary>The refreshToken property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? RefreshToken { get; set; }
#nullable restore
#else
        public string RefreshToken { get; set; }
#endif
        /// <summary>
        /// Instantiates a new <see cref="global::LGDXRobotCloud.UI.Client.Models.RefreshTokenResponseDto"/> and sets the default values.
        /// </summary>
        public RefreshTokenResponseDto()
        {
            AdditionalData = new Dictionary<string, object>();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::LGDXRobotCloud.UI.Client.Models.RefreshTokenResponseDto"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static global::LGDXRobotCloud.UI.Client.Models.RefreshTokenResponseDto CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::LGDXRobotCloud.UI.Client.Models.RefreshTokenResponseDto();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "accessToken", n => { AccessToken = n.GetStringValue(); } },
                { "expiresMins", n => { ExpiresMins = n.GetIntValue(); } },
                { "refreshToken", n => { RefreshToken = n.GetStringValue(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("accessToken", AccessToken);
            writer.WriteIntValue("expiresMins", ExpiresMins);
            writer.WriteStringValue("refreshToken", RefreshToken);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}
#pragma warning restore CS0618
