﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using ShadowEditor.Model.System;

namespace ShadowEditor.Server.Helpers
{
    /// <summary>
    /// 用户帮助器
    /// </summary>
    /// <remarks>UserModel带有密码信息，注意不要返回给前端。</remarks>
    public class UserHelper
    {
        /// <summary>
        /// 获取当前用户
        /// </summary>
        /// <returns></returns>
        public static UserModel GetCurrentUser()
        {
            var cookies = HttpContext.Current.Request.Cookies;

            // 获取cookie
            var cookie = cookies.Get(FormsAuthentication.FormsCookieName);

            if (cookie == null)
            {
                return null;
            }

            // 解析ticket数据
            var ticket = FormsAuthentication.Decrypt(cookie.Value);

            LoginTicketDataModel userData = null;

            try
            {
                userData = JsonConvert.DeserializeObject<LoginTicketDataModel>(ticket.UserData);
            }
            catch (Exception ex)
            {
                var log = LogHelper.GetLogger(typeof(UserHelper));
                log.Error("User ticket deserialized failed.", ex);
                return null;
            }

            // 获取用户信息
            ObjectId objectId;

            if (!ObjectId.TryParse(userData.UserID, out objectId))
            {
                var log = LogHelper.GetLogger(typeof(UserHelper));
                log.Error("UserID parse failed.");
                return null;
            }

            var mongo = new MongoHelper();
            var filter = Builders<BsonDocument>.Filter.Eq("ID", objectId);

            var doc = mongo.FindOne(Constant.UserCollectionName, filter);

            if (doc == null)
            {
                return null;
            }

            // 获取角色和权限信息
            var roleID = "";
            var roleName = "";
            var operatingAuthorities = new List<String>();

            if (doc.Contains("RoleID") && ObjectId.TryParse(doc["RoleID"].ToString(), out objectId))
            {
                filter = Builders<BsonDocument>.Filter.Eq("ID", objectId);

                var role = mongo.FindOne(Constant.RoleCollectionName, filter);

                if (role != null)
                {
                    roleID = role["ID"].ToString();
                    roleName = role["Name"].ToString();

                    filter = Builders<BsonDocument>.Filter.Eq("RoleID", roleID);

                    var authorities = mongo.FindMany(Constant.OperatingAuthorityCollectionName, filter).ToList();

                    foreach (var authority in authorities)
                    {
                        operatingAuthorities.Add(authority["AuthorityID"].ToString());
                    }
                }
            }

            return new UserModel
            {
                ID = doc["ID"].ToString(),
                Username = doc["Username"].ToString(),
                Name = doc["Name"].ToString(),
                Password = doc["Password"].ToString(),
                RoleID = roleID,
                RoleName = roleName,
                OperatingAuthorities = operatingAuthorities,
                Gender = doc["Gender"].ToInt32(),
                Phone = doc["Phone"].ToString(),
                Email = doc["Email"].ToString(),
                QQ = doc["QQ"].ToString(),
                CreateTime = doc["CreateTime"].ToLocalTime(),
                UpdateTime = doc["UpdateTime"].ToLocalTime(),
                Salt = doc["Salt"].ToString(),
                Status = doc["Status"].ToInt32(),
            };
        }
    }
}
