using Riok.Mapperly.Abstractions;
using SA.Identity.Api.ViewModels.Users;
using SA.Identity.Application.Dtos.Users;

namespace SA.Identity.Api.Mappers.Users;

[Mapper]
public static partial class UserMapper
{
    public static partial UserResponse ToUserResponse(UserDto dto);
    public static partial UserDetailResponse ToUserDetailResponse(UserDetailDto dto);
    public static partial UserDetailRoleResponse ToUserDetailRoleResponse(UserDetailRoleDto dto);
    public static partial UserDetailAssignmentResponse ToUserDetailAssignmentResponse(UserDetailAssignmentDto dto);
    public static partial UserDetailPermissionResponse ToUserDetailPermissionResponse(UserDetailPermissionDto dto);

    public static UserListResponse ToUserListResponse(
        IReadOnlyList<UserDto> items, int total, int page, int pageSize)
    {
        return new UserListResponse(
            items.Select(ToUserResponse).ToList(),
            total,
            page,
            pageSize);
    }
}
