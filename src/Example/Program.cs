
using Upgradier.Core;
using Upgradier.SqlServer;

const string ENVIRONMENT = "dev";

UpdateBuilder builder = new UpdateBuilder()
    //.WithFileBatchAdapter("")
    .AddSqlServerEngine(ENVIRONMENT);

using UpdateManager updateManager = builder.Build();
IEnumerable<UpdateResult> updateResults = await updateManager.Update();