using Authenticate.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Authenticate.Service
{
    public class UserService : IUser
    {
        private readonly ApplicationDBContext _context;

        public UserService(ApplicationDBContext context)
        {
            _context = context;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(x => x.Username == username);
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;

        }
        public User GetById(int id)
        {
            var user = _context.Users.Where(x => x.Id == id)
                 .FirstOrDefault();

            if(user == null)
                throw new ArgumentException ("The Id is invalid, pls pass in a correct Id");
          
            return user;
        }

        public User Create(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required");

            if (_context.Users.Any(x => x.Username == user.Username))
                throw new ArgumentException("Username \"" + user.Username + "\" is already taken");

            if (_context.Users.Any(x => x.FirstName == user.FirstName))
                throw new ArgumentException("Username \"" + user.FirstName + "\" is already taken");

            if (_context.Users.Any(x => x.LastName == user.LastName))
                throw new ArgumentException("Username \"" + user.LastName + "\" is already taken");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

      
        public void Delete(int id)
        {
           var user = _context.Users.Find(id);
            if(user == null)
                throw new ArgumentException("Invalid ID passed");

            _context.Users.Remove(user);
            _context.SaveChanges();
           

        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public void Update(User user, string password = null)
        {

            var users = _context.Users.Find(user.Id);

            if (user == null)
                throw new ArgumentException("User not found");

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(user.Username) && user.Username != users.Username)
            {
                // throw error if the new username is already taken
                if (_context.Users.Any(x => x.Username == user.Username))
                    throw new ArgumentException("Username " + user.Username + " is already taken");

                user.Username = user.Username;
            }

            // update user properties if provided
            if (!string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName))
            {
                users.FirstName = user.FirstName;
                users.LastName = user.LastName;
            }
          

            // update user properties if provided
            if (!string.IsNullOrWhiteSpace(user.LastName))
                users.LastName = user.LastName;

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _context.Users.Attach(user);
            _context.SaveChanges();

        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be empty or whitespace, only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }


        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {

            if (password == null)
                throw new ArgumentNullException("password");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            if (storedHash.Length != 64)
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");

            if (storedSalt.Length != 128)
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;

        }

    }
}
