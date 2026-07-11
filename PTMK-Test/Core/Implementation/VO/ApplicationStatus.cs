using PTMK_Test.Core.Implementation.Enums;

namespace PTMK_Test.Core.Implementation.VO
{
    public record struct ApplicationStatus
    {
        public readonly ApplicationStatusType StatusType => _statusType;

        private ApplicationStatusType _statusType;

        public ApplicationStatus()
        {
            _statusType = ApplicationStatusType.New;
        }

        public bool TrySetCompleted()
        {
            if (_statusType == ApplicationStatusType.InProgress)
            {
                _statusType = ApplicationStatusType.Completed;
                return true;
            }

            return false;
        }

        public bool TrySetInProgress()
        {
            if (_statusType == ApplicationStatusType.New)
            {
                _statusType = ApplicationStatusType.InProgress;
                return true;
            }

            return false;
        }

        public bool TryMoveNext()
        {
            return _statusType switch
            {
                ApplicationStatusType.New => TrySetInProgress(),
                ApplicationStatusType.InProgress => TrySetCompleted(),
                ApplicationStatusType.Completed => false,
                _ => false
            };
        }

        public static implicit operator ApplicationStatusType(ApplicationStatus status)
            => status._statusType;
    }
}
