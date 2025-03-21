using System.Text.Json;
using INTERNAL_SOURCE_LOAD.Models;

namespace INTERNAL_SOURCE_LOAD_TEST.TestData
{
    public class TrainStationTestData : TestDataFixture
    {
        public static JsonElement GetValidTrainStationJson()
        {
            return ParseJson(@"
                {
                    ""Name"": ""Berlin Hbf"",
                    ""Departures"": [
                        {
                            ""DepartureStationName"": ""Berlin Hbf"",
                            ""DestinationStationName"": ""Munich Hbf"",
                            ""DepartureTime"": ""2024-03-07T12:00:00"",
                            ""Platform"": ""1"",
                            ""Train"": {
                                ""G"": ""ICE"",
                                ""L"": ""123""
                            }
                        }
                    ]
                }");
        }

        public static JsonElement GetNewTrainStationJson()
        {
            return ParseJson(@"
                {
                    ""Name"": ""New Station"",
                    ""Departures"": []
                }");
        }

        public static JsonElement GetEmptyTrainStationJson()
        {
            return ParseJson(@"
                {
                    ""Name"": ""Berlin Hbf"",
                    ""Departures"": []
                }");
        }

        public static (TrainStation Station, Train Train, Departure Departure) GetSimpleTrainStation()
        {
            var train = new Train("ICE", "DB123");
            var departure = new Departure(
                DepartureStationName: "Berlin",
                DestinationStationName: "Munich",
                ViaStationNames: new List<string> { "Frankfurt" },
                DepartureTime: DateTime.Now,
                Train: train,
                Platform: "1",
                TrainStationID: null  // This should be updated by the foreign key update
            );
            var trainStation = new TrainStation("Berlin Hbf", new List<Departure> { departure });
            return (trainStation, train, departure);
        }

        public static TrainStation GetComplexTrainStation()
        {
            var train1 = new Train("ICE", "DB123");
            var train2 = new Train("TGV", "FR456");
            var train3 = new Train("ICE", "DB789");

            var departure1 = new Departure(
                DepartureStationName: "Berlin Hbf",
                DestinationStationName: "Munich Hbf",
                ViaStationNames: new List<string> { "Frankfurt" },
                DepartureTime: DateTime.Now,
                Train: train1,
                Platform: "1"
            );

            var departure2 = new Departure(
                DepartureStationName: "Paris",
                DestinationStationName: "Lyon",
                ViaStationNames: new List<string> { "Dijon" },
                DepartureTime: DateTime.Now,
                Train: train2,
                Platform: "2"
            );

            var departure3 = new Departure(
                DepartureStationName: "Berlin",
                DestinationStationName: "Paris",
                ViaStationNames: new List<string> { "Frankfurt" },
                DepartureTime: DateTime.Now,
                Train: train3,
                Platform: "1"
            );

            return new TrainStation("Berlin Hbf", new List<Departure> { departure1, departure2, departure3 });
        }

        public static Dictionary<object, long> GetModelIds(TrainStation station)
        {
            var modelIds = new Dictionary<object, long>();
            var baseId = 42L;

            foreach (var departure in station.Departures)
            {
                modelIds[departure.Train] = baseId++;
                modelIds[departure] = baseId++;
            }

            modelIds[station] = baseId;
            return modelIds;
        }

        public static Dictionary<object, long> GetSimpleModelIds(Train train, Departure departure, TrainStation station)
        {
            return new Dictionary<object, long>
            {
                { train, 42 },      // Train ID
                { station, 55 },    // TrainStation ID
                { departure, 123 }   // Departure ID
            };
        }
    }
}