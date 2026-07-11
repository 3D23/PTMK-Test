using PTMK_Test.Core.Implementation.Enums;
using PTMK_Test.Core.Implementation.VO;

namespace PTMK_Test.Core.Implementation.Models
{
    public sealed class Employee
    {
        public Guid ID => _id;
        public Name? FullName => _fullName;
        public DepartmentType Department => _department;
        public PositionType Position => _position;

        private readonly Guid _id;
        private Name? _fullName;
        private DepartmentType _department;
        private PositionType _position;

        private Employee() { }

        public Employee(
            Name fullName,
            DepartmentType department,
            PositionType position)
        {
            _id = Guid.NewGuid();
            _fullName = fullName;
            _department = department;
            _position = position;
        }
    }
}
