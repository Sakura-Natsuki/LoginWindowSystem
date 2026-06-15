using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Configuration;
using LoginWindowSystem.Models;

namespace LoginWindowSystem.Services
{
    public class DatabaseService
    {
        private string CONN_STR = GetConnectionString();

        private static string GetConnectionString()
        {
            var connStr = System.Configuration.ConfigurationManager.ConnectionStrings["CyberpunkDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connStr))
            {
                throw new InvalidOperationException("未在 App.config 中找到连接字符串 CyberpunkDB");
            }

            return connStr;
        }

        public UserModel ValidateLogin(string username, string password)
        {
            //1.将明文修改为SHA256哈希
            string hash = Sha256(password);

            //2.创建数据库连接
            using (var conn = new SqlConnection(CONN_STR))
            {
                conn.Open();

                //参数化查询：@u和@p是占位参数符
                var sql = "SELECT Id,Username,PasswordHash,Nickname,CreatedAt FROM Users WHERE Username=@u AND PasswordHash=@p";

                //3.创建sql命令对象
                using (var cmd = new SqlCommand(sql, conn))
                {
                    //将参数安全地绑定
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", hash);

                    //4.执行查询，获取只读数据流
                    //ExecuteReader用于SELECT查询，返回一个向前只读的数据读取器
                    using (var r = cmd.ExecuteReader())
                    {
                        //5.尝试读取第一行数据
                        //Read()返回true表示有数据行，false表示结果集为空
                        if (r.Read())
                        {
                            //6.将数据库行映射为UserModel对象
                            return new UserModel
                            {
                                Id = (int)r["Id"],
                                Username = (string)r["Username"],
                                PasswordHash = (string)r["PasswordHash"],
                                Nickname = (string)r["Nickname"],
                                CreatedAt = (DateTime)r["CreatedAt"],
                            };
                        }
                    }
                }
            }

            //未找到匹配的用户，返回null表示登录失败
            return null;
        }

        public void InitDatabase()
        {
            using (var conn = new SqlConnection(CONN_STR))
            {
                conn.Open();

                var sql = @"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
CREATE TABLE Users(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(64) NOT NULL,
    Nickname NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
IF NOT EXISTS (SELECT * FROM Users WHERE Username='admin')
INSERT INTO Users(Username,PasswordHash,Nickname) VALUES('admin',@hash,N'管理员');
";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@hash", Sha256("123456"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool RegisterUser(string username, string password, string nickname)
        {
            string hash = Sha256(password);

            using (var conn = new SqlConnection(CONN_STR))
            {
                conn.Open();

                var checkSql = "Select Count(*) From Users Where Username = @u";

                using (var checkCmd = new SqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@u",username);

                    int count = (int)checkCmd.ExecuteScalar();

                    if(count > 0)
                    {
                        return false;
                    }
                }

                var insertSql = "INSERT INTO Users (Username, PasswordHash, Nickname) VALUES (@u, @p, @n)";

                using (var insertCmd = new SqlCommand(insertSql, conn))
                {
                    insertCmd.Parameters.AddWithValue("@u",username);

                    insertCmd.Parameters.AddWithValue("@p",hash);

                    insertCmd.Parameters.AddWithValue("@n",nickname);

                    int rows = insertCmd.ExecuteNonQuery();

                    return rows > 0;
                }
            }
        }

        public List<UserModel> GetAllUser()
        {
            var users = new List<UserModel>();

            using (var conn = new SqlConnection(CONN_STR))
            {
                conn.Open();

                var sql = "select Id, Username, PasswordHash, Nickname, CreatedAt FROM Users order by CreatedAt DESC";

                using (var cmd = new SqlCommand(sql,conn))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            users.Add(new UserModel
                            {
                                Id = (int)r["Id"],
                                Username = (string)r["Username"],
                                Nickname = (string)r["Nickname"],
                                CreatedAt = (DateTime)r["CreatedAt"],
                                PasswordHash = (string)r["PasswordHash"]
                            });
                        }
                    }
                }
            }

            return users;
        }

        public static string Sha256(string raw)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));

                var sb = new StringBuilder();

                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
