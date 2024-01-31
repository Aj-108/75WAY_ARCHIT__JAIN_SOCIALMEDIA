using System;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using Social_Media.Dto;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace Social_Media
{
    public interface IUserService
    {
        public Task<ResponseDto> UserRegister(UserRegisterDto userData);

        public Task<ResponseDto> UserLogin(UserLoginDto userData);

        public Task<ResponseDto> FollowUser(int userId,int followingUserId);

        public Task<List<string>> getTopFollow();
    }

	public class UserService : IUserService
	{
        public IDbConnection Db { get; set; }
        public IConfiguration Config { get; set; }


        public UserService(IDbConnection _db,IConfiguration _config)
        {
            Db = _db;
            Config = _config;
        }

        public async Task<ResponseDto> UserRegister(UserRegisterDto userData)
        {
            ResponseDto result = new()
            {
                Status = false,
                Message = "",
                Data = ""
            };
            try
            {

                using (var con = new SqlConnection(Db.GetConnectionString()))
                {
                    await con.OpenAsync();

                    //Checking if the user already exists or not
                    var sqlCheck = "SELECT COUNT(*) FROM UserDB WHERE email = @email OR username = @username;";
                    using (var cmd = new SqlCommand(sqlCheck, con))
                    {
                        cmd.Parameters.AddWithValue("@email", userData.User_email);
                        cmd.Parameters.AddWithValue("@username", userData.User_name);
                        int existingUserCount = (int)await cmd.ExecuteScalarAsync();
                        if (existingUserCount > 0)
                        {
                            result.Message = "User with same username or email already exits ";
                            return result;
                        }
                    }

                    string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";
                    if (!Regex.IsMatch(userData.User_email, emailPattern))
                    {
                        result.Message = "Incorrect Email format";
                        return result;
                    }

                    string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";
                    if (Regex.IsMatch(userData.User_password, passwordPattern))
                    {
                        result.Message = "Incorrect Password format";
                        return result;
                    }

                    // Adding user into Database
                    StringBuilder sql = new StringBuilder();
                    sql.Append("INSERT INTO UserDB(username,email,password,followers) ");
                    sql.Append("VALUES(@name,@email,@password,0);");
                    using (var cmd = new SqlCommand(sql.ToString(), con))
                    {
                        string passwordHash = BCrypt.Net.BCrypt.HashPassword(userData.User_password);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@name", userData.User_name);
                        cmd.Parameters.AddWithValue("@email", userData.User_email);
                        cmd.Parameters.AddWithValue("@password", passwordHash);
                
                        cmd.ExecuteNonQuery();
                        result.Status = true;
                        result.Message = "User Registered Successfully";
                        return result; 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message);
            }
        }


        public async Task<ResponseDto> UserLogin(UserLoginDto userData)
        {
            ResponseDto result = new()
            {
                Status = false,
                Message = "",
                Data = ""
            };
            try
            {
                using (var con = new SqlConnection(Db.GetConnectionString()))
                {
                    // Checking if the userExists or Not
                    var sql = "SELECT USER_ID,PASSWORD,USERNAME FROM UserDB WHERE EMAIL = @email ";
                    await con.OpenAsync();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@email", userData.User_email);
                        var sr = await cmd.ExecuteReaderAsync();
                        if (sr.HasRows)
                        {
                            while (sr.Read())
                            {
                                string hashedPassword = sr["password"].ToString();
                                bool verified = BCrypt.Net.BCrypt.Verify(userData.User_password, hashedPassword);
                                if (!verified)
                                {
                                    result.Message = "Password is incorrect";
                                    return result;
                                }

                                // generate token ...
                                var token = GenerateToken(sr["user_id"].ToString(), sr["username"].ToString());
                                result.Status = true;
                                result.Message = "Logged in Successfully";
                                result.Data = token;
                                return result;
                            }
                        }
                
                            result.Message = "User not found ";
                            return result;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResponseDto> FollowUser(int userId,int followingUserId)
        {
            ResponseDto result = new()
            {
                Status = false,
                Message = ""
            };
            try
            {
                if(userId == followingUserId)
                {
                    result.Message = "Bad Request";
                    return result;
                }

                using(var con = new SqlConnection(Db.GetConnectionString()))
                {
                    await con.OpenAsync();

                    // Cheking if the user exists or not 
                    var sqlCheck = "SELECT COUNT(*) FROM UserDB WHERE user_id = @userId ;";
                    using (var cmd = new SqlCommand(sqlCheck, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        int existingUserCount = (int)await cmd.ExecuteScalarAsync();
                        if (existingUserCount < 0)
                        {
                            result.Message = "User not found ";
                            return result;
                        }
                    }

                    // Checking if user is already followed by this account or not
                    string sql = "SELECT COUNT(*) FROM FOLLOW WHERE FOLLOWUSER_ID = @userId And FOLLOWINGUSER_ID = @followingUserId";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@followingUserId", followingUserId);
                        int alreadyFollowed = (int)await cmd.ExecuteScalarAsync();
                        if (alreadyFollowed > 0)
                        {
                            result.Status = true;
                            result.Message = "Already followed by this account";
                            return result;
                        }
                    }

                    //Following user
                    DateTime today = DateTime.Now;
                    sql = "INSERT INTO FOLLOW(FOLLOWUSER_ID,FOLLOWINGUSER_ID,FOLLOWED_AT) VALUES (@userId,@followingUserId,@followedAt)";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@followingUserId", followingUserId);
                        cmd.Parameters.AddWithValue("@followedAt", today);
                        cmd.ExecuteNonQuery();
                        
                    }


                    sql = "UPDATE UserDB SET followers=followers+1  WHERE USER_ID = @userId";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                        result.Status = true;
                        result.Message = "User followed Successfully";
                        return result;
                    }

                }


            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> getTopFollow()
        {
            List<string> result = new List<string>();
            try
            {

                using (var con = new SqlConnection(Db.GetConnectionString()))
                {
                    await con.OpenAsync();

                    string sql = "SELECT username FROM USERDB ORDER BY FOLLOWERS ;";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        string temp;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                temp = reader.GetString(reader.GetOrdinal("username"));
                                result.Add(temp);
                            }
                            return result;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string GenerateToken(string ID,string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
            issuer: Config["Jwt:Issuer"],
            audience: Config["Jwt:Issuer"],
            claims: new[] {
                new Claim(ClaimTypes.UserData, ID),
                new Claim(ClaimTypes.Name,username)
            },
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: credentials
            ); 

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

