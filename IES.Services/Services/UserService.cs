using FastMapper;
using IES.Domain.Entities.Models;
using IES.Domain.Interfaces.UnitOfWork;
using IES.Services.DTO.DTO;
using IES.Services.Interfaces.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IES.Services.Services
{
    public class UserService : IUserService {

   public IUnitOfWork unitOfWork;
    public UserService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }
    public async Task<object> SignIn(UserDTO userDTO)
    {
        return await GetToken(userDTO);
    }

    public async Task<object> GetToken(UserDTO userDTO)
    {
        User user = await unitOfWork.IdentityUserManager.FindByEmailAsync(userDTO.Email);
        var roles = await unitOfWork.IdentityUserManager.GetRolesAsync(user);
        if (user is not null && await unitOfWork.IdentityUserManager.CheckPasswordAsync(user, userDTO.Password))
        {
            IdentityOptions options = new IdentityOptions();
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim("UserId", user.Id.ToString()),
                        new Claim("UserName", user.UserName.ToString()),
                        new Claim("FirstName", user.FirstName.ToString()),
                        new Claim("LastName", user.LastName.ToString())
                }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890123456")), SecurityAlgorithms.HmacSha256Signature)
            };
            foreach (var role in roles)
            {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            }
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);
            return new { token };
        }
        else
        {
            throw new Exception();
        }

    }

    public async Task<object> SignUp(UserDTO userDTO)
    {
        User user = new User
        {
            Email = userDTO.Email,
            FirstName = userDTO.FirstName,
            LastName = userDTO.LastName,
            UserName = userDTO.FirstName + "." + userDTO.LastName
        };
        await unitOfWork.IdentityUserManager.CreateAsync(user, userDTO.Password);
        unitOfWork.Commit();
        return await GetToken(userDTO);
    }

    public async Task<String> ChangePassword(string email, string oldPassword, string newPassword)
    {
        User user = await unitOfWork.IdentityUserManager.FindByEmailAsync(email);

        if (user == null)
        {
            return "User with this email was not found";
        }

        IdentityResult result = await unitOfWork.IdentityUserManager.ChangePasswordAsync(user, oldPassword, newPassword);

        if (result.Succeeded)
        {
            return "Password has been changed";
        }
        else
        {
            return "Error occured";
        }
    }
        public IEnumerable<UserDTO> GetAll()
        {
            return TypeAdapter.Adapt<IEnumerable<UserDTO>>(unitOfWork.Users.GetAll());
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync()
        {
            var users = TypeAdapter.Adapt<IEnumerable<UserDTO>>(await unitOfWork.Users.GetAllAsync());
            return users;
        }
        public async Task<string> DeleteUser(string userId)
        {
            var user = await unitOfWork.IdentityUserManager.FindByIdAsync(userId);
            await unitOfWork.IdentityUserManager.DeleteAsync(user);
            unitOfWork.Commit();
            return "User " + user.FirstName.ToString() + " deleted";
        }

        //public void Update(UserDTO userDTO)
        //{
        //    unitOfWork.IdentityUserManager.UpdateAsync(TypeAdapter.Adapt<UserGroup>(userGroupDTO));
        //}
    }
}

//public class UserGroupService : IUserGroupService
//{
//    public IUnitOfWork unitOfWork;
//    public UserGroupService(IUnitOfWork unitOfWork)
//    {
//        this.unitOfWork = unitOfWork;
//    }

//    public async Task<string> AddUserToGroup(string userId, int userGroupId)
//    {
//        //var existingBlog = new Blog { BlogId = 1, Name = "ADO.NET Blog" };

//        //using (var context = new BloggingContext())
//        //{
//        //    context.Entry(existingBlog).State = EntityState.Modified;

//        //    // Do some more work...  

//        //    context.SaveChanges();
//        //}

//        User user = await unitOfWork.IdentityUserManager.FindByIdAsync(userId);
//        if (user != null)
//        {
//            user.UserGroupId = userGroupId;
//            await unitOfWork.IdentityUserManager.UpdateAsync(user);
//            unitOfWork.Commit();
//        }
//        return "User was succsesfully added to group";
//    }

//    public void Create(UserGroupCreateDTO userGroupDTO)
//    {
//        unitOfWork.UserGroups.Create(TypeAdapter.Adapt<UserGroup>(userGroupDTO));
//        unitOfWork.Commit();
//    }

//    public IEnumerable<UserGroupDTO> GetAll()
//    {
//        return TypeAdapter.Adapt<IEnumerable<UserGroupDTO>>(unitOfWork.UserGroups.GetAll());
//    }

//    public async Task<IEnumerable<UserGroupDTO>> GetAllAsync()
//    {
//        var users = TypeAdapter.Adapt<IEnumerable<UserGroupDTO>>(await unitOfWork.UserGroups.GetAllAsync());
//        return users;
//    }

//    public UserGroupDTO GetById(int id)
//    {
//        return TypeAdapter.Adapt<UserGroupDTO>(unitOfWork.UserGroups.GetById(id));
//    }

//    public async Task<UserGroupDTO> GetByIdAsync(int id)
//    {
//        return TypeAdapter.Adapt<UserGroupDTO>(await unitOfWork.UserGroups.GetByIdAsync(id));
//    }

//    public UserGroupDTO GetGroupWithUsersById(int id)
//    {
//        List<User> users = unitOfWork.Users.GetAll().Where(x => x.UserGroupId == id).ToList();
//        var userDTO = TypeAdapter.Adapt<List<UserDTO>>(users);
//        UserGroupDTO userGroup = TypeAdapter.Adapt<UserGroupDTO>(unitOfWork.UserGroups.GetById(id));
//        userGroup.Users = userDTO;
//        return userGroup;
//    }


//    public void Remove(UserGroupDTO userGroupDTO)
//    {
//        unitOfWork.UserGroups.Remove(TypeAdapter.Adapt<UserGroup>(userGroupDTO));
//    }

//    public void Update(UserGroupDTO userGroupDTO)
//    {
//        unitOfWork.UserGroups.Update(TypeAdapter.Adapt<UserGroup>(userGroupDTO));
//    }
//}