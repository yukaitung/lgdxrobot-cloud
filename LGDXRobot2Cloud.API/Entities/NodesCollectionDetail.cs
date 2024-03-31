using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.API.Entities
{
  public class NodesCollectionDetail
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required Node Node { get; set; }

    [ForeignKey("NodesCollectionId")]
    public required NodesCollection NodesCollection { get; set; }

    public int NodesCollectionId { get; set; }
  }
}