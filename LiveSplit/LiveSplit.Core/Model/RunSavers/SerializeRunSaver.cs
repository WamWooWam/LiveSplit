using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LiveSplit.Model.RunSavers
{
    public class SerializeRunSaver : IRunSaver
    {
        public void Save(IRun run, Stream stream)
        {
            var obj = (IRun)run.Clone();
            foreach (var segment in obj)
            {
                segment.SplitTime = default(Time);
            }

            var formatter = new JsonSerializer();
            using var writer = new StreamWriter(stream, encoding: Encoding.UTF8, leaveOpen: true);
            formatter.Serialize(writer, obj);
        }
    }
}
