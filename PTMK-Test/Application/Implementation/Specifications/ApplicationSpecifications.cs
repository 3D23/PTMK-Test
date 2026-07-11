using PTMK_Test.Application.Interface;
using PTMK_Test.Core.Implementation.Enums;
using System.Linq.Expressions;

namespace PTMK_Test.Application.Implementation.Specifications.Application
{
    public sealed class StatusSpecification(ApplicationStatusType status)
        : IApplicationSpecification
    {
        public Expression<Func<Core.Implementation.Models.Application, bool>> ToExpression()
            => app => app.Status.StatusType == status;
    }

    public sealed class OverdueSpecification(bool isOverdue)
        : IApplicationSpecification
    {
        public Expression<Func<Core.Implementation.Models.Application, bool>> ToExpression()
        {
            var now = DateTime.UtcNow;
            return isOverdue
                ? app => app.Deadline < now && app.Status.StatusType != ApplicationStatusType.Completed
                : app => app.Deadline >= now || app.Status.StatusType == ApplicationStatusType.Completed;
        }
    }

    public sealed class ExecutorSpecification(Guid executorId)
        : IApplicationSpecification
    {
        public Expression<Func<Core.Implementation.Models.Application, bool>> ToExpression()
            => app => app.ExecutorId == executorId;
    }
}
