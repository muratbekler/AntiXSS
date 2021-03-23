using Microsoft.Security.Application;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ConsoleApp2
{
    public class TestClass
    {
        public int Id { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            AClass tcObject2 = new AClass
            {
                Id = 1001,
                AName = "Murat",
                ListB = new List<BClass>
                {
                    new BClass
                    {
                        Id = 102,
                        Name = "<script>alert('test')</script>",
                        BClasses = new List<BClass>
                        {
                            new BClass
                            {
                                Id = 101,
                                Name = "Ahmet"
                            }
                        }
                    }
                }
            };
            var script = Regex.Match("", @"<|>|(|)&lt|%3c|script")?.Value;
            var typ = tcObject2.GetType();
            var obj = typ.Assembly.CreateInstance(typ.FullName);
            var vls = typ.GetProperties();
            foreach (var mem in vls)
            {
                SetListValue(tcObject2, mem.Name);
            }
        }
        static object GetValue(object dotNetType, string forObject)
        {
            MemberInfo memberInfo = dotNetType.GetType().GetMember(forObject)[0];
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(dotNetType);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(dotNetType);
                default:
                    throw new NotImplementedException();
            }
        }
        static void SetValue(object dotNetType, string forObject)
        {
            object fieldInfos = GetValue(dotNetType, forObject);
            if (fieldInfos != null)
            {
                var isxss = !string.IsNullOrEmpty(Regex.Match(fieldInfos.ToString(), @"<|>|(|)&lt|%3c|script")?.Value);
                if (isxss)
                {
                    var value = Sanitizer.GetSafeHtmlFragment(fieldInfos.ToString());
                    MemberInfo memberInfo = dotNetType.GetType().GetMember(forObject)[0];
                    if (memberInfo.MemberType == MemberTypes.Field)
                    {
                        FieldInfo item = ((FieldInfo)memberInfo);
                        item.SetValue(dotNetType, value);
                    }
                    else if (memberInfo.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo item = ((PropertyInfo)memberInfo);
                        item.SetValue(dotNetType, value);
                    }
                }
            }
        }
        static void SetListValue(object tcObject, string forObject)
        {
            object fieldInfos = GetValue(tcObject, forObject);
            if ((fieldInfos as IList) != null)
            {
                IList listObject = (fieldInfos as IList);
                Type type = listObject[0].GetType();
                IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
                foreach (var item in listObject)
                {
                    foreach (var ti in type.GetProperties())
                    {
                        object fieldiItem = GetValue(item, ti.Name);
                        if ((fieldiItem as IList) != null)
                        {
                            SetListValue(item, ti.Name);
                        }
                        SetListValue(item, ti.Name);
                        SetValue(item, ti.Name);
                    }

                    list.Add(item);
                }
                SetValue(tcObject, forObject);
            }
            else if (fieldInfos != null)
            {
                SetValue(tcObject, forObject);
            }
        }
    }
}
