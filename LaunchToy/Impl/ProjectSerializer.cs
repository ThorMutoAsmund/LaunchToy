using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LaunchToy.Impl
{
    //public class PartSpecifiedConcreteClassConverter : DefaultContractResolver
    //{
    //    protected override JsonConverter ResolveContractConverter(Type objectType)
    //    {
    //        if (typeof(Part).IsAssignableFrom(objectType) && !objectType.IsAbstract)
    //            return null;
    //        return base.ResolveContractConverter(objectType);
    //    }
    //}

    //public class BaseConverter : JsonConverter
    //{
    //    private static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new PartSpecifiedConcreteClassConverter() };

    //    public override bool CanWrite => false;
    //    public override bool CanConvert(Type objectType) => objectType == typeof(Part);

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        var jObject = JObject.Load(reader);
    //        var objType = jObject[nameof(Part.ObjType)].Value<int>();

    //        switch (objType)
    //        {
    //            case Sample.ObjTypeValue:
    //                return JsonConvert.DeserializeObject<Sample>(jObject.ToString(), SpecifiedSubclassConversion);
    //            default:
    //                throw new JsonSerializationException($"Cannot deserialize part with ObjType {objType}");
    //        }
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        throw new NotImplementedException(); // won't be called because CanWrite returns false
    //    }
    //}
    public class ProjectSerializer
    {
        public static int CurrentVersion = 1;

        public Project? Project { get; set; }

        //private static JsonConverter[] Converters = { new BaseConverter() };
        //public ProjectSerializer()
        //{
        //}

        private void WriteToFile(string projectFilePath)
        {
            if (this.Project != null)
            {
                this.Project.Version = CurrentVersion;

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);

                File.WriteAllText(projectFilePath, json);
            }
        }


        public static ProjectSerializer? FromFile(string projectFilePath)
        {
            var json = File.ReadAllText(projectFilePath);
            return JsonConvert.DeserializeObject<ProjectSerializer>(json, new JsonSerializerSettings() { /*Converters = Converters*/ });
        }

        public static void ToFile(Project project)
        {
            new ProjectSerializer()
            {
                Project = project,
            }.WriteToFile(project.ProjectFilePath);
        }
    }
}
