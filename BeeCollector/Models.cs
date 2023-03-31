

using FreeSql.DataAnnotations;
using static FreeSql.Internal.GlobalFilter;

[Index(nameof(Telemetry) + "_" + nameof(Timestamp) + "Index", nameof(Timestamp))]
[Index(nameof(Telemetry) + "_" + nameof(Hive) + "Index", nameof(Hive))]
[Index(nameof(Telemetry) + "_" + nameof(Sensor) + "Index", nameof(Sensor))]
[Index(nameof(Telemetry) + "_" + nameof(Type) + "Index", nameof(Type))]
public class Telemetry
{
	[Column(IsIdentity = true, IsPrimary = true, IsNullable = false)]
	public int Id { get; set; }
	public string Hive { get; set; }
	public string Sensor { get; set; }
	public string Type { get; set; }
	public double Value { get; set; }
	public DateTime Timestamp { get; set; }

}