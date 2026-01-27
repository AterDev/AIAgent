namespace Entity.KnowledgeBaseMod;

using System.ComponentModel;

public enum RagDocumentStatus
{
    [Description("Pending")]
    Pending = 0,

    [Description("Parsing")]
    Parsing = 1,

    [Description("Vectorizing")]
    Vectorizing = 2,

    [Description("Completed")]
    Completed = 3,

    [Description("Failed")]
    Failed = 4,
}
