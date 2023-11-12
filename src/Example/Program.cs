EnvironmentVariables.SetExecutionEnvironment(EnvironmentVariables.UPGRADIER_ENV_DEV);

UpdateBuilder updateBuilder = new UpdateBuilder()
    .WithFileBatchStrategy("Batches")
    .WithSourceProvider(() => new FileSourceProvider("Sources.json"))
    .AddSqlServerEngine()
    .AddMySqlServerEngine()
    .AddPostgreSqlServerEngine();

UpdateManager updateManager = updateBuilder.Build();
IEnumerable<UpdateResult> updateResults = await updateManager.Update();

foreach (UpdateResult result in updateResults)
{
    Console.WriteLine(result);
}