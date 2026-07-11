using PTMK_Test.Core.Implementation.VO;

namespace PTMK_Test.Core.Implementation.Models
{
    public class Application
    {
        public Guid ID => _id;
        public string? Number => _number;
        public DateTime CreatedAt => _createdAt;
        public Guid AuthorId => _authorId;
        public Guid ExecutorId => _executorId;
        public string? Description => _description;
        public DateTime Deadline => _deadline;
        public ApplicationStatus Status => _status;

        private readonly Guid _id;
        private string? _number;
        private DateTime _createdAt;
        private Guid _authorId;
        private Guid _executorId;
        private string? _description;
        private DateTime _deadline;
        private ApplicationStatus _status;

        private Application() { }

        public Application(
            string number,
            DateTime createdAt,
            Guid authorId, 
            Guid executorId,
            DateTime deadline,
            string description = "")
        {
            if (deadline <= createdAt)
                throw new ArgumentException(
                    $"{nameof(Application)} Срок выполнения ({deadline:dd.MM.yyyy HH:mm}) должен быть больше даты создания ({createdAt:dd.MM.yyyy HH:mm})",
                    nameof(deadline));

            _id = Guid.NewGuid();
            _status = new();
            _number = number;
            _createdAt = createdAt;
            _authorId = authorId;
            _executorId = executorId;
            _deadline = deadline;
            _description = description;
        }

        public bool TrySetInProgress()
        {
            var currentStatus = _status;
            bool isTransitionValid = currentStatus.TrySetInProgress();

            if (isTransitionValid)
                _status = currentStatus;

            return isTransitionValid;
        }

        public bool TrySetCompleted()
        {
            var currentStatus = _status;
            bool isTransitionValid = currentStatus.TrySetCompleted();

            if (isTransitionValid)
                _status = currentStatus;

            return isTransitionValid;
        }

        public void ChangeExecutor(Guid newExecutorId) =>
            _executorId = newExecutorId;
    }
}
