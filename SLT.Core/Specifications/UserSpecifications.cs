using SLT.Core.Entities;

namespace SLT.Core.Specifications;

public class UserByEmailSpec : BaseSpecification<User>
{
    public UserByEmailSpec(string email)
        : base(u => u.Email == email.ToLower()) { }
}

public class UserEmailExistsSpec : BaseSpecification<User>
{
    public UserEmailExistsSpec(string email)
        : base(u => u.Email == email.ToLower()) { }
}