using LiteDB;
using System.Collections.Generic;
using System.Linq;

namespace CampusLostFoundSystem
{
    public static class DatabaseHelper
    {
        public static List<UserAccount> GetAllUsers()
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var users = db.GetCollection<UserAccount>("users");
                return users.FindAll().ToList();
            }
        }

        public static void AddUser(UserAccount user)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var users = db.GetCollection<UserAccount>("users");

                if (users.FindOne(u => u.Username == user.Username) != null)
                    return;

                users.Insert(user);
            }
        }

        public static UserAccount LoginUser(string username, string password)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var users = db.GetCollection<UserAccount>("users");
                return users.FindOne(u => u.Username == username && u.Password == password);
            }
        }

        public static void CreateDefaultAdmin()
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var users = db.GetCollection<UserAccount>("users");

                if (users.FindOne(u => u.Username == "admin") == null)
                {
                    users.Insert(new UserAccount
                    {
                        FullName = "System Admin",
                        Username = "admin",
                        Password = "admin123",
                        Role = "Admin"
                    });
                }
            }
        }

        private static string dbPath = "CampusLostFound.db";

        public static void AddItem(LostFoundItem item)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var items = db.GetCollection<LostFoundItem>("items");
                items.Insert(item);
            }
        }

        public static List<LostFoundItem> GetItems(string search = "")
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var items = db.GetCollection<LostFoundItem>("items");
                var list = items.FindAll().ToList();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();

                    list = list.Where(i =>
                        i.ItemName.ToLower().Contains(search) ||
                        i.Category.ToLower().Contains(search) ||
                        i.Status.ToLower().Contains(search) ||
                        i.Location.ToLower().Contains(search)
                    ).ToList();
                }

                return list;
            }
        }

        public static void DeleteItem(int id)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var items = db.GetCollection<LostFoundItem>("items");
                items.Delete(id);
            }
        }
        public static void UpdateItem(LostFoundItem item)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var items = db.GetCollection<LostFoundItem>("items");
                items.Update(item);
            }
        }

        public static void AddClaimRequest(ClaimRequest request)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var claims = db.GetCollection<ClaimRequest>("claims");
                claims.Insert(request);
            }
        }

        public static List<ClaimRequest> GetClaimRequests()
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var claims = db.GetCollection<ClaimRequest>("claims");
                return claims.FindAll().ToList();
            }
        }

        public static void UpdateClaimRequest(ClaimRequest request)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var claims = db.GetCollection<ClaimRequest>("claims");
                claims.Update(request);
            }
        }
        public static void AddLog(ActivityLog log)
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var logs = db.GetCollection<ActivityLog>("logs");
                logs.Insert(log);
            }
        }

        public static List<ActivityLog> GetLogs()
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var logs = db.GetCollection<ActivityLog>("logs");

                return logs.FindAll()
                    .OrderByDescending(x => x.DateCreated)
                    .ToList();
            }
        }
        public static void ClearLogs()
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var logs = db.GetCollection<ActivityLog>("logs");
                logs.DeleteAll();
            }
        }
    }
}




