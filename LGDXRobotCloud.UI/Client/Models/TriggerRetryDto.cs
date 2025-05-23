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
    public partial class TriggerRetryDto : IAdditionalDataHolder, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
        public IDictionary<string, object> AdditionalData { get; set; }
        /// <summary>The autoTask property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public global::LGDXRobotCloud.UI.Client.Models.AutoTaskSearchDto? AutoTask { get; set; }
#nullable restore
#else
        public global::LGDXRobotCloud.UI.Client.Models.AutoTaskSearchDto AutoTask { get; set; }
#endif
        /// <summary>The body property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? Body { get; set; }
#nullable restore
#else
        public string Body { get; set; }
#endif
        /// <summary>The createdAt property</summary>
        public DateTimeOffset? CreatedAt { get; set; }
        /// <summary>The id property</summary>
        public int? Id { get; set; }
        /// <summary>The sameTriggerFailed property</summary>
        public int? SameTriggerFailed { get; set; }
        /// <summary>The trigger property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public global::LGDXRobotCloud.UI.Client.Models.TriggerListDto? Trigger { get; set; }
#nullable restore
#else
        public global::LGDXRobotCloud.UI.Client.Models.TriggerListDto Trigger { get; set; }
#endif
        /// <summary>
        /// Instantiates a new <see cref="global::LGDXRobotCloud.UI.Client.Models.TriggerRetryDto"/> and sets the default values.
        /// </summary>
        public TriggerRetryDto()
        {
            AdditionalData = new Dictionary<string, object>();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::LGDXRobotCloud.UI.Client.Models.TriggerRetryDto"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static global::LGDXRobotCloud.UI.Client.Models.TriggerRetryDto CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::LGDXRobotCloud.UI.Client.Models.TriggerRetryDto();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "autoTask", n => { AutoTask = n.GetObjectValue<global::LGDXRobotCloud.UI.Client.Models.AutoTaskSearchDto>(global::LGDXRobotCloud.UI.Client.Models.AutoTaskSearchDto.CreateFromDiscriminatorValue); } },
                { "body", n => { Body = n.GetStringValue(); } },
                { "createdAt", n => { CreatedAt = n.GetDateTimeOffsetValue(); } },
                { "id", n => { Id = n.GetIntValue(); } },
                { "sameTriggerFailed", n => { SameTriggerFailed = n.GetIntValue(); } },
                { "trigger", n => { Trigger = n.GetObjectValue<global::LGDXRobotCloud.UI.Client.Models.TriggerListDto>(global::LGDXRobotCloud.UI.Client.Models.TriggerListDto.CreateFromDiscriminatorValue); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<global::LGDXRobotCloud.UI.Client.Models.AutoTaskSearchDto>("autoTask", AutoTask);
            writer.WriteStringValue("body", Body);
            writer.WriteDateTimeOffsetValue("createdAt", CreatedAt);
            writer.WriteIntValue("id", Id);
            writer.WriteIntValue("sameTriggerFailed", SameTriggerFailed);
            writer.WriteObjectValue<global::LGDXRobotCloud.UI.Client.Models.TriggerListDto>("trigger", Trigger);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}
#pragma warning restore CS0618
