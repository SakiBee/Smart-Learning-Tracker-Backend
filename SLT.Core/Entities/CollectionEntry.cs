namespace SLT.Core.Entities;

public class CollectionEntry : BaseEntity
{
    public Guid CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;

    public Guid LearningEntryId { get; set; }
    public LearningEntry LearningEntry { get; set; } = null!;
}