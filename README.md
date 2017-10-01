# GDataDB

Forked from: https://github.com/mausch/GDataDB.

GDataDB is a database-like interface to Google Spreadsheets for .Net.<br/>
It models spreadsheets as databases, worksheets as database tables, and rows as records.

How to get started with OAuth2:
1. Visit http://console.developers.google.com with your google account.
2. Create a new project (Example Project), enable the Drive API.
3. Create a new client ID of type "service account" (example@exampleproject.gserviceaccount.com) and download the P12 key.
4. Name that P12 whatever you want and change extension to .bytes (ExampleKey.bytes) and put it in Resources
5. To access a spreadsheet (Example Database), share it with the service account email (example@exampleproject.gserviceaccount.com).
```csharp
// Spreadsheet looks like:
// | Enemy Name  |  Hit Points |
// | Slime       |  10         |
// | Orc         |  200        |
public class EnemyRowData {
	// NOTE (darren): must be properties due to GDataDB code
	// NOTE (darren): these property names must match the first row
	// of the spread sheet (Enemy Name -> EnemyName)
	public string EnemyName { get; set; }
	public string HitPoints { get; set; }
}

public static class DatabaseExample {
	// Will run on application start
	[RuntimeInitializeOnLoadMethod]
	private static void Initialize() {
		byte[] keyBytes = (Resources.Load("ExampleKey") as TextAsset).bytes;
		IDatabaseClient client = new DatabaseClient("example@exampleproject.gserviceaccount.com", keyBytes);
		IDatabase database = client.GetDatabase("Example Database");
		ITable<EnemyRowData> enemyDataTable = database.GetTable<EnemyRowData>("Enemy Data");
		foreach (IRow<EnemyRowData> enemyRow in enemyDataTable.FindAll()) {
			EnemyRowData enemyData = enemyRow.Element;
			... // do something with the data
		}
	}
}
```


## Changelog
* Added Newtonsoft Unity package
* Removed Nuget scaffolding to allow direct placement into project / submodule-ing
* Documentation
