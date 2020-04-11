namespace GameServer.Serializer
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    public static class DataSerializer
    {
        public static T ReadSerializedData<T>(Stream data)
            where T : class
        {
            data.Seek(0, SeekOrigin.Begin);
            var xmlSerializer = new XmlSerializer(typeof(T));
            try
            {
                var deserializedData = xmlSerializer.Deserialize(data) as T;
                if (deserializedData is null)
                {
                    Console.WriteLine("Invalid data.");
                }

                return deserializedData;
            }
            catch
            {
                Console.WriteLine("Invalid data.");
                return null;
            }
        }

        public static Stream CreateSerializedData<T>(T data)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var dataAsStream = new MemoryStream();

            try
            {
                xmlSerializer.Serialize(dataAsStream, data);
                return dataAsStream;
            }
            catch
            {
                Console.WriteLine("Invalid data.");
                return null;
            }
        }
    }
}
