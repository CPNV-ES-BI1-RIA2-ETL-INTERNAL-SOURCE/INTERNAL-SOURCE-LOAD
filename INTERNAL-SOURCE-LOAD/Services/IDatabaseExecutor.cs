namespace INTERNAL_SOURCE_LOAD.Services
{
    public interface IDatabaseExecutor
    {
        void Execute(string connectionString);
        long ExecuteAndReturnId(string connectionString);
    }

}
