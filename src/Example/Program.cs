using Upgradier.Core;
using Upgradier.SqlServer;

const string ENVIRONMENT = "dev";

UpdateBuilder updateBuilder = new UpdateBuilder()
    .WithFileBatchAdapter("Batches", ENVIRONMENT)
    .WithSourceProvider(() => new FileSourceProvider("Sources.json", ENVIRONMENT))
    .AddSqlServerEngine(ENVIRONMENT);

using UpdateManager updateManager = updateBuilder.Build();
IEnumerable<UpdateResult> updateResults = await updateManager.Update();

foreach (UpdateResult result in updateResults)
{
    Console.WriteLine(result);
}