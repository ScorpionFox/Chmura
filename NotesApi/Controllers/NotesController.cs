using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;

namespace NotesApi.Controllers;

[ApiController]
[Route("notes")]
public class NotesController : ControllerBase
{
    private readonly NoteDbContext _db;

    public NotesController(NoteDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Note>>> GetAll()
    {
        var notes = await _db.Notes
            .OrderByDescending(n => n.Id)
            .ToListAsync();

        return Ok(notes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Note>> GetById(int id)
    {
        var note = await _db.Notes.FindAsync(id);
        if (note == null) return NotFound();

        return Ok(note);
    }

    public record CreateNoteRequest(string Title, string Content);

    [HttpPost]
    public async Task<ActionResult<Note>> Create([FromBody] CreateNoteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest("Title is required.");

        var note = new Note
        {
            Title = req.Title.Trim(),
            Content = req.Content?.Trim() ?? "",
            CreatedAt = DateTime.UtcNow
        };

        _db.Notes.Add(note);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
    }

    public record UpdateNoteRequest(string Title, string Content);

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Note>> Update(int id, [FromBody] UpdateNoteRequest req)
    {
        var note = await _db.Notes.FindAsync(id);
        if (note == null) return NotFound();

        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest("Title is required.");

        note.Title = req.Title.Trim();
        note.Content = req.Content?.Trim() ?? "";
        note.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(note);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var note = await _db.Notes.FindAsync(id);
        if (note == null) return NotFound();

        _db.Notes.Remove(note);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
