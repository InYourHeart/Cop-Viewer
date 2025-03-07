namespace CoP_Viewer.Source.Model
{
    public class Entity
    {
        private Dictionary<string, object> attributes;
        private Dictionary<string, object> children;

        public Entity()
        {
            attributes = [];
            children = [];
        }


        public object? GetAttribute(string key)
        {
            return GetFromDictionary(key, attributes);
        }

        public void SetAttribute(string key, object value)
        {
            SetToDictionary(key, value, attributes);
        }

        public Entity? GetDirectChild(string key)
        {
            return (Entity?) GetFromDictionary(key, children);
        }

        public void SetDirectChild(string key, Entity value)
        {
            SetToDictionary(key, value, children);
        }

        private object? GetFromDictionary(string key, Dictionary<string,object> dict)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            else
            {
                return null;
            }
        }

        private void SetToDictionary(string key, object value, Dictionary<string,object> dict)
        {
            if (!dict.TryAdd(key, value))
            {
                dict[key] = value;
            }
        }
    }
}
