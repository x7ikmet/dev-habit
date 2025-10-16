﻿namespace DevHabit.Api.DTOs.Tags;

public sealed record TagCollectionDto
{
    public List<TagDto> Data { get; set; }
}

public sealed record TagDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
