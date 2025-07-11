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
    public partial class WaypointListDto : IAdditionalDataHolder, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
        public IDictionary<string, object> AdditionalData { get; set; }
        /// <summary>The id property</summary>
        public int? Id { get; set; }
        /// <summary>The name property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? Name { get; set; }
#nullable restore
#else
        public string Name { get; set; }
#endif
        /// <summary>The realm property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public global::LGDXRobotCloud.UI.Client.Models.RealmSearchDto? Realm { get; set; }
#nullable restore
#else
        public global::LGDXRobotCloud.UI.Client.Models.RealmSearchDto Realm { get; set; }
#endif
        /// <summary>The rotation property</summary>
        public double? Rotation { get; set; }
        /// <summary>The x property</summary>
        public double? X { get; set; }
        /// <summary>The y property</summary>
        public double? Y { get; set; }
        /// <summary>
        /// Instantiates a new <see cref="global::LGDXRobotCloud.UI.Client.Models.WaypointListDto"/> and sets the default values.
        /// </summary>
        public WaypointListDto()
        {
            AdditionalData = new Dictionary<string, object>();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::LGDXRobotCloud.UI.Client.Models.WaypointListDto"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static global::LGDXRobotCloud.UI.Client.Models.WaypointListDto CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::LGDXRobotCloud.UI.Client.Models.WaypointListDto();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "id", n => { Id = n.GetIntValue(); } },
                { "name", n => { Name = n.GetStringValue(); } },
                { "realm", n => { Realm = n.GetObjectValue<global::LGDXRobotCloud.UI.Client.Models.RealmSearchDto>(global::LGDXRobotCloud.UI.Client.Models.RealmSearchDto.CreateFromDiscriminatorValue); } },
                { "rotation", n => { Rotation = n.GetDoubleValue(); } },
                { "x", n => { X = n.GetDoubleValue(); } },
                { "y", n => { Y = n.GetDoubleValue(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteIntValue("id", Id);
            writer.WriteStringValue("name", Name);
            writer.WriteObjectValue<global::LGDXRobotCloud.UI.Client.Models.RealmSearchDto>("realm", Realm);
            writer.WriteDoubleValue("rotation", Rotation);
            writer.WriteDoubleValue("x", X);
            writer.WriteDoubleValue("y", Y);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}
#pragma warning restore CS0618
