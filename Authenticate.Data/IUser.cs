using System;
using System.Collections.Generic;
using System.Text;

namespace Authenticate.Data
{
    public interface IUser
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User Create(User user, string password);

        void Update(User user, string password = null);
        void Delete(int id);
    }
}
