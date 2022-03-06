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
    /// ��һ��ʵ��ת�����ֵ䲢��ֵ
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


    //����ο�
    public static void InitTextData<T>(T data, Dictionary<string, Text> dictText)
    {
        if (data == null || dictText == null) return;

        Type type = typeof(T);
        var targetTypes = new List<Type>() { typeof(string), typeof(int), typeof(float) };
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);//��Ա
                                                                                 //type.GetProperties();//����
                                                                                 //TODO Ŀǰ��public�ĳ�Ա���������������ԡ�������Ҫ�����������͵�����

        Text tmp;
        for (int i = 0; i < fields.Length; i++)
        {
            //ֻ����ָ������
            if (targetTypes.Contains(fields[i].FieldType))
            {
                string str = fields[i].GetValue(data)?.ToString();
                if (dictText.TryGetValue(fields[i].Name, out tmp))
                    tmp.text = str;
            }
        }
    }

    //Task�ο�
    static Task<int> CreateTask(string name)
    {
        return new Task<int>(() => TaskMethod(name));
    }
    static int TaskMethod(string name)
    {
        Console.WriteLine("Task {0} �������߳�idΪ{1}���߳��ϡ��Ƿ����̳߳����̣߳�:{2}",
        name, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
        Thread.Sleep(2000);
        return 42;
    }
}
