using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IUserRepository
    {
        public User GetAll(int IdUser);
    }
}
