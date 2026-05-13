using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginWindowSystem.Models
{
    public class UserModel
    {
        //用户id
        //对应数据库主键
        public int Id { get; set; }

        //用户名称
        public string Username { get; set; }

        //用户密码
        //存储的SHA256哈希
        public string PasswordHash { get; set; }

        //显示的用户昵称
        public string Nickname { get; set; }

        //注册时间
        public DateTime CreatedAt { get; set; }

    }
}
