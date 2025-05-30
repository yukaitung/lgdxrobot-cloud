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
    public partial class AutoTaskDetailUpdateDto : IAdditionalDataHolder, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
        public IDictionary<string, object> AdditionalData { get; set; }
        /// <summary>The customRotation property</summary>
        public double? CustomRotation { get; set; }
        /// <summary>The customX property</summary>
        public double? CustomX { get; set; }
        /// <summary>The customY property</summary>
        public double? CustomY { get; set; }
        /// <summary>The id property</summary>
        public int? Id { get; set; }
        /// <summary>The order property</summary>
        public int? Order { get; set; }
        /// <summary>The waypointId property</summary>
        public int? WaypointId { get; set; }
        /// <summary>
        /// Instantiates a new <see cref="global::LGDXRobotCloud.UI.Client.Models.AutoTaskDetailUpdateDto"/> and sets the default values.
        /// </summary>
        public AutoTaskDetailUpdateDto()
        {
            AdditionalData = new Dictionary<string, object>();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::LGDXRobotCloud.UI.Client.Models.AutoTaskDetailUpdateDto"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static global::LGDXRobotCloud.UI.Client.Models.AutoTaskDetailUpdateDto CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::LGDXRobotCloud.UI.Client.Models.AutoTaskDetailUpdateDto();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "customRotation", n => { CustomRotation = n.GetDoubleValue(); } },
                { "customX", n => { CustomX = n.GetDoubleValue(); } },
                { "customY", n => { CustomY = n.GetDoubleValue(); } },
                { "id", n => { Id = n.GetIntValue(); } },
                { "order", n => { Order = n.GetIntValue(); } },
                { "waypointId", n => { WaypointId = n.GetIntValue(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteDoubleValue("customRotation", CustomRotation);
            writer.WriteDoubleValue("customX", CustomX);
            writer.WriteDoubleValue("customY", CustomY);
            writer.WriteIntValue("id", Id);
            writer.WriteIntValue("order", Order);
            writer.WriteIntValue("waypointId", WaypointId);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}
#pragma warning restore CS0618
