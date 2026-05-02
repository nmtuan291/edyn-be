namespace ForumService.ForumService.Application.Enums;

[Flags]
public enum ForumPermissionType
{
    None             = 0,
    ManageForumInfo  = 1 << 0,
    ManageRoles      = 1 << 1,
    DeleteForum      = 1 << 2,
    PinThread        = 1 << 3,
    LockThread       = 1 << 4,
    DeleteThread     = 1 << 5,
    EditAnyThread    = 1 << 6,
    DeleteComment    = 1 << 7,
    EditAnyComment   = 1 << 8,
    BanMember        = 1 << 9,
    ManageTags       = 1 << 10,
    CreateThread     = 1 << 11,
    CreateComment    = 1 << 12,
    Vote             = 1 << 13,

    All = ManageForumInfo | ManageRoles | DeleteForum | PinThread | LockThread
        | DeleteThread | EditAnyThread | DeleteComment | EditAnyComment
        | BanMember | ManageTags | CreateThread | CreateComment | Vote
}
