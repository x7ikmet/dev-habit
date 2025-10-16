namespace DevHabit.Api.DTOs.Tags;

public sealed record UpdatedTagDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
