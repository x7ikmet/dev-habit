﻿using DevHabit.Api.Entities;
using FluentValidation;
namespace DevHabit.Api.DTOs.Habits;

public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] AllowedUnits =
    {
        "minutes", "hours", "steps", "km", "cal",
        "pages", "books", "tasks", "sessions"
    };
    private static readonly string[] AllowedUnitsForBinaryHabits = ["sessions", "tasks"];

    public CreateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100)
            .WithMessage("Habit name must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invaild habit type");

        RuleFor(x => x.Frequency.Type)
            .IsInEnum()
            .WithMessage("Invalid frquency period");

        RuleFor(x => x.Frequency.TimePerPeriod)
            .GreaterThan(0)
            .WithMessage("Frequency must be greater than 0");

        RuleFor(x => x.Target.Value)
            .GreaterThan(0)
            .WithMessage("Target value must be greater than 0");

        RuleFor(x => x.Target.Unit)
            .NotEmpty()
            .Must(unit => AllowedUnits.Contains(unit.ToUpperInvariant()))
            .WithMessage($"Unit must be one of: {string.Join(", ", AllowedUnits)}");

        RuleFor(x => x.EndDate)
            .Must(date => date is null || date.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("End date must be in the future");

        When(x => x.Milestone is not null, () =>
        {
            RuleFor(x => x.Milestone!.Target)
                .GreaterThan(0)
                .WithMessage("WithMessage target must be greater than 0");
        });

        RuleFor(x => x.Target.Unit)
            .Must((dto, unit) => IsTargetUnitCompatibleWithType(dto.Type, unit));
    }

    private static bool IsTargetUnitCompatibleWithType(HabitType type, string unit)
    {
        string normalizedUnit = unit.ToUpperInvariant();
        return type switch
        {
            HabitType.Binary => AllowedUnitsForBinaryHabits.Contains(normalizedUnit),
            HabitType.Measurable => AllowedUnits.Contains(normalizedUnit),
            _ => false
        };

    }
}
