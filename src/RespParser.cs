using System.Diagnostics;
namespace codecrafters_redis;
public class RespObject { }
public class RespArray : RespObject
{
  public RespObject[] Items { get; set; } = null!;
}
public class RespString : RespObject
{
  public string Value { get; set; } = null!;
}
public class RespParser
{
  public RespObject Parse(string command)
  {
    var (result, _) = InnerParse(command);
    return result;
  }
  private (RespObject, string) InnerParse(string command)
  {
    switch (command[0])
    {
      case '*':
        return ParseArray(command);
      case '$':
        return ParseBulkString(command);
    }
    throw new Exception($"Unknown command '{command}'");
  }
  private (RespArray, string) ParseArray(string command)
  {
    Debug.Assert(command[0] == '*');
    var (count, rest) = TakeFirst(command);
    var itemCount = int.Parse(count[1..]);
    var items = new RespObject[itemCount];
    for (var i = 0; i < itemCount; i++)
    {
      var (item, rest2) = InnerParse(rest);
      items[i] = item;
      rest = rest2;
    }
    var arr = new RespArray { Items = items };
    return (arr, rest);
  }
  private (RespString, string) ParseBulkString(string command)
  {
    Debug.Assert(command[0] == '$');
    var (length, rest) = TakeFirst(command);
    var stringLength = int.Parse(length[1..]);
    var (value, rest2) = TakeFirst(rest);
    Debug.Assert(value.Length == stringLength);
    var str = new RespString { Value = value };
    return (str, rest2);
  }
  private (string, string) TakeFirst(string line)
  {
    var tokens = line.Split("\r\n", 2);
    return (tokens[0], tokens[1]);
  }
}