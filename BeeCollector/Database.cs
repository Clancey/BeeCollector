using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.ApplicationInsights.Channel;
public class Database
{
	HttpClient httpClient = new HttpClient();
	public static Database Shared { get; set; } = new Database();

	const string axiomDataset = "bees";
	public Database()
	{
		httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiConstants.AxiumAuthKey}");
		var fsqlb = new FreeSql.FreeSqlBuilder()
				.UseConnectionString(FreeSql.DataType.SqlServer, ApiConstants.DatabaseString);

		fsqlb = fsqlb.UseAutoSyncStructure(true); //automatically synchronize the entity structure to the database
		db = fsqlb.Build();
		var models = new[]{
			typeof(Telemetry),
			};
		db.CodeFirst?.SyncStructure(entityTypes: models);
	}
	IFreeSql db;
	DebounceDispatcher axiomDebounce = new DebounceDispatcher(1000);
	DebounceDispatcher dbDebounce = new DebounceDispatcher(1000);
	List<Telemetry> axiomTelemetry = new List<Telemetry>();
	List<Telemetry> dbTelemetry = new List<Telemetry>();
	public void ProcessMessage(string topic, string payload)
	{
		try
		{
			Console.WriteLine($"Topic: {topic}, Payload: {payload}");
			Logger.SharedLogger.LogInformation($"Topic: {topic}, Payload: {payload}");
			var parts = topic.Split("/");
			var hive = parts[0];
			var sensor = parts[1];
			var type = parts[2];
			var value = payload;
			var t = new Telemetry()
			{
				Hive = hive,
				Sensor = sensor,
				Type = type,
				Value = double.Parse(value),
				Timestamp = DateTime.Now
			};
			axiomTelemetry.Add(t);
			Console.WriteLine($"Calling Debouncer for Axiom: {axiomTelemetry.Count}");
			axiomDebounce.Debounce(() =>
			{
				UploadDataToAxiom();
			});
			if (hive == "TestHive")
				return;
			dbTelemetry.Add(t);
			Console.WriteLine($"Calling Debouncer for Database: {dbTelemetry.Count}");
			dbDebounce.Debounce(() =>
			{
				UploadDataToDatabase();
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
	}
	Task axiomTask;
	Task UploadDataToAxiom()
	{
		if (axiomTask?.IsCompleted ?? true)
			axiomTask = uploadDataToAxiom();
		return axiomTask;
	}
	async Task uploadDataToAxiom()
	{
		Console.WriteLine("Starting upload to Axiom");
		var queue = axiomTelemetry.ToList();
		axiomTelemetry.Clear();
		try
		{

			var updateData = queue.Select(x => new
			{
				Hive = x.Hive,
				x.Sensor,
				x.Type,
				x.Value,
				_time = x.Timestamp,
			}).ToList();
			var json = JsonSerializer.Serialize(updateData);
			var message = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
			var r = await httpClient.PostAsync($"https://api.axiom.co/v1/datasets/{axiomDataset}/ingest", message);
			Console.WriteLine($"Sent to Axiom:{r.IsSuccessStatusCode}");
		}
		catch (Exception ex)
		{
			Logger.SharedLogger.LogError(ex, "Error submitting data to Axiom");
			axiomTelemetry.InsertRange(0, queue);
		}
	}
	Task databaseTask;

	Task UploadDataToDatabase()
	{
		if (databaseTask?.IsCompleted ?? true)
			databaseTask = uploadDataToDatabase();
		return databaseTask;
	}
	async Task uploadDataToDatabase()
	{
		var queue = dbTelemetry.ToArray();
		dbTelemetry.Clear();
		try
		{
			var r = await db.Insert<Telemetry>(queue).ExecuteAffrowsAsync();
			Console.WriteLine($"Lines inserted: {r}");
		}
		catch (Exception ex) {
			dbTelemetry.InsertRange(0, queue);
			Logger.SharedLogger.LogError(ex, "Error Inserting data to the database");
		}
	}

}