using AutoMapper;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace ForumService.ForumService.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ForumDbContext _context;
    private readonly IMapper _mapper;

    public CommentRepository(ForumDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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
                    parentComment.ChildrenComments = new List<CommentEf>();
                    
                parentComment.ChildrenComments.Add(comment);
            }
        }
        allComments = allComments.Where(comment => comment.ParentId == null).ToList();
        
        return _mapper.Map<List<Comment>>(allComments);
    }
    
    public async Task InsertCommentAsync(Comment comment)
    {
        CommentEf commentEf = _mapper.Map<CommentEf>(comment);
        await _context.Comments.AddAsync(commentEf);
    }
    
    public async Task DeleteCommentById(Guid commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment != null)
            _context.Comments.Remove(comment);
    }

    public async Task<Comment?> GetParentCommentAsync(Guid commentId)
    {
        var comment = await _context.Comments
            .Where(c => c.Id == commentId)
            .SingleOrDefaultAsync();
        
        return _mapper.Map<Comment>(comment);
    }
}