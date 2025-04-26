using LiteDB;

namespace RaceServer
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AccountDatabase
    {
        private readonly LiteDatabase _db;

        public AccountDatabase(string path)
        {
            _db = new LiteDatabase(path);
            var accounts = _db.GetCollection<Account>("accounts");
            accounts.EnsureIndex(u => u.Username, unique: true);
        }

        public bool CreateUser(Account account)
        {
            var accounts = _db.GetCollection<Account>();
            if (accounts.Exists(u => u.Username == account.Username)) { return false; }

            accounts.Insert(account);
            return true;
        }

        public Account GetUser(string username)
        {
            return _db.GetCollection<Account>()
                .FindOne(u => u.Username == username);
        }
    }
}