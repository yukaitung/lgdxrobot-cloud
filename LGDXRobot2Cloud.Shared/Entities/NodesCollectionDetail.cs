using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.Shared.Entities
{
  public class NodesCollectionDetail
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("NodeId")]
    public required Node Node { get; set; }

    public int NodeId { get; set; }

    public bool AutoRestart { get; set; }

    [ForeignKey("NodesCollectionId")]
    public required NodesCollection NodesCollection { get; set; }

    public int NodesCollectionId { get; set; }
  }
}