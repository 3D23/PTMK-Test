using System.Linq.Expressions;

namespace PTMK_Test.Application.Interface
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ToExpression();
    }

    public interface IApplicationSpecification : ISpecification<Core.Implementation.Models.Application> { }
    public interface IEmployeeSpecification : ISpecification<Core.Implementation.Models.Employee> { }
}
