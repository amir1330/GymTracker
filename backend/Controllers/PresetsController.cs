using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymTracker.Models;
using GymTracker.Services;
using GymTracker.DTOs.Presets;
using AutoMapper;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PresetsController : ControllerBase
{
    private readonly PresetsService _presetsService;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public PresetsController(PresetsService presetsService, UserManager<User> userManager, IMapper mapper)
    {
        _presetsService = presetsService;
        _userManager = userManager;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var presets = await _presetsService.GetAllAsync(userId);
        return Ok(presets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var preset = await _presetsService.GetByIdAsync(id, userId);
        if (preset == null) return NotFound();
        return Ok(preset);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePresetRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var preset = new Preset
        {
            Name = request.Name,
            UserId = userId,
            PresetExercises = request.Exercises.Select(e => new PresetExercise
            {
                ExerciseId = e.ExerciseId,
                DefaultSets = e.DefaultSets,
                DefaultReps = e.DefaultReps,
                DefaultWeight = e.DefaultWeight,
                DefaultDuration = e.DefaultDuration
            }).ToList()
        };

        var created = await _presetsService.CreateAsync(preset);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePresetRequest request)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);

        var preset = new Preset
        {
            Name = request.Name,
            PresetExercises = request.Exercises.Select(e => new PresetExercise
            {
                PresetId = id,
                ExerciseId = e.ExerciseId,
                DefaultSets = e.DefaultSets,
                DefaultReps = e.DefaultReps,
                DefaultWeight = e.DefaultWeight,
                DefaultDuration = e.DefaultDuration
            }).ToList()
        };

        var updated = await _presetsService.UpdateAsync(id, preset, userId);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(_userManager.GetUserId(User)!);
        var deleted = await _presetsService.DeleteAsync(id, userId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
