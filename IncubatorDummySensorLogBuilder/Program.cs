using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ConsoleApp1
{
  class Program
  {
    static readonly string _dateFormat = "yyyy-MM-dd hh:mm:ss";
    static readonly string _floatingFormat = "0.0000000000000";
    static readonly List<int> _periods = new List<int> { 1, 15, 60, 360, 1440 };
    static readonly DateTime _now = DateTime.Now;
    static readonly string _filepath = "d:\\insert.sql";
    static readonly int _rowCount = 1000;

    static void Main(string[] args)
    {
      List<string> temperatureTables = new List<string> { "TemperatureP1", "TemperatureP15", "TemperatureP60", "TemperatureP360", "TemperatureP1440" };
      List<string> humidityTables = new List<string> { "HumidityP1", "HumidityP15", "HumidityP60", "HumidityP360", "HumidityP1440" };
      List<string> co2Tables = new List<string> { "Co2P1", "Co2P15", "Co2P60", "Co2P360", "Co2P1440" };

      Dictionary<int, List<int>> temperatureDefinitionMap = new Dictionary<int, List<int>>();
      Dictionary<int, List<int>> humidityDefinitionMap = new Dictionary<int, List<int>>();
      Dictionary<int, List<int>> co2DefinitionMap = new Dictionary<int, List<int>>();

      temperatureDefinitionMap[0] = new List<int> { 100, 200, 300 };
      temperatureDefinitionMap[1] = new List<int> { 100, 200, 300 };
      temperatureDefinitionMap[2] = new List<int> { 100, 200, 300 };

      humidityDefinitionMap[0] = new List<int> { 100, 200, 300 };

      co2DefinitionMap[0] = new List<int> { 100, 200, 300 };

      StringBuilder queries = new StringBuilder();

      queries.Append(BuildDeleteQuery(temperatureTables));
      queries.Append(BuildDeleteQuery(humidityTables));
      queries.Append(BuildDeleteQuery(co2Tables));
      queries.Append(BuildQuery(temperatureTables, temperatureDefinitionMap, _rowCount, 25, 40));
      queries.Append(BuildQuery(humidityTables, humidityDefinitionMap, _rowCount, 0.1, 0.9));
      queries.Append(BuildQuery(co2Tables, co2DefinitionMap, _rowCount, 0.01, 0.2));

      string queryString = queries.ToString();

      File.WriteAllText(_filepath, queryString);
    }

    static string BuildQuery(List<string> tables, Dictionary<int, List<int>> tableDefinitionMap, int length, double min, double max)
    {
      StringBuilder queries = new StringBuilder();
      Random rnd = new Random();

      for (int i = 0; i < tables.Count; i++)
      {
        string tableName = tables[i];
        int period = _periods[i];

        DateTime date = new DateTime(_now.Year, _now.Month, _now.Day, _now.Hour, _now.Minute, _now.Second);

        for (int j = 0; j < length; j++)
        {
          foreach (KeyValuePair<int, List<int>> kvp in tableDefinitionMap)
          {
            int sensorId = kvp.Key;
            List<int> sensorTypeIds = kvp.Value;

            foreach (int sensorTypeId in sensorTypeIds)
            {
              double mean = min + ((max - min) * rnd.NextDouble());

              string query =
                string.Format("INSERT INTO dbo.\"{0}\"(\"SensorId\", \"Type_Id\", \"Mean\", \"Min\", \"Max\", \"CreatedTime\", \"ModifiedTime\", \"IsDeleted\")	VALUES('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}');",
                tableName,
                sensorId,
                sensorTypeId,
                mean.ToString(_floatingFormat, CultureInfo.InvariantCulture),
                min.ToString(_floatingFormat, CultureInfo.InvariantCulture),
                max.ToString(_floatingFormat, CultureInfo.InvariantCulture),
                date.ToString(_dateFormat),
                date.ToString(_dateFormat), "false");

              queries.AppendLine(query);
            }
          }

          date = date.AddMinutes(period * -1);
        }

        queries.AppendLine();
      }

      return queries.ToString();
    }

    static string BuildDeleteQuery(List<string> tables)
    {
      StringBuilder queries = new StringBuilder();

      for (int i = 0; i < tables.Count; i++)
      {
        string tableName = tables[i];

        string query =
          string.Format("DELETE FROM dbo.\"{0}\";",
          tableName);

        queries.AppendLine(query);
      }

      queries.AppendLine();

      return queries.ToString();
    }
  }
}
