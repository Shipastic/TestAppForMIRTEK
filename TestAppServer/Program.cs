using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Collections;

public class SocketServer
{
    static int port = 8005;
    static void Main(string[] args)
    {
            Receive();    
    } 
   public struct Car
    {
        public  string? Mark;
        public  ushort? Year ;
        public  float?  EngineCapacity ;
        public  byte?   CountDoors ;

        public Car()
        {
            Mark           = "Nissan";
            Year           = 2008;
            EngineCapacity = 1.6f;
            CountDoors     = null;
        }
    }

    /// <summary>
    /// Метод для формирования байтовой последовательности Года выпуска
    /// </summary>
    /// <param name="car"></param>
    /// <returns></returns>
    public string GetYear(Car car)
    {
        ushort maxYear = 0x12;

        List<string> substrYear = new List<string>();

        string resYear = null;

        string arrayYear = null;

        if (car.Year != null)
        {
            if (car.Year < 4096)
            {
                arrayYear = $"0{(ushort)car.Year:X}";
            }
            else
            {
                arrayYear = $"{(ushort)car.Year:X}";
            }

            for (int i = 0; i < arrayYear.Length; i+=2)
            {
                substrYear.Add($"0x{arrayYear.Substring(i, 2)}");
            }
        }
        else
        {
            arrayYear = "0x00";

            resYear = "0x00";
        }
        foreach (var item in substrYear)
        {
            resYear += item + " ";
        }

        resYear = $"0x{maxYear:X} {resYear}";

        return resYear.Trim();
    }

    /// <summary>
    /// Метод формирования байтовой последовательности Марки автомобиля
    /// </summary>
    /// <param name="car"></param>
    /// <returns></returns>
    public string GetMark(Car car)
    {       
        string ResultStringMark = null;

        ushort maxCharMark = 0x9;

        string countChar = null;

        if (car.Mark != null)
        {
            byte[] ByteMark = Encoding.ASCII.GetBytes(car.Mark);

            if (ByteMark.Length > 0 && ByteMark.Length <= maxCharMark)
            {
                countChar = $"0x0{ByteMark.Length:X} ";

                foreach (byte OneByte in ByteMark)
                {
                    ResultStringMark += $"0x{OneByte:X} ";
                }
            }
            else
            {
                countChar = $"0x0{ByteMark.Length:X} ";

                ResultStringMark = "0x00 ";
            }
        }
        else
        {
            countChar = "0x00 ";

            ResultStringMark = "0x00 ";
        }

        ResultStringMark = $"0x0{maxCharMark:X} {countChar}{ResultStringMark}";

        return ResultStringMark;
    }

