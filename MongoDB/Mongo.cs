using System.IO;
using MongoDB.Driver;

namespace IngoBot.MongoDB;

public static class Mongo
{
    private static string ConnectionString = File.ReadAllText("config/CONNECTIONSTRING");

    public static MongoClient Conn { get; } = new(ConnectionString);
    public static IMongoDatabase Database { get; } = Conn.GetDatabase("ingobot");
}
