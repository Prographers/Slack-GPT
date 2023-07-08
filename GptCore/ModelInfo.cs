namespace GptCore;

/// <summary>
///     Represents information about a specific AI model.
/// </summary>
public class ModelInfo
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelInfo" /> class.
    /// </summary>
    /// <param name="model">The model name.</param>
    /// <param name="aliases">The model aliases.</param>
    public ModelInfo(string model, params string[] aliases)
    {
        Model = model;
        Aliases = aliases;
    }

    /// <summary>
    ///     Gets or sets the model name.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    ///     Gets or sets the model aliases.
    /// </summary>
    public string[] Aliases { get; set; }
}