    /// <summary>
    /// Метод для получени байтовой последовательности объема двигателя
    /// </summary>
    /// <param name="car"></param>
    public string GetEngineCapacity(Car car)
    {
        ushort maxValue = 0x13;

        int SearcIndex = 0;

        string resSumMantissa = null;

        string capacityValue = null;

        string resultNumberWithSign = null;

        string resultNumber = null;

        string res = null;

        string[] resultGroup = null;

        var lengthFractPart = Convert.ToString(car.EngineCapacity);

        var listNumbers = FloatToBin(car.EngineCapacity);

        string[] arNumber = listNumbers.Split('.');

        SearcIndex = GetExponentNumber(arNumber);

        GetSubstrExp(arNumber, SearcIndex, SearcIndex);

        string expNumber = ConvertExponent(SearcIndex, arNumber);

        resSumMantissa = Convert.ToString(SetMantissa(arNumber),2);

        //Если число положительное
        if (car.EngineCapacity != null)
        {
            if (car.EngineCapacity > 0)
            {
                resultNumberWithSign = $"0{expNumber}{resSumMantissa}";
            }
            else
               if (car.EngineCapacity < 0)
            {
                resultNumberWithSign = $"1{expNumber}{resSumMantissa}";
            }
        }
        else
        {
            resultNumber = $"0x{maxValue:X} 0x00";
        }

        resultGroup = BreakNum(resultNumberWithSign);

        resultNumber = TranslateNumber(resultGroup);

        res = GetFormatString(resultNumber);

        ///Получаем экспоненту
        int GetExponentNumber(string[] _numberArr)
        {
            int _NumberCount = 0;

            if (Int32.Parse(_numberArr[0]) == 1)
            {
                return 127;
            }
            else
            {              
                _NumberCount =  _numberArr[0].IndexOf("1")+1;

                if (_NumberCount != 0)
                {
                    string strIdx = _numberArr[0].Substring(_NumberCount, _numberArr[0].Length - 1);

                    int serchIdx = strIdx.Length;

                    return 127 + serchIdx;
                }
                else
                {
                    _NumberCount =  _numberArr[1].IndexOf("1");

                    string strIdx = _numberArr[1].Substring(0, _NumberCount);

                    int serchIdx = strIdx.Length+1;

                    return 127 - serchIdx; 
                }
            }
        }

        ///Преобразование экспоненты в двоичный формат
        string ConvertExponent(int SearcIndex, string[] _numberArr)
        {
            string BinaryExp = Convert.ToString(SearcIndex, 2);

            _numberArr[0] = BinaryExp;

            return _numberArr[0];   
        }

        ///Получаем число для переноса
        void GetSubstrExp(string[] _numberArr, int CountChars, int NumberExp)
        {
            int _NumberCount = 0;

            if (NumberExp > 127)
            {
                _NumberCount =  _numberArr[0].IndexOf("1")+1;

                string SubstrExp = _numberArr[0].Substring(_NumberCount, CountChars-127);

                _numberArr[1] = SubstrExp + _numberArr[1];
            }
            else
            {
                _NumberCount =  _numberArr[1].IndexOf("1");

                string SubstrExp = _numberArr[1].Substring(0, _NumberCount);

                _numberArr[1] = _numberArr[1].Substring(SubstrExp.Length+1, _numberArr[1].Length - SubstrExp.Length - 1);
            }
        }

        ///Установка мантиссы числа
        int SetMantissa(string[] _numberArr)
        {
            //1001100110011001100 - 19чисел
            double SumMantis = 0;

            int LengthMantis = _numberArr[1].Length;

            //Длина мантиссы должна быть равна 23 знакам
            if (LengthMantis <23)
            {
                while (LengthMantis <23)
                {
                    _numberArr[1] += "0";
                    LengthMantis++;
                }
            }
            else
                if(LengthMantis > 23)
            {
                _numberArr[1] = _numberArr[1].Substring(0, 22);
            }

            char[] arMantis = _numberArr[1].Reverse().ToArray();

            //Высчитываем значение мантиссы
            for (int i = 0; i < _numberArr[1].Length; i++)
            {
                SumMantis += Math.Pow(2, i) * Char.GetNumericValue(arMantis[i]);
            }

            return (int)SumMantis;
        }

        ///Метод для формирования числа в двоичном представлении в строке
        string FloatToBin(float? num)
        {
            string list = null;

            //конвертируем целую часть в двоичный код
            int intPart = (int)num;

            string s1 = Convert.ToString(intPart, 2);

            list = $"{s1}.";

            //конвертируем дробную часть в двоичный код
            float? floatPart = num - intPart;
            string s2 = string.Empty;
            for (int i = 0; i < 23; i++)
            {
                floatPart = floatPart * 2;
                if (floatPart >= 1)
                {
                    s2 += 1;
                    floatPart = floatPart - 1;
                }
                else
                {
                    s2 += 0;
                }
            }
            list += s2;

            return list;
        }

        return res;
    }

    /// <summary>
    /// Метод для формирования массива 4-значных чисел
    /// </summary>
    /// <param name="resultNumberWithSign"></param>
    /// <returns></returns>
    string[] BreakNum(string resultNumberWithSign)
    {
        //"0111101110011001100110011000000"
        if (resultNumberWithSign.Length <32)
        {
            for (int i = resultNumberWithSign.Length; i < 32; i++)
            {
                resultNumberWithSign = $"0{resultNumberWithSign}";
            }
        }     

        string[] arrayBlock = new string[8];     

        int j = 0;
        for (int i = 0; i < 8; i++)
        {     
            arrayBlock[i] = resultNumberWithSign.Substring(j, 4);
            j+=4;                   
        }
        return arrayBlock;
    }

