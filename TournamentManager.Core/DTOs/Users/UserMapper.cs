using TournamentManager.Core.Models;

namespace TournamentManager.Core.DTOs.Users
{
    public static class UserMapper
    {
        public static UserDto ToDto(this Models.User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Patronymic = user.Patronymic,
                Login = user.Login,
                Password = user.PasswordHash
            };
        }
    }
}
