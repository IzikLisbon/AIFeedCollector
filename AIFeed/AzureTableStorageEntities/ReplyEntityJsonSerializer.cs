using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace AIFeed.AzureTableStorageEntities
{
    public class ReplyEntitiesJsonSerializer
    {
        public static string Serialize(List<ReplyEntity> replies)
        {
            var stream = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(List<ReplyEntity>));
            ser.WriteObject(stream, replies);
            stream.Position = 0;
            var streamReader = new StreamReader(stream);
            string replyAsJson = streamReader.ReadToEnd();
            return replyAsJson;
        }

        public static List<ReplyEntity> Deserialize(string repliesAsJson)
        {
            if (string.IsNullOrEmpty(repliesAsJson))
            {
                return new List<ReplyEntity>();
            }

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(repliesAsJson));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<ReplyEntity>));
            List<ReplyEntity> replies = (List<ReplyEntity>)ser.ReadObject(stream);

            return replies;
        }
    }
}
