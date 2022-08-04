using FastMapper;
using IES.Domain.Entities.Models;
using IES.Domain.Interfaces.UnitOfWork;
using IES.Services.DTO.DTO;
using IES.Services.Interfaces.Interfaces;
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
    public class DealService : IDealService
    {
        public IUnitOfWork unitOfWork;
        public DealService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void Create(DealCreateDTO dealDTO)
        {

            var price = unitOfWork.Companys.GetById((int)dealDTO.CompanyId).StockPrice;
            var count = dealDTO.CountOfStockDeal;

            if (dealDTO.ResultPrice == 0)
            {
                dealDTO.ResultPrice =  count * price;
            }

            unitOfWork.Deals.Create(TypeAdapter.Adapt<Deal>(dealDTO));
            unitOfWork.Commit();
        }

        public int GetPrice(int id)
        {
            var Deal = unitOfWork.Deals.GetById(id);
            return Deal.CountOfStockDeal * Deal.Company.StockPrice;
        }

        public IEnumerable<DealDTO> GetAll()
        {
            return TypeAdapter.Adapt<IEnumerable<DealDTO>>(unitOfWork.Deals.GetAll());
        }

        //public IEnumerable<DealDTO> GetDealsById(int Id)
        //{
        //    List<Deal> deals = unitOfWork.Deals.
        //    return Map(statistics);

        //    var deals = TypeAdapter.Adapt<IEnumerable<DealDTO>>(await unitOfWork.Deals.Get());
        //    return deals;
        //}

        public async Task<IEnumerable<DealDTO>> GetAllAsync()
        {
            var deals = TypeAdapter.Adapt<IEnumerable<DealDTO>>(await unitOfWork.Deals.GetAllAsync());
            return deals;
        }

        public List<Deal> GetByUserId(string Id)
        {
            var alldeal = unitOfWork.Deals.GetAll().Where(x=>x.UserId==Id);
            return alldeal.ToList();
        }

        public void AddDealToUserList(int dealId, string userId)
        {
            var alldeal = GetByUserId(userId);
            alldeal.Add(unitOfWork.Deals.GetById(dealId));
        }

        public void UpdateUserDealsList(int dealId)
        {
            DealDTO dealDTO = TypeAdapter.Adapt <DealDTO> (unitOfWork.Deals.GetById(dealId));
            List<Deal> main = GetByUserId(dealDTO.UserId);
            dealDTO.User.Deals = main;

            unitOfWork.Users.Update(dealDTO.User);
            unitOfWork.Commit();
        }

        public bool Update(DealDTO dealDTO)
        {
            unitOfWork.Deals.Update(TypeAdapter.Adapt<DealDTO, Deal>(dealDTO));
            return unitOfWork.Commit() > 0;
        }

        public List<Deal> GetByCompanyId(int Id)
        {
            var alldeal = unitOfWork.Deals.GetAll().Where(x => x.CompanyId == Id);
            return alldeal.ToList();
        }

        public async Task<bool> ConfirmDeal(int dealId)
        {
            DealDTO dealDTO = TypeAdapter.Adapt<DealDTO>(unitOfWork.Deals.GetById(dealId));
            UserDTO user = TypeAdapter.Adapt<UserDTO>(await unitOfWork.IdentityUserManager.FindByIdAsync(dealDTO.UserId));
            CompanyDTO company = TypeAdapter.Adapt<CompanyDTO>(unitOfWork.Companys.GetById((int)dealDTO.CompanyId));

            if ((dealDTO.ResultPrice - user.Resources) >= 0)
            {
                company.CountOfStock -= dealDTO.CountOfStockDeal;
                company.Resources += dealDTO.ResultPrice;

                dealDTO.IsSuccsesful = true;  

                user.Resources = company.CountOfStock -= dealDTO.ResultPrice;

                unitOfWork.Companys.Update(TypeAdapter.Adapt<Company>(company));
                unitOfWork.Deals.Update(TypeAdapter.Adapt<Deal>(dealDTO));
                await unitOfWork.IdentityUserManager.UpdateAsync(TypeAdapter.Adapt<User>(user));
                return true;
            }
            else
            return false;
        }
    }
}

//public QuizDTO GetById(int id)
//{
//    Quiz quiz = unitOfWork.Quizes.GetQuiz(id);
//    TypeAdapterConfig<Quiz, QuizDTO>.NewConfig().MaxDepth(3);
//    return TypeAdapter.Adapt<QuizDTO>(quiz);
//}

//public class CourseService : ICourseService
//{
//    public IUnitOfWork unitOfWork;
//    public CourseService(IUnitOfWork unitOfWork)
//    {
//        this.unitOfWork = unitOfWork;
//    }
//    public void Create(CourseDTO course)
//    {
//        unitOfWork.Courses.Create(TypeAdapter.Adapt<Course>(course));
//    }

//    public IEnumerable<CourseDTO> GetAll()
//    {
//        throw new NotImplementedException();
//    }

//    public Task<IEnumerable<CourseDTO>> GetAllAsync()
//    {
//        throw new NotImplementedException();
//    }

//    public CourseDTO GetById(int id)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<CourseDTO> GetByIdAsync(int id)
//    {
//        throw new NotImplementedException();
//    }

//    public void Remove(CourseDTO course)
//    {
//        throw new NotImplementedException();
//    }

//    public void Update(CourseDTO course)
//    {
//        throw new NotImplementedException();
//    }
//}





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