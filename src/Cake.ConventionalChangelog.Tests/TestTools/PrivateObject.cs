using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cake.ConventionalChangelog.Tests.TestTools
{
    public class PrivateObject
    {
        private readonly object _obj;
        private readonly Type _type;

        public PrivateObject(object obj)
        {
            _obj = obj;
            _type = obj.GetType();
        }

        public PrivateObject(Type type)
        {
            _type = type;
            _obj = Activator.CreateInstance(type, Array.Empty<object>());          
        }

        public object Invoke(string methodName, object[] parameters)
        {
            try
            {
                var method = _type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                 .Where(x => x.Name == methodName && x.IsPrivate)
                 .First();

                var result = method.Invoke(_obj, parameters);
                return result;
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public object GetField(string name, BindingFlags bindingFlags)
        {
            var field = _type.GetField(name, bindingFlags);

            return field.GetValue(_obj);
        }
    }
}
