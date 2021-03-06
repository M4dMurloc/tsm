using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;

public class NeiroWeb
{
    // это по сути контейнер для массива нейронов neironArray
    // загружет его при создании из файла и сохраняет при выходе

    public const int neironInArrayWidth = 100; // количество по горизонтали
    public const int neironInArrayHeight = 100; // количество по вертикали
    private const string memory = "memory.txt"; // имя файла хранения сети
    private List<Neiron> neironArray = null; // массив нейронов

    private double match_ratio = 0.0;

    private int select_quest_index = -1;
    private List<Neiron> questArray = null;

    public NeiroWeb()
    {
        //Debug.Log("InitWeb");
        neironArray = InitWeb();
        questArray = InitWeb();
    }

    //Открывает текстовой файл и преобразовывает его в массив нейронов
    private static List<Neiron> InitWeb()
    {
        if (!File.Exists(Application.persistentDataPath + "/" + memory))
        {
            //Debug.Log("файл не найден, беру из ресурсов");

            TextAsset text_asset = Resources.Load<TextAsset>("memory");
            string jStr_res = text_asset.text;

            return JsonConvert.DeserializeObject<List<Neiron>>(jStr_res);
        }
        //Debug.Log("файл найден");

        string[] lines = File.ReadAllLines(Application.persistentDataPath + "/" + memory);
        if (lines.Length == 0) return new List<Neiron>();
        
        string jStr = lines[0];
        return JsonConvert.DeserializeObject<List<Neiron>>(jStr);
    }

    //Сравнивает входной массив с каждым нейроном из сети и 
    //возвращает имя нейрона наиболее похожего на него
    //именно эта функция отвечает за распознавание образа

    public string CheckLitera(int[,] arr)
    {
        string res = null;
        double max = 0;
        foreach (var n in neironArray)
        {
            double d = n.GetRes(arr);

            if (d > max)
            {
                max = d;
                res = n.GetName();
            }
        }
        match_ratio = max;

        if (max > 0.7) return res;
        else return null;      
    }

    //Сохраняет массив нейронов в файл
    public void SaveState()
    {
        string json = JsonConvert.SerializeObject(neironArray);
        StreamWriter file = new StreamWriter(Application.persistentDataPath + "/" + memory);
        file.WriteLine(json);
        file.Close();

        //StreamWriter file_test = new StreamWriter("TEST.txt");
        //for (int i = 0; i < 100; i++)
        //{
        //    for (int j = 0; j < 100; j++)
        //    {
        //        file_test.Write(neironArray[4].weight[i, j] + "\t");
        //    }
        //    file_test.Write("\n");
        //}
        //file_test.Close();
    }

    //Получить список имён образов, имеющихся в памяти
    public string[] GetLiteras()
    {
        var res = new List<string>();
        for (int i = 0; i < neironArray.Count; i++) res.Add(neironArray[i].GetName());
        res.Sort();
        return res.ToArray();
    }

    //Этот метод заносит в память нейрона с именем trainingName
    // новый вариант образа data

    public void SetTraining(string trainingName, int[,] data, Text textPanel)
    {
        Neiron neiron = neironArray.Find(v => v.obj_name.Equals(trainingName));
        if (neiron == null) // если нейрона с таким именем не существует, создадим новыи и добавим
        {                   // его в массив нейронов
            neiron = new Neiron();
            neiron.Clear(trainingName, neironInArrayWidth, neironInArrayHeight);
            neironArray.Add(neiron);
        }
        int countTrainig = neiron.Training(data); // обучим нейрон новому образу
        textPanel.text += ". Вариантов образа в памяти: " + countTrainig.ToString();
    }

    public string SetQuest()
    {
        if(select_quest_index != -1)
        {
            questArray.Remove(questArray[select_quest_index]);
        }

        select_quest_index = UnityEngine.Random.Range(0, questArray.Count);
        return questArray[select_quest_index].GetName();
    }

    public int GenerateScore()
    {
        double buff = match_ratio * 100;
        return (int)buff;
    }
}
