using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

class Calculator
{
    static void Main()
    {
        using var db = new CalcContext();
        Console.WriteLine($"Database path: {db.DbPath}.");


        Console.WriteLine("Если вы хотите восстановить данные из файла json - напишите json");
        Console.WriteLine("Если вы хотите восстановить данные из файла xml - напишите xml");
        Console.WriteLine("Если вы хотите восстановить данные из файла SQLite - напишите sqlite");
        string xmlFilePath1 = "XMLData.xml";
        XDocument docs = XDocument.Load(xmlFilePath1);
        double[] xmlArray = docs.Root?
                .Element("items")?
                .Elements("item")
                .Select(e => double.Parse(e.Value))
                .ToArray();

        if (xmlArray != null)
            foreach (var item in xmlArray) ;
        string jsonFilePath = "JSONData.json";
        string jsonText = File.ReadAllText(jsonFilePath);
        var jsonArray = JsonConvert.DeserializeObject<double[]>(jsonText);
        double[] resultsArray = new double[10];
        string inf = Console.ReadLine();
        if (inf == "json")
        {

            for (int i = 0; i < 10; i++)
            {
                resultsArray[i] = jsonArray[i];
                Console.Write(resultsArray[i]);
                Console.Write(' ');
            }
        }
        if (inf == "xml")
        {

            for (int i = 0; i < 10; i++)
            {
                resultsArray[i] = xmlArray[i];
                Console.Write(resultsArray[i]);
                Console.Write(' ');
            }
        }
        if (inf == "sqlite")
        {
            string connectionString = "Data Source=путь_к_вашей_базе_данных.sqlite;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Nums";
                using (SqliteCommand command = new SqliteCommand(sql, connection))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        var nums = new List<double>();

                        while (reader.Read())
                        {
                            double num = reader.GetDouble(0); // Предполагается, что числа хранятся в первом столбце
                            nums.Add(num);
                        }
                        resultsArray = nums.OrderBy(n => n).ToArray();
                        for (int i = 0; i < 10; i++)
                        {
                            resultsArray[i] = nums[i];
                            Console.Write(resultsArray[i]);
                            Console.Write(' ');
                        }
                    }
                }
            }



            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("when a first symbol on line is ‘>’ – enter operand(number)");
            Console.WriteLine("when a first symbol on line is ‘@’ – enter operation");
            Console.WriteLine("operation is one of ‘+’, ‘-‘, ‘/’, ‘*’ or ‘#’ followed with number of evaluation step");
            Console.WriteLine("‘q’ to exit");
            Console.Write("> ");
            double.TryParse(Console.ReadLine(), out double result);
            resultsArray[0] = result;
            int numofcount = 0;







            while (true)
            {
            Found: Console.WriteLine($"[#{numofcount}]: {result}");
                Console.Write("@ ");
                string operationInput = Console.ReadLine();

                if (operationInput.ToLower() == "q")
                    break;
                if (operationInput.StartsWith("#") == true)
                {
                    string valueXString = operationInput.Substring(1);
                    if (int.TryParse(valueXString, out int valueX))
                    {
                        result = resultsArray[valueX];
                    }
                    else
                    {
                        Console.WriteLine("Неверный формат числа после символа '#'.");
                        continue;
                    }
                    numofcount++;
                    goto Found;
                }
                Console.Write("> ");
                string input = Console.ReadLine();

                if (!double.TryParse(input, out double num2))
                {
                    Console.WriteLine("Ошибка: введено некорректное число!");
                    continue;
                }

                switch (operationInput)
                {
                    case "+":
                        result += num2;
                        break;
                    case "-":
                        result -= num2;
                        break;
                    case "*":
                        result *= num2;
                        break;
                    case "/":
                        if (num2 != 0)
                        {
                            result /= num2;
                        }
                        else
                        {
                            Console.WriteLine("Ошибка: деление на ноль!");
                            continue;
                        }
                        break;
                    default:
                        Console.WriteLine("Ошибка: введена неверная операция!");
                        continue;
                }
                numofcount++;
                resultsArray[numofcount] = result;
            }
            Console.WriteLine("Программа завершена.");
            Console.WriteLine("Выберите способ сохранения данных, написав цифру: 1.JSON, 2.XML, 3.SQLite");
            string saveInput = Console.ReadLine();
            switch (saveInput)
            {
                case "1":
                    string JSONData = JsonConvert.SerializeObject(resultsArray);
                    string fileName = "JSONData.json";
                    File.WriteAllText(fileName, JSONData);
                    Console.WriteLine(File.ReadAllText(fileName));
                    break;
                case "2":
                    XDocument doc = new XDocument(
                    new XElement("root",
                        new XElement("items",
                            from item in resultsArray
                            select new XElement("item", item)
                        )
                    )
                );
                    string xmlFilePath = "XMLData.xml";
                    doc.Save(xmlFilePath);
                    break;
                case "3":
                    {
                        db.Add(new Calculators { nums = resultsArray });
                        db.SaveChanges();
                    }
                    break;
            }
        }
    }
}