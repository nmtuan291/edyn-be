using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForumService.ForumService.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ForumDbContext _context;

    public CommentRepository(ForumDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Comment>> GetCommentByThreadIdAsync(Guid threadId)
    {
        var  allComments = await _context.Comments
            .Where(t => t.ThreadId == threadId)
            .ToListAsync();

        var commentDict = allComments.ToDictionary(c => c.Id);
        foreach (var comment in allComments)
        {
            if (comment.ParentId != null && commentDict.TryGetValue(comment.ParentId.Value, out var parentComment))
            {
                if (parentComment.ChildrenComments == null) 
                    parentComment.ChildrenComments = new List<Comment>();
                    
                parentComment.ChildrenComments.Add(comment);
            }
        }
            
        return allComments.Where(comment => comment.ParentId == null).ToList();
    }
    
    public async Task InsertCommentAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
    }
    
    public async Task DeleteCommentById(Guid commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment != null)
            _context.Comments.Remove(comment);
    }

    public async Task<Comment?> GetParentCommentAsync(Guid commentId)
    {
        return await _context.Comments
            .Where(c => c.Id == commentId)
            .SingleOrDefaultAsync();
    }
}