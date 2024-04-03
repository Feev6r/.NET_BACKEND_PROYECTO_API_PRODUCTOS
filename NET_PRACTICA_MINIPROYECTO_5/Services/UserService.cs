using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;
using System.Security.Claims;

namespace NET_PRACTICA_MINIPROYECTO_5.Services
{
    public class UserService : IUserService
    {

        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User GetUser(ClaimsPrincipal UserClaims)
        {
            try
            {
                int idUser = int.Parse(UserClaims.FindFirst("UserId")!.Value);

                return _userRepository.GetAll(idUser);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
