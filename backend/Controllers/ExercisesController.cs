using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public ExercisesController(ExercisesService exercisesService, UserManager<User> userManager, IMapper mapper)
    {
        _exercisesService = exercisesService;
        _userManager = userManager;
        _mapper = mapper;
    }

    private int UserId => int.Parse(_userManager.GetUserId(User)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var exercises = await _exercisesService.GetAllAsync(UserId);
        return Ok(exercises);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var exercise = await _exercisesService.GetByIdAsync(id, UserId);
        if (exercise == null) return NotFound();
        return Ok(exercise);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExerciseRequest request)
    {
        if (await _exercisesService.NameExistsAsync(request.Name, UserId))
            return BadRequest(new { message = "Exercise name already exists" });

        var exercise = _mapper.Map<Exercise>(request);
        var created = await _exercisesService.CreateAsync(exercise, UserId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExerciseRequest request)
    {
        if (await _exercisesService.IsDefaultAsync(id))
            return BadRequest(new { message = "Cannot modify default exercises" });
        if (!await _exercisesService.IsOwnedByAsync(id, UserId))
            return Forbid();

        if (await _exercisesService.NameExistsAsync(request.Name, UserId))
        {
            var existing = await _exercisesService.GetByIdAsync(id, UserId);
            if (existing?.Name != request.Name)
                return BadRequest(new { message = "Exercise name already exists" });
        }

        var exercise = _mapper.Map<Exercise>(request);
        var updated = await _exercisesService.UpdateAsync(id, exercise, UserId);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _exercisesService.IsDefaultAsync(id))
            return BadRequest(new { message = "Cannot delete default exercises" });
        if (!await _exercisesService.IsOwnedByAsync(id, UserId))
            return Forbid();
        if (await _exercisesService.IsInUseAsync(id))
            return BadRequest(new { message = "Exercise is in use" });

        var deleted = await _exercisesService.DeleteAsync(id, UserId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
