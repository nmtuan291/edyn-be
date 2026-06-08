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
    
    public async Task<List<Comment>> GetCommentByThreadIdAsync(Guid threadId, CancellationToken cancellationToken = default)
    {
        var  allComments = await _context.Comments
            .AsNoTracking()
            .Where(t => t.ThreadId == threadId)
            .ToListAsync(cancellationToken);

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

    public async Task<Comment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == commentId, cancellationToken);
        
        return _mapper.Map<Comment>(comment);
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

    public async Task<Comment?> GetParentCommentAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .Where(c => c.Id == commentId)
            .SingleOrDefaultAsync(cancellationToken);
        
        return _mapper.Map<Comment>(comment);
    }
    
    public async Task UpdateCommentAsync(Comment comment)
    {
        var ef = await _context.Comments
            .FirstAsync(t => t.Id == comment.Id);

        _mapper.Map(comment, ef);
    }

    public async Task<(List<(Comment Comment, string ThreadTitle, string RealmShortName)> Comments, int TotalCount)> GetCommentsByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Comments
            .AsNoTracking()
            .Include(c => c.ThreadEf)
            .ThenInclude(t => t.ForumEf)
            .Where(c => c.OwnerId == userId && !c.Deleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new 
            { 
                CommentEf = c, 
                ThreadTitle = c.ThreadEf.Title, 
                RealmShortName = c.ThreadEf.ForumEf.ShortName 
            })
            .ToListAsync(cancellationToken);

        var result = comments.Select(c => (
            Comment: _mapper.Map<Comment>(c.CommentEf),
            ThreadTitle: c.ThreadTitle,
            RealmShortName: c.RealmShortName
        )).ToList();

        return (result, totalCount);
    }
}