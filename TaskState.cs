namespace SqliteToCsv
{
    internal class TaskState
    {
        public TaskState()
        {
            Status = State.Initilizing;
        }

        public TaskState(int id)
        {
            Id = id;
            Status = State.Initilizing;
        }

        public State Status { get; set; }
        public int Id { get; set; }
    }

    internal enum State
    {
        None = 0,
        Initilizing = 1,
        Running = 2,
        Error = 3,
        Paused = 4,
        Complete = 5
    }
}