    /// <summary>
    /// Метод для перевода числа в шестнадцатеричный формат
    /// </summary>
    /// <param name="arrayBlock"></param>
    /// <returns></returns>
    string TranslateNumber(string[] arrayBlock)
    {
        string resStringNumber = null;

        int[] intMatch = new int[8];

        for (int i = 1; i <= arrayBlock.Length; i++)
        {
            var _Num = Convert.ToString((Convert.ToByte(arrayBlock[i-1], 2)), 16);

            //Делим на группы
            if (i % 2 ==0)
            {
                resStringNumber +=_Num + " ";
            }
            else
            {
                resStringNumber += _Num;
            }
        }

        return resStringNumber.Trim();
    }

    /// <summary>
    /// Метод для формирования исходной строки
    /// </summary>
    /// <param name="resStringNumber"></param>
    /// <returns></returns>
    public string GetFormatString(string resStringNumber)
    {
        string result = null;

        string[] arrString = resStringNumber.Split(" ");

        for (int i = 0; i < arrString.Length; i++)
        {
            arrString[i] = "0x"+arrString[i];

            result += arrString[i] + " ";
        }
        return result;

    }

    /// <summary>
    /// Метод для формирования полной последовательности байт на отправку
    /// </summary>
    /// <param name="car"></param>
    public string SendSequenceByte(Car car)
    {
        string resultString = null;

        //Признак начала структуры
        string beginStruct = "0x02";

        string countFiels = null;

        int countField = 0;

        ///Проверка заполненных полей у структуры
        foreach (System.Reflection.FieldInfo field in car.GetType().GetFields())
        {
            object value = field.GetValue(car);
            if (value != null)
                countField++;
            
        }
        countFiels = $"0x{countField} ";

        //string CountFieldFull

        resultString = $"{beginStruct} {countFiels}{GetMark(car)}{GetYear(car)} {GetEngineCapacity(car)}";

        return resultString;
    }

   
    /// <summary>
    /// Метод для установки связи и отправки/получения сообщений
    /// </summary>
    static void Receive()
    {
        Car car = new Car();

        SocketServer socketServer = new SocketServer();

        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

        // создаем сокет Tcp/Ip
        Socket slistener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // назначаем сокет локальной конечной точке и запускаем листенер
        try
        {
            slistener.Bind(ipEndPoint);

            slistener.Listen(10);

            while (true)
            {
                Console.WriteLine($"Ожидание подключения к  {ipEndPoint}...");
                socketServer.SendSequenceByte(car);
                Socket handler = slistener.Accept();
                
                StringBuilder builder = new StringBuilder();
                string response = null;

                int bytes = 0; // количество полученных байтов

                byte[] bytesArray = new byte[1024]; // буфер для получаемых данных

                byte[] msg = new byte[1024]; //буфер для ответа

                string data = null;
                do
                {

                    int bytesRec = handler.Receive(bytesArray);
                    data += Encoding.Unicode.GetString(bytesArray, 0, bytesRec);
                    response = data;
                    builder.Append(Encoding.Unicode.GetString(bytesArray, 0, bytesRec));
                }
                while (handler.Available > 0);

                Console.WriteLine($"Полученный текст:  {builder}");
                switch (response)
                {
                    case "1":
                        string theReply1 = $"Ответ {socketServer.SendSequenceByte(car)}";
                        msg = Encoding.Unicode.GetBytes(theReply1);
                        break;
                    case "2":
                        string theReply2 = $"Ответ {socketServer.SendSequenceByte(car)}";
                        msg = Encoding.Unicode.GetBytes(theReply2);
                        break;
                    default:
                        string theReply3 = $"Ответ: неизвестный запрос!";
                        msg = Encoding.Unicode.GetBytes(theReply3);
                        break;
                }
                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
