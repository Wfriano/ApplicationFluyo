namespace FluyoV2.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string UsersCollectionName { get; set; } = string.Empty;
        public string AccountsCollectionName { get; set; } = string.Empty;
        public string TransactionsCollectionName { get; set; } = string.Empty;
        public string GoalsCollectionName { get; set; } = string.Empty;

    }
}
