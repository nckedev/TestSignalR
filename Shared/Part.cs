using System.Runtime.Serialization;

namespace Shared;

[DataContract]
public class Part
{
    [DataMember(Name = "Id")]
    public  string Id { get; set; }
    
    [DataMember(Name = "Name")]
    public  string Name { get; set; }
}