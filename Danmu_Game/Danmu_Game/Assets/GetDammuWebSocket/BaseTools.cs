using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class BaseTools
{
    /// <summary>
    /// 把一个实例转换成字典并赋值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Dictionary<string, string> ConvertClassToDic<T>(T data)
    {
        Type type = typeof(T);
        PropertyInfo[] Props = type.GetProperties();
        Dictionary<string, string> Dic = new Dictionary<string, string>();
        for (int i = 0; i < Props.Length; i++)
        {
            string Name = Props[i].Name;
            Dic.Add(Name, (string)Props[i].GetValue(data));
        }
        return Dic;
    }


    //反射参考
    public static void InitTextData<T>(T data, Dictionary<string, Text> dictText)
    {
        if (data == null || dictText == null) return;

        Type type = typeof(T);
        var targetTypes = new List<Type>() { typeof(string), typeof(int), typeof(float) };
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);//成员
                                                                                 //type.GetProperties();//属性
                                                                                 //TODO 目前是public的成员变量，不包括属性。后续需要屈服数据类型的特性

        Text tmp;
        for (int i = 0; i < fields.Length; i++)
        {
            //只处理指定类型
            if (targetTypes.Contains(fields[i].FieldType))
            {
                string str = fields[i].GetValue(data)?.ToString();
                if (dictText.TryGetValue(fields[i].Name, out tmp))
                    tmp.text = str;
            }
        }
    }

    //Task参考
    static Task<int> CreateTask(string name)
    {
        return new Task<int>(() => TaskMethod(name));
    }
    static int TaskMethod(string name)
    {
        Console.WriteLine("Task {0} 运行在线程id为{1}的线程上。是否是线程池中线程？:{2}",
        name, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
        Thread.Sleep(2000);
        return 42;
    }
}
