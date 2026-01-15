using Microsoft.EntityFrameworkCore;
using NotesApi.Models;

namespace NotesApi.Data;

public class NoteDbContext : DbContext
{
    public NoteDbContext(DbContextOptions<NoteDbContext> options) : base(options) { }

    public DbSet<Note> Notes => Set<Note>();
}
