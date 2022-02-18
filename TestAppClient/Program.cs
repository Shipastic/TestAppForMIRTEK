using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class SocketClient
{
    static int port = 8005;
    static void Main(string[] args)
    {
            SendMessage();
    }

    static void SendMessage()
    {
        // буфер для входящих данных
        byte[] bytes = new byte[1024];
        byte[] msg = new byte[1024];
        // соединяемся с удаленным устройством

        try
        {
            // Устанавливаем удаленную конечную точку для сокета" ■* '
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Соединяем сокет с удаленной конечной точкой <■•>!"
            sender.Connect(ipEndPoint);
            Console.WriteLine($"Соединение с сокетом {sender.RemoteEndPoint}");
            Console.WriteLine("Отправьте цифру, соответствующую вашему выбору, на сервер:\n1 - Вывести весь список автомобилей\n" +
                                                                                          "2 - Вывести конкретный автомобиль\n" +
                                                                                          "3 - Выйти из программы\n");

            //Console.WriteLine("Введите сообщение");
            while (true)
            {
                string AnswerMessage = Console.ReadLine();              

                if (AnswerMessage != null)
                {
                    msg = Encoding.Unicode.GetBytes(AnswerMessage);
                }
                else
                {
                    Console.WriteLine("Вы ничего не выбрали");
                }

                // Отправляем запрос
                sender.Send(msg);

                int bytesRec = 0;

                //Получаем ответ         
                StringBuilder builder = new StringBuilder();
                do
                {
                    bytesRec = sender.Receive(bytes, bytes.Length, 0);
                    builder.Append(Encoding.Unicode.GetString(bytes, 0, bytesRec));
                }

                while (sender.Available > 0);

                Console.WriteLine($"Ответ сервера:  {builder}");

                if (AnswerMessage != "3")
                {
                    SendMessage();
                }
                else
                {
                    break;
                }
                //Освобождаем сокеT
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            Console.Write("Соединение закрыто");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e}");
        }
    }
}