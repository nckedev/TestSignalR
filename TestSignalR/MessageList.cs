using Newtonsoft.Json;

namespace TestSignalR;

public class MessageList
{
    private static Dictionary<Guid, object?> _messages;

    public MessageList()
    {
        _messages = new Dictionary<Guid, object?>();
    }

    public Guid NewEntry()
    {
        var key = Guid.NewGuid();
        if (!TrySetKey(key))
        {
            throw new Exception("Key finns redan");
        }

        return key;
    }

    private bool TrySetKey(Guid key)
    {
        if (!_messages.ContainsKey(key))
        {
            _messages.TryAdd(key, null);
            return true;
        }

        return false;
    }

    public bool TrySet(Guid key, object? obj)
    {
        if (_messages.ContainsKey(key))
        {
            _messages[key] = obj;
            return true;
        }

        throw new KeyNotFoundException("nyckeln finns inte i listan ");
    }

    public object? Get(Guid key)
    {
        if (_messages.TryGetValue(key, out var obj))
        {
            _messages.Remove(key);
            return obj;
        }

        return null;
    }

    public void Remove(Guid key)
    {
        _messages.Remove(key);
    }

    public bool TryGet<T>(Guid key, out T? value)
    {
        if (_messages.TryGetValue(key, out var o))
        {
            //o is the type T we're looking for 
            if (o is T obj)
            {
                value = obj;
                _messages.Remove(key);
                return true;
            }
            //o is not null and not T, it has to be json string
            else if (o is not null)
            {
                var res = JsonConvert.DeserializeObject<T>(o.ToString() ?? "");
                if (res is not null)
                {
                    value = res;
                    _messages.Remove(key);
                    return true;
                }
            }
        }
        
        value = default;
        return false;
    }


    public int CountAll => _messages.Count;
    public int CountEmpty => _messages.Count(k => k.Value == null);
    public int CountSet => _messages.Count(k => k.Value != null);
}