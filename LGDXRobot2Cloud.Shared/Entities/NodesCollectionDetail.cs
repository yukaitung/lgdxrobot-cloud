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
    public Node Node { get; set; } = null!;

    public int NodeId { get; set; }

    public bool AutoRestart { get; set; }

    [ForeignKey("NodesCollectionId")]
    public NodesCollection NodesCollection { get; set; } = null!;

    public int NodesCollectionId { get; set; }
  }
}