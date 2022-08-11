namespace SpendPoint
{
    public class StatusViewModel
    {
        public StatusViewModel(Status status = Status.Failed)
        {
            if (status == Status.Success)
            {
                StatusCode = Status.Success;
                StatusMessage = "Success";
            }
            else
            {
                StatusCode = Status.Failed;
                StatusMessage = "Failed";
            }
        }

        public Status StatusCode { get; set; }

        public string StatusMessage { get; set; }

        public int EntityId { get; set; }

        public int NextIndexId { get; set; }
    }

    public enum Status
    {
        Success = 0,
        Failed = 1,
        Warning = 2,
    }
}
