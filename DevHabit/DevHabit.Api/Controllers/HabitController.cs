﻿using System.Linq.Expressions;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits()
    {
        List<HabitDto> habits = await dbContext
            .Habits
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync();
        var habitsCollectionDto = new HabitsCollectionDto
        {
            Data = habits
        };
        return Ok(habitsCollectionDto); 
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabit(string id)
    {
        HabitWithTagsDto? habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDtoWithTags())
            .FirstOrDefaultAsync();
        if (habit == null)
        {
            return NotFound();
        }
        return Ok(habit);
    }

    

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(
        CreateHabitDto createHabitDto,
        IValidator<CreateHabitDto> validator
        //ProblemDetailsFactory problemDetailsFactory
        )
    {

        //ValidationResult validationResult = await validator.ValidateAsync(createHabitDto);
        //if (!validationResult.IsValid)
        //{ 
        //    return BadRequest(validationResult.ToDictionary());
        //}

        await validator.ValidateAndThrowAsync(createHabitDto);

        var habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();
         
        var habitDto = habit.ToDto();
        return CreatedAtAction(nameof(GetHabit),new { id = habit.Id }, habitDto);

    }


    [HttpPut("{id}")]
    public async Task<ActionResult<HabitDto>> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if(habit == null)
        {
            return NotFound();
        }
         
        habit.UpdateFromDto(updateHabitDto);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    


    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit == null)
        {
            return NotFound();
        }

        HabitDto habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto, ModelState);

        if(!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }
        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        
        return NoContent();
    }
}
