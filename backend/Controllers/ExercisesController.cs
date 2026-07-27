using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymTracker.Models;
using GymTracker.Services;
using GymTracker.DTOs.Exercises;
using AutoMapper;

namespace GymTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExercisesController : ControllerBase
{
    private readonly ExercisesService _exercisesService;
    private readonly IMapper _mapper;

    public ExercisesController(ExercisesService exercisesService, IMapper mapper)
    {
        _exercisesService = exercisesService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var exercises = await _exercisesService.GetAllAsync();
        return Ok(exercises);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var exercise = await _exercisesService.GetByIdAsync(id);
        if (exercise == null) return NotFound();
        return Ok(exercise);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExerciseRequest request)
    {
        if (await _exercisesService.NameExistsAsync(request.Name))
            return BadRequest(new { message = "Exercise name already exists" });

        var exercise = _mapper.Map<Exercise>(request);
        exercise.IsDefault = false;

        var created = await _exercisesService.CreateAsync(exercise);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExerciseRequest request)
    {
        if (await _exercisesService.IsDefaultAsync(id))
            return BadRequest(new { message = "Cannot modify default exercises" });

        var exercise = _mapper.Map<Exercise>(request);
        var updated = await _exercisesService.UpdateAsync(id, exercise);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _exercisesService.IsDefaultAsync(id))
            return BadRequest(new { message = "Cannot delete default exercises" });
        if (await _exercisesService.IsInUseAsync(id))
            return BadRequest(new { message = "Exercise is in use" });

        var deleted = await _exercisesService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
