using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Security.Cryptography;
using LoginWindowSystem.Models;

namespace LoginWindowSystem.Services
{
    public class DatabaseService
    {
        private const string CONN_STR =
            "Data Source=.;Initial Catalog=CyberpunkLoginDB;Integrated Security=Ture;";

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
                            return new UserModel{
